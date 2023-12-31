using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Substrate
{
    public class BigIntegerConverter : JsonConverter<BigInteger>
    {
        public override BigInteger Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return BigInteger.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, BigInteger value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}