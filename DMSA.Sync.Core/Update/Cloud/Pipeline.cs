using ApiManager;
using DMSA.Models.Odoo.General.Requests;
using DMSA.Models.Odoo.General.Responses;
using DMSA.Models.Odoo.Specials;
using DMSA.Sync.Core.Database.Sqlite;
using DMSA.Sync.Core.Database.Sqlite.DebitCollection;
using DMSA.Sync.Core.Database.Sqlite.Payments;
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
    public partial class Pipeline
    {
        [Obsolete]
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

        //Archivos con un tamaño maximo de 3.14 MB
        const int MAX_PART_SIZE = (int)(3.14 * 1024 * 1024);
        const int MAX_PART_SIZE_LONG = (int)(6.6 * 1024 * 1024);

        [Obsolete("No utilizado")]
        public async Task<(mnsa_attachment, bool)> UploadSqliteZip(Func<int, int, Task>? onProgress = null)
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
                server = Constants.Session.odooConnection.Host,
                database_name = Constants.Session.odooConnection.DbName,
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

                if(file_upload_response.result  == 0)
                {
                    Debug.WriteLine($"Hubo un error al enviar el archivo: {file_upload_response.error.message}");
                    return (mnsaAttachment, false);
                }

                Debug.WriteLine($"Parte {partName} subida con ID: {file_upload_response.result}");

                await hub.Link(packageId, file_upload_response.result);

                if (onProgress != null)
                    await onProgress(i + 1, totalParts);
            }

            Debug.WriteLine("✅ Todas las partes enviadas correctamente");

            return (mnsaAttachment, true);
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

        //[Obsolete("Parece que no es usado")]
        //public async Task<bool> AvailableZipPack()
        //{
        //    HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

        //    var top5List = await hubMnsaAttachment.GetTop5();

        //    if (top5List != null && top5List.result != null && top5List.result.Length > 0)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public async Task<mnsa_attachment> NewestZipPack()
        {
            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

            string packName = Constants.Session.odooConnection.DbNameSqlite + ".zip";
            var top5List = await hubMnsaAttachment.GetTop5(packName);

            if (top5List != null && top5List.result != null && top5List.result.Length > 0)
            {
                return top5List.result[0];
            }

            return null;
        }

        //public async Task<bool> DownloadSqliteZip(bool removeTmpFile, Func<int, int, Task>? onProgress = null)
        //{
        //    bool boolResponse = false;

        //    HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);
        //    HubIrAttachment hubIrAttachment = new HubIrAttachment(Constants.Session);
        //    string packName = Constants.Session.odooConnection.DbNameSqlite + ".zip";
        //    var top5List = await hubMnsaAttachment.GetTop5(packName);

        //    int indexFile = 0;
        //    int totalFiles = 0;

        //    if(top5List != null && top5List.result!=null && top5List.result.Length > 0)
        //    {
        //        var item_first = top5List.result[0];

        //        var attachmentIds = item_first._attachment_ids
        //            .OrderBy(id => id)
        //            .ToList();

        //        string originalName = item_first.file_name;
        //        string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
        //        string ext = Path.GetExtension(originalName);

        //        string randomSuffix = Guid.NewGuid().ToString("N"); // sin guiones

        //        string tempZipPath = Path.Combine(
        //            FileSystem.AppDataDirectory,
        //            $"{nameWithoutExt}_{randomSuffix}{ext}"
        //        );

        //        if (attachmentIds.Count == 0)
        //        {
        //            return false;
        //        }

        //        totalFiles = attachmentIds.Count;

        //        using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
        //        {
        //            foreach (var item in attachmentIds)
        //            {
        //                indexFile++;
                        
        //                var ir_attachment_data = await hubIrAttachment.GetItem(item);
        //                var item_ir = ir_attachment_data.result[0];

        //                Debug.WriteLine($"Descargando archivo: {item_ir.name} de tamaño {item_ir.file_size} MB");

        //                if (onProgress != null)
        //                    await onProgress(indexFile, totalFiles);

        //                var partBytes = await hubMnsaAttachment.DownloadFileAsync(item);
        //                await output.WriteAsync(partBytes, 0, partBytes.Length);
        //            }
        //        }

        //        Debug.WriteLine($"ZIP reconstruido en: {tempZipPath}");

        //        bool exists = ZipContainsFile(tempZipPath, Constants.Session.odooConnection.DbNameSqlite);

        //        if (exists)
        //        {                    
        //            string extractPath = FileSystem.AppDataDirectory;
        //            ZipFile.ExtractToDirectory(tempZipPath, extractPath, true);
        //            Debug.WriteLine("ZIP descomprimido correctamente");
        //            await Task.Delay(2000);
        //            boolResponse = true;
        //        }
        //        else
        //        {
        //            Debug.WriteLine("El archivo no pertenece a esta conexión. No se va a restaurar.");
        //        }

        //        if (removeTmpFile)
        //            File.Delete(tempZipPath);                
        //    }

        //    return boolResponse;
        //}

        public bool ZipContainsFile(string zipPath, string fileName)
        {
            using var zip = ZipFile.OpenRead(zipPath);

            return zip.Entries.Any(e =>
                string.Equals(e.Name, fileName, StringComparison.OrdinalIgnoreCase));
        }

        //public async Task<bool> RequiredNewUpload()
        //{
        //    HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

        //    var topList = await hubMnsaAttachment.GetLastest(Constants.Session.CurrentUserFront.log_fec_acceso);

        //    if (topList?.result == null || topList.result.Length == 0)
        //        return true;

        //    var item = topList.result[0];

        //    if (!item.date_data_cutoff.HasValue)
        //        return true;

        //    DateTime cutoff = item.date_data_cutoff.Value.Date;
        //    DateTime now = Constants.Session.CurrentUserFront.log_fec_acceso.Date;

        //    double daysDiff = Math.Abs((now - cutoff).TotalDays);

        //    return daysDiff >= 1;
        //}

        public async Task<bool> ExistAttachRecord()
        {
            MnsaAttachmentDb mnsaAttachmentDb = new MnsaAttachmentDb(Constants.Session.odooConnection.DbNameSqlite);
            var record = await mnsaAttachmentDb.GetLastUpdate();
            if(record != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsValidData()
        {
            var accountMoveDb = new AccountMoveDb(Constants.Session.odooConnection.DbNameSqlite);
            var record = await accountMoveDb.GetItemsAsync(x=> x.id > 0);

            //var multipleCobrosInvoiceDb = new MultipleCobrosInvoiceDb(Constants.Session.odooConnection.DbNameSqlite);

            if (record != null && record.Count > 2000)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> InsertAttachRecord(mnsa_attachment mnsa_Attachment)
        {
            MnsaAttachmentDb mnsaAttachmentDb = new MnsaAttachmentDb(Constants.Session.odooConnection.DbNameSqlite);
            var record = await mnsaAttachmentDb.InsertAsync(mnsa_Attachment);            
            return true;
        }

        public async Task<bool> ResetUserData()
        {
            var accountPaymentDailyDb = new AccountPaymentDailyDb(Constants.Session.odooConnection.DbNameSqlite);
            await accountPaymentDailyDb.DropTableAsync();

            var multipleCobrosInvoice = new MultipleCobrosInvoiceDb(Constants.Session.odooConnection.DbNameSqlite);
            await multipleCobrosInvoice.DropTableAsync();

            var multipleCobrosInvoiceLine = new MultipleCobrosInvoiceLineDb(Constants.Session.odooConnection.DbNameSqlite);
            await multipleCobrosInvoiceLine.DropTableAsync();

            var multipleCobrosInvoiceLineAi = new MultipleCobrosInvoiceLineAiDb(Constants.Session.odooConnection.DbNameSqlite);
            await multipleCobrosInvoiceLineAi.DropTableAsync();

            var creditNoteRequestDb = new CreditNoteRequestDb(Constants.Session.odooConnection.DbNameSqlite);
            await creditNoteRequestDb.DropTableAsync();

            var creditNoteRequestDetailDb = new CreditNoteRequestDetailDb(Constants.Session.odooConnection.DbNameSqlite);
            await creditNoteRequestDetailDb.DropTableAsync();

            var creditNoteRequestGroupDb = new CreditNoteRequestGroupDb(Constants.Session.odooConnection.DbNameSqlite);
            await creditNoteRequestGroupDb.DropTableAsync();

            return true;
        }
    }
}
