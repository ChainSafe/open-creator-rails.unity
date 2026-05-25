using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class AccessAssetInteractable : Interactable
    {
        public static readonly string[] AssetIds = { "default_asset_id_1", "default_asset_id_3" };

        [field: SerializeField] private GameObject _assetGate;
        
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
                if (await TryAccess())
                {
                    return;
                }
                
                int length = AssetIds.Length;
            
                SubscribeModel model = new SubscribeModel {  Prices = new (string price, string duration)[length] };

                for (int i = 0; i < length; i++)
                {
                    if (OpenCreatorRailsService.Instance.TryGetAsset(AssetIds[i], out IAsset asset))
                    {
                        model.Prices[i].price = $"{asset.SubscriptionPricePrimaryUnit} {asset.TokenSymbol} /";
                    
                        switch (i)
                        {
                            case 0:
                                model.Prices[i].duration = "month";
                                break;
                            case 1:
                                model.Prices[i].duration = "year";
                                break;
                        }
                    }
                }

                UIController.Instance.LoadWithModel<SubscribeController, SubscribeModel>(model);
            });
        }

        public async UniTask<bool> TryAccess()
        {
            foreach (string assetId in AssetIds)
            {
                if (OpenCreatorRailsService.Instance.TryGetAsset(assetId, out IAsset asset))
                {
                    if (await asset.IsSubscriptionActive(Player.Instance.SubscriberId))
                    {
                        GrantAccess();

                        return true;
                    }
                }
            }
            
            return false;
        }

        private void GrantAccess()
        {
            _assetGate.SetActive(false);
                
            gameObject.SetActive(false);
            
            Player.Instance.Interactable = false;
        }
    }
}