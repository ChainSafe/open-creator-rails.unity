#!/bin/bash

release_tag="$1"

RELEASE_TAG="$release_tag" git submodule foreach '
  git add *
  git stash
  git reset --hard
  
  git fetch origin --tags

  default_ref=$(git symbolic-ref --short refs/remotes/origin/HEAD 2>/dev/null)
  default_branch=${default_ref#origin/}

  if [ -n "$RELEASE_TAG" ] && git rev-parse -q --verify "refs/tags/$RELEASE_TAG" >/dev/null; then
    git checkout "tags/$RELEASE_TAG"
  elif [ -n "v$RELEASE_TAG" ] && git rev-parse -q --verify "refs/tags/v$RELEASE_TAG" >/dev/null; then
    git checkout "tags/v$RELEASE_TAG"
  else
    git checkout "$default_branch"
    git pull origin "$default_branch"
  fi
  git submodule update --init --recursive
'

./scripts/generateCsharpContracts.sh
