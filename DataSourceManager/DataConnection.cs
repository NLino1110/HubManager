using Models.DMSA.Shared.Security;
using Models.DMSA.Shared.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataSourceManager.Tools
{
    public class DataConnection
    {
        private string BuildByDriver(Dataservers dataservers)
        {
            string strConnection = "";
            string persistsecurityinfo = "true";
            string convertzerodatetime = "true";

            //MySql by default
            if (dataservers.Driver.ToLower() == "mysql" || dataservers.Driver.ToLower() == "")
            {
                strConnection = "Server=" + dataservers.Server + ";Port=" + dataservers.Port + 
                    ";userid=" + dataservers.User + ";password=" + dataservers.Password + 
                    ";Database=" + dataservers.Database + ";persistsecurityinfo=" + persistsecurityinfo + 
                    ";convert zero datetime=" + convertzerodatetime;
            }

            //Oracle by default
            if (dataservers.Driver.ToLower() == "oracle")
            {
                var CnType = dataservers.Type.Trim();
                strConnection = $"User Id={dataservers.User};Password={dataservers.Password};Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = {dataservers.Server})(PORT = {dataservers.Port})))(CONNECT_DATA =({CnType} = {dataservers.Database})))";
            }

            return strConnection;
        }

        public string GetDefaultConnectionString(string byDriver)
        {
            string strConnection = "";
            foreach ( var serverSetting in ConfigurationHelper.GetAppSettings().profile.DataServers)
            {
                if (serverSetting.Driver.ToLower().Equals(byDriver.ToLower()) && serverSetting.default_setup)
                {
                    strConnection = BuildByDriver(serverSetting);
                    break;
                }
            }

            //string Server = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Server;
            //string Port = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Port;
            //string user = ConfigurationHelper.GetAppSettings().profile.DataServers[0].User;
            //string password = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Password;
            //string Database = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Database;
            //string Driver = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Driver;
            //string CnType = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Type;

            //string persistsecurityinfo = "true";
            //string convertzerodatetime = "true";

            //string strConnection = "";

            ////MySql by default
            //if (Driver.ToLower() == "mysql" || Driver.ToLower() == "")
            //{
            //    strConnection = "Server=" + Server + ";Port=" + Port + ";userid=" + user + ";password=" + password + ";Database=" + Database + ";persistsecurityinfo=" + persistsecurityinfo + ";convert zero datetime=" + convertzerodatetime;
            //}

            ////Oracle by default
            //if (Driver.ToLower() == "oracle")
            //{
            //    CnType = CnType.Trim();                
            //    strConnection = $"User Id={user};Password={password};Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = {Server})(PORT = {Port})))(CONNECT_DATA =({CnType} = {Database})))";
            //}

            return strConnection;
        }

        public string GetConnectionString()
        {
            string Server = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Server;
            string Port = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Port;
            string user = ConfigurationHelper.GetAppSettings().profile.DataServers[0].User;
            string password = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Password;
            string Database = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Database;
            string Driver = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Driver;
            string CnType = ConfigurationHelper.GetAppSettings().profile.DataServers[0].Type;

            //string Server = ConfigurationHelper.GetValue($"DataServers:{section}:Server");
            //string Port = ConfigurationHelper.GetValue($"DataServers:{section}:Port");
            //string user = ConfigurationHelper.GetValue($"DataServers:{section}:User");
            //string password = ConfigurationHelper.GetValue($"DataServers:{section}:Password");
            //string Database = ConfigurationHelper.GetValue($"DataServers:{section}:Database");
            //string Driver = ConfigurationHelper.GetValue($"DataServers:{section}:Driver");
            //string CnType = ConfigurationHelper.GetValue($"DataServers:{section}:Type");

            //string Server = "localhost";
            //string Port = "3306";
            //string user = "root";
            //string password = "";
            //string Database = "middleware_vtex";

            string persistsecurityinfo = "true";
            string convertzerodatetime = "true";

            string strConnection = "";

            //MySql by default
            if(Driver.ToLower() == "mysql" || Driver.ToLower() == "")
            {
                strConnection = "Server=" + Server + ";Port=" + Port + ";userid=" + user + ";password=" + password + ";Database=" + Database + ";persistsecurityinfo=" + persistsecurityinfo + ";convert zero datetime=" + convertzerodatetime;
            }

            //Oracle by default
            if (Driver.ToLower() == "oracle")
            {
                CnType = CnType.Trim();
                //SERVICE_NAME
                //SID
                //El tipo de conexión debe parametrizarse
                //if(CnType.Trim().Equals("SID"))
                //{

                //}

                //strConnection = $"User Id={user};Password={password};Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = {Server})(PORT = {Port})))(CONNECT_DATA =(SID = {Database})))";
                strConnection = $"User Id={user};Password={password};Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = {Server})(PORT = {Port})))(CONNECT_DATA =({CnType} = {Database})))";
            }

            return strConnection;
        }
    }
}
