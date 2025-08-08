//using Microsoft.Extensions.Configuration;
//using System;

///// <summary>
///// 
///// </summary>
//namespace DataSourceManager.Tools
//{
//    /// <summary>
//    /// Esta clase ayuda a obtener los datos de la configuracion de la aplicación sin utilizar el IConfigurationRoot
//    /// del arranque, ya que este es inalcanzable en este tipo de aplicaciones.
//    /// Utilizar la alternativa de una variable static en el StartUp no es recomendable por cuestiones de seguridad
//    /// 
//    /// </summary>
//    public class ConfigurationHelper
//    {
//        static private IConfigurationRoot builder = null;

//        /// <summary>
//        /// Lee el valor de la variable especificada por el KeyMap, por ejemplo
//        /// DataServers:Target:Database
//        /// </summary>
//        /// <param name="KeyMap"></param>
//        /// <returns></returns>
//        static public string GetValue(string KeyMap)
//        {
//            if (builder == null)
//            {
//                builder = new ConfigurationBuilder()
//                .SetBasePath(Environment.CurrentDirectory)
//                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
//            }

//            return builder.GetSection(KeyMap).Value;
//        }
//    }
//}
