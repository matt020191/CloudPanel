using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using CloudPanel.Base.Database.Models;
using System.Collections;

namespace CloudPanel.Citrix
{
    public class XenDesktop7 : CitrixPowershell
    {
        private readonly ILog logger = LogManager.GetLogger("Citrix");

        public XenDesktop7(string uri, string username, string password) : base(uri, username, password)
        {
            logger.DebugFormat("Opening XenDesktop7 class with {0}, {1}, <password not shown>", uri, username);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Add-PSSnapIn");
            cmd.AddParameter("Name", "Citrix.*.Admin.V*");
            _powershell.Commands = cmd;
            _powershell.Invoke();
        }

        public List<CitrixDesktopGroups> GetDesktopGroups()
        {
            logger.DebugFormat("Querying desktop groups from Citrix");

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerDesktopGroup");
            _powershell.Commands = cmd;

            List<CitrixDesktopGroups> desktopGroups = new List<CitrixDesktopGroups>();

            var objects = _powershell.Invoke();
            foreach (PSObject desktopGroup in objects)
            {
                logger.DebugFormat("Found a total of {0} desktop groups", objects.Count);

                var newDesktopGroup = new CitrixDesktopGroups();
                newDesktopGroup.Uid = (int)desktopGroup.Properties["Uid"].Value;
                newDesktopGroup.UUID = (Guid)desktopGroup.Properties["UUID"].Value;
                newDesktopGroup.Name = desktopGroup.Properties["Name"].Value.ToString();
                newDesktopGroup.PublishedName = desktopGroup.Properties["PublishedName"].Value.ToString();
                newDesktopGroup.IsEnabled = (bool)desktopGroup.Properties["Enabled"].Value;
                newDesktopGroup.LastRetrieved = DateTime.Now;

                if (desktopGroup.Properties["Description"].Value != null)
                    newDesktopGroup.Description = desktopGroup.Properties["Description"].Value.ToString();

                desktopGroups.Add(newDesktopGroup);
            }

            return desktopGroups;
        }

        public List<CitrixDesktops> GetDesktops(int desktopGroupUid)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerDesktop");
            cmd.AddParameter("DesktopGroupUid", desktopGroupUid);
            _powershell.Commands = cmd;

            List<CitrixDesktops> desktops = new List<CitrixDesktops>();

            var objects = _powershell.Invoke();
            foreach (PSObject desktop in objects)
            {
                var newDesktop = new CitrixDesktops();
                newDesktop.Uid = (int)desktop.Properties["Uid"].Value;
                newDesktop.SID = desktop.Properties["SID"].Value.ToString();
                newDesktop.MachineUid = (int)desktop.Properties["MachineUid"].Value;
                newDesktop.MachineName = desktop.Properties["MachineName"].Value.ToString();
                newDesktop.DesktopGroupID = desktopGroupUid;
                newDesktop.CatalogUid = (int)desktop.Properties["CatalogUid"].Value;
                newDesktop.CatalogName = desktop.Properties["CatalogName"].Value.ToString();
                newDesktop.LastRetrieved = DateTime.Now;
                
                if (desktop.Properties["IPAddress"].Value != null)
                    newDesktop.IPAddress = desktop.Properties["IPAddress"].Value.ToString();

                if (desktop.Properties["DNSName"].Value != null)
                newDesktop.DNSName = desktop.Properties["DNSName"].Value.ToString();

                if (desktop.Properties["OSType"].Value != null)
                    newDesktop.OSType = desktop.Properties["OSType"].Value.ToString();

                if (desktop.Properties["OSVersion"].Value != null)
                    newDesktop.OSVersion = desktop.Properties["OSVersion"].Value.ToString();

                if (desktop.Properties["AgentVersion"].Value != null)
                    newDesktop.AgentVersion = desktop.Properties["AgentVersion"].Value.ToString();

                desktops.Add(newDesktop);
            }

            return desktops;
        }

        public List<CitrixApplications> GetApplications(int desktopGroupUid)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerApplication");
            cmd.AddParameter("DesktopGroupUid", desktopGroupUid);
            _powershell.Commands = cmd;

            List<CitrixApplications> applications = new List<CitrixApplications>();

            var objects = _powershell.Invoke();
            foreach (PSObject application in objects)
            {
                var newApp = new CitrixApplications();
                newApp.Uid = (int)application.Properties["Uid"].Value;
                newApp.UUID = (Guid)application.Properties["UUID"].Value;
                newApp.Name = application.Properties["Name"].Value.ToString();
                newApp.PublishedName = application.Properties["PublishedName"].Value.ToString();
                newApp.IsEnabled = (bool)application.Properties["Enabled"].Value;
                newApp.ApplicationName = application.Properties["ApplicationName"].Value.ToString();
                newApp.LastRetrieved = DateTime.Now;

                applications.Add(newApp);
            }

            return applications;
        }
    }
}
