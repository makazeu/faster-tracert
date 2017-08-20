using System;
using System.Net;
using System.Net.Sockets;

namespace faster_tracert {
    public class NetUtils {
        private static string IpAddrLibrary = "qqwry.dat";
        
        public static IPAddress GetIpAddressFromString(string target) {
            var ip = ParseIpAddress(target) ?? ResolveDomainName(target);
            return ip;
        }

        public static IPAddress ParseIpAddress(string ip) {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(ip, out ipAddress)) return null;
            switch (ipAddress.AddressFamily) {
                case AddressFamily.InterNetwork:
                    return ipAddress;
                //case AddressFamily.InterNetworkV6:
                //    return null;
                default:
                    return null;
            }
        }

        public static IPAddress ResolveDomainName(string domain) {
            IPAddress ipAddress = null;
            try {
                var hostEntry = Dns.GetHostEntry(domain);
                foreach (var entry in hostEntry.AddressList) {
                    if (entry.AddressFamily != AddressFamily.InterNetwork) continue;
                    ipAddress = new IPAddress(entry.GetAddressBytes());
                    break;
                }
            }
            catch {
                ipAddress = null;
            }
            return ipAddress;
        }

        public static string GetIpLocation(string ip) {
            var location = "";
            try {
                location = IpAddrLib.IPLocate(IpAddrLibrary, ip);
            }
            catch(Exception ex) {
                location = ex.Message;
            }
            return location;
        }
    }
}