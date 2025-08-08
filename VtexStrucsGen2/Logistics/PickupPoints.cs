using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VtexStrucsGen2.Logistics
{
    public class PickupPoints
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("instructions")]
        public string instructions { get; set; }

        [JsonProperty("formatted_address")]
        public string formatted_address { get; set; }

        [JsonProperty("address")]
        public address address { get; set; }

        [JsonProperty("isActive")]
        public bool isActive { get; set; }

        [JsonProperty("distance")]
        public double distance { get; set; }

        [JsonProperty("seller")]
        public string seller { get; set; }

        [JsonProperty("_sort")]
        public List<double> _sort { get; set; }

        [JsonProperty("businessHours")]
        public List<businessHour> businessHours { get; set; }

        [JsonProperty("tagsLabel")]
        public List<string> tagsLabel { get; set; }

        [JsonProperty("pickupHolidays")]
        public List<object> pickupHolidays { get; set; }

        [JsonProperty("isThirdPartyPickup")]
        public bool isThirdPartyPickup { get; set; }

        [JsonProperty("accountOwnerName")]
        public string accountOwnerName { get; set; }

        [JsonProperty("accountOwnerId")]
        public string accountOwnerId { get; set; }

        [JsonProperty("parentAccountName")]
        public string parentAccountName { get; set; }

        [JsonProperty("originalId")]
        public string originalId { get; set; }
    }

    public class address
    {
        [JsonProperty("postalCode")]
        public string postalCode { get; set; }

        [JsonProperty("country")]
        public country country { get; set; }

        [JsonProperty("city")]
        public string city { get; set; }

        [JsonProperty("state")]
        public string state { get; set; }

        [JsonProperty("neighborhood")]
        public string neighborhood { get; set; }

        [JsonProperty("street")]
        public string street { get; set; }

        [JsonProperty("number")]
        public string number { get; set; }

        [JsonProperty("complement")]
        public string complement { get; set; }

        [JsonProperty("reference")]
        public string reference { get; set; }

        [JsonProperty("location")]
        public location location { get; set; }
    }

    public class country
    {
        [JsonProperty("acronym")]
        public string acronym { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }
    }

    public class location
    {
        [JsonProperty("latitude")]
        public double latitude { get; set; }

        [JsonProperty("longitude")]
        public double longitude { get; set; }
    }

    public class businessHour
    {
        [JsonProperty("dayOfWeek")]
        public int dayOfWeek { get; set; }

        [JsonProperty("openingTime")]
        public string openingTime { get; set; }

        [JsonProperty("closingTime")]
        public string closingTime { get; set; }
    }

}
