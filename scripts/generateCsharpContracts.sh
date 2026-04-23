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
for contract in "${contracts[@]}"; do
    jq '.abi' "out/${contract}.sol/${contract}.json" > "${contract}.abi"
done

popd

Nethereum.Generator.Console generate from-project
