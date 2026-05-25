using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public abstract class BaseController : MonoBehaviour, IController
    {
        public abstract VisualTreeAsset VisualTreeAsset { get; protected set; }
        
        public VisualElement Root { get; set; }

        public virtual void OnLoad()
        {
            Player.Instance.Pause();
            
            Cursor.lockState = CursorLockMode.None;
        }

        public virtual void OnUnload()
        {
            Player.Instance.Resume();
            
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}