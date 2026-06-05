#!/bin/bash

version=$1
notes=$2

if [ "$(git branch --show-current)" != "main" ]; then
  git checkout main
fi

git pull

./scripts/updateSubmodules.sh $version

./scripts/duplicateSamples.sh

git add io.chainsafe.open-creator-rails/

echo "Please make sure ./SampleProject compiles and all tests pass, run ./scripts/localSetup.sh to run dependencies locally. "

read -n 1 -p "Continue? (y/n): " response </dev/tty

if [ $response != "Y" ] && [ $response != "y" ]; then
  exit 1;
fi

git commit -m "Release $version" --allow-empty
git push origin main

gh release create "$version" \
  --title "$version" \
  --notes "$notes" \
  deployments/*

git tag "v$version" "$version"
git push origin "v$version"