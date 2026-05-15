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
    public class OpenCreatorRailsService : MonoBehaviour
    {
        public static OpenCreatorRailsService Instance { get; private set; }

        public static ABIEncode ABIEncode { get; private set; } =  new ABIEncode();
        
        public IWalletProvider WalletProvider { get; private set; }

        public IIndexerProvider IndexerProvider { get; private set; }
        
        public IEventHandler EventHandler { get; private set; }
        
        public Web3 Web3 { get; private set; }

        [SerializeField] private List<Asset> _assets;

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

        public async UniTask Connect(int index = 0)
        {
            Web3 = await WalletProvider.Connect(index);

            IWeb3Initialized[] connectedHandlers = GetComponents<IWeb3Initialized>();
            
            await connectedHandlers.ForEachAsync(handler => handler.Connected(Web3));
            
            await Assets.ForEachAsync(asset => !connectedHandlers.Contains(asset) ? asset.Connected(Web3) : UniTask.CompletedTask);
        }

        public bool TryGetAsset(string assetId, out IAsset asset, EthereumAddress? registryAddress = null)
        {
            asset = Assets.FirstOrDefault(asset => asset.AssetId == assetId && (registryAddress == null || registryAddress.Value == asset.RegistryAddress));
            
            return asset != null;
        }

        public static async UniTask<AssetRegistryService> DeployAssetRegistry(BigInteger registryFeeShare)
        {
            return await AssetRegistryService.DeployContractAndGetServiceAsync(Instance.Web3,
                new AssetRegistryDeployment { RegistryFeeShare = registryFeeShare });
        }
        
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