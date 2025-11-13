using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Native
{
    [Table("res_partner")]
    public class res_partner : OdooEntity, INotifyPropertyChanged
    {
        [PrimaryKey]
        //[AutoIncrement]
        //[JsonIgnore]
        //public int id_sequence { get; set; }
        public int id { get; set; }
        [Ignore]
        public JToken company_id { get; set; }
        [JsonIgnore]
        public int _company_id
        {
            get => GetId(company_id);
            set => company_id = SetId(company_id, value);
        }
        [Column("vat")]
        public string vat { get; set; }
        [Column("vat_doc")]
        public string vat_doc { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public string email { get; set; }
        [Ignore]
        public JToken user_id { get; set; }
        [JsonIgnore]
        public int _user_id
        {
            get => GetId(user_id);
            set => user_id = SetId(user_id, value);
        }
        
        public string user_login { get; set; }
        public decimal total_due { get; set; }
        public decimal total_overdue { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
        public decimal total_invoiced { get; set; }

        public string street { get; set; }
        public string street2 { get; set; }

        //[JsonIgnore]
        //[Ignore]
        public decimal total_to_beat { get; set; }

        //[JsonIgnore]
        //[Ignore]
        public decimal positive_balance { get; set; }

        [Ignore]
        public JToken doc_type_identification_id { get; set; }
        [JsonIgnore]
        public int _doc_type_identification_id
        {
            get => GetId(doc_type_identification_id);
            set => doc_type_identification_id = SetId(doc_type_identification_id, value);
        }

        [Ignore]
        [JsonIgnore]
        public string doc_type_identification_name { get; set; }
        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }
        public DateTime? birthdate { get; set; }
        
        [Ignore]
        public JToken parent_id { get; set; }
        //public int commercial_parent_id { get; set; }
        [JsonIgnore]
        public int _parent_id
        {
            get => GetId(parent_id);
            set => parent_id = SetId(parent_id, value);
        }
        public string zip { get; set; }
        public string phone { get; set; }
        public string mobile { get; set; }

        [Ignore]
        public JToken city_id { get; set; }
        [JsonIgnore]
        public int _city_id
        {
            get => GetId(city_id);
            set => city_id = SetId(city_id, value);
        }
        
        public string city { get; set; }
        [Ignore]
        public JToken state_id { get; set; }
        [JsonIgnore]
        public int _state_id
        {
            get => GetId(state_id);
            set => state_id = SetId(state_id, value);
        }
        
        [Ignore]
        public JToken country_id { get; set; }
        [JsonIgnore]
        public int _country_id
        {
            get => GetId(country_id);
            set => country_id = SetId(country_id, value);
        }
        
        public string contact_address_complete { get; set; }
        public bool active { get; set; }

        [Ignore]
        [JsonIgnore]
        public string client_type_display
        {
            get => string.Concat(doc_type_identification_id, "-", doc_type_identification_name);
        }

        [Ignore]
        [JsonIgnore]
        public string title
        {
            get => string.Concat(id, " - ", name);
        }
                
        [JsonIgnore]
        private bool _isSelected;
        [Ignore]
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        [Ignore]
        [Column("product_pricelist_id")]
        public JToken product_pricelist_id { get; set; }

        [JsonIgnore]
        public int _product_pricelist_id
        {
            get => GetId(product_pricelist_id);
            set => product_pricelist_id = SetId(product_pricelist_id, value);
        }

        [Ignore]
        [Column("adic_comercial_id")]
        public JToken adic_comercial_id { get; set; }

        [JsonIgnore]
        public int _adic_comercial_id
        {
            get => GetId(adic_comercial_id);
            set => adic_comercial_id = SetId(adic_comercial_id, value);
        }

        [Ignore]
        [JsonIgnore]
        [Column("adic_comercial_secundarios_ids")]        
        public string adic_comercial_secundarios_ids { get; set; }
        [Column("adic_lunes")]
        public bool adic_lunes { get; set; }
        [Column("adic_martes")]
        public bool adic_martes { get; set; }
        [Column("adic_miercoles")]
        public bool adic_miercoles { get; set; }
        [Column("adic_jueves")]
        public bool adic_jueves { get; set; }
        [Column("adic_viernes")]
        public bool adic_viernes { get; set; }
        [Column("adic_sabado")]
        public bool adic_sabado { get; set; }
        [Column("adic_domingo")]
        public bool adic_domingo { get; set; }
        
        [Ignore]
        [Column("calificacion_crediticia_id")]
        public JToken calificacion_crediticia_id { get; set; }
        [JsonIgnore]
        public int _calificacion_crediticia_id
        {
            get => GetId(calificacion_crediticia_id);
            set => calificacion_crediticia_id = SetId(calificacion_crediticia_id, value);
        }
        public decimal facturacion_cupo_minimo { get; set; }
        
        public decimal facturacion_cupo_maximo { get; set; }
        
        public decimal facturacion_saldo_cupo { get; set; }
        
        public int facturacion_dias_credito { get; set; }
        
        public int facturacion_dias_credito_limite { get; set; }
        
        public string misc_comentarios { get; set; }

        [Column("is_salesman")]
        public bool is_salesman { get; set; }

        [Column("sale_available")]
        public bool sale_available { get; set; }



        [Ignore]
        [JsonIgnore]
        public string display_channel_name { get; set; }
        [Ignore]
        [JsonIgnore]
        public string display_seller_name { get; set; }

        [Ignore]
        [JsonIgnore]
        public string display_ranking_credit { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
