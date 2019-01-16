using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AzureRBLCheck
{
    public static class GetHosts
    {
        [FunctionName("GetHosts")]
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

            // Create the resources
            Azure az = new Azure(storageAccountName, storageAccountKey);

            // Get the hosts
            List<Host> hosts = az.GetHosts();

            // Log the end
            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");

            // Return the result
            var jsonToReturn = JsonConvert.SerializeObject(hosts);
            return (ActionResult)new OkObjectResult(jsonToReturn);
        }
    }
}
