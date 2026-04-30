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
        public static byte[] Keccack256(this byte[] value)
        {
            return Sha3Keccack.Current.CalculateHash(value);
        }
        
        public static string Keccack256(this string value)
        {
            return Sha3Keccack.Current.CalculateHash(value).EnsureHexPrefix();
        }
        
        public static byte[] Keccack256Bytes(this string value)
        {
            string hash = Keccack256(value);
            
            return hash.HexToByteArray();
        }

        public static byte[] GetABIEncoded(this ABIValue[] values)
        {
            return OpenCreatorRailsService.ABIEncode.GetABIEncoded(values);
        }

        public static void SubscribeToEvent<T>(this ContractWeb3ServiceBase service, EventDelegate<T> @delegate) where T : IEventDTO, new()
        {
            OpenCreatorRailsService.Instance.EventHandler.Subscribe(new EthereumAddress(service.ContractAddress), service.Web3, @delegate);
        }

        public static DateTime FromUnixTimeToLocalDateTime(this BigInteger unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds((long) unixTime).DateTime.ToLocalTime();
        }
        
        public static UniTask ForEachAsync<T>(
            this IEnumerable<T> collection,
            Func<T, UniTask> action)
        {
            return UniTask.WhenAll(collection.Select(action));
        }
    }
}