using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Non-generic marker interface for asset contract event handlers.
    /// Used as a common collection type when storing handlers of mixed event types.
    /// Cast to <see cref="IAssetEventHandler{T}"/> to handle a specific event type,
    /// or use the <c>Extensions.Get&lt;T&gt;</c> helper to filter by type.
    /// </summary>
    public interface IAssetEventHandler
    {
        
    }

    /// <summary>
    /// Typed event handler for a specific Nethereum event DTO type <typeparamref name="T"/>.
    /// Implement this interface on any component that needs to react to a specific contract event emitted by an asset.
    /// </summary>
    /// <typeparam name="T">The Nethereum event DTO type this handler processes, implements <see cref="IEventDTO"/>.</typeparam>
    public interface IAssetEventHandler<T> : IAssetEventHandler where T : IEventDTO
    {
        /// <summary>
        /// Called by the event dispatch pipeline when an event of type <typeparamref name="T"/>
        /// is received from the contract.
        /// </summary>
        /// <param name="event">The decoded event data.</param>
        void HandleEvent(T @event);
    }
}