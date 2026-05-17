using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Abstraction over the off-chain indexer used to fetch indexed data.
    /// Implement this interface to integrate an indexer service as a data source.
    /// </summary>
    public interface IIndexerProvider
    {
        /// <summary>Base URL of the indexer service.</summary>
        public string IndexerUrl { get; }

        /// <summary>
        /// Fetches the full state of an asset from the indexer, including its configuration
        /// and all known subscription records.
        /// </summary>
        /// <param name="assetIdHash">
        /// Keccak-256 hex hash of the human-readable asset ID. Use
        /// <c>assetId.Keccack256()</c> to compute this value.
        /// </param>
        /// <param name="registryAddress">Address of the <c>AssetRegistry</c> contract that created the asset.</param>
        /// <returns>An <see cref="AssetDto"/> populated with the asset's on-chain configuration and subscriptions.</returns>
        UniTask<AssetDto> GetAsset(string assetIdHash, EthereumAddress registryAddress);
    }
}
