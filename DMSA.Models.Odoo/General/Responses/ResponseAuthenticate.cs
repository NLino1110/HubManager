using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.General.Responses
{  
    public class ResponseAuthenticate
    {
        public string jsonrpc { get; set; }
        public object id { get; set; }
        public ResultAuth result { get; set; }
        public Error error { get; set; }
        public CookieCollection Cookies { get; set; }
    }

    public class ResultAuth
    {
        public int uid { get; set; }
        public bool is_system { get; set; }
        public bool is_admin { get; set; }
        public User_Context user_context { get; set; }
        public string db { get; set; }
        public string server_version { get; set; }
        public object[] server_version_info { get; set; }
        public string support_url { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string partner_display_name { get; set; }
        public int company_id { get; set; }
        public int partner_id { get; set; }
        public string webbaseurl { get; set; }
        public int active_ids_limit { get; set; }
        public object profile_session { get; set; }
        public object profile_collectors { get; set; }
        public object profile_params { get; set; }
        public int max_file_upload_size { get; set; }
        public bool home_action_id { get; set; }
        public Cache_Hashes cache_hashes { get; set; }
        public Currencies currencies { get; set; }
        public Bundle_Params bundle_params { get; set; }
        public User_Companies user_companies { get; set; }
        public bool show_effect { get; set; }
        public bool display_switch_company_menu { get; set; }
        public int[] user_id { get; set; }
        public int max_time_between_keys_in_ms { get; set; }
        public string warning { get; set; }
        public string expiration_date { get; set; }
        public string expiration_reason { get; set; }
        public object[] web_tours { get; set; }
        public bool tour_disable { get; set; }
        public string notification_type { get; set; }
        public bool map_box_token { get; set; }
        public bool odoobot_initialized { get; set; }
        public bool ocn_token_key { get; set; }
        public string fcm_project_id { get; set; }
        public int inbox_action { get; set; }
        public bool iap_company_enrich { get; set; }
        public string dbuuid { get; set; }
        public bool multi_lang { get; set; }
        public Uom_Ids uom_ids { get; set; }
    }

    public class User_Context
    {
        public string lang { get; set; }
        public string tz { get; set; }
        public int uid { get; set; }
    }

    public class Cache_Hashes
    {
        public string translations { get; set; }
        public string load_menus { get; set; }
    }

    public class Currencies
    {
        public _7 _7 { get; set; }
        public _1 _1 { get; set; }
        public _154 _154 { get; set; }
        public _2 _2 { get; set; }
    }

    public class _7
    {
        public string symbol { get; set; }
        public string position { get; set; }
        public int[] digits { get; set; }
    }

    public class _1
    {
        public string symbol { get; set; }
        public string position { get; set; }
        public int[] digits { get; set; }
    }

    public class _154
    {
        public string symbol { get; set; }
        public string position { get; set; }
        public int[] digits { get; set; }
    }

    public class _2
    {
        public string symbol { get; set; }
        public string position { get; set; }
        public int[] digits { get; set; }
    }

    public class Bundle_Params
    {
        public string lang { get; set; }
    }

    public class User_Companies
    {
        public int current_company { get; set; }
        public Allowed_Companies allowed_companies { get; set; }
    }

    public class Allowed_Companies
    {
        public _11 _1 { get; set; }
        public _5 _5 { get; set; }
        public _6 _6 { get; set; }
        public _71 _7 { get; set; }
    }

    public class _11
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sequence { get; set; }
        public int timesheet_uom_id { get; set; }
        public float timesheet_uom_factor { get; set; }
    }

    public class _5
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sequence { get; set; }
        public int timesheet_uom_id { get; set; }
        public float timesheet_uom_factor { get; set; }
    }

    public class _6
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sequence { get; set; }
        public int timesheet_uom_id { get; set; }
        public float timesheet_uom_factor { get; set; }
    }

    public class _71
    {
        public int id { get; set; }
        public string name { get; set; }
        public int sequence { get; set; }
        public int timesheet_uom_id { get; set; }
        public float timesheet_uom_factor { get; set; }
    }

    public class Uom_Ids
    {
        public _4 _4 { get; set; }
    }

    public class _4
    {
        public int id { get; set; }
        public string name { get; set; }
        public float rounding { get; set; }
        public string timesheet_widget { get; set; }
    }

}
