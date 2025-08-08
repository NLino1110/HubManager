using Newtonsoft.Json;

namespace ApiTradeHub.Services.Sales.Models
{
    public class Inventory
    {
        [JsonProperty("warehouse")]
        public Warehouse Warehouse { get; set; }

        [JsonProperty("stock")]
        public double Stock { get; set; }

        [JsonProperty("reserved")]
        public double Reserved { get; set; }
    }
}
