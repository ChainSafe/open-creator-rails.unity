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
using Nethereum.ABI;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.EIP712.EIP2612;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.Web3;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails
{
    public class Asset : MonoBehaviour
    {
        [SerializeField] private EthereumAddress _registryAddress;

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
                await OpenCreatorRailsService.Instance.IndexerProvider.GetAsset(AssetIdHash, _registryAddress);

            Address = assetDto.Address;
            SubscriptionPrice = assetDto.SubscriptionPrice;
            Owner = assetDto.Owner;
            TokenAddress = assetDto.TokenAddress;
            Subscriptions = assetDto.Subscriptions;

            Service = new AssetService(web3, Address.Value);
            PermitService = new ERC20PermitService(web3, TokenAddress.Value);
            AssetRegistryService = new AssetRegistryService(web3, _registryAddress.Value);

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
            Service.SubscribeToEvent<SubscriptionExtendedEventDTO>(SubscriptionExtended);
            Service.SubscribeToEvent<SubscriptionPriceUpdatedEventDTO>(SubscriptionPriceUpdated);
            Service.SubscribeToEvent<SubscriptionCancelledEventDTO>(SubscriptionCancelled);
            Service.SubscribeToEvent<SubscriptionRevokedEventDTO>(SubscriptionRevoked);
            Service.SubscribeToEvent<OwnershipTransferredEventDTO>(OwnershipTransferred);
        }

        public bool IsOwner()
        {
            return Owner == OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;
        }

        public async UniTask<bool> HasAccess(string subscriberId)
        {
            return await Service.IsSubscriptionActiveQueryAsync(subscriberId.Keccack256Bytes());
        }

        public async UniTask<DateTime> Subscribe(string subscriberId, TimeSpan duration)
        {
            (Permit permit, TypedData<Domain> typedData) = await GetPermit(duration);

            EthECDSASignature signature =
                OpenCreatorRailsService.Instance.WalletProvider.SignTypedData(permit, typedData);

            byte[] subscriberHashBytes = GetSubscriberIdHash(subscriberId);

            TransactionReceipt receipt = await Service.SubscribeRequestAndWaitForReceiptAsync(subscriberHashBytes,
                permit.Owner, permit.Spender, permit.Value, permit.Deadline, signature.V[0], signature.R, signature.S);

            BigInteger? endTime =
                receipt.DecodeAllEvents<SubscriptionExtendedEventDTO>().FirstOrDefault()?.Event.EndTime ??
                receipt.DecodeAllEvents<SubscriptionAddedEventDTO>()[0].Event.EndTime;

            return endTime.Value.FromUnixTimeToLocalDateTime();
        }

        // TODO
        // GetSubscription(subscriberId)
        // CancelSubscription(subscriberId)
        // Subscribe 

        // Owner
        // SetSubscriptionPrice
        // ClaimCreatorFee()
        // ClaimCreatorFee batch
        // RevokeSubscription(subscriberId)

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

        private byte[] GetSubscriberIdHash(string subscriberId)
        {
            EthereumAddress account = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;

            return new ABIValue[] { new ABIValue("string", subscriberId), new ABIValue("address", account.Value) }
                .GetABIEncoded()
                .Keccack256();
        }

        #region Event Delegates

        private void SubscriptionAdded(SubscriptionAddedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex();
            EthereumAddress payer = new EthereumAddress(@event.Payer);
            DateTime startTime = @event.StartTime.FromUnixTimeToLocalDateTime();
            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();
            BigInteger nonce = @event.Nonce;

            Subscriptions.Add(new SubscriptionDto(subscriberIdHash, payer, startTime, endTime, true, nonce));
        }

        private void SubscriptionExtended(SubscriptionExtendedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex();
            DateTime endTime = @event.EndTime.FromUnixTimeToLocalDateTime();

            int index = Subscriptions.FindIndex(subscription => subscription.SubscriberIdHash == subscriberIdHash);

            Subscriptions[index] = Subscriptions[index].Extended(endTime);
        }

        private void SubscriptionPriceUpdated(SubscriptionPriceUpdatedEventDTO @event)
        {
            SubscriptionPrice = @event.NewSubscriptionPrice;
        }

        private void SubscriptionCancelled(SubscriptionCancelledEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex();

            SubscriptionDeactivated(subscriberIdHash);
        }

        private void SubscriptionRevoked(SubscriptionRevokedEventDTO @event)
        {
            string subscriberIdHash = @event.Subscriber.ToHex();

            SubscriptionDeactivated(subscriberIdHash);
        }

        private void OwnershipTransferred(OwnershipTransferredEventDTO @event)
        {
            Owner = new EthereumAddress(@event.NewOwner);
        }

        private void SubscriptionDeactivated(string subscriberIdHash)
        {
            int index = Subscriptions.FindIndex(subscription => subscription.SubscriberIdHash == subscriberIdHash);

            Subscriptions[index] = Subscriptions[index].Deactivated();
        }

        #endregion
    }
}