using CloudPanel.Citrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using CloudPanel.Base.Citrix;
using CloudPanel.Base.Database.Models;

namespace CloudPanel.Demo.Citrix.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("START");

            try
            {
                Console.WriteLine("Connecting to Citrix");
                XenDesktop7 xd = new XenDesktop7("http://##########.cloud.local:5985/wsman", @"###############", "##########################");

                Console.WriteLine("Getting desktop groups");
                var desktopGroups = xd.GetDesktopGroups();

                Console.WriteLine("Found a total of {0} desktop groups", desktopGroups.Count);
                foreach (var desktopGroup in desktopGroups)
                {
                    Console.WriteLine("Getting desktops for {0}", desktopGroup.Name);
                    var desktops = xd.GetDesktops(desktopGroup.Uid);

                    Console.WriteLine("Found a total of {0} desktops for group {1}", desktops.Count, desktopGroup.Name);

                    Console.WriteLine("Getting applications for {0}", desktopGroup.Name);
                    var applications = xd.GetApplications(desktopGroup.Uid);

                    Console.WriteLine("Found a total of {0} applications for group {1}", applications.Count, desktopGroup.Name);
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