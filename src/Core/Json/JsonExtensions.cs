using System;
using System.Text.Json;

namespace XRepo.Core.Json
{
    internal static class JsonExtensions
    {
        public static void WriteProperty(this Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
        {
            if (options.IgnoreNullValues && value == null)
            {
                return;
            }
            
            writer.WritePropertyName(propertyName);
            switch (value)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case int i:
                    writer.WriteNumberValue(i);
                    break;
                case long l:
                    writer.WriteNumberValue(l);
                    break;
                case string s:
                    writer.WriteStringValue(s);
                    break;
                case bool b:
                    writer.WriteBooleanValue(b);
                    break;
                default:
                    JsonSerializer.Serialize(writer, value, value.GetType(), options);
                    break;
            }
        }

        public static object GetNumberAs(this Utf8JsonReader reader, Type targetNumberType)
        {
            if (targetNumberType == typeof(long))
            {
                return reader.GetInt64();
            }
            else if (targetNumberType == typeof(int))
            {
                return reader.GetInt32();
            }
            else
            {
                throw new InvalidOperationException($"Reading type '{targetNumberType.FullName}' as a number not currently supported");
            }
        }
    }
}