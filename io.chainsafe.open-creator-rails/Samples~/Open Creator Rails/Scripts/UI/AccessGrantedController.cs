using UnityEngine;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class AccessGrantedController : BaseController
    {
        [field: SerializeField] public override VisualTreeAsset VisualTreeAsset { get; protected set; }

        public override void OnLoad()
        {
            base.OnLoad();

            Button closeButton = Root.Q<Button>("close-button");
            
            closeButton.clicked += () =>
            {
                UIController.Instance.Unload();
            };
        }
    }
}