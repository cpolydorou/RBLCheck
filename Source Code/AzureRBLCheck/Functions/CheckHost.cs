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
using System.Threading;
using System.Collections.Concurrent;
using System.Linq;

namespace AzureRBLCheck
{
    public static class CheckHost
    {
        [FunctionName("CheckHost")]
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
            int maxThreads = int.Parse(config["Threads"]);

            // Get the values from the request
            string ip = req.Query["IP"];

            // Validate the input
            if (ip == null)
                return (ActionResult)new BadRequestObjectResult("Failed to get host. IP cannot be null.");

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

            // Create the tasks
            //            Task<RBLResult>[] tasks = new Task<RBLResult>[MyRBLs.Count];                
            List<Task<RBLResult>> tasks = new List<Task<RBLResult>>();
            foreach(RBL r in MyRBLs)
            {
                tasks.Add(new Task<RBLResult>(() => r.Query(ip)));
            }

            // Start the first batch of tasks
            for(int i=0; i<maxThreads && i<tasks.Count; i++)
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
                if(tasks.Count > 0)
                {
                    foreach(Task<RBLResult> nextTask in tasks)
                    {
                        if(nextTask.Status == TaskStatus.Created)
                        {
                            nextTask.Start();
                            break;
                        }
                    }
                }
            }

            Task.WaitAll(tasks.ToArray());



            /*
            var maxThreads = 4;
            var q = new ConcurrentQueue<RBL>(MyRBLs);
            var tasks = new List<Task>();
            for (int n = 0; n < maxThreads; n++)
            {
                tasks.Add(Task<RBLResult>.Run(async () => {
                    while (q.TryDequeue(out RBL r))
                    {
                        RBLResult rs = r.Query(ip);
                        return rs;
                    }
                    return null;
                }));
            }
            await Task.WhenAll(tasks);
            */








            /*

            // Create the tasks
            Task<RBLResult>[] tasks = new Task<RBLResult>[MyRBLs.Count];
            for(int i = 0; i< MyRBLs.Count; i++)
            {
                ThreadInfo ti = new ThreadInfo();
                ti.rbl = MyRBLs[i];
                ti.host = ip;

                tasks[i] = new Task<RBLResult>(() => ProcessHost(ti));
            }

            // Run the tasks
            int completedTasks = 0;
            int currentTask = 0;
            int runningTasks = 0;
            int maxThreads = int.Parse(config["Threads"]);

            // Start the first batch
            for(int i=0; i<maxThreads; i++)
            {
                if (i >= MyRBLs.Count)
                    break;
                else
                {
                    currentTask = i;
                    runningTasks++;
                    tasks[i].Start();
                }
            }

            */


            /*
                // ***Create a query that, when executed, returns a collection of tasks.
            IEnumerable<Task<RBLResult>> tasksQuery =
                from rbl in MyRBLs
                select Process(rbl, ip);

            // ***Use ToList to execute the query and start the tasks.
            List<Task<RBLResult>> tasks = tasksQuery.ToList();

            // ***Add a loop to process the tasks one at a time until none remain.
            while (tasks.Count > 0)
            {
                // Identify the first task that completes.
                Task<RBLResult> firstFinishedTask = await Task.WhenAny(tasks);

                // ***Remove the selected task from the list so that you don't
                // process it more than once.
                tasks.Remove(firstFinishedTask);

                // Await the completed task.
                RBLResult r = await firstFinishedTask;
                results.Add(r);
            }
            */


            /*



            while (completedTasks < MyRBLs.Count)
            {

                //var finishedTask = tasks[Task.WaitAny(tasks)];
                //tasks = tasks.Except(new[] { finishedTask }).ToArray();

                // Wait for any task to complete
                int completed = Task.WaitAny(tasks);

                // Get the result
                results.Add(tasks[completed].Result);

                // Increase the completed task counter
                completedTasks++;

                // Search for the next task to start
                for (int i = 0; i < tasks.Length; i++)
                {
                    if (!tasks[i].IsCompleted && tasks[i].Status != TaskStatus.Running)
                    {
                        tasks[i].Start();
                        break;
                    }
                }
            }


                int ia = 0;

            /*
            foreach (RBL l in MyRBLs)
            {


                // Declare a new argument object.
                ThreadInfo threadInfo = new ThreadInfo();
                threadInfo.rbl = l;
                threadInfo.host = ip;

                // Send the custom object to the threaded method.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessHost), threadInfo);

            }
            */


            /*
            foreach (RBL l in MyRBLs)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(TaskCallBack), l);

                RBLResult r = l.Query(ip);
                results.Add(r);

                if (r.IsListed)
                    log.LogInformation($"\tHost {r.Host} is listed on {r.RBL}");
                else
                    log.LogInformation($"\tHost {r.Host} is NOT listed on {r.RBL}");
            }
            */

            // Log the end
            log.LogInformation($"C# Timer trigger function completed at: {DateTime.Now}");

            // Return the result
            var jsonToReturn = JsonConvert.SerializeObject(results);
            return (ActionResult)new OkObjectResult(jsonToReturn);
        }




        class ThreadInfo
        {
            public RBL rbl { get; set; }
            public string host { get; set; }

        }


        private static async Task<RBLResult> Process(RBL r, string IP)
        {
            return r.Query(IP);
        }

        private static RBLResult ProcessHost(ThreadInfo a)
        {
            // Constrain the number of worker threads
            // (Omitted here.)

            // We receive the threadInfo as an uncasted object.
            // Use the 'as' operator to cast it to ThreadInfo.
            ThreadInfo threadInfo = a as ThreadInfo;

            return threadInfo.rbl.Query(threadInfo.host);
            //string fileName = threadInfo.FileName;
            //int index = threadInfo.SelectedIndex;
        }

    }
}
