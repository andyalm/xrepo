using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace XRepo.Core.Json
{
    public class ExplicitJsonConverter<T> : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var typeDescriptor = GetTypeDescriptor(typeToConvert);
            var instance = (T)Activator.CreateInstance(typeToConvert, nonPublic:true);
            ExplicitPropertyDescriptor currentProperty = null;
            while (reader.TokenType != JsonTokenType.EndObject)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        if (!typeDescriptor.Properties.TryGetValue(reader.GetString(), out currentProperty))
                        {
                            reader.Skip();
                        }
                        break;
                    case JsonTokenType.String:
                        var stringValue = reader.GetString();
                        object convertedValue = stringValue;
                        if (currentProperty.PropertyType == typeof(DateTime))
                        {
                            convertedValue = DateTime.Parse(stringValue);
                        }
                        else if(currentProperty.PropertyType == typeof(DateTimeOffset))
                        {
                            convertedValue = DateTimeOffset.Parse(stringValue);
                        }
                        currentProperty.SetValue(instance, convertedValue);
                        break;
                    case JsonTokenType.Number:
                        var numberValue = reader.GetNumberAs(currentProperty.PropertyType);
                        currentProperty.SetValue(instance, numberValue);
                        break;
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        var boolValue = reader.GetBoolean();
                        currentProperty.SetValue(instance, boolValue);
                        break;
                    case JsonTokenType.StartObject:
                        if (currentProperty != null)
                        {
                            var oValue = JsonSerializer.Deserialize(ref reader, currentProperty.PropertyType, options);
                            currentProperty.SetValue(instance, oValue);
                        }
                        break;
                    case JsonTokenType.StartArray:
                        if (!currentProperty.IsEnumerable)
                        {
                            throw new InvalidOperationException($"The property '{currentProperty.Name}' does not support arrays");
                        }

                        var aValue = JsonSerializer.Deserialize(ref reader, currentProperty.PropertyType, options);
                        currentProperty.SetValue(instance, aValue);
                        break;
                }
                reader.Read();
            }

            return instance;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            var typeDescriptor = GetTypeDescriptor(value.GetType());
            writer.WriteStartObject();
            foreach (var propertyDescriptor in typeDescriptor.Properties.Values)
            {
                var propertyValue = propertyDescriptor.GetValue(value);
                writer.WriteProperty(propertyDescriptor.Name, propertyValue, options);
            }
            writer.WriteEndObject();
        }

        private ExplicitTypeDescriptor GetTypeDescriptor(Type type)
        {
            return _typeDescriptorCache.GetOrAdd(type, _ => ExplicitTypeDescriptor.FromType(type));
        }
        
        private static ConcurrentDictionary<Type, ExplicitTypeDescriptor> _typeDescriptorCache = new ConcurrentDictionary<Type, ExplicitTypeDescriptor>();
    }
}