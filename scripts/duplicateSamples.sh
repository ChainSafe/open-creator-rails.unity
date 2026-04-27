#!/bin/bash

# Get the directory where the script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

PACKAGE_JSON="$REPO_ROOT/io.chainsafe.open-creator-rails/package.json"

# Extract common values from package.json
VERSION=$(jq -r '.version' "$PACKAGE_JSON")
NAME=$(jq -r '.name' "$PACKAGE_JSON")
DISPLAY_NAME=$(jq -r '.displayName' "$PACKAGE_JSON")

# Iterate through samples
SAMPLES_LENGTH=$(jq '.samples | length' "$PACKAGE_JSON")

for (( i=0; i<$SAMPLES_LENGTH; i++ )); do
    SAMPLE_DISPLAY_NAME=$(jq -r ".samples[$i].displayName" "$PACKAGE_JSON")
    SAMPLE_PATH=$(jq -r ".samples[$i].path" "$PACKAGE_JSON")

    SOURCE_DIR="$REPO_ROOT/SampleProject/Assets/Samples/$DISPLAY_NAME/$VERSION/$SAMPLE_DISPLAY_NAME"
    DEST_DIR="$REPO_ROOT/$NAME/$SAMPLE_PATH"

    if [ ! -d "$SOURCE_DIR" ]; then
        continue
    fi

    # Make sure destination exists
    mkdir -p "$DEST_DIR"

    # Remove everything in destination folder
    rm -rf "${DEST_DIR:?}"/*

    # Copy everything from source to destination
    cp -r "$SOURCE_DIR"/. "$DEST_DIR/"

    # Force git add everything in the destination folder
    git add -f "$DEST_DIR"
done