using System.Collections;
using System.Numerics;
using System.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Util;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Runtime
{
    public class OpenCreatorRailsServiceTests
    {
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetup()
        {
            yield return SceneManager.LoadSceneAsync(0);
        }

        [Test]
        public void Test_SingletonInstance()
        {
            var instance = OpenCreatorRailsService.Instance;
            Assert.NotNull(instance);
            LogAssert.Expect(LogType.Error, $"There is more than one instance of {nameof(OpenCreatorRailsService)}");
            new GameObject("DoubleInstance").AddComponent<OpenCreatorRailsService>();
            Assert.AreSame(instance, OpenCreatorRailsService.Instance);
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
