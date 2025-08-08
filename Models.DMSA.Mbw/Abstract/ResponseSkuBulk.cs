using Newtonsoft.Json;

namespace Models.DMSA.Mbw.Abstract
{
    public class ResponseSkuBulk
    {
        [JsonProperty("inventory")]
        public Inventory_resp[]? inventory { get; set; }
        public string[]? non_field_errors { get; set; }
        public string? external_id { get; set; }
        public int? status_code { get; set; }
    }

    public class Inventory_resp
    {
        [JsonProperty("warehouse")]
        public Warehouse_resp warehouse { get; set; }
    }

    public class Warehouse_resp
    {
        [JsonProperty("name")]
        public string[]? name { get; set; }
    }
}
