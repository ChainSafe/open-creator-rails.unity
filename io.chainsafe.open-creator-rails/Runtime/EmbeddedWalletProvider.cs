using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.EIP712;
using Nethereum.HdWallet;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Built-in <see cref="IWalletProvider"/> implementation that derives an HD wallet from a
    /// BIP-39 mnemonic stored in <see cref="Secrets.FilePath"/>. Add this component to the same
    /// GameObject as <see cref="OpenCreatorRailsService"/> and set the <c>Chain Id</c> field in
    /// the Inspector.
    /// <para>
    /// The secrets file <see cref="Secrets.FilePath"/> must contain a <c>"mnemonic"</c> key (12- or 24-word BIP-39 phrase)
    /// and an <c>"rpcUrl"</c> key (JSON-RPC endpoint URL). Credentials are read once during
    /// <see cref="IInitializeHandler.InitializeAsync"/>, before wallet connection.
    /// </para>
    /// </summary>
    public class EmbeddedWalletProvider : MonoBehaviour, IWalletProvider, IInitializeHandler
    {
        private const string MnemonicKey = "mnemonic";
        
        private const string RPCUrlKey = "rpcUrl";

        public Wallet Wallet { get; private set; }
        
        public string RpcUrl { get; private set; }
        
        public BigInteger ChainId => _chainId;

        public int ConnectedAccountIndex { get; private set; } = 0;

        public EthereumAddress ConnectedAccount => new EthereumAddress(Wallet.GetAccount(ConnectedAccountIndex).Address);

        [SerializeField] private int _chainId;

        private readonly Eip712TypedDataSigner _typedDataSigner = new Eip712TypedDataSigner();
        
        private readonly EthereumMessageSigner _messageSigner =  new EthereumMessageSigner();

        public UniTask InitializeAsync()
        {
            string mnemonic = Secrets.Get<string>(MnemonicKey);
            
            RpcUrl = Secrets.Get<string>(RPCUrlKey);
            
            Wallet = new Wallet(mnemonic, null);
            
            return UniTask.CompletedTask;
        }
        
        public UniTask<Web3> Connect(int index = 0)
        {
            ConnectedAccountIndex  = index;
            
            var account = Wallet.GetAccount(ConnectedAccountIndex, ChainId);
            
            var web3 = new Web3(account, RpcUrl);
            
            return UniTask.FromResult(web3);
        }

        public EthECDSASignature SignMessage(byte[] message)
        {
            string signature = _messageSigner.Sign(message, Wallet.GetEthereumKey(ConnectedAccountIndex));

            return EthECDSASignatureFactory.ExtractECDSASignature(signature);
        }

        public EthECDSASignature SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData)
        {
            string signature = _typedDataSigner.SignTypedDataV4(message, typedData, Wallet.GetEthereumKey(ConnectedAccountIndex));

            return EthECDSASignatureFactory.ExtractECDSASignature(signature);
        }

        public UniTask Disconnect()
        {
            return UniTask.CompletedTask;
        }
    }
}