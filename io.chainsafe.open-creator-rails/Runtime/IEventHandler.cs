using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails
{
    public delegate void EventDelegate<T>(T eventDto)  where T : IEventDTO;
    
    public interface IEventHandler
    {
        void Subscribe<T>(EthereumAddress address, IWeb3 web3, EventDelegate<T> @delegate)
            where T : IEventDTO, new();
    }
}