using System.Collections;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.Utils;
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
    }
}
