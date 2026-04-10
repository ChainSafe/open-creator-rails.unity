using Cysharp.Threading.Tasks;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IWalletProvider
    {
        public int ChainId { get; }
        
        UniTask<Web3> Connect();
        
        UniTask Disconnect();
    }
}