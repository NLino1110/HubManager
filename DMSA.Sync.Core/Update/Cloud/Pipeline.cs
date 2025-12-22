using ApiManager;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Specials;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMSA.Sync.Core.Update.Cloud
{
    public class Pipeline
    {
        public async Task<string> CompressDatabaseAsync_Old(string dbPath)
        {
            string zipPath = dbPath + ".zip";

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(
                    dbPath,
                    Path.GetFileName(dbPath),
                    CompressionLevel.SmallestSize
                );
            }

            return zipPath;
        }

        public async Task<string> CompressDatabaseAsync(string dbPath)
        {
            if (!File.Exists(dbPath))
                throw new FileNotFoundException("No se encontró la base de datos", dbPath);

            string tempCopyPath = dbPath + ".tmp";
            string zipPath = dbPath + ".zip";

            // Eliminar restos previos
            if (File.Exists(tempCopyPath))
                File.Delete(tempCopyPath);

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            // 1️⃣ Copiar archivo SIN bloquear el original
            using (var source = new FileStream(
                dbPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            using (var dest = new FileStream(
                tempCopyPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None))
            {
                await source.CopyToAsync(dest);
            }

            // 2️⃣ Comprimir la copia
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(
                    tempCopyPath,
                    Path.GetFileName(dbPath),
                    CompressionLevel.SmallestSize
                );
            }

            // 3️⃣ Limpiar temporal
            File.Delete(tempCopyPath);

            return zipPath;
        }


        public async Task UploadSqliteZip_old()
        {            
            string dbPath = FileSystem.AppDataDirectory;
            dbPath = Path.Combine(dbPath, Constants.Session.odooConnection.DbNameSqlite);
            //creamos comprimido de la base de datos
            Debug.WriteLine($"Comenzando compresion de la base de datos: {dbPath}");
            string zipPath = await CompressDatabaseAsync(dbPath);
            Debug.WriteLine($"Base de datos comprimida en: {zipPath}");

            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);
            Debug.WriteLine("Iniciando envio del archivo comprimido al servidor...");
            byte[] fileBytes = await File.ReadAllBytesAsync(zipPath);

            string uploadFileName = $"{Constants.Session.odooConnection.DbNameSqlite}.zip";

            mnsa_attachment mnsaAttachment = new mnsa_attachment()
            {
                file_name = uploadFileName,                
                file_type = "application/zip",
                date_data_cutoff = DateTime.UtcNow,
                mobile_app_id = Constants.Session.AppMobileId,
            };

            var responseData = await hubMnsaAttachment.CreatePackage(mnsaAttachment);

            Debug.WriteLine("Archivo enviado correctamente al servidor.");

            if (responseData.result > 0)
            {
                Debug.WriteLine($"Respuesta recibida del servidor. Tamaño: {responseData}");
            }
            else
            {
                Debug.WriteLine("No se recibió respuesta del servidor o el tamaño es cero.");
            }

        }

        const int MAX_PART_SIZE = (int)(3.14 * 1024 * 1024);

        public async Task UploadSqliteZip()
        {
            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                Constants.Session.odooConnection.DbNameSqlite
            );

            Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
            string zipPath = await CompressDatabaseAsync(dbPath);

            byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);

            Debug.WriteLine($"ZIP generado. Tamaño total: {zipBytes.Length / 1024 / 1024.0:F2} MB");

            HubMnsaAttachment hub = new HubMnsaAttachment(Constants.Session);

            mnsa_attachment mnsaAttachment = new mnsa_attachment()
            {
                file_name = Path.GetFileName(zipPath),
                file_type = "application/zip",
                date_data_cutoff = DateTime.UtcNow,
                mobile_app_id = Constants.Session.AppMobileId,
            };

            // 🔹 1️⃣ Crear paquete lógico (mnsa.package)
            var responseSend = await hub.CreatePackage(mnsaAttachment);

            int packageId = responseSend.result;

            Debug.WriteLine($"Paquete creado con ID: {packageId}");

            // 🔹 2️⃣ Partir el ZIP
            var parts = SplitFile(zipBytes, MAX_PART_SIZE).ToList();
            int totalParts = parts.Count;

            Debug.WriteLine($"Archivo dividido en {totalParts} partes");

            // 🔹 3️⃣ Subir cada parte como ir.attachment
            for (int i = 0; i < totalParts; i++)
            {
                string partName = $"{Constants.Session.odooConnection.DbNameSqlite}.part{i + 1:D2}.zip";

                Debug.WriteLine($"Subiendo {partName} ({i + 1}/{totalParts})");

                var file_upload_response = await hub.SendAttachment(new ir_attachment()
                {
                    name = partName,
                    datas = Convert.ToBase64String(parts[i]),
                    mimetype = "application/zip",
                    res_field = "attachment_ids",
                    res_model = "mnsa.attachment",
                    res_id = packageId
                });

                Debug.WriteLine($"Parte {partName} subida con ID: {file_upload_response.result}");


                await hub.Link(packageId, file_upload_response.result);
            }

            Debug.WriteLine("✅ Todas las partes enviadas correctamente");
        }

        public static IEnumerable<byte[]> SplitFile(byte[] fileBytes, int chunkSize)
        {
            int offset = 0;

            while (offset < fileBytes.Length)
            {
                int size = Math.Min(chunkSize, fileBytes.Length - offset);
                byte[] chunk = new byte[size];
                Buffer.BlockCopy(fileBytes, offset, chunk, 0, size);
                offset += size;
                yield return chunk;
            }
        }



        public async Task<bool> AvailableZipPack()
        {
            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

            var top5List = await hubMnsaAttachment.GetTop5();

            if (top5List != null && top5List.result != null && top5List.result.Length > 0)
            {
                return true;
            }

            return false;
        }

        public async Task DownloadSqliteZip()
        {
            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);
            HubIrAttachment hubIrAttachment = new HubIrAttachment(Constants.Session);

            var top5List = await hubMnsaAttachment.GetTop5();

            if(top5List != null && top5List.result!=null && top5List.result.Length > 0)
            {
                var item_first = top5List.result[0];

                var attachmentIds = item_first._attachment_ids
                    .OrderBy(id => id)
                    .ToList();

                string originalName = item_first.file_name;
                string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
                string ext = Path.GetExtension(originalName);

                string randomSuffix = Guid.NewGuid().ToString("N"); // sin guiones

                string tempZipPath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    $"{nameWithoutExt}_{randomSuffix}{ext}"
                );

                using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
                {
                    foreach (var item in attachmentIds)
                    {
                        //int file_size = (item_first.file_size / (1024 * 2));
                        //Debug.WriteLine($"Descargando archivo: {item_first.file_name} de tamaño {file_size} MB");

                        var ir_attachment_data = await hubIrAttachment.GetItem(item);
                        var item_ir = ir_attachment_data.result[0];

                        Debug.WriteLine($"Descargando archivo: {item_ir.name} de tamaño {item_ir.file_size} MB");

                        var partBytes = await hubMnsaAttachment.DownloadFileAsync(item);
                        await output.WriteAsync(partBytes, 0, partBytes.Length);
                    }
                }

                Debug.WriteLine($"ZIP reconstruido en: {tempZipPath}");

                // 🔹 Ahora SÍ se puede descomprimir
                string extractPath = FileSystem.AppDataDirectory;
                ZipFile.ExtractToDirectory(tempZipPath, extractPath, true);

                Debug.WriteLine("ZIP descomprimido correctamente");
            }            
        }

        public async Task<bool> RequiredNewUpload()
        {
            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

            var top5List = await hubMnsaAttachment.GetTop5();

            if (top5List?.result == null || top5List.result.Length == 0)
                return true;

            var item = top5List.result[0];

            if (!item.date_data_cutoff.HasValue)
                return true;

            DateTime cutoff = item.date_data_cutoff.Value;
            DateTime now = DateTime.UtcNow;

            double daysDiff = Math.Abs((now - cutoff).TotalDays);

            return daysDiff >= 7;
        }
    }
}
