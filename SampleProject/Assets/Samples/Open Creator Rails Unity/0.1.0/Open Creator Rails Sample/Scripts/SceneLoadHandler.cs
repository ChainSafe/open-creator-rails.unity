using System.Linq;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.Web3;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class SceneLoadHandler : MonoBehaviour, IWeb3Initialized, IDisconnectedHandler
    {
        private Asset[] _assets;
        
        public async UniTask Connected(Web3 web3)
        {
            await SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            
            _assets = FindObjectsByType<Asset>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            await _assets.ForEachAsync(asset => OpenCreatorRailsService.Instance.TryAddAsset(asset));
        }

        public async UniTask Disconnected()
        {
            if (SceneManager.GetSceneByBuildIndex(1).isLoaded)
            {
                await SceneManager.UnloadSceneAsync(1);
            }

            OpenCreatorRailsService.Instance.Assets.RemoveAll(asset => _assets.Contains(asset));
        }
    }
}