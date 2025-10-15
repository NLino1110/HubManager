using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DMSA.Models.Odoo.Json.Converters
{
    public class OdooNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            if (reader.TokenType == JsonToken.Boolean) return null; // false => null

            if (reader.TokenType == JsonToken.String)
            {
                var s = (string)reader.Value;
                if (string.IsNullOrWhiteSpace(s) || s.Equals("false", StringComparison.OrdinalIgnoreCase))
                    return null;

                // Odoo suele dar "yyyy-MM-dd HH:mm:ss"
                if (DateTime.TryParseExact(s,
                                           new[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" },
                                           CultureInfo.InvariantCulture,
                                           DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                           out var dt))
                    return dt;

                // Intento genérico como fallback
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                                      DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
                    return dt;

                throw new JsonSerializationException($"No se pudo parsear la fecha: '{s}'");
            }

            throw new JsonSerializationException($"Token inesperado para DateTime?: {reader.TokenType}");
        }

        public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
        {
            if (value.HasValue)
                writer.WriteValue(value.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            else
                writer.WriteNull();
        }
    }

}
