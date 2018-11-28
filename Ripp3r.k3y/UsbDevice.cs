using System;
using Microsoft.Win32;

namespace Ripp3r
{
    public class UsbDevice : IDisposable
    {
        private RegistryKey key;

        private UsbDevice(string device)
        {
            Device = device;
            key = FindDevice(device);
        }

        public static UsbDevice Parse(string device)
        {
            UsbDevice dev = new UsbDevice(device);
            return dev.key != null ? dev : null;
        }

        private static RegistryKey FindDevice(string device)
        {
            string[] parts = device.Split('#');
            if (parts.Length >= 3)
            {
                string deviceType = parts[0].Substring(parts[0].IndexOf(@"?\", StringComparison.Ordinal) + 2);
                string deviceInstanceId = parts[1];
                string deviceUniqueId = parts[2];
                string regPath = @"SYSTEM\CurrentControlSet\Enum\" + deviceType + "\\" + deviceInstanceId + "\\" +
                                 deviceUniqueId;

                return Registry.LocalMachine.OpenSubKey(regPath);
            }
            return null;
        }

        public bool IsUsbStorage
        {
            get { return key.GetStringValue("Service") == "USBSTOR"; }
        }

        public string FriendlyName
        {
            get 
            { 
                string name = key.GetStringValue("FriendlyName");
                return string.IsNullOrEmpty(name) ? key.GetStringValue("DeviceDesc") : name;
            }
        }

        public Guid ClassGuid
        {
            get { return Guid.Parse(key.GetStringValue("ClassGUID")); }
        }

        public string Device { get; private set; }

        public void Dispose()
        {
            if (key == null) return;

            key.Dispose();
            key = null;
        }
    }
}
