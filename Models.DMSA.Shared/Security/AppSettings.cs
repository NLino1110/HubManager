using Models.DMSA.Shared.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.DMSA.Shared.Security
{
    public class AppSettings
    {
        public string UseProfile { get; set; }
        public Profile profile { get; set; }
    }

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

    public class _odoo
    {
        public string ApiBaseAddressOdoo { get; set; }
        public string uid { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string api_key { get; set; }
        public string access_token { get; set; }
    }

    public class _Tasks
    {
        public _Tasks_Struct CacheBuilder { get; set; }
    }

    public class _Tasks_Struct
    {
        public string name { get; set; }
        public string? description { get; set; }
        public string schedule { get; set; }
        public bool enabled { get; set; }
        public ItemBuild[] items { get; set; }
    }

    public class _EmailSettings
    {
        public bool Enabled { get; set; }
        public string Period { get; set; }
    }

    //public class Erpnext
    //{
    //    public string Server { get; set; }
    //    public string WorkingDirectory { get; set; }
    //    public string LinuxUser { get; set; }
    //    public string LinuxPassword { get; set; }
    //    public string mariadb_root_password { get; set; }
    //    public string newsite_admin_password { get; set; }
    //}

    public class Dataservers
    {
        public string IdConnection { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public string Driver { get; set; }
        public string Type { get; set; }
        public bool default_setup { get; set;}
    }

    //public class Dataservers
    //{
    //    public Import Import { get; set; }
    //}

    //public class Import
    //{
    //    public string Server { get; set; }
    //    public string Port { get; set; }
    //    public string User { get; set; }
    //    public string Password { get; set; }
    //    public string Database { get; set; }
    //    public string Driver { get; set; }
    //}

    public class Logging
    {
        public Loglevel LogLevel { get; set; }
    }

    public class Loglevel
    {
        public string Default { get; set; }
        public string Microsoft { get; set; }
        public string MicrosoftHostingLifetime { get; set; }
    }

}
