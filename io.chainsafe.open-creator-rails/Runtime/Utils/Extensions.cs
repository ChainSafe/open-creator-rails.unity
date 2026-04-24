using System;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Newtonsoft.Json.Linq;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    public static class Extensions
    {
        public static string Keccack256(this string value)
        {
            return Sha3Keccack.Current.CalculateHash(value).EnsureHexPrefix();
        }
        
        public static byte[] Keccack256Bytes(this string value)
        {
            string hash = Keccack256(value);
            
            return hash.HexToByteArray();
        }
    }
}