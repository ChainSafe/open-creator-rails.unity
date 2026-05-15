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

namespace Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition
{


    public partial class AssetDeployment : AssetDeploymentBase
    {
        public AssetDeployment() : base(BYTECODE) { }
        public AssetDeployment(string byteCode) : base(byteCode) { }
    }

    public class AssetDeploymentBase : ContractDeploymentMessage
    {
        public static string BYTECODE = "";
        public AssetDeploymentBase() : base(BYTECODE) { }
        public AssetDeploymentBase(string byteCode) : base(byteCode) { }
        [Parameter("bytes32", "_assetId", 1)]
        public virtual byte[] AssetId { get; set; }
        [Parameter("uint256", "_subscriptionPrice", 2)]
        public virtual BigInteger SubscriptionPrice { get; set; }
        [Parameter("uint256", "_subscriptionDuration", 3)]
        public virtual BigInteger SubscriptionDuration { get; set; }
        [Parameter("address", "_tokenAddress", 4)]
        public virtual string TokenAddress { get; set; }
        [Parameter("address", "_owner", 5)]
        public virtual string Owner { get; set; }
    }

    public partial class CancelSubscriptionFunction : CancelSubscriptionFunctionBase { }

    [Function("cancelSubscription")]
    public class CancelSubscriptionFunctionBase : FunctionMessage
    {
        [Parameter("string", "subscriberId", 1)]
        public virtual string SubscriberId { get; set; }
        [Parameter("bytes", "signature", 2)]
        public virtual byte[] Signature { get; set; }
    }

    public partial class ClaimCreatorFeeFunction : ClaimCreatorFeeFunctionBase { }

    [Function("claimCreatorFee", "uint256")]
    public class ClaimCreatorFeeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class ClaimCreatorFee1Function : ClaimCreatorFee1FunctionBase { }

    [Function("claimCreatorFee", "uint256")]
    public class ClaimCreatorFee1FunctionBase : FunctionMessage
    {
        [Parameter("bytes32[]", "_subscribers", 1)]
        public virtual List<byte[]> Subscribers { get; set; }
    }

    public partial class ClaimRegistryFeeFunction : ClaimRegistryFeeFunctionBase { }

    [Function("claimRegistryFee", typeof(ClaimRegistryFeeOutputDTO))]
    public class ClaimRegistryFeeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class ClaimRegistryFee1Function : ClaimRegistryFee1FunctionBase { }

    [Function("claimRegistryFee", "uint256")]
    public class ClaimRegistryFee1FunctionBase : FunctionMessage
    {
        [Parameter("bytes32[]", "_subscribers", 1)]
        public virtual List<byte[]> Subscribers { get; set; }
    }

    public partial class GetAssetIdFunction : GetAssetIdFunctionBase { }

    [Function("getAssetId", "bytes32")]
    public class GetAssetIdFunctionBase : FunctionMessage
    {

    }

    public partial class GetRegistryAddressFunction : GetRegistryAddressFunctionBase { }

    [Function("getRegistryAddress", "address")]
    public class GetRegistryAddressFunctionBase : FunctionMessage
    {

    }

    public partial class GetSubscriptionDurationFunction : GetSubscriptionDurationFunctionBase { }

    [Function("getSubscriptionDuration", "uint256")]
    public class GetSubscriptionDurationFunctionBase : FunctionMessage
    {

    }

    public partial class GetSubscriptionExpirationFunction : GetSubscriptionExpirationFunctionBase { }

    [Function("getSubscriptionExpiration", "uint256")]
    public class GetSubscriptionExpirationFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class GetSubscriptionPriceFunction : GetSubscriptionPriceFunctionBase { }

    [Function("getSubscriptionPrice", "uint256")]
    public class GetSubscriptionPriceFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "count", 1)]
        public virtual BigInteger Count { get; set; }
    }

    public partial class GetSubscriptionPriceAndDurationFunction : GetSubscriptionPriceAndDurationFunctionBase { }

    [Function("getSubscriptionPriceAndDuration", typeof(GetSubscriptionPriceAndDurationOutputDTO))]
    public class GetSubscriptionPriceAndDurationFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "count", 1)]
        public virtual BigInteger Count { get; set; }
    }

    public partial class GetTokenAddressFunction : GetTokenAddressFunctionBase { }

    [Function("getTokenAddress", "address")]
    public class GetTokenAddressFunctionBase : FunctionMessage
    {

    }

    public partial class IsSubscriberRevokedFunction : IsSubscriberRevokedFunctionBase { }

    [Function("isSubscriberRevoked", "bool")]
    public class IsSubscriberRevokedFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class IsSubscriptionActiveFunction : IsSubscriptionActiveFunctionBase { }

    [Function("isSubscriptionActive", "bool")]
    public class IsSubscriptionActiveFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class IsSubscriptionExpiredFunction : IsSubscriptionExpiredFunctionBase { }

    [Function("isSubscriptionExpired", "bool")]
    public class IsSubscriptionExpiredFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
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

    public partial class RevokeSubscriptionFunction : RevokeSubscriptionFunctionBase { }

    [Function("revokeSubscription")]
    public class RevokeSubscriptionFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class SetSubscriptionPriceFunction : SetSubscriptionPriceFunctionBase { }

    [Function("setSubscriptionPrice")]
    public class SetSubscriptionPriceFunctionBase : FunctionMessage
    {
        [Parameter("uint256", "newSubscriptionPrice", 1)]
        public virtual BigInteger NewSubscriptionPrice { get; set; }
    }

    public partial class SubscribeFunction : SubscribeFunctionBase { }

    [Function("subscribe", "uint256")]
    public class SubscribeFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("address", "payer", 2)]
        public virtual string Payer { get; set; }
        [Parameter("address", "spender", 3)]
        public virtual string Spender { get; set; }
        [Parameter("uint256", "count", 4)]
        public virtual BigInteger Count { get; set; }
        [Parameter("uint256", "deadline", 5)]
        public virtual BigInteger Deadline { get; set; }
        [Parameter("uint8", "v", 6)]
        public virtual byte V { get; set; }
        [Parameter("bytes32", "r", 7)]
        public virtual byte[] R { get; set; }
        [Parameter("bytes32", "s", 8)]
        public virtual byte[] S { get; set; }
    }

    public partial class TransferOwnershipFunction : TransferOwnershipFunctionBase { }

    [Function("transferOwnership")]
    public class TransferOwnershipFunctionBase : FunctionMessage
    {
        [Parameter("address", "newOwner", 1)]
        public virtual string NewOwner { get; set; }
    }

    public partial class UnrevokeSubscriptionFunction : UnrevokeSubscriptionFunctionBase { }

    [Function("unrevokeSubscription")]
    public class UnrevokeSubscriptionFunctionBase : FunctionMessage
    {
        [Parameter("bytes32", "subscriber", 1)]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class CreatorFeeClaimedEventDTO : CreatorFeeClaimedEventDTOBase { }

    [Event("CreatorFeeClaimed")]
    public class CreatorFeeClaimedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "amount", 2, false )]
        public virtual BigInteger Amount { get; set; }
        [Parameter("uint256", "claimedAtTimestamp", 3, true )]
        public virtual BigInteger ClaimedAtTimestamp { get; set; }
        [Parameter("uint256", "claimedAtNonce", 4, true )]
        public virtual BigInteger ClaimedAtNonce { get; set; }
    }

    public partial class CreatorFeeClaimedBatchEventDTO : CreatorFeeClaimedBatchEventDTOBase { }

    [Event("CreatorFeeClaimedBatch")]
    public class CreatorFeeClaimedBatchEventDTOBase : IEventDTO
    {
        [Parameter("bytes32[]", "subscribers", 1, true )]
        public virtual List<byte[]> Subscribers { get; set; }
        [Parameter("uint256", "totalAmount", 2, false )]
        public virtual BigInteger TotalAmount { get; set; }
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

    public partial class SubscriptionAddedEventDTO : SubscriptionAddedEventDTOBase { }

    [Event("SubscriptionAdded")]
    public class SubscriptionAddedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "startTime", 2, true )]
        public virtual BigInteger StartTime { get; set; }
        [Parameter("uint256", "endTime", 3, true )]
        public virtual BigInteger EndTime { get; set; }
        [Parameter("address", "payer", 4, false )]
        public virtual string Payer { get; set; }
        [Parameter("uint256", "subscriptionPrice", 5, false )]
        public virtual BigInteger SubscriptionPrice { get; set; }
        [Parameter("uint256", "registryFeeShare", 6, false )]
        public virtual BigInteger RegistryFeeShare { get; set; }
    }

    public partial class SubscriptionCancelledEventDTO : SubscriptionCancelledEventDTOBase { }

    [Event("SubscriptionCancelled")]
    public class SubscriptionCancelledEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "nonce", 2, true )]
        public virtual BigInteger Nonce { get; set; }
        [Parameter("uint256", "endTime", 3, true )]
        public virtual BigInteger EndTime { get; set; }
    }

    public partial class SubscriptionExtendedEventDTO : SubscriptionExtendedEventDTOBase { }

    [Event("SubscriptionExtended")]
    public class SubscriptionExtendedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "endTime", 2, true )]
        public virtual BigInteger EndTime { get; set; }
    }

    public partial class SubscriptionPriceUpdatedEventDTO : SubscriptionPriceUpdatedEventDTOBase { }

    [Event("SubscriptionPriceUpdated")]
    public class SubscriptionPriceUpdatedEventDTOBase : IEventDTO
    {
        [Parameter("uint256", "newSubscriptionPrice", 1, false )]
        public virtual BigInteger NewSubscriptionPrice { get; set; }
    }

    public partial class SubscriptionRemovedEventDTO : SubscriptionRemovedEventDTOBase { }

    [Event("SubscriptionRemoved")]
    public class SubscriptionRemovedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class SubscriptionRenewedEventDTO : SubscriptionRenewedEventDTOBase { }

    [Event("SubscriptionRenewed")]
    public class SubscriptionRenewedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "startTime", 2, true )]
        public virtual BigInteger StartTime { get; set; }
        [Parameter("uint256", "endTime", 3, true )]
        public virtual BigInteger EndTime { get; set; }
        [Parameter("uint256", "nonce", 4, false )]
        public virtual BigInteger Nonce { get; set; }
        [Parameter("address", "payer", 5, false )]
        public virtual string Payer { get; set; }
        [Parameter("uint256", "subscriptionPrice", 6, false )]
        public virtual BigInteger SubscriptionPrice { get; set; }
        [Parameter("uint256", "registryFeeShare", 7, false )]
        public virtual BigInteger RegistryFeeShare { get; set; }
    }

    public partial class SubscriptionRevokedEventDTO : SubscriptionRevokedEventDTOBase { }

    [Event("SubscriptionRevoked")]
    public class SubscriptionRevokedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
        [Parameter("uint256", "nonce", 2, true )]
        public virtual BigInteger Nonce { get; set; }
        [Parameter("uint256", "endTime", 3, true )]
        public virtual BigInteger EndTime { get; set; }
    }

    public partial class SubscriptionUnrevokedEventDTO : SubscriptionUnrevokedEventDTOBase { }

    [Event("SubscriptionUnrevoked")]
    public class SubscriptionUnrevokedEventDTOBase : IEventDTO
    {
        [Parameter("bytes32", "subscriber", 1, true )]
        public virtual byte[] Subscriber { get; set; }
    }

    public partial class ECDSAInvalidSignatureError : ECDSAInvalidSignatureErrorBase { }
    [Error("ECDSAInvalidSignature")]
    public class ECDSAInvalidSignatureErrorBase : IErrorDTO
    {
    }

    public partial class ECDSAInvalidSignatureLengthError : ECDSAInvalidSignatureLengthErrorBase { }

    [Error("ECDSAInvalidSignatureLength")]
    public class ECDSAInvalidSignatureLengthErrorBase : IErrorDTO
    {
        [Parameter("uint256", "length", 1)]
        public virtual BigInteger Length { get; set; }
    }

    public partial class ECDSAInvalidSignatureSError : ECDSAInvalidSignatureSErrorBase { }

    [Error("ECDSAInvalidSignatureS")]
    public class ECDSAInvalidSignatureSErrorBase : IErrorDTO
    {
        [Parameter("bytes32", "s", 1)]
        public virtual byte[] S { get; set; }
    }

    public partial class InsufficientFundsError : InsufficientFundsErrorBase { }
    [Error("InsufficientFunds")]
    public class InsufficientFundsErrorBase : IErrorDTO
    {
    }

    public partial class InvalidOwnerError : InvalidOwnerErrorBase { }
    [Error("InvalidOwner")]
    public class InvalidOwnerErrorBase : IErrorDTO
    {
    }

    public partial class InvalidSignatureError : InvalidSignatureErrorBase { }
    [Error("InvalidSignature")]
    public class InvalidSignatureErrorBase : IErrorDTO
    {
    }

    public partial class InvalidSpenderError : InvalidSpenderErrorBase { }
    [Error("InvalidSpender")]
    public class InvalidSpenderErrorBase : IErrorDTO
    {
    }

    public partial class InvalidSubscriptionDurationError : InvalidSubscriptionDurationErrorBase { }
    [Error("InvalidSubscriptionDuration")]
    public class InvalidSubscriptionDurationErrorBase : IErrorDTO
    {
    }

    public partial class InvalidTokenAddressError : InvalidTokenAddressErrorBase { }
    [Error("InvalidTokenAddress")]
    public class InvalidTokenAddressErrorBase : IErrorDTO
    {
    }

    public partial class OnlyRegistryUnauthorizedAccountError : OnlyRegistryUnauthorizedAccountErrorBase { }
    [Error("OnlyRegistryUnauthorizedAccount")]
    public class OnlyRegistryUnauthorizedAccountErrorBase : IErrorDTO
    {
    }

    public partial class OnlyUnrevokedUnauthorizedSubscriberError : OnlyUnrevokedUnauthorizedSubscriberErrorBase { }
    [Error("OnlyUnrevokedUnauthorizedSubscriber")]
    public class OnlyUnrevokedUnauthorizedSubscriberErrorBase : IErrorDTO
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

    public partial class PermitFailedError : PermitFailedErrorBase { }
    [Error("PermitFailed")]
    public class PermitFailedErrorBase : IErrorDTO
    {
    }

    public partial class ReentrancyGuardReentrantCallError : ReentrancyGuardReentrantCallErrorBase { }
    [Error("ReentrancyGuardReentrantCall")]
    public class ReentrancyGuardReentrantCallErrorBase : IErrorDTO
    {
    }

    public partial class SafeERC20FailedOperationError : SafeERC20FailedOperationErrorBase { }

    [Error("SafeERC20FailedOperation")]
    public class SafeERC20FailedOperationErrorBase : IErrorDTO
    {
        [Parameter("address", "token", 1)]
        public virtual string Token { get; set; }
    }

    public partial class SubscriptionAlreadyRevokedError : SubscriptionAlreadyRevokedErrorBase { }
    [Error("SubscriptionAlreadyRevoked")]
    public class SubscriptionAlreadyRevokedErrorBase : IErrorDTO
    {
    }

    public partial class SubscriptionCancellationFailedError : SubscriptionCancellationFailedErrorBase { }
    [Error("SubscriptionCancellationFailed")]
    public class SubscriptionCancellationFailedErrorBase : IErrorDTO
    {
    }

    public partial class SubscriptionNotFoundError : SubscriptionNotFoundErrorBase { }
    [Error("SubscriptionNotFound")]
    public class SubscriptionNotFoundErrorBase : IErrorDTO
    {
    }

    public partial class SubscriptionNotRevokedError : SubscriptionNotRevokedErrorBase { }
    [Error("SubscriptionNotRevoked")]
    public class SubscriptionNotRevokedErrorBase : IErrorDTO
    {
    }

    public partial class SubscriptionRevocationFailedError : SubscriptionRevocationFailedErrorBase { }
    [Error("SubscriptionRevocationFailed")]
    public class SubscriptionRevocationFailedErrorBase : IErrorDTO
    {
    }







    public partial class ClaimRegistryFeeOutputDTO : ClaimRegistryFeeOutputDTOBase { }

    [FunctionOutput]
    public class ClaimRegistryFeeOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "claimedAmount", 1)]
        public virtual BigInteger ClaimedAmount { get; set; }
        [Parameter("uint256", "claimedAtTimestamp", 2)]
        public virtual BigInteger ClaimedAtTimestamp { get; set; }
        [Parameter("uint256", "claimedAtNonce", 3)]
        public virtual BigInteger ClaimedAtNonce { get; set; }
    }



    public partial class GetAssetIdOutputDTO : GetAssetIdOutputDTOBase { }

    [FunctionOutput]
    public class GetAssetIdOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bytes32", "", 1)]
        public virtual byte[] ReturnValue1 { get; set; }
    }

    public partial class GetRegistryAddressOutputDTO : GetRegistryAddressOutputDTOBase { }

    [FunctionOutput]
    public class GetRegistryAddressOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class GetSubscriptionDurationOutputDTO : GetSubscriptionDurationOutputDTOBase { }

    [FunctionOutput]
    public class GetSubscriptionDurationOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "", 1)]
        public virtual BigInteger ReturnValue1 { get; set; }
    }

    public partial class GetSubscriptionExpirationOutputDTO : GetSubscriptionExpirationOutputDTOBase { }

    [FunctionOutput]
    public class GetSubscriptionExpirationOutputDTOBase : IFunctionOutputDTO 
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

    public partial class GetSubscriptionPriceAndDurationOutputDTO : GetSubscriptionPriceAndDurationOutputDTOBase { }

    [FunctionOutput]
    public class GetSubscriptionPriceAndDurationOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("uint256", "price", 1)]
        public virtual BigInteger Price { get; set; }
        [Parameter("uint256", "duration", 2)]
        public virtual BigInteger Duration { get; set; }
    }

    public partial class GetTokenAddressOutputDTO : GetTokenAddressOutputDTOBase { }

    [FunctionOutput]
    public class GetTokenAddressOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("address", "", 1)]
        public virtual string ReturnValue1 { get; set; }
    }

    public partial class IsSubscriberRevokedOutputDTO : IsSubscriberRevokedOutputDTOBase { }

    [FunctionOutput]
    public class IsSubscriberRevokedOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class IsSubscriptionActiveOutputDTO : IsSubscriptionActiveOutputDTOBase { }

    [FunctionOutput]
    public class IsSubscriptionActiveOutputDTOBase : IFunctionOutputDTO 
    {
        [Parameter("bool", "", 1)]
        public virtual bool ReturnValue1 { get; set; }
    }

    public partial class IsSubscriptionExpiredOutputDTO : IsSubscriptionExpiredOutputDTOBase { }

    [FunctionOutput]
    public class IsSubscriptionExpiredOutputDTOBase : IFunctionOutputDTO 
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












}
