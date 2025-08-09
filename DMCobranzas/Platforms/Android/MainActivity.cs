using System.Diagnostics;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views.InputMethods;
using Android.Views;
using Android.Widget;
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

                ////int permisoCount = 2000;
                ////foreach (var permiso in permisosSolicitados)
                ////{
                ////    // Haz algo con cada permiso solicitado, por ejemplo, imprímelo
                ////    System.Diagnostics.Debug.WriteLine("Permiso solicitado: " + permiso);
                ////    var statusPermi = CheckSelfPermission(permiso);
                ////    System.Diagnostics.Debug.WriteLine(statusPermi);

                ////    if (statusPermi != Permission.Granted)
                ////    {
                ////        if (permiso != "android.permission.POST_NOTIFICATIONS" &&
                ////            permiso != "android.permission.ACCESS_BACKGROUND_LOCATION" &&
                ////            permiso != "android.permission.READ_MEDIA_AUDIO" &&
                ////            permiso != "android.permission.READ_MEDIA_VIDEO" &&
                ////            permiso != "android.permission.READ_MEDIA_IMAGES" &&
                ////            permiso != PackageName + ".DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION")
                ////        {
                ////            Timer timer = new Timer(TimerCallback, permiso, permisoCount, 0);

                ////            //timer = new Timer(TimerCallback, null, 0, 2000);
                ////            //timer = new Timer(TimerCallback, permiso, 0, 2000);
                ////            timers.Add(timer);

                ////            permisoCount += permisoCount;
                ////        }
                ////    }
                ////}
            }

            //System.Diagnostics.Debug.WriteLine("Permisos:");
            
            ////var status = CheckSelfPermission(Manifest.Permission.Bluetooth);
            //var status = CheckSelfPermission(Manifest.Permission.BluetoothScan);

            //System.Diagnostics.Debug.WriteLine(Manifest.Permission.BluetoothScan);
            //System.Diagnostics.Debug.WriteLine(status);

            //System.Diagnostics.Debug.WriteLine(Manifest.Permission.Bluetooth);
            //System.Diagnostics.Debug.WriteLine(CheckSelfPermission(Manifest.Permission.Bluetooth));

            //System.Diagnostics.Debug.WriteLine(Manifest.Permission.BluetoothAdmin);
            //System.Diagnostics.Debug.WriteLine(CheckSelfPermission(Manifest.Permission.BluetoothAdmin));

            //System.Diagnostics.Debug.WriteLine(Manifest.Permission.BluetoothAdvertise);
            //System.Diagnostics.Debug.WriteLine(CheckSelfPermission(Manifest.Permission.BluetoothAdvertise));

            //System.Diagnostics.Debug.WriteLine(Manifest.Permission.BluetoothConnect);
            //System.Diagnostics.Debug.WriteLine(CheckSelfPermission(Manifest.Permission.BluetoothConnect));

            //System.Diagnostics.Debug.WriteLine(Manifest.Permission.BluetoothPrivileged);
            //System.Diagnostics.Debug.WriteLine(CheckSelfPermission(Manifest.Permission.BluetoothPrivileged));

            //if (status != Permission.Granted)
            //{
            //    RequestPermissions(new string[] { Manifest.Permission.Bluetooth }, BT1_PERMISSION_CODE);
            //}
        }

        //private void TimerCallback(object state)
        //{
        //    RequestPermissions(new string[] { state.ToString() }, BT1_PERMISSION_CODE);

        //    System.Diagnostics.Debug.WriteLine("Permisos solicitados:" + state.ToString());
        //    //// Obtener el temporizador actual
        //    //Timer timer = (Timer)state;

        //    //// Detener el temporizador
        //    //timer.Dispose();

        //    //// Eliminar el temporizador de la lista
        //    //timers.Remove(timer);
        //}

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

                //if (grantResults[0] == Permission.Granted)
                //{
                //    // Permiso otorgado
                //}
                //else
                //{
                //    // Permiso denegado
                //}

                //System.Diagnostics.Debug.WriteLine("Permisos:" + grantResults[0].ToString());
            }
        }
    }
}