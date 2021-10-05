using System;
using System.Collections.Generic;
using System.Text;

namespace AzureRBLCheck
{
    public class Host
    {
        public string Name { get; set; }
        public string IP { get; set; }

        public Host(string Name, string IP)
        {
            this.Name = Name;
            this.IP = IP;
        }
    }
}
