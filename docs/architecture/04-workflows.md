# 04 — Workflows

Sequence diagrams for the three core SDK workflows. These diagrams are normative — SDK implementations must conform to these interaction patterns.

---

## 1. Subscribe

A subscriber obtains access to an asset by paying via ERC20 permit (EIP-2612). The permit avoids a separate `approve` transaction.

```mermaid
sequenceDiagram
    title Subscribe — Full Flow
    autonumber

    actor dApp
    participant SDK
    participant IWalletProvider as Wallet
    participant IPermitService as Permit
    participant AssetRegistry
    participant Asset
    participant Token

    dApp->>SDK: subscribe(assetId, subscriberId, payer, duration)
    SDK->>AssetRegistry: getSubscriptionPrice(assetId, duration)
    AssetRegistry->>Asset: getSubscriptionPrice(duration)
    Asset-->>AssetRegistry: price
    AssetRegistry-->>SDK: price

    SDK->>Permit: buildPermit(owner=payer, spender=assetAddress, value=price, deadline)
    Permit->>Token: nonces(payer)
    Token-->>Permit: nonce
    Permit-->>SDK: Permit struct + EIP-712 typed data

    SDK->>Wallet: signTypedData(permit, typedData)
    Wallet-->>SDK: EthECDSASignature (v, r, s)

    Note over SDK: subscriberBytes32 = keccak256(abi.encode(subscriberId, payer))

    SDK->>AssetRegistry: subscribe(assetId, subscriberBytes32, payer, assetAddress, value, deadline, v, r, s)
    AssetRegistry->>Asset: subscribe(subscriberBytes32, payer, spender, value, deadline, v, r, s)
    Asset->>Token: permit(payer, assetAddress, value, deadline, v, r, s)
    Token-->>Asset: ok
    Asset->>Token: transferFrom(payer, asset, value)
    Token-->>Asset: ok
    Asset->>Asset: _addSubscription(subscriberBytes32, startTime, endTime, ...)
    Asset-->>Asset: emit SubscriptionAdded(subscriberBytes32, startTime, endTime, nonce, payer, price, registryFeeShare)
    Asset-->>AssetRegistry: endTime
    AssetRegistry-->>SDK: endTime
    SDK-->>dApp: endTime (DateTime)
```

**SDK responsibility:** derive `subscriberBytes32` before the contract call. Neither the registry nor the asset derives it — the SDK is the derivation boundary.

---

## 2. Cancel Subscription (Two-Step)

Subscriber-initiated cancellation uses a commit-reveal pattern to prevent front-running. The owner can bypass this with `revokeSubscription`.

```mermaid
sequenceDiagram
    title Cancel Subscription — Two-Step Commit-Reveal
    autonumber

    actor dApp
    participant SDK
    participant IWalletProvider as Wallet
    participant Asset

    dApp->>SDK: commitCancellation(assetAddress, subscriberId)
    Note over SDK: msg.sender = subscriberAddress (wallet)

    SDK->>Asset: commitCancellation(subscriberId)
    Note over Asset: key = keccak256(abi.encode(subscriberId, msg.sender))\ncancellations[key] = block.timestamp
    Asset-->>SDK: timestamp (uint256)
    SDK-->>dApp: timestamp

    Note over dApp: dApp stores timestamp, waits for tx confirmation

    dApp->>SDK: cancelSubscription(assetAddress, subscriberId, timestamp)

    Note over SDK: Build EIP-712 cancellation payload:\n{ assetAddress, subscriberId, timestamp }

    SDK->>Wallet: signTypedData(cancellationPayload, domain)
    Wallet-->>SDK: signature (bytes)

    SDK->>Asset: cancelSubscription(subscriberId, timestamp, signature)
    Note over Asset: key = keccak256(abi.encode(subscriberId, msg.sender))\nverify cancellations[key] == timestamp\nverify ECDSA signature\n_removeSubscription(key)\nemit SubscriptionCancelled(key)
    Asset-->>SDK: tx receipt
    SDK-->>dApp: ok
```

**Key security properties:**
- The commit binds `(subscriberId, msg.sender)` on-chain — no other address can cancel for this subscriber.
- The reveal requires a valid ECDSA signature from the same wallet, preventing replay.
- Owner can always call `revokeSubscription(bytes32 subscriber)` as an emergency escape hatch — this is single-step.

---

## 3. HasAccess (with On-Chain Fallback)

Access checks use the indexer for speed but always fall back to the authoritative on-chain state.

```mermaid
sequenceDiagram
    title HasAccess — Indexer with On-Chain Fallback
    autonumber

    actor dApp
    participant SDK
    participant IIndexerProvider as Indexer
    participant Asset

    dApp->>SDK: getSubscriptionStatus(assetAddress, user, source="auto")

    alt source == "indexer" or (source == "auto" and indexer configured)
        SDK->>Indexer: getSubscription(assetAddress, user)
        alt indexer returns data
            Indexer-->>SDK: IndexerSubscription { isActive, endTime }
            SDK-->>dApp: SubscriptionStatus { isActive, endTime }
        else indexer unavailable or no record
            Note over SDK: Fall through to on-chain
        end
    end

    SDK->>Asset: isSubscriptionActive(subscriberBytes32)
    Asset-->>SDK: bool
    SDK->>Asset: getSubscription(subscriberBytes32)
    Asset-->>SDK: endTime (uint256)
    SDK-->>dApp: SubscriptionStatus { isActive, endTime }
```

**Source parameter semantics:**

| `source` | Behaviour |
|----------|-----------|
| `"auto"` (default) | Indexer if configured → on-chain if indexer fails or returns nothing |
| `"indexer"` | Indexer only — throws if unavailable |
| `"onchain"` | On-chain only — skips indexer entirely |

**Unity note:** `HasAccess(subscriberId)` currently calls `isSubscriptionActive(Keccack256Bytes(subscriberId))` directly on-chain (no indexer fallback). The on-chain path is correct once the derivation formula is fixed; indexer fallback is tracked as an enhancement (see [05-current-divergences.md](./05-current-divergences.md)).
