using Cysharp.Threading.Tasks;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Lifecycle hook called by <see cref="OpenCreatorRailsService.Disconnect"/> before the
    /// wallet session is torn down.
    /// </summary>
    public interface IDisconnectedHandler
    {
        /// <summary>
        /// Called once when <see cref="OpenCreatorRailsService.Disconnect"/> is invoked, before
        /// the wallet provider is disconnected.
        /// </summary>
        UniTask Disconnected();
    }
}