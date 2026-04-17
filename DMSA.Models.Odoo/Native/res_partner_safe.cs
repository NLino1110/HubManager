using DMSA.Models.Odoo.Base;
using Newtonsoft.Json;
using SQLite;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DMSA.Models.Odoo.Native
{
    [Table("res_partner_safe")]
    public class res_partner_safe : OdooEntity, INotifyPropertyChanged
    {
        public int id { get; set; }

        // Identification mapped from ClienteAprobacion (IDENTIFICACION)
        [Column("vat_doc")]
        public string? vat_doc { get; set; }

        // Standard res.partner fields
        [Column("vat")]
        public string? vat { get; set; }
        public string? name { get; set; }
        public string? display_name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? mobile { get; set; }
        public string? street { get; set; }
        public string? street2 { get; set; }
        public string? zip { get; set; }
        public string? city { get; set; }

        [Column("create_date")]
        public DateTime? create_date { get; set; }

        [Column("write_date")]
        public DateTime? write_date { get; set; }

        // Fields from ClienteAprobacion renamed to standard names
        [Column("order_number")]
        public int? order_number { get; set; } // NUMPEDIDO

        [Column("agency_code")]
        public int? agency_code { get; set; } // CODAGENCIA

        [Column("client_code")]
        public int? client_code { get; set; } // CODCLIENTE

        [Column("company_code")]
        public string? company_code { get; set; } // CODEMPRESA

        [Column("seller_code")]
        public string? seller_code { get; set; } // CODVENDEDOR

        [Column("user_code")]
        public string? user_code { get; set; } // CODUSUARIO

        [Column("application_origin")]
        public string? application_origin { get; set; } // APLICACIONORIGEN

        [Column("application_version")]
        public string? application_version { get; set; } // APLICACIONVERSION

        [Column("platform_origin")]
        public string? platform_origin { get; set; } // PLATAFORMAORIGEN

        [Column("platform_modified")]
        public string? platform_modified { get; set; } // PLATAFORMAMODIFICA

        [Column("device_brand")]
        public string? device_brand { get; set; } // MARCAEQUIPO

        [Column("device_model")]
        public string? device_model { get; set; } // MODELOEQUIPO

        [Column("state_code")]
        public int? state_code { get; set; } // CODESTADO

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
