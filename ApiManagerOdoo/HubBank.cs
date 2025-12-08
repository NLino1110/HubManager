using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;


namespace ApiManager
{
    public class HubBank : HubBase
    {
        string[] fields_array = new[] {
            "id",
            "name",
            "street",
            "street2",
            "city"
        };

        public HubBank(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.bank";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni)
        {
            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                //new object[] { "date", ">=", $"{dateIni.ToString("yyyy-MM-dd")}" },
                //new object[] { "invoice_date", "!=", false },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<Bank_Id[]>?> GetAll(string bank_ids)
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
            return await SearchRead<ApiResponseOdooRpcT<Bank_Id[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<Bank_Id[]>?> GetAll(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                
            };
            return await SearchRead<ApiResponseOdooRpcT<Bank_Id[]>>(args, _custom_args, kwargs, true);
        }
    }
}
