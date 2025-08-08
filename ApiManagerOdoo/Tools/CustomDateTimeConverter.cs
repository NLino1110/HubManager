using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiManagerOdoo.Tools
{
    using Newtonsoft.Json;
    using System;

    public class CustomDateTimeConverter : JsonConverter
    {
        private readonly string _dateFormat;

        public CustomDateTimeConverter(string dateFormat)
        {
            _dateFormat = dateFormat;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime dateTime)
            {
                writer.WriteValue(dateTime.ToString(_dateFormat));
            }
            else
            {
                throw new JsonSerializationException("Expected DateTime object value.");
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return DateTime.Parse((string) reader.Value);
            }
            throw new JsonSerializationException("Expected string token.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
        }
    }
}
