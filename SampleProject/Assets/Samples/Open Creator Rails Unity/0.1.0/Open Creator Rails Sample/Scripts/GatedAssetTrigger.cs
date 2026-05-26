using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class GatedAssetTrigger : Interactable
    {
        [field: SerializeField] private GatedAssetSubscriptionHandler _subscriptionHandler;
        
        private bool Connected => OpenCreatorRailsService.Instance.Connected;
        
        protected override void Enable()
        {
            if (!Connected) return;
            
            base.Enable();
        }

        protected override void Disable()
        {
            if (!Connected) return;
            
            base.Disable();
        }

        protected override void Interact()
        {
            UIController.Instance.LoadOverlay(async () =>
            {
                if (await _subscriptionHandler.TryAccess())
                {
                    return;
                }
                
                UIController.Instance.LoadWithModel<SubscribeController, SubscribeModel>(new SubscribeModel(_subscriptionHandler.Assets));
            });
        }
    }
}