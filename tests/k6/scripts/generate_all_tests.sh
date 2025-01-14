#!/bin/bash

# Get directory path from the first command line argument
DIRECTORY_PATH="$1"

# Verify that the directory exists
if [[ ! -d "$DIRECTORY_PATH" ]]; then
    echo "The supplied directory does not exist!"
    exit 1
fi

# Initialize import and function strings
IMPORT_STATEMENTS=""
FUNCTION_BODY=""

for JS_FILE in "$DIRECTORY_PATH"/*.js; do
    # Skip files in the "performance" directory or the "all-tests.js" file
    if [[ $DIRECTORY_PATH == *"/performance/"* ]] ||
    [[ $(basename "$JS_FILE" .js) == "all-tests" ]]; then
        continue
    fi

    BASE_NAME=$(basename "$JS_FILE" .js)

    # Append to import strings
    IMPORT_STATEMENTS="${IMPORT_STATEMENTS}import { default as ${BASE_NAME} } from './${BASE_NAME}.js';"$'\n'

    # Append to function body
    FUNCTION_BODY="${FUNCTION_BODY}  ${BASE_NAME}();"$'\n'
done

# Combine all the content
SCRIPT_CONTENT="// This file is generated, see \"scripts\" directory
${IMPORT_STATEMENTS}
export default function() {
${FUNCTION_BODY}}"

# Output the script content to "all-tests.js"
echo "$SCRIPT_CONTENT" >"${DIRECTORY_PATH}/all-tests.js"

echo "all-tests.js has been generated successfully."
