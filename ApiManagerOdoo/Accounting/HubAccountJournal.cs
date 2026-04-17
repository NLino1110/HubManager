using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

using System.Data;


namespace ApiManagerOdoo.Accounting
{
    public class HubAccountJournal : HubBase
    {
        string[] fields_array = {
            "id",
            "name",
            "code",
            "type",
            "active",
            "use_mobile_app",
            "mobile_app_tag_ids",
            "bank_account_id",
            "company_id",
            "inbound_payment_method_line_ids",
            "aplica_cheque",
            "aplica_tarjeta",
            "create_date",
            "write_date"
        };

        public HubAccountJournal(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.journal";
        }

        public async Task<ApiResponseOdooRpc?> GetCount(string company_ids)
        {
            int[] company_ids_array = company_ids.Split(',')
                                      .Select(int.Parse)
                                      .ToArray();

            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
            object[] _custom_args = new object[] {
                new object[] {"type", "in", new string[] { "cash", "bank" } },
                new object[] { "company_id.id", "in", company_ids_array },
                new object[] { "|" },
                new object[] { "code", "like", $"B%" },
                new object[] { "|" },
                new object[] { "code", "like", $"EF%" },
                new object[] { "code", "=", $"CCLI" },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetAccountJournal(int[] company_ids)
        {
            //string domains = $"domain=[('use_mobile_app', '=', True),('active', '=', True),('company_id.id','in',[{company_ids}])]";

            int limit = 100;
            int index = 0;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                //TODO: Activar cuando se corrija el codigo de Odoo
                new object[] { "use_mobile_app", "=", true},
                new object[] { "active", "=", true },
                new object[] { "company_id", "in", company_ids },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCount(int year, int month, int day)
        {
            var kwargs = new
            {
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                //new object[] { "use_mobile_app", "=", true},
                new object[] { "active", "=", true },
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 00:00:00" },
            };
            return await GetCount(args, _custom_args);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetAccountJournal(string company_ids)
        {
            //string domains = $"domain=[('use_mobile_app', '=', True),('active', '=', True),('company_id.id','in',[{company_ids}])]";
            
            int limit = 100;
            int index = 0;

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                
                //new object[] { "use_mobile_app", "=", true },
                new object[] { "active", "=", true},
                new object[] { "company_id.id", "in", $"[{company_ids}]" },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetCountByCreateDate(int year, int month, int day)
        {
            object[] args = new object[] { };            
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

        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetByCreateDate(int year, int month, int day)
        {
            int limit = 100;
            int index = 0;

            var kwargs = new
            {
                //limit = limit,
                //offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetByWriteDate_dl(int year, int month, int day)
        {
            //int limit = 100;
            //int index = 0;

            var kwargs = new
            {
                //limit = limit,
                //offset = (index * limit),
                fields = new object[] { "id", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetByCreateDate_dl(int year, int month, int day)
        {
            //int limit = 100;
            //int index = 0;

            var kwargs = new
            {
                //limit = limit,
                //offset = (index * limit),
                fields = new object[] { "id", "create_date", "write_date" }
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetItems(DateTime dateIni, int limit, int index)
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
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpcT<account_journal[]>?> GetByWriteDate(int year, int month, int day)
        { 
            //int limit = 100;
            //int index = 0;

            var kwargs = new
            {
                //limit = limit,
                //offset = (index * limit),
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "active", "=", true },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_journal[]>>(args, _custom_args, kwargs, true);
        }
    }
}
