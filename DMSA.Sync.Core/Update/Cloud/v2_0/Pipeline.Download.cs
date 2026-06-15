using ApiManager;
using ApiManagerOdoo.Specials;
using DMSA.Models.Odoo.Abstract.Server;
using DMSA.Models.Odoo.Specials;
using System.Diagnostics;
using System.IO.Compression;
using static ApiManager.HubMnsaAttachment;

namespace DMSA.Sync.Core.Update.Cloud.v2_0
{
    public partial class Pipeline    
    {
        //public async Task<bool> DownloadSqliteZipCustomMode2(string dbNameSqlite, bool removeTmpFile)
        //{
        //    bool boolResponse = false;

        //    var hubMnsaAttachment = new HubPackageClient(Constants.Session);
            
        //    var top5List = await hubMnsaAttachment.GetTop5(dbNameSqlite);

        //    if (top5List?.result != null && top5List.result.Length > 0)
        //    {
        //        var item_first = top5List.result[0];

        //        var linesUrlIds = item_first._lines_url
        //            .OrderBy(id => id)
        //            .ToList();

        //        string originalName = item_first.file_name;
        //        string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
        //        string ext = Path.GetExtension(originalName);

        //        string randomSuffix = Guid.NewGuid().ToString("N");

        //        string tempZipPath = Path.Combine(
        //            FileSystem.AppDataDirectory,
        //            $"{nameWithoutExt}_{randomSuffix}{ext}"
        //        );

        //        if (linesUrlIds.Count == 0)
        //            return false;

        //        using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
        //        {
        //            int total = linesUrlIds.Count;
        //            int count = 0;

        //            foreach (var item in linesUrlIds)
        //            {
        //                count++;
        //                Debug.WriteLine($"Descargando {count}/{total}");

        //                var ir_attachment_data = await hubIrAttachmentLine.GetItem(item);
        //                var item_ir = ir_attachment_data.result[0];
        //                string FullUrl = item_ir.url;

        //                var partBytes = await hubMnsaAttachment.DownloadFileMode2Async(FullUrl);

        //                if (partBytes == null || partBytes.Length == 0)
        //                {
        //                    Debug.WriteLine($"ERROR: Parte {count} vacía");
        //                    return false;
        //                }

        //                Debug.WriteLine($"Parte {count}: {partBytes.Length} bytes");

        //                await output.WriteAsync(partBytes, 0, partBytes.Length);
        //            }
        //        }

        //        bool exists = ZipContainsFile(tempZipPath, nameWithoutExt);

        //        if (exists)
        //        {
        //            string extractPath = FileSystem.AppDataDirectory;
        //            ZipFile.ExtractToDirectory(tempZipPath, extractPath, true);

        //            await Task.Delay(2000);
        //            boolResponse = true;
        //        }

        //        if (removeTmpFile)
        //            File.Delete(tempZipPath);
        //    }

        //    return boolResponse;
        //}

        public async Task<bool> DownloadPackage(Package package, 
            bool removeTmpFile, 
            Func<int, int, Task>? onProgress = null)
        {
            if(package == null)
                return false;

            bool boolResponse = false;

            var hubPackageClient = new HubPackageClient(Constants.Session);
            
            var filesList = (await hubPackageClient.GetFiles(package.name))
                .OrderBy(id => id)
                .ToList();

            string originalName = package.file_name;
            string nameWithoutExt = Path.GetFileNameWithoutExtension(originalName);
            string ext = Path.GetExtension(originalName);

            string randomSuffix = Guid.NewGuid().ToString("N");

            string tempZipPath = Path.Combine(
                FileSystem.AppDataDirectory,
                $"{nameWithoutExt}_{randomSuffix}{ext}"
            );

            if (filesList.Count == 0)
                return false;

            using (var output = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write))
            {
                int total = filesList.Count;
                int count = 0;

                foreach (var item in filesList)
                {
                    count++;
                    Debug.WriteLine($"Descargando {count}/{total}");

                    var partBytes = await hubPackageClient.DownloadDirect(item.url);
                    
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
