using DMSA.Models.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Update;
using DMSA.Models.Security;
using Models.DMSA.Shared.General;
using Models.DMSA.Shared.Structs;
using Models.DMSA.Shared.Tools;
using Newtonsoft.Json;
using RestSharp;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ResourceBuilder.Data
{
    public partial class BuilderService
    {
        internal int default_limit = 300;
        ApiManager.HubProductProduct hubProductProduct { get; set; }
        AppSession _appSession { get; set; }

        public BuilderService()
        {
            var appSetting = ConfigurationHelper.GetAppSettings();

            _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;

            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };

            hubProductProduct = new ApiManager.HubProductProduct(_appSession);
        }

        private bool CreateJson(string filename, string prefix)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        "tmp",
                        "android", "sqlite");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(Path.Combine(path, filename)))
                File.Delete(Path.Combine(path, filename));

            File.WriteAllText(Path.Combine(path, filename), prefix);

            //Inicio
            //{
            //"FACNOTACREDITOCAB":
            //[

            //Fin
            //]

            //FACNOTACREDITOCAB
            //FACNOTACREDITOCAB.json

            return true;
        }

        public bool CreateJson_v2(string current_model, string filename, string prefix, int year, int month, int day)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        "tmp",
                        "android", "sqlite", current_model, 
                        year.ToString(), month.ToString(), day.ToString());

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(Path.Combine(path, filename)))
                File.Delete(Path.Combine(path, filename));

            File.WriteAllText(Path.Combine(path, filename), prefix);

            return true;
        }

        private bool AppendToJson(string filename, string content)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "resources",
                        "tmp",
                        "android", "sqlite", filename);
            File.AppendAllText(path, content);
            return true;
        }

        public bool AppendToJson_v2(string current_model, string filename, string content, int year, int month, int day)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "resources",
                        "tmp",
                        "android", "sqlite", current_model, year.ToString(), month.ToString(), day.ToString(), filename);
            File.AppendAllText(path, content);
            return true;
        }

        //private byte[] ReadBytes(string filename)
        //{
        //    var path = Path.Combine(Directory.GetCurrentDirectory(),
        //                "wwwroot",
        //                "resources",
        //                "tmp",
        //                "android", "sqlite", filename);

        //    return File.ReadAllBytes(path);
        //}

        //private byte[] CreateZip(string AttachName, byte[] body)
        //{
        //    using (var compressedFileStream = new MemoryStream())
        //    {
        //        //Create an archive and store the stream in memory.
        //        using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
        //        {
        //            //foreach (var caseAttachmentModel in caseAttachmentModels)
        //            //{
        //            //Create a zip entry for each attachment
        //            var zipEntry = zipArchive.CreateEntry(AttachName);

        //            //Get the stream of the attachment
        //            using (var originalFileStream = new MemoryStream(body))
        //            using (var zipEntryStream = zipEntry.Open())
        //            {
        //                //Copy the attachment stream to the zip entry stream
        //                originalFileStream.CopyTo(zipEntryStream);
        //            }
        //            //}
        //        }

        //        return compressedFileStream.ToArray();
        //    }
        //}

        private void CreateZipFile(IEnumerable<FileInfo> files, string archiveName)
        {
            using (var stream = File.Create(archiveName))
            using (ZipArchive archive = new ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Create))
            {
                foreach (var item in files)
                {
                    archive.CreateEntryFromFile(item.FullName, item.Name, CompressionLevel.Optimal);
                }
            }
        }

        private bool SaveZip(string filename, byte[] body)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "resources",
                        "tmp",
                        "android", "sqlite", filename);

            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllBytes(path, body);
            return true;
        }

        public async Task<bool> ProcessTable_v2(string jsonName, string actionName)
        {
            int multiplicador = 50;
            decimal numproc = 0;
            decimal numproc_up = 0;

            int countItems = 100; //total estimado

            if (countItems > multiplicador)
                numproc_up = Math.Ceiling((decimal)countItems / multiplicador);
            else
                numproc_up = countItems + 1;

            //Task[] taskArray = new Task[(int) numproc];

            List<Task> taskArray = new List<Task>();
            for (int i = 0; i < numproc_up; i++)
            {
                bool lastGroup = false;
                int proc = i + 1;

                if (!((i + 1) < numproc_up))
                    lastGroup = true;

                //taskArray.Add(Task.Factory.StartNew(() => ProcessGroup_Up(proc, cuenta, lastGroup, countItems)));
            }

            Task.WaitAll(taskArray.ToArray());

            return false;
        }

        public async Task<bool> ProcessTable(string jsonName, string actionName)
        {
            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            bool noSalir = true;
            
            //Se creará un archivo json por cada página
            // luego todos estos archivos json seran incluidos en el archivo zip

            for (int indice = 1; noSalir; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                string Action = actionName;
                string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string cadenaJson = "{\"indice\":" + indice.ToString() + ",\"fechaActualizaTablet\":\"2021-01-01 00:00:00\",\"esActualizacion\": true}";
                // "{\"codusuario\":\"" + EntryUserName.Text + "\",\"codclave\":\"" + EntryPassword.Text + "\",\"fechatablet\":\"" + currentDateTime + "\"}";

                List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

                parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
                parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));
                parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", "djimenez", ParameterType.QueryString));

                //App.Current.MainPage = new MainPage();
                SoapClient client = new SoapClient();
                var result = await client.asyncPostJson(parameters.ToArray());
                //Console.WriteLine(result);


                if (result != "")
                {
                    var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonResponseObject>(result);

                    if (resultUser != null)
                    {
                        if (resultUser.data != null)
                        {
                            //Iniciar el Json
                            CreateJson(jsonFileName, @"{
                                """ + jsonName + @""":
                                [");

                            //Debe terminar el proceso
                            if (resultUser.final)
                                noSalir = false;

                            var oldString = resultUser.data.ToString();
                            string newString = string.Join(" ", Regex.Split(oldString, @"(?:\r\n|\n|\r)"));
                            newString = newString.Trim();
                            newString = newString.Remove(0, 1);
                            newString = newString.Remove(newString.Length - 1, 1);
                            newString = newString.Trim();
                            //Console.Write(newString);

                            //if (indice > 1)
                            //    newString = "," + newString;

                            //Contenido de la página
                            AppendToJson(jsonFileName, newString);

                            //Finalizar el Json
                            AppendToJson(jsonFileName, "]}");

                            //if (indice == 10)
                            //    noSalir = false;
                        }
                        else
                        {
                            noSalir = false;
                        }
                    }
                    else
                    {
                        noSalir = false;
                    }

                    //dynamic resultUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
                    //Console.WriteLine(resultUsers.data.ToString());
                    //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<List<User>>(resultUsers.data.ToString());
                    //App.Current.MainPage = new MainPage();
                    //App.Current.MainPage = new AppShell();                
                }
                else
                {
                    noSalir = false;
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if(indice>=600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    noSalir = false;
                }
            }

            //var path = Path.Combine(Directory.GetCurrentDirectory(),
            //            "wwwroot/resources",
            //            "tmp",
            //            "android", "sqlite");

            //var dir = new DirectoryInfo(path);
            //FileInfo[] files = dir.GetFiles("*.json");
            
            //CreateZipFile(files, Path.Combine(path , jsonName + ".zip"));

            //var jsonBytes = ReadBytes(jsonFileName);
            //byte[] bytesXmlZip = CreateZip(jsonFileName, jsonBytes);

            //SaveZip(jsonName+ ".zip", bytesXmlZip);
            
            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        private bool PutInFile(string jsonFileName, string jsonName, string resultData)
        {
            //Iniciar el Json
            CreateJson(jsonFileName, @"{
                            """ + jsonName + @""":
                            [");

            //Debe terminar el proceso
            //if (resultData.final)
            //    noSalir = false;

            //var oldString = resultData;
            string newString = string.Join(" ", Regex.Split(resultData, @"(?:\r\n|\n|\r)"));
            newString = newString.Trim();
            newString = newString.Remove(0, 1);
            newString = newString.Remove(newString.Length - 1, 1);
            newString = newString.Trim();
            //Console.Write(newString);

            //if (indice > 1)
            //    newString = "," + newString;

            //Contenido de la página
            AppendToJson(jsonFileName, newString);

            //Finalizar el Json
            AppendToJson(jsonFileName, "]}");

            //if (indice == 10)
            //    noSalir = false;

            return true;
        }


        public bool PutInFile_v2(string current_model, string jsonFileName, string jsonName, string resultData, int year, int month, int day)
        {
            //Iniciar el Json
            CreateJson_v2(current_model, jsonFileName, @"{
                            """ + jsonName + @""":
                            [", 
                            year, 
                            month, 
                            day);

            //var oldString = resultData;
            string newString = string.Join(" ", Regex.Split(resultData, @"(?:\r\n|\n|\r)"));
            newString = newString.Trim();
            newString = newString.Remove(0, 1);
            newString = newString.Remove(newString.Length - 1, 1);
            newString = newString.Trim();
            //Console.Write(newString);

            //Contenido de la página
            AppendToJson_v2(current_model, jsonFileName, newString, year, month, day);

            //Finalizar el Json
            AppendToJson_v2(current_model, jsonFileName, "]}", year, month, day);

            return true;
        }

        public async Task<bool> ProcessResPartner(string jsonName, string actionName)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;            
            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token
            };

            int uid = 2;

            bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            //bool noSalir = true;

            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = uid;
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

            ApiManager.HubResPartner hubPartner = new ApiManager.HubResPartner(_appSession);            
            var resultCount = await hubPartner.GetCount(1,1,1);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / default_limit;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                apiRequest.uid = uid;
                //apiRequest.cadenaJson = cadenaJson;
                apiRequest.index = indice;
                apiRequest.update = esActualizacion;
                apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

                DateTime dateIni = DateTime.Parse(fechaActualizaTablet);
                //var responseAll = await hubPartner.GetSpecial(apiRequest);
                var responseAll = await hubPartner.GetByCreateDateRange(default_limit, indice, dateIni, dateIni);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                    if (resultData != null)
                    {
                        PutInFile(jsonFileName, jsonName, resultData);
                    }
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<bool> ProcessProductTemplate(string jsonName, string actionName)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;
            
            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                uid = int.Parse(appSetting.profile.Odoo.uid),
                databasename = appSetting.profile.Odoo.Database
            };

            int uid = 2;

            bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            //bool noSalir = true;

            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = uid;
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

            ApiManager.HubProductTemplate hubmanager = new ApiManager.HubProductTemplate(_appSession);
            var resultCount = await hubmanager.GetCount();

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / default_limit;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                apiRequest.uid = uid;
                //apiRequest.cadenaJson = cadenaJson;
                apiRequest.index = indice;
                apiRequest.update = esActualizacion;
                apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

                var responseAll = await hubmanager.GetItems(default_limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                    if (resultData != null)
                    {
                        PutInFile(jsonFileName, jsonName, resultData);
                    }
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }
        
        public async Task<bool> ProcessAccountMove(string jsonName, string actionName)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;
            
            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key
            };

            int uid = 2;

            bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            //bool noSalir = true;

            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = uid;
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;
            DateTime dateIni = DateTime.Parse(fechaActualizaTablet);

            ApiManager.HubAccountMove hubmanager = new ApiManager.HubAccountMove(_appSession);
            var resultCount = await hubmanager.GetHeaderCount(dateIni.Year, dateIni.Month, dateIni.Day);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / default_limit;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                apiRequest.uid = uid;
                //apiRequest.cadenaJson = cadenaJson;
                apiRequest.index = indice;
                apiRequest.update = esActualizacion;
                apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

                var responseAll = await hubmanager.GetAccountMoves(dateIni, default_limit, indice);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                    if (resultData != null)
                    {
                        PutInFile(jsonFileName, jsonName, resultData);
                    }                
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }


        private List<Tuple<int, int, int>> ConstruirListaFechas()
        {

            int limitBackYear = 2;

            List<Tuple<int, int, int>> fechas = new List<Tuple<int, int, int>>();

            // Fecha actual hasta ayer
            DateTime fechaActual = DateTime.Now.Date.AddDays(-1);

            // Hace dos años
            DateTime fechaInicio = fechaActual.AddYears(-limitBackYear).AddMonths(1).AddDays(-fechaActual.Day + 1);

            // Agregar todas las fechas desde hace dos años hasta ayer
            for (DateTime fecha = fechaInicio; fecha <= fechaActual; fecha = fecha.AddDays(1))
            {
                fechas.Add(new Tuple<int, int, int>(fecha.Year, fecha.Month, fecha.Day));
            }

            return fechas;
        }

        /// <summary>
        /// RequiredDataUpdate: Devuelve una lista de Años, meses y días, que representan los archivos inexistentes
        /// dentro de un rango de fecha determinado (2 años ej.), por lo que se asume que esa información debe ser
        /// extraida para crear sus respectivos archivos.
        /// </summary>
        /// <param name="current_model"></param>
        /// <returns>List<Tuple<int, int, int>></returns>
        private List<Tuple<int, int, int>> RequiredDataUpdateByFilesExists(string current_model, List<Tuple<int, int, int>> listaFechasReference)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            string publish_store = appSetting.profile.PublishStore;

            List<Tuple<int, int, int>> listaFechas = listaFechasReference;

            if(listaFechas == null)
            {
                listaFechas = ConstruirListaFechas();
            }            

            //foreach (var fechaUnica in listaFechas)
            //{
            //    fechas.RemoveAll(fecha => fecha.Item1 == fechaEncontrada.Item1 && fecha.Item2 == fechaEncontrada.Item2 && fecha.Item3 == fechaEncontrada.Item3);
            //}

            //int anios_retraso = 1;

            List<Tuple<int, int, int>> fechasEncontradas = new List<Tuple<int, int, int>>();

            //string current_model = "account_move";

            var directorio = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/resources",
                publish_store, 
                current_model);

            if(!Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            // Obtener todos los archivos zip en el directorio y subdirectorios
            string[] archivosZip = Directory.GetFiles(directorio, "*.zip", SearchOption.AllDirectories);

            foreach (string carpetaAnio in Directory.EnumerateDirectories(directorio))
            {
                string nombreAnio = Path.GetFileNameWithoutExtension(carpetaAnio);
                if (!int.TryParse(nombreAnio, out int anio))
                {
                    // Manejar error si el nombre del año no tiene el formato correcto
                    Console.WriteLine($"Error al procesar carpeta: {carpetaAnio}");
                    continue;
                }
                
                foreach (string carpetaMes in Directory.EnumerateDirectories(carpetaAnio))
                {
                    string nombreMes = Path.GetFileNameWithoutExtension(carpetaMes);
                    if (!int.TryParse(nombreMes, out int mes))
                    {
                        // Manejar error si el nombre del mes no tiene el formato correcto
                        continue;
                    }

                    foreach(string fileItem in Directory.EnumerateFiles(carpetaMes))
                    {
                        string nombreFile = Path.GetFileNameWithoutExtension(fileItem);

                        if (!int.TryParse(nombreFile, out int dia))
                        {                            
                            continue;
                        }

                        //Console.WriteLine($"{anio}-{mes:00}: {String.Join(", ", dia)}");
                        //Console.WriteLine($"{anio}-{mes:00}: {dia:00}");
                        fechasEncontradas.Add(new Tuple<int, int, int>(anio, mes, dia));
                    }
                }
            }

            // Mostrar las fechas encontradas
            //Console.WriteLine("Fechas encontradas:");
            //foreach (var fecha in fechasEncontradas)
            //{
            //    Console.WriteLine($"{fecha.Item1}-{fecha.Item2}-{fecha.Item3}");
            //}

            foreach (var fechaEncontrada in fechasEncontradas)
            {
                listaFechas.RemoveAll(fecha => fecha.Item1 == fechaEncontrada.Item1 && fecha.Item2 == fechaEncontrada.Item2 && fecha.Item3 == fechaEncontrada.Item3);
            }

            //Console.WriteLine("Fechas finales:");
            //foreach (var fecha in listaFechas)
            //{
            //    Console.WriteLine($"{fecha.Item1}-{fecha.Item2}-{fecha.Item3}");
            //}

            //Console.ReadLine();

            return listaFechas;
        }

        private (DateTime date, bool is_update) GetLastDate(string current_model)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            string publish_store = appSetting.profile.PublishStore;

            DateTime last_date = DateTime.Now;
            bool is_update = false;

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        publish_store,
                        "_bulk");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (File.Exists(Path.Combine(path, "" + current_model + "_base.json")))
            {
                var fileContent = File.ReadAllText(Path.Combine(path, "" + current_model + "_base.json"));
                update_pack_info? update_Pack_Info = JsonConvert.DeserializeObject<update_pack_info>(fileContent);

                if (update_Pack_Info != null)
                {
                    is_update = true;

                    if (File.Exists(Path.Combine(path, "" + current_model + "_update.json")))
                    {
                        var fileContentUpdate = File.ReadAllText(Path.Combine(path, "" + current_model + "_update.json"));
                        update_pack_info? update_Pack_Info_update = JsonConvert.DeserializeObject<update_pack_info>(fileContentUpdate);

                        if (update_Pack_Info_update != null)
                        {
                            //Existe Base y Update (*.json)
                            // se lee fecha desde archivo/clase
                            last_date = update_Pack_Info_update.pack_base_date;                            
                        }
                    }
                    else
                    {
                        //Solo existe Base
                        // se lee fecha desde servidor (previamente asignada)
                        //last_date = DateTime.Now;
                        last_date = update_Pack_Info.pack_base_date;
                    }
                }
            }

            return (last_date, is_update);
        }

        /// <summary>
        /// Step 3
        /// </summary>
        /// <param name="current_model"></param>
        /// <param name="last_date"></param>
        /// <returns></returns>
        private async Task<List<Tuple<int, int, int>>> RequiredDataUpdateByDataMods(string current_model, DateTime last_date)
        {
            //DateTime last_date = DateTime.Now;
            
            //var listaFechas = ConstruirListaFechas();

            List<Tuple<int, int, int>> fechasEncontradas = new List<Tuple<int, int, int>>();

            //TODO: Se esta usando fecha de prueba
            var responseDates = await EvalDataDates(current_model, last_date.Year, last_date.Month, last_date.Day, 0);
            //
            //var responseDates = await EvalDataDates(current_model, last_date.Year, 3, 1, 0);

            foreach (var item in responseDates)
            {
                fechasEncontradas.Add(new Tuple<int, int, int>(item.Year, item.Month, item.Day));
            }

            //Console.WriteLine("Fechas finales:");
            //foreach (var fecha in listaFechas)
            //{
            //    Console.WriteLine($"{fecha.Item1}-{fecha.Item2}-{fecha.Item3}");
            //}

            //Console.ReadLine();

            return fechasEncontradas;
        }


        private async Task<List<Tuple<int, int, int>>> RequiredDataCreatedByDataMods(string current_model, DateTime last_date)
        {
            //DateTime last_date = DateTime.Now;

            //var listaFechas = ConstruirListaFechas();

            List<Tuple<int, int, int>> fechasEncontradas = new List<Tuple<int, int, int>>();

            //TODO: Se esta usando fecha de prueba
            var responseDates = await EvalDataCreatedDates(current_model, last_date.Year, last_date.Month, last_date.Day, 0);
            
            foreach (var item in responseDates)
            {
                fechasEncontradas.Add(new Tuple<int, int, int>(item.Year, item.Month, item.Day));
            }

            //Console.WriteLine("Fechas finales:");
            //foreach (var fecha in listaFechas)
            //{
            //    Console.WriteLine($"{fecha.Item1}-{fecha.Item2}-{fecha.Item3}");
            //}

            //Console.ReadLine();

            return fechasEncontradas;
        }

        async Task<List<string>> ProcessChunks(List<Tuple<int, int, int>> fechasGetData, int chunkSize, string current_model, List<fileData> filesData)
        {
            var tasks = new List<Task<string>>();

            for (int chunkIndex = 0; chunkIndex < (fechasGetData.Count + chunkSize - 1) / chunkSize; chunkIndex++)
            {
                tasks.Add(ProcessChunk(fechasGetData, chunkSize, chunkIndex, current_model, filesData));
            }

            var resultsArray = await Task.WhenAll(tasks);
            return new List<string>(resultsArray);
        }

        async Task<string> ProcessChunk(List<Tuple<int, int, int>> fechasGetData, int chunkSize, int chunkIndex, string current_model, List<fileData> filesData)
        {
            int start = chunkIndex * chunkSize;
            int end = Math.Min(start + chunkSize, fechasGetData.Count);

            var tasks = new List<Task>();

            for (int i = start; i < end; i += 10)
            {
                var batchTasks = new List<Task>();

                for (int j = i; j < i + 10 && j < end; j++)
                {
                    var fecha = fechasGetData[j];
                    batchTasks.Add(ProcessFecha(fecha, current_model, filesData));
                }

                await Task.WhenAll(batchTasks);
            }

            return $"Chunk {chunkIndex} processed";
        }

        async Task ProcessFecha(Tuple<int, int, int> fecha, string current_model, List<fileData> filesData)
        {
            int year = fecha.Item1;
            int month = fecha.Item2;
            int day = fecha.Item3;

            Console.WriteLine($"Procesando datos de {year}-{month}-{day} modelo:{current_model}");

            bool forUpdate = false;
            var resultCount = await EvalCount(current_model, year, month, day, forUpdate);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return;
            }

            int countTotal = resultCount.result / 300;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                Console.WriteLine("Procesando mes " + month + " dia " + day.ToString() + " Página:" + indice);

                string jsonFileName = day.ToString() + "_" + indice.ToString() + ".json";
                var resultData = await EvalData(current_model, year, month, day, indice, forUpdate);
                //Console.WriteLine("ProcessFecha");
                //Console.WriteLine($"{current_model} {jsonFileName} {current_model} {resultData} {year} {month} {day}");

                if (resultData == null)
                {
                    Console.WriteLine("Error resultData NUll " + month + " dia " + day.ToString() + " Página:" + indice);
                    break;
                }

                PutInFile_v2(current_model, jsonFileName, current_model, resultData, year, month, day);
            }

            FileInfo fileinfo = ProcessZipGroup_v2(current_model, year, month, day);
            if (fileinfo != null) // si es nulo no existe el archivo
            {
                filesData.Add(new fileData()
                {
                    name = fileinfo.Name,
                    hash = CalculateFileHash(fileinfo.FullName),
                    create_date = fileinfo.CreationTime,
                    year = year,
                    month = month,
                    day = day
                });
            }
            else
            {
                Console.WriteLine("Error al obtener datos ProcessZipGroup_v2 ", current_model, year, month, day);
            }
        }

        [Obsolete]
        async Task<string> ProcessChunk__(List<Tuple<int, int, int>> fechasGetData, int chunkSize, int chunkIndex, string current_model, List<fileData> filesData)
        {
            int start = chunkIndex * chunkSize;
            int end = Math.Min(start + chunkSize, fechasGetData.Count);

            //for (int i = chunkIndex * chunkSize; i < (chunkIndex + 1) * chunkSize && i < fechasGetData.Count; i++)
            for (int i = start; i < end; i++)
            {
                var fecha = fechasGetData[i];

                // Año-Mes-día
                Console.WriteLine($"Procesando datos de {fecha.Item1}-{fecha.Item2}-{fecha.Item3} modelo:{current_model}");

                int year = fecha.Item1;
                int month = fecha.Item2;
                int day = fecha.Item3;

                bool forUpdate = false;

                var resultCount = await EvalCount(current_model, year, month, day, forUpdate);

                Debug.WriteLine(resultCount.result);

                if (resultCount.result == 0)
                {
                    continue;
                }

                int countTotal = resultCount.result / 300;

                for (int indice = 0; indice <= countTotal; indice++)
                {
                    string jsonFileName = day.ToString() + "_" + indice.ToString() + ".json";
                                       
                    var resultData = await EvalData(current_model, year, month, day, indice, forUpdate);
                    PutInFile_v2(current_model, jsonFileName, current_model, resultData, year, month, day);

                    Console.WriteLine("Página:" + indice);

                    //if (indice >= 600)
                    //{
                    //    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    //    break;
                    //}
                }

                FileInfo fileinfo = ProcessZipGroup_v2(current_model, year, month, day);
                filesData.Add(new fileData()
                {
                    name = fileinfo.Name,
                    hash = CalculateFileHash(fileinfo.FullName),
                    create_date = fileinfo.CreationTime,
                    year = year,
                    month = month,
                    day = day
                });
            }

            return $"Chunk {chunkIndex} processed";
        }
        
        AppSession GetAppSession()
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;
            
            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                    access_token = appSetting.profile.Odoo.access_token,
                    uid = int.Parse(appSetting.profile.Odoo.uid),
                    username = appSetting.profile.Odoo.User,
                    password = appSetting.profile.Odoo.Password,
                databasename = appSetting.profile.Odoo.Database
            };
            return _appSession;
        }

        string CalculateFileHash(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public async Task<string[]> ProcessModelByChunk(string current_model, string actionName)
        {            
            int limitBackYear = 2;
            
            //2 Años atras desde enero 1
            DateTime startDateReference = new DateTime(DateTime.Now.Date.AddYears(-limitBackYear).Year, 1, 1);
            var listFechaReference = await RequiredDataCreatedByDataMods(current_model, startDateReference);

            List<Tuple<int, int, int>> fechasGetData = RequiredDataUpdateByFilesExists(current_model, listFechaReference);

            var resultDateEval = GetLastDate(current_model);
            //Evalúa la fecha que debe tomar de referencia para la actualizacion
            DateTime last_date = resultDateEval.date;

            //Se vacia la lista de carpetas requeridas ya que si es actualizacion solo debe
            // procesar los datos nuevos
            if(resultDateEval.is_update && fechasGetData.Count == 0)
            {
                fechasGetData = new List<Tuple<int, int, int>>();
            }

            List<Tuple<int, int, int>> fechasGetByData = await RequiredDataUpdateByDataMods(current_model, last_date);
            
            HashSet<Tuple<int, int, int>> fechasSet = new HashSet<Tuple<int, int, int>>(fechasGetData);

            // Agregamos los elementos de fechasGetByData porque son registros de datos que han sido modificados
            // posterior a la fecha anterior de actualización, estos días deben considerarse para ser agregados 

            foreach (var fecha in fechasGetByData)
            {
                if (fechasSet.Add(fecha))
                {
                    fechasGetData.Add(fecha);
                }
            }

            DateTime dateTimeIni = DateTime.Now;

            int chunkSize = 10;

            List<fileData> filesData = new List<fileData>();

            var results = await ProcessChunks(fechasGetData, chunkSize, current_model, filesData);

            //foreach (var result in results)
            //{
            //    Console.WriteLine(result);
            //}

            CreateBulkInfoFile(current_model, filesData, resultDateEval.is_update);

            return results.ToArray();
        }

        private void CreateBulkInfoFile(string current_model, List<fileData> filesData, bool is_update)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            string publish_store = appSetting.profile.PublishStore;

            update_pack_info update_Pack_Info = new update_pack_info();
            update_Pack_Info.pack_base_date = DateTime.Now;
            update_Pack_Info.description = "Automático";
            
            update_Pack_Info.details = new List<Detail>()
            {                
                new Detail()
                {
                    model = current_model,
                    total_files = filesData.Count,
                    files = filesData.ToArray()
                }
            }.ToArray();

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        publish_store, 
                        "_bulk");

            string json_content = JsonConvert.SerializeObject(update_Pack_Info, Formatting.Indented);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filename = "" + current_model + "_base.json";

            if(is_update)
                filename = "" + current_model + "_update.json";

            File.WriteAllText(Path.Combine(path, filename),json_content);
        }

        public async Task<bool> ProcessAccountMoveLines(string jsonName, string actionName)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo; // "http://192.168.204.75:8069";
            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key
            };

            int uid = 2;

            bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            DateTime dateTimeIni = DateTime.Now;

            //Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            //bool noSalir = true;

            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = uid;
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;

            DateTime dateIni = DateTime.Parse(fechaActualizaTablet);
            ApiManager.HubAccountMoveLine hubmanager = new ApiManager.HubAccountMoveLine(_appSession);
            var resultCount = await hubmanager.GetDetailCount(dateIni);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                apiRequest.uid = uid;
                //apiRequest.cadenaJson = cadenaJson;
                apiRequest.index = indice;
                apiRequest.update = esActualizacion;                

                var responseAll = await hubmanager.GetAccountMoveLines(dateIni);

                if (responseAll.result != null && responseAll.result.Length > 0)
                {

                    //foreach(var lineItem in responseAll.data)
                    //{
                    //    if(lineItem.account_id != null && lineItem.account_id.Length > 0)
                    //    {
                    //        lineItem.accountId = lineItem.account_id[0].id;
                    //    }
                    //}

                    var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                    if (resultData != null)
                    {
                        PutInFile(jsonFileName, jsonName, resultData);
                    }
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<bool> ProcessAccountJournal(string jsonName, string actionName)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();

            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.ApiBaseAddressOdoo;
            
            _appSession.CurrentUser = new User()
            {
                api_key = appSetting.profile.Odoo.api_key,
                access_token = appSetting.profile.Odoo.access_token,
                username = appSetting.profile.Odoo.User,
                password = appSetting.profile.Odoo.Password,
                uid = 2, //appSetting.profile.Odoo.uid,
            };

            int uid = 2;

            bool esActualizacion = false;
            string fechaActualizaTablet = "2021-01-01 00:00:00";

            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());
                        
            ApiRequestOdoo_v1 apiRequest = new ApiRequestOdoo_v1();
            apiRequest.uid = uid;
            apiRequest.password = appSetting.profile.Odoo.Password;
            apiRequest.databasename = appSetting.profile.Odoo.Database;
            apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

            ApiManager.HubJournal hubmanager = new ApiManager.HubJournal(_appSession);
            string companies_ids = "1,5";
            var resultCount = await hubmanager.GetCount(companies_ids);

            Debug.WriteLine(resultCount.result);

            if (resultCount.result == 0)
            {
                return false;
            }

            int countTotal = resultCount.result / 300;

            for (int indice = 0; indice <= countTotal; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                apiRequest.uid = uid;
                //apiRequest.cadenaJson = cadenaJson;
                apiRequest.index = indice;
                apiRequest.update = esActualizacion;
                apiRequest.dateIni = DateTime.Parse(fechaActualizaTablet);

                var responseAll = await hubmanager.GetAccountJournal(companies_ids);

                if (responseAll != null && responseAll.result != null && responseAll.result.Length > 0)
                {
                    var resultData = Newtonsoft.Json.JsonConvert.SerializeObject(responseAll.result);

                    if (resultData != null)
                    {
                        PutInFile(jsonFileName, jsonName, resultData);
                    }
                }

                Console.WriteLine("Página:" + indice);

                //TODO: Se fuerza la salida para que no se quede ciclado en caso de que haya
                // problemas de conexion con el servidor
                // el objetivo es que el servidor no se sobrecargue

                if (indice >= 600)
                {
                    Console.WriteLine("Página " + indice + ": Se terminará el proceso.");
                    break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<bool> ProcessTableV3(string jsonName)
        {
            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            bool noSalir = true;

            //Se creará un archivo json por cada página
            // luego todos estos archivos json seran incluidos en el archivo zip

            NotaCreditoDetController notaCreditoDetController = new NotaCreditoDetController();

            for (int indice = 1; noSalir; indice++)
            {
                string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                var resultUser = await notaCreditoDetController.GetData(indice, 0, "");

                if (resultUser != null)
                {
                    if (resultUser.data != null)
                    {
                        //Iniciar el Json
                        CreateJson(jsonFileName, @"{
                            """ + jsonName + @""":
                            [");

                        //Debe terminar el proceso
                        //if (resultUser.final)
                        //    noSalir = false;

                        var oldString = Newtonsoft.Json.JsonConvert.SerializeObject(resultUser.data);
                        //var oldString = resultUser.data.ToString();
                        string newString = string.Join(" ", Regex.Split(oldString, @"(?:\r\n|\n|\r)"));
                        newString = newString.Trim();
                        newString = newString.Remove(0, 1);
                        newString = newString.Remove(newString.Length - 1, 1);
                        newString = newString.Trim();
                            
                        //Contenido de la página
                        AppendToJson(jsonFileName, newString);

                        //Finalizar el Json
                        AppendToJson(jsonFileName, "]}");

                        //if (indice == 10)
                            //noSalir = false;
                    }
                    else
                    {
                        noSalir = false;
                    }
                }
                else
                {
                    noSalir = false;
                }                

                Console.WriteLine("Página:" + indice);
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<ApiResponse_v2> getUsers()
        {
            string Action = "OBTENER_USUARIOS_WSJSON";
            string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string cadenaJson = "{\"fechatablet\":\"2023-06-19 11:11\"}";            

            List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

            parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
            parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));            

            //App.Current.MainPage = new MainPage();
            SoapClient client = new SoapClient();
            var result = await client.asyncPostJson(parameters.ToArray());
            //Console.WriteLine(result);

            if (result != "")
            {
                var listUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v2>(result);
                Debug.WriteLine("Okis");

                return listUsers;
                //var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonResponseObject>(result);
                //if (resultUser != null)
                //{
                //    if (resultUser.data != null)
                //    {
                //        //string oldString = resultUser.data.ToString();
                //        ////string newString = resultUser.data.ToString();
                //        //string newString = string.Join(" ", Regex.Split(oldString, @"(?:\r\n|\n|\r)"));
                //        //newString = newString.Trim();
                //        //newString = newString.Remove(0, 1);
                //        //newString = newString.Remove(newString.Length - 1, 1);
                //        //newString = newString.Trim();

                //        //var listUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse_v2>(newString);
                //        //Debug.WriteLine("Okis");
                //    }
                //}
            }

            return new ApiResponse_v2()
            {
                exito = false,
                data = new Models.DMSA.Shared.Security.UserSingle[0] { 
                }
            };
        }

        public async Task<bool> ProcessTableForAllUsers(string jsonName, string actionName)
        {
            DateTime dateTimeIni = DateTime.Now;

            Console.WriteLine("Iniciando proceso:" + jsonName + " " + DateTime.Now.ToString());

            //Se obtienen todos los usuarios activos

            var listUsers = await getUsers();
            if (listUsers.data != null && listUsers.data.Length > 0)
            {
                int indiceG = 1;

                foreach (var user in listUsers.data)
                {
                    bool noSalir = true;
                    Debug.WriteLine("Procesando usuario: " + user.codigo);
                    //Se creará un archivo json por cada página
                    // luego todos estos archivos json seran incluidos en el archivo zip

                    for (int indice = indiceG; noSalir; indice++)
                    {
                        indiceG++;

                        string jsonFileName = jsonName + "_" + indice.ToString() + ".json";

                        string Action = actionName;
                        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        string cadenaJson = "{\"indice\":" + indice.ToString() + ",\"fechaActualizaTablet\":\"2021-01-01 00:00:00\",\"esActualizacion\": true}";
                        // "{\"codusuario\":\"" + EntryUserName.Text + "\",\"codclave\":\"" + EntryPassword.Text + "\",\"fechatablet\":\"" + currentDateTime + "\"}";

                        List<RestSharp.Parameter> parameters = new List<RestSharp.Parameter>();

                        parameters.Add(RestSharp.Parameter.CreateParameter("accion", Action, ParameterType.QueryString));
                        parameters.Add(RestSharp.Parameter.CreateParameter("cadenaJson", cadenaJson, ParameterType.QueryString));
                        parameters.Add(RestSharp.Parameter.CreateParameter("codusuario", user.codigo, ParameterType.QueryString));

                        //App.Current.MainPage = new MainPage();
                        SoapClient client = new SoapClient();
                        var result = await client.asyncPostJson(parameters.ToArray());
                        //Console.WriteLine(result);


                        if (result != "")
                        {
                            var resultUser = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonResponseObject>(result);

                            if (resultUser != null)
                            {
                                if (resultUser.data != null)
                                {
                                    //Iniciar el Json
                                    CreateJson(jsonFileName, @"{
                                """ + jsonName + @""":
                                [");

                                    //Debe terminar el proceso
                                    if (resultUser.final)
                                        noSalir = false;

                                    var oldString = resultUser.data.ToString();
                                    string newString = string.Join(" ", Regex.Split(oldString, @"(?:\r\n|\n|\r)"));
                                    newString = newString.Trim();
                                    newString = newString.Remove(0, 1);
                                    newString = newString.Remove(newString.Length - 1, 1);
                                    newString = newString.Trim();

                                    //Contenido de la página
                                    AppendToJson(jsonFileName, newString);

                                    //Finalizar el Json
                                    AppendToJson(jsonFileName, "]}");

                                }
                                else
                                {
                                    noSalir = false;
                                }
                            }
                            else
                            {
                                noSalir = false;
                            }
                        }

                        Console.WriteLine("Página:" + indice);
                    }
                    //break;
                }
            }

            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        private void ProcessZipGroup(string jsonName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        "tmp",
                        "android", "sqlite");

            var dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles(jsonName+"*.json");

            if (files.Length > 0)
            {
                CreateZipFile(files, Path.Combine(path, jsonName + ".zip"));
                Console.WriteLine("Guardado:" + jsonName + ".zip" + " " + DateTime.Now.ToString());
            }
            else
            {
                Console.WriteLine("No encontrados:" + jsonName);
            }
        }

        public FileInfo ProcessZipGroup_v2(string current_model, int year, int month, int day)
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            string publish_store = appSetting.profile.PublishStore;

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        publish_store, 
                        current_model, 
                        year.ToString(), month.ToString(), day.ToString());

            var path_final = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        publish_store, 
                        current_model,
                        year.ToString(), month.ToString());

            if(!Path.Exists(path))
            {
                Console.WriteLine("No existe archivo " + path);
                return null;
            }

            var dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.json");

            if (files.Length > 0)
            {
                CreateZipFile(files, Path.Combine(path_final, day.ToString() + ".zip"));
                Console.WriteLine("Procesado:" + year.ToString() + "-" + month.ToString() + " " + DateTime.Now.ToString());
                Console.WriteLine("Guardado:" + day.ToString() + ".zip" + " " + DateTime.Now.ToString());

                //Eliminar archivos después de crear el zip
                //Console.WriteLine("Eliminando:" + path);
                Directory.Delete(path, true);
            }
            else
            {
                Console.WriteLine("No encontrados:" + day.ToString());
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(path_final, day.ToString() + ".zip"));
            return fileInfo;
        }

        private bool RemoveCacheFiles()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "resources",
                        "tmp",
                        "android", 
                        "sqlite");

            var dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.json");
            
            foreach ( FileInfo file in files )
            {
                if (File.Exists(file.FullName))
                    File.Delete(file.FullName);
            }

            return true;
        }

        public async Task<bool> SendRequest(ItemBuild buildItem)
        {
            if(buildItem.Name == "COBCARTERACAB" || buildItem.Name == "COBCARTERADET")
            {                
                await ProcessTableForAllUsers(buildItem.Name, buildItem.ActioName);
            }
            else
            {
                await ProcessTable(buildItem.Name, buildItem.ActioName);
            }

            ProcessZipGroup(buildItem.Name);
            RemoveCacheFiles();

            //await ProcessTable("FACNOTACREDITOCAB", "OBTENER_FAC_NOTACREDITO_CAB");
            //await ProcessTable("FACNOTACREDITODET", "OBTENER_FAC_NOTACREDITO_DET");
            //await ProcessTable("COBCARTERACAB", "OBTENER_CARTERA_CAB");
            //await ProcessTable("COBCARTERADET", "OBTENER_CARTERA_DET");

            //await ProcessTable("COBRECIBOCAB", "OBTENER_CARTERA_CAB");
            //await ProcessTable("COBCIERRE", "OBTENER_CARTERA_CAB");
            //await ProcessTable("GENMODULOSNC", "OBTENER_CARTERA_CAB");
            //await ProcessTable("GENTIPOSNOTACREDITO", "OBTENER_CARTERA_CAB");
            //await ProcessTable("SOLICITUDESNC", "OBTENER_CARTERA_CAB");
            //await ProcessTable("NCPARAMETROS", "OBTENER_CARTERA_CAB");
            //await ProcessTable("COBPARAMETROS", "OBTENER_CARTERA_CAB");
            //await ProcessTable("COBUSUARIOS", "OBTENER_CARTERA_CAB");
            return true;
        }

        public async Task<bool> SendRequestOdoo(ItemBuild buildItem)
        {
            //if (buildItem.Name == "COBCARTERACAB" || buildItem.Name == "COBCARTERADET")
            //{
            //    await ProcessTableForAllUsers(buildItem.Name, buildItem.ActioName);
            //}
            //else
            //{

            //}

            switch (buildItem.ActioName)
            {                
                case "account_move":
                    {
                        //await ProcessAccountMoveByChunk(buildItem.Name, buildItem.ActioName);
                        await ProcessAccountMove(buildItem.Name, buildItem.ActioName);
                    }
                    break;
                case "account_move_line":
                    {
                        await ProcessAccountMoveLines(buildItem.Name, buildItem.ActioName);
                    }
                    break;
                case "res_partner_build":
                    {
                        await ProcessResPartner(buildItem.Name, buildItem.ActioName);
                    }
                    break;
                case "account_journal_build":
                    {
                        await ProcessAccountJournal(buildItem.Name, buildItem.ActioName);
                    }
                    break;
                case "product_template_build":
                    {
                        await ProcessProductTemplate(buildItem.Name, buildItem.ActioName);
                    }
                    break;
                case "product_product_build":
                    {
                        //await ProcessProductProduct(buildItem.Name, buildItem.ActioName);
                    }
                    break;
            }

            ProcessZipGroup(buildItem.Name);
            RemoveCacheFiles();
            return true;
        }

        public async Task<bool> SendRequestOdooChunks(ItemBuild buildItem)
        {
            Console.WriteLine("Iniciando proceso:" + buildItem.Name + " " + DateTime.Now.ToString());
            
            await ProcessModelByChunk(buildItem.Name, buildItem.ActioName);

            Console.WriteLine("Finalizando proceso:" + buildItem.Name + " " + DateTime.Now.ToString());

            //ProcessZipGroup(buildItem.Name);
            //RemoveCacheFiles();
            //ClearSessions();
            return true;
        }

        public async Task<bool> SendRequestV2(ItemBuild buildItem)
        {           
            await ProcessTableV3(buildItem.Name);

            ProcessZipGroup(buildItem.Name);
            RemoveCacheFiles();
           
            return true;
        }

        ResponseAuthenticate responseUser;
        internal void setAuthentication(ResponseAuthenticate? _responseUser)
        {
            responseUser = _responseUser;
        }
    }
}