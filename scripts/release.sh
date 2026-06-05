#!/bin/bash

version=$1
notes=$2

if [ "$(git branch --show-current)" != "main" ]; then
  git checkout main
fi

git pull

# Bump package version to the release version
jq --arg v "$version" '.version = $v' io.chainsafe.open-creator-rails/package.json > tmp.json \
  && mv tmp.json io.chainsafe.open-creator-rails/package.json

./scripts/updateSubmodules.sh $version

./scripts/duplicateSamples.sh

git add io.chainsafe.open-creator-rails/

echo "Please make sure ./SampleProject compiles and all tests pass, run ./scripts/localSetup.sh to run dependencies locally. "

read -p "Continue? (y/n): " response </dev/tty

if [ $response != "Y" ] && [ $response != "y" ]; then
  exit 1;
fi

git commit -m "Release $version" --allow-empty
git push origin main

gh release create "$version" \
  --title "$version" \
  --notes "$notes" \
  io.chainsafe.open-creator-rails/package.json

git tag "v$version" HEAD
git push origin "v$version"