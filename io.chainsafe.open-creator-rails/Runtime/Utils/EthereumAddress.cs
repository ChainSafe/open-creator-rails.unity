using System;
using Nethereum.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    [Serializable]
    public struct EthereumAddress : IEquatable<EthereumAddress>
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

        public static bool operator ==(EthereumAddress a, EthereumAddress b)
        {
            return a.Value == b.Value;
        }
        
        public static bool operator !=(EthereumAddress a, EthereumAddress b)
        {
            return a.Value != b.Value;
        }

        public override bool Equals(object obj)
        {
            return obj switch
            {
                EthereumAddress address => address == this,
                string str => str == Value,
                _ => false
            };
        }

        public bool Equals(EthereumAddress other)
        {
            return other == this;
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
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