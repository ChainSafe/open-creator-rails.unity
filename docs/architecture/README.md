# Open Creator Rails — Unified SDK Architecture

This directory contains the normative architecture documentation for all Open Creator Rails SDK implementations. It closes [issue #22](https://github.com/ChainSafe/open-creator-rails.unity/issues/22) and establishes the **unified SDK interface contract** that all current and future SDK platforms must conform to.

## Document Map

| File | Contents | Audience |
|------|----------|----------|
| [01-domain-model.md](./01-domain-model.md) | Protocol-level concepts: `SubscriberIdentity`, `Subscription`, `Asset`, `AssetRegistry`, `CancellationCommitment` | All |
| [02-sdk-interfaces.md](./02-sdk-interfaces.md) | Shared abstractions: `IAssetClient`, `ISubscriptionService`, `IIndexerProvider`, `IWalletProvider`, `IPermitService` | SDK developers |
| [03-platform-adapters.md](./03-platform-adapters.md) | Concrete realizations: TypeScript (`OcrSdk` + viem) and Unity (`OpenCreatorRailsService` + Nethereum) | SDK developers |
| [04-workflows.md](./04-workflows.md) | Sequence diagrams: subscribe, two-step cancel, hasAccess with fallback | SDK developers, integration engineers |
| [05-current-divergences.md](./05-current-divergences.md) | Gap register: current vs. target state per SDK, with file citations and alignment actions | TPM, SDK leads |

Read in order for full context. For a quick integration summary, start with `01` and `04`.

---

## Interpretation Guide

### Subscriber Identity (B2 Model — locked)

Every subscriber is identified by **two fields**:

| Field | Type | Example |
|-------|------|---------|
| `subscriberId` | `string` | `"user-uuid-4a2b3c"` |
| `address` | `Address` (wallet) | `0xabc...def` |

The on-chain `bytes32` key is always:

```
keccak256(abi.encode(subscriberId, address))
```

- `subscriberId` is **application-defined** — it is NOT derived from the wallet address.  
- The same wallet can hold subscriptions under different `subscriberId` values.  
- There is no default: callers must always supply both fields.

This is called **Interpretation B2** and supersedes all previous derivation approaches. See [01-domain-model.md](./01-domain-model.md) for full rationale.

### Source Parameter

All `getSubscriptionStatus` and `hasAccess` methods accept an optional `source` parameter:

- `"auto"` — try indexer, fall back to on-chain (default)
- `"indexer"` — indexer only, throws if unavailable
- `"onchain"` — skip indexer, always hit the contract

The indexer is never authoritative. On-chain state always wins on conflict.

### Diagram Conventions

- Mermaid class diagrams use `<<interface>>` for abstractions.
- `⚠ TODO` annotations in class diagrams mark methods that are declared but not yet implemented (see `05-current-divergences.md` for issue tracking).
- `✓ implemented` marks confirmed, reviewed implementations.
- All sequence diagrams use `autonumber` — step numbers are stable reference points in code review comments.

---

## Relationship to Invariants

These diagrams are the normative source for two `.invariants` files:

| File | Scope | Changed in this PR |
|------|-------|--------------------|
| `open-creator-rails.unity/.invariants` | Unity SDK assertions | Yes — `subscriber_identity_derivation` rewritten, `cancel_commitment_protocol` added |
| `/.invariants` (workspace apex) | Cross-repo protocol guarantees | Companion change (separate commit, TPM sign-off required) |

The invariants are machine-readable assertions consumed by the OCR conformance agent. Keeping them in sync with these diagrams is a **TPM responsibility** — the conformance agent will flag any new issue that touches FROZEN surfaces.

---

## Maintenance Process

### When to update this documentation

| Trigger | Files to update |
|---------|----------------|
| New SDK method added | `02-sdk-interfaces.md`, `03-platform-adapters.md`, `05-current-divergences.md` (remove gap row) |
| Subscriber derivation formula change | `01-domain-model.md`, `03-platform-adapters.md`, `04-workflows.md`, both `.invariants` files — **requires TPM sign-off** |
| New cancel/subscribe flow | `04-workflows.md`, `02-sdk-interfaces.md` |
| New SDK platform (e.g. mobile) | All files — add platform column/section to `03-platform-adapters.md` |
| New gap discovered | `05-current-divergences.md` — add row immediately, link GitHub issue |
| Gap resolved | `05-current-divergences.md` — update severity to RESOLVED, keep row for audit trail |

### Review requirements

- Changes to `01-domain-model.md` or either `.invariants` file require **TPM approval**.
- Changes to `03-platform-adapters.md` that modify the `⚠ TODO` / `✓ implemented` status require a corresponding code-level PR to be merged first.
- Changes to `05-current-divergences.md` that close a gap row require a passing CI run on the implementation PR.

### Diagram validation

All Mermaid diagrams render on GitHub. Before merging any PR that touches these files:
1. Open the file in GitHub's preview — verify all diagrams render without parse errors.
2. Check that cross-links between files resolve (click each `[filename.md]` link in preview).
3. Run the conformance agent against any `.invariants` change — the agent must not flag new FROZEN violations.

---

## Background

This documentation was produced as part of the ratification of protocol changes introduced in `open-creator-rails` PRs #115 and #119. Those PRs changed the `cancelSubscription` ABI and `SubscriptionAdded` event signature without a prior architecture review, creating FROZEN-level invariant violations across all SDKs.

This architecture set:
1. Establishes the canonical unified design going forward (B2 subscriber model).
2. Documents the current gaps clearly so implementation PRs have a precise target.
3. Updates the `.invariants` files to reflect the new canonical state, so the conformance agent can correctly classify future change requests.

See `reporting_docs/ratification-plan-pr115-pr119.md` in the workspace for the full ratification context.
