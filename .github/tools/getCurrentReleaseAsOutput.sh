#!/bin/bash

set -e

# Check if required arguments are provided
if [ "$#" -ne 2 ]; then
    echo "Usage: $0 <resource-group> <app-name>" >&2
    exit 1
fi

RESOURCE_GROUP="$1"
APP_NAME="$2"

# Function to extract version from image tag
extract_version() {
    local image_tag="$1"
    echo "$image_tag" | awk -F: '{print $2}' | awk -F- '{print $1}'
}

# Get the image of the most recently created active revision
image_tag=$(az containerapp revision list --name "$APP_NAME" --resource-group "$RESOURCE_GROUP" \
    --query "sort_by([?properties.active==\`true\`], &properties.createdTime)[-1].properties.template.containers[0].image" \
    -o tsv)

# Extract the version from the image tag
version=$(extract_version "$image_tag")

if [ -z "$version" ]; then
    echo "Error: Unable to extract version from image tag" >&2
    exit 1
fi

# Validate the extracted version
if ! [[ $version =~ ^[0-9]+\.[0-9]+\.[0-9]+$ ]]; then
    echo "Error: Extracted version '$version' is not in the expected format (x.y.z)" >&2
    exit 1
fi

# Output only the version
echo "$version"
