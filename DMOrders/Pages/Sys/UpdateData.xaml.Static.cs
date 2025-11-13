using CommunityToolkit.Maui.Alerts;
using Newtonsoft.Json;
using RestSharp;
using CommunityToolkit.Maui.Core;
using DMSA.Models.Odoo.Origin;
using DMSA.Models.Odoo.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMOrders.Services.Update;
using DMOrders.Services.Database.Sqlite;
using System.Diagnostics;
using DMSA.Models.Odoo.Native;

namespace DMOrders.Pages.Sys
{
    public partial class UpdateData
    {

        private async Task LaunchCacheModeByChunks(ProgressBarAnimationBehaviorPage obj,
            IToast toast,
            ToastDuration duration,
            double fontSize,
            CancellationTokenSource cancellationTokenSource)
        {
            string textToast = "Actualización por cache iniciada...";
            await Toast.Make(textToast, duration, fontSize).Show();
            //Se crea el directorio para descargas
            string DeviceStorage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "tmp");
            System.IO.Directory.CreateDirectory(DeviceStorage);

            obj.SetTitle("Iniciando descarga de archivos de cache...");

            string[] models = {
            "account_move",
            "account_move_line",
            "account_journal",
            "res_partner",
            "product_template",
            "product_product",
        };

            if (Directory.Exists(Path.Combine(DeviceStorage, "_bulk")))
            {
                Directory.Delete(Path.Combine(DeviceStorage, "_bulk"), true);
            }

            Directory.CreateDirectory(Path.Combine(DeviceStorage, "_bulk"));

            if (!Directory.Exists(Path.Combine(DeviceStorage, "_bulk_data")))
            {
                Directory.CreateDirectory(Path.Combine(DeviceStorage, "_bulk_data"));
            }

            foreach (string current_model in models)
            {
                obj.SetSubTitle("Leyendo informacion de " + current_model);

                try
                {
                    string base_filename = "_bulk/" + current_model + "_base.json";
                    string update_filename = "_bulk/" + current_model + "_update.json";

                    await DownloadResource(obj,
                        toast,
                        duration,
                        fontSize,
                        cancellationTokenSource,
                        DeviceStorage,
                        base_filename);

                    await DownloadResource(obj,
                        toast,
                        duration,
                        fontSize,
                        cancellationTokenSource,
                        DeviceStorage,
                        update_filename);

                    //Procesar lista de archivos para ser descargados

                    update_pack_info update_Pack_Info_base = null;
                    update_pack_info update_Pack_Info_data = null;

                    if (File.Exists(Path.Combine(DeviceStorage, base_filename)))
                    {
                        string base_filename_content = File.ReadAllText(Path.Combine(DeviceStorage, base_filename));

                        update_Pack_Info_base = JsonConvert.DeserializeObject<update_pack_info>(base_filename_content);

                        if (File.Exists(Path.Combine(DeviceStorage, update_filename)))
                        {
                            string update_filename_content = File.ReadAllText(Path.Combine(DeviceStorage, update_filename));
                            if (update_filename_content != "")
                            {
                                update_pack_info update_Pack_Info_update = JsonConvert.DeserializeObject<update_pack_info>(update_filename_content);
                                UpdateFiles(update_Pack_Info_update, update_Pack_Info_base);
                            }
                        }
                    }

                    bool ShouldCreateFile = false;
                    if (ShouldCreateFile || !File.Exists(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json")))
                    {
                        update_pack_info update_Pack_Info_update_new = new update_pack_info();
                        update_Pack_Info_update_new.pack_base_date = DateTime.Now;
                        update_Pack_Info_update_new.description = update_Pack_Info_base.description;
                        update_Pack_Info_update_new.details = new Detail[1] {
                        new Detail()
                        {
                            files = new fileData[0],
                            model = current_model,
                            total_files = 0
                        }
                    };
                        //update_Pack_Info_update_new.details[0].files = ;
                        //Se crea archivo de dato para indicar la ultima actualizacion
                        //if (!File.Exists(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json")))
                        //{
                        //Sino existe o si es una actualizacion forzada se almacena nuevo
                        string base_filename_content_mixed = JsonConvert.SerializeObject(update_Pack_Info_update_new);
                        File.WriteAllBytes(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json"),
                            Encoding.ASCII.GetBytes(base_filename_content_mixed));
                        //}
                    }

                    string data_base_filename_content = File.ReadAllText(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json"));
                    update_Pack_Info_data = JsonConvert.DeserializeObject<update_pack_info>(data_base_filename_content);

                    update_pack_info update_Pack_Info_data_tmp = JsonConvert.DeserializeObject<update_pack_info>(data_base_filename_content);

                    UpdateFiles(update_Pack_Info_base, update_Pack_Info_data);

                    //Hubo cambios (solo se comparan los detalles)
                    if (update_Pack_Info_data.details != update_Pack_Info_data_tmp.details)
                    {
                        update_Pack_Info_data.pack_base_date = DateTime.Now;
                        //Se vuelve a guardar el archivo de datos locales con los nuevos datos de actualización
                        string base_filename_content_mixed_new = JsonConvert.SerializeObject(update_Pack_Info_data);
                        File.WriteAllBytes(Path.Combine(DeviceStorage, "_bulk_data/" + current_model + "_base.json"),
                            Encoding.ASCII.GetBytes(base_filename_content_mixed_new));
                    }

                    //Solo se intentarán actualizar los archivos diferenciados
                    var distinctFiles = GetDistinctUpdatePackInfo(update_Pack_Info_data_tmp, update_Pack_Info_data);

                    //Console.WriteLine(distinctFiles);

                    //Listado de archivos que serán actualizados
                    //Download
                    foreach (var detail in distinctFiles.details)
                    {
                        foreach (var fileItem in detail.files)
                        {
                            try
                            {
                                string finalFileUrl = detail.model + "/" +
                                    fileItem.year.ToString() + "/" +
                                    fileItem.month.ToString() + "/" +
                                    fileItem.name.ToString() + "";

                                //Descarga/Descrompresión/Insersión de cada uno de los archivos de la lista
                                await DownloadResource(obj,
                                    toast,
                                    duration,
                                    fontSize,
                                    cancellationTokenSource,
                                    DeviceStorage + "/_zip",
                                    finalFileUrl);
                            }
                            catch (Exception ex)
                            {
                                textToast = "Error download:" + ex.Message;
                                toast = Toast.Make(textToast, duration, fontSize);
                                await toast.Show(cancellationTokenSource.Token);

                                //Se elimina el archivo que provocó el error
                                //update_Pack_Info_data.

                                continue;
                            }
                        }
                    }

                    ServerPuller serverPuller = new ServerPuller();

                    //Extract
                    foreach (var detail in distinctFiles.details)
                    {
                        int account_move_count = 0;
                        int account_move_line_count = 0;
                        int res_partner_count = 0;
                        int account_journal_count = 0;
                        int product_template_count = 0;
                        int product_product_count = 0;

                        foreach (var fileItem in detail.files)
                        {
                            string finalFileUrl = detail.model + "/" +
                                fileItem.year.ToString() + "/" +
                                fileItem.month.ToString() + "/" +
                                fileItem.name.ToString() + "";

                            string DeviceStorageJson = Path.Combine(DeviceStorage, "_extract", finalFileUrl + "_tmp");

                            var ListFiles = await ExtractZipFile(DeviceStorage, finalFileUrl, toast, duration, fontSize, cancellationTokenSource);

                            //Console.WriteLine(ListFiles);

                            switch (detail.model)
                            {
                                case "account_move":
                                    {
                                        if (account_move_count == 0)
                                        {
                                            var database = new AccountMoveDb();
                                            await database.Truncate();
                                        }
                                        await ProcAccountMove(obj, ListFiles);
                                        account_move_count++;
                                    }
                                    break;
                                case "account_move_line":
                                    {
                                        if (account_move_line_count == 0)
                                        {
                                            var database = new AccountMoveLineDb();
                                            await database.Truncate();
                                        }

                                        await ProcAccountMoveLine(obj, ListFiles);

                                        account_move_line_count++;
                                    }
                                    break;
                                case "res_partner":
                                    {
                                        if (res_partner_count == 0)
                                        {
                                            var database = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
                                            await database.Truncate();
                                        }
                                        await ProcPartner(obj, ListFiles);
                                        res_partner_count++;
                                    }
                                    break;
                                case "account_journal":
                                    {
                                        if (account_journal_count == 0)
                                        {
                                            var database = new AccountJournalDb();
                                            await database.Truncate();
                                        }
                                        await ProcAccountJournal(obj, ListFiles);
                                        account_journal_count++;
                                    }
                                    break;
                                case "product_template":
                                    {
                                        if (product_template_count == 0)
                                        {
                                            var database = new ProductTemplateDb(App.Session.odooConnection.DbNameSqlite);
                                            await database.Truncate();
                                        }
                                        await ProcProducts(obj, ListFiles);
                                        product_template_count++;
                                    }
                                    break;
                                case "product_product":
                                    {
                                        if (product_product_count == 0)
                                        {
                                            var database = new ProductProductDb(App.Session.odooConnection.DbNameSqlite);
                                            await database.Truncate();
                                        }
                                        await serverPuller.ProcProductProduct(obj, ListFiles);
                                        product_product_count++;
                                    }
                                    break;
                            }

                            //Eliminar carpeta descomprimida temporal
                            Directory.Delete(DeviceStorageJson, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    textToast = "Error download:" + ex.Message;
                    toast = Toast.Make(textToast, duration, fontSize);
                    await toast.Show(cancellationTokenSource.Token);
                    continue;
                }
            }
        }

        private async Task ProcAccountJournal(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new AccountJournalDb();

            //if (ListFiles.Length > 0)
            //{
            //    await database.Truncate();
            //}

            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
                string jsonFileItem = File.ReadAllText(fileNameJson);
                var listObjects = JsonConvert.DeserializeObject<AccountJournalOrigin>(jsonFileItem);

                int totalItems = listObjects.account_journal.Length;
                int curIndex = 1;
                double percentProcess = 0;

                try
                {
                    //listObjects.account_journal = FixAccountMove(listObjects.account_journal);
                    //await database.InsertBatchAsync(listObjects.account_journal);
                    //await database.UpdateAllAsync(listObjects.account_journal);

                    List<string> accounts_journal_ids_list = new List<string>();
                    InboundPaymentMethodDb inboundPaymentMethodDb = new InboundPaymentMethodDb();
                    await inboundPaymentMethodDb.TruncateAsync();

                    foreach (var itemData in listObjects.account_journal)
                    {
                        //Se evalúa si debe usarse en la app
                        if (!itemData.use_mobile_app)
                        {
                            var isForApp = itemData.mobile_app_tag_ids.Where(i => i.code == App.Session.AppCodeOdoo).FirstOrDefault();
                            if (isForApp != null)
                            {

                            }
                            else
                            {
                                continue;
                            }
                        }

                        //accountJournalDb.InsertAsync(itemData);
                        itemData.BankAccountId = 0;
                        if (itemData.bank_account_id.Count > 0)
                        {
                            itemData.BankAccountId = itemData.bank_account_id.FirstOrDefault().id;

                            //Se agrega a la lista
                            accounts_journal_ids_list.Add(itemData.bank_account_id.FirstOrDefault().id.ToString());
                        }

                        itemData.CompanyId = 0;
                        if (itemData.company_id.Count > 0)
                        {
                            itemData.CompanyId = itemData.company_id.FirstOrDefault().id;
                        }

                        if (itemData.inbound_payment_method_line_ids.Count > 0)
                        {
                            itemData.inbound_payment_method_line_ids.ForEach(x => x.parent_id = itemData.id);
                            await inboundPaymentMethodDb.InsertBatchAsync(itemData.inbound_payment_method_line_ids.ToArray());
                        }

                        //Solo se insertarán las cuentas que tengan habilitado su uso en las apps móviles
                        await database.InsertAsync(itemData);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Error: provocado por " + fileNameJson);
                    Debug.WriteLine("Error: " + listObjects.account_journal);
                }

                fileIndex++;
            }
        }

        private async Task ProcAccountMoveLine(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new AccountMoveLineDb();
            //if (ListFiles.Length > 0)
            //{
            //    await database.Truncate();
            //}
            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                //database = null;
                //database = new FacNotaCreditoDetDb();
                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
                string jsonFileItem = File.ReadAllText(fileNameJson);
                var listObjects = JsonConvert.DeserializeObject<AccountMoveLineOrigin>(jsonFileItem);

                int totalItems = listObjects.account_move_line.Length;
                int curIndex = 1;
                double percentProcess = 0;

                try
                {
                    foreach (var amlItem in listObjects.account_move_line)
                    {
                        amlItem.productId = get_from_token(amlItem.product_id);
                        amlItem.accountId = get_from_token(amlItem.account_id);
                        amlItem.moveId = get_from_token(amlItem.move_id);

                        //if (amlItem.product_id != null && amlItem.product_id.Length > 0)
                        //{
                        //    amlItem.productId = amlItem.product_id[0].id;
                        //}

                        //if (amlItem.account_id != null && amlItem.account_id.Length > 0)
                        //{
                        //    amlItem.accountId = amlItem.account_id[0].id;
                        //}

                        //if (amlItem.move_id != null && amlItem.move_id.Length > 0)
                        //{
                        //    amlItem.moveId = amlItem.move_id[0].id;
                        //}
                    }

                    await database.InsertBatchAsync(listObjects.account_move_line);
                    //await database.UpdateAllAsync(listObjects.FACNOTACREDITODET);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Error: provocado por " + fileNameJson);
                    Debug.WriteLine("Error: " + listObjects.account_move_line);
                    //Se inicia el intento de insersión individual

                    //TODO: Crear metodo para esta clase
                    //await TryIndividualInsertAsync(listObjects.account_move);
                }

                fileIndex++;
            }
        }

        private async Task ProcAccountMove(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new AccountMoveDb();
            //if (ListFiles.Length > 0)
            //{
            //    await database.Truncate();
            //}
            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                //database = null;
                //database = new FacNotaCreditoDetDb();
                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
                string jsonFileItem = File.ReadAllText(fileNameJson);
                var listObjects = JsonConvert.DeserializeObject<AccountMoveOrigin>(jsonFileItem);

                int totalItems = listObjects.account_move.Length;
                int curIndex = 1;
                double percentProcess = 0;

                try
                {
                    listObjects.account_move = FixAccountMove(listObjects.account_move);

                    await database.InsertBatchAsync(listObjects.account_move);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Error: provocado por " + fileNameJson);
                    Debug.WriteLine("Error: " + listObjects.account_move);
                }

                fileIndex++;
            }
        }

        private async Task ProcPartner(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new ResPartnerDb(App.Session.odooConnection.DbNameSqlite);
            //if (ListFiles.Length > 0)
            //{
            //    await database.Truncate();
            //}
            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                //database = null;
                //database = new FacNotaCreditoDetDb();
                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
                string jsonFileItem = File.ReadAllText(fileNameJson);
                var listObjects = JsonConvert.DeserializeObject<PartnerOrigin>(jsonFileItem);

                int totalItems = listObjects.res_partner.Length;
                int curIndex = 1;
                double percentProcess = 0;

                try
                {
                    List<res_partner> res_partnerFinal = new List<res_partner>();

                    foreach (var partnerItem in listObjects.res_partner)
                    {
                        if (partnerItem.vat != null && partnerItem.vat.Trim().Equals("false"))
                        {
                            partnerItem.vat = "";
                        }

                        if (partnerItem.email != null && partnerItem.email.Trim().Equals("false"))
                        {
                            partnerItem.email = "";
                        }

                        if (partnerItem.street != null && partnerItem.street.Trim().Equals("false"))
                        {
                            partnerItem.street = "";
                        }

                        if (partnerItem.street2 != null && partnerItem.street2.Trim().Equals("false"))
                        {
                            partnerItem.street2 = "";
                        }

                        if (partnerItem.city != null && partnerItem.city.Trim().Equals("false"))
                        {
                            partnerItem.city = "";
                        }

                        res_partnerFinal.Add(partnerItem);

                    }

                    //res_partner[] res_partner
                    //await database.InsertBatchAsync(listObjects.res_partner);
                    await database.InsertBatchAsync(res_partnerFinal.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Error: provocado por " + fileNameJson);
                    Debug.WriteLine("Error: " + listObjects.res_partner);
                }

                fileIndex++;
            }
        }

        private async Task ProcProducts(ProgressBarAnimationBehaviorPage obj, string[] ListFiles)
        {
            var database = new ProductTemplateDb(App.Session.odooConnection.DbNameSqlite);
            //if (ListFiles.Length > 0)
            //{
            //    await database.Truncate();
            //}
            int fileIndex = 1;

            foreach (var fileNameJson in ListFiles)
            {
                Debug.WriteLine("Procesando archivo de cache:");
                Debug.WriteLine(fileNameJson);

                obj.SetTitle($"Proc. {Path.GetFileName(fileNameJson)} ({fileIndex}/{ListFiles.Length})");

                //string jsonFileItem = File.ReadAllText(Path.Combine(DeviceStorageJson, ZipFileName.Replace(".zip", ".json")));
                string jsonFileItem = File.ReadAllText(fileNameJson);
                var listObjects = JsonConvert.DeserializeObject<ProductTemplateOrigin>(jsonFileItem);

                int totalItems = listObjects.product_template.Length;
                int curIndex = 1;
                double percentProcess = 0;

                try
                {
                    List<product_template> res_partnerFinal = new List<product_template>();

                    res_partnerFinal = listObjects.product_template.ToList();

                    await database.InsertBatchAsync(res_partnerFinal.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine("Error: provocado por " + fileNameJson);
                    Debug.WriteLine("Error: " + listObjects.product_template);
                }

                fileIndex++;
            }
        }

        private List<Tuple<int, int, int>> RequiredDataUpdate(string current_model)
        {
            var listaFechas = ConstruirListaFechas();

            //foreach (var fechaUnica in listaFechas)
            //{
            //    fechas.RemoveAll(fecha => fecha.Item1 == fechaEncontrada.Item1 && fecha.Item2 == fechaEncontrada.Item2 && fecha.Item3 == fechaEncontrada.Item3);
            //}

            //int anios_retraso = 1;

            List<Tuple<int, int, int>> fechasEncontradas = new List<Tuple<int, int, int>>();

            //string current_model = "account_move";

            var directorio = Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot/resources",
                "tmp",
                "android", "sqlite", current_model);

            if (!Directory.Exists(directorio))
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

                    foreach (string fileItem in Directory.EnumerateFiles(carpetaMes))
                    {
                        string nombreFile = Path.GetFileNameWithoutExtension(fileItem);

                        if (!int.TryParse(nombreFile, out int dia))
                        {
                            continue;
                        }

                        //Console.WriteLine($"{anio}-{mes:00}: {String.Join(", ", dia)}");
                        Console.WriteLine($"{anio}-{mes:00}: {dia:00}");
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

        public static update_pack_info GetDistinctUpdatePackInfo(update_pack_info source, update_pack_info destination)
        {
            var distinctUpdatePackInfo = new update_pack_info
            {
                pack_base_date = DateTime.Now,
                description = "Distinct Files Update",
                details = new List<Detail>().ToArray()
            };

            var allModels = source.details.Select(d => d.model).Union(destination.details.Select(d => d.model)).Distinct();
            var distinctDetails = new List<Detail>();

            foreach (var model in allModels)
            {
                var sourceDetail = source.details.FirstOrDefault(d => d.model == model);
                var destDetail = destination.details.FirstOrDefault(d => d.model == model);

                var sourceFiles = sourceDetail?.files ?? Array.Empty<fileData>();
                var destFiles = destDetail?.files ?? Array.Empty<fileData>();

                var distinctFiles = new List<fileData>();

                foreach (var sourceFile in sourceFiles)
                {
                    var destFile = destFiles.FirstOrDefault(f =>
                        f.name == sourceFile.name &&
                        f.year == sourceFile.year &&
                        f.month == sourceFile.month &&
                        f.day == sourceFile.day);

                    if (destFile == null ||
                        destFile.create_date != sourceFile.create_date ||
                        destFile.hash != sourceFile.hash)
                    {
                        distinctFiles.Add(sourceFile);
                    }
                }

                foreach (var destFile in destFiles)
                {
                    var sourceFile = sourceFiles.FirstOrDefault(f =>
                        f.name == destFile.name &&
                        f.year == destFile.year &&
                        f.month == destFile.month &&
                        f.day == destFile.day);

                    if (sourceFile == null ||
                        sourceFile.create_date != destFile.create_date ||
                        sourceFile.hash != destFile.hash)
                    {
                        distinctFiles.Add(destFile);
                    }
                }

                if (distinctFiles.Any())
                {
                    distinctDetails.Add(new Detail
                    {
                        model = model,
                        total_files = distinctFiles.Count,
                        files = distinctFiles.ToArray()
                    });
                }
            }

            distinctUpdatePackInfo.details = distinctDetails.ToArray();
            return distinctUpdatePackInfo;
        }


        public static void UpdateFiles(update_pack_info source, update_pack_info destination)
        {
            foreach (var sourceDetail in source.details)
            {
                var destDetail = destination.details.FirstOrDefault(d => d.model == sourceDetail.model);
                if (destDetail != null)
                {
                    var updatedFiles = destDetail.files.ToList();
                    foreach (var sourceFile in sourceDetail.files)
                    {
                        var destFile = updatedFiles.FirstOrDefault(f => f.name == sourceFile.name &&
                        f.year == sourceFile.year &&
                        f.month == sourceFile.month &&
                        f.day == sourceFile.day);
                        if (destFile != null)
                        {
                            if (destFile.create_date != sourceFile.create_date || destFile.hash != sourceFile.hash)
                            {
                                // Update destination file data
                                destFile.create_date = sourceFile.create_date;
                                destFile.hash = sourceFile.hash;
                                destFile.year = sourceFile.year;
                                destFile.month = sourceFile.month;
                                destFile.day = sourceFile.day;
                            }
                        }
                        else
                        {
                            // Add new file data to destination
                            updatedFiles.Add(sourceFile);
                        }
                    }
                    destDetail.files = updatedFiles.ToArray();
                    destDetail.total_files = destDetail.files.Length;
                }
                else
                {
                    // Add new detail to destination
                    var updatedDetails = destination.details.ToList();
                    updatedDetails.Add(sourceDetail);
                    destination.details = updatedDetails.ToArray();
                }
            }
        }

        private account_move[] FixAccountMove(account_move[] account_Moves)
        {
            foreach (var amItem in account_Moves)
            {
                if (amItem.reversed_entry_id != null && amItem.reversed_entry_id.Length > 0)
                {
                    amItem._reversed_entry_id = amItem.reversed_entry_id[0].id;
                }

                if (amItem.partner_id != null && amItem.partner_id.Length > 0)
                {
                    amItem._partner_id = amItem.partner_id[0].id;
                }

                if (amItem.journal_id != null && amItem.journal_id.Length > 0)
                {
                    amItem._journal_id = amItem.journal_id[0].id;
                }

                if (amItem.l10n_latam_document_type_id != null && amItem.l10n_latam_document_type_id.Length > 0)
                {
                    amItem._l10n_latam_document_type_id = amItem.l10n_latam_document_type_id[0].id;
                }

                if (amItem.invoice_user_id != null && amItem.invoice_user_id.Length > 0)
                {
                    amItem._invoice_user_id = amItem.invoice_user_id[0].id;
                }

                if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                {
                    amItem._printer_id = amItem.printer_id[0].id;
                }

                if (amItem.printer_id != null && amItem.printer_id.Length > 0)
                {
                    amItem._printer_id = amItem.printer_id[0].id;
                }

                if (amItem.company_id != null && amItem.company_id.Length > 0)
                {
                    amItem._company_id = amItem.company_id[0].id;
                }

                if (amItem.team_id != null && amItem.team_id.Length > 0)
                {
                    amItem._team_id = amItem.team_id[0].id;
                }
            }

            return account_Moves;
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

    }
}
