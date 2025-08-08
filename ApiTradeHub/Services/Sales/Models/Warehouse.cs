using Newtonsoft.Json;

namespace ApiTradeHub.Services.Sales.Models
{
    public class Warehouse
    {
        [JsonProperty("external_id")]
        public string? ExternalId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }
    
    //public class InventoryResponse
    //{
    //    [JsonProperty("inventory")]
    //    public List<Inventory> InventoryList { get; set; } = new List<Inventory>();
    //}

    public class ParametersMode1
    {
        public List<object> articulos { get; set; }
        public List<object> marcas { get; set; }  // Puede contener enteros y cadenas
    }

    public class ArticuloDTOProduct
    {
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("reference")]
        public string Reference { get; set; }
    }    

    public class PrecioDTO
    {
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public double Value { get; set; }


        [JsonProperty("store")]
        public Store Store { get; set; }
        [JsonProperty("minimun")]
        public double? Minimum { get; set; }
        [JsonProperty("start")]
        public string? Start { get; set; }
        [JsonProperty("end")]
        public string? End { get; set; }
        [JsonProperty("status")]
        public bool? Status { get; set; }
    }

    public class Store
    {
        [JsonProperty("external_id")]
        public long ExternalId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("ecommerce")]
        public bool Ecommerce { get; set; }
    }

    //public class Price
    //{
    //    [JsonProperty("external_id")]
    //    public long ExternalId { get; set; }
    //    public Store Store { get; set; }
    //    public string Type { get; set; }
    //    public double Minimum { get; set; }
    //    public double Value { get; set; }
    //    public string Start { get; set; }
    //    public string End { get; set; }
    //    public bool Status { get; set; }
    //}

    public class Article
    {
        [JsonProperty("external_id")]
        public long ExternalId { get; set; }
        public List<PrecioDTO> Prices { get; set; } = new List<PrecioDTO>();
    }

    public class DiscountPayload
    {
        [JsonProperty("articles")]
        public List<ArticuloDTO> Articles { get; set; } = new List<ArticuloDTO>();
    }
}
