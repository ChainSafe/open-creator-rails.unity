using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.HdWallet;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class EmbeddedWalletProvider : MonoBehaviour, IWalletProvider
    {
        private const string MnemonicKey = "mnemonic";
        
        private const string RPCUrlKey = "rpcUrl";
        
        [SerializeField] private int _chainId;

        public int ChainId => _chainId;

        public UniTask<Web3> Connect()
        {
            string mnemonic = Secrets.Get<string>(MnemonicKey);
            
            string rpcUrl = Secrets.Get<string>(RPCUrlKey);
            
            var wallet = new Wallet(mnemonic, null);

            var account = wallet.GetAccount(0, ChainId);

            var web3 = new Web3(account, rpcUrl);
            
            return UniTask.FromResult(web3);
        }

        public UniTask Disconnect()
        {
            return UniTask.CompletedTask;
        }
    }
}