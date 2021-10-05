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
using System.Linq;

namespace AzureRBLCheck.Functions
{
    public static class CheckDomain
    {
        [FunctionName("CheckDomain")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, Microsoft.Azure.WebJobs.ExecutionContext context)
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

            // Get the parallelization configuration
            int maxThreads = 0;
            try
            {
                int.TryParse(config["Threads"], out maxThreads);
            }
            catch(Exception e)
            {
                maxThreads = 20;
            }

            // Get the values from the request
            string name = req.Query["Name"];

            // Validate the input
            if (name == null)
                return (ActionResult)new BadRequestObjectResult("Failed to get the domain. Name cannot be null.");

            // Create the resources
            Azure az = new Azure(storageAccountName, storageAccountKey);

            // Check if the domain exists
            if (!az.ExistsDomain(name))
                return (ActionResult)new BadRequestObjectResult("The supplied domain does not exist.");

            // Read the RBLs from the configuration
            List<RBL> MyRBLs = az.GetRBLs().Where(x => x.Type.ToUpper().Equals("DOMAIN")).ToList();

            // Process each host
            log.LogInformation($"Processing domain: {name}");

            // The results
            List<RBLDomainResult> results = new List<RBLDomainResult>();

            // Create the tasks
            List<Task<RBLDomainResult>> tasks = new List<Task<RBLDomainResult>>();
            foreach (RBL r in MyRBLs)
            {
                tasks.Add(new Task<RBLDomainResult>(() => r.QueryDomain(name)));
            }

            // Start the first batch of tasks
            for (int i = 0; i < maxThreads && i < tasks.Count; i++)
            {
                tasks[i].Start();
            }

            // While there are tasks that are not completed,
            while (tasks.Count > 0)
            {
                // Wait for a task to complete
                var t = await Task.WhenAny(tasks);
                tasks.Remove(t);

                // Process the result
                try
                {
                    results.Add(t.Result);
                }
                catch (OperationCanceledException) { }
                catch (Exception exc) { }

                // Start the next task
                if (tasks.Count > 0)
                {
                    foreach (Task<RBLDomainResult> nextTask in tasks)
                    {
                        if (nextTask.Status == TaskStatus.Created)
                        {
                            nextTask.Start();
                            break;
                        }
                    }
                }
            }

            // Wait for all tasks (in case we missed one).
            Task.WaitAll(tasks.ToArray());

            // Log the end
            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");

            // Return the result
            var jsonToReturn = JsonConvert.SerializeObject(results);
            return (ActionResult)new OkObjectResult(jsonToReturn);
        }
    }
}
