using Newtonsoft.Json;

namespace Models.DMSA.Mbw.Abstract
{
    public class ArticuloDTO
    {
        [JsonProperty("product")]
        public ArticuloDTOProduct Product { get; set; }

        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("weight")]
        public double Weight { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("length")]
        public double Length { get; set; }

        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("show_web")]
        public bool ShowWeb { get; set; }

        [JsonProperty("show_store")]
        public bool ShowStore { get; set; }

        [JsonProperty("unidadpresentacion")]
        public string UnidadPresentacion { get; set; }

        [JsonProperty("prices")]
        public List<PrecioDTO> Prices { get; set; } = new List<PrecioDTO>();

        [JsonProperty("inventory")]
        public List<Inventory> InventoryList { get; set; } // = new List<Inventory>();
    }
}
