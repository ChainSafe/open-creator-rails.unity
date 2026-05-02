using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class PollingEventHandler : MonoBehaviour, IEventHandler, IWeb3Initialized
    {
        private Func<UniTask> _pollEvent;

        [SerializeField] private int _pollingInterval = 12;
        
        private BigInteger _lastBlock;

        private float _time;

        private readonly HashSet<string> _hashes = new HashSet<string>();
        
        private void OnEnable()
        {
            UpdateLoopAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
        
        public async UniTask Connected(Web3 web3)
        {
            _lastBlock= await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
        }

        public void Subscribe<T>(EthereumAddress address, IWeb3 web3, EventDelegate<T> @delegate) where T: IEventDTO, new()
        {
            Event<T> @event = web3.Eth.GetEvent<T>(address.Value);

            _pollEvent += async () =>
            {
                BigInteger currentBlock = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

                if (currentBlock - _lastBlock == 0)
                {
                    return;
                }
                
                var filter = @event.CreateFilterInput(new BlockParameter(new HexBigInteger(_lastBlock)), new BlockParameter(new HexBigInteger(currentBlock)));

                var logs = await @event.GetAllChangesAsync(filter);
                
                foreach (var log in logs)
                {
                    if (_hashes.Add(log.Log.TransactionHash))
                    {
                        @delegate?.Invoke(log.Event);
                    }
                }
            };
        }
        
        private async UniTaskVoid UpdateLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _time += Time.deltaTime;

                if (_time >= _pollingInterval)
                {
                    if (_pollEvent != null)
                    {
                        await _pollEvent.Invoke();
                        
                        _lastBlock = await OpenCreatorRailsService.Instance.Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                    }
                    
                    _time = 0;
                }
                
                await UniTask.NextFrame(cancellationToken);
            }
        }
    }
}