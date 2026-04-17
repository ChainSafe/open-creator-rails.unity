using System;
using Nethereum.Util;
using Newtonsoft.Json;
using UnityEngine;

namespace Io.ChainSafe.OpenCreatorRails.Utils
{
    [Serializable]
    [JsonConverter(typeof(EthereumAddressJsonConverter))]
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

    public class EthereumAddressJsonConverter : JsonConverter<EthereumAddress>
    {
        public override void WriteJson(JsonWriter writer, EthereumAddress value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override EthereumAddress ReadJson(JsonReader reader, Type objectType, EthereumAddress existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            string value;

            switch (reader.TokenType)
            {
                case JsonToken.String:
                    value = (string)reader.Value;
                    break;
                case JsonToken.Null:
                    value = null;
                    break;
                default:
                    throw new InvalidCastException();
            }

            return new EthereumAddress(value);
        }
    }
}