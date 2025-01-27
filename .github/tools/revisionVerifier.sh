if ! command -v jq 2>&1 >/dev/null
then
    echo "jq could not be found, attempting to install from tdnf ..."
    tdnf install -y jq
fi

if [ -z "$1" ]; then
  echo "Usage: $0 <revision-name>"
  exit 1
fi

if [ -z "$2" ]; then
  echo "Usage: $0 <resource-group-name>"
  exit 1
fi

revision_name="$1"
resource_group="$2"
query_filter="{name:name, runningState:properties.runningState, healthState:properties.healthState}"

# Define exit codes
readonly EXIT_SUCCESS=0
readonly EXIT_NOT_READY=1
readonly EXIT_FAILED=2

verify_revision() {
  local json_output

  # Fetch app revision
  json_output=$(az containerapp revision show -g "$resource_group" --revision "$revision_name" --query "$query_filter" 2>/dev/null)

  health_state=$(echo $json_output | jq -r '.healthState')
  running_state=$(echo $json_output | jq -r '.runningState')

  echo "Revision $revision_name status:"
  echo "-----------------------------"
  echo "Health state: $health_state"
  echo "Running state: $running_state"
  echo " "

  # Check if running state is Failed
  if [[ $running_state == "Failed" ]]; then
    echo "Error: Revision $revision_name has failed."
    return $EXIT_FAILED
  fi

  # Check health and running status
  if [[ $health_state == "Healthy" && ($running_state == "Running" || $running_state == "RunningAtMaxScale") ]]; then
    return $EXIT_SUCCESS
  else
    return $EXIT_NOT_READY
  fi
}

attempt=1

# Loop until verified or failed (GitHub action will do a timeout)
while true; do
  verify_revision
  result=$?

  case $result in
    $EXIT_SUCCESS)
      echo "Revision $revision_name is healthy and running"
      exit $EXIT_SUCCESS
      ;;
    $EXIT_FAILED)
      echo "Revision $revision_name has failed. Exiting."
      exit $EXIT_FAILED
      ;;
    $EXIT_NOT_READY)
      echo "Attempt $attempt: Waiting for revision $revision_name ..."
      sleep 10 # Sleep for 10 seconds
      attempt=$((attempt+1))
      ;;
  esac
done
