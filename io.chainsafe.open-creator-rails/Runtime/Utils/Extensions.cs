using System;
using System.Collections.Generic;
using System.Numerics;
using Cysharp.Threading.Tasks;
using Nethereum.ABI;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Nethereum.Web3;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    public static class Extensions
    {
        /// <summary>Computes the Keccak-256 hash of a byte array.</summary>
        /// <param name="value">The input bytes to hash.</param>
        /// <returns>The 32-byte Keccak-256 hash.</returns>
        public static byte[] Keccack256(this byte[] value)
        {
            return Sha3Keccack.Current.CalculateHash(value);
        }

        /// <summary>Computes the Keccak-256 hash of a UTF-8 string and returns it as a <c>0x</c>-prefixed hex string.</summary>
        /// <param name="value">The input string to hash.</param>
        /// <returns>A <c>0x</c>-prefixed hex string of the 32-byte Keccak-256 hash.</returns>
        public static string Keccack256(this string value)
        {
            return Sha3Keccack.Current.CalculateHash(value).EnsureHexPrefix();
        }

        /// <summary>ABI-encodes an array of <see cref="ABIValue"/> entries.</summary>
        /// <param name="values">The ABI values to encode.</param>
        /// <returns>The ABI-encoded byte array.</returns>
        public static byte[] GetABIEncoded(this ABIValue[] values)
        {
            return OpenCreatorRailsService.ABIEncode.GetABIEncoded(values);
        }

        /// <summary>
        /// Computes the on-chain subscriber identity hash for the given subscriber ID and the
        /// currently connected wallet address: <c>keccak256(abi.encode(subscriberId, connectedAccount))</c>.
        /// This is the <c>bytes32</c> value passed to all subscriber-facing contract functions.
        /// </summary>
        /// <param name="subscriberId">The plain-text subscriber identity string.</param>
        /// <returns>The 32-byte subscriber identity hash.</returns>
        public static byte[] ToSubscriberIdHash(this string subscriberId)
        {
            EthereumAddress account = OpenCreatorRailsService.Instance.WalletProvider.ConnectedAccount;

            return subscriberId.ToSubscriberIdHash(account);
        }

        /// <summary>
        /// Registers an <see cref="EventDelegate{T}"/> listener for
        /// events emitted by the contract bound to <paramref name="service"/> via
        /// <see cref="IEventHandler.Subscribe{T}"/>.
        /// </summary>
        /// <typeparam name="T">The Nethereum event DTO type to listen for, implements <see cref="IEventDTO"/>.</typeparam>
        /// <param name="service">The contract service to subscribe events to.</param>
        /// <param name="delegate">Callback invoked for the event occurrence.</param>
        public static void SubscribeToEvent<T>(this ContractWeb3ServiceBase service, EventDelegate<T> @delegate) where T : IEventDTO, new()
        {
            OpenCreatorRailsService.Instance.EventHandler.Subscribe(new EthereumAddress(service.ContractAddress), service.Web3, @delegate);
        }
        
        /// <summary>
        /// Removes a previously registered <see cref="EventDelegate{T}"/> listener for events
        /// emitted by the contract bound to <paramref name="service"/> via
        /// <see cref="IEventHandler.Unsubscribe{T}"/>.
        /// </summary>
        /// <typeparam name="T">The Nethereum event DTO type to unsubscribe from, implements <see cref="IEventDTO"/>.</typeparam>
        /// <param name="service">The contract service whose address and <see cref="IWeb3"/> instance are used.</param>
        /// <param name="delegate">The callback to remove; must be the same delegate instance passed to <see cref="SubscribeToEvent{T}"/>.</param>
        public static void UnsubscribeToEvent<T>(this ContractWeb3ServiceBase service, EventDelegate<T> @delegate) where T : IEventDTO, new()
        {
            OpenCreatorRailsService.Instance.EventHandler.Unsubscribe(new EthereumAddress(service.ContractAddress), service.Web3, @delegate);
        }

        /// <summary>Converts a Unix timestamp (seconds since epoch) to a local <see cref="DateTime"/>.</summary>
        /// <param name="unixTime">Unix timestamp in seconds.</param>
        /// <returns>The equivalent local <see cref="DateTime"/>.</returns>
        public static DateTime FromUnixTimeToLocalDateTime(this BigInteger unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds((long) unixTime).DateTime.ToLocalTime();
        }

        /// <summary>
        /// Runs an async action on every element of a collection concurrently and returns a
        /// <see cref="UniTask"/> that completes when all actions have finished.
        /// </summary>
        /// <typeparam name="T">The element type of the collection.</typeparam>
        /// <param name="collection">The collection to iterate.</param>
        /// <param name="action">The async action to invoke for each element.</param>
        /// <returns>A <see cref="UniTask"/> that completes when all invocations have finished.</returns>
        public static UniTask ForEachAsync<T>(
            this IEnumerable<T> collection,
            Func<T, UniTask> action)
        {
            return UniTask.WhenAll(collection.Select(action));
        }

        /// <summary>
        /// Computes the on-chain subscriber identity hash for the given subscriber ID and an
        /// explicit wallet address: <c>keccak256(abi.encode(subscriberId, address))</c>.
        /// </summary>
        /// <param name="subscriberId">The plain-text subscriber identity string.</param>
        /// <param name="address">The wallet address to bind the subscriber identity to.</param>
        /// <returns>The 32-byte subscriber identity hash.</returns>
        public static byte[] ToSubscriberIdHash(this string subscriberId, EthereumAddress address)
        {
            return new ABIValue[] { new ("string", subscriberId), new ("address", address.Value) }
                .GetABIEncoded()
                .Keccack256();
        }
        
        public static decimal PowerOfTen(this BigInteger exponent)
        {
            decimal result = 1m;
            for (int i = 0; i < exponent; i++) result *= 10m;
            return result;
        }
    }
}