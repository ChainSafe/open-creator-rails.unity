using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Samples
{
    public class GatedAssetSubscriptionHandler : MonoBehaviour, IAssetEventHandler<SubscriptionAddedEventDTO>,
        IAssetEventHandler<SubscriptionRenewedEventDTO>, IAssetEventHandler<SubscriptionExtendedEventDTO>,
        IAssetEventHandler<SubscriptionCancelledEventDTO>, IAssetEventHandler<SubscriptionRevokedEventDTO>,
        IAssetEventHandler<SubscriptionRemovedEventDTO>
    {
        [field: SerializeField] private GatedAssetTrigger _interactable;

        [field: SerializeField] private GameObject _assetGate;

        public IAsset[] Assets { get; private set; }

        private void Awake()
        {
            Assets = GetComponents<IAsset>();
        }

        public void InvokeAssetEvent(SubscriptionAddedEventDTO @event) => TryAccess(@event);

        public void InvokeAssetEvent(SubscriptionRenewedEventDTO @event) => TryAccess(@event);

        public void InvokeAssetEvent(SubscriptionExtendedEventDTO @event) => TryAccess(@event);

        public void InvokeAssetEvent(SubscriptionCancelledEventDTO @event) => TryAccess(@event);

        public void InvokeAssetEvent(SubscriptionRevokedEventDTO @event) => TryAccess(@event);

        public void InvokeAssetEvent(SubscriptionRemovedEventDTO @event) => TryAccess(@event);

        public async UniTask<bool> TryAccess()
        {
            foreach (IAsset asset in Assets)
            {
                if (await asset.IsSubscriptionActive(Player.Instance.SubscriberId))
                {
                    GrantAccess();

                    return true;
                }
            }

            RestrictAccess();

            return false;
        }

        private void TryAccess(IEventDTO @event)
        {
            if (IsSubscriber(@event))
            {
                UIController.Instance.LoadOverlay(() => TryAccess());
            }
        }

        private bool IsSubscriber(IEventDTO @event)
        {
            byte[] subscriberHashBytes = @event switch
            {
                SubscriptionAddedEventDTO e => e.Subscriber,
                SubscriptionRenewedEventDTO e => e.Subscriber,
                SubscriptionExtendedEventDTO e => e.Subscriber,
                SubscriptionCancelledEventDTO e => e.Subscriber,
                SubscriptionRevokedEventDTO e => e.Subscriber,
                SubscriptionUnrevokedEventDTO e => e.Subscriber,
                SubscriptionRemovedEventDTO e => e.Subscriber,
                _ => null
            };

            return Player.Instance.SubscriberId.ToSubscriberIdHash()
                .ToHex(true)
                .Equals(subscriberHashBytes?.ToHex(true));
        }

        private void GrantAccess()
        {
            _assetGate.SetActive(false);

            _interactable.gameObject.SetActive(false);
        }

        private void RestrictAccess()
        {
            _assetGate.SetActive(true);

            _interactable.gameObject.SetActive(true);
        }
    }
}