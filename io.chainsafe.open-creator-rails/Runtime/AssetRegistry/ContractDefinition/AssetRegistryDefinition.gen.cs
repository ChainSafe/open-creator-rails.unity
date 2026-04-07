using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Contracts.CQS;
using Nethereum.Contracts;
using System.Threading;

namespace Io.ChainSafe.OpenCreatorRails.AssetRegistry.ContractDefinition
{


    public partial class AssetRegistryDeployment : AssetRegistryDeploymentBase
    {
        public AssetRegistryDeployment() : base(BYTECODE) { }
        public AssetRegistryDeployment(string byteCode) : base(byteCode) { }
    }

    public class AssetRegistryDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public AssetRegistryDeploymentBase() : base(BYTECODE) { }
        public AssetRegistryDeploymentBase(string byteCode) : base(byteCode) { }
        [Parameter("uint256", "_registryFeeShare", 1)]
        public virtual BigInteger RegistryFeeShare { get; set; }
    }

    public partial class AssetsFunction : AssetsFunctionBase { }

    [Function("assets", "address")]
    public class AssetsFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public partial class CancelSubscriptionFunction : CancelSubscriptionFunctionBase { }

    [Function("cancelSubscription")]
    public class CancelSubscriptionFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32", "_subscriber", 2)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class ClaimRegistryFeeFunction : ClaimRegistryFeeFunctionBase { }

    [Function("claimRegistryFee", "uint256")]
    public class ClaimRegistryFeeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32[]", "_subscribers", 2)]
        public virtual List<byte[]> Subscribers { get; set; }
    }

    public partial class ClaimRegistryFee1Function : ClaimRegistryFee1FunctionBase { }

    [Function("claimRegistryFee", "uint256")]
    public class ClaimRegistryFee1FunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32", "_subscriber", 2)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class CreateAssetFunction : CreateAssetFunctionBase { }

    [Function("createAsset", "address")]
    public class CreateAssetFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("uint256", "_subscriptionPrice", 2)]
        public virtual BigInteger SubscriptionPrice { get; set; }
        [Parameter("address", "_tokenAddress", 3)]
        public virtual string TokenAddress { get; set; }
        [Parameter("address", "_owner", 4)]
        public virtual string Owner { get; set; }
    }

    public partial class GetAssetFunction : GetAssetFunctionBase { }

    [Function("getAsset", "address")]
    public class GetAssetFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
    }

    public partial class GetCreatorFeeFunction : GetCreatorFeeFunctionBase { }

    [Function("getCreatorFee", "uint256")]
    public class GetCreatorFeeFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_value", 1)]
        public virtual BigInteger Value { get; set; }
    }

    public partial class GetCreatorFeeShareFunction : GetCreatorFeeShareFunctionBase { }

    [Function("getCreatorFeeShare", "uint256")]
    public class GetCreatorFeeShareFunctionBase : FunctionMessage
    {

    }

    public partial class GetFeeSharesFunction : GetFeeSharesFunctionBase { }

    [Function("getFeeShares", typeof(GetFeeSharesOutputDTO))]
    public class GetFeeSharesFunctionBase : FunctionMessage
    {

    }

    public partial class GetFeesFunction : GetFeesFunctionBase { }

    [Function("getFees", typeof(GetFeesOutputDTO))]
    public class GetFeesFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_value", 1)]
        public virtual BigInteger Value { get; set; }
    }

    public partial class GetOwnerFunction : GetOwnerFunctionBase { }

    [Function("getOwner", "address")]
    public class GetOwnerFunctionBase : FunctionMessage
    {

    }

    public partial class GetRegistryFeeFunction : GetRegistryFeeFunctionBase { }

    [Function("getRegistryFee", "uint256")]
    public class GetRegistryFeeFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_value", 1)]
        public virtual BigInteger Value { get; set; }
    }

    public partial class GetRegistryFeeShareFunction : GetRegistryFeeShareFunctionBase { }

    [Function("getRegistryFeeShare", "uint256")]
    public class GetRegistryFeeShareFunctionBase : FunctionMessage
    {

    }

    public partial class GetSubscriptionFunction : GetSubscriptionFunctionBase { }

    [Function("getSubscription", "uint256")]
    public class GetSubscriptionFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32", "_subscriber", 2)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class GetSubscriptionPriceFunction : GetSubscriptionPriceFunctionBase { }

    [Function("getSubscriptionPrice", "uint256")]
    public class GetSubscriptionPriceFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("uint256", "_duration", 2)]
        public virtual BigInteger Duration { get; set; }
    }

    public partial class IsSubscriptionActiveFunction : IsSubscriptionActiveFunctionBase { }

    [Function("isSubscriptionActive", "bool")]
    public class IsSubscriptionActiveFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32", "_subscriber", 2)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class OwnerFunction : OwnerFunctionBase { }

    [Function("owner", "address")]
    public class OwnerFunctionBase : FunctionMessage
    {

    }

    public partial class RenounceOwnershipFunction : RenounceOwnershipFunctionBase { }

    [Function("renounceOwnership")]
    public class RenounceOwnershipFunctionBase : FunctionMessage
    {

    }

    public partial class SubscribeFunction : SubscribeFunctionBase { }

    [Function("subscribe", "uint256")]
    public class SubscribeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32", "_subscriber", 2)]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("address", "_payer", 3)]
        public virtual string Payer { get; set; }
        [Parameter("address", "_spender", 4)]
        public virtual string Spender { get; set; }
        [Parameter("uint256", "_value", 5)]
        public virtual BigInteger Value { get; set; }
        [Parameter("uint256", "_deadline", 6)]
        public virtual BigInteger Deadline { get; set; }
        [Parameter("uint8", "_v", 7)]
        public virtual byte V { get; set; }
        [Parameter("bytes32", "_r", 8)]
        public virtual byte[] R { get; set; }
        [Parameter("bytes32", "_s", 9)]
        public virtual byte[] S { get; set; }
    }

    public partial class TransferOwnershipFunction : TransferOwnershipFunctionBase { }

    [Function("transferOwnership")]
    public class TransferOwnershipFunctionBase : FunctionMessage
    {
        [Parameter("address", "newOwner", 1)]
        public virtual string NewOwner { get; set; }
    }

    public partial class UpdateRegistryFeeShareFunction : UpdateRegistryFeeShareFunctionBase { }

    [Function("updateRegistryFeeShare")]
    public class UpdateRegistryFeeShareFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "_registryFeeShare", 1)]
        public virtual BigInteger RegistryFeeShare { get; set; }
    }

    public partial class ViewAssetFunction : ViewAssetFunctionBase { }

    [Function("viewAsset", "bool")]
    public class ViewAssetFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
    }

    public partial class AssetCreatedEventDTO : AssetCreatedEventDTOBase { }

    [Event("AssetCreated")]
    public class AssetCreatedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "assetId", 1, true )]
        public virtual byte[] AssetId { get; set; }
        [Parameter("address", "asset", 2, true )]
        public virtual string Asset { get; set; }
        [Parameter("uint256", "subscriptionPrice", 3, false )]
        public virtual BigInteger SubscriptionPrice { get; set; }
        [Parameter("address", "tokenAddress", 4, false )]
        public virtual string TokenAddress { get; set; }
        [Parameter("address", "owner", 5, true )]
        public virtual string Owner { get; set; }
    }

    public partial class OwnershipTransferredEventDTO : OwnershipTransferredEventDTOBase { }

    [Event("OwnershipTransferred")]
    public class OwnershipTransferredEventDTOBase : IEventDTO
    {
        [Parameter("address", "previousOwner", 1, true )]
        public virtual string PreviousOwner { get; set; }
        [Parameter("address", "newOwner", 2, true )]
        public virtual string NewOwner { get; set; }
    }

    public partial class RegistryFeeClaimedEventDTO : RegistryFeeClaimedEventDTOBase { }

    [Event("RegistryFeeClaimed")]
    public class RegistryFeeClaimedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "amount", 2, false )]
        public virtual BigInteger Amount { get; set; }
    }

    public partial class RegistryFeeClaimedBatchEventDTO : RegistryFeeClaimedBatchEventDTOBase { }

    [Event("RegistryFeeClaimedBatch")]
    public class RegistryFeeClaimedBatchEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "assetId", 1, true )]
        public virtual byte[] AssetId { get; set; }
        [Parameter("bytes32[]", "subscribers", 2, true )]
        public virtual List<byte[]> Subscribers { get; set; }
        [Parameter("uint256", "totalAmount", 3, false )]
        public virtual BigInteger TotalAmount { get; set; }
    }

    public partial class RegistryFeeShareUpdatedEventDTO : RegistryFeeShareUpdatedEventDTOBase { }

    [Event("RegistryFeeShareUpdated")]
    public class RegistryFeeShareUpdatedEventDTOBase : IEventDTO
    {
        [Parameter("uint256", "newRegistryFeeShare", 1, false )]
        public virtual BigInteger NewRegistryFeeShare { get; set; }
    }

    public partial class AssetAlreadyExistsError : AssetAlreadyExistsErrorBase { }
    [Error("AssetAlreadyExists")]
    public class AssetAlreadyExistsErrorBase : IErrorDTO
    {
    }

    public partial class AssetNotFoundError : AssetNotFoundErrorBase { }
    [Error("AssetNotFound")]
    public class AssetNotFoundErrorBase : IErrorDTO
    {
    }

    public partial class OwnableInvalidOwnerError : OwnableInvalidOwnerErrorBase { }

    [Error("OwnableInvalidOwner")]
    public class OwnableInvalidOwnerErrorBase : IErrorDTO
    {
        [Parameter("address", "owner", 1)]
        public virtual string Owner { get; set; }
    }

    public partial class OwnableUnauthorizedAccountError : OwnableUnauthorizedAccountErrorBase { }

    [Error("OwnableUnauthorizedAccount")]
    public class OwnableUnauthorizedAccountErrorBase : IErrorDTO
    {
        [Parameter("address", "account", 1)]
        public virtual string Account { get; set; }
    }

    public partial class RegistryFeeShareOutOfBoundsError : RegistryFeeShareOutOfBoundsErrorBase { }
    [Error("RegistryFeeShareOutOfBounds")]
    public class RegistryFeeShareOutOfBoundsErrorBase : IErrorDTO
    {
    }

    public partial class AssetsOutputDTO : AssetsOutputDTOBase { }

    [FunctionOutput]
    public class AssetsOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }









    public partial class GetAssetOutputDTO : GetAssetOutputDTOBase { }

    [FunctionOutput]
    public class GetAssetOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class GetCreatorFeeOutputDTO : GetCreatorFeeOutputDTOBase { }

    [FunctionOutput]
    public class GetCreatorFeeOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetCreatorFeeShareOutputDTO : GetCreatorFeeShareOutputDTOBase { }

    [FunctionOutput]
    public class GetCreatorFeeShareOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetFeeSharesOutputDTO : GetFeeSharesOutputDTOBase { }

    [FunctionOutput]
    public class GetFeeSharesOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
        [Parameter("uint256", "", 2)]
        public virtual BigInteger ReturnValue2 { get; set; }
    }

    public partial class GetFeesOutputDTO : GetFeesOutputDTOBase { }

    [FunctionOutput]
    public class GetFeesOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "creatorFee", 1)]
        public virtual BigInteger CreatorFee { get; set; }
        [Parameter("uint256", "registryFee", 2)]
        public virtual BigInteger RegistryFee { get; set; }
    }

    public partial class GetOwnerOutputDTO : GetOwnerOutputDTOBase { }

    [FunctionOutput]
    public class GetOwnerOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class GetRegistryFeeOutputDTO : GetRegistryFeeOutputDTOBase { }

    [FunctionOutput]
    public class GetRegistryFeeOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetRegistryFeeShareOutputDTO : GetRegistryFeeShareOutputDTOBase { }

    [FunctionOutput]
    public class GetRegistryFeeShareOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetSubscriptionOutputDTO : GetSubscriptionOutputDTOBase { }

    [FunctionOutput]
    public class GetSubscriptionOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetSubscriptionPriceOutputDTO : GetSubscriptionPriceOutputDTOBase { }

    [FunctionOutput]
    public class GetSubscriptionPriceOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class IsSubscriptionActiveOutputDTO : IsSubscriptionActiveOutputDTOBase { }

    [FunctionOutput]
    public class IsSubscriptionActiveOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class OwnerOutputDTO : OwnerOutputDTOBase { }

    [FunctionOutput]
    public class OwnerOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }









    public partial class ViewAssetOutputDTO : ViewAssetOutputDTOBase { }

    [FunctionOutput]
    public class ViewAssetOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }
}
