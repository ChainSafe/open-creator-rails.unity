using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Callback invoked when a new contract event of type <typeparamref name="T"/> is received.
    /// </summary>
    /// <typeparam name="T">The Nethereum event DTO type, implements <see cref="IEventDTO"/>.</typeparam>
    /// <param name="eventDto">The decoded event data.</param>
    public delegate void EventDelegate<T>(T eventDto)  where T : IEventDTO;

    /// <summary>
    /// Abstraction over the mechanism used to listen for on-chain contract events.
    /// Implement this interface to integrate events listening and update data based on realtime changes.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// Registers a listener for events of type <typeparamref name="T"/> emitted by the
        /// contract at <paramref name="address"/>. The <paramref name="delegate"/> is invoked
        /// once for each new event occurrence.
        /// </summary>
        /// <typeparam name="T">
        /// The Nethereum event DTO type to listen for, implements <see cref="IEventDTO"/>.
        /// Must be constructible with a parameterless constructor.
        /// </typeparam>
        /// <param name="address">On-chain address of the contract to watch.</param>
        /// <param name="web3">The <see cref="IWeb3"/> instance used to query for logs.</param>
        /// <param name="delegate">Callback invoked for each new event instance received.</param>
        void Subscribe<T>(EthereumAddress address, IWeb3 web3, EventDelegate<T> @delegate)
            where T : IEventDTO, new();
    }
}
