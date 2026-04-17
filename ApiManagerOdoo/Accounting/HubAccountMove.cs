using ApiManagerOdoo.Base;
using DMSA.Models.Odoo.Accounting;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Security;

namespace ApiManagerOdoo.Accounting
{
    public class HubAccountMove : HubBase
    {  
        string[] fields_array = {
            "id",
            "name",
            "partner_id",
            "invoice_date",
            "invoice_date_due",
            "payment_state",
            "state",
            "move_type",
            "journal_id",
            "amount_residual",
            "amount_untaxed_signed",
            "amount_total_signed",
            "amount_total",
            "amount_tax",
            "l10n_latam_document_type_id",
            "invoice_user_id",
            "company_id",
            "team_id",
            "invoice_line_ids",
            "reversed_entry_id",
            "ref",
            "refund_invoice_ids",
            "docnum_mask",
            "create_date",
            "write_date",
            //"printer_id"
        };
        public HubAccountMove(AppSession _setAppSession) : base(_setAppSession)
        {
            EndPointApi = "/connect/get_account_move";
            EndPointApi = "/web/dataset/call_kw";
            _modelname = "account.move";
        }

        //public HubAccountMove(AppSession _setAppSession, string modelname) : base(_setAppSession)
        //{

        //}

        //string EndPointServer = "";
        //string EndPointApi = "/connect/get_account_move";

        //readonly RestSharpMiddle _client;
        //private AppSession _appSession { get; }

        //[Inject]
        //private IConfiguration configuration { get; set; }
        //private static readonly IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddEnvironmentVariables().Build();

        public async Task<ApiResponseOdooRpc?> GetHeaderCount(int year, int month, int day)
        {
            //string _EndPointApi = "/api/account.move";
            //string domains = $"domain=[('invoice_date','>=','{apiRequestOdoo_V1.dateIni.ToString("yyyy-MM-dd")}')]";

            object[] args = new object[] { };

            object[] _custom_args = new object[] {
                new object[] { "write_date", ">=", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "state", "=", "posted" },
                new object[] { "move_type", "=", "out_invoice" },
                new object[] { "invoice_date", "!=", false },
            };
            return await GetCount(args, _custom_args);
        }

        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMoves(DateTime dateIni, int limit, int index)
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
                new object[] { "state", "=", "posted" },
                new object[] { "move_type", "=", "out_invoice" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        public async Task<ApiResponseOdooRpc?> GetHeaderCountByDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
            object[] _custom_args = new object[] {
                new object[] {"create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] {"create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
            };
            return await GetCount(args, _custom_args);
        }

        //public async Task<ApiResponseOdoo?> __GetHeaderCountByDate(ApiRequestOdoo_v1 apiRequestOdoo_V1, int year, int month, int day)
        //{
        //    string _EndPointApi = "/api/account.move";
        //    string api_key = _appSession.CurrentUser.api_key;
        //    string domains = $"domain=[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59'),('invoice_date','!=', False)]";
        //    string EndPointParams = "";
        //    EndPointParams = $"/search_count?api_key={api_key}&{domains}";

        //    var restRequest = new RestRequest(_EndPointApi + EndPointParams);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    var result = await _client.RestClient().ExecuteGetAsync(restRequest);
        //    //Console.WriteLine(result);

        //    if (result != null && result.Content != null & result.Content != "")
        //    {
        //        var resultNative = JsonConvert.DeserializeObject<ApiResponseOdoo>(result.Content);
        //        return resultNative;
        //    }

        //    return new ApiResponseOdoo()
        //    {
        //        responseCode = 500,
        //        count = 0,
        //        message = "Error al obtener datos."
        //    };
        //}

        //[Obsolete("Debe ser eliminado.")]
        //public async Task<ApiResponse_account_move?> _Get(ApiRequestOdoo_v1 apiRequestOdoo_V1, string fields, string domains) //int year, int month, int day)
        //{            
        //    string limit = $"limit={apiRequestOdoo_V1.limit}";
        //    string offset = $"offset={apiRequestOdoo_V1.index * apiRequestOdoo_V1.limit}";
        //    string EndPointParams = $"?{fields}&{domains}&{limit}&{offset}";

        //    var restRequest = await _client.BuildAuthorizedRequest(EndPointApi + EndPointParams);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    //_client.setUseAuthorization();
        //    var result = await _client.RestClient().ExecuteGetAsync(restRequest);
        //    //Console.WriteLine(result);

        //    if(ApiManagerOdoo.Tools.Validations.IsValidResponse(result))
        //    //if (result != null && result.Content != null & result.Content != "")
        //    {
        //        //Console.WriteLine(result.Content);
        //        var resultNative = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_account_move>(result.Content);
        //        return resultNative;
        //    }

        //    return new ApiResponse_account_move()
        //    {
        //        responseCode = 500,
        //        message = "Error al obtener datos."
        //    };
        //}

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByDate(int year, int month, int day, int limit, int index)
        {
            //string fields = "fields=['id','name','partner_id','invoice_date','invoice_date_due','payment_state','move_type','journal_id','amount_residual','amount_untaxed_signed','amount_total_signed','amount_total','amount_tax','l10n_latam_document_type_id','invoice_user_id','company_id','team_id','invoice_line_ids','reversed_entry_id','ref','refund_invoice_ids','printer_id']";
            
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">=", $"{year}-{month:00}-{day:00} 00:00:00" },
                new object[] { "create_date", "<=", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },                
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpc?> GetHeaderCountByWriteDate(int year, int month, int day)
        {
            object[] args = new object[] { };
            //string _custom_args = $"[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await GetCount(args, _custom_args);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByWriteDate(int year, int month, int day, int limit, int index)
        {
            //string fields = "fields=['id','name','partner_id','invoice_date','invoice_date_due','payment_state','move_type','journal_id','amount_residual','amount_untaxed_signed','amount_total_signed','amount_total','amount_tax','l10n_latam_document_type_id','invoice_user_id','company_id','team_id','invoice_line_ids','reversed_entry_id','ref','refund_invoice_ids','printer_id']";
            
            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {                
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetAccountMovesByWriteDate_dl(int year, int month, int day, int limit, int index)
        {
            string[] fields_array = {
            "id",
            "write_date"
            };

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "write_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);

        }

        [Obsolete]
        public async Task<ApiResponseOdooRpcT<account_move[]>?> GetByCreateDate_dl(int year, int month, int day, int limit, int index)
        {
            string[] fields_array = {
            "id",
            "create_date",
            "write_date"
            };

            var kwargs = new
            {
                limit,
                offset = index * limit,
                fields = fields_array
            };

            object[] args = new object[] { };
            object[] _custom_args = new object[] {
                new object[] { "create_date", ">", $"{year}-{month:00}-{day:00} 23:59:59" },
                new object[] { "invoice_date", "!=", false },
            };
            return await SearchRead<ApiResponseOdooRpcT<account_move[]>>(args, _custom_args, kwargs, true);
        }



        //public async Task<ApiResponseOdoo?> __GetDetailCount(ApiRequestOdoo_v1 apiRequestOdoo_V1)
        //{
        //    string _EndPointApi = "/api/account.move.line";
        //    string api_key = _appSession.CurrentUser.api_key;            
        //    string domains = $"domain=[('date','>=','{apiRequestOdoo_V1.dateIni.ToString("yyyy-MM-dd")}')]";            
        //    string EndPointParams = "";
        //    EndPointParams = $"/search_count?api_key={api_key}&{domains}";

        //    var restRequest = new RestRequest(_EndPointApi + EndPointParams);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    var result = await _client.RestClient().ExecuteGetAsync(restRequest);
        //    //Console.WriteLine(result);

        //    if (result != null && result.Content != null & result.Content != "")
        //    {
        //        var resultNative = JsonConvert.DeserializeObject<ApiResponseOdoo>(result.Content);
        //        return resultNative;
        //    }

        //    return new ApiResponseOdoo()
        //    {
        //        responseCode = 500,
        //        count = 0,
        //        message = "Error al obtener datos."
        //    };
        //}
        
        //public async Task<ApiResponse_account_move_line?> __GetAccountMoveLines(ApiRequestOdoo_v1 apiRequestOdoo_V1)
        //{
        //    string _EndPointApi = "/api/account.move.line";

        //    string api_key = _appSession.CurrentUser.api_key;
        //    string fields = "fields=['id','move_id','sequence','name','product_id','quantity','price_unit','price_subtotal','discount_balance','price_total','discount_percentage','tax_ids','analytic_line_ids','display_type','account_id','create_date','write_date']";
        //    //string domains = "domain=[('invoice_date','>=','2023-08-29'),('invoice_date','<=','2023-08-29')]";
        //    string domains = $"domain=[('date','>=','{apiRequestOdoo_V1.dateIni.ToString("yyyy-MM-dd")}')]";
        //    //string domains = $"domain=[('id','in',('8471', '8474'))]";
        //    string limit = $"limit={apiRequestOdoo_V1.limit}";

        //    string offset = $"offset={apiRequestOdoo_V1.index * apiRequestOdoo_V1.limit}";
        //    string EndPointParams = $"/search?api_key={api_key}&{fields}&{domains}&{limit}&{offset}";
        //    EndPointParams = $"/search?api_key={api_key}&{fields}&{domains}&{limit}&{offset}";

        //    var restRequest = new RestRequest(_EndPointApi + EndPointParams);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    var result = await _client.RestClient().ExecuteGetAsync(restRequest);
        //    //Console.WriteLine(result);

        //    if (result != null && result.Content != null & result.Content != "")
        //    {
        //        var resultNative = JsonConvert.DeserializeObject<ApiResponse_account_move_line>(result.Content);
        //        return resultNative;
        //    }

        //    return new ApiResponse_account_move_line()
        //    {
        //        responseCode = 500,
        //        message = "Error al obtener datos."
        //    };
        //}

        //public async Task<ApiResponseOdoo?> __GetAccountMoveLinesCountByDate(ApiRequestOdoo_v1 apiRequestOdoo_V1, int year, int month, int day)
        //{
        //    string _EndPointApi = "/api/account.move.line";
        //    string api_key = _appSession.CurrentUser.api_key;
        //    string domains = $"domain=[('create_date','>=','{year}-{month:00}-{day:00} 00:00:00'),('create_date','<=','{year}-{month:00}-{day:00} 23:59:59')]";
        //    string EndPointParams = "";
        //    EndPointParams = $"/search_count?api_key={api_key}&{domains}";

        //    var restRequest = new RestRequest(_EndPointApi + EndPointParams);
        //    restRequest.RequestFormat = DataFormat.Json;
        //    var result = await _client.RestClient().ExecuteGetAsync(restRequest);
        //    //Console.WriteLine(result);

        //    if (result != null && result.Content != null & result.Content != "")
        //    {
        //        var resultNative = JsonConvert.DeserializeObject<ApiResponseOdoo>(result.Content);
        //        return resultNative;
        //    }

        //    return new ApiResponseOdoo()
        //    {
        //        responseCode = 500,
        //        count = 0,
        //        message = "Error al obtener datos."
        //    };
        //}

    }
}
