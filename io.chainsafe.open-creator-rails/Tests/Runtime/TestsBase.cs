using System.Collections;
using Io.ChainSafe.OpenCreatorRails;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.Runtime
{
    public class TestsBase
    {
        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetup()
        {
            yield return SceneManager.LoadSceneAsync(0);
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