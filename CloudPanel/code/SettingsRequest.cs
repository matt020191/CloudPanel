using CloudPanel.Base.Config;
using log4net;
//
// Copyright (c) 2014, Jacob Dixon
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by KnowMoreIT and Compsys.
// 4. Neither the name of KnowMoreIT and Compsys nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY Jacob Dixon ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL Jacob Dixon BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CloudPanel
{
    public class SettingsRequest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsRequest));

        internal static readonly string path = HttpContext.Current.Server.MapPath("~/Config/settings.xml");

        public static void RetrieveSettings()
        {
            try
            {
                XDocument xDoc = XDocument.Load(path);

                /* Load CloudPanel Settings */
                log.Debug("Populating settings from config file");
                var settings = from s in xDoc.Elements("cloudpanel").Elements("Settings")
                               select s;

                foreach (var s in settings)
                {
                    Settings.CompanyName = s.Element("CompanyName").Value;
                    Settings.HostingOU = s.Element("HostingOU").Value;
                    Settings.PrimaryDC = s.Element("DomainController").Value;
                    Settings.Username = s.Element("Username").Value;
                    Settings.EncryptedPassword = s.Element("Password").Value;
                    Settings.SuperAdmins = s.Element("SuperAdmins").Value.Split(',');
                    Settings.BillingAdmins = s.Element("BillingAdmins").Value.Split(',');
                    Settings.SaltKey = s.Element("SaltKey").Value;

                    bool enabled = false;
                    bool.TryParse(s.Element("ResellersEnabled").Value, out enabled);
                    Settings.ResellersEnabled = enabled;
                }


                /* Load Exchange Settings */
                log.Debug("Loading Exchange settings from config file");
                var exchange = from s in xDoc.Elements("cloudpanel").Elements("Exchange")
                               select s;

                foreach (var s in exchange)
                {
                    Settings.ExchangeServer = s.Element("Server").Value;
                    Settings.ExchangePFServer = s.Element("PFServer").Value;

                    int defaultVersion = 2013;
                    int.TryParse(s.Element("Version").Value, out defaultVersion);
                    Settings.ExchangeVersion = defaultVersion;

                    bool defaultBool = true;
                    bool.TryParse(s.Element("SSL").Value, out defaultBool);
                    Settings.ExchangeSSL = defaultBool;

                    bool.TryParse(s.Element("PFEnabled").Value, out defaultBool);
                    Settings.ExchangePFEnabled = defaultBool;

                    Settings.ExchangeConnection = s.Element("Connection").Value;
                    Settings.ExchangeDatabases = s.Element("Databases").Value.Split(',');
                    Settings.ExchangeGALName = s.Element("ExchangeGALName").Value;
                    Settings.ExchangeABPName = s.Element("ExchangeABPName").Value;
                    Settings.ExchangeOALName = s.Element("ExchangeOALName").Value;
                    Settings.ExchangeUSERSName = s.Element("ExchangeUSERSName").Value;
                    Settings.ExchangeCONTACTSName = s.Element("ExchangeCONTACTSName").Value;
                    Settings.ExchangeROOMSName = s.Element("ExchangeROOMSName").Value;
                    Settings.ExchangeGROUPSName = s.Element("ExchangeGROUPSName").Value;
                    Settings.ExchangeOU = s.Element("ExchangeOU").Value;
                }

                /* Load Module Settings */
                log.Debug("Loading module settings from config file");
                var modules = from s in xDoc.Element("cloudpanel").Elements("Modules")
                              select s;

                foreach (var s in modules)
                {
                    bool enabled = false;

                    bool.TryParse(s.Element("Exchange").Value, out enabled);
                    Settings.ExchangeModule = enabled;

                    bool.TryParse(s.Element("Citrix").Value, out enabled);
                    Settings.CitrixModule = enabled;

                    bool.TryParse(s.Element("Lync").Value, out enabled);
                    Settings.LyncModule = enabled;
                }

                /* Load Advanced Settings */
                log.Debug("Loading advanced settings from config file");
                var advanced = from s in xDoc.Element("cloudpanel").Elements("Advanced")
                               select s;

                foreach (var a in advanced)
                {
                    Settings.ApplicationsOU = a.Element("ApplicationsOU").Value;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading settings", ex);

                // Set error value so people can't login // TODO
            }
        }

        public static void SaveSetting(string root, string element, string value)
        {
            try
            {
                XDocument xDoc = XDocument.Load(path);

                var settings = from s in xDoc.Elements("cloudpanel").Elements(root)
                               select s;

                foreach (var e in settings.Elements())
                {
                    if (e.Name == element)
                    {
                        e.Value = value;
                    }
                }

                xDoc.Save(path);
            }
            catch (Exception ex)
            {
                log.Error("Error saving settings", ex);
            }
        }
    }
}