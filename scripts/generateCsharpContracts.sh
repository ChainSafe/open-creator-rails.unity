#!/bin/bash
set -euo pipefail

CONTRACTS_DIRECTORY="open-creator-rails/"

GENERATOR_CONFIG="Nethereum.Generator.json"
mapfile -t contracts < <(jq -r '.ABIConfigurations[].ContractName' "$GENERATOR_CONFIG")

cleanup() {
    for contract in "${contracts[@]}"; do
      rm -f "$CONTRACTS_DIRECTORY/${contract}.abi"
    done
}
trap cleanup EXIT

pushd "$CONTRACTS_DIRECTORY"

forge build
for i in "${!contracts[@]}"; do
    contract="${contracts[$i]}"
    jq '.abi' "out/${contract}.sol/${contract}.json" > "${contract}.abi"
    
    BYTECODE=$(jq -r '.bytecode.object' "out/${contract}.sol/${contract}.json")
    GENERATOR_CONFIG_RELATIVE_PATH="../$GENERATOR_CONFIG"
    
    jq --argjson index "$i" \
    --arg bytecode "$BYTECODE" \
    --arg contractName "$contract" \
    '.ABIConfigurations[$index].ByteCode=$bytecode' \
    $GENERATOR_CONFIG_RELATIVE_PATH > "$GENERATOR_CONFIG_RELATIVE_PATH.tmp.json" && mv "$GENERATOR_CONFIG_RELATIVE_PATH.tmp.json" $GENERATOR_CONFIG_RELATIVE_PATH
done

popd

Nethereum.Generator.Console generate from-project
