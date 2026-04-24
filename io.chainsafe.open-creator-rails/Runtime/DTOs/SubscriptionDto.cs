using System;
using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct SubscriptionDto
    {
        public string SubscriberIdHash { get; private set; }

        public EthereumAddress Payer { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public bool IsActive { get; private set; }

        public BigInteger Nonce { get; private set; }

        public SubscriptionDto(string subscriberIdHash, EthereumAddress payer, DateTime startTime, DateTime endTime,
            bool isActive, BigInteger nonce)
        {
            SubscriberIdHash = subscriberIdHash;
            Payer = payer;
            StartTime = startTime;
            EndTime = endTime;
            IsActive = isActive;
            Nonce = nonce;
        }
    }
}