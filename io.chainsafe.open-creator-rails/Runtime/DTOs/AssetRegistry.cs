using System.Numerics;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct AssetRegistry
    {
        public EthereumAddress Address { get; set; }
        
        public BigInteger RegistryFeeShare { get; private set; }

        public BigInteger CreatorFeeShare { get; private set; }
        
        public EthereumAddress Owner { get; set; }
        
        public AssetRegistryService Service { get; private set; }

        public AssetRegistry(EthereumAddress address, BigInteger registryFeeShare, EthereumAddress owner)
        {
            RegistryFeeShare = registryFeeShare;
            // It's a Percentage
            CreatorFeeShare = new BigInteger(100) - RegistryFeeShare;
            Address = address;
            Owner = owner;

            Service = new AssetRegistryService(OpenCreatorRailsService.Instance.Web3, Address.Value);
        }
    }
}