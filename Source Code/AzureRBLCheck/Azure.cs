using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureRBLCheck
{
    class Azure
    {

        CloudStorageAccount storageAccount;
        CloudTableClient tableClient;
        //        = new CloudStorageAccount(
        //    new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
        //        "cpolydorouazurerbl", "ICJj4Hc350b2OKPrdMRIcErOt/bfgzn3jGeIoJJCSPbwfdFoNuCdVfuvBK/RHBtVpll3U4hU99dbev+gsbG71w=="), true);


        // Create the table client.

        // Get a reference to a table named "peopleTable"
        CloudTable HostTable;
        CloudTable RBLTable;


        public Azure(string Name, string Key)
        {
            // Initialize the storage account
            this.storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(Name, Key), true);

            // 
            tableClient = storageAccount.CreateCloudTableClient();

            HostTable = tableClient.GetTableReference("Hosts");
            RBLTable = tableClient.GetTableReference("RBLs");

        }

        public List<string> GetHosts()
        {
            List<string> hostList = new List<string>();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<HostEntity> query = new TableQuery<HostEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<HostEntity> resultSegment = HostTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (HostEntity he in resultSegment.Results)
                {
                    hostList.Add(he.PartitionKey);
                }
            } while (token != null);

            return hostList;
        }
        public List<RBL> GetRBLs()
        {
            List<RBL> rblList = new List<RBL>();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<RBLEntity> query = new TableQuery<RBLEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<RBLEntity> resultSegment = RBLTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (RBLEntity he in resultSegment.Results)
                {
                    RBL r = new RBL();
                    r.Name = he.PartitionKey;
                    r.fqdn = he.RowKey;

                    rblList.Add(r);
                }
            } while (token != null);

            return rblList;
        }
    }

    public class HostEntity : TableEntity
    {
    }

    public class RBLEntity : TableEntity
    {
    }
}
