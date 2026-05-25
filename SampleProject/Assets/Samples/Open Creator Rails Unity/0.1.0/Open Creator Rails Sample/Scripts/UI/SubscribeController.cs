using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class SubscribeController : BaseController
    {
        [field: SerializeField] public override VisualTreeAsset VisualTreeAsset { get; protected set; }

        [SerializeField] private AccessAssetInteractable _interactable;
        
        public override void OnLoad()
        {
            base.OnLoad();
            
            TabView tabView = Root.Q<TabView>("tab-view");

            List<Tab> tabs = tabView.Query<Tab>().ToList();

            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];

                string assetId = AccessAssetInteractable.AssetIds[i];

                if (OpenCreatorRailsService.Instance.TryGetAsset(assetId, out IAsset asset))
                {
                    Button subscribeButton = tab.Q<Button>("subscribe-button");

                    subscribeButton.clicked += delegate
                    {
                        UIController.Instance.LoadOverlay(async () =>
                        {
                            DateTime endTime = await asset.Subscribe(Player.Instance.SubscriberId, 1);

                            await _interactable.TryAccess();
                            
                            UIController.Instance.LoadWithModel<AccessGrantedController, AccessGrantedModel>(new AccessGrantedModel(endTime));
                        });
                    };
                }
            }
            
            Button closeButton = Root.Q<Button>("close-button");
            
            closeButton.clicked += () =>
            {
                UIController.Instance.Unload();
            };
        }
    }
}