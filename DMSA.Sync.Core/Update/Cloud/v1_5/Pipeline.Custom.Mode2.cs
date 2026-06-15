using ApiManager;
using DMSA.Models.Odoo.Specials;
using System.Diagnostics;
using System.IO.Compression;
using static ApiManager.HubMnsaAttachment;

namespace DMSA.Sync.Core.Update.Cloud.v1_5
{
    public partial class Pipeline    
    {
        public async Task<(string, bool, responseUpload)> UploadSqliteZipNonAttach(string dbNameSqlite)
        {
            string dbPath = Path.Combine(
                FileSystem.AppDataDirectory,
                dbNameSqlite
            );

            string zipPath = string.Empty;
            string final_url = string.Empty;
            responseUpload file_upload_response = null;

            try
            {
                Debug.WriteLine($"Comprimiendo base de datos: {dbPath}");
                zipPath = await CompressDatabaseAsync(dbPath);

                byte[] zipBytes = await File.ReadAllBytesAsync(zipPath);

                Debug.WriteLine($"ZIP generado. Tamaño total: {zipBytes.Length / 1024 / 1024.0:F2} MB");

                HubMnsaAttachment hub = new HubMnsaAttachment(Constants.Session);

                int packageId = 1381;

                //var parts = SplitFile(zipBytes, MAX_PART_SIZE_LONG).ToList();
                //int totalParts = parts.Count;
                //Una sola parte para poder descargar
                var parts = new List<byte[]> { zipBytes };
                int totalParts = 1;

                string package_name = Constants.Session.AppCodeOdoo + "_app_package_" +
                dbNameSqlite + "_" +
                DateTime.Now.ToString("yyyyMMddHHmmss");

                for (int i = 0; i < totalParts; i++)
                {
                    string partName = $"{packageId}_{dbNameSqlite}_part_{(i + 1):D6}.zip";

                    file_upload_response = await hub.SendToExternalServer(parts[i], partName, package_name);                    
                    if (file_upload_response == null || file_upload_response.url == String.Empty)
                        return (final_url, false, file_upload_response);

                    //if(file_upload_response.status_code != 200)
                    //{
                    //    Debug.WriteLine($"Upload error: {file_upload_response.message}");
                    //    return (final_url, false);
                    //}

                    final_url = file_upload_response.url;
                }

                return (final_url, true, file_upload_response);
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

        public async Task<bool> DownloadSqliteZipByPackage(string packageName, bool removeTmpFile, Func<int, int, Task>? onProgress = null)
        {
            bool boolResponse = false;

            var parts = packageName.Split('_');
            var nameParts = parts.Skip(3).Take(parts.Length - 4);
            string originalName = string.Join("_", nameParts) + ".zip";

            //string originalName = $"prod1_macronegocios.zip";

            HubMnsaAttachment hubMnsaAttachment = new HubMnsaAttachment(Constants.Session);

            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
            string ext = Path.GetExtension(originalName);

            var linesUrlIds = new List<string>();
            linesUrlIds.Add($"1381_{nameWithoutExt}_part_000001.zip");

            string randomSuffix = Guid.NewGuid().ToString("N");

            string tempZipPath = Path.Combine(
                FileSystem.AppDataDirectory,
                $"{nameWithoutExt}_{randomSuffix}{ext}"
            );

            if (linesUrlIds.Count == 0)
                return false;

            using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
            {
                int total = linesUrlIds.Count;
                int count = 0;

                foreach (var item in linesUrlIds)
                {
                    count++;
                    Debug.WriteLine($"Descargando {count}/{total}");
                    
                    string FullUrl = $"https://manager.dmujeres.ec:5001/uploads/zips/{packageName}/{item}"; ;

                    var partBytes = await hubMnsaAttachment.DownloadFileMode2Async(FullUrl);

                    if (partBytes == null || partBytes.Length == 0)
                    {
                        Debug.WriteLine($"ERROR: Parte {count} vacía");
                        return false;
                    }

                    Debug.WriteLine($"Parte {count}: {partBytes.Length} bytes");

                    if (onProgress != null)
                        await onProgress(count, total);

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

            return boolResponse;
        }
    }
}
