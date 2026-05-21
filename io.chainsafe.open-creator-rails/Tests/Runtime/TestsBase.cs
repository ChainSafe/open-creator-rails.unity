using System.Collections;
using System.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Nethereum.JsonRpc.Client;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Runtime
{
    public class TestsBase
    {
        private string _snapshotId;
        
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetup()
        {
            yield return SceneManager.LoadSceneAsync(0);
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
            // Since OpenCreatorRailsService isn't destroyed on SceneLoad (DontDestroyOnLoad)
            // we have to destroy it explicitly so the next SceneLoad loads a fresh instance 
            Object.Destroy(OpenCreatorRailsService.Instance.gameObject);
            
            yield return null;
        }
    }
}