using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;

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