using CloudPanel.Base.Config;
using log4net;
using System;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CloudPanel.Code
{
    public class SettingsReader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SettingsReader));

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
                    Settings.UsersOU = s.Element("UsersOU").Value;
                    Settings.PrimaryDC = s.Element("PrimaryDC").Value;
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
                    Settings.ExchangeServer = s.Element("ExchangeServer").Value;
                    Settings.ExchangePFServer = s.Element("ExchangePFServer").Value;
                    Settings.ExchangeRoleAssignment = s.Element("ExchangeRoleAssignment").Value;

                    int defaultVersion = 2013;
                    int.TryParse(s.Element("ExchangeVersion").Value, out defaultVersion);
                    Settings.ExchangeVersion = defaultVersion;

                    bool defaultBool = true;
                    bool.TryParse(s.Element("ExchangeSSL").Value, out defaultBool);
                    Settings.ExchangeSSL = defaultBool;

                    bool.TryParse(s.Element("ExchangePFEnabled").Value, out defaultBool);
                    Settings.ExchangePFEnabled = defaultBool;

                    Settings.ExchangeConnection = s.Element("ExchangeConnection").Value;
                    Settings.ExchangeDatabases = s.Element("ExchangeDatabases").Value.Split(',');
                    Settings.ExchangeGALName = s.Element("ExchangeGALName").Value;
                    Settings.ExchangeABPName = s.Element("ExchangeABPName").Value;
                    Settings.ExchangeOALName = s.Element("ExchangeOALName").Value;
                    Settings.ExchangeUSERSName = s.Element("ExchangeUSERSName").Value;
                    Settings.ExchangeCONTACTSName = s.Element("ExchangeCONTACTSName").Value;
                    Settings.ExchangeROOMSName = s.Element("ExchangeROOMSName").Value;
                    Settings.ExchangeGROUPSName = s.Element("ExchangeGROUPSName").Value;
                    Settings.ExchangeOUName = s.Element("ExchangeOU").Value;
                }

                /* Load Module Settings */
                log.Debug("Loading module settings from config file");
                var modules = from s in xDoc.Element("cloudpanel").Elements("Modules")
                              select s;

                foreach (var s in modules)
                {
                    bool enabled = false;

                    bool.TryParse(s.Element("ExchangeModule").Value, out enabled);
                    Settings.ExchangeModule = enabled;

                    bool.TryParse(s.Element("CitrixModule").Value, out enabled);
                    Settings.CitrixModule = enabled;

                    bool.TryParse(s.Element("LyncModule").Value, out enabled);
                    Settings.LyncModule = enabled;
                }

                /* Load Advanced Settings */
                log.Debug("Loading advanced settings from config file");
                var advanced = from s in xDoc.Element("cloudpanel").Elements("Advanced")
                               select s;

                foreach (var a in advanced)
                {
                    Settings.ApplicationsOUName = a.Element("ApplicationsOU").Value;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading settings", ex);

                // Set error value so people can't login // TODO
            }
        }

        public static void SaveSetting(string element, string value)
        {
            try
            {
                XDocument xDoc = XDocument.Load(path);

                /*var settings = from s in xDoc.Elements("cloudpanel").Elements(root)
                               select s;

                foreach (var e in settings.Elements())
                {
                    if (e.Name == element)
                    {
                        e.Value = value;
                    }
                }*/

                var all = from s in xDoc.Elements("cloudpanel").Elements()
                          select s;

                foreach (var e in all.Elements())
                {
                    if (e.Name.LocalName.Equals(element))
                    {
                        e.Value = value;
                        log.DebugFormat("Updating setting {0} from value {1} to value {2}", e.Name.LocalName, e.Value, value);
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