using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace DMCobranzas.Settings.helpers
{
    internal class MyBluetoothPermission : BasePlatformPermission
    {
//#if ANDROID
//    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
//        new List<(string permission, bool isRuntime)>
//        {
//            ("android.permission.BLUETOOTH_SCAN", true),
//            ("android.permission.BLUETOOTH_CONNECT", true)
//        }.ToArray();
//#endif

        //public override Task<PermissionStatus> GetPermissionStatus()=> Task.FromResult(IsDeclaredInManifest(Manifest.Permission.BatteryStas) ? )
    }
}
