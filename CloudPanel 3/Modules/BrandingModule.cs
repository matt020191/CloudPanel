using CloudPanel.Base.Branding;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace CloudPanel.Modules
{
    public class BrandingModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BrandingModule));
        private static readonly string path = HttpContext.Current.Server.MapPath("~/Config/brandings.xml");

        public static List<CompanyBranding> AllBrandings = null;

        public BrandingModule()
        {

        }

        public static void LoadBrandings()
        {
            try
            {
                logger.DebugFormat("Loading all branding from file");
                
                XDocument xDoc = XDocument.Load(path);
                var brandings = from b in xDoc.Elements("Brandings").Elements() 
                                select b;

                logger.DebugFormat("Processing brandings that were found");
                AllBrandings = new List<CompanyBranding>();

                foreach (var b in brandings)
                {
                    var newBranding = new CompanyBranding();
                    newBranding.Header = b.Value;
                    newBranding.Name = b.Element("Name").Value;
                    newBranding.Phone = b.Element("Phone").Value;
                    newBranding.SupportEmail = b.Element("SupportEmail").Value;
                    newBranding.LoginLogo = b.Element("LoginLogo").Value;
                    newBranding.TopLogo = b.Element("TopLogo").Value;
                    newBranding.Theme = b.Element("Theme").Value;

                    AllBrandings.Add(newBranding);
                    logger.DebugFormat("Completed loaded branding for {0}", newBranding.Header);
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Failed to load brandings from file {0}: {1}", path, ex.ToString());
            }
        }

        public static CompanyBranding GetBranding(string httpHeader)
        {
            logger.DebugFormat("Processing branding request for {0}", httpHeader);

            var returnBranding = DefaultBranding();
            if (AllBrandings != null)
            {
                var brandingExist = (from b in AllBrandings
                                     where b.Header == httpHeader
                                     select b).FirstOrDefault();

                if (brandingExist != null)
                    return brandingExist;
                else
                    logger.DebugFormat("Branding request for {0} was not found. Returning default.", httpHeader);
            }
            else
                logger.DebugFormat("No brandings have been defined");

            return returnBranding;
        }

        private static CompanyBranding DefaultBranding()
        {
            return new CompanyBranding()
            {
                Name = "CloudPanel",
                Phone = "1 (501) 758-6818",
                SupportEmail = "support@compsys.com",
                TopLogo = "~/Content/img/logo.png",
                LoginLogo = "~/Content/img/logo-big.png",
                Theme = "theme-default.css"
            };
        }
    }
}