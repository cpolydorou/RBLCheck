using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

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
        CloudTable DomainTable;
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
            DomainTable = tableClient.GetTableReference("Domains");
        }
        #endregion

        #region Host related functions
        /// <summary>
        /// Get all hosts
        /// </summary>
        /// <returns></returns>
        public List<Host> GetHosts()
        {
            // A list to save the results
            List<Host> result = new List<Host>();

            // Construct the query operation for all host entities.
            TableQuery<HostEntity> query = new TableQuery<HostEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            // Get each host.
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

            // Construct the query operation for all host entities where the PartitionKey is equal to the IP.
            TableQuery<HostEntity> query = new TableQuery<HostEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, IP));

            // Get the hosts.
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

            // Search for the host and remove it.
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
            // A list to save the results
            List<RBL> result = new List<RBL>();

            // Construct the query operation for all RBL entities.
            TableQuery<RBLEntity> query = new TableQuery<RBLEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            // Get each RBL
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<RBLEntity> resultSegment = RBLTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (RBLEntity re in resultSegment.Results)
                {
                    RBL r = new RBL(re.PartitionKey, re.RowKey.ToLower(), re.Type);
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
            // Construct the query operation to get the RBL.
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
        /// <param name="Type"></param>
        public void AddRBL(string Name, string FQDN, string Type)
        {
            // Create a RBL entity
            RBLEntity rbl = new RBLEntity();
            rbl.PartitionKey = Name;
            rbl.RowKey = FQDN.ToLower();
            rbl.Type = Type.ToUpper();

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
            // Check if the RBL exists
            if (!ExistsRBL(FQDN))
                throw new Exception("RBL does not exist.");

            // Create the table query.
            TableQuery<RBLEntity> rangeQuery = new TableQuery<RBLEntity>().Where(
                TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, FQDN.ToLower()));

            // Search for and remove the RBL.
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

        #region Domain related functions
        /// <summary>
        /// Get all domains
        /// </summary>
        /// <returns></returns>
        public List<Domain> GetDomains()
        {
            // A list to save the results
            List<Domain> result = new List<Domain>();

            // Construct the query operation for all domain entities.
            TableQuery<DomainEntity> query = new TableQuery<DomainEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.NotEqual, ""));

            // Get all domains.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<DomainEntity> resultSegment = DomainTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (DomainEntity de in resultSegment.Results)
                {
                    Domain d = new Domain(de.PartitionKey);
                    result.Add(d);
                }
            } while (token != null);

            // Return the result
            return result;
        }

        /// <summary>
        /// Check if a domain exists
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public bool ExistsDomain(string Name)
        {
            // Construct the query operation for all domain entities based on the PartitionKey.
            TableQuery<DomainEntity> query = new TableQuery<DomainEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Name.ToLower()));

            // Get the domains.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<DomainEntity> resultSegment = DomainTable.ExecuteQuerySegmentedAsync(query, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (DomainEntity de in resultSegment.Results)
                {
                    if (de.PartitionKey.ToLower().Equals(Name.ToLower()))
                        return true;
                }
            } while (token != null);

            // Return the result
            return false;
        }

        /// <summary>
        /// Add a domain
        /// </summary>
        /// <param name="Name"></param>
        public void AddDomain(string Name)
        {
            // Create a domain entity
            DomainEntity domain = new DomainEntity();
            domain.PartitionKey = Name.ToLower();
            domain.RowKey = Name.ToLower();

            // Create the TableOperation object that inserts the domain entity.
            TableOperation insertOperation = TableOperation.Insert(domain);

            // Check if the domain is already added.
            if (ExistsDomain(Name))
                throw new Exception("Domain already exists.");

            // Execute the insert operation.
            var result = DomainTable.ExecuteAsync(insertOperation).Result;
        }

        /// <summary>
        /// Remove a domain
        /// </summary>
        /// <param name="Name"></param>
        public void RemoveDomain(string Name)
        {
            // Check if the domain exists
            if (!ExistsDomain(Name))
                throw new Exception("Domain does not exist.");

            // Create the table query.
            TableQuery<DomainEntity> rangeQuery = new TableQuery<DomainEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, Name.ToLower()));

            // Get the domain.
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<DomainEntity> resultSegment = DomainTable.ExecuteQuerySegmentedAsync(rangeQuery, token).Result;
                token = resultSegment.ContinuationToken;

                foreach (DomainEntity entity in resultSegment.Results)
                {
                    TableOperation removeOperation = TableOperation.Delete(entity);
                    DomainTable.ExecuteAsync(removeOperation);
                }
            } while (token != null);
        }
        #endregion#endregion
        #endregion
    }

    #region Table Entities
    /// <summary>
    /// Table entity for the Hosts table
    /// </summary>
    public class HostEntity : TableEntity
    {
    }

    /// <summary>
    /// Table entity for the RBLs table
    /// </summary>
    public class RBLEntity : TableEntity
    {
        public string Type { get; set; }
    }

    /// <summary>
    /// Table entity for the Domains table
    /// </summary>
    public class DomainEntity : TableEntity
    {
    }
    #endregion
}
