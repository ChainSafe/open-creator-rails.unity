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
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.ContractDefinition;

namespace Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service
{
    public partial class AssetRegistryService: AssetRegistryServiceBase
    {
        public static Task<TransactionReceipt> DeployContractAndWaitForReceiptAsync(Nethereum.Web3.IWeb3 web3, AssetRegistryDeployment assetRegistryDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            return web3.Eth.GetContractDeploymentHandler<AssetRegistryDeployment>().SendRequestAndWaitForReceiptAsync(assetRegistryDeployment, cancellationTokenSource);
        }

        public static Task<string> DeployContractAsync(Nethereum.Web3.IWeb3 web3, AssetRegistryDeployment assetRegistryDeployment)
        {
            return web3.Eth.GetContractDeploymentHandler<AssetRegistryDeployment>().SendRequestAsync(assetRegistryDeployment);
        }

        public static async Task<AssetRegistryService> DeployContractAndGetServiceAsync(Nethereum.Web3.IWeb3 web3, AssetRegistryDeployment assetRegistryDeployment, CancellationTokenSource cancellationTokenSource = null)
        {
            var receipt = await DeployContractAndWaitForReceiptAsync(web3, assetRegistryDeployment, cancellationTokenSource);
            return new AssetRegistryService(web3, receipt.ContractAddress);
        }

        public AssetRegistryService(Nethereum.Web3.IWeb3 web3, string contractAddress) : base(web3, contractAddress)
        {
        }

    }


    public partial class AssetRegistryServiceBase: ContractWeb3ServiceBase
    {

        public AssetRegistryServiceBase(Nethereum.Web3.IWeb3 web3, string contractAddress) : base(web3, contractAddress)
        {
        }

        public Task<string> AssetsQueryAsync(AssetsFunction assetsFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<AssetsFunction, string>(assetsFunction, blockParameter);
        }

        
        public virtual Task<string> AssetsQueryAsync(byte[] returnValue1, BlockParameter blockParameter = null)
        {
            var assetsFunction = new AssetsFunction();
                assetsFunction.ReturnValue1 = returnValue1;
            
            return ContractHandler.QueryAsync<AssetsFunction, string>(assetsFunction, blockParameter);
        }

        public virtual Task<string> ClaimRegistryFeeRequestAsync(ClaimRegistryFeeFunction claimRegistryFeeFunction)
        {
             return ContractHandler.SendRequestAsync(claimRegistryFeeFunction);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(ClaimRegistryFeeFunction claimRegistryFeeFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimRegistryFeeFunction, cancellationToken);
        }

        public virtual Task<string> ClaimRegistryFeeRequestAsync(byte[] assetId, List<byte[]> subscribers)
        {
            var claimRegistryFeeFunction = new ClaimRegistryFeeFunction();
                claimRegistryFeeFunction.AssetId = assetId;
                claimRegistryFeeFunction.Subscribers = subscribers;
            
             return ContractHandler.SendRequestAsync(claimRegistryFeeFunction);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(byte[] assetId, List<byte[]> subscribers, CancellationTokenSource cancellationToken = null)
        {
            var claimRegistryFeeFunction = new ClaimRegistryFeeFunction();
                claimRegistryFeeFunction.AssetId = assetId;
                claimRegistryFeeFunction.Subscribers = subscribers;
            
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

        public virtual Task<string> ClaimRegistryFeeRequestAsync(byte[] assetId, byte[] subscriber)
        {
            var claimRegistryFee1Function = new ClaimRegistryFee1Function();
                claimRegistryFee1Function.AssetId = assetId;
                claimRegistryFee1Function.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAsync(claimRegistryFee1Function);
        }

        public virtual Task<TransactionReceipt> ClaimRegistryFeeRequestAndWaitForReceiptAsync(byte[] assetId, byte[] subscriber, CancellationTokenSource cancellationToken = null)
        {
            var claimRegistryFee1Function = new ClaimRegistryFee1Function();
                claimRegistryFee1Function.AssetId = assetId;
                claimRegistryFee1Function.Subscriber = subscriber;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(claimRegistryFee1Function, cancellationToken);
        }

        public virtual Task<string> CreateAssetRequestAsync(CreateAssetFunction createAssetFunction)
        {
             return ContractHandler.SendRequestAsync(createAssetFunction);
        }

        public virtual Task<TransactionReceipt> CreateAssetRequestAndWaitForReceiptAsync(CreateAssetFunction createAssetFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(createAssetFunction, cancellationToken);
        }

        public virtual Task<string> CreateAssetRequestAsync(byte[] assetId, BigInteger subscriptionPrice, BigInteger subscriptionDuration, string tokenAddress, string owner)
        {
            var createAssetFunction = new CreateAssetFunction();
                createAssetFunction.AssetId = assetId;
                createAssetFunction.SubscriptionPrice = subscriptionPrice;
                createAssetFunction.SubscriptionDuration = subscriptionDuration;
                createAssetFunction.TokenAddress = tokenAddress;
                createAssetFunction.Owner = owner;
            
             return ContractHandler.SendRequestAsync(createAssetFunction);
        }

        public virtual Task<TransactionReceipt> CreateAssetRequestAndWaitForReceiptAsync(byte[] assetId, BigInteger subscriptionPrice, BigInteger subscriptionDuration, string tokenAddress, string owner, CancellationTokenSource cancellationToken = null)
        {
            var createAssetFunction = new CreateAssetFunction();
                createAssetFunction.AssetId = assetId;
                createAssetFunction.SubscriptionPrice = subscriptionPrice;
                createAssetFunction.SubscriptionDuration = subscriptionDuration;
                createAssetFunction.TokenAddress = tokenAddress;
                createAssetFunction.Owner = owner;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(createAssetFunction, cancellationToken);
        }

        public virtual Task<string> EmitRegistryFeeClaimedEventRequestAsync(EmitRegistryFeeClaimedEventFunction emitRegistryFeeClaimedEventFunction)
        {
             return ContractHandler.SendRequestAsync(emitRegistryFeeClaimedEventFunction);
        }

        public virtual Task<TransactionReceipt> EmitRegistryFeeClaimedEventRequestAndWaitForReceiptAsync(EmitRegistryFeeClaimedEventFunction emitRegistryFeeClaimedEventFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(emitRegistryFeeClaimedEventFunction, cancellationToken);
        }

        public virtual Task<string> EmitRegistryFeeClaimedEventRequestAsync(byte[] assetId, byte[] subscriber, BigInteger claimedAmount, BigInteger claimedAtTimestamp, BigInteger claimedAtNonce)
        {
            var emitRegistryFeeClaimedEventFunction = new EmitRegistryFeeClaimedEventFunction();
                emitRegistryFeeClaimedEventFunction.AssetId = assetId;
                emitRegistryFeeClaimedEventFunction.Subscriber = subscriber;
                emitRegistryFeeClaimedEventFunction.ClaimedAmount = claimedAmount;
                emitRegistryFeeClaimedEventFunction.ClaimedAtTimestamp = claimedAtTimestamp;
                emitRegistryFeeClaimedEventFunction.ClaimedAtNonce = claimedAtNonce;
            
             return ContractHandler.SendRequestAsync(emitRegistryFeeClaimedEventFunction);
        }

        public virtual Task<TransactionReceipt> EmitRegistryFeeClaimedEventRequestAndWaitForReceiptAsync(byte[] assetId, byte[] subscriber, BigInteger claimedAmount, BigInteger claimedAtTimestamp, BigInteger claimedAtNonce, CancellationTokenSource cancellationToken = null)
        {
            var emitRegistryFeeClaimedEventFunction = new EmitRegistryFeeClaimedEventFunction();
                emitRegistryFeeClaimedEventFunction.AssetId = assetId;
                emitRegistryFeeClaimedEventFunction.Subscriber = subscriber;
                emitRegistryFeeClaimedEventFunction.ClaimedAmount = claimedAmount;
                emitRegistryFeeClaimedEventFunction.ClaimedAtTimestamp = claimedAtTimestamp;
                emitRegistryFeeClaimedEventFunction.ClaimedAtNonce = claimedAtNonce;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(emitRegistryFeeClaimedEventFunction, cancellationToken);
        }

        public Task<string> GetAssetQueryAsync(GetAssetFunction getAssetFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetAssetFunction, string>(getAssetFunction, blockParameter);
        }

        
        public virtual Task<string> GetAssetQueryAsync(byte[] assetId, BlockParameter blockParameter = null)
        {
            var getAssetFunction = new GetAssetFunction();
                getAssetFunction.AssetId = assetId;
            
            return ContractHandler.QueryAsync<GetAssetFunction, string>(getAssetFunction, blockParameter);
        }

        public Task<BigInteger> GetCreatorFeeQueryAsync(GetCreatorFeeFunction getCreatorFeeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetCreatorFeeFunction, BigInteger>(getCreatorFeeFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetCreatorFeeQueryAsync(BigInteger value, BlockParameter blockParameter = null)
        {
            var getCreatorFeeFunction = new GetCreatorFeeFunction();
                getCreatorFeeFunction.Value = value;
            
            return ContractHandler.QueryAsync<GetCreatorFeeFunction, BigInteger>(getCreatorFeeFunction, blockParameter);
        }

        public Task<BigInteger> GetCreatorFeeShareQueryAsync(GetCreatorFeeShareFunction getCreatorFeeShareFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetCreatorFeeShareFunction, BigInteger>(getCreatorFeeShareFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetCreatorFeeShareQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetCreatorFeeShareFunction, BigInteger>(null, blockParameter);
        }

        public virtual Task<GetFeeSharesOutputDTO> GetFeeSharesQueryAsync(GetFeeSharesFunction getFeeSharesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetFeeSharesFunction, GetFeeSharesOutputDTO>(getFeeSharesFunction, blockParameter);
        }

        public virtual Task<GetFeeSharesOutputDTO> GetFeeSharesQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetFeeSharesFunction, GetFeeSharesOutputDTO>(null, blockParameter);
        }

        public virtual Task<GetFeesOutputDTO> GetFeesQueryAsync(GetFeesFunction getFeesFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetFeesFunction, GetFeesOutputDTO>(getFeesFunction, blockParameter);
        }

        public virtual Task<GetFeesOutputDTO> GetFeesQueryAsync(BigInteger value, BlockParameter blockParameter = null)
        {
            var getFeesFunction = new GetFeesFunction();
                getFeesFunction.Value = value;
            
            return ContractHandler.QueryDeserializingToObjectAsync<GetFeesFunction, GetFeesOutputDTO>(getFeesFunction, blockParameter);
        }

        public Task<string> GetOwnerQueryAsync(GetOwnerFunction getOwnerFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetOwnerFunction, string>(getOwnerFunction, blockParameter);
        }

        
        public virtual Task<string> GetOwnerQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetOwnerFunction, string>(null, blockParameter);
        }

        public Task<BigInteger> GetRegistryFeeQueryAsync(GetRegistryFeeFunction getRegistryFeeFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRegistryFeeFunction, BigInteger>(getRegistryFeeFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetRegistryFeeQueryAsync(BigInteger value, BlockParameter blockParameter = null)
        {
            var getRegistryFeeFunction = new GetRegistryFeeFunction();
                getRegistryFeeFunction.Value = value;
            
            return ContractHandler.QueryAsync<GetRegistryFeeFunction, BigInteger>(getRegistryFeeFunction, blockParameter);
        }

        public Task<BigInteger> GetRegistryFeeShareQueryAsync(GetRegistryFeeShareFunction getRegistryFeeShareFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRegistryFeeShareFunction, BigInteger>(getRegistryFeeShareFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetRegistryFeeShareQueryAsync(BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetRegistryFeeShareFunction, BigInteger>(null, blockParameter);
        }

        public Task<BigInteger> GetSubscriptionDurationQueryAsync(GetSubscriptionDurationFunction getSubscriptionDurationFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSubscriptionDurationFunction, BigInteger>(getSubscriptionDurationFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetSubscriptionDurationQueryAsync(byte[] assetId, BlockParameter blockParameter = null)
        {
            var getSubscriptionDurationFunction = new GetSubscriptionDurationFunction();
                getSubscriptionDurationFunction.AssetId = assetId;
            
            return ContractHandler.QueryAsync<GetSubscriptionDurationFunction, BigInteger>(getSubscriptionDurationFunction, blockParameter);
        }

        public Task<BigInteger> GetSubscriptionExpirationQueryAsync(GetSubscriptionExpirationFunction getSubscriptionExpirationFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSubscriptionExpirationFunction, BigInteger>(getSubscriptionExpirationFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetSubscriptionExpirationQueryAsync(byte[] assetId, byte[] subscriber, BlockParameter blockParameter = null)
        {
            var getSubscriptionExpirationFunction = new GetSubscriptionExpirationFunction();
                getSubscriptionExpirationFunction.AssetId = assetId;
                getSubscriptionExpirationFunction.Subscriber = subscriber;
            
            return ContractHandler.QueryAsync<GetSubscriptionExpirationFunction, BigInteger>(getSubscriptionExpirationFunction, blockParameter);
        }

        public Task<BigInteger> GetSubscriptionPriceQueryAsync(GetSubscriptionPriceFunction getSubscriptionPriceFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<GetSubscriptionPriceFunction, BigInteger>(getSubscriptionPriceFunction, blockParameter);
        }

        
        public virtual Task<BigInteger> GetSubscriptionPriceQueryAsync(byte[] assetId, BigInteger count, BlockParameter blockParameter = null)
        {
            var getSubscriptionPriceFunction = new GetSubscriptionPriceFunction();
                getSubscriptionPriceFunction.AssetId = assetId;
                getSubscriptionPriceFunction.Count = count;
            
            return ContractHandler.QueryAsync<GetSubscriptionPriceFunction, BigInteger>(getSubscriptionPriceFunction, blockParameter);
        }

        public virtual Task<GetSubscriptionPriceAndDurationOutputDTO> GetSubscriptionPriceAndDurationQueryAsync(GetSubscriptionPriceAndDurationFunction getSubscriptionPriceAndDurationFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryDeserializingToObjectAsync<GetSubscriptionPriceAndDurationFunction, GetSubscriptionPriceAndDurationOutputDTO>(getSubscriptionPriceAndDurationFunction, blockParameter);
        }

        public virtual Task<GetSubscriptionPriceAndDurationOutputDTO> GetSubscriptionPriceAndDurationQueryAsync(byte[] assetId, BigInteger count, BlockParameter blockParameter = null)
        {
            var getSubscriptionPriceAndDurationFunction = new GetSubscriptionPriceAndDurationFunction();
                getSubscriptionPriceAndDurationFunction.AssetId = assetId;
                getSubscriptionPriceAndDurationFunction.Count = count;
            
            return ContractHandler.QueryDeserializingToObjectAsync<GetSubscriptionPriceAndDurationFunction, GetSubscriptionPriceAndDurationOutputDTO>(getSubscriptionPriceAndDurationFunction, blockParameter);
        }

        public Task<bool> IsSubscriberRevokedQueryAsync(IsSubscriberRevokedFunction isSubscriberRevokedFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsSubscriberRevokedFunction, bool>(isSubscriberRevokedFunction, blockParameter);
        }

        
        public virtual Task<bool> IsSubscriberRevokedQueryAsync(byte[] assetId, byte[] subscriber, BlockParameter blockParameter = null)
        {
            var isSubscriberRevokedFunction = new IsSubscriberRevokedFunction();
                isSubscriberRevokedFunction.AssetId = assetId;
                isSubscriberRevokedFunction.Subscriber = subscriber;
            
            return ContractHandler.QueryAsync<IsSubscriberRevokedFunction, bool>(isSubscriberRevokedFunction, blockParameter);
        }

        public Task<bool> IsSubscriptionActiveQueryAsync(IsSubscriptionActiveFunction isSubscriptionActiveFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsSubscriptionActiveFunction, bool>(isSubscriptionActiveFunction, blockParameter);
        }

        
        public virtual Task<bool> IsSubscriptionActiveQueryAsync(byte[] assetId, byte[] subscriber, BlockParameter blockParameter = null)
        {
            var isSubscriptionActiveFunction = new IsSubscriptionActiveFunction();
                isSubscriptionActiveFunction.AssetId = assetId;
                isSubscriptionActiveFunction.Subscriber = subscriber;
            
            return ContractHandler.QueryAsync<IsSubscriptionActiveFunction, bool>(isSubscriptionActiveFunction, blockParameter);
        }

        public Task<bool> IsSubscriptionExpiredQueryAsync(IsSubscriptionExpiredFunction isSubscriptionExpiredFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<IsSubscriptionExpiredFunction, bool>(isSubscriptionExpiredFunction, blockParameter);
        }

        
        public virtual Task<bool> IsSubscriptionExpiredQueryAsync(byte[] assetId, byte[] subscriber, BlockParameter blockParameter = null)
        {
            var isSubscriptionExpiredFunction = new IsSubscriptionExpiredFunction();
                isSubscriptionExpiredFunction.AssetId = assetId;
                isSubscriptionExpiredFunction.Subscriber = subscriber;
            
            return ContractHandler.QueryAsync<IsSubscriptionExpiredFunction, bool>(isSubscriptionExpiredFunction, blockParameter);
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

        public virtual Task<string> SubscribeRequestAsync(SubscribeFunction subscribeFunction)
        {
             return ContractHandler.SendRequestAsync(subscribeFunction);
        }

        public virtual Task<TransactionReceipt> SubscribeRequestAndWaitForReceiptAsync(SubscribeFunction subscribeFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(subscribeFunction, cancellationToken);
        }

        public virtual Task<string> SubscribeRequestAsync(byte[] assetId, byte[] subscriber, string payer, string spender, BigInteger count, BigInteger deadline, byte v, byte[] r, byte[] s)
        {
            var subscribeFunction = new SubscribeFunction();
                subscribeFunction.AssetId = assetId;
                subscribeFunction.Subscriber = subscriber;
                subscribeFunction.Payer = payer;
                subscribeFunction.Spender = spender;
                subscribeFunction.Count = count;
                subscribeFunction.Deadline = deadline;
                subscribeFunction.V = v;
                subscribeFunction.R = r;
                subscribeFunction.S = s;
            
             return ContractHandler.SendRequestAsync(subscribeFunction);
        }

        public virtual Task<TransactionReceipt> SubscribeRequestAndWaitForReceiptAsync(byte[] assetId, byte[] subscriber, string payer, string spender, BigInteger count, BigInteger deadline, byte v, byte[] r, byte[] s, CancellationTokenSource cancellationToken = null)
        {
            var subscribeFunction = new SubscribeFunction();
                subscribeFunction.AssetId = assetId;
                subscribeFunction.Subscriber = subscriber;
                subscribeFunction.Payer = payer;
                subscribeFunction.Spender = spender;
                subscribeFunction.Count = count;
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

        public virtual Task<string> UpdateRegistryFeeShareRequestAsync(UpdateRegistryFeeShareFunction updateRegistryFeeShareFunction)
        {
             return ContractHandler.SendRequestAsync(updateRegistryFeeShareFunction);
        }

        public virtual Task<TransactionReceipt> UpdateRegistryFeeShareRequestAndWaitForReceiptAsync(UpdateRegistryFeeShareFunction updateRegistryFeeShareFunction, CancellationTokenSource cancellationToken = null)
        {
             return ContractHandler.SendRequestAndWaitForReceiptAsync(updateRegistryFeeShareFunction, cancellationToken);
        }

        public virtual Task<string> UpdateRegistryFeeShareRequestAsync(BigInteger registryFeeShare)
        {
            var updateRegistryFeeShareFunction = new UpdateRegistryFeeShareFunction();
                updateRegistryFeeShareFunction.RegistryFeeShare = registryFeeShare;
            
             return ContractHandler.SendRequestAsync(updateRegistryFeeShareFunction);
        }

        public virtual Task<TransactionReceipt> UpdateRegistryFeeShareRequestAndWaitForReceiptAsync(BigInteger registryFeeShare, CancellationTokenSource cancellationToken = null)
        {
            var updateRegistryFeeShareFunction = new UpdateRegistryFeeShareFunction();
                updateRegistryFeeShareFunction.RegistryFeeShare = registryFeeShare;
            
             return ContractHandler.SendRequestAndWaitForReceiptAsync(updateRegistryFeeShareFunction, cancellationToken);
        }

        public Task<bool> ViewAssetQueryAsync(ViewAssetFunction viewAssetFunction, BlockParameter blockParameter = null)
        {
            return ContractHandler.QueryAsync<ViewAssetFunction, bool>(viewAssetFunction, blockParameter);
        }

        
        public virtual Task<bool> ViewAssetQueryAsync(byte[] assetId, BlockParameter blockParameter = null)
        {
            var viewAssetFunction = new ViewAssetFunction();
                viewAssetFunction.AssetId = assetId;
            
            return ContractHandler.QueryAsync<ViewAssetFunction, bool>(viewAssetFunction, blockParameter);
        }

        public override List<Type> GetAllFunctionTypes()
        {
            return new List<Type>
            {
                typeof(AssetsFunction),
                typeof(ClaimRegistryFeeFunction),
                typeof(ClaimRegistryFee1Function),
                typeof(CreateAssetFunction),
                typeof(EmitRegistryFeeClaimedEventFunction),
                typeof(GetAssetFunction),
                typeof(GetCreatorFeeFunction),
                typeof(GetCreatorFeeShareFunction),
                typeof(GetFeeSharesFunction),
                typeof(GetFeesFunction),
                typeof(GetOwnerFunction),
                typeof(GetRegistryFeeFunction),
                typeof(GetRegistryFeeShareFunction),
                typeof(GetSubscriptionDurationFunction),
                typeof(GetSubscriptionExpirationFunction),
                typeof(GetSubscriptionPriceFunction),
                typeof(GetSubscriptionPriceAndDurationFunction),
                typeof(IsSubscriberRevokedFunction),
                typeof(IsSubscriptionActiveFunction),
                typeof(IsSubscriptionExpiredFunction),
                typeof(OwnerFunction),
                typeof(RenounceOwnershipFunction),
                typeof(SubscribeFunction),
                typeof(TransferOwnershipFunction),
                typeof(UpdateRegistryFeeShareFunction),
                typeof(ViewAssetFunction)
            };
        }

        public override List<Type> GetAllEventTypes()
        {
            return new List<Type>
            {
                typeof(AssetCreatedEventDTO),
                typeof(OwnershipTransferredEventDTO),
                typeof(RegistryFeeClaimedEventDTO),
                typeof(RegistryFeeClaimedBatchEventDTO),
                typeof(RegistryFeeShareUpdatedEventDTO)
            };
        }

        public override List<Type> GetAllErrorTypes()
        {
            return new List<Type>
            {
                typeof(AssetAlreadyExistsError),
                typeof(AssetNotFoundError),
                typeof(OnlyAssetUnauthorizedAccountError),
                typeof(OnlyUnrevokedUnauthorizedSubscriberError),
                typeof(OwnableInvalidOwnerError),
                typeof(OwnableUnauthorizedAccountError),
                typeof(RegistryFeeShareOutOfBoundsError)
            };
        }
    }
}
