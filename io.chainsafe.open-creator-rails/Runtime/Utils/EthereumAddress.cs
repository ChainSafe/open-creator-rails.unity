using System;
using Nethereum.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    /// <summary>
    /// A validated Ethereum address value type. Wraps a hex address string and
    /// throws <see cref="InvalidEthereumAddressException"/> on construction if the value is
    /// not a valid Ethereum address. Serializable in the Unity Inspector via <see cref="SerializeField"/>.
    /// </summary>
    [Serializable]
    public struct EthereumAddress : IEquatable<EthereumAddress>
    {
        [field: SerializeField] public string Value { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="EthereumAddress"/> with the given hex address string
        /// and immediately validates its format.
        /// </summary>
        /// <param name="address">A hex Ethereum address string.</param>
        /// <exception cref="InvalidEthereumAddressException">
        /// Thrown if <paramref name="address"/> is not a valid Ethereum address hex format.
        /// </exception>
        public EthereumAddress(string address)
        {
            Value = address;

            AssertIsValid();
        }

        private void AssertIsValid()
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