using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Models.DMSA.Shared.Security;

namespace Models.DMSA.Shared.Tools
{
    /// <summary>
    /// Esta clase ayuda a obtener los datos de la configuracion de la aplicación sin utilizar el IConfigurationRoot
    /// del arranque, ya que este es inalcanzable en este tipo de aplicaciones.
    /// Utilizar la alternativa de una variable static en el StartUp no es recomendable por cuestiones de seguridad
    /// 
    /// </summary>
    public class ConfigurationHelper
    {
        static private IConfigurationRoot builder = null;
        static private AppSettings appSettings { get; set; }

        static public AppSettings GetAppSettings()
        {
            return appSettings;
            //return appSettings ?? (appSettings = new AppSettings());
        }

        static public void setAppSettings(AppSettings newAppSettings)
        {
            appSettings = newAppSettings;
        }

        /// <summary>
        /// Lee el valor de la variable especificada por el KeyMap, por ejemplo
        /// DataServers:Target:Database
        /// </summary>
        /// <param name="KeyMap"></param>
        /// <returns></returns>
        //static public string _GetValue(string KeyMap)
        //{
        //    if (builder == null)
        //    {
        //        builder = new ConfigurationBuilder()
        //        .SetBasePath(Environment.CurrentDirectory)
        //        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //        //#if DEBUG
        //        //.AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: true)
        //        //.AddJsonFile($"appsettings.Development.v2.json", optional: false, reloadOnChange: true)                
        //        //#endif
        //        .Build();
        //    }

        //    return builder.GetSection(KeyMap).Value;
        //}
    }
}
