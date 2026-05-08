#!/bin/bash
# TODO only pull releases via Git tags or something
git submodule foreach 'git fetch origin && git checkout main && git pull origin main'

./scripts/generateCsharpContracts.sh