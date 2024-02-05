if [ -z "$1" ]; then
  echo "Usage: $0 <job-name>"
  exit 1
fi

if [ -z "$2" ]; then
  echo "Usage: $0 <resource-group-name>"
  exit 1
fi

# todo: use something else than git sha to target the job execution
if [ -z "$3" ]; then
  echo "Usage: $0 <git-sha>"
  exit 1
fi

job_name="$1"
resource_group="$2"
git_sha="$3"
query_filter="[?properties.template.containers[?contains(image, '$git_sha')]].{name: name, status: properties.status} | [0]"

verify_job_succeeded() {
  local current_job_execution
  
  current_job_execution=$(az containerapp job execution list -n "$job_name" -g "$resource_group" --query "$query_filter" 2>/dev/null)

  if [ -z "$current_job_execution" ]; then
      echo "No job execution found for job $job_name"
      return 1
  fi
    
  current_job_execution_name=$(echo $json_output | jq -r '.name')
  current_job_execution_status=$(echo $json_output | jq -r '.status')

  echo "Job execution state for job $job_name status:"
  echo "-----------------------------"
  echo "Name: $current_job_execution_name"
  echo "Running status: $current_job_execution_status"
  echo " "
  
  # Check job execution status
  if [[ $current_job_execution_status == "Succeeded"]]; then
    return 0  # OK!
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
    echo "Attempt $attempt: Waiting for job $job_name ..."
    sleep 10 # Sleep for 10 seconds
    attempt=$((attempt+1))
  fi
done