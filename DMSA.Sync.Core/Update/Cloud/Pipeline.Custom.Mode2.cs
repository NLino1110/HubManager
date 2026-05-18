using ApiManager;
using DMSA.Models.Odoo.Specials;
using System.Diagnostics;
using System.IO.Compression;

namespace DMSA.Sync.Core.Update.Cloud
{
    public partial class Pipeline    
    {
        
        public async Task<(mnsa_attachment, bool)> UploadSqliteZipCustomMode2(string dbNameSqlite)
        {
            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                dbNameSqlite
            );

            string zipPath = string.Empty;

            try
            {
                Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
                zipPath = await CompressDatabaseAsync(dbPath);

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

                var responseSend = await hub.CreatePackage(mnsaAttachment);
                int packageId = responseSend.result;

                var parts = SplitFile(zipBytes, MAX_PART_SIZE_LONG).ToList();
                int totalParts = parts.Count;

                for (int i = 0; i < totalParts; i++)
                {
                    string partName = $"{packageId}_{dbNameSqlite}_part_{(i + 1):D6}.zip";

                    var file_upload_response = await hub.SendAttachmentMode2(new mnsa_attachment_line()
                    {
                        name = partName,
                        url = "",
                        file_name = partName,
                        file_bytes = parts[i],
                        file_type = "application/zip",
                        package_id = packageId
                    }, dbNameSqlite);

                    if (file_upload_response.result == 0)
                        return (mnsaAttachment, false);

                    await hub.LinkMode2(packageId, file_upload_response.result);
                }

                return (mnsaAttachment, true);
            }
            finally
            {

                try
                {
                    if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                        Debug.WriteLine($"ZIP eliminado: {zipPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"No se pudo eliminar el ZIP: {ex.Message}");
                }
            }
        }

        public async Task<bool> DownloadSqliteZipCustomMode2(string dbNameSqlite, bool removeTmpFile)
        {
            bool boolResponse = false;

            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);
            HubIrAttachmentLine hubIrAttachmentLine = new HubIrAttachmentLine(Constants.Session);

            var top5List = await hubMnsaAttachment.GetTop5(dbNameSqlite);

            if (top5List?.result != null && top5List.result.Length > 0)
            {
                var item_first = top5List.result[0];

                var linesUrlIds = item_first._lines_url
                    .OrderBy(id => id)
                    .ToList();

                string originalName = item_first.file_name;
                string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
                string ext = Path.GetExtension(originalName);

                string randomSuffix = Guid.NewGuid().ToString("N");

                string tempZipPath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    $"{nameWithoutExt}_{randomSuffix}{ext}"
                );

                if (linesUrlIds.Count == 0)
                    return false;

                //using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
                //{
                //    int total = linesUrlIds.Count;
                //    int count = 0;
                //    foreach (var item in linesUrlIds)
                //    {
                //        count++;
                //        Debug.WriteLine("Descargando archivo " + count + " de " + total );
                //        var ir_attachment_data = await hubIrAttachmentLine.GetItem(item);
                //        var item_ir = ir_attachment_data.result[0];
                //        string FullUrl = item_ir.url;

                //        var partBytes = await hubMnsaAttachment.DownloadFileMode2Async(FullUrl);
                //        await output.WriteAsync(partBytes, 0, partBytes.Length);
                //    }
                //}

                using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
                {
                    int total = linesUrlIds.Count;
                    int count = 0;

                    foreach (var item in linesUrlIds)
                    {
                        count++;
                        Debug.WriteLine($"Descargando {count}/{total}");

                        var ir_attachment_data = await hubIrAttachmentLine.GetItem(item);
                        var item_ir = ir_attachment_data.result[0];
                        string FullUrl = item_ir.url;

                        var partBytes = await hubMnsaAttachment.DownloadFileMode2Async(FullUrl);

                        if (partBytes == null || partBytes.Length == 0)
                        {
                            Debug.WriteLine($"ERROR: Parte {count} vacía");
                            return false;
                        }

                        Debug.WriteLine($"Parte {count}: {partBytes.Length} bytes");

                        await output.WriteAsync(partBytes, 0, partBytes.Length);
                    }
                }

                bool exists = ZipContainsFile(tempZipPath, nameWithoutExt);

                if (exists)
                {
                    string extractPath = FileSystem.AppDataDirectory;
                    ZipFile.ExtractToDirectory(tempZipPath, extractPath, true);

                    await Task.Delay(2000);
                    boolResponse = true;
                }

                if (removeTmpFile)
                    File.Delete(tempZipPath);
            }

            return boolResponse;
        }        
    }
}
