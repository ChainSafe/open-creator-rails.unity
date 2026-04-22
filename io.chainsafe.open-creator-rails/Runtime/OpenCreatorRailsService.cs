using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.EIP712.EIP2612;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class OpenCreatorRailsService : MonoBehaviour
    {
        public static OpenCreatorRailsService Instance { get; private set; }

        public IWalletProvider WalletProvider { get; private set; }

        public IIndexerProvider IndexerProvider { get; private set; }

        public Web3 Web3 { get; private set; }

        [Tooltip("Asset Registry Contract Addresses.")] [SerializeField]
        private EthereumAddress[] _registryAddresses;

        private AssetRegistry[] _registries;

        private readonly Dictionary<int, Asset[]> _assets = new Dictionary<int, Asset[]>();

        public bool Initialized { get; private set; }
        
        public event Action OnInitialized;

        private async void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"There is more than one instance of {nameof(OpenCreatorRailsService)}");
                
                return;
            }

            Instance = this;

            await GetComponents<IInitializeHandler>().ForEachAsync(handler => handler.InitializeAsync().AsTask());
            
            WalletProvider = GetComponent<IWalletProvider>();
            IndexerProvider = GetComponent<IIndexerProvider>();

            Web3 = await WalletProvider.Connect();

            await Initialize();
            
            OnInitialized?.Invoke();
            
            Initialized = true;
        }

        private async UniTask Initialize()
        {
            int index = 0;

            _registries = await _registryAddresses.Select(async registryAddress =>
                {
                    registryAddress.AssertIsValid();

                    _assets.Add(index, await IndexerProvider.GetAssets(registryAddress));

                    index++;

                    return await IndexerProvider.GetAssetRegistry(registryAddress);
                })
                .ToArray();
        }

        public Asset[] GetAssets(int index = 0)
        {
            if (!_assets.TryGetValue(index, out Asset[] assets))
            {
                throw new IndexOutOfRangeException();
            }

            return assets;
        }
        
        public Asset GetAsset(string assetId, int index = 0)
        {
            Asset[] assets = GetAssets(index);

            return assets.First(a => a.AssetIdHash == assetId.Keccack256());
        }

        // TODO
        // ClaimCreatorFee
        // SetSubscriptionPrice
        // GetSubscriptionPrice(assetId, index = 0)
        // GetToken(assetId, index = 0)
        // GetSubscription(assetId, index = 0)
        // GetSubscription(assetId, index = 0)
        // HasAccess(subscriberId, assetId, index = 0)
        // RevokeSubscription(subscriberId, assetId, index = 0)
        
        public async UniTask<DateTime> Subscribe(string assetId, string subscriberId, TimeSpan duration, int index = 0)
        {
            Asset asset = GetAsset(assetId, index);

            (Permit permit, TypedData<Domain> typedData) = await asset.GetPermit(duration);
            
            EthECDSASignature signature = WalletProvider.SignTypedData(permit, typedData);

            byte[] subscriberHashBytes = subscriberId.Keccack256Bytes();
            
            TransactionReceipt receipt = await asset.Service.SubscribeRequestAndWaitForReceiptAsync(subscriberHashBytes, permit.Owner, permit.Spender, permit.Value, permit.Deadline, signature.V[0], signature.R, signature.S);

            BigInteger? endTime = receipt.DecodeAllEvents<SubscriptionExtendedEventDTO>().FirstOrDefault()?.Event.EndTime ??
                                  receipt.DecodeAllEvents<SubscriptionAddedEventDTO>()[0].Event.EndTime;

            return DateTimeOffset.FromUnixTimeSeconds((long) endTime.Value).DateTime.ToLocalTime();
        }
        
        public async UniTask Reconnect(int index = 0)
        {
            Web3 = await WalletProvider.Connect(index);
        }
        
        private async void OnDestroy()
        {
            Instance = null;

            Web3 = null;

            await WalletProvider.Disconnect();
        }
    }
}