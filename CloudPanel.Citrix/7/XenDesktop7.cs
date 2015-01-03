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
using CloudPanel.Base.Citrix;
using System.Reflection;

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
            cmd.AddParameter("Name", "Citrix.Broker.Admin.V2");
            _powershell.Commands = cmd;
            _powershell.Invoke();
        }

        /// <summary>
        /// Gets the desktop groups in Citrix
        /// </summary>
        /// <returns></returns>
        public List<CitrixDesktopGroups> GetDesktopGroups()
        {
            logger.DebugFormat("Querying desktop groups from Citrix");

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerDesktopGroup");
            _powershell.Commands = cmd;

            List<CitrixDesktopGroups> desktopGroups = new List<CitrixDesktopGroups>();

            var objects = _powershell.Invoke();
            HandleErrors();

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

        /// <summary>
        /// Gets the desktops that belong to a specific desktop group
        /// </summary>
        /// <param name="desktopGroupUid"></param>
        /// <returns></returns>
        public List<CitrixDesktops> GetDesktops(int desktopGroupUid)
        {
            logger.DebugFormat("Querying desktops for desktop group uid {0}", desktopGroupUid);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerDesktop");
            cmd.AddParameter("DesktopGroupUid", desktopGroupUid);
            _powershell.Commands = cmd;

            List<CitrixDesktops> desktops = new List<CitrixDesktops>();

            var objects = _powershell.Invoke();
            HandleErrors();

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
                else
                    newDesktop.IPAddress = "Unknown";

                if (desktop.Properties["DNSName"].Value != null)
                    newDesktop.DNSName = desktop.Properties["DNSName"].Value.ToString();
                else
                    newDesktop.DNSName = "Unknown";

                if (desktop.Properties["OSType"].Value != null)
                    newDesktop.OSType = desktop.Properties["OSType"].Value.ToString();
                else
                    newDesktop.OSType= "Unknown";

                if (desktop.Properties["OSVersion"].Value != null)
                    newDesktop.OSVersion = desktop.Properties["OSVersion"].Value.ToString();
                else
                    newDesktop.OSVersion = "Unknown";

                if (desktop.Properties["AgentVersion"].Value != null)
                    newDesktop.AgentVersion = desktop.Properties["AgentVersion"].Value.ToString();
                else
                    newDesktop.AgentVersion = "Unknown";

                desktops.Add(newDesktop);
            }

            logger.DebugFormat("Returning a total of {0} desktops", desktops.Count);
            return desktops;
        }

        /// <summary>
        /// Gets the applications that belong to a specific desktop group
        /// </summary>
        /// <param name="desktopGroupUid"></param>
        /// <returns></returns>
        public List<CitrixApplications> GetApplications(int desktopGroupUid)
        {
            logger.DebugFormat("Querying applications for desktop group uid {0}", desktopGroupUid);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerApplication");
            cmd.AddParameter("DesktopGroupUid", desktopGroupUid);
            _powershell.Commands = cmd;

            List<CitrixApplications> applications = new List<CitrixApplications>();

            var objects = _powershell.Invoke();
            HandleErrors();

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

            logger.DebugFormat("Returning a total of {0} applications", applications.Count);
            return applications;
        }

        /// <summary>
        /// Gets a specific session by the session key
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <returns></returns>
        public BrokerSession GetSession(Guid sessionKey)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerSession");
            cmd.AddParameter("SessionKey", sessionKey);
            _powershell.Commands = cmd;

            BrokerSession newSession = new BrokerSession();
            var objects = _powershell.Invoke();
            HandleErrors();

            foreach (PSObject s in objects)
            {
                foreach (var prop in s.Properties)
                {
                    if (prop.Value != null)
                    {
                        logger.DebugFormat("Found property {0}", prop.Name);
                        var newSessionProp = (from p in newSession.GetType().GetProperties()
                                              where p.Name == prop.Name
                                              select p).FirstOrDefault();

                        if (newSessionProp != null)
                        {
                            logger.DebugFormat("New session property {0} exists with type {1}. Setting value to {2} with type of {3}",
                                                newSessionProp.Name,
                                                newSessionProp.PropertyType.FullName,
                                                prop.Value,
                                                prop.TypeNameOfValue);
                            newSessionProp.SetValue(newSession, prop.Value, null);
                        }
                    }
                }
            }

            return newSession;
        }

        /// <summary>
        /// Gets all sessions in Citrix
        /// </summary>
        /// <returns></returns>
        public List<BrokerSession> GetSessions()
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerSession");
            _powershell.Commands = cmd;

            List<BrokerSession> sessions = new List<BrokerSession>();

            var objects = _powershell.Invoke();
            HandleErrors();

            foreach (PSObject s in objects)
            {
                var newSession = new BrokerSession();
                foreach (var prop in s.Properties)
                {
                    if (prop.Value != null)
                    {
                        logger.DebugFormat("Found property {0}", prop.Name);
                        var newSessionProp = (from p in newSession.GetType().GetProperties()
                                              where p.Name == prop.Name
                                              select p).FirstOrDefault();

                        if (newSessionProp != null)
                        {
                            logger.DebugFormat("New session property {0} exists with type {1}. Setting value to {2} with type of {3}", 
                                                newSessionProp.Name,
                                                newSessionProp.PropertyType.FullName,
                                                prop.Value,
                                                prop.TypeNameOfValue);
                            newSessionProp.SetValue(newSession, prop.Value, null);
                        }
                    }
                }

                sessions.Add(newSession);
            }

            return sessions;
        }

        /// <summary>
        /// Gets sessions from a specific desktop group
        /// </summary>
        /// <param name="desktopGroupUid"></param>
        /// <returns></returns>
        public List<BrokerSession> GetSessionsByDesktopGroup(int desktopGroupUid)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerSession");
            cmd.AddParameter("DesktopGroupUid", desktopGroupUid);
            _powershell.Commands = cmd;

            List<BrokerSession> sessions = new List<BrokerSession>();

            var objects = _powershell.Invoke();
            HandleErrors();

            foreach (PSObject s in objects)
            {
                var newSession = new BrokerSession();
                foreach (var prop in s.Properties)
                {
                    if (prop.Value != null)
                    {
                        logger.DebugFormat("Found property {0}", prop.Name);
                        var newSessionProp = (from p in newSession.GetType().GetProperties()
                                              where p.Name == prop.Name
                                              select p).FirstOrDefault();

                        if (newSessionProp != null)
                        {
                            logger.DebugFormat("New session property {0} exists with type {1}. Setting value to {2} with type of {3}",
                                                newSessionProp.Name,
                                                newSessionProp.PropertyType.FullName,
                                                prop.Value,
                                                prop.TypeNameOfValue);
                            newSessionProp.SetValue(newSession, prop.Value, null);
                        }
                    }
                }

                sessions.Add(newSession);
            }

            return sessions;
        }

        /// <summary>
        /// Gets all sessions for a specific desktop
        /// </summary>
        /// <param name="desktopUid">Desktop Uid to query the sessions for.</param>
        /// <returns></returns>
        public List<BrokerSession> GetSessionsByDesktop(int desktopUid)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-BrokerSession");
            cmd.AddParameter("DesktopUid", desktopUid);
            _powershell.Commands = cmd;

            List<BrokerSession> sessions = new List<BrokerSession>();

            var objects = _powershell.Invoke();
            HandleErrors();

            foreach (PSObject s in objects)
            {
                var newSession = new BrokerSession();
                foreach (var prop in s.Properties)
                {
                    if (prop.Value != null)
                    {
                        logger.DebugFormat("Found property {0}", prop.Name);
                        var newSessionProp = (from p in newSession.GetType().GetProperties()
                                              where p.Name == prop.Name
                                              select p).FirstOrDefault();

                        if (newSessionProp != null)
                        {
                            logger.DebugFormat("New session property {0} exists with type {1}. Setting value to {2} with type of {3}", 
                                                newSessionProp.Name,
                                                newSessionProp.PropertyType.FullName,
                                                prop.Value,
                                                prop.TypeNameOfValue);
                            newSessionProp.SetValue(newSession, prop.Value, null);
                        }
                    }
                }

                sessions.Add(newSession);
            }

            return sessions;
        }

        /// <summary>
        /// Logs off all users 
        /// </summary>
        /// <param name="values"></param>
        public void LogOffSessionsBySessionKeys(string[] sessionKeys)
        {
            logger.DebugFormat("Logging off {0} sessions", sessionKeys.Length);
            foreach (var s in sessionKeys)
            {
                logger.DebugFormat("Logging off session {0}", s);

                PSCommand cmd = new PSCommand();
                cmd.AddCommand("Get-BrokerSession");
                cmd.AddArgument(s);
                cmd.AddCommand("Stop-BrokerSession");
                _powershell.Commands = cmd;
                _powershell.Invoke();

                HandleErrors();
            }
        }

        /// <summary>
        /// Sends messages by session keys
        /// </summary>
        /// <param name="sessionKeys"></param>
        /// <param name="messageStyle"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public void SendMessageBySessionKeys(string[] sessionKeys, string messageStyle, string title, string text)
        {
            logger.DebugFormat("Sending message to {0} sessions with style {1}, title {2}, and text {3}", sessionKeys.Length, messageStyle, title, text);

            foreach (var s in sessionKeys)
            {
                logger.DebugFormat("Sending message to {0}", s);

                if (!string.IsNullOrEmpty(s))
                {
                    PSCommand cmd = new PSCommand();
                    cmd.AddCommand("Get-BrokerSession");
                    cmd.AddArgument(s);
                    cmd.AddCommand("Send-BrokerSessionMessage");
                    cmd.AddParameter("MessageStyle", messageStyle);
                    cmd.AddParameter("Title", title);
                    cmd.AddParameter("Text", text);
                    _powershell.Commands = cmd;
                    _powershell.Invoke();

                    HandleErrors();
                }
            }
        }
    }
}
