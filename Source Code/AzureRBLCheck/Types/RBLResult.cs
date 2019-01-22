using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRBLCheck
{
    public class RBLHostResult
    {
        #region Properties
        public string RBL { get; set; }
        public string Host { get; set; }
        public bool IsListed { get; set; }
        public string Details { get; set; }
        #endregion

        #region Methods
        #region Constructors
        public RBLHostResult() { }
        public RBLHostResult(string RBLName, string Host)
        {
            this.RBL = RBLName;
            this.Host = Host;
        }
        #endregion
        #endregion
    }

    public class RBLDomainResult
    {
        #region Properties
        public string RBL { get; set; }
        public string Domainname { get; set; }
        public bool IsListed { get; set; }
        public string Details { get; set; }
        #endregion

        #region Methods
        #region Constructors
        public RBLDomainResult() { }
        public RBLDomainResult(string RBLName, string Name)
        {
            this.RBL = RBLName;
            this.Domainname = Name;
        }
        #endregion
        #endregion
    }
}
