using System;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct Subscription
    {
        public string AssetId { get; private set; }
        
        public string SubscriberId { get; private set; }

        public EthereumAddress Payer { get; private set; }

        public DateTime StartTime { get; private set; }
        
        public DateTime EndTime { get; private set; }
        
        public DateTime Now { get; private set; }

        public bool IsActive { get; private set; }
        
        public int Nonce { get; private set; }
        
        public EthereumAddress RegistryAddress { get; private set; }
    }
}