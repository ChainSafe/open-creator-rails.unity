using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.ERC20Permit.Service;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Represents an on-chain asset managed by an <c>AssetRegistry</c> contract.
    /// Exposes the asset's configuration, and all relevant subscriber and owner operations.
    /// </summary>
    public interface IAsset : IInitializeHandler, IWeb3Initialized, IDisconnectedHandler
    {
        /// <summary>Address of the <c>AssetRegistry</c> contract that deployed this asset.</summary>
        public EthereumAddress RegistryAddress { get; }

        /// <summary>Human-readable asset identifier.</summary>
        public string AssetId { get; }

        /// <summary>
        /// Keccak-256 hex hash of <see cref="AssetId"/>. This is the <c>bytes32</c> value
        /// used on-chain to identify the asset.
        /// </summary>
        public string AssetIdHash => AssetId.Keccack256();

        /// <summary>On-chain address of the deployed <c>Asset</c> contract.</summary>
        public EthereumAddress Address { get; }

        /// <summary>
        /// Price for a single subscription period denominated in the payment token's smallest unit
        /// (e.g. wei for an 18-decimal token).
        /// </summary>
        public BigInteger SubscriptionPrice { get; }

        public decimal SubscriptionPricePrimaryUnit => (decimal)SubscriptionPrice / TokenDecimals.PowerOfTen();

        /// <summary>Fixed length of one subscription period. Subscriptions are always whole multiples of this duration.</summary>
        public TimeSpan SubscriptionDuration { get; }

        /// <summary>
        /// Current owner of the asset.
        /// </summary>
        public EthereumAddress Owner { get; }

        /// <summary>
        /// Address of the ERC-20 payment token used for subscriptions.
        /// The token must implement ERC-2612 (permit).
        /// </summary>
        public EthereumAddress TokenAddress { get; }

        public string TokenSymbol { get; }

        public BigInteger TokenDecimals { get; }

        /// <summary>
        /// Cached list of all known subscription records for this asset,
        /// kept in sync with the chain via contract events after <see cref="IWeb3Initialized.Connected"/> is called.
        /// </summary>
        public List<SubscriptionDto> Subscriptions { get; }

        /// <summary>Low-level Nethereum service wrapper for the <c>Asset</c> contract.</summary>
        public AssetService Service { get; }

        /// <summary>Low-level Nethereum service wrapper for the ERC-20 permit token used by this asset.</summary>
        public ERC20PermitService PermitService { get; }

        /// <summary>Low-level Nethereum service wrapper for the <c>AssetRegistry</c> that owns this asset.</summary>
        public AssetRegistryService AssetRegistryService { get; }

        /// <summary>
        /// Re-fetches the asset's full state (configuration and all subscription records) from
        /// the indexer and updates the local cache. Use this to force a refresh outside the
        /// normal event-driven update cycle.
        /// </summary>
        UniTask Refresh();

        // ── Subscriber operations ──────────────────────────────────────────────

        /// <summary>
        /// Returns the local <see cref="DateTime"/> at which the subscriber's current subscription expires.
        /// Returns <see cref="DateTime.MinValue"/> if no subscription has ever been created for this subscriber.
        /// Read-only.
        /// </summary>
        /// <param name="subscriberId">
        /// Plain-text subscriber identity string. The SDK derives the on-chain hash as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        /// <returns>The subscription expiry as a local <see cref="DateTime"/>.</returns>
        UniTask<DateTime> GetSubscriptionExpiration(string subscriberId);

        /// <summary>
        /// Returns <c>true</c> if the subscriber's subscription has expired (expiry &lt;= now)
        /// or has never existed.
        /// Read-only
        /// </summary>
        /// <param name="subscriberId">
        /// Plain-text subscriber identity string. The SDK derives the on-chain hash as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        /// <returns><c>true</c> if the subscription is expired or absent; otherwise <c>false</c>.</returns>
        UniTask<bool> IsSubscriptionExpired(string subscriberId);

        /// <summary>
        /// Returns <c>true</c> if the subscriber has been permanently revoked by the asset owner.
        /// Read-only
        /// </summary>
        /// <param name="subscriberId">
        /// Plain-text subscriber identity string. The SDK derives the on-chain hash as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        /// <returns><c>true</c> if the subscriber is revoked; otherwise <c>false</c>.</returns>
        UniTask<bool> IsSubscriberRevoked(string subscriberId);

        /// <summary>
        /// Returns <c>true</c> if the subscriber has an active subscription, meaning it is
        /// neither expired nor revoked.
        /// Read-only
        /// </summary>
        /// <param name="subscriberId">
        /// Plain-text subscriber identity string. The SDK derives the on-chain hash as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        /// <returns><c>true</c> if the subscription is active; otherwise <c>false</c>.</returns>
        UniTask<bool> IsSubscriptionActive(string subscriberId);

        /// <summary>
        /// Returns the total token cost and total time for the given number of subscription periods.
        /// Use this to preview the payment required before calling <see cref="Subscribe"/>.
        /// Read-only.
        /// </summary>
        /// <param name="count">Number of subscription periods to calculate for (must be at least 1).</param>
        /// <returns>
        /// A tuple of (<c>price</c>, <c>duration</c>) where <c>price</c> is the total token amount
        /// in the token's smallest unit and <c>duration</c> is the total subscription time.
        /// </returns>
        UniTask<(BigInteger price, TimeSpan duration)> GetSubscriptionPriceAndDuration(BigInteger count);

        /// <summary>
        /// Subscribes the connected wallet for the given number of periods, or extends/renews
        /// an existing subscription. Payment is collected via an ERC-2612 permit, no separate
        /// approval transaction is required.
        /// <para>
        /// The connected wallet is both the payer and subscriber. The payer is the refund
        /// beneficiary if the subscription is later cancelled or revoked.
        /// </para>
        /// <para>
        /// Reverts if the subscriber is permanently revoked.
        /// </para>
        /// </summary>
        /// <param name="subscriberId">
        /// Plain-text subscriber identity string. The SDK derives the on-chain hash as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        /// <param name="count">Number of full subscription periods to purchase (must be at least 1).</param>
        /// <returns>The new subscription expiry as a local <see cref="DateTime"/>.</returns>
        UniTask<DateTime> Subscribe(string subscriberId, BigInteger count);

        /// <summary>
        /// Cancels the connected wallet's subscription by submitting an off-chain ECDSA signature.
        /// <para>
        /// Only whole remaining subscription periods are refunded to the original payer;
        /// the current (partial) period is non-refundable.
        /// </para>
        /// <para>
        /// The connected wallet must be the subscriber address used when the
        /// subscription was created. Reverts if the subscriber is permanently revoked.
        /// </para>
        /// </summary>
        /// <param name="subscriberId">
        /// Plain-text subscriber identity string. The SDK derives the on-chain hash as
        /// <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        UniTask CancelSubscription(string subscriberId);

        // ── Asset Owner operations ─────────────────────────────────────────────

        /// <summary>
        /// Updates the per-period subscription price.
        /// <para>
        /// <b>Asset Owner only</b>.
        /// </para>
        /// </summary>
        /// <param name="newSubscriptionPrice">
        /// New price per subscription period in the token's smallest unit
        /// (e.g. wei for an 18-decimal token).
        /// </param>
        UniTask SetSubscriptionPrice(BigInteger newSubscriptionPrice);

        /// <summary>
        /// Claims all accrued creator fees for a single subscriber. Accrual covers completed
        /// subscription periods since the last claim; dust from a fully-ended subscription is
        /// also included.
        /// <para>
        /// <b>Asset Owner only</b>.
        /// </para>
        /// </summary>
        /// <param name="subscriberIdHash">
        /// Subscriber identity hash derived from <c>keccak256(abi.encode(subscriberId, subscriberAddress))</c>.
        /// </param>
        /// <returns>The amount of creator fee claimed, in the token's smallest unit.</returns>
        UniTask<BigInteger> ClaimCreatorFee(string subscriberIdHash);

        /// <summary>
        /// Claims all accrued creator fees for a single subscriber, using an explicit subscriber
        /// address to compute the identity hash.
        /// <para><b>Asset Owner only</b>.</para>
        /// </summary>
        /// <param name="subscriberId">The plain-text subscriber identity string.</param>
        /// <param name="subscriberAddress">
        /// The wallet address bound to the subscriber identity. The on-chain hash is derived as
        /// <c>keccak256(abi.encode(subscriberId, subscriberAddress))</c>.
        /// </param>
        /// <returns>The amount of creator fee claimed, in the token's smallest unit.</returns>
        UniTask<BigInteger> ClaimCreatorFee(string subscriberId, EthereumAddress subscriberAddress);

        /// <summary>
        /// Claims accrued creator fees for multiple subscribers in a single transaction.
        /// Subscribers with no accrued fee are silently skipped.
        /// <para>
        /// <b>Asset Owner only</b>.
        /// </para>
        /// </summary>
        /// <param name="subscriberIdHashes">
        /// Array of subscriber identity hashes derived from <c>keccak256(abi.encode(subscriberId, subscriberAddress))</c>.
        /// </param>
        /// <returns>The total creator fee claimed across all subscribers, in the token's smallest unit.</returns>
        UniTask<BigInteger> ClaimCreatorFee(string[] subscriberIdHashes);

        /// <summary>
        /// Claims accrued creator fees for multiple subscribers in a single transaction, using
        /// explicit subscriber addresses to compute identity hashes.
        /// Subscribers with no accrued fee are silently skipped.
        /// <para><b>Asset Owner only</b>.</para>
        /// </summary>
        /// <param name="subscribers">
        /// Array of <c>(subscriberId, subscriberAddress)</c> pairs. Each identity hash is derived as
        /// <c>keccak256(abi.encode(subscriberId, subscriberAddress))</c>.
        /// </param>
        /// <returns>The total creator fee claimed across all subscribers, in the token's smallest unit.</returns>
        UniTask<BigInteger> ClaimCreatorFee((string subscriberId, EthereumAddress subscriberAddress)[] subscribers);

        /// <summary>
        /// Revokes a subscriber's subscription and immediately terminating their access.
        /// All remaining time (including partial-period dust) is refunded to the original payer(s).
        /// The subscriber is permanently blocked from resubscribing and cancelling until
        /// <see cref="UnrevokeSubscription(string)"/> is called.
        /// <para>
        /// <b>Asset Owner only</b>.
        /// </para>
        /// </summary>
        /// <param name="subscriberIdHash">
        /// Subscriber identity hash derived from <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        UniTask RevokeSubscription(string subscriberIdHash);

        /// <summary>
        /// Revokes a subscriber's subscription using an explicit subscriber address to compute
        /// the identity hash.
        /// <para><b>Asset Owner only</b>.</para>
        /// </summary>
        /// <param name="subscriberId">The plain-text subscriber identity string.</param>
        /// <param name="subscriberAddress">
        /// The wallet address bound to the subscriber identity. The on-chain hash is derived as
        /// <c>keccak256(abi.encode(subscriberId, subscriberAddress))</c>.
        /// </param>
        UniTask RevokeSubscription(string subscriberId, EthereumAddress subscriberAddress);

        /// <summary>
        /// Lifts a permanent revocation for a subscriber, allowing them to resubscribe.
        /// <para>
        /// <b>Asset Owner only</b>.
        /// </para>
        /// </summary>
        /// <param name="subscriberIdHash">
        /// Subscriber identity hash derived from <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// </param>
        UniTask UnrevokeSubscription(string subscriberIdHash);

        /// <summary>
        /// Lifts a permanent revocation for a subscriber using an explicit subscriber address to
        /// compute the identity hash.
        /// <para><b>Asset Owner only</b>.</para>
        /// </summary>
        /// <param name="subscriberId">The plain-text subscriber identity string.</param>
        /// <param name="subscriberAddress">
        /// The wallet address bound to the subscriber identity. The on-chain hash is derived as
        /// <c>keccak256(abi.encode(subscriberId, subscriberAddress))</c>.
        /// </param>
        UniTask UnrevokeSubscription(string subscriberId, EthereumAddress subscriberAddress);
    }
}