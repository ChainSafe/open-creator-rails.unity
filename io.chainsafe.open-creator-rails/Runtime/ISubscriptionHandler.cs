using System;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface ISubscriptionHandler : IAssetEventHandler<SubscriptionAddedEventDTO>,
        IAssetEventHandler<SubscriptionRenewedEventDTO>, IAssetEventHandler<SubscriptionExtendedEventDTO>,
        IAssetEventHandler<SubscriptionCancelledEventDTO>, IAssetEventHandler<SubscriptionRevokedEventDTO>,
        IAssetEventHandler<SubscriptionUnrevokedEventDTO>, IAssetEventHandler<SubscriptionRemovedEventDTO>,
        IAssetEventHandler<SubscriptionPriceUpdatedEventDTO>
    {
        void IAssetEventHandler<SubscriptionAddedEventDTO>.HandleEvent(SubscriptionAddedEventDTO @event)
        {
            SubscriptionAdded(@event);
        }

        void IAssetEventHandler<SubscriptionRenewedEventDTO>.HandleEvent(SubscriptionRenewedEventDTO @event)
        {
            SubscriptionRenewed(@event);
        }

        void IAssetEventHandler<SubscriptionExtendedEventDTO>.HandleEvent(SubscriptionExtendedEventDTO @event)
        {
            SubscriptionExtended(@event);
        }
        
        void IAssetEventHandler<SubscriptionCancelledEventDTO>.HandleEvent(SubscriptionCancelledEventDTO @event)
        {
            SubscriptionCancelled(@event);
        }
        
        void IAssetEventHandler<SubscriptionRevokedEventDTO>.HandleEvent(SubscriptionRevokedEventDTO @event)
        {
            SubscriptionRevoked(@event);
        }
        
        void IAssetEventHandler<SubscriptionUnrevokedEventDTO>.HandleEvent(SubscriptionUnrevokedEventDTO @event)
        {
            SubscriptionUnrevoked(@event);
        }

        void IAssetEventHandler<SubscriptionRemovedEventDTO>.HandleEvent(SubscriptionRemovedEventDTO @event)
        {
            SubscriptionRemoved(@event);
        }
        
        void IAssetEventHandler<SubscriptionPriceUpdatedEventDTO>.HandleEvent(SubscriptionPriceUpdatedEventDTO @event)
        {
            SubscriptionPriceUpdated(@event);
        }

        void SubscriptionAdded(SubscriptionAddedEventDTO @event);

        void SubscriptionRenewed(SubscriptionRenewedEventDTO @event);

        void SubscriptionExtended(SubscriptionExtendedEventDTO @event);

        void SubscriptionCancelled(SubscriptionCancelledEventDTO @event);
        
        void SubscriptionRevoked(SubscriptionRevokedEventDTO @event);
        
        void SubscriptionUnrevoked(SubscriptionUnrevokedEventDTO @event);
        
        void SubscriptionRemoved(SubscriptionRemovedEventDTO @event);
        
        void SubscriptionPriceUpdated(SubscriptionPriceUpdatedEventDTO @event);
    }
}