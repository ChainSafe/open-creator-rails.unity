using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Abstraction over the wallet used to connect, disconnect and sign data and messages.
    /// Implement this interface to integrate your own wallet implementation.
    /// </summary>
    public interface IWalletProvider
    {
        /// <summary>EIP-155 chain ID of the network the wallet is connected to.</summary>
        public BigInteger ChainId { get; }

        /// <summary>
        /// HD derivation index of the currently connected account.
        /// Set by the <c>index</c> argument passed to <see cref="Connect"/>.
        /// </summary>
        public int ConnectedAccountIndex { get; }

        /// <summary>Hex address of the currently connected account. Available after <see cref="Connect"/> succeeds.</summary>
        public EthereumAddress ConnectedAccount { get; }

        /// <summary>
        /// Derives wallet account and returns an initialized Nethereum <see cref="Web3"/>
        /// instance connected to the configured RPC endpoint.
        /// </summary>
        /// <param name="index">HD wallet derivation index (default <c>0</c>).</param>
        /// <returns>NEthereum <see cref="Web3"/> instance.</returns>
        UniTask<Web3> Connect(int index = 0);

        /// <summary>
        /// Signs a raw byte array using EIP-191 personal sign with the
        /// currently connected account's private key.
        /// </summary>
        /// <param name="message">The raw message bytes to sign.</param>
        /// <returns>The ECDSA signature produced by the connected account.</returns>
        EthECDSASignature SignMessage(byte[] message);

        /// <summary>
        /// Signs an EIP-712 typed data structure with the currently connected account's private key.
        /// </summary>
        /// <typeparam name="T">The type of the message payload.</typeparam>
        /// <typeparam name="TDomain">The type of the EIP-712 domain separator.</typeparam>
        /// <param name="message">The typed message to sign.</param>
        /// <param name="typedData">The EIP-712 typed data definition including domain and type schema.</param>
        /// <returns>The ECDSA signature produced by the connected account.</returns>
        EthECDSASignature SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData);

        /// <summary>
        /// Disconnects the wallet and releases any associated resources.
        /// </summary>
        UniTask Disconnect();
    }
}
