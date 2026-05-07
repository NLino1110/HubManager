using System.Diagnostics;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views.InputMethods;
using Android.Views;
using Android.Widget;
using DMCobranzas.Platforms.Android;
//using AndroidX.Activity;
//using Xamarin.Forms;
//using Microsoft.AppCenter;
//using Microsoft.AppCenter.Analytics;
//using Microsoft.AppCenter.Crashes;

namespace DMCobranzas
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private static int BT1_PERMISSION_CODE = 1001;
        //private static int BT2_PERMISSION_CODE = 1002;
        //private static int BT3_PERMISSION_CODE = 1003;
        //private static int BT4_PERMISSION_CODE = 1004;
        //private static int BT5_PERMISSION_CODE = 1005;
        
        private List<Timer> timers = new List<Timer>();
        string[] permisosPorSolicitar = new string[] { };
        int currentPermision = 0;

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                var view = CurrentFocus;
                if (view is EditText editText)
                {
                    editText.ClearFocus();
                    InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(view.WindowToken, 0);
                }
            }

            return base.DispatchTouchEvent(e);
        }

        protected override void OnCreate(Bundle saveInstanceState)
        {
            base.OnCreate(saveInstanceState);
            //AppCenter.Start("ddce22e0-8e29-4098-a3b0-81039da36af1",
            //    typeof(Analytics), typeof(Crashes));

            if (DeviceHelper.IsTablet())
            {                
                RequestedOrientation = ScreenOrientation.Landscape;
            }
            else
            {                
                RequestedOrientation = ScreenOrientation.Portrait;
            }

            RequestAllPermission();
        }

        void RequestAllPermission()
        {
            //string[] permisosSolicitados = packageInfo.RequestedPermissions;

            System.Diagnostics.Debug.WriteLine(PackageName);
            PackageInfo packageInfo = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.Permissions);

            string[] permisosSolicitados = packageInfo.RequestedPermissions.ToArray();

            if (permisosSolicitados != null)
            {
                List<string> permisosPorSolicitarLista = new List<string>();
                foreach (var permiso in permisosSolicitados)
                {
                    // Haz algo con cada permiso solicitado, por ejemplo, imprímelo
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
                    //currentPermision++;
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

                if(permisosPorSolicitar.Length == currentPermision +  1)
                {
                    //Se finaliza recurrencia
                    return;
                }

                currentPermision++;

                System.Diagnostics.Debug.WriteLine("Permisos a solicitar:" + permisosPorSolicitar[currentPermision]);
                RequestPermissions(new string[] { permisosPorSolicitar[currentPermision] }, currentPermision);
            }
        }
    }
}