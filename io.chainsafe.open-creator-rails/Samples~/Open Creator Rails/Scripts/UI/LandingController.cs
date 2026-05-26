using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class LandingController : BaseController
    {
        [field: SerializeField] public override VisualTreeAsset VisualTreeAsset { get; protected set; }

        public override void OnLoad()
        {
            base.OnLoad();

            Button connectButton = Root.Q<Button>("connect-button");
            Button disconnectButton = Root.Q<Button>("disconnect-button");
            Button resumeButton = Root.Q<Button>("resume-button");

            connectButton.clicked += () =>
            {
                // Account indices 5 - 11 are seeded w/ 1000 TEST Tokens
                UIController.Instance.LoadOverlay(async () =>
                {
                    await OpenCreatorRailsService.Instance.Connect(5);
                    
                    UIController.Instance.Unload();
                });
            };
            
            disconnectButton.clicked += () =>
            {
                UIController.Instance.LoadOverlay(async () =>
                {
                    await OpenCreatorRailsService.Instance.Disconnect();
                });
            };

            resumeButton.clicked += () =>
            {
                UIController.Instance.Unload();
            };
        }
    }
}