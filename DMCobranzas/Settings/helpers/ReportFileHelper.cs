using System.Text;

namespace DMCobranzas.Settings.helpers;

public static class ReportFileHelper
{
    public static async Task<string> SaveCsvToDownloadsAsync(string fileName, string content)
    {
#if ANDROID
        return await SaveToDownloadsAndroidAsync(fileName, content);
#else
        var downloadsDir = GetDownloadsDirectory();
        Directory.CreateDirectory(downloadsDir);
        var filePath = GetUniqueFilePath(downloadsDir, fileName);
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
        return filePath;
#endif
    }

    private static string GetDownloadsDirectory()
    {
#if WINDOWS
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
#elif IOS || MACCATALYST
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
#else
        return Path.Combine(FileSystem.AppDataDirectory, "Downloads");
#endif
    }

#if ANDROID
    private static async Task<string> SaveToDownloadsAndroidAsync(string fileName, string content)
    {
        var context = Platform.CurrentActivity ?? Android.App.Application.Context;

        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Q)
        {
            var values = new Android.Content.ContentValues();
            values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
            values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "text/csv");
            values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, Android.OS.Environment.DirectoryDownloads);

            var uri = context.ContentResolver!.Insert(Android.Provider.MediaStore.Downloads.ExternalContentUri, values)
                ?? throw new IOException("No se pudo crear el archivo en Descargas.");

            await using var stream = context.ContentResolver.OpenOutputStream(uri)!;
            await using var writer = new StreamWriter(stream, Encoding.UTF8);
            await writer.WriteAsync(content);

            return Path.Combine("Descargas", fileName);
        }

        var downloadsDir = Android.OS.Environment
            .GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)!
            .AbsolutePath!;
        Directory.CreateDirectory(downloadsDir);
        var filePath = GetUniqueFilePath(downloadsDir, fileName);
        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
        return filePath;
    }
#endif

    private static string GetUniqueFilePath(string directory, string fileName)
    {
        var filePath = Path.Combine(directory, fileName);
        if (!File.Exists(filePath))
            return filePath;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var counter = 1;

        do
        {
            filePath = Path.Combine(directory, $"{name} ({counter}){ext}");
            counter++;
        } while (File.Exists(filePath));

        return filePath;
    }
}
