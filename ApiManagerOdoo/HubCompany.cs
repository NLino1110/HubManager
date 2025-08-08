using ApiManagerOdoo.Base;
using AppManagerOdoo.Tools;
using CobranzasDMSA.Models;
using DMSA.Models.Clientes;
using DMSA.Models.General;
using DMSA.Models.General.Responses;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;

namespace ApiManager
{
    public class HubCompany : HubBase
    {        
        string[] fields_array = new[] {
            "id", 
            "name",
            "partner_id",
            "email",
            "phone",
            "mobile",
            "social_twitter",
            "social_facebook",
            "social_github",
            "social_linkedin",
            "social_youtube",
            "social_youtube",
            //"check_journal_id"
        };

        readonly RestSharpMiddle _client;
        private AppSession _appSession { get; }

        public HubCompany(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.company";
        }


        public async Task<ApiResponseOdooRpcT<res_company[]>?> GetAll(string bank_ids)
        {
            int limit = 100;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {
                    "id","in",bank_ids
                },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_company[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<res_company[]>?> GetAll()
        {
            int limit = 100;
            int index = 0;

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                
            };
            return await SearchRead<ApiResponseOdooRpcT<res_company[]>>(args, _custom_args, kwargs, true);
        }
    }
}
