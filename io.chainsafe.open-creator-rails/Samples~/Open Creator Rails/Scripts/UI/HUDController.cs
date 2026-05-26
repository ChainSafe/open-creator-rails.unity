using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class HUDController : MonoBehaviour, IController
    {
        [field: SerializeField] public VisualTreeAsset VisualTreeAsset { get; private set; }
        
        public VisualElement Root { get; set; }

        public void OnLoad()
        {
            
        }

        public void OnUnload()
        {
            
        }
    }
}