using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Contracts.Asset.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.AssetRegistry.Service;
using Io.ChainSafe.OpenCreatorRails.Contracts.ERC20Permit.ContractDefinition;
using Io.ChainSafe.OpenCreatorRails.Contracts.ERC20Permit.Service;
using Io.ChainSafe.OpenCreatorRails.DTOs;
using Io.ChainSafe.OpenCreatorRails.Utils;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.EIP712.EIP2612;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class Asset : MonoBehaviour, IAsset
    {
        [field: SerializeField] public EthereumAddress RegistryAddress { get; private set; }

        [field: SerializeField] public string AssetId { get; private set; }

        public string AssetIdHash => AssetId.Keccack256();

        public EthereumAddress Address { get; private set; }

        public BigInteger SubscriptionPrice { get; private set; }

        public EthereumAddress Owner { get; private set; }

        public EthereumAddress TokenAddress { get; private set; }

        public List<SubscriptionDto> Subscriptions { get; private set; }

        public AssetService Service { get; private set; }

        public ERC20PermitService PermitService { get; private set; }

        public AssetRegistryService AssetRegistryService { get; private set; }

        private Eip712DomainOutputDTO _domain;

        private TypedData<Domain> _typedData;

        public async UniTask Connected(Web3 web3)
        {
            AssetDto assetDto =
                await OpenCreatorRailsService.Instance.IndexerProvider.GetAsset(AssetIdHash, RegistryAddress);

            Address = assetDto.Address;
            SubscriptionPrice = assetDto.SubscriptionPrice;
            Owner = assetDto.Owner;
            TokenAddress = assetDto.TokenAddress;
            Subscriptions = assetDto.Subscriptions;

            Service = new AssetService(web3, Address.Value);
            PermitService = new ERC20PermitService(web3, TokenAddress.Value);
            AssetRegistryService = new AssetRegistryService(web3, RegistryAddress.Value);

            SubscribeToEvents();

            _domain = await PermitService.Eip712DomainQueryAsync();

            _typedData = EIP2612TypeFactory.GetTypedDefinition();

            _typedData.Domain = new Domain
            {
                Name = _domain.Name,
                Version = _domain.Version,
                ChainId = _domain.ChainId,
                VerifyingContract = _domain.VerifyingContract
            };
        }

        private void SubscribeToEvents()
        {
            Service.SubscribeToEvent<SubscriptionAddedEventDTO>(SubscriptionAdded);
            Service.SubscribeToEvent<SubscriptionRenewedEventDTO>(SubscriptionRenewed);
            Service.SubscribeToEvent<SubscriptionExtendedEventDTO>(SubscriptionExtended);
            Service.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(SubscriptionPriceUpdated);
            Service.SubscribeToEvent<SubscriptionCancelledEventDTO>(SubscriptionCancelled);
            Service.SubscribeToEvent<SubscriptionRevokedEventDTO>(SubscriptionRevoked);
            Service.SubscribeToEvent<SubscriptionUnrevokedEventDTO>(SubscriptionUnrevoked);
            Service.SubscribeToEvent<SubscriptionRemovedEventDTO>(SubscriptionRemoved);
            Service.SubscribeToEvent<OwnershipTransferredEventDTO>(OwnershipTransferred);
        }

        private void AssertOwner()
        {
            EthereumAddress account = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;
            
            if (Owner != account)
            {
                throw new UnauthorizedAccessException(nameof(OwnableInvalidOwnerError));
            }
        }

        public async UniTask<DateTime> GetSubscriptionExpiration(string subscriberId)
        {
            byte[] subscriberHashBytes = subscriberId.ToSubscriberIdHash();
            
            BigInteger endTime = await Service.GetSubscriptionExpirationQueryAsync(subscriberHashBytes);

            return endTime.FromUnixTimeToLocalDateTime();
        }

        public async UniTask<bool> IsSubscriptionExpired(string subscriberId)
        {
            byte[]  subscriberHashBytes = subscriberId.ToSubscriberIdHash();

            return await Service.IsSubscriptionExpiredQueryAsync(subscriberHashBytes);
        }

        public async UniTask<bool> IsSubscriberRevoked(string subscriberId)
        {
            byte[]  subscriberHashBytes = subscriberId.ToSubscriberIdHash();

            return await Service.IsSubscriberRevokedQueryAsync(subscriberHashBytes);
        }

        public async UniTask<bool> IsSubscriptionActive(string subscriberId)
        {
            byte[]  subscriberHashBytes = subscriberId.ToSubscriberIdHash();

            return await Service.IsSubscriptionActiveQueryAsync(subscriberHashBytes);
        }

        public async UniTask<DateTime> Subscribe(string subscriberId, TimeSpan duration)
        {
            (Permit permit, TypedData<Domain> typedData) = await GetPermit(duration);

            EthECDSASignature signature =
                OpenCreatorRailsService.Instance.WalletProvider.SignTypedData(permit, typedData);

            byte[] subscriberHashBytes = subscriberId.ToSubscriberIdHash();

            TransactionReceipt receipt = await Service.SubscribeRequestAndWaitForReceiptAsync(subscriberHashBytes,
                permit.Owner, permit.Spender, permit.Value, permit.Deadline, signature.V[0], signature.R, signature.S);

            IEventDTO @event = receipt.DecodeAllEvents<SubscriptionExtendedEventDTO>().FirstOrDefault()?.Event ?? 
                               receipt.DecodeAllEvents<SubscriptionRenewedEventDTO>().FirstOrDefault()?.Event as IEventDTO ??
                               receipt.DecodeAllEvents<SubscriptionAddedEventDTO>()[0].Event;

            switch (@event)
            {
                case SubscriptionExtendedEventDTO subscriptionExtendedEventDto:
                    SubscriptionExtended(subscriptionExtendedEventDto);
                    return subscriptionExtendedEventDto.EndTime.FromUnixTimeToLocalDateTime();
                case SubscriptionRenewedEventDTO subscriptionRenewedEventDto:
                    SubscriptionRenewed(subscriptionRenewedEventDto);
                    return subscriptionRenewedEventDto.EndTime.FromUnixTimeToLocalDateTime();
                case SubscriptionAddedEventDTO subscriptionAddedEventDto:
                    SubscriptionAdded(subscriptionAddedEventDto);
                    return subscriptionAddedEventDto.EndTime.FromUnixTimeToLocalDateTime();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTask<(Permit permit, TypedData<Domain> typedData)> GetPermit(TimeSpan duration)
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
                Deadline = DateTimeOffset.UtcNow.ToUnixTimeSeconds() +
                           new BigInteger(TimeSpan.FromMinutes(30).TotalSeconds)
            };

            return (permit, _typedData);
        }

        public async UniTask CancelSubscription(string subscriberId)
        {
            BigInteger chainId = OpenCreatorRailsService.Instance.WalletProvider.ChainId;

            string address = Address.Value;

            string subscriberIdHash = subscriberId.ToSubscriberIdHash().ToHex(true);

            byte[] encoded = OpenCreatorRailsService.ABIEncode.GetABIEncodedPacked(new("uint256", chainId),
                new("address", address), new("bytes32", subscriberIdHash));

            byte[] hash = encoded.Keccack256();
            
            EthECDSASignature signature =
                OpenCreatorRailsService.Instance.WalletProvider.SignMessage(hash);
            
            var receipt = await Service.CancelSubscriptionRequestAndWaitForReceiptAsync(subscriberId, signature.To64ByteArray());

            if (!SubscriptionRemoved(receipt))
            {
                SubscriptionCancelledEventDTO @event = receipt.DecodeAllEvents<SubscriptionCancelledEventDTO>()[0].Event;
                
                SubscriptionCancelled(@event);
            }
        }
        
        public async UniTask SetSubscriptionPrice(BigInteger newSubscriptionPrice)
        {
            AssertOwner();
            
            var receipt = await Service.SetSubscriptionPriceRequestAndWaitForReceiptAsync(newSubscriptionPrice);

            SubscriptionPriceUpdatedEventDTO @event = receipt.DecodeAllEvents<SubscriptionPriceUpdatedEventDTO>()[0].Event;

            SubscriptionPriceUpdated(@event);
        }

        public async UniTask<BigInteger> ClaimCreatorFee(string subscriberId)
        {
            AssertOwner();

            byte[] subscriberHashBytes = subscriberId.ToSubscriberIdHash();
            
            var receipt = await Service.ClaimCreatorFeeRequestAndWaitForReceiptAsync(subscriberHashBytes);
            
            BigInteger amount =
                receipt.DecodeAllEvents<CreatorFeeClaimedEventDTO>()[0].Event.Amount;

            return amount;
        }
        
        public async UniTask<BigInteger> ClaimCreatorFee(string[] subscriberIds)
        {
            AssertOwner();

            List<byte[]> subscriberHashBytesList = subscriberIds.Select(subscriberId => subscriberId.ToSubscriberIdHash()).ToList();
            
            var receipt = await Service.ClaimCreatorFeeRequestAndWaitForReceiptAsync(subscriberHashBytesList);
            
            BigInteger amount =
                receipt.DecodeAllEvents<CreatorFeeClaimedBatchEventDTO>()[0].Event.TotalAmount;

            return amount;
        }

        public async UniTask RevokeSubscription(string subscriberId)
        {
            AssertOwner();
            
            byte[] subscriberHashBytes = subscriberId.ToSubscriberIdHash();
            
            var receipt = await Service.RevokeSubscriptionRequestAndWaitForReceiptAsync(subscriberHashBytes);

            if (!SubscriptionRemoved(receipt))
            {
                SubscriptionRevokedEventDTO @event = receipt.DecodeAllEvents<SubscriptionRevokedEventDTO>()[0].Event;
            
                SubscriptionRevoked(@event);
            }
        }

        public async UniTask UnrevokeSubscription(string subscriberId)
        {
            byte[] subscriberHashBytes = subscriberId.ToSubscriberIdHash();

            var receipt = await Service.UnrevokeSubscriptionRequestAndWaitForReceiptAsync(subscriberHashBytes);
            
            SubscriptionUnrevokedEventDTO @event = receipt.DecodeAllEvents<SubscriptionUnrevokedEventDTO>()[0].Event;
            
            SubscriptionUnrevoked(@event);
        }

        private bool SubscriptionRemoved(TransactionReceipt receipt)
        {
            SubscriptionRemovedEventDTO @event = receipt.DecodeAllEvents<SubscriptionRemovedEventDTO>().FirstOrDefault()?.Event;

            if (@event != null)
            {
                SubscriptionRemoved(@event);
                
                return true;
            }
            
            return false;
        }
        
        #region Event Delegates

        private void SubscriptionAdded(SubscriptionAddedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);
            DateTime startTime = @event.StartTime.FromUnixTimeToLocalDateTime();
            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();
            BigInteger subscriptionPrice = @event.SubscriptionPrice;
            BigInteger registryFeeShare = @event.RegistryFeeShare;
            EthereumAddress payer = new EthereumAddress(@event.Payer);
            
            // New subscriber
            if (!Subscriptions.Exists(subscription => subscription.SubscriberIdHash == subscriberIdHash))
            {
                Subscriptions.Add(new SubscriptionDto(subscriberIdHash, BigInteger.Zero, startTime, endTime, subscriptionPrice, registryFeeShare, payer, false, false, true));
            }
        }
        
        private void SubscriptionRenewed(SubscriptionRenewedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);
            BigInteger nonce = @event.Nonce;
            DateTime startTime = @event.StartTime.FromUnixTimeToLocalDateTime();
            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();
            BigInteger subscriptionPrice = @event.SubscriptionPrice;
            BigInteger registryFeeShare = @event.RegistryFeeShare;
            EthereumAddress payer = new EthereumAddress(@event.Payer);
            
            // New nonce
            if (!Subscriptions.Exists(subscription => subscription.SubscriberIdHash == subscriberIdHash && subscription.Nonce == nonce))
            {
                Subscriptions.Add(new SubscriptionDto(subscriberIdHash, nonce, startTime, endTime, subscriptionPrice, registryFeeShare, payer, false, false, true));
            }
        }

        private void SubscriptionExtended(SubscriptionExtendedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);
            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();

            BigInteger nonce = Subscriptions.Where(subscription => subscription.SubscriberIdHash == subscriberIdHash).Max(subscription => subscription.Nonce);

            int index = Subscriptions.FindIndex(subscription => subscription.SubscriberIdHash == subscriberIdHash && subscription.Nonce == nonce);
            
            if (index != - 1)
            {
                Subscriptions[index] = Subscriptions[index].Extended(endTime);
            }
        }

        private void SubscriptionPriceUpdated(SubscriptionPriceUpdatedEventDTO @event)
        {
            BigInteger newSubscriptionPrice = @event.NewSubscriptionPrice;
            
            if (SubscriptionPrice != newSubscriptionPrice)
            {
                SubscriptionPrice = newSubscriptionPrice;
            }
        }

        private void SubscriptionCancelled(SubscriptionCancelledEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);

            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();
            
            SubscriptionCancelledOrRevoked(subscriberIdHash, @event.Nonce, endTime);
        }

        private void SubscriptionRevoked(SubscriptionRevokedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);

            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();
            
            SubscriptionCancelledOrRevoked(subscriberIdHash, @event.Nonce, endTime);
            
            List<int> indexes = Subscriptions.Where(subscription => subscription.SubscriberIdHash == subscriberIdHash)
                .Select((_, index) => index).ToList();
            
            foreach (int index in indexes)
            {
                Subscriptions[index] = Subscriptions[index].Revoked();
            }
        }
        
        private void SubscriptionRemoved(SubscriptionRemovedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);

            Subscriptions.RemoveAll(subscription => subscription.SubscriberIdHash == subscriberIdHash);
        }

        private void SubscriptionUnrevoked(SubscriptionUnrevokedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);

            List<int> indexes = Subscriptions.Where(subscription => subscription.SubscriberIdHash == subscriberIdHash)
                .Select((_, index) => index).ToList();
            
            foreach (int index in indexes)
            {
                Subscriptions[index] = Subscriptions[index].Unrevoked();
            }
        }
        
        private void OwnershipTransferred(OwnershipTransferredEventDTO @event)
        {
            EthereumAddress newOwner = new EthereumAddress(@event.NewOwner);

            if (Owner != newOwner)
            {
                Owner = newOwner;
            }
        }

        private void SubscriptionCancelledOrRevoked(string subscriberIdHash, BigInteger nonce, DateTime endTime)
        {
            Subscriptions.RemoveAll(subscription => subscription.SubscriberIdHash == subscriberIdHash &&  subscription.Nonce > nonce);
            
            int index  = Subscriptions.FindIndex(subscription =>  subscription.SubscriberIdHash == subscriberIdHash  && subscription.Nonce == nonce);

            if (index != - 1)
            {
                Subscriptions[index] = Subscriptions[index].Shortened(endTime);
            }
        }

        #endregion
    }
}