using System;
using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct Subscription
    {
        public string AssetIdHash { get; private set; }

        public string SubscriberIdHash { get; private set; }

        public EthereumAddress Payer { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public bool IsActive { get; private set; }

        public BigInteger Nonce { get; private set; }

        public EthereumAddress RegistryAddress { get; private set; }

        public Subscription(string assetIdHash, string subscriberIdHash, EthereumAddress payer, DateTime startTime,
            DateTime endTime, bool isActive, BigInteger nonce, EthereumAddress registryAddress)
        {
            AssetIdHash = assetIdHash;
            SubscriberIdHash = subscriberIdHash;
            Payer = payer;
            StartTime = startTime;
            EndTime = endTime;
            IsActive = isActive;
            Nonce = nonce;
            RegistryAddress = registryAddress;
        }
    }
}