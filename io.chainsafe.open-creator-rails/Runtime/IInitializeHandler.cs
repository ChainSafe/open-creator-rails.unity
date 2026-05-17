using Cysharp.Threading.Tasks;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Lifecycle hook called once by <see cref="OpenCreatorRailsService"/> during
    /// <c>Awake</c>. Implement this interface on any component/script attached on the same
    /// GameObject as <see cref="OpenCreatorRailsService"/> that needs to perform asynchronous setup.
    /// </summary>
    public interface IInitializeHandler
    {
        /// <summary>
        /// Performs any required asynchronous initialization. Called once during
        /// <see cref="OpenCreatorRailsService"/> <c>Awake</c>.
        /// </summary>
        public UniTask InitializeAsync();
    }
}
