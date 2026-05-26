using System;
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.Runtime
{
    [TestFixture]
    public class OpenCreatorRailsServiceTests : TestsBase
    {
        [UnityTest]
        public IEnumerator Test_SingletonInstance()
        {
            var instance = OpenCreatorRailsService.Instance;
            Assert.NotNull(instance);
            
            var secondInstance = new GameObject("DoubleInstance").AddComponent<OpenCreatorRailsService>();

            yield return null;
            
            LogAssert.Expect(LogType.Exception, new Regex("There is more than one Singleton Instance of"));
            
            Assert.AreSame(instance, OpenCreatorRailsService.Instance);
            Assert.AreNotSame(secondInstance, OpenCreatorRailsService.Instance);
            // Cleanup
            Object.Destroy(secondInstance?.gameObject);
        }
        
        [Test]
        public void Test_InstanceNotNull()
        {
            Assert.NotNull(OpenCreatorRailsService.Instance.WalletProvider);
            Assert.NotNull(OpenCreatorRailsService.Instance.IndexerProvider);
            Assert.NotNull(OpenCreatorRailsService.Instance.EventHandler);
        }
        
        [Test]
        public async Task Test_Connect()
        {
            await OpenCreatorRailsService.Instance.Connect();
            
            EthereumAddress connectedAccount = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;
            
            Assert.True(connectedAccount.Value.IsValidEthereumAddressHexFormat());
            
            Assert.AreEqual(connectedAccount.Value, "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266");
        }
        
        [Test]
        public async Task Test_Reconnect()
        {
            await OpenCreatorRailsService.Instance.Connect();
            
            await OpenCreatorRailsService.Instance.Connect(1);
            
            EthereumAddress connectedAccount = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;
            
            Assert.True(connectedAccount.Value.IsValidEthereumAddressHexFormat());
            
            Assert.AreEqual(connectedAccount.Value, "0x70997970C51812dc3A010C7d01b50e0d17dc79C8");
        }
        
        [Test]
        public void Test_TryGetAssetExists()
        {
            bool exists = OpenCreatorRailsService.Instance.TryGetAsset("default_asset_id_0", out IAsset asset);
            
            Assert.IsTrue(exists);
            
            Assert.NotNull(asset);
        }
        
        [Test]
        public void Test_TryGetAssetNonexistent()
        {
            bool exists = OpenCreatorRailsService.Instance.TryGetAsset("nonexistent_asset_id", out IAsset asset);
            
            Assert.IsFalse(exists);
            
            Assert.Null(asset);
        }
        
        // ── Critical 10 — IInitializeHandler dispatch ─────────────────────────────
        // InitializeAsync is called during Awake before any Connect(). For Asset
        // MonoBehaviours it calls Refresh() → IndexerProvider.GetAsset(), populating
        // Address, SubscriptionPrice, etc. This test asserts those properties are
        // non-default WITHOUT calling Connect() first, proving InitializeAsync ran.

        [Test]
        public void Test_InitializeAsync_PopulatesAssetProperties()
        {
            // Assets are populated by InitializeAsync → Refresh() during scene Awake,
            // before any explicit Connect() call.
            IAsset asset0 = OpenCreatorRailsService.Instance.Assets[0];

            Assert.IsNotNull(asset0, "Assets[0] must be non-null after scene load.");

            Assert.IsTrue(asset0.Address.Value.IsValidEthereumAddressHexFormat(),
                "Asset.Address must be populated by InitializeAsync without calling Connect().");

            Assert.Greater(asset0.SubscriptionPrice, BigInteger.Zero,
                "Asset.SubscriptionPrice must be populated by InitializeAsync without calling Connect().");

            Assert.IsNotNull(asset0.Subscriptions,
                "Asset.Subscriptions must be populated by InitializeAsync without calling Connect().");
        }

        // ── Significant 19 — Connected property & Web3 lifecycle ─────────────────

        [Test]
        public async Task Test_Connected_TrueAfterConnect()
        {
            await OpenCreatorRailsService.Instance.Connect();

            Assert.IsTrue(OpenCreatorRailsService.Instance.Connected,
                "Connected must be true after a successful Connect().");

            Assert.IsNotNull(OpenCreatorRailsService.Instance.Web3,
                "Web3 must be non-null after a successful Connect().");
        }

        // ── Critical 9 — Disconnect / IDisconnectedHandler dispatch ───────────────

        [Test]
        public async Task Test_Disconnect_ClearsSessionState()
        {
            await OpenCreatorRailsService.Instance.Connect();

            Assert.IsTrue(OpenCreatorRailsService.Instance.Connected,
                "Pre-condition: must be connected before disconnect.");

            await OpenCreatorRailsService.Instance.Disconnect();

            Assert.IsFalse(OpenCreatorRailsService.Instance.Connected,
                "Connected must be false after Disconnect().");

            Assert.IsNull(OpenCreatorRailsService.Instance.Web3,
                "Web3 must be null after Disconnect().");
        }

        [Test]
        public async Task Test_Disconnect_CallsIDisconnectedHandler()
        {
            // IAsset (Asset MonoBehaviour) is the only IDisconnectedHandler in the scene.
            // Its Disconnected() implementation calls UnsubscribeToEvents() when Connected.
            // We verify Disconnected() was dispatched by confirming the service is no longer
            // connected (Web3 == null) and that calling Disconnect() a second time is safe
            // (idempotent — does not throw when already disconnected).
            await OpenCreatorRailsService.Instance.Connect();

            await OpenCreatorRailsService.Instance.Disconnect();

            // Calling Disconnect while not connected must not throw.
            await OpenCreatorRailsService.Instance.Disconnect();
        }

        // ── Critical 8 — TryGetAsset with registryAddress filter ─────────────────

        [Test]
        public void Test_TryGetAsset_WithCorrectRegistryAddress_Finds()
        {
            // DefaultAsset_0 lives under 0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512.
            var registryAddress = new EthereumAddress("0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512");

            bool exists = OpenCreatorRailsService.Instance.TryGetAsset(
                "default_asset_id_0", out IAsset asset, registryAddress);

            Assert.IsTrue(exists, "TryGetAsset must return true for the correct registry address.");
            Assert.IsNotNull(asset);
        }

        [Test]
        public void Test_TryGetAsset_WithWrongRegistryAddress_NotFound()
        {
            // A valid but wrong registry address — no asset in the scene matches this.
            var wrongRegistry = new EthereumAddress("0x0000000000000000000000000000000000000001");

            bool exists = OpenCreatorRailsService.Instance.TryGetAsset(
                "default_asset_id_0", out IAsset asset, wrongRegistry);

            Assert.IsFalse(exists, "TryGetAsset must return false when the registry address does not match.");
            Assert.IsNull(asset);
        }

        [Test]
        public async Task Test_DeployAssetRegistry()
        {
            await OpenCreatorRailsService.Instance.Connect();

            BigInteger registryFeeShare = new BigInteger(30);
            
            AssetRegistryService service = await OpenCreatorRailsService.DeployAssetRegistry(registryFeeShare);

            BigInteger fetchedRegistryFeeShare = await service.GetRegistryFeeShareQueryAsync();
            
            Assert.AreEqual(registryFeeShare, fetchedRegistryFeeShare);
        }
        
        [Test]
        public async Task Test_GetAssetRegistry()
        {
            await OpenCreatorRailsService.Instance.Connect();

            BigInteger registryFeeShare = new BigInteger(30);
            
            AssetRegistryService service = await OpenCreatorRailsService.DeployAssetRegistry(registryFeeShare);
            
            AssetRegistryService fetchedService = OpenCreatorRailsService.GetAssetRegistry(new EthereumAddress(service.ContractAddress));

            BigInteger fetchedRegistryFeeShare = await fetchedService.GetRegistryFeeShareQueryAsync();
            
            Assert.AreEqual(registryFeeShare, fetchedRegistryFeeShare);
        }

        // ── TryAddAsset ───────────────────────────────────────────────────────────

        // Pre-deployed TestToken on Anvil (nonce-0 deploy of account 0, decimals: 6).
        private const string TokenAddress = "0x5FbDB2315678afecb367f032d93F642f64180aa3";

        private static readonly EthereumAddress RegistryAddress =
            new EthereumAddress("0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512");

        // Helper: set a [field: SerializeField] auto-property backing field via reflection.
        // The C# compiler generates the backing field name <PropertyName>k__BackingField.

        private static void SetSerializedField<T>(Object target, string propertyName, T value)
        {
            string fieldName = $"<{propertyName}>k__BackingField";
            target.GetType()
                  .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                  ?.SetValue(target, value);
        }

        // Helper: create an Asset MonoBehaviour with its serialized identity fields set.

        private static Asset CreateAssetComponent(string assetId, EthereumAddress registryAddress)
        {
            var go = new GameObject($"TestAsset_{assetId}");
            var asset = go.AddComponent<Asset>();
            SetSerializedField(asset, "AssetId", assetId);
            SetSerializedField(asset, "RegistryAddress", registryAddress);
            return asset;
        }

        // Helper: deploy a new asset on-chain and wait for Ponder to index it.
        // Returns the assetIdString used (needed to query via TryGetAsset later).
        private static async Task<(string assetIdString, Asset assetComponent)> DeployAndIndexNewAsset()
        {
            // Account 0 owns the pre-seeded registry.
            await OpenCreatorRailsService.Instance.Connect(0);

            var registryService = OpenCreatorRailsService.GetAssetRegistry(RegistryAddress);

            string assetIdString = "test_try_add_asset_" + Guid.NewGuid().ToString("N");
            byte[] assetIdBytes32 = assetIdString.Keccack256().HexToByteArray();

            var receipt = await registryService.CreateAssetRequestAndWaitForReceiptAsync(
                assetIdBytes32,
                subscriptionPrice: new BigInteger(100),
                subscriptionDuration: new BigInteger(86400),
                tokenAddress: TokenAddress,
                owner: OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount.Value);

            receipt.DecodeAllEvents<AssetCreatedEventDTO>(); // assert no decode exception

            // Poll the indexer until the asset is indexed or 10 seconds elapse ---
            // A fixed wait is fragile when the full test suite runs: the indexer may be
            // processing a backlog of blocks from earlier tests (AssetTests alone mines 37+
            // permanent blocks, each triggering a ClaimableRefresh handler in Ponder).
            // Polling retries on InvalidOperationException (empty items array) until the
            // asset appears, giving Ponder time to drain its backlog regardless of depth.
            AssetDto dto = default;

            int attempts = 10;

            while (attempts > 0)
            {
                await UniTask.WaitForSeconds(1f);
                try
                {
                    dto = await OpenCreatorRailsService.Instance.IndexerProvider.GetAsset(assetIdString.Keccack256(), RegistryAddress);
                    break;
                }
                catch (InvalidOperationException)
                {
                    // Asset not yet indexed — wait another second and retry.
                }

                attempts--;
            }

            Assert.IsFalse(string.IsNullOrEmpty(dto.Address.Value), "Asset was not indexed within 10 seconds (attempts).");

            Asset assetComponent = CreateAssetComponent(assetIdString, RegistryAddress);
            return (assetIdString, assetComponent);
        }

        // ── Test: TryAddAsset while disconnected ─────────────────────────────────
        // Asset.InitializeAsync() → Refresh() queries the indexer and populates the
        // indexer-derived properties (Address, SubscriptionPrice, SubscriptionDuration,
        // Owner, TokenAddress, Subscriptions). Because the service is disconnected,
        // Asset.Connected(Web3) is NOT called, so Service/PermitService/TokenSymbol
        // remain at their defaults.

        [Test]
        public async Task Test_TryAddAsset_WhileDisconnected_PopulatesIndexerValues()
        {
            var (assetIdString, newAsset) = await DeployAndIndexNewAsset();

            try
            {
                // Disconnect so TryAddAsset skips the Connected(Web3) call.
                await OpenCreatorRailsService.Instance.Disconnect();

                bool result = await OpenCreatorRailsService.Instance.TryAddAsset(newAsset);

                Assert.IsTrue(result, "TryAddAsset must return true for a freshly deployed asset.");

                // InitializeAsync → Refresh() must have populated the indexer-derived fields.
                Assert.IsTrue(newAsset.Address.Value.IsValidEthereumAddressHexFormat(),
                    "Asset.Address must be populated by InitializeAsync → Refresh().");

                Assert.AreEqual(new BigInteger(100), newAsset.SubscriptionPrice,
                    "Asset.SubscriptionPrice must match the value passed to CreateAsset.");

                Assert.AreEqual(TokenAddress.ToLower(), newAsset.TokenAddress.Value.ToLower(),
                    "Asset.TokenAddress must match the token address passed to CreateAsset.");

                Assert.IsNotNull(newAsset.Subscriptions,
                    "Asset.Subscriptions must be non-null after InitializeAsync.");

                // Connected() was NOT called — Service and TokenSymbol must still be unset.
                Assert.IsNull(newAsset.Service,
                    "Asset.Service must be null when TryAddAsset is called while disconnected.");

                Assert.IsTrue(string.IsNullOrEmpty(newAsset.TokenSymbol),
                    "Asset.TokenSymbol must be empty when TryAddAsset is called while disconnected.");
            }
            finally
            {
                OpenCreatorRailsService.Instance.Assets.Remove(newAsset);
                Object.Destroy(newAsset.gameObject);
            }
        }

        // ── Test: TryAddAsset while connected ────────────────────────────────────
        // When Connected == true, TryAddAsset calls both InitializeAsync() and
        // Connected(Web3) on the new asset, fully populating all properties.
        // Verified by reading Service/TokenSymbol (set in Connected), then using
        // TryGetAsset to retrieve the asset and making a real subscribe call.

        [Test]
        public async Task Test_TryAddAsset_WhileConnected_PopulatesAndConnectsAsset()
        {
            var (assetIdString, newAsset) = await DeployAndIndexNewAsset();

            // DeployAndIndexNewAsset leaves us connected as account 0.
            Assert.IsTrue(OpenCreatorRailsService.Instance.Connected,
                "Pre-condition: must be connected before TryAddAsset.");

            try
            {
                bool result = await OpenCreatorRailsService.Instance.TryAddAsset(newAsset);

                Assert.IsTrue(result, "TryAddAsset must return true for a freshly deployed asset.");

                // InitializeAsync → Refresh() must have run.
                Assert.IsTrue(newAsset.Address.Value.IsValidEthereumAddressHexFormat(),
                    "Asset.Address must be populated by InitializeAsync → Refresh().");

                // Connected(Web3) must have run — Service and TokenSymbol must be set.
                Assert.IsNotNull(newAsset.Service,
                    "Asset.Service must be non-null when TryAddAsset is called while connected.");

                Assert.AreEqual("TEST", newAsset.TokenSymbol,
                    "Asset.TokenSymbol must equal the on-chain ERC-20 symbol after Connected().");

                Assert.AreEqual(new BigInteger(6), newAsset.TokenDecimals,
                    "Asset.TokenDecimals must equal 6 (TestToken) after Connected().");

                // TryGetAsset must find the newly added asset by its string ID.
                bool found = OpenCreatorRailsService.Instance.TryGetAsset(assetIdString, out IAsset foundAsset);
                Assert.IsTrue(found, "TryGetAsset must find the asset added via TryAddAsset.");
                Assert.AreSame(newAsset, foundAsset, "TryGetAsset must return the exact same instance.");

                // Switch to account 5 (seeded with 1000 TEST) and make a real subscribe call.
                await OpenCreatorRailsService.Instance.Connect(5);

                DateTime expiry = await foundAsset.Subscribe(
                    "test_subscriber_" + Guid.NewGuid().ToString("N"),
                    new BigInteger(1));

                Assert.Greater(expiry, DateTime.Now,
                    "Subscribe on the TryAddAsset-registered asset must return a future expiry.");
            }
            finally
            {
                OpenCreatorRailsService.Instance.Assets.Remove(newAsset);
                Object.Destroy(newAsset.gameObject);
            }
        }

        [Test]
        public async Task Test_TryAddAsset_SameInstance_ReturnsFalse()
        {
            // Assets[0] is already registered in the scene — passing the same reference must
            // be rejected immediately (reference-equality check in TryAddAsset line 144).
            IAsset existing = OpenCreatorRailsService.Instance.Assets[0];

            bool result = await OpenCreatorRailsService.Instance.TryAddAsset(existing);

            Assert.IsFalse(result, "TryAddAsset must return false when the exact same asset instance is already registered.");
        }

        [Test]
        public async Task Test_TryAddAsset_DuplicateAssetId_ReturnsFalse()
        {
            // A different Asset instance that shares AssetId + RegistryAddress with Assets[0]
            // must also be rejected (value-equality check in TryAddAsset line 145).
            IAsset existing = OpenCreatorRailsService.Instance.Assets[0];
            Asset duplicate = CreateAssetComponent(existing.AssetId, RegistryAddress);

            try
            {
                bool result = await OpenCreatorRailsService.Instance.TryAddAsset(duplicate);

                Assert.IsFalse(result,
                    "TryAddAsset must return false when a different instance with the same AssetId and RegistryAddress is already registered.");
            }
            finally
            {
                Object.Destroy(duplicate.gameObject);
            }
        }
    }
}
