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
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    /// <summary>
    /// MonoBehaviour implementation of <see cref="IAsset"/>. Add one instance to a GameObject in
    /// your scene for each on-chain asset you want to track, fill in its <c>Registry Address</c>
    /// and <c>Asset Id</c> fields in the Inspector, then drag it into the <b>Assets</b> list on
    /// <see cref="OpenCreatorRailsService"/>.
    /// <para>
    /// After <see cref="OpenCreatorRailsService.Connect"/> is called, the component fetches the
    /// asset's full state from the indexer, registers contract event listeners, and keeps the
    /// local <see cref="IAsset.Subscriptions"/> list in sync automatically.
    /// </para>
    /// </summary>
    public class Asset : MonoBehaviour, IAsset
    {
        [field: SerializeField] public EthereumAddress RegistryAddress { get; private set; }

        [field: SerializeField] public string AssetId { get; private set; }

        public string AssetIdHash => AssetId.Keccack256();

        public EthereumAddress Address { get; private set; }

        public BigInteger SubscriptionPrice { get; private set; }

        public TimeSpan SubscriptionDuration { get; private set; }

        public EthereumAddress Owner { get; private set; }

        public EthereumAddress TokenAddress { get; private set; }
        
        public string TokenSymbol { get; private set; }
        
        public BigInteger TokenDecimals { get; private set; }

        public List<SubscriptionDto> Subscriptions { get; private set; }

        public AssetService Service { get; private set; }

        public ERC20PermitService PermitService { get; private set; }

        public AssetRegistryService AssetRegistryService { get; private set; }

        private List<IAssetEventHandler> _assetEventHandlers;
        
        private Eip712DomainOutputDTO _domain;

        private TypedData<Domain> _typedData;

        public async UniTask InitializeAsync()
        {
            await Refresh();

            _assetEventHandlers = GetComponents<IAssetEventHandler>().ToList();
        }
        
        public async UniTask Connected(Web3 web3)
        {
            Service = new AssetService(web3, Address.Value);
            PermitService = new ERC20PermitService(web3, TokenAddress.Value);

            TokenSymbol = await PermitService.SymbolQueryAsync();
            TokenDecimals = await PermitService.DecimalsQueryAsync();
            
            AssetRegistryService = new AssetRegistryService(web3, RegistryAddress.Value);

            if (_domain == null)
            {
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
            
            SubscribeToEvents();
        }

        public async UniTask Refresh()
        {
            AssetDto assetDto =
                await OpenCreatorRailsService.Instance.IndexerProvider.GetAsset(AssetIdHash, RegistryAddress);

            Address = assetDto.Address;
            SubscriptionPrice = assetDto.SubscriptionPrice;
            SubscriptionDuration = assetDto.SubscriptionDuration;
            Owner = assetDto.Owner;
            TokenAddress = assetDto.TokenAddress;
            Subscriptions = assetDto.Subscriptions;
        }
        
        public UniTask Disconnected()
        {
            if (OpenCreatorRailsService.Instance.Connected)
            {
                UnubscribeToEvents();
            }
                
            return UniTask.CompletedTask;
        }

        private void SubscribeToEvents()
        {
            Service.SubscribeToEvent<SubscriptionAddedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionRenewedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionExtendedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionRemovedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionCancelledEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionRevokedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<SubscriptionUnrevokedEventDTO>(_assetEventHandlers);
            Service.SubscribeToEvent<OwnershipTransferredEventDTO>(_assetEventHandlers);
        }
        
        private void UnubscribeToEvents()
        {
            Service.UnsubscribeToEvent<SubscriptionAddedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionRenewedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionExtendedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionRemovedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionCancelledEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionRevokedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<SubscriptionUnrevokedEventDTO>(_assetEventHandlers);
            Service.UnsubscribeToEvent<OwnershipTransferredEventDTO>(_assetEventHandlers);
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

        public async UniTask<(BigInteger price, TimeSpan duration)> GetSubscriptionPriceAndDuration(BigInteger count)
        {
            var output = await Service.GetSubscriptionPriceAndDurationQueryAsync(count);

            return (output.Price, TimeSpan.FromSeconds((long)output.Duration));
        }

        public async UniTask<DateTime> Subscribe(string subscriberId, BigInteger count)
        {
            (Permit permit, TypedData<Domain> typedData) = await GetPermit(count);

            EthECDSASignature signature =
                OpenCreatorRailsService.Instance.WalletProvider.SignTypedData(permit, typedData);

            byte[] subscriberHashBytes = subscriberId.ToSubscriberIdHash();

            TransactionReceipt receipt = await Service.SubscribeRequestAndWaitForReceiptAsync(subscriberHashBytes,
                permit.Owner, permit.Spender, count, permit.Deadline, signature.V[0], signature.R, signature.S);

            IEventDTO @event = receipt.DecodeAllEvents<SubscriptionExtendedEventDTO>().FirstOrDefault()?.Event ?? 
                               receipt.DecodeAllEvents<SubscriptionRenewedEventDTO>().FirstOrDefault()?.Event as IEventDTO ??
                               receipt.DecodeAllEvents<SubscriptionAddedEventDTO>()[0].Event;

            switch (@event)
            {
                case SubscriptionExtendedEventDTO subscriptionExtendedEventDto:
                    _assetEventHandlers.HandleEvents(subscriptionExtendedEventDto);
                    return subscriptionExtendedEventDto.EndTime.FromUnixTimeToLocalDateTime();
                case SubscriptionRenewedEventDTO subscriptionRenewedEventDto:
                    _assetEventHandlers.HandleEvents(subscriptionRenewedEventDto);
                    return subscriptionRenewedEventDto.EndTime.FromUnixTimeToLocalDateTime();
                case SubscriptionAddedEventDTO subscriptionAddedEventDto:
                    _assetEventHandlers.HandleEvents(subscriptionAddedEventDto);
                    return subscriptionAddedEventDto.EndTime.FromUnixTimeToLocalDateTime();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async UniTask<(Permit permit, TypedData<Domain> typedData)> GetPermit(BigInteger count)
        {
            EthereumAddress payer = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;

            BigInteger value = await Service.GetSubscriptionPriceQueryAsync(count);

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

            byte[] subscriberIdHash = subscriberId.ToSubscriberIdHash();

            byte[] encoded = OpenCreatorRailsService.ABIEncode.GetABIEncodedPacked(new("uint256", chainId),
                new("address", address), new("bytes32", subscriberIdHash));

            byte[] hash = encoded.Keccack256();
            
            EthECDSASignature signature =
                OpenCreatorRailsService.Instance.WalletProvider.SignMessage(hash);

            byte[] signatureBytes = signature.R.Concat(signature.S).Concat(signature.V).ToArray();

            var receipt = await Service.CancelSubscriptionRequestAndWaitForReceiptAsync(subscriberId, signatureBytes);

            if (!SubscriptionRemoved(receipt))
            {
                receipt.DecodeAndHandleEvents<SubscriptionCancelledEventDTO>(_assetEventHandlers);
            }
        }
        
        public async UniTask SetSubscriptionPrice(BigInteger newSubscriptionPrice)
        {
            AssertOwner();
            
            var receipt = await Service.SetSubscriptionPriceRequestAndWaitForReceiptAsync(newSubscriptionPrice);

            receipt.DecodeAndHandleEvents<SubscriptionPriceUpdatedEventDTO>(_assetEventHandlers);
        }

        private async UniTask<BigInteger> ClaimCreatorFee(byte[] subscriberIdHash)
        {
            AssertOwner();

            var receipt = await Service.ClaimCreatorFeeRequestAndWaitForReceiptAsync(subscriberIdHash);

            var @event = receipt.DecodeAllEvents<CreatorFeeClaimedEventDTO>()[0].Event;
            
            _assetEventHandlers.HandleEvents(@event);
            
            return @event.Amount;
        }
        
        public UniTask<BigInteger> ClaimCreatorFee(string subscriberIdHash)
        {
            byte[] subscriberHashBytes = subscriberIdHash.HexToByteArray();
            
            return ClaimCreatorFee(subscriberHashBytes);
        }

        public UniTask<BigInteger> ClaimCreatorFee(string subscriberId, EthereumAddress subscriberAddress)
        {
            byte[] subscriberIdHash = subscriberId.ToSubscriberIdHash(subscriberAddress);
            
            return ClaimCreatorFee(subscriberIdHash);
        }

        private async UniTask<BigInteger> ClaimCreatorFee(List<byte[]> subscriberIdHashes)
        {
            AssertOwner();

            var receipt = await Service.ClaimCreatorFeeRequestAndWaitForReceiptAsync(subscriberIdHashes);
            
            var @event = receipt.DecodeAllEvents<CreatorFeeClaimedBatchEvent>()[0].Event;

            _assetEventHandlers.HandleEvents(@event);
            
            return @event.TotalAmount;
        }
        
        public UniTask<BigInteger> ClaimCreatorFee(string[] subscriberIdHashes)
        {
            List<byte[]> subscriberIdHashesBytesList =
                subscriberIdHashes.Select(subscriberIdHash => subscriberIdHash.HexToByteArray()).ToList();
            
            return ClaimCreatorFee(subscriberIdHashesBytesList);
        }

        public UniTask<BigInteger> ClaimCreatorFee((string subscriberId, EthereumAddress subscriberAddress)[] subscribers)
        {
            List<byte[]> subscriberIdHashes = subscribers.Select(subscriber =>
                    subscriber.subscriberId.ToSubscriberIdHash(subscriber.subscriberAddress))
                .ToList();

            return ClaimCreatorFee(subscriberIdHashes);
        }

        private async UniTask RevokeSubscription(byte[] subscriberIdHash)
        {
            AssertOwner();
            
            var receipt = await Service.RevokeSubscriptionRequestAndWaitForReceiptAsync(subscriberIdHash);

            if (!SubscriptionRemoved(receipt))
            {
                receipt.DecodeAndHandleEvents<SubscriptionRevokedEventDTO>(_assetEventHandlers);
            }
        }
        
        public UniTask RevokeSubscription(string subscriberIdHash)
        {
            byte[] subscriberIdHashBytes = subscriberIdHash.HexToByteArray();
            
            return RevokeSubscription(subscriberIdHashBytes);
        }

        public UniTask RevokeSubscription(string subscriberId, EthereumAddress subscriberAddress)
        {
            byte[] subscriberIdHash = subscriberId.ToSubscriberIdHash(subscriberAddress);
            
            return RevokeSubscription(subscriberIdHash);
        }

        private async UniTask UnrevokeSubscription(byte[] subscriberIdHash)
        {
            AssertOwner();
            
            var receipt = await Service.UnrevokeSubscriptionRequestAndWaitForReceiptAsync(subscriberIdHash);
            
            receipt.DecodeAndHandleEvents<SubscriptionUnrevokedEventDTO>(_assetEventHandlers);
        }
        
        public UniTask UnrevokeSubscription(string subscriberIdHash)
        {
            byte[] subscriberHashBytes = subscriberIdHash.HexToByteArray();

            return UnrevokeSubscription(subscriberHashBytes);
        }

        public UniTask UnrevokeSubscription(string subscriberId, EthereumAddress subscriberAddress)
        {
            byte[] subscriberIdHash = subscriberId.ToSubscriberIdHash(subscriberAddress);
            
            return UnrevokeSubscription(subscriberIdHash);
        }

        private bool SubscriptionRemoved(TransactionReceipt receipt)
        {
            SubscriptionRemovedEventDTO @event = receipt.DecodeAllEvents<SubscriptionRemovedEventDTO>().FirstOrDefault()?.Event;

            if (@event != null)
            {
                _assetEventHandlers.HandleEvents(@event);
                
                return true;
            }
            
            return false;
        }
        
        #region Event Delegates

        public void SubscriptionAdded(SubscriptionAddedEventDTO @event)
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
        
        public void SubscriptionRenewed(SubscriptionRenewedEventDTO @event)
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

        public void SubscriptionExtended(SubscriptionExtendedEventDTO @event)
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

        public void SubscriptionPriceUpdated(SubscriptionPriceUpdatedEventDTO @event)
        {
            BigInteger newSubscriptionPrice = @event.NewSubscriptionPrice;
            
            if (SubscriptionPrice != newSubscriptionPrice)
            {
                SubscriptionPrice = newSubscriptionPrice;
            }
        }

        public void SubscriptionCancelled(SubscriptionCancelledEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);

            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();
            
            SubscriptionCancelledOrRevoked(subscriberIdHash, @event.Nonce, endTime);
        }

        public void SubscriptionRevoked(SubscriptionRevokedEventDTO @event)
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
        
        public void SubscriptionRemoved(SubscriptionRemovedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex(true);

            Subscriptions.RemoveAll(subscription => subscription.SubscriberIdHash == subscriberIdHash);
        }

        public void SubscriptionUnrevoked(SubscriptionUnrevokedEventDTO @event)
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