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
            Button connectButton = Root.Q<Button>("connect-button");
            Button disconnectButton = Root.Q<Button>("disconnect-button");

            connectButton.clicked += () =>
            {
                // Account indices 5 - 11 are seeded w/ 1000 TEST Tokens
                UIController.Instance.LoadOverlay(() => OpenCreatorRailsService.Instance.Connect(5));
                
                Cursor.lockState = CursorLockMode.Locked;
            };
            
            disconnectButton.clicked += () =>
            {
                UIController.Instance.LoadOverlay(() => OpenCreatorRailsService.Instance.Disconnect());
                
                Cursor.lockState = CursorLockMode.None;
            };
        }

        public void OnUnload()
        {
            
        }
    }
}