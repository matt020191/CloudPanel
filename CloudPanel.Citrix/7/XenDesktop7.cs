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
        public XenDesktop7(string uri, string username, string password) : base(uri, username, password)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Add-PSSnapIn");
            cmd.AddParameter("Name", "Citrix.*.Admin.V*");
            _powershell.Commands = cmd;
            _powershell.Invoke();
        }

        public List<CitrixDesktopGroups> GetDesktopGroups()
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerDesktopGroup");
            _powershell.Commands = cmd;

            List<CitrixDesktopGroups> desktopGroups = new List<CitrixDesktopGroups>();

            var objects = _powershell.Invoke();
            foreach (PSObject desktopGroup in objects)
            {
                var newDesktopGroup = new CitrixDesktopGroups();
                newDesktopGroup.Uid = (int)desktopGroup.Properties["Uid"].Value;
                newDesktopGroup.UUID = (Guid)desktopGroup.Properties["UUID"].Value;
                newDesktopGroup.Name = desktopGroup.Properties["Name"].Value.ToString();
                newDesktopGroup.PublishedName = desktopGroup.Properties["PublishedName"].Value.ToString();
                newDesktopGroup.IsEnabled = (bool)desktopGroup.Properties["Enabled"].Value;

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
                newDesktop.OSType = desktop.Properties["OSType"].Value.ToString();
                newDesktop.OSVersion = desktop.Properties["OSVersion"].Value.ToString();
                newDesktop.MachineUid = (int)desktop.Properties["MachineUid"].Value;
                newDesktop.MachineName = desktop.Properties["MachineName"].Value.ToString();
                newDesktop.IPAddress = desktop.Properties["IPAddress"].Value.ToString();
                newDesktop.DesktopGroupID = desktopGroupUid;
                newDesktop.DNSName = desktop.Properties["DNSName"].Value.ToString();
                newDesktop.CatalogUid = (int)desktop.Properties["CatalogUid"].Value;
                newDesktop.CatalogName = desktop.Properties["CatalogName"].Value.ToString();
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

                applications.Add(newApp);
            }

            return applications;
        }
    }
}
