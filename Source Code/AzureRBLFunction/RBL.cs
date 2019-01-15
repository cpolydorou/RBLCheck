using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace AzureRBLFunction
{
    public class RBL
    {
        #region Properties
        public string Name { get; set; }
        public string fqdn { get; set; }
        #endregion

        #region Methods
        public RBLResult Query(string IP)
        {
            // Create the result object
            RBLResult result = new RBLResult(this.Name, IP);

            // Create the record to query
            string[] parts = IP.Split('.');
            string record = parts[3] + '.' + parts[2] + '.' + parts[1] + '.' + parts[0] + '.' + fqdn;
            
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
        #endregion
    }
}
