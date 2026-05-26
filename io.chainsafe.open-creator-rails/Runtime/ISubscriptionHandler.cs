using System;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Composite <see cref="IAssetEventHandler"/> that covers all eight subscription lifecycle events
    /// emitted by an <c>Asset</c> contract.
    /// <para>
    /// Implement the abstract methods (<see cref="SubscriptionAdded"/>, <see cref="SubscriptionRenewed"/>,
    /// etc.) to react to specific events.
    /// </para>
    /// <para>
    /// <see cref="IAsset"/> extends this interface, so every asset participates in the event pipeline.
    /// </para>
    /// </summary>
    public interface ISubscriptionHandler : IAssetEventHandler<SubscriptionAddedEventDTO>,
        IAssetEventHandler<SubscriptionRenewedEventDTO>, IAssetEventHandler<SubscriptionExtendedEventDTO>,
        IAssetEventHandler<SubscriptionCancelledEventDTO>, IAssetEventHandler<SubscriptionRevokedEventDTO>,
        IAssetEventHandler<SubscriptionUnrevokedEventDTO>, IAssetEventHandler<SubscriptionRemovedEventDTO>,
        IAssetEventHandler<SubscriptionPriceUpdatedEventDTO>
    {
        void IAssetEventHandler<SubscriptionAddedEventDTO>.InvokeAssetEvent(SubscriptionAddedEventDTO @event)
        {
            SubscriptionAdded(@event);
        }

        void IAssetEventHandler<SubscriptionRenewedEventDTO>.InvokeAssetEvent(SubscriptionRenewedEventDTO @event)
        {
            SubscriptionRenewed(@event);
        }

        void IAssetEventHandler<SubscriptionExtendedEventDTO>.InvokeAssetEvent(SubscriptionExtendedEventDTO @event)
        {
            SubscriptionExtended(@event);
        }

        void IAssetEventHandler<SubscriptionCancelledEventDTO>.InvokeAssetEvent(SubscriptionCancelledEventDTO @event)
        {
            SubscriptionCancelled(@event);
        }

        void IAssetEventHandler<SubscriptionRevokedEventDTO>.InvokeAssetEvent(SubscriptionRevokedEventDTO @event)
        {
            SubscriptionRevoked(@event);
        }

        void IAssetEventHandler<SubscriptionUnrevokedEventDTO>.InvokeAssetEvent(SubscriptionUnrevokedEventDTO @event)
        {
            SubscriptionUnrevoked(@event);
        }

        void IAssetEventHandler<SubscriptionRemovedEventDTO>.InvokeAssetEvent(SubscriptionRemovedEventDTO @event)
        {
            SubscriptionRemoved(@event);
        }

        void IAssetEventHandler<SubscriptionPriceUpdatedEventDTO>.InvokeAssetEvent(SubscriptionPriceUpdatedEventDTO @event)
        {
            SubscriptionPriceUpdated(@event);
        }

        /// <summary>Called when a first-time subscription record is created for a subscriber (<c>SubscriptionAdded</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionAdded(SubscriptionAddedEventDTO @event);

        /// <summary>Called when a new subscription record is created for a subscriber who had prior history (<c>SubscriptionRenewed</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionRenewed(SubscriptionRenewedEventDTO @event);

        /// <summary>Called when an active subscription's end time is extended in-place under the same terms (<c>SubscriptionExtended</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionExtended(SubscriptionExtendedEventDTO @event);

        /// <summary>Called when a subscriber cancels their subscription (<c>SubscriptionCancelled</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionCancelled(SubscriptionCancelledEventDTO @event);

        /// <summary>Called when the asset owner revokes a subscriber's access (<c>SubscriptionRevoked</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionRevoked(SubscriptionRevokedEventDTO @event);

        /// <summary>Called when the asset owner lifts a permanent revocation for a subscriber (<c>SubscriptionUnrevoked</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionUnrevoked(SubscriptionUnrevokedEventDTO @event);

        /// <summary>Called when all subscription records for a subscriber are deleted from chain state (<c>SubscriptionRemoved</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionRemoved(SubscriptionRemovedEventDTO @event);

        /// <summary>Called when the asset owner updates the per-period subscription price (<c>SubscriptionPriceUpdated</c> event).</summary>
        /// <param name="event">The decoded event data.</param>
        void SubscriptionPriceUpdated(SubscriptionPriceUpdatedEventDTO @event);
    }
}