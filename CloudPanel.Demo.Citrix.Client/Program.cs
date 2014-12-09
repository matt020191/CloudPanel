using CloudPanel.Citrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace CloudPanel.Demo.Citrix.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("START");

            try
            {
                XenDesktop7 xd = new XenDesktop7("http://server2.domain.local:5985/wsman", @"DOMAIN\Administrator", "#######");
                string[] blah = xd.GetCatalogs();

                foreach (var b in blah)
                {
                    Console.WriteLine(b);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadKey();
        }
    }
}