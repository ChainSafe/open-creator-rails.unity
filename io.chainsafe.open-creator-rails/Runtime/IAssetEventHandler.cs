using Nethereum.ABI.FunctionEncoding.Attributes;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IAssetEventHandler
    {
        
    }
    
    public interface IAssetEventHandler<T> : IAssetEventHandler where T : IEventDTO
    {
        void HandleEvent(T @event);
    }
}