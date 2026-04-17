using Android.Content.Res;
using Android.App;
using Application = Android.App.Application;

namespace DMCobranzas.Platforms.Android
{
    public static class DeviceHelper
    {
        public static bool IsTablet()
        {
            var metrics = Application.Context.Resources.DisplayMetrics;

            float widthDp = metrics.WidthPixels / metrics.Density;
            float heightDp = metrics.HeightPixels / metrics.Density;

            float smallestWidth = Math.Min(widthDp, heightDp);

            return smallestWidth >= 600;
        }
    }
}
