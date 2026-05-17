using Cysharp.Threading.Tasks;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Lifecycle hook called by <see cref="OpenCreatorRailsService.Connect"/> after a
    /// <see cref="Web3"/> instance has been created. Implement this interface on any
    /// component, attached on the same GameObject as <see cref="OpenCreatorRailsService"/>,
    /// that needs a reference to <see cref="Web3"/> for setup.
    /// </summary>
    public interface IWeb3Initialized
    {
        /// <summary>
        /// Called once after <see cref="OpenCreatorRailsService.Connect"/> successfully
        /// creates a <see cref="Web3"/> instance. Use this for any initial setup
        /// that needs a <see cref="Web3"/> instance.
        /// </summary>
        /// <param name="web3">The active <see cref="Web3"/> instance for the connected account.</param>
        UniTask Connected(Web3 web3);
    }
}
