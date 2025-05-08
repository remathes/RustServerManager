using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

namespace RustServerManager.Utils
{
    public static class NetworkCounterDetector
    {
        public static bool TryGetInstanceNameByIp(string ipAddress, out string instanceName)
        {
            instanceName = string.Empty;

            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up);

            foreach (var ni in interfaces)
            {
                var props = ni.GetIPProperties();
                if (props.UnicastAddresses.Any(u => u.Address.ToString() == ipAddress))
                {
                    var allInstances = new PerformanceCounterCategory("Network Interface").GetInstanceNames();
                    instanceName = allInstances.FirstOrDefault(i =>
                        ni.Description.Length >= 4 &&
                        i.Length >= 4 &&
                        ni.Description[^4..] == i[^4..]);

                    return !string.IsNullOrEmpty(instanceName);
                }
            }

            return false;
        }
    }
}
