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
using System.Text.RegularExpressions;

namespace AzureRBLCheck
{
    public static class AddHost
    {
        [FunctionName("AddHost")]
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
            catch(Exception e)
            {
                return (ActionResult)new BadRequestObjectResult("Failed to initialize the Azure interface. " + e.Message);
            }

            // Get the values from the request
            string hostname = req.Query["Hostname"];
            string ip = req.Query["IP"];

            // Validate the input
            if (hostname == null)
                return (ActionResult)new BadRequestObjectResult("Failed to add the host. The supplied hostname is not valid.");
            if (ip == null)
                return (ActionResult)new BadRequestObjectResult("Failed to add the host. The supplied IP is not valid.");
            Regex ipRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if(!ipRegex.IsMatch(ip))
                return (ActionResult)new BadRequestObjectResult("Failed to add the host. The supplied IP is not valid.");

            // Add the host
            try
            {
                az.AddHost(hostname, ip);
            }
            catch(Exception e)
            {
                return (ActionResult)new BadRequestObjectResult("Failed to add the host. " + e.Message);
            }

            // Return a success result
            return new OkObjectResult($"The host {hostname} - {ip} has been added.");
        }
    }
}
