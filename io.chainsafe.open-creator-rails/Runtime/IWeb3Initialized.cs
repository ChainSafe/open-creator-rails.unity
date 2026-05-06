using Cysharp.Threading.Tasks;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IWeb3Initialized
    {
        UniTask Connected(Web3 web3);
    }
}