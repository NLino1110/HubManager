using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

namespace DMDataSafe
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        string[] permisosPorSolicitar = new string[] { };
        int currentPermision = 0;

        protected override void OnCreate(Bundle saveInstanceState)
        {
            base.OnCreate(saveInstanceState);

            RequestAllPermission();
        }

        void RequestAllPermission()
        {

            System.Diagnostics.Debug.WriteLine(PackageName);
            PackageInfo packageInfo = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.Permissions);

            string[] permisosSolicitados = packageInfo.RequestedPermissions.ToArray();

            if (permisosSolicitados != null)
            {
                List<string> permisosPorSolicitarLista = new List<string>();
                foreach (var permiso in permisosSolicitados)
                {
                    System.Diagnostics.Debug.WriteLine("Permiso solicitado: " + permiso);
                    var statusPermi = CheckSelfPermission(permiso);
                    System.Diagnostics.Debug.WriteLine(statusPermi);

                    if (statusPermi != Permission.Granted)
                    {
                        if (permiso != "android.permission.POST_NOTIFICATIONS" &&
                                    permiso != "android.permission.ACCESS_BACKGROUND_LOCATION" &&
                                    permiso != "android.permission.ACCESS_MEDIA_LOCATION" &&
                                    permiso != "android.permission.READ_MEDIA_AUDIO" &&
                                    permiso != "android.permission.READ_MEDIA_VIDEO" &&
                                    permiso != "android.permission.READ_MEDIA_IMAGES" &&
                                    permiso != "android.permission.ACCESS_COARSE_LOCATION" &&
                                    permiso != "android.permission.BATTERY_STATS" &&
                                    permiso != PackageName + ".DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION")
                        {
                            permisosPorSolicitarLista.Add(permiso);
                        }
                    }
                }

                permisosPorSolicitar = permisosPorSolicitarLista.ToArray();

                if (permisosPorSolicitar.Length > 0)
                {
                    RequestPermissions(new string[] { permisosSolicitados[currentPermision] }, currentPermision);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == currentPermision)
            {
                System.Diagnostics.Debug.WriteLine(grantResults);

                if (permisosPorSolicitar.Length == currentPermision + 1)
                {
                    return;
                }

                currentPermision++;

                System.Diagnostics.Debug.WriteLine("Permisos a solicitar:" + permisosPorSolicitar[currentPermision]);
                RequestPermissions(new string[] { permisosPorSolicitar[currentPermision] }, currentPermision);
            }
        }
    }
}