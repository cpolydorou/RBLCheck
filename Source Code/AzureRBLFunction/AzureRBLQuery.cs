using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AzureRBLFunction
{
    public static class AzureRBLQuery
    {
        [FunctionName("AzureRBLQuery")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // Log the start
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            // Read the RBLs from the configuration
            List<RBL> MyRBLs = ReadRBLs();

            // Read the hosts from the configuration
            List<string> MyHosts = ReadHosts();

            // Process each host
            foreach(string host in MyHosts)
            {
                log.Info($"Processing host: {host}");

                foreach(RBL l in MyRBLs)
                {
                    RBLResult r = l.Query(host);

                    if(r.IsListed)
                        log.Info($"Host {r.Host} is listed on {r.RBL}");
                    else
                        log.Info($"Host {r.Host} is NOT listed on {r.RBL}");
                }
            }

            // Log the end
            log.Info($"C# Timer trigger function completed at: {DateTime.Now}");
        }

        static List<string> ReadHosts()
        {
            List<string> Hosts = new List<string>();
            Hosts.Add("195.167.99.198");
            Hosts.Add("195.167.99.199");

            return Hosts;
        }

        static List<RBL> ReadRBLs()
        {
            List<RBL> RBLs = new List<RBL>();
            RBL MyRBL = new RBL();
            MyRBL.Name = "SpamHaus Zen";
            MyRBL.fqdn = "zen.spamhaus.org";
            RBLs.Add(MyRBL);

            return RBLs;
        }

    }
}
