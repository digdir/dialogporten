usage_params="<job-name> <resource-group-name> <image-tag>"

if [ -z "$1" ]; then
  echo "Usage: $0 $usage_params"
  exit 1
fi

if [ -z "$2" ]; then
  echo "Usage: $0 $usage_params"
  exit 1
fi

if [ -z "$3" ]; then
  echo "Usage: $0 $usage_params"
  exit 1
fi

job_name="$1"
resource_group="$2"
image_tag="$3"
query_filter="[?properties.template.containers[?ends_with(image, ':$image_tag')]].{name: name, status: properties.status} | [0]"

echo "Verifying job $job_name for image tag $image_tag"
echo " "

verify_job_succeeded() {
  local current_job_execution
  
  current_job_execution=$(az containerapp job execution list -n "$job_name" -g "$resource_group" --query "$query_filter" 2>/dev/null)

  if [ -z "$current_job_execution" ]; then
      echo "No job execution found for job $job_name"
      return 1
  fi
    
  current_job_execution_name=$(echo $current_job_execution | jq -r '.name')
  current_job_execution_status=$(echo $current_job_execution | jq -r '.status')

  echo "Container: $current_job_execution_name"
  echo "Running status: $current_job_execution_status"
  
  # Check job execution status
  if [[ $current_job_execution_status == "Succeeded" ]]; then
    return 0  # OK!
  elif [[ $current_job_execution_status == "Failed" ]]; then
    echo "Job execution failed. Exiting script." 
    exit 1
  else
    return 1  # Not OK!
  fi
}

attempt=1

# Loop until verified (GitHub action will do a timeout)
while true; do
  if verify_job_succeeded; then
    echo "Job $job_name has succeeded"
    break
  else
    attempt=$((attempt+1))
    echo " "
    echo "-----------------------------"
    echo " "
    echo "Attempt $attempt: Waiting for job $job_name ..."
    sleep 10 # Sleep for 10 seconds
  fi
done