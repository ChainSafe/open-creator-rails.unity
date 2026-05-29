using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nethereum.JsonRpc.Client;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// Nethereum <c>RequestInterceptor</c> that automatically deduplicates transaction hashes
    /// with the active <see cref="IEventHandler"/>. Installed on the <c>Web3</c> client by
    /// <see cref="OpenCreatorRailsService.Connect"/>.
    /// <para>
    /// When the SDK sends a transaction (<c>eth_sendRawTransaction</c> or
    /// <c>eth_sendTransaction</c>), the returned hash is immediately forwarded to
    /// <see cref="IEventHandler.DeduplicateEvent"/> so that the event polling loop never
    /// re-delivers events emitted by the SDK's own transactions.
    /// </para>
    /// </summary>
    public class TransactionInterceptor : RequestInterceptor
    {
        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request, string route = null)
        {
            await UniTask.SwitchToMainThread();
            
            var result = await interceptedSendRequestAsync(request, route);

            if (request.Method == "eth_sendRawTransaction" || request.Method == "eth_sendTransaction")
            {
                string hash = result?.ToString();

                if (!string.IsNullOrEmpty(hash))
                {
                    OpenCreatorRailsService.Instance.EventHandler.DeduplicateEvent(hash);
                }
            }

            return result;
        }
    }
}