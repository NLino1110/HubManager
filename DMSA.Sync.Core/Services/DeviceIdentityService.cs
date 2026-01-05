namespace DMSA.Sync.Core.Services
{
    public static class DeviceIdentityService
    {
        static string _cached;

        public static async Task<string> GetCachedAsync()
        {
            if (_cached != null) return _cached;
            _cached = await GetDeviceUUIDAsync();
            return _cached;
        }

        public static async Task<string> GetDeviceUUIDAsync()
        {
            try
            {
                var v = await SecureStorage.GetAsync("device_uuid");
                if (!string.IsNullOrEmpty(v))
                    return v;

                v = Guid.NewGuid().ToString();
                await SecureStorage.SetAsync("device_uuid", v);
                return v;
            }
            catch
            {
                var v = Preferences.Get("device_uuid", null);
                if (v != null) return v;

                v = Guid.NewGuid().ToString();
                Preferences.Set("device_uuid", v);
                return v;
            }
        }
    }
}
