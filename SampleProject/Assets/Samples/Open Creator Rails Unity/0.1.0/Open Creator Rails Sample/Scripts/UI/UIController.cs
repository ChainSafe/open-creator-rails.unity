using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class UIController : Singleton<UIController>
    {
        [SerializeField] private VisualTreeAsset _loadingOverlay;

        [SerializeField] private UIDocument _hud;
        [SerializeField] private UIDocument _ui;
        [SerializeField] private UIDocument _overlay;

        private IController[] _controllers;

        private void OnEnable()
        {
            _controllers = GetComponentsInChildren<IController>();

            LoadWithModel<HUDController, HUDModel>(new HUDModel(), true);
        }

        private T _Load<T>(bool hud = false) where T : IController
        {
            Unload(hud);

            UIDocument document = hud ? _hud : _ui;

            IController controller = _controllers.Single(menu => menu is T);

            controller.VisualTreeAsset.CloneTree(document.rootVisualElement);

            controller.Root = document.rootVisualElement;

            return (T)controller;
        }

        public void Load<T>(bool hud = false) where T : IController
        {
            IController controller = _Load<T>(hud);

            controller.OnLoad();
        }

        public void LoadWithModel<TController, TModel>(TModel model, bool hud = false)
            where TController : IController where TModel : IModel
        {
            IController controller = _Load<TController>(hud);

            controller.Root.dataSource = model;

            controller.OnLoad();
        }

        public void Unload(bool hud = false)
        {
            VisualElement root = (hud ? _hud : _ui).rootVisualElement;

            if (root != null)
            {
                IController controller = _controllers.SingleOrDefault(controller => root == controller.Root);

                if (controller != null)
                {
                    controller.OnUnload();
                    
                    controller.Root = null;
                }
                
                root.Clear();
            }
        }

        public void LoadOverlay(Func<UniTask> action)
        {
            _overlay.rootVisualElement.Clear();

            _loadingOverlay.CloneTree(_overlay.rootVisualElement);

            action?.Invoke()
                .ContinueWith(() =>
                {
                    UniTask.SwitchToMainThread();

                    _overlay.rootVisualElement.Clear();
                });
        }
    }
}