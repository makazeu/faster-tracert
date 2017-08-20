using System;
using System.Net;
using System.Text;

namespace faster_tracert {
    class Program {
        static void Main(string[] args) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            string targetName = "";
            int hops = 30;
            int timeout = 3000;

            try {
                for (var i = 0; i < args.Length; i++) {
                    if (args[i][0] == '-') {
                        switch (args[i][1]) {
                            case 'h':
                                i++;
                                hops = Convert.ToInt32(args[i]);
                                if (hops <= 0) {
                                    hops = 3000;
                                }
                                break;
                            case 't':
                                i++;
                                timeout = Convert.ToInt32(args[i]);
                                break;
                        }
                    }
                    else {
                        targetName = args[i];
                        break;
                    }
                }
            }
            catch {
                targetName = "";
            }
            
            // prints header
            Console.WriteLine("faster-tracert v1.0  by makazeu");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine($"destination hostname: {targetName}");
            Console.WriteLine($"hops: {hops},  timeout: {timeout}ms");
            Console.WriteLine();
            
            var ipAddress = NetUtils.GetIpAddressFromString(targetName);
            if (ipAddress == null) {
                Console.WriteLine("invalid hostname or ip address!");
                return;
            }
            
            Console.WriteLine($"tracing route to {ipAddress} ...");
            Traceroute(ipAddress, hops, timeout);
        }

        private static void Traceroute(IPAddress ip, int hops, int timeout) {
            var pingAction = new PingAction(hops);
            pingAction.Run(ip, timeout);

            pingAction.Disp();
            pingAction.Dispose();
        }
    }
}