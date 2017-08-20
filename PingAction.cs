using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace faster_tracert {
    
    internal enum PingStatus {
        Waiting,
        TtlExpired,
        Finish,
        Timedout,
        Unknown
    }

    internal class PingRun : IDisposable {
        
        public PingStatus Status { get; private set; }
        private readonly Ping ping;
        private IPAddress currentIp;
        private readonly PingOptions pingOptions;
        private readonly byte[] data = new byte[0]; // ICMP data
        
        private int n; // TTL of current ping run
        private long startTime;
        private long stopTime;

        public PingRun(int n) {
            this.n = n;
            Status = PingStatus.Waiting;
            ping = new Ping();
            ping.PingCompleted += ping_completed;
            pingOptions = new PingOptions(n+1, true); // TTL
        }

        public void Run(IPAddress ipAddress, int timeout) {
            ping.SendAsync(ipAddress, timeout, data, pingOptions, null);
            startTime = DateTime.Now.Ticks;
        }

        private void ping_completed(object sender, PingCompletedEventArgs e) {
            stopTime = DateTime.Now.Ticks;
            if (e.Reply == null) return;
            
            currentIp = e.Reply.Address;
            switch (e.Reply.Status) {
                case IPStatus.Success:
                    Status = PingStatus.Finish;
                    break;
                case IPStatus.TtlExpired:
                    Status = PingStatus.TtlExpired;
                    break;
                case IPStatus.TimedOut:
                    Status = PingStatus.Timedout;
                    break;
                default:
                    Status = PingStatus.Unknown;
                    break;
            }
        }
        
        public void Dispose() {
            ping.Dispose();
        }

        public void Disp() {
            if (Status == PingStatus.Waiting || Status == PingStatus.Timedout) {
                Console.WriteLine(" {0,2}\t *\t request timed out.", n + 1);
            }
            else {
                float nsec = stopTime - startTime;
                var msec = (long) nsec / 10000;
                Console.WriteLine(" {0,2}\t{1,0} ms\t{2}\t  {3}", 
                    n + 1, msec, currentIp, 
                    NetUtils.GetIpLocation(currentIp.ToString()));
            }
        }
    }
    
    public class PingAction : IDisposable {
        private readonly List<PingRun> pingRuns = new List<PingRun>();

        private const int TimeWaitOtherPingRuns = 300; // ms

        public PingAction(int hop) {
            for (var i = 0; i < hop; i++) {
                pingRuns.Add(new PingRun(i));
            }
        }

        public void Run(IPAddress ipAddress, int timeout) {
            pingRuns.ForEach(pingRun => {
                pingRun.Run(ipAddress, timeout);
            });

            while (!IsFinished()) {
                // wait...
            }
        }

        private bool IsFinished() {
            if (pingRuns.All(pingRun => pingRun.Status != PingStatus.Finish))
                return pingRuns.All(pingRun => pingRun.Status != PingStatus.Waiting);
            Thread.Sleep(TimeWaitOtherPingRuns);
            return true;
        }
        
        public void Dispose() {
            pingRuns.ForEach(pingRun => pingRun.Dispose());
        }

        public void Disp() {
            foreach (var pingRun in pingRuns) {
                pingRun.Disp();
                if (pingRun.Status == PingStatus.Finish) {
                    break;
                }
            }
            
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("traceroute ended.");
        }
    }
}