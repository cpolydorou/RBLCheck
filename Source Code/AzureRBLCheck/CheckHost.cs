using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AzureRBLCheck
{
    public static class CheckHost
    {
        [FunctionName("CheckHost")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            // Log the start
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Initialize the configuration object
            var config = new ConfigurationBuilder()
                             .SetBasePath(context.FunctionAppDirectory)
                             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                             .AddEnvironmentVariables()
                             .Build();

            // Get the storage account information
            string storageAccountName = config["StorageAccountName"];
            string storageAccountKey = config["StorageAccountKey"];

            // Get the values from the request
            string ip = req.Query["IP"];

            // Create the resources
            Azure az = new Azure(storageAccountName, storageAccountKey);

            // Check if the host exists
            if(!az.ExistsHost(ip))
                return (ActionResult)new BadRequestObjectResult("The supplied host does not exist.");

            // Read the RBLs from the configuration
            List<RBL> MyRBLs = az.GetRBLs();

            // Process each host
            log.LogInformation($"Processing host: {ip}");

            // The results
            List<RBLResult> results = new List<RBLResult>();

            foreach (RBL l in MyRBLs)
            {
                RBLResult r = l.Query(ip);
                results.Add(r);

                if (r.IsListed)
                    log.LogInformation($"\tHost {r.Host} is listed on {r.RBL}");
                else
                    log.LogInformation($"\tHost {r.Host} is NOT listed on {r.RBL}");
            }

            // Log the end
            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");

            // Return the result
            var jsonToReturn = JsonConvert.SerializeObject(results);
            return (ActionResult)new OkObjectResult(jsonToReturn);
        }
    }
}
