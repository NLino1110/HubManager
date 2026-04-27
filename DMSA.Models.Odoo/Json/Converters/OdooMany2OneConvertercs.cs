using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMSA.Models.Odoo.Json.Converters
{
    public class OdooMany2OneConverter : JsonConverter<OdooMany2One>
    {
        public override OdooMany2One? ReadJson(JsonReader reader, Type objectType, OdooMany2One? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer)
            {
                return new OdooMany2One
                {
                    Id = Convert.ToInt32(reader.Value)
                };
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                var arr = JArray.Load(reader);

                return new OdooMany2One
                {
                    Id = arr.Count > 0 ? (int?)arr[0] : null,
                    Name = arr.Count > 1 ? arr[1]?.ToString() : null
                };
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, OdooMany2One? value, JsonSerializer serializer)
        {
            // Para Odoo SOLO enviamos el ID
            writer.WriteValue(value?.Id);
        }
    }
}
