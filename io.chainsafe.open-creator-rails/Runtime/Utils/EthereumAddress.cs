using System;
using Nethereum.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    [Serializable]
    public struct EthereumAddress
    {
        [field: SerializeField] public string Value { get; private set; }

        public EthereumAddress(string address)
        {
            Value = address;

            AssertIsValid();
        }

        public void AssertIsValid()
        {
            if (!Value.IsValidEthereumAddressHexFormat())
            {
                Value = default;

                throw new InvalidEthereumAddressException(Value);
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public class InvalidEthereumAddressException : Exception
    {
        public string Address { get; private set; }

        public InvalidEthereumAddressException(string address) : base($"Invalid Ethereum Address : {address}")
        {
            Address = address;
        }
    }
}