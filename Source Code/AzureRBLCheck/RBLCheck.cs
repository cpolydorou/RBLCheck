using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureRBLCheck
{
    public static class RBLCheck
    {
        [FunctionName("RBLCheck")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            // Create the resources
            Azure az = new Azure("cpolydorouazurerbl", "ICJj4Hc350b2OKPrdMRIcErOt/bfgzn3jGeIoJJCSPbwfdFoNuCdVfuvBK/RHBtVpll3U4hU99dbev+gsbG71w==");

            // Log the start
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Read the RBLs from the configuration
            List<RBL> MyRBLs = az.GetRBLs();

            // Read the hosts from the configuration
            List<string> MyHosts = az.GetHosts();

            // Process each host
            foreach (string host in MyHosts)
            {
                log.LogInformation($"Processing host: {host}");

                foreach (RBL l in MyRBLs)
                {
                    RBLResult r = l.Query(host);

                    if (r.IsListed)
                        log.LogInformation($"\tHost {r.Host} is listed on {r.RBL}");
                    else
                        log.LogInformation($"\tHost {r.Host} is NOT listed on {r.RBL}");
                }
            }

            // Log the end
            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");
        }
    }
}
