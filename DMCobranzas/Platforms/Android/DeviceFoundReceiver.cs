using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Bluetooth;
using DMCobranzas.Services;


namespace DMCobranzas.Platforms.Android
{
    public class DeviceFoundReceiver : BroadcastReceiver
    {
        private readonly Action<BluetoothDeviceInfo> _callback;

        public DeviceFoundReceiver(Action<BluetoothDeviceInfo> callback)
        {
            _callback = callback;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == BluetoothDevice.ActionFound)
            {
                var device =
                    (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                if (device != null)
                {
                    _callback?.Invoke(new BluetoothDeviceInfo
                    {
                        Name = device.Name,
                        Address = device.Address
                    });
                }
            }
        }
    }
}
