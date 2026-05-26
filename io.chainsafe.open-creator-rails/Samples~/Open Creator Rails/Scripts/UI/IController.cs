using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public interface IController
    {
        public VisualTreeAsset VisualTreeAsset { get; }
        
        public VisualElement Root { get; set; }
        
        void OnLoad();
        
        void OnUnload();
    }
}