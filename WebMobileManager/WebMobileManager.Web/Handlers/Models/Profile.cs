namespace WebMobileManager.Web.Handlers.Models
{
    public class Profile
    {
        //public string SignFiles { get; set; }
        public string? ApiTradeHub { get; set; }
        public string ApiBaseAddress { get; set; }
        public string PublishStore { get; set; }
        //public string ResourcesPath { get; set; }
        //public string AmbienteDestino { get; set; }
        //public Erpnext ErpNext { get; set; }        
        public _EmailSettings EmailSettings { get; set; }
        public Dataservers[] DataServers { get; set; }
        //public _Tasks[] Tasks { get; set; }
        public _Tasks_Struct[] Tasks { get; set; }

        public _odoo Odoo { get; set; }
    }
}
