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
        #region Properties
        // The Azure objects
        CloudStorageAccount storageAccount;
        CloudTableClient tableClient;
        CloudTable HostTable;
        CloudTable RBLTable;
        #endregion

        #region Methods
        #region Constructors
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
        #endregion

        #region Host related functions
        /// <summary>
        /// Get all hosts
        /// </summary>
        /// <returns></returns>
        public List<Host> GetHosts()
        {
            // The results
            List<Host> result = new List<Host>();

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
                    Host s = new Host(he.RowKey, he.PartitionKey);
                    result.Add(s);
                }
            } while (token != null);

            // Return the result
            return result;
        }

        /// <summary>
        /// Check if a host exists
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public bool ExistsHost(string IP)
        {
            // Check the IP string
            Regex ipRegex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            if (!ipRegex.IsMatch(IP))
                throw new Exception("The supplied IP is not valid.");

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<HostEntity> query = new TableQuery<HostEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, IP));

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

        /// <summary>
        /// Add a host
        /// </summary>
        /// <param name="Hostname"></param>
        /// <param name="IPAddress"></param>
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

        /// <summary>
        /// Remove a host
        /// </summary>
        /// <param name="IPAddress"></param>
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
        #endregion

        #region RBL related functions
        /// <summary>
        /// Get all RBLs
        /// </summary>
        /// <returns></returns>
        public List<RBL> GetRBLs()
        {
            // The results
            List<RBL> result = new List<RBL>();

            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<RBLEntity> query = new TableQuery<RBLEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<RBLEntity> resultSegment = RBLTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (RBLEntity re in resultSegment.Results)
                {
                    RBL r = new RBL(re.PartitionKey, re.RowKey.ToLower());
                    result.Add(r);
                }
            } while (token != null);

            // Return the result
            return result;
        }

        /// <summary>
        /// Check if RBL exists
        /// </summary>
        /// <param name="FQDN"></param>
        /// <returns></returns>
        public bool ExistsRBL(string FQDN)
        {
            // Construct the query operation for all customer entities where PartitionKey="Smith".
            TableQuery<RBLEntity> query = new TableQuery<RBLEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, FQDN.ToLower()));

            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<RBLEntity> resultSegment = RBLTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (RBLEntity re in resultSegment.Results)
                {
                    if (re.RowKey.ToLower().Equals(FQDN.ToLower()))
                        return true;
                }
            } while (token != null);

            // Return the result
            return false;
        }

        /// <summary>
        /// Add a RBL
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="FQDN"></param>
        public void AddRBL(string Name, string FQDN)
        {
            // Create a RBL entity
            RBLEntity rbl = new RBLEntity();
            rbl.PartitionKey = Name;
            rbl.RowKey = FQDN.ToLower();

            // Create the TableOperation object that inserts the host entity.
            TableOperation insertOperation = TableOperation.Insert(rbl);

            // Check if the host is already added.
            if (ExistsRBL(FQDN))
                throw new Exception("RBL already exists.");

            // Execute the insert operation.
            var result = RBLTable.ExecuteAsync(insertOperation).Result;
        }

        /// <summary>
        /// Remove a RBL
        /// </summary>
        /// <param name="FQDN"></param>
        public void RemoveRBL(string FQDN)
        {
            // Check if the host exists
            if (!ExistsRBL(FQDN))
                throw new Exception("RBL does not exist.");

            // Create the table query.
            TableQuery<RBLEntity> rangeQuery = new TableQuery<RBLEntity>().Where(
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, FQDN.ToLower()));

            // Print the fields for each customer.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<RBLEntity> resultSegment = RBLTable.ExecuteQuerySegmentedAsync(rangeQuery, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (RBLEntity entity in resultSegment.Results)
                {
                    TableOperation removeOperation = TableOperation.Delete(entity);
                    RBLTable.ExecuteAsync(removeOperation);
                }
            } while (token != null);
        }
        #endregion
        #endregion
    }

    #region Table Entities
    public class HostEntity : TableEntity
    {
    }

    public class RBLEntity : TableEntity
    {
    }
    #endregion
}
