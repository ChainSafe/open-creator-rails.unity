using System.Linq;
using Cysharp.Threading.Tasks;
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

        [SerializeField] private Asset[] _assets;

        public Asset[] Assets => _assets;

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

            await GetComponents<IWeb3Initialized>().ForEachAsync(handler => handler.Connected(Web3));
            
            await Assets.ForEachAsync(asset => asset.Connected(Web3));
        }

        public bool TryGetAsset(string assetId, out Asset asset, EthereumAddress? registryAddress = null)
        {
            asset = _assets.FirstOrDefault(asset => asset.AssetId == assetId && (registryAddress == null || registryAddress.Value == asset.RegistryAddress));
            
            return asset != null;
        }

        private async void OnDestroy()
        {
            Instance = null;

            Web3 = null;

            await WalletProvider.Disconnect();
        }
    }
}