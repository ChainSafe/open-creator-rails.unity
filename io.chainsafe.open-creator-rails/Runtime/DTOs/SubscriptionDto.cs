using System;
using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct SubscriptionDto
    {
        public string SubscriberIdHash { get; private set; }

        public BigInteger Nonce { get; private set; }
        
        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public BigInteger SubscriptionPrice { get; private set; }
        
        public BigInteger RegistryFeeShare { get; private set; }
        
        public EthereumAddress Payer { get; private set; }
        
        public SubscriptionDto(string subscriberIdHash, BigInteger nonce, DateTime startTime, DateTime endTime,
            BigInteger subscriptionPrice, BigInteger registryFeeShare, EthereumAddress payer)
        {
            SubscriberIdHash = subscriberIdHash;
            Nonce = nonce;
            StartTime = startTime;
            EndTime = endTime;
            SubscriptionPrice = subscriptionPrice;
            RegistryFeeShare = registryFeeShare;
            Payer = payer;
        }

        public SubscriptionDto Extended(DateTime endTime)
        {
            if (EndTime < endTime)
            {
                EndTime = endTime;
            }

            return this;
        }
        
        public SubscriptionDto Shortened(DateTime endTime)
        {
            if (endTime < EndTime)
            {
                EndTime = endTime;
            }

            return this;
        }
    }
}