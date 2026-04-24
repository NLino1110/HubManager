using SQLite;

namespace DMSA.Models.Odoo.Customers
{
    public class CustomerDataConsentResponse
    {
        public int CustomerId { get; set; }

        public bool IsConsentGiven { get; set; }

        public DateTime CreateDate { get; set; }

        public string ConsentType { get; set; }
        
        public string Source { get; set; }
                
        public string PolicyVersion { get; set; }

        public string IpAddress { get; set; }

        public string RecordedBy { get; set; }

        public string Email { get; set; }
        [Column("doc_type_identification_id")]
        public string doc_type_identification_id { get; set; }
        [Column("vat_doc")]
        public string vat_doc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime WriteDate { get; set; }
        public string ApplicationOrigin { get; set; }
        public string device_app_version { get; set; }
        public string device_platform_mod { get; set; }
        public string device_platform_source { get; set; }
        public string device_manufacturer { get; set; }
        public string device_model { get; set; }
        public int CODESTADO { get; set; }
    }
}
