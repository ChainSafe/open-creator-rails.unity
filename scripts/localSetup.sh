#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
OPEN_CREATOR_RAILS_ROOT="$(cd "$ROOT/open-creator-rails" && pwd)"

cleanup() {
    if [ -n "$PONDER_PID" ]; then
      kill "$PONDER_PID" 2>/dev/null || true
    fi
    
    if [ -n "$ANVIL_PID" ]; then
      kill "$ANVIL_PID" 2>/dev/null || true
    fi
    
    popd >/dev/null || true
}

trap cleanup EXIT

pushd "$OPEN_CREATOR_RAILS_ROOT" >/dev/null || exit 1

source .env.local

export SOURCE_SCRIPT=true

anvil &
ANVIL_PID=$!

until nc -z -w 1 127.0.0.1 8545; do :; done

source ./scripts/utils.sh

echo "[]" > "./$(get_deployments_file)"

./scripts/seed.sh

CHAIN_ID=$(cast chain-id --rpc-url "$RPC_URL")

TOKEN_ADDRESS=$(get_token_address)

DEPLOYMENTS_FILE=$(get_deployments_file)

popd >/dev/null || true

# PONDER INDEXER
INDEXER_ROOT="$(cd "$ROOT/open-creator-rails.indexer" && pwd)"

pushd "$INDEXER_ROOT" >/dev/null || exit 1

DEPLOY_DIR="$INDEXER_ROOT/config/deployments"

mkdir -p DEPLOY_DIR

cp "$OPEN_CREATOR_RAILS_ROOT/$DEPLOYMENTS_FILE" "$DEPLOY_DIR/registries_$CHAIN_ID.json"

DEPLOYMENTS_FILE="$DEPLOY_DIR/registries_$CHAIN_ID.json"

TOKEN_ADDRESSES_FILE="$DEPLOY_DIR/token_addresses.json"

if [ ! -f TOKEN_ADDRESSES_FILE ]; then
    echo "{}" > "$TOKEN_ADDRESSES_FILE"
fi

jq --arg chainId "$CHAIN_ID" \
   --arg address "$TOKEN_ADDRESS" \
'.[$chainId] = $address' \
"$TOKEN_ADDRESSES_FILE" > tmp.json && mv tmp.json "$TOKEN_ADDRESSES_FILE"

node "./scripts/sync-deployments.js"

rm -rf "./.ponder/pglite"

PONDER_RPC_URL_31337=http://127.0.0.1:8545 pnpm dev &
PONDER_PID=$!

wait $PONDER_PID