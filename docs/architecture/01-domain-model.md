# 01 — Domain Model

Protocol-level concepts that both SDKs share. These classes/types have **no platform dependency** — they exist as pure data and behaviour contracts. Every SDK layer must faithfully represent them.

## Class Diagram

```mermaid
classDiagram
    direction TB

    class SubscriberIdentity {
        +subscriberId : string
        +address : Address
        +toBytes32() bytes32
    }
    note for SubscriberIdentity "Canonical formula (B2):\nkeccak256(abi.encode(subscriberId, address))\n\nsubscriberId is application-defined (e.g. 'user-uuid-123').\naddress is the wallet that signs transactions.\nBoth are REQUIRED — no default derivation."

    class Subscription {
        +startTime : uint256
        +endTime : uint256
        +subscriptionPrice : uint256
        +registryFeeShare : uint256
        +payer : address
    }
    note for Subscription "Active when block.timestamp < endTime.\nStored on-chain under bytes32 key = SubscriberIdentity.toBytes32()."

    class CancellationCommitment {
        +subscriberKey : bytes32
        +timestamp : uint256
    }
    note for CancellationCommitment "Intermediate state in two-step cancel.\nsubscriberKey = keccak256(abi.encode(subscriberId, msg.sender)).\nExpires when cancelSubscription() is successfully called."

    class Asset {
        +assetId : bytes32
        +owner : address
        +subscriptionPrice : uint256
        +tokenAddress : address
        +registryAddress : address
        +subscribe(subscriberId, payer, permit) uint256
        +commitCancellation(subscriberId) uint256
        +cancelSubscription(subscriberId, timestamp, signature)
        +revokeSubscription(subscriber) onlyOwner
        +isSubscriptionActive(subscriber) bool
        +getSubscription(subscriber) uint256
        +getSubscriptionPrice(duration) uint256
        +claimCreatorFee(subscriber)
        +setSubscriptionPrice(price) onlyOwner
    }

    class AssetRegistry {
        +registryFeeShare : uint256
        +assets : mapping bytes32→address
        +createAsset(assetId, price, token, owner) address
        +subscribe(assetId, subscriberId, payer, ...) uint256
        +getAsset(assetId) address
        +viewAsset(assetId) bool
        +isSubscriptionActive(assetId, subscriber) bool
        +getSubscription(assetId, subscriber) uint256
        +getSubscriptionPrice(assetId, duration) uint256
        +claimRegistryFee(assetId, subscriber)
    }

    AssetRegistry "1" --> "*" Asset : deploys and routes
    Asset "1" --> "*" Subscription : stores (bytes32 → Subscription)
    Asset "1" --> "*" CancellationCommitment : pending (bytes32 → uint256)
    SubscriberIdentity --> Subscription : identity key
    SubscriberIdentity --> CancellationCommitment : identity key
```

## Key Invariants

| Concept | Rule |
|---------|------|
| `SubscriberIdentity.toBytes32()` | `keccak256(abi.encode(subscriberId, address))` — both fields required |
| Subscription active | `block.timestamp < endTime` |
| On-chain authority | On-chain state supersedes indexer — indexer is a read cache only |
| Two-step cancel | `commitCancellation` → sign payload → `cancelSubscription` — single-step revocation is owner-only (`revokeSubscription`) |
| Fee split | Revenue split at subscription time; `registryFeeShare` is set at registry level |

## Events Emitted by Asset

| Event | When |
|-------|------|
| `SubscriptionAdded(bytes32 subscriber, uint256 startTime, uint256 endTime, uint256 nonce, address payer, uint256 price, uint256 registryFeeShare)` | New subscription |
| `SubscriptionExtended(bytes32 subscriber, uint256 endTime)` | Subscription renewed before expiry |
| `SubscriptionCancelled(bytes32 subscriber)` | Two-step cancel completed |
| `SubscriptionRevoked(bytes32 subscriber)` | Owner-revoked subscription |

> **Note:** `SubscriptionCancelled` emits only `bytes32 subscriber`. The original `subscriberId` string and `address` cannot be recovered from the event alone. This is a known indexer observability gap — see [05-current-divergences.md](./05-current-divergences.md).
