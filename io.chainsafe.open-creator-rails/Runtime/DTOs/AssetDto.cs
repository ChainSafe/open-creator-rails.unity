using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct AssetDto
    {
        public EthereumAddress Address { get; private set; }

        public BigInteger SubscriptionPrice { get; private set; }

        public EthereumAddress Owner { get; private set; }

        public EthereumAddress TokenAddress { get; private set; }

        public SubscriptionDto[] Subscriptions { get; private set; }

        public AssetDto(EthereumAddress address, BigInteger subscriptionPrice, EthereumAddress owner,
            EthereumAddress tokenAddress, SubscriptionDto[] subscriptions)
        {
            Address = address;
            SubscriptionPrice = subscriptionPrice;
            Owner = owner;
            TokenAddress = tokenAddress;
            Subscriptions = subscriptions;
        }
    }
}