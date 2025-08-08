using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VtexStrucs.Comm
{
    public class IVtexObject
    {
        //public string accountName { get; set; }
        //public string environment { get; set; }
        public string baseUrl { get; set; }
        public Dictionary<string, string> headers { get; set; }
        public vTex_Account account { get; set; }

        public bool SetAccount(vTex_Account accountParam)
        {
            account = accountParam;            
            
            if(headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            baseUrl = $"{account.Account_Name}.{account.Environment}";
            headers.Add("Content-Type", "application/json");
            headers.Add("Accept", "application/json");
            headers.Add("X-VTEX-API-AppToken", account.vTexApiToken);
            headers.Add("X-VTEX-API-AppKey", account.vTexApiKey);
            return true;
        }
    }
}
