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
    public static class RemoveDomain
    {
        [FunctionName("RemoveDomain")]
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
            if (storageAccountKey == null)
                return (ActionResult)new BadRequestObjectResult("Failed to read the storage account key from the configuration.");
            if (storageAccountName == null)
                return (ActionResult)new BadRequestObjectResult("Failed to read the storage account name from the configuration.");

            // Create the resources
            Azure az;
            try
            {
                az = new Azure(storageAccountName, storageAccountKey);
            }
            catch (Exception e)
            {
                return (ActionResult)new BadRequestObjectResult("Failed to initialize the Azure interface. " + e.Message);
            }

            // Get the values from the request
            string name = req.Query["Name"];
            if (name == null)
                return (ActionResult)new BadRequestObjectResult("Failed to get the domain name from the request.");

            // Remove the domain
            try
            {
                az.RemoveDomain(name);
            }
            catch (Exception e)
            {
                return (ActionResult)new BadRequestObjectResult("Failed remove the domain. " + e.Message);
            }

            // Return the result
            return (ActionResult)new OkObjectResult($"The domain {name} has been removed");
        }
    }
}
