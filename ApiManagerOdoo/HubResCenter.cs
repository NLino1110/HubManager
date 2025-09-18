using System.Reflection;
using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Native;
using DMSA.Models.Security;
using Newtonsoft.Json;

namespace ApiManager
{
    public class HubResCenter : HubBase
    {
        string[] fields_array = new[] { "id", "name", "clave_externa", "type_center", "create_date", "write_date" };

        private readonly Type _type;
        public HubResCenter(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "res.center";

            _type = typeof(res_center);
        }

        public async Task<ApiResponseOdooRpc?> GetCount()
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {};
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<res_center[]>> GetItems(int limit, int index, int year, int month, int day)
        {

            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
            };

            return await SearchRead<ApiResponseOdooRpcT<res_center[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };            
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<res_center[]>> GetByCreateDate(int limit, int index, int year, int month, int day)
        {
            
            var kwargs = new
            {
                limit = limit,
                offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                //new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            return await SearchRead<ApiResponseOdooRpcT<res_center[]>>( args, _custom_args, kwargs);
        }

        public async Task<object?> GetByWriteDate_dl(int year, int month, int day)
        {            
            var kwargs = new
            {
                fields = new[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };

            //return await SearchRead<ApiResponseOdooRpcT<product_template[]>>(apiRequestOdoo_V1, args, _custom_args, kwargs);

            var method = typeof(HubStore).GetMethod(nameof(SearchRead), BindingFlags.NonPublic | BindingFlags.Instance);
            var generic = method.MakeGenericMethod(_type);
            return await (Task<object?>)generic.Invoke(this, new object[] {year, month, day });
        }

        public async Task<ApiResponseOdooRpcT<res_center[]>?> GetByCreateDate_dl(int year, int month, int day)
        {            
            var kwargs = new
            {
                fields = new[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_center[]>>(args, _custom_args, kwargs);
        }

        public async Task<ApiResponseOdooRpcT<res_center[]>?> GetByWriteDate(int year, int month, int day)
        {            
            var kwargs = new
            {
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await SearchRead<ApiResponseOdooRpcT<res_center[]>>(args, _custom_args, kwargs);
        }
    }
}
