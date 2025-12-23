using DMSA.Models.Odoo.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{
    public class BoolOrIntConverter : JsonConverter<int?>
    {
        public override int? ReadJson(JsonReader reader, Type objectType, int? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Boolean)
                return null;

            if (reader.TokenType == JsonToken.Integer)
                return Convert.ToInt32(reader.Value);

            return null;
        }

        public override void WriteJson(JsonWriter writer, int? value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }


    public class ApiResponseOdooRpcV2
    {
        public string jsonrpc { get; set; }
        public int id { get; set; }

        [JsonConverter(typeof(BoolOrIntConverter))]
        public int? result { get; set; }

        public Error? error { get; set; }
    }
}
