using Newtonsoft.Json;

namespace Models.DMSA.Mbw.Abstract
{
    public class Inventory
    {
        [JsonProperty("warehouse")]
        public Warehouse Warehouse { get; set; }

        [JsonProperty("stock")]
        public double Stock { get; set; }

        [JsonProperty("reserved")]
        public double Reserved { get; set; }

        [JsonProperty("fechaultingreso")]
        public DateTime? fechaultingreso { get; set; }
        [JsonProperty("fechaultegreso")]
        public DateTime? fechaultegreso { get; set; }
            
    }
}
