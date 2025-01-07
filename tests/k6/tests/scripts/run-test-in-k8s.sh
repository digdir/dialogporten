#!/bin/bash

tokengenuser=${TOKEN_GENERATOR_USERNAME}
tokengenpasswd=${TOKEN_GENERATOR_PASSWORD}

help() {
    echo "Usage: $0 [OPTIONS]"
    echo "Options:"
    echo "  -f, --filename       Specify the filename of the k6 script archive"
    echo "  -c, --configmapname  Specify the name of the configmap to create"
    echo "  -n, --name           Specify the name of the test run"
    echo "  -v, --vus            Specify the number of virtual users"
    echo "  -d, --duration       Specify the duration of the test"
    echo "  -p, --parallelism    Specify the level of parallelism"
    echo "  -h, --help           Show this help message"
    exit 0
}

print_logs() {
    POD_LABEL="k6-test=$name"
    K8S_CONTEXT="k6tests-cluster"
    K8S_NAMESPACE="default"

    for pod in $(kubectl --context "$K8S_CONTEXT" -n "$K8S_NAMESPACE" get pods -l "$POD_LABEL" -o name); do 
        if [[ $pod != *"initializer"* ]]; then
            echo ---------------------------
            echo $pod
            echo ---------------------------
            kubectl --context "$K8S_CONTEXT" -n "$K8S_NAMESPACE" logs --tail=-1 $pod
        fi
    done
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        -h|--help)
            help
            ;;
        -f|--filename)
            filename="$2"
            shift 2
            ;;
        -c|--configmapname)
            configmapname="$2"
            shift 2
            ;;
        -n|--name)
            name="$2"
            shift 2
            ;;
        -v|--vus)
            vus="$2"
            shift 2
            ;;
        -d|--duration)
            duration="$2"
            shift 2
            ;;
        -p|--parallelism)
            parallelism="$2"
            shift 2
            ;;
        *)
            echo "Invalid option: $1"
            help
            exit 1
            ;;
    esac
done

k6 archive $filename -e API_VERSION=v1 -e API_ENVIRONMENT=yt01 -e TOKEN_GENERATOR_USERNAME=$tokengenuser -e TOKEN_GENERATOR_PASSWORD=$tokengenpasswd
# Create configmap from archive.tar
kubectl create configmap $configmapname --from-file=archive.tar

# Create the config.yml file from a string
cat <<EOF > config.yml
apiVersion: k6.io/v1alpha1
kind: TestRun
metadata:
  name: $name
spec:
  arguments: --out experimental-prometheus-rw --vus=$vus --duration=$duration
  parallelism: $parallelism
  script:
    configMap:
      name: $configmapname
      file: archive.tar
  runner:
    env:
      - name: K6_PROMETHEUS_RW_SERVER_URL
        value: "http://kube-prometheus-stack-prometheus.monitoring:9090/api/v1/write"
    metadata:
      labels:
        k6-test: $name
EOF
# Apply the config.yml configuration
kubectl apply -f config.yml

# Wait for the job to finish
wait_timeout="${duration}100s"
kubectl --context k6tests-cluster wait --for=jsonpath='{.status.stage}'=finished testrun/$name --timeout=$wait_timeout

# Print the logs of the pods
print_logs
# Delete the search-dialog.yml configuration
kubectl delete -f config.yml

# Delete the configmap
kubectl delete configmap $configmapname

# Delete the archive.tar and the config.yml files
rm archive.tar config.yml