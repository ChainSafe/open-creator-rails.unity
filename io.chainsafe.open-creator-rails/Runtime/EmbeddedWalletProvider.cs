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
    public class EmbeddedWalletProvider : MonoBehaviour, IWalletProvider, IInitializeHandler
    {
        private const string MnemonicKey = "mnemonic";
        
        private const string RPCUrlKey = "rpcUrl";

        public Wallet Wallet { get; private set; }
        
        public string RpcUrl { get; private set; }
        
        public int ChainId => _chainId;

        public int ConnectedAccountIndex { get; private set; } = 0;

        public EthereumAddress ConnectedAccount => new EthereumAddress(Wallet.GetAccount(ConnectedAccountIndex).Address);

        [SerializeField] private int _chainId;

        private readonly Eip712TypedDataSigner _signer = new Eip712TypedDataSigner();

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

        public EthECDSASignature SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData)
        {
            string signature = _signer.SignTypedDataV4(message, typedData, Wallet.GetEthereumKey(ConnectedAccountIndex));

            return EthECDSASignatureFactory.ExtractECDSASignature(signature);
        }

        public UniTask Disconnect()
        {
            return UniTask.CompletedTask;
        }
    }
}