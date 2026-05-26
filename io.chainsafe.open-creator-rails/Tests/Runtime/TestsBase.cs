using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Nethereum.JsonRpc.Client;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Runtime
{
    public class TestsBase
    {
        private string _snapshotId;

        private Object _instance;
        
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetup()
        {
            string path = "Packages/io.chainsafe.open-creator-rails/Tests/Runtime/TestSuite.prefab";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            var instantiate = Object.InstantiateAsync(prefab);

            yield return instantiate;
            
            _instance = instantiate.Result[0];
            
            yield return new WaitUntil(() => OpenCreatorRailsService.Instance.Initialized);
        }
        
        [SetUp]
        public virtual async Task SetUp()
        {
            if (OpenCreatorRailsService.Instance.Connected)
            {
                // snapshot anvil state
                _snapshotId = await OpenCreatorRailsService.Instance.Web3.Client
                    .SendRequestAsync<string>(new RpcRequest(1, "evm_snapshot"));
            }
        }
        
        [TearDown]
        public virtual async Task TearDown()
        {
            if (!string.IsNullOrEmpty(_snapshotId) && OpenCreatorRailsService.Instance.Connected)
            {
                // Revert snapshot state
                await OpenCreatorRailsService.Instance.Web3.Client.SendRequestAsync(new RpcRequest(1, "evm_revert",
                    _snapshotId));
            }
        }
        
        [UnityOneTimeTearDown]
        public IEnumerator OneTimeTearDown()
        {
            if (OpenCreatorRailsService.Instance.Connected)
            {
                yield return OpenCreatorRailsService.Instance.Disconnect();
            }
            
            Object.Destroy(_instance);
            
            yield return null;
        }
    }
}