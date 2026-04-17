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
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;

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
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;            
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
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;
            
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
                
        AppSession GetAppSession()
        {
            var appSetting = ConfigurationHelper.GetAppSettings();
            AppSession _appSession = new AppSession();
            _appSession.odooConnection.Host = appSetting.profile.Odoo.Host;
            
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