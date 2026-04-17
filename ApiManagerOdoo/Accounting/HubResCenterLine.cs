using System.Reflection;
using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;
using Newtonsoft.Json;

namespace ApiManagerOdoo.Accounting
{
    public class HubResCenterLine : HubBase
    {
        string[] fields_array = new[] { 
            "id", 
            "name",
            "display_name",
            "code",
            "doc_authorization_line_id",
            "center_id", 
            "create_date", 
            "write_date" 
        };

        private readonly Type _type;
        public HubResCenterLine(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.center.line";

            _type = typeof(res_center_line);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni, int center_id)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                 new object[] { "center_id", "=", center_id }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(DateTime dateIni, int[] center_ids)
        {
            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                 new object[] { "center_id", "in", center_ids }
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<res_center_line[]>?> GetItems(DateTime dateIni, int limit, int index)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") }
            };
            return await SearchRead<ApiResponseOdooRpcT<res_center_line[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_center_line[]>?> GetItems(DateTime dateIni, int limit, int index, int center_id)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "center_id", "=", center_id }
            };
            return await SearchRead<ApiResponseOdooRpcT<res_center_line[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<res_center_line[]>?> GetItems(DateTime dateIni, int limit, int index, int[] center_ids)
        {
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", dateIni.ToString("yyyy-MM-dd") },
                new object[] { "center_id", "in", center_ids }
            };
            return await SearchRead<ApiResponseOdooRpcT<res_center_line[]>>(args, _custom_args, kwargs, true);
        }
    }
}
