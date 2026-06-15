using System.IO.Compression;

namespace DMSA.Sync.Core.Update.Cloud.v1_5
{
    public partial class Pipeline
    {
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

        public bool ZipContainsFile(string zipPath, string fileName)
        {
            using var zip = ZipFile.OpenRead(zipPath);

            return zip.Entries.Any(e =>
                string.Equals(e.Name, fileName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
