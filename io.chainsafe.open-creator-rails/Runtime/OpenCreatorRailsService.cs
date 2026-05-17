using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Singleton service for the Open Creator Rails SDK. Add this component to a GameObject
    /// in your scene alongside your chosen <see cref="IWalletProvider"/>, <see cref="IIndexerProvider"/>,
    /// and <see cref="IEventHandler"/> implementations. The GameObject is marked
    /// <c>DontDestroyOnLoad</c> and only one instance, <see cref="OpenCreatorRailsService.Instance"/>, may exist at a time.
    /// </summary>
    public class OpenCreatorRailsService : MonoBehaviour
    {
        public static OpenCreatorRailsService Instance { get; private set; }

        public static ABIEncode ABIEncode { get; private set; } =  new ABIEncode();

        /// <summary>
        /// The active wallet provider resolved from the same GameObject during <c>Awake</c>.
        /// </summary>
        public IWalletProvider WalletProvider { get; private set; }

        /// <summary>
        /// The active indexer provider resolved from the same GameObject during <c>Awake</c>.
        /// </summary>
        public IIndexerProvider IndexerProvider { get; private set; }

        /// <summary>
        /// The active event handler resolved from the same GameObject during <c>Awake</c>.
        /// </summary>
        public IEventHandler EventHandler { get; private set; }

        /// <summary>
        /// The Nethereum <c>Web3</c> instance created by <see cref="Connect"/>. 
        /// <c>null</c> until <see cref="Connect"/> has been called successfully.
        /// </summary>
        public Web3 Web3 { get; private set; }

        [SerializeField] private List<Asset> _assets;

        /// <summary>
        /// The list of <see cref="IAsset"/> instances configured on this service in the Inspector.
        /// Each asset is populated with on-chain state after <see cref="Connect"/> completes.
        /// </summary>
        public List<IAsset> Assets => _assets.ConvertAll(asset => (IAsset) asset);

        private async void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one instance of {nameof(OpenCreatorRailsService)}");
                
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            await GetComponents<IInitializeHandler>().ForEachAsync(handler => handler.InitializeAsync());
            
            WalletProvider = GetComponent<IWalletProvider>();
            IndexerProvider = GetComponent<IIndexerProvider>();
            EventHandler = GetComponent<IEventHandler>();
        }

        /// <summary>
        /// Connects the wallet at the given HD Wallet index, then initializes all
        /// <see cref="IWeb3Initialized"/> components and all configured <see cref="Assets"/>.
        /// </summary>
        /// <param name="index">
        /// HD wallet account index to connect (default <c>0</c>). Pass a higher value to
        /// use a different derived account from the same mnemonic.
        /// </param>
        public async UniTask Connect(int index = 0)
        {
            Web3 = await WalletProvider.Connect(index);

            IWeb3Initialized[] connectedHandlers = GetComponents<IWeb3Initialized>();
            
            await connectedHandlers.ForEachAsync(handler => handler.Connected(Web3));
            
            await Assets.ForEachAsync(asset => !connectedHandlers.Contains(asset) ? asset.Connected(Web3) : UniTask.CompletedTask);
        }

        /// <summary>
        /// Looks up a configured asset by its human-readable ID and optional registry address.
        /// </summary>
        /// <param name="assetId">The human-readable asset identifier.</param>
        /// <param name="asset">
        /// When this method returns <c>true</c>, contains the matching <see cref="IAsset"/>;
        /// otherwise <c>null</c>.
        /// </param>
        /// <param name="registryAddress">
        /// Optional registry address to narrow the search in case the same <paramref name="assetId"/> is used across multiple registries.
        /// </param>
        /// <returns><c>true</c> if a matching asset was found; otherwise <c>false</c>.</returns>
        public bool TryGetAsset(string assetId, out IAsset asset, EthereumAddress? registryAddress = null)
        {
            asset = Assets.FirstOrDefault(asset => asset.AssetId == assetId && (registryAddress == null || registryAddress.Value == asset.RegistryAddress));
            
            return asset != null;
        }

        /// <summary>
        /// Deploys a new <c>AssetRegistry</c> contract to the chain and returns a service
        /// wrapper for it. Requires an active <see cref="Web3"/> connection.
        /// </summary>
        /// <param name="registryFeeShare">
        /// Percentage of each subscription payment allocated to the registry (0–100).
        /// The creator receives the remainder (<c>100 - registryFeeShare</c>).
        /// </param>
        /// <returns>
        /// A <see cref="AssetRegistryService"/> bound to the newly deployed contract address.
        /// </returns>
        public static async UniTask<AssetRegistryService> DeployAssetRegistry(BigInteger registryFeeShare)
        {
            return await AssetRegistryService.DeployContractAndGetServiceAsync(Instance.Web3,
                new AssetRegistryDeployment { RegistryFeeShare = registryFeeShare });
        }

        /// <summary>
        /// Returns a <see cref="AssetRegistryService"/> wrapper for an already-deployed
        /// <c>AssetRegistry</c> contract. Requires an active <see cref="Web3"/> connection.
        /// </summary>
        /// <param name="address">The on-chain address of the deployed <c>AssetRegistry</c> contract.</param>
        /// <returns>A <see cref="AssetRegistryService"/> bound to the given address.</returns>
        public static AssetRegistryService GetAssetRegistry(EthereumAddress address)
        {
            return new AssetRegistryService(Instance.Web3, address.Value);
        }
        
        private async void OnDestroy()
        {
            Instance = null;

            Web3 = null;

            await WalletProvider.Disconnect();
        }
    }
}
