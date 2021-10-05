using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AzureRBLCheck
{
    public class RBL
    {
        #region Properties
        public string Name { get; set; }
        public string FQDN { get; set; }
        public string Type { get; set; }
        #endregion

        #region Methods

        #region Constructors
        public RBL(string Name, string FQDN, string Type)
        {
            this.Name = Name;
            this.FQDN = FQDN;
            this.Type = Type;
        }
        #endregion

        #region RBL related
        /// <summary>
        /// Query a RBL
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        public RBLHostResult QueryHost(string IP)
        {
            // Create the result object
            RBLHostResult result = new RBLHostResult(this.Name, IP);

            // Create the record to query
            string[] parts = IP.Split('.');
            string record = parts[3] + '.' + parts[2] + '.' + parts[1] + '.' + parts[0] + '.' + FQDN;
            
            // Query the list
            try
            {
                IPHostEntry host = Dns.GetHostEntry(record);

                if (host.AddressList != null)
                {
                    // The host is found
                    result.IsListed = true;
                    result.Details = string.Empty;
                }
            }
            catch (Exception e)
            {
                if(e.Message.Equals("No such host is known"))
                {
                    result.IsListed = false;
                    result.Details = string.Empty;
                }
            }

            if(result.IsListed)
            {
                // Query the txt record
            }

            // Return the result
            return result;
        }
        public RBLDomainResult QueryDomain(string Name)
        {
            // Create the result object
            RBLDomainResult result = new RBLDomainResult(this.Name, Name);

            // Create the record to query            
            string record = Name + '.' + FQDN;

            // Query the list
            try
            {
                IPHostEntry host = Dns.GetHostEntry(record);

                if (host.AddressList != null)
                {
                    // The host is found
                    result.IsListed = true;
                    result.Details = string.Empty;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Equals("No such host is known"))
                {
                    result.IsListed = false;
                    result.Details = string.Empty;
                }
            }

            if (result.IsListed)
            {
                // Query the txt record
            }

            // Return the result
            return result;
        }
        #endregion
        #endregion
    }
}
