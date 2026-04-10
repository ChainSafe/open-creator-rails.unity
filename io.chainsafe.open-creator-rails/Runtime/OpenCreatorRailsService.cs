using System;
using System.Collections.Generic;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Nethereum.Util;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class OpenCreatorRailsService : MonoBehaviour
    {
        public OpenCreatorRailsService Instance { get; private set; }
        
        public IWalletProvider WalletProvider { get; private set; }
        
        [Tooltip("Asset Registry Contract Addresses.")]
        [SerializeField] private string[] _registries = Array.Empty<string>();
        
        private Web3 _web3;

        private List<AssetRegistryService> _registryServices = new List<AssetRegistryService>();
        
        private async void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one instance of {nameof(OpenCreatorRailsService)}");
                
                return;
            }
            
            Instance = this;

            WalletProvider = GetComponent<IWalletProvider>();

            _web3 = await WalletProvider.Connect();

            InitializeRegistries();
        }

        private void InitializeRegistries()
        {
            foreach (string registry in _registries)
            {
                if (!registry.IsValidEthereumAddressHexFormat())
                {
                    Debug.LogError($"Invalid registry address {registry}");
                    
                    continue;
                }
                
                var registryService = new AssetRegistryService(_web3, registry);
                
                _registryServices.Add(registryService);
            }
        }
        
        private async void OnDestroy()
        {
            Instance = null;
            
            _web3 =  null;
            
            await WalletProvider.Disconnect();
        }
    }
}