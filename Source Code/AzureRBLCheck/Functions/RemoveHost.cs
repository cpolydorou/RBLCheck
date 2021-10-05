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

namespace AzureRBLCheck
{
    public static class RemoveHost
    {
        [FunctionName("RemoveHost")]
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

            // Check the supplied values
            if(storageAccountKey == null)
                return (ActionResult)new BadRequestObjectResult("Failed to read the storage account key from the configuration.");
            if(storageAccountName == null)
                return (ActionResult)new BadRequestObjectResult("Failed to read the storage account name from the configuration.");

            // Create the resources
            Azure az;
            try
            {
                az = new Azure(storageAccountName, storageAccountKey);
            }
            catch(Exception e)
            {
                return (ActionResult)new BadRequestObjectResult("Failed to initialize the Azure interface. " + e.Message);
            }

            // Get the values from the request
            string ip = req.Query["IP"];
            if(ip == null)
                return (ActionResult)new BadRequestObjectResult("Failed to get the host IP from the request.");

            // Remove the host
            try
            {
                az.RemoveHost(ip);
            }
            catch(Exception e)
            {
                return (ActionResult)new BadRequestObjectResult("Failed remove the host. " + e.Message);
            }

            // Return the result
            return (ActionResult)new OkObjectResult($"The host {ip} has been removed");
        }
    }
}
