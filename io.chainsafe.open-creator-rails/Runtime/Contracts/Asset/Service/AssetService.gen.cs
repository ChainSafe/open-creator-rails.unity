using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Web3;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Contracts;
using System.Threading;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;

namespace Io.ChainSafe.OpenCreatorRails.Contracts.Asset.Service
{
    public partial class AssetService: AssetServiceBase
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.IWeb3 web3, AssetDeployment assetDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<AssetDeployment>().SendRequestAndWaitForReceiptAsync(assetDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.IWeb3 web3, AssetDeployment assetDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<AssetDeployment>().SendRequestAsync(assetDeployment);
        }

        public static async Task<AssetService> DeployContractAndGetServiceAsync(Nethereum.Web3.IWeb3 web3, AssetDeployment assetDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, assetDeployment, cancellationTokenSource);
            return new AssetService(web3, receipt.ContractAddress);
        }

        public AssetService(Nethereum.Web3.IWeb3 web3, string contractAddress) : base(web3, contractAddress)
        {
        }

    }


    public partial class AssetServiceBase: ContractWeb3ServiceBase
    {

        public AssetServiceBase(Nethereum.Web3.IWeb3 web3, string contractAddress) : base(web3, contractAddress)
        {
        }

        public virtual Task<string> CancelSubscriptionRequestAsync(CancelSubscriptionFunction cancelSubscriptionFunction)
        {
             return ContractHandler.SendRequestAsync(cancelSubscriptionFunction);
        }

        public virtual Task<TransactionReceipt> CancelSubscriptionRequestAndWaitForReceiptAsync(CancelSubscriptionFunction cancelSubscriptionFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(cancelSubscriptionFunction, cancellationToken);
        }

        public virtual Task<string> CancelSubscriptionRequestAsync(byte[] subscriber)
        {
            var cancelSubscriptionFunction = new CancelSubscriptionFunction();
                cancelSubscriptionFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAsync(cancelSubscriptionFunction);
        }

        public virtual Task<TransactionReceipt> CancelSubscriptionRequestAndWaitForReceiptAsync(byte[] subscriber, CancellationTokenSource cancellationToken = null)
        {
            var cancelSubscriptionFunction = new CancelSubscriptionFunction();
                cancelSubscriptionFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(cancelSubscriptionFunction, cancellationToken);
        }

        public virtual Task<string> ClaimCreatorFeeRequestAsync(ClaimCreatorFeeFunction claimCreatorFeeFunction)
        {
             return ContractHandler.SendRequestAsync(claimCreatorFeeFunction);
        }

        public virtual Task<TransactionReceipt> ClaimCreatorFeeRequestAndWaitForReceiptAsync(ClaimCreatorFeeFunction claimCreatorFeeFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimCreatorFeeFunction, cancellationToken);
        }

        public virtual Task<string> ClaimCreatorFeeRequestAsync(byte[] subscriber)
        {
            var claimCreatorFeeFunction = new ClaimCreatorFeeFunction();
                claimCreatorFeeFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAsync(claimCreatorFeeFunction);
        }

        public virtual Task<TransactionReceipt> ClaimCreatorFeeRequestAndWaitForReceiptAsync(byte[] subscriber, CancellationTokenSource cancellationToken = null)
        {
            var claimCreatorFeeFunction = new ClaimCreatorFeeFunction();
                claimCreatorFeeFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimCreatorFeeFunction, cancellationToken);
        }

        public virtual Task<string> ClaimCreatorFeeRequestAsync(ClaimCreatorFee1Function claimCreatorFee1Function)
        {
             return ContractHandler.SendRequestAsync(claimCreatorFee1Function);
        }

        public virtual Task<TransactionReceipt> ClaimCreatorFeeRequestAndWaitForReceiptAsync(ClaimCreatorFee1Function claimCreatorFee1Function, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimCreatorFee1Function, cancellationToken);
        }

        public virtual Task<string> ClaimCreatorFeeRequestAsync(List<byte[]> subscribers)
        {
            var claimCreatorFee1Function = new ClaimCreatorFee1Function();
                claimCreatorFee1Function.Subscribers = subscribers;
            
             return ContractHandler.SendRequestAsync(claimCreatorFee1Function);
        }

        public virtual Task<TransactionReceipt> ClaimCreatorFeeRequestAndWaitForReceiptAsync(List<byte[]> subscribers, CancellationTokenSource cancellationToken = null)
        {
            var claimCreatorFee1Function = new ClaimCreatorFee1Function();
                claimCreatorFee1Function.Subscribers = subscribers;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimCreatorFee1Function, cancellationToken);
        }

        public virtual Task<string> ClaimRegistryFeeRequestAsync(ClaimRegistryFeeFunction claimRegistryFeeFunction)
        {
             return ContractHandler.SendRequestAsync(claimRegistryFeeFunction);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(ClaimRegistryFeeFunction claimRegistryFeeFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimRegistryFeeFunction, cancellationToken);
        }

        public virtual Task<string> ClaimRegistryFeeRequestAsync(byte[] subscriber)
        {
            var claimRegistryFeeFunction = new ClaimRegistryFeeFunction();
                claimRegistryFeeFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAsync(claimRegistryFeeFunction);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(byte[] subscriber, CancellationTokenSource cancellationToken = null)
        {
            var claimRegistryFeeFunction = new ClaimRegistryFeeFunction();
                claimRegistryFeeFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimRegistryFeeFunction, cancellationToken);
        }

        public virtual Task<string> ClaimRegistryFeeRequestAsync(ClaimRegistryFee1Function claimRegistryFee1Function)
        {
             return ContractHandler.SendRequestAsync(claimRegistryFee1Function);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(ClaimRegistryFee1Function claimRegistryFee1Function, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimRegistryFee1Function, cancellationToken);
        }

        public virtual Task<string> ClaimRegistryFeeRequestAsync(List<byte[]> subscribers)
        {
            var claimRegistryFee1Function = new ClaimRegistryFee1Function();
                claimRegistryFee1Function.Subscribers = subscribers;
            
             return ContractHandler.SendRequestAsync(claimRegistryFee1Function);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(List<byte[]> subscribers, CancellationTokenSource cancellationToken = null)
        {
            var claimRegistryFee1Function = new ClaimRegistryFee1Function();
                claimRegistryFee1Function.Subscribers = subscribers;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimRegistryFee1Function, cancellationToken);
        }

        public Task<byte[]> GetAssetIdQueryAsync(GetAssetIdFunction getAssetIdFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAssetIdFunction, byte[]>(getAssetIdFunction, blockParameter);
        }

        
        public virtual Task<byte[]> GetAssetIdQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAssetIdFunction, byte[]>(null, blockParameter);
        }

        public Task<string> GetRegistryAddressQueryAsync(GetRegistryAddressFunction getRegistryAddressFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRegistryAddressFunction, string>(getRegistryAddressFunction, blockParameter);
        }

        
        public virtual Task<string> GetRegistryAddressQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRegistryAddressFunction, string>(null, blockParameter);
        }

        public Task<BigInteger> GetSubscriptionQueryAsync(GetSubscriptionFunction getSubscriptionFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSubscriptionFunction, BigInteger>(getSubscriptionFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetSubscriptionQueryAsync(byte[] subscriber, BlockParameter blockParameter = null)
        {
            var getSubscriptionFunction = new GetSubscriptionFunction();
                getSubscriptionFunction.Subscriber = subscriber;
            
            return ContractHandler.QueryAsync<GetSubscriptionFunction, BigInteger>(getSubscriptionFunction, blockParameter);
        }

        public Task<BigInteger> GetSubscriptionPriceQueryAsync(GetSubscriptionPriceFunction getSubscriptionPriceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSubscriptionPriceFunction, BigInteger>(getSubscriptionPriceFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetSubscriptionPriceQueryAsync(BigInteger duration, BlockParameter blockParameter = null)
        {
            var getSubscriptionPriceFunction = new GetSubscriptionPriceFunction();
                getSubscriptionPriceFunction.Duration = duration;
            
            return ContractHandler.QueryAsync<GetSubscriptionPriceFunction, BigInteger>(getSubscriptionPriceFunction, blockParameter);
        }

        public Task<string> GetTokenAddressQueryAsync(GetTokenAddressFunction getTokenAddressFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetTokenAddressFunction, string>(getTokenAddressFunction, blockParameter);
        }

        
        public virtual Task<string> GetTokenAddressQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetTokenAddressFunction, string>(null, blockParameter);
        }

        public Task<bool> IsSubscriptionActiveQueryAsync(IsSubscriptionActiveFunction isSubscriptionActiveFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsSubscriptionActiveFunction, bool>(isSubscriptionActiveFunction, blockParameter);
        }

        
        public virtual Task<bool> IsSubscriptionActiveQueryAsync(byte[] subscriber, BlockParameter blockParameter = null)
        {
            var isSubscriptionActiveFunction = new IsSubscriptionActiveFunction();
                isSubscriptionActiveFunction.Subscriber = subscriber;
            
            return ContractHandler.QueryAsync<IsSubscriptionActiveFunction, bool>(isSubscriptionActiveFunction, blockParameter);
        }

        public Task<string> OwnerQueryAsync(OwnerFunction ownerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(ownerFunction, blockParameter);
        }

        
        public virtual Task<string> OwnerQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<OwnerFunction, string>(null, blockParameter);
        }

        public virtual Task<string> RenounceOwnershipRequestAsync(RenounceOwnershipFunction renounceOwnershipFunction)
        {
             return ContractHandler.SendRequestAsync(renounceOwnershipFunction);
        }

        public virtual Task<string> RenounceOwnershipRequestAsync()
        {
             return ContractHandler.SendRequestAsync<RenounceOwnershipFunction>();
        }

        public virtual Task<TransactionReceipt> RenounceOwnershipRequestAndWaitForReceiptAsync(RenounceOwnershipFunction renounceOwnershipFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(renounceOwnershipFunction, cancellationToken);
        }

        public virtual Task<TransactionReceipt> RenounceOwnershipRequestAndWaitForReceiptAsync(CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync<RenounceOwnershipFunction>(null, cancellationToken);
        }

        public virtual Task<string> RevokeSubscriptionRequestAsync(RevokeSubscriptionFunction revokeSubscriptionFunction)
        {
             return ContractHandler.SendRequestAsync(revokeSubscriptionFunction);
        }

        public virtual Task<TransactionReceipt> RevokeSubscriptionRequestAndWaitForReceiptAsync(RevokeSubscriptionFunction revokeSubscriptionFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(revokeSubscriptionFunction, cancellationToken);
        }

        public virtual Task<string> RevokeSubscriptionRequestAsync(byte[] subscriber)
        {
            var revokeSubscriptionFunction = new RevokeSubscriptionFunction();
                revokeSubscriptionFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAsync(revokeSubscriptionFunction);
        }

        public virtual Task<TransactionReceipt> RevokeSubscriptionRequestAndWaitForReceiptAsync(byte[] subscriber, CancellationTokenSource cancellationToken = null)
        {
            var revokeSubscriptionFunction = new RevokeSubscriptionFunction();
                revokeSubscriptionFunction.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(revokeSubscriptionFunction, cancellationToken);
        }

        public virtual Task<string> SetSubscriptionPriceRequestAsync(SetSubscriptionPriceFunction setSubscriptionPriceFunction)
        {
             return ContractHandler.SendRequestAsync(setSubscriptionPriceFunction);
        }

        public virtual Task<TransactionReceipt> SetSubscriptionPriceRequestAndWaitForReceiptAsync(SetSubscriptionPriceFunction setSubscriptionPriceFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setSubscriptionPriceFunction, cancellationToken);
        }

        public virtual Task<string> SetSubscriptionPriceRequestAsync(BigInteger newSubscriptionPrice)
        {
            var setSubscriptionPriceFunction = new SetSubscriptionPriceFunction();
                setSubscriptionPriceFunction.NewSubscriptionPrice = newSubscriptionPrice;
            
             return ContractHandler.SendRequestAsync(setSubscriptionPriceFunction);
        }

        public virtual Task<TransactionReceipt> SetSubscriptionPriceRequestAndWaitForReceiptAsync(BigInteger newSubscriptionPrice, CancellationTokenSource cancellationToken = null)
        {
            var setSubscriptionPriceFunction = new SetSubscriptionPriceFunction();
                setSubscriptionPriceFunction.NewSubscriptionPrice = newSubscriptionPrice;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(setSubscriptionPriceFunction, cancellationToken);
        }

        public virtual Task<string> SubscribeRequestAsync(SubscribeFunction subscribeFunction)
        {
             return ContractHandler.SendRequestAsync(subscribeFunction);
        }

        public virtual Task<TransactionReceipt> SubscribeRequestAndWaitForReceiptAsync(SubscribeFunction subscribeFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(subscribeFunction, cancellationToken);
        }

        public virtual Task<string> SubscribeRequestAsync(byte[] subscriber, string payer, string spender, BigInteger value, BigInteger deadline, byte v, byte[] r, byte[] s)
        {
            var subscribeFunction = new SubscribeFunction();
                subscribeFunction.Subscriber = subscriber;
                subscribeFunction.Payer = payer;
                subscribeFunction.Spender = spender;
                subscribeFunction.Value = value;
                subscribeFunction.Deadline = deadline;
                subscribeFunction.V = v;
                subscribeFunction.R = r;
                subscribeFunction.S = s;
            
             return ContractHandler.SendRequestAsync(subscribeFunction);
        }

        public virtual Task<TransactionReceipt> SubscribeRequestAndWaitForReceiptAsync(byte[] subscriber, string payer, string spender, BigInteger value, BigInteger deadline, byte v, byte[] r, byte[] s, CancellationTokenSource cancellationToken = null)
        {
            var subscribeFunction = new SubscribeFunction();
                subscribeFunction.Subscriber = subscriber;
                subscribeFunction.Payer = payer;
                subscribeFunction.Spender = spender;
                subscribeFunction.Value = value;
                subscribeFunction.Deadline = deadline;
                subscribeFunction.V = v;
                subscribeFunction.R = r;
                subscribeFunction.S = s;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(subscribeFunction, cancellationToken);
        }

        public virtual Task<string> TransferOwnershipRequestAsync(TransferOwnershipFunction transferOwnershipFunction)
        {
             return ContractHandler.SendRequestAsync(transferOwnershipFunction);
        }

        public virtual Task<TransactionReceipt> TransferOwnershipRequestAndWaitForReceiptAsync(TransferOwnershipFunction transferOwnershipFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferOwnershipFunction, cancellationToken);
        }

        public virtual Task<string> TransferOwnershipRequestAsync(string newOwner)
        {
            var transferOwnershipFunction = new TransferOwnershipFunction();
                transferOwnershipFunction.NewOwner = newOwner;
            
             return ContractHandler.SendRequestAsync(transferOwnershipFunction);
        }

        public virtual Task<TransactionReceipt> TransferOwnershipRequestAndWaitForReceiptAsync(string newOwner, CancellationTokenSource cancellationToken = null)
        {
            var transferOwnershipFunction = new TransferOwnershipFunction();
                transferOwnershipFunction.NewOwner = newOwner;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(transferOwnershipFunction, cancellationToken);
        }

        public override List<Type> GetAllFunctionTypes()
        {
            return new List<Type>
            {
                typeof(CancelSubscriptionFunction),
                typeof(ClaimCreatorFeeFunction),
                typeof(ClaimCreatorFee1Function),
                typeof(ClaimRegistryFeeFunction),
                typeof(ClaimRegistryFee1Function),
                typeof(GetAssetIdFunction),
                typeof(GetRegistryAddressFunction),
                typeof(GetSubscriptionFunction),
                typeof(GetSubscriptionPriceFunction),
                typeof(GetTokenAddressFunction),
                typeof(IsSubscriptionActiveFunction),
                typeof(OwnerFunction),
                typeof(RenounceOwnershipFunction),
                typeof(RevokeSubscriptionFunction),
                typeof(SetSubscriptionPriceFunction),
                typeof(SubscribeFunction),
                typeof(TransferOwnershipFunction)
            };
        }

        public override List<Type> GetAllEventTypes()
        {
            return new List<Type>
            {
                typeof(CreatorFeeClaimedEventDTO),
                typeof(OwnershipTransferredEventDTO),
                typeof(SubscriptionAddedEventDTO),
                typeof(SubscriptionCancelledEventDTO),
                typeof(SubscriptionExtendedEventDTO),
                typeof(SubscriptionPriceUpdatedEventDTO),
                typeof(SubscriptionRevokedEventDTO)
            };
        }

        public override List<Type> GetAllErrorTypes()
        {
            return new List<Type>
            {
                typeof(InsufficientFundsError),
                typeof(InvalidOwnerError),
                typeof(InvalidSpenderError),
                typeof(InvalidTokenAddressError),
                typeof(OnlyRegistryUnauthorizedAccountError),
                typeof(OwnableInvalidOwnerError),
                typeof(OwnableUnauthorizedAccountError),
                typeof(PermitFailedError),
                typeof(ReentrancyGuardReentrantCallError),
                typeof(SafeERC20FailedOperationError),
                typeof(SubscriptionCancellationFailedError),
                typeof(SubscriptionNotFoundError),
                typeof(SubscriptionRevocationFailedError)
            };
        }
    }
}
