using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AzureRBLCheck
{
    class Azure
    {
        // The Azure objects
        CloudStorageAccount storageAccount;
        CloudTableClient tableClient;
        CloudTable HostTable;
        CloudTable RBLTable;

        public Azure(string Name, string Key)
        {
            // Initialize the storage account
            this.storageAccount = new CloudStorageAccount(
                new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(Name, Key), true);

            // Create the table clients
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

        public bool ExistsHost(string IP)
        {
            // Check the IP string
            Regex ipRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (!ipRegex.IsMatch(IP))
                throw new Exception("The supplied IP is not valid.");

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
                    if (he.PartitionKey.Equals(IP))
                        return true;
                }
            } while (token != null);

            // Return the result
            return false;
        }

        public void AddHost(string Hostname, string IPAddress)
        {
            // Create a host entity
            HostEntity host = new HostEntity();
            host.PartitionKey = IPAddress;
            host.RowKey = Hostname;

            // Create the TableOperation object that inserts the host entity.
            TableOperation insertOperation = TableOperation.Insert(host);

            // Check if the host is already added.
            if (ExistsHost(IPAddress))
                throw new Exception("Host already exists.");

            // Execute the insert operation.
            var result = HostTable.ExecuteAsync(insertOperation).Result;
        }

        public void RemoveHost(string IPAddress)
        {
            // Check if the host exists
            if (!ExistsHost(IPAddress))
                throw new Exception("Host does not exist.");

            // Create the table query.
            TableQuery<HostEntity> rangeQuery = new TableQuery<HostEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, IPAddress));

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<HostEntity> resultSegment = HostTable.ExecuteQuerySegmentedAsync(rangeQuery, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (HostEntity entity in resultSegment.Results)
                {
                    TableOperation removeOperation = TableOperation.Delete(entity);
                    HostTable.ExecuteAsync(removeOperation);
                }
            } while (token != null);
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
