using System;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.ERC20Permit.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Contracts.ERC20Permit.Service;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.EIP712.EIP2612;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails.DTOs
{
    public struct Asset
    {
        public string AssetIdHash { get; private set; }
        
        public EthereumAddress Address { get; private set; }

        public BigInteger SubscriptionPrice { get; private set; }
        
        public EthereumAddress Owner { get; private set; }
        
        public EthereumAddress TokenAddress { get; private set; }
        
        public EthereumAddress RegistryAddress { get; private set; }

        public Subscription[] Subscriptions { get; private set; }
        
        public AssetService Service { get; private set; }

        public ERC20PermitService PermitService { get; private set; }

        private Eip712DomainOutputDTO _domain;
        
        private TypedData<Domain> _typedData;

        public Asset(string assetIdHash, EthereumAddress address, BigInteger subscriptionPrice, EthereumAddress owner, EthereumAddress tokenAddress, EthereumAddress registryAddress, Subscription[] subscriptions)
        {
            AssetIdHash = assetIdHash;
            Address = address;
            SubscriptionPrice = subscriptionPrice;
            Owner = owner;
            TokenAddress = tokenAddress;
            RegistryAddress = registryAddress;
            Subscriptions = subscriptions;
            
            Web3 web3 = OpenCreatorRailsService.Instance.Web3;
            
            Service = new AssetService(web3, Address.Value);
            PermitService = new ERC20PermitService(web3, TokenAddress.Value);
            
            _domain = null;
            _typedData = null;
        }

        public async UniTask<(Permit permit, TypedData<Domain> typedData)> GetPermit(TimeSpan duration)
        {
            EthereumAddress payer = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;
            
            BigInteger value = SubscriptionPrice * new BigInteger(duration.TotalSeconds);

            BigInteger nonce = await PermitService.NoncesQueryAsync(payer.Value);
            
            Permit permit = new Permit
            {
                Owner = payer.Value,
                Spender = Address.Value,
                Value = value,
                Nonce = nonce,
                Deadline = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + new BigInteger(TimeSpan.FromMinutes(30).TotalSeconds)
            };
            
            _domain ??= await PermitService.Eip712DomainQueryAsync();

            if (_typedData == null)
            {
                _typedData ??= EIP2612TypeFactory.GetTypedDefinition();
            
                _typedData.Domain = new Domain
                {
                    Name = _domain.Name,
                    Version = _domain.Version,
                    ChainId = _domain.ChainId,
                    VerifyingContract = _domain.VerifyingContract
                };
            }
            
            return (permit, _typedData);
        }
    }
}