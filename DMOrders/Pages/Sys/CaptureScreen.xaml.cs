using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

namespace DMOrders.Pages.Sys;

public partial class CaptureScreen : ContentPage
{
	public CaptureScreen()
	{
		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await AutoCapture();
    }

    public async Task<string> Capturar()
    {
        var tcs = new TaskCompletionSource<bool>();

        void handler(object sender, EventArgs e)
        {
            tcs.TrySetResult(true);
        }

        cameraView.HandlerChanged += handler;

        var cameras = await cameraView.GetAvailableCameras(CancellationToken.None);

        var frontCamera = cameras
            .FirstOrDefault(c => c.Position == CameraPosition.Front);

        if (frontCamera != null)
        {
            cameraView.SelectedCamera = frontCamera;
        }

        await tcs.Task;

        cameraView.HandlerChanged -= handler;

        await Task.Delay(300);

        var photo = await cameraView.CaptureImage(CancellationToken.None);

        var filePath = Path.Combine(FileSystem.CacheDirectory, "capture.jpg");

        using (var stream = File.OpenWrite(filePath))
        {
            await photo.CopyToAsync(stream);
        }

        return filePath;
    }

    public async Task AutoCapture()
    {
        cameraView.IsVisible = true;
        await Task.Delay(1000);        
        var path = await Capturar();

        await GuardarEnGaleria(path);

        cameraView.IsVisible = false;
    }

    public async Task GuardarEnGaleria(string filePath)
    {
#if ANDROID
        var context = Android.App.Application.Context;

        var values = new Android.Content.ContentValues();
        values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, Path.GetFileName(filePath));
        values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/jpeg");
        values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "DCIM/Camera");

        var uri = context.ContentResolver.Insert(
            Android.Provider.MediaStore.Images.Media.ExternalContentUri, values);

        using (var outputStream = context.ContentResolver.OpenOutputStream(uri))
        using (var inputStream = File.OpenRead(filePath))
        {
            await inputStream.CopyToAsync(outputStream);
        }
#endif
    }
}