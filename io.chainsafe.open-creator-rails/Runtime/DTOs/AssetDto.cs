using System;
using System.Collections.Generic;
using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    /// <summary>
    /// Immutable snapshot of an on-chain asset's configuration and subscription state,
    /// as returned by <see cref="IIndexerProvider.GetAsset"/>. Used to populate an
    /// <see cref="IAsset"/> after <see cref="OpenCreatorRailsService.Connect"/> completes.
    /// </summary>
    public struct AssetDto
    {
        /// <summary>On-chain address of the deployed <c>Asset</c> contract.</summary>
        public EthereumAddress Address { get; private set; }

        /// <summary>
        /// Price for a single subscription period in the payment token's smallest unit
        /// (e.g. wei for an 18-decimal token).
        /// </summary>
        public BigInteger SubscriptionPrice { get; private set; }

        /// <summary>Fixed length of one subscription period. Subscriptions are always whole multiples of this duration.</summary>
        public TimeSpan SubscriptionDuration { get; private set; }

        /// <summary>Address of the asset owner who receives the creator share of subscription fees.</summary>
        public EthereumAddress Owner { get; private set; }

        /// <summary>
        /// Address of the ERC-20 payment token used for subscriptions.
        /// The token must implement ERC-2612 (permit).
        /// </summary>
        public EthereumAddress TokenAddress { get; private set; }

        /// <summary>All subscription records known to the indexer at the time of the query.</summary>
        public List<SubscriptionDto> Subscriptions { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="AssetDto"/> with the given asset configuration and subscription list.
        /// </summary>
        /// <param name="address">On-chain address of the <c>Asset</c> contract.</param>
        /// <param name="subscriptionPrice">Price per subscription period in the token's smallest unit.</param>
        /// <param name="subscriptionDuration">Fixed length of one subscription period.</param>
        /// <param name="owner">Address of the asset owner.</param>
        /// <param name="tokenAddress">Address of the ERC-20 payment token (must support ERC-2612).</param>
        /// <param name="subscriptions">List of subscription records indexed for this asset.</param>
        public AssetDto(EthereumAddress address, BigInteger subscriptionPrice, TimeSpan subscriptionDuration,
            EthereumAddress owner, EthereumAddress tokenAddress, List<SubscriptionDto> subscriptions)
        {
            Address = address;
            SubscriptionPrice = subscriptionPrice;
            SubscriptionDuration = subscriptionDuration;
            Owner = owner;
            TokenAddress = tokenAddress;
            Subscriptions = subscriptions;
        }
    }
}
