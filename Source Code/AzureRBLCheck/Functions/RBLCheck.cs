using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AzureRBLCheck
{
    public static class RBLCheck
    {
        [FunctionName("RBLCheck")]
        public static void Run([TimerTrigger("0 */30 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            // Log the start
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Temporarily disable the task
            log.LogInformation($"This task has been temporarily disabled");
            return;


            // Initialize the configuration object
            var config = new ConfigurationBuilder()
                             .SetBasePath(context.FunctionAppDirectory)
                             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables()
                             .Build();

            // Get the storage account information
            string storageAccountName = config["StorageAccountName"];
            string storageAccountKey = config["StorageAccountKey"];

            // Create the resources
            Azure az = new Azure(storageAccountName, storageAccountKey);

            // Read the RBLs from the configuration
            List<RBL> MyRBLs = az.GetRBLs();

            // Read the hosts from the configuration
            List<Host> MyHosts = az.GetHosts();

            // Process each host
            foreach (Host host in MyHosts)
            {
                log.LogInformation($"Processing host: {host.Name}");

                foreach (RBL l in MyRBLs)
                {
                    RBLHostResult r = l.QueryHost(host.IP);

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
