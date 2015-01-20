﻿using CloudPanel.Base.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Scheduler
{
    public class StaticSettings
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reads the configuration file and stores the data in memory
        /// </summary>
        public static bool LoadSettings(string path)
        {
            try
            {
                XDocument xDoc = XDocument.Load(path);

                /* Load CloudPanel Settings */
                var x = from s in xDoc.Elements("cloudpanel") select s;

                // Settings node
                Settings.ConnectionString = Read(ref x, "Settings", "Database");
                Settings.CompanyName = Read(ref x, "Settings", "CompanyName");
                Settings.HostingOU = Read(ref x, "Settings", "HostingOU");
                Settings.UsersOU = Read(ref x, "Settings", "UsersOU");
                Settings.PrimaryDC = Read(ref x, "Settings", "PrimaryDC");
                Settings.Username = Read(ref x, "Settings", "Username");
                Settings.EncryptedPassword = Read(ref x, "Settings", "Password");
                Settings.SuperAdmins = Read(ref x, "Settings", "SuperAdmins").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Settings.BillingAdmins = Read(ref x, "Settings", "BillingAdmins").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Settings.ResellersEnabled = bool.Parse(Read(ref x, "Settings", "ResellersEnabled"));
                Settings.SaltKey = Read(ref x, "Settings", "SaltKey");

                // Exchange node
                Settings.ExchangeRoleAssignment = Read(ref x, "Exchange", "ExchangeRoleAssignment");
                Settings.ExchangeServer = Read(ref x, "Exchange", "ExchangeServer");
                Settings.ExchangePFServer = Read(ref x, "Exchange", "ExchangePFServer");
                Settings.ExchangePFEnabled = bool.Parse(Read(ref x, "Exchange", "ExchangePFEnabled"));
                Settings.ExchangeVersion = int.Parse(Read(ref x, "Exchange", "ExchangeVersion"));
                Settings.ExchangeConnection = Read(ref x, "Exchange", "ExchangeConnection");
                Settings.ExchangeMaxAliases = int.Parse(Read(ref x, "Exchange", "ExchangeMaxAliases"));
                Settings.ExchangeDatabases = Read(ref x, "Exchange", "ExchangeDatabases").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Settings.ExchangeGALName = Read(ref x, "Exchange", "ExchangeGALName");
                Settings.ExchangeABPName = Read(ref x, "Exchange", "ExchangeABPName");
                Settings.ExchangeOALName = Read(ref x, "Exchange", "ExchangeOALName");
                Settings.ExchangeUSERSName = Read(ref x, "Exchange", "ExchangeUSERSName");
                Settings.ExchangeCONTACTSName = Read(ref x, "Exchange", "ExchangeCONTACTSName");
                Settings.ExchangeROOMSName = Read(ref x, "Exchange", "ExchangeROOMSName");
                Settings.ExchangeGROUPSName = Read(ref x, "Exchange", "ExchangeGROUPSName");
                Settings.ExchangeGroupsOU = Read(ref x, "Exchange", "ExchangeGroupsOU");
                Settings.ExchangeContactsOU = Read(ref x, "Exchange", "ExchangeContactsOU");
                Settings.ExchangeRoomsOU = Read(ref x, "Exchange", "ExchangeRoomsOU");
                Settings.ExchangeResourceOU = Read(ref x, "Exchange", "ExchangeResourceOU");
                Settings.ExchangeOUName = Read(ref x, "Exchange", "ExchangeOUName");

                // Modules node
                Settings.ExchangeModule = bool.Parse(Read(ref x, "Modules", "ExchangeModule"));
                Settings.CitrixModule = bool.Parse(Read(ref x, "Modules", "CitrixModule"));
                Settings.LyncModule = bool.Parse(Read(ref x, "Modules", "LyncModule"));

                // Citrix node
                Settings.ApplicationsOUName = Read(ref x, "Citrix", "ApplicationsOUName");

                // Support Notifications
                Settings.SNEnabled = bool.Parse(Read(ref x, "Notifications", "Enabled"));
                Settings.SNFrom = Read(ref x, "Notifications", "FromAddress");
                Settings.SNTo = Read(ref x, "Notifications", "ToAddress");
                Settings.SNServer = Read(ref x, "Notifications", "MailServer");
                Settings.SNPort = int.Parse(Read(ref x, "Notifications", "MailPort"));
                Settings.SNUsername = Read(ref x, "Notifications", "MailUsername");
                Settings.SNEncryptedPassword = Read(ref x, "Notifications", "MailPassword");

                return true;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error loading settings: {0}", ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Reads a specific element from a specific node
        /// </summary>
        /// <param name="x"></param>
        /// <param name="node"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private static string Read(ref IEnumerable<XElement> x, string node, string element)
        {
            logger.DebugFormat("Reading node {0} and element {1}", node, element);
            var value = x.Descendants(node).Elements(element).FirstOrDefault().Value;
            return value;
        }
    }
}