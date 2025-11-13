using CobranzasDMSA_Odoo.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMSA.Models.Odoo.Native
{
    public class account_move_base
    {
        [PrimaryKey]
        public int id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        
        public string invoice_origin { get; set; }
        public DateTime invoice_date { get; set; }
        public DateTime invoice_date_due { get; set; }
        public string payment_state { get; set; }
        public string move_type { get; set; }
        public string l10n_ec_authorization_number { get; set; }

        public decimal amount_residual { get; set; }
        public decimal amount_untaxed_signed { get; set; }
        public decimal amount_total_signed { get; set; }
        public decimal amount_total { get; set; }
        public decimal amount_tax { get; set; }

        //public int _partner_id { get; set; }
        [Ignore]
        public Reverse_Entry_Ids[] reversed_entry_id { get; set; }
        [JsonProperty("ref")]
        public string _ref { get; set; }

        [JsonIgnore]
        public int _reversed_entry_id { get; set; }

        public string _refund_invoice_ids
        {
            get
            {
                return JsonConvert.SerializeObject(refund_invoice_ids);
            }
            set
            {
                value = JsonConvert.SerializeObject(refund_invoice_ids);
            }
        }

        [Ignore]
        public Refund_Invoice_Ids[] refund_invoice_ids { get; set; }

        public int _partner_id { get; set; }
        
        public int _journal_id { get; set; }
        
        public int _l10n_latam_document_type_id { get; set; }
        public int _invoice_user_id { get; set; }
        

        public int _printer_id { get; set; }
        

        public int _company_id { get; set; }

        public int _team_id { get; set; }
        
        [Ignore]
        [JsonConverter(typeof(PartnerListConverter))]
        public List<Partner_Id> PartnerIds { get; set; }
        [Ignore]
        public Journal_Id[] journal_id { get; set; }
        [Ignore]
        public L10n_Latam_Document_Type_Id[] l10n_latam_document_type_id { get; set; }
        [Ignore]
        public Invoice_User_Id[] invoice_user_id { get; set; }
        [Ignore]
        public res_company[] company_id { get; set; }
        [Ignore]
        public Team_Id[] team_id { get; set; }
        [Ignore]
        public Invoice_Line_Ids[] invoice_line_ids { get; set; }
        [Ignore]
        public Printer_Ids[] printer_id { get; set; }
        [JsonProperty("create_date")]
        [Column("create_date")]
        public DateTime? create_date { get; set; }


        [JsonProperty("write_date")]
        [Column("write_date")]
        public DateTime? write_date { get; set; }
    }

    public class PartnerListConverter : JsonConverter<List<Partner_Id>>
    {
        public override List<Partner_Id> ReadJson(JsonReader reader, Type objectType, List<Partner_Id> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var partners = new List<Partner_Id>();

            try
            {
                // Verificar si el token actual es un array
                if (reader.TokenType == JsonToken.StartArray)
                {
                    // Cargar el array JSON
                    JArray array = JArray.Load(reader);

                    // Iterar sobre cada elemento del array
                    foreach (var item in array)
                    {
                        if (item.Type == JTokenType.Array && item.Count() >= 2)
                        {
                            var partner = new Partner_Id
                            {
                                id = item[0].Type == JTokenType.Integer ? item[0].Value<int>() : 0,
                                name = item[1].Type == JTokenType.String ? item[1].Value<string>() : string.Empty
                            };

                            partners.Add(partner);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones, puedes registrar el error o manejarlo según tus necesidades
                Console.WriteLine($"Error al deserializar Partner List: {ex.Message}");
            }

            return partners;
        }

        public override void WriteJson(JsonWriter writer, List<Partner_Id> value, JsonSerializer serializer)
        {
            // Convertir una lista de Partner a un array de arrays
            var array = new JArray();

            foreach (var partner in value)
            {
                var partnerArray = new JArray
                {
                    partner.id,
                    partner.name
                };

                array.Add(partnerArray);
            }

            array.WriteTo(writer);
        }
    }


    //public class Partner_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Journal_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class L10n_Latam_Document_Type_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Invoice_User_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Team_Id
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Invoice_Line_Ids
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Reverse_Entry_Ids
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Refund_Invoice_Ids
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}

    //public class Printer_Ids
    //{
    //    public int id { get; set; }
    //    public string name { get; set; }
    //}
}
