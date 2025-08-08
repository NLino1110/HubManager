using RestSharp;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Transactions;

namespace ResourceBuilder.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        public Task<WeatherForecast[]> GetForecastAsync(DateOnly startDate)
        {
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray());
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

        private byte[] ReadBytes(string filename)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "resources",
                        "tmp",
                        "android", "sqlite", filename);
            
            return File.ReadAllBytes(path);
        }

        private byte[] CreateZip(string AttachName, byte[] body)
        {
            using (var compressedFileStream = new MemoryStream())
            {
                //Create an archive and store the stream in memory.
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                {
                    //foreach (var caseAttachmentModel in caseAttachmentModels)
                    //{
                    //Create a zip entry for each attachment
                    var zipEntry = zipArchive.CreateEntry(AttachName);

                    //Get the stream of the attachment
                    using (var originalFileStream = new MemoryStream(body))
                    using (var zipEntryStream = zipEntry.Open())
                    {
                        //Copy the attachment stream to the zip entry stream
                        originalFileStream.CopyTo(zipEntryStream);
                    }
                    //}
                }

                return compressedFileStream.ToArray();
            }
        }

        private static void CreateZipFile(IEnumerable<FileInfo> files, string archiveName)
        {
            using (var stream = File.OpenWrite(archiveName))
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

                Console.WriteLine("Página:" + indice);
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(),
                        "wwwroot/resources",
                        "tmp",
                        "android", "sqlite");

            var dir = new DirectoryInfo(path);
            FileInfo[] files = dir.GetFiles("*.json");
            
            CreateZipFile(files, Path.Combine(path , jsonName + ".zip"));

            //var jsonBytes = ReadBytes(jsonFileName);
            //byte[] bytesXmlZip = CreateZip(jsonFileName, jsonBytes);

            //SaveZip(jsonName+ ".zip", bytesXmlZip);

            Console.WriteLine("Guardado:" + jsonName + ".zip" + " " + DateTime.Now.ToString());
            
            TimeSpan span = (DateTime.Now - dateTimeIni);

            Console.WriteLine(String.Format("Lapso transcurrido: {0} days, {1} hours, {2} minutes, {3} seconds",
                span.Days, span.Hours, span.Minutes, span.Seconds));

            return true;
        }

        public async Task<bool> SendRequest()
        {
            await ProcessTable("FACNOTACREDITOCAB", "OBTENER_FAC_NOTACREDITO_CAB");
            //await ProcessTable("FACNOTACREDITODET", "OBTENER_FAC_NOTACREDITO_DET");
            return true;
        }
    }
}