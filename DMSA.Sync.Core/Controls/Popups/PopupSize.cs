using Microsoft.Maui.Devices;

namespace DMSA.Sync.Core.Controls.Popups;

public class PopupSizeConstants
{
	public PopupSizeConstants(IDeviceDisplay deviceDisplay)
	{
        CalculateSizes(deviceDisplay);
    }

	public void CalculateSizes(IDeviceDisplay deviceDisplay)
	{
        Tiny = new(100, 100);
        Small = new(300, 300);
        SmallWide = new Size(300, 135);

        //Al parecer es un BUG deviceDisplay no devuelve medidas cuando es Windows y se carga por primera vez
        // en evento OnAppering
        if (DeviceInfo.Current.Platform == DevicePlatform.WinUI && deviceDisplay.MainDisplayInfo.Width == 0)
        {
            //TODO: Se colocan medidas arbitrarias temporalmente
            Medium = new(400,400);
            Large = new(500,500);
            return;
        }

        
        var width = deviceDisplay.MainDisplayInfo.Width / deviceDisplay.MainDisplayInfo.Density;
        var height = deviceDisplay.MainDisplayInfo.Height / deviceDisplay.MainDisplayInfo.Density;

        if (DeviceInfo.Current.Idiom == DeviceIdiom.Phone
            || DeviceInfo.Current.Platform == DevicePlatform.Android
            || DeviceInfo.Current.Platform == DevicePlatform.iOS)
        {
            Medium = new(Math.Max(width - 12, 280), height * 0.88);
            Large = new(Math.Max(width - 8, 300), height * 0.92);
            return;
        }

        Medium = new(0.7 * width, 0.6 * height);
        Large = new(0.9 * width, 0.8 * height);
        
        //#if WINDOWS
        //        Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
        //        {
        //            var nativeWindow = handler.PlatformView;
        //            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
        //            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        //            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Nearest);                        
        //        });
        //#endif
    }

    // examples for fixed sizes
    public Size Tiny { get; set; }
    public Size Small { get; set; }

    public Size SmallWide { get; set; }

    // examples for relative to screen sizes
    public Size Medium { get; set; }

	public Size Large { get; set; }
}