using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustServerManager.Utils
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;

    public static class ActiveNetworkAdapterResolver
    {
        [DllImport("Iphlpapi.dll", SetLastError = true)]
        private static extern int GetBestInterface(uint destAddr, out uint bestIfIndex);

        public static NetworkInterface GetActiveAdapter(string targetIp = "8.8.8.8")
        {
            uint index = GetBestInterfaceIndex(targetIp);

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var ipv4Props = nic.GetIPProperties().GetIPv4Properties();
                if (ipv4Props != null && ipv4Props.Index == index)
                    return nic;
            }

            return null;
        }

        public static string GetPerformanceCounterName(NetworkInterface nic)
        {
            var category = new PerformanceCounterCategory("Network Interface");
            var instanceNames = category.GetInstanceNames();

            string nicDesc = nic.Description.ToLowerInvariant().Replace(" ", "").Replace("#", "");

            foreach (var name in instanceNames)
            {
                string norm = name.ToLowerInvariant().Replace(" ", "").Replace("#", "");

                if (norm.Contains(nicDesc) || nicDesc.Contains(norm))
                {
                    Debug.WriteLine($"✅ Matched counter: {name}");
                    return name;
                }
            }

            Debug.WriteLine("❌ No matching performance counter for NIC: " + nic.Description);
            return null;
        }

        private static uint GetBestInterfaceIndex(string ipAddress)
        {
            var ip = IPAddress.Parse(ipAddress).GetAddressBytes();
            if (BitConverter.IsLittleEndian)
                Array.Reverse(ip);

            uint dest = BitConverter.ToUInt32(ip, 0);
            int result = GetBestInterface(dest, out uint index);

            if (result != 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return index;
        }
    }
}
