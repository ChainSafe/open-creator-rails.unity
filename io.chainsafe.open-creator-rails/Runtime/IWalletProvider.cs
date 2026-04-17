using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.EIP712;
using Nethereum.Signer;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IWalletProvider
    {
        public int ChainId { get; }

        public int ConnectedAccountIndex { get; }

        public EthereumAddress ConnectedAccount { get; }
        
        UniTask<Web3> Connect(int index = 0);

        EthECDSASignature SignTypedData<T, TDomain>(T message, TypedData<TDomain> typedData);
        
        UniTask Disconnect();
    }
}