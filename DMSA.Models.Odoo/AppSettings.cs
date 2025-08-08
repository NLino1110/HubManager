using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Models.Odoo
{
    public class AppSettings
    {
        //[PrimaryKey]
        //[AutoIncrement]
        //public int id { get; set; }

        [PrimaryKey]
        public string name { get; set; }
        public string description { get; set; }
        public string value { get; set; }
        public bool forAdmin { get; set; }

        public AppSettings()
        {

        }

        public List<AppSettings> LoadDefault()
        {
            List<AppSettings> defaultSettings = new List<AppSettings>();
            defaultSettings.Add( new AppSettings()
                {
                    name = "is_production",
                    description = "Mode of application production/develop",
                    value = "false",
                    forAdmin = true,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "is_test_mode",
                    description = "Mode of application for testing",
                    value = "true",
                    forAdmin = true,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "endpoint_server_dev",
                    description = "Url Odoo Api Developer",
                    value = "http://192.168.204.66:8069",
                    forAdmin = false,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "endpoint_server_prod",
                    description = "Url Odoo Api Production",
                    value = "http://192.168.0.124:8090",
                    forAdmin = false,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "url_cache_files_internal",
                    description = "Cache Path Internal",
                    value = "/resources/tmp/android/sqlite/",
                    forAdmin = false,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "url_cache_files_external",
                    description = "Cache Path External",
                    value = "/resources/tmp/android/sqlite/",
                    forAdmin = false,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "url_resources_dev",
                    description = "Resources Server Api Developer",
                    value = "https://192.168.204.66:2443",
                    forAdmin = false,
                }
            );

            defaultSettings.Add(new AppSettings()
                {
                    name = "url_resources_prod",
                    description = "Resources Server Api Production",
                    value = "https://200.110.68.142:2443",
                    forAdmin = false,
                }
            );

            defaultSettings.Add(new AppSettings()
            {
                name = "url_report_server",
                description = "Reports Server Url",
                value = "http://192.168.0.120:8080/jasperserver",
                forAdmin = false,
            }
            );

            defaultSettings.Add(new AppSettings()
            {
                name = "default_database",
                description = "Default database",
                value = "macronegocios_dev2",
                forAdmin = true,
            }
            );

            defaultSettings.Add(new AppSettings()
            {
                name = "db_limit_default",
                description = "Default Limit database",
                value = "300",
                forAdmin = true,
            }
            );

            defaultSettings.Add(new AppSettings()
            {
                name = "sync_date_since",
                description = "Date Sync Since",
                value = "2023/01/01",
                forAdmin = true,
            }
            );

            return defaultSettings;
        }
    }
}
