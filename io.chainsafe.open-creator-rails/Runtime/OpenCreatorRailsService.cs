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
    public class OpenCreatorRailsService : Singleton<OpenCreatorRailsService>
    {
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

        /// <summary>
        /// <c>true</c> when a wallet session is active (i.e. <see cref="Connect"/> has been
        /// called successfully and <see cref="Disconnect"/> has not yet been called);
        /// otherwise <c>false</c>.
        /// </summary>
        public bool Connected => Web3 != null;

        [SerializeField] private List<Asset> _assets;

        /// <summary>
        /// The list of <see cref="IAsset"/> instances configured on this service in the Inspector.
        /// Each asset is populated with on-chain state after <see cref="Connect"/> completes.
        /// </summary>
        public List<IAsset> Assets { get; private set; }

        protected override async void Awake()
        {
            base.Awake();
            
            // Assign / Reference
            WalletProvider = GetComponent<IWalletProvider>();
            IndexerProvider = GetComponent<IIndexerProvider>();
            EventHandler = GetComponent<IEventHandler>();
            
            Assets = new List<IAsset>(_assets);
            
            // Initialize
            IInitializeHandler[] initializeHandlers = GetComponents<IInitializeHandler>();
            
            await initializeHandlers.ForEachAsync(handler => handler.InitializeAsync());
            
            await Assets.ForEachAsync(asset => !initializeHandlers.Contains(asset) ? asset.InitializeAsync() : UniTask.CompletedTask);
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
            // In case of a reconnect
            if (Connected)
            {
                await Disconnect();
            }
            
            Web3 = await WalletProvider.Connect(index);

            Web3.Client.OverridingRequestInterceptor = new TransactionInterceptor();
            
            IWeb3Initialized[] connectedHandlers = GetComponents<IWeb3Initialized>();
            
            await connectedHandlers.ForEachAsync(handler => handler.Connected(Web3));
            
            await Assets.ForEachAsync(asset => !connectedHandlers.Contains(asset) ? asset.Connected(Web3) : UniTask.CompletedTask);
        }
        
        /// <summary>
        /// Tears down the active wallet session. Calls <see cref="IDisconnectedHandler.Disconnected"/>
        /// on all components and configured <see cref="Assets"/>, then disconnects the
        /// <see cref="IWalletProvider"/> and clears <see cref="Web3"/>.
        /// <para>
        /// Called automatically by <see cref="Connect"/> when reconnecting and on
        /// <c>OnDestroy</c>. Can also be called manually to log out.
        /// </para>
        /// </summary>
        public async UniTask Disconnect()
        {
            IDisconnectedHandler[] disconnectedHandlers = GetComponents<IDisconnectedHandler>();
            
            await disconnectedHandlers.ForEachAsync(handler => handler.Disconnected());

            await Assets.ForEachAsync(asset => !disconnectedHandlers.Contains(asset) ? asset.Disconnected() : UniTask.CompletedTask);
            
            // Disconnect Wallet last
            await WalletProvider.Disconnect();
            
            Web3 = null;
        }

        public async UniTask<bool> TryAddAsset(IAsset asset)
        {
            if (Assets.Contains(asset) ||
                Assets.Any(a => a.AssetId == asset.AssetId && a.RegistryAddress == asset.RegistryAddress))
            {
                return false;
            }

            Assets.Add(asset);

            if (Connected)
            {
                await asset.Connected(Web3);
            }

            return true;
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
        
        protected override async void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            if (Connected)
            {
                await Disconnect();
            }
            
            base.OnDestroy();
        }
    }
}
