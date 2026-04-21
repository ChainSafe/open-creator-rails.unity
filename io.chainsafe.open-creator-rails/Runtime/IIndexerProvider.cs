using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IIndexerProvider
    {
        public string IndexerUrl { get; }
        
        UniTask<AssetRegistry> GetAssetRegistry(EthereumAddress registryAddress);
        
        UniTask<Asset[]> GetAssets(EthereumAddress registryAddress);

        //TODO
        // GetMyAssets(address, registryAddress)
        
        public UniTask<Subscription[]> GetAssetSubscriptions(string assetIdHash, EthereumAddress registryAddress);

        public UniTask<Subscription> GetSubscription(string assetIdHash, string subscriberIdHash, EthereumAddress registryAddress);

        public UniTask<Subscription[]> GetSubscriptions(string subscriberIdHash, EthereumAddress registryAddress);
        
        public UniTask<bool> HasAccess(string subscriberIdHash, string assetIdHash, EthereumAddress registryAddress);
    }
}