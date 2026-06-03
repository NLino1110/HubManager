using DMSA.Models.Odoo.Tools;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo.Abstract
{    
    public partial class OdooConnection
    {
        public List<OdooConnection> LoadDefaultCobranzas()
        {
            List<OdooConnection> defaultSettings = new List<OdooConnection>();
            
            defaultSettings.Add(new OdooConnection()
            {
                Id = 2,
                CompanyId = 1,
                Name = "Macronegocios (DEV)",
                Host = "https://dev-qa.macronegocios/",
                DbName = "macronegocios",
                Username = "admin_cobranzas",
                Password = CryptoHelper.Encrypt("2OGFnIMttn3I8FdB"),
                Active = false,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                HostPush = "https://admin.dmujeres.ec:1500/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                UsernameFront = "jchonillo",
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 73,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 3,
                CompanyId = 1,
                Name = "DMujeres (DEV)",
                Host = "https://dev-qa.macronegocios/",
                DbName = "dmujeressa",
                Username = "admin_cobranzas",
                Password = CryptoHelper.Encrypt("2OGFnIMttn3I8FdB"),
                Active = false,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 147,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 31,
                CompanyId = 1,
                Name = "Macronegocios (QA)",
                Host = "http://qa.macronegocios:8069/",
                DbName = "qa.macronegocios",
                Username = "admin_cobranzas",
                Password = CryptoHelper.Encrypt("2OGFnIMttn3I8FdB"),
                Active = false,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 147,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 6,
                CompanyId = 1,
                Name = "DMujeres Prod",
                Host = "https://www.dmujeressa.ec/",
                DbName = "dmujeressa",
                Username = "admin_cobranzas",
                Password = CryptoHelper.Encrypt("2OGFnIMttn3I8FdB"),
                Active = true,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = true,
                IsTestMode = false,
                DataToleranceDays = 365,
                sale_channel_default = 3,
                res_center_default = 2,
                prefix_db = "prod1_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 147,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 7,
                CompanyId = 1,
                Name = "Macronegocios Prod",
                Host = "https://www.macronegocios.ec/",
                DbName = "macronegocios",
                Username = "admin_cobranzas",
                Password = CryptoHelper.Encrypt("2OGFnIMttn3I8FdB"),
                Active = true,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = true,
                IsTestMode = false,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "prod1_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 73,
                project_id = 2,
                stage_id = 0,
            }
            );

            return defaultSettings;
        }



        public List<OdooConnection> LoadDefaultPedidos()
        {
            List<OdooConnection> defaultSettings = new List<OdooConnection>();

            defaultSettings.Add(new OdooConnection()
            {
                Id = 2,
                CompanyId = 1,
                Name = "Macronegocios (DEV)",
                Host = "https://dev-qa.macronegocios/",
                DbName = "macronegocios",
                Username = "admin_orders",
                Password = CryptoHelper.Encrypt("Ddwon4wShi8JkINH"),
                Active = false,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                HostPush = "https://admin.dmujeres.ec:1500/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                UsernameFront = "jchonillo",
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 73,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 3,
                CompanyId = 1,
                Name = "DMujeres (DEV)",
                Host = "https://dev-qa.macronegocios/",
                DbName = "dmujeressa",
                Username = "admin_orders",
                Password = CryptoHelper.Encrypt("Ddwon4wShi8JkINH"),
                Active = false,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 147,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 31,
                CompanyId = 1,
                Name = "Macronegocios (QA)",
                Host = "http://qa.macronegocios:8069/",
                DbName = "qa.macronegocios",
                Username = "admin_orders",
                Password = CryptoHelper.Encrypt("Ddwon4wShi8JkINH"),
                Active = false,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = false,
                IsTestMode = true,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "dev_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 147,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 6,
                CompanyId = 1,
                Name = "DMujeres Prod",
                Host = "https://www.dmujeressa.ec/",
                DbName = "dmujeressa",
                Username = "admin_orders",
                Password = CryptoHelper.Encrypt("Ddwon4wShi8JkINH"),
                Active = true,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = true,
                IsTestMode = false,
                DataToleranceDays = 365,
                sale_channel_default = 3,
                res_center_default = 2,
                prefix_db = "prod1_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 147,
                project_id = 2,
                stage_id = 0,
            }
            );

            defaultSettings.Add(new OdooConnection()
            {
                Id = 7,
                CompanyId = 1,
                Name = "Macronegocios Prod",
                Host = "https://www.macronegocios.ec/",
                DbName = "macronegocios",
                Username = "admin_orders",
                Password = CryptoHelper.Encrypt("Ddwon4wShi8JkINH"),
                Active = true,
                HostDump = "https://manager.dmujeres.ec:5001",
                HostDumpApiKey = "t.0.0.r.1381",
                DumpService = "/resources/tmp/android/sqlite/",
                DbLimitDefault = 300,
                IsProduction = true,
                IsTestMode = false,
                DataToleranceDays = 365,
                sale_channel_default = 8,
                res_center_default = 49,
                prefix_db = "prod1_",
                email_domain = "macronegocios.ec",
                preload_email_domain = true,
                PasswordFront = "Y0.dM3c",
                subclasificacion_gasto_default = 73,
                project_id = 2,
                stage_id = 0,
            }
            );

            return defaultSettings;
        }
    }
}
