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
    public interface IAsset : IWeb3Initialized
    {
        public EthereumAddress RegistryAddress { get; }

        public string AssetId { get; }

        public string AssetIdHash => AssetId.Keccack256();

        public EthereumAddress Address { get; }

        public BigInteger SubscriptionPrice { get; }

        public EthereumAddress Owner { get; }

        public EthereumAddress TokenAddress { get; }

        public List<SubscriptionDto> Subscriptions { get; }

        public AssetService Service { get; }

        public ERC20PermitService PermitService { get; }

        public AssetRegistryService AssetRegistryService { get; }
        
        // For subscriber
        // UniTask<DateTime> GetSubscriptionExpiration(string subscriberId);
        UniTask<DateTime> Subscribe(string subscriberId, TimeSpan duration);
        // UniTask CancelSubscription(string subscriberId);

        // For Asset Owner
        // UniTask SetSubscriptionPrice(BigInteger newSubscriptionPrice);
        // UniTask<BigInteger> ClaimCreatorFee(string subscriberId);
        // UniTask<BigInteger> ClaimCreatorFee(string[] subscriberIds);
        // UniTask RevokeSubscription(string subscriberId);
    }
}