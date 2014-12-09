using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;

namespace CloudPanel.Citrix
{
    public class XenDesktop7 : CitrixPowershell
    {
        public XenDesktop7(string uri, string username, string password) : base(uri, username, password)
        {
        }

        public string[] GetCatalogs()
        {
            Console.WriteLine("Getting catalogs");

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerCatalog");
            _powershell.Commands = cmd;

            List<string> catalogs = new List<string>();
            var objects = _powershell.Invoke();
            foreach (var o in objects)
            {
                catalogs.Add(o.Properties["Name"].Value.ToString());
            }

            return catalogs.ToArray();
        }
    }
}
