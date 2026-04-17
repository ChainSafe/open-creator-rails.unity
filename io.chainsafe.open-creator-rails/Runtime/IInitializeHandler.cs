using Cysharp.Threading.Tasks;

namespace Io.ChainSafe.OpenCreatorRails
{
    public interface IInitializeHandler
    {
        UniTask InitializeAsync();
    }
}