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

verify_revision() {
  local json_output
  
  # Fetch app revision
  json_output=$(az containerapp revision show -g "$resource_group" --revision "$revision_name" --query "$query_filter" 2>/dev/null)
  
  echo $json_output
  # Check health and running status
  if [[ $json_output == *'"healthState": "Healthy",'*'"runningState": "Running"'* || $json_output == *'"healthState": "Healthy",'*'"runningState": "RunningAtMaxScale"'* ]]; then
    return 0  # OK!
  else
    return 1  # Not OK!
  fi
}

# Loop until verified (GitHub action will do a timeout)
while true; do
  if verify_revision; then
    echo "Revision $revision_name is healthy and running"
    break
  else
    echo "Waiting for revision $revision_name ..."
    sleep 10 # Sleep for 10 seconds
  fi
done
