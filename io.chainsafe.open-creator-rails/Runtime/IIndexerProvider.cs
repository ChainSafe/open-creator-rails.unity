using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IIndexerProvider
    {
        public string IndexerUrl { get; }
        
        UniTask<AssetDto> GetAsset(string assetIdHash, EthereumAddress registryAddress);
    }
}