using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureRBLCheck
{
    public class RBLResult
    {
        #region Properties
        public string RBL { get; set; }
        public string Host { get; set; }
        public bool IsListed { get; set; }
        public string Details { get; set; }
        #endregion

        #region Methods
        #region Constructors
        public RBLResult() { }
        public RBLResult(string RBLName, string Host)
        {
            this.RBL = RBLName;
            this.Host = Host;
        }
        #endregion
        #endregion
    }
}
