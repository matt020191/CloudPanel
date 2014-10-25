using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public static class Extensions
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(Extensions));

        public static bool Accept(this Request request, string contentType)
        {
            return request.Headers.Keys.Contains("Accept")
                && request.Headers["Accept"]
                            .Any(c => c.Contains(contentType));
        }

        public static string GetCompanyCode(this NancyContext context)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                return user.SelectedCompanyCode;
            }
            else
                return string.Empty;
        }

        public static string GetCompanyName(this NancyContext context)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                return user.SelectedCompanyName;
            }
            else
                return string.Empty;
        }

        public static string GetResellerCode(this NancyContext context)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                return user.SelectedResellerCode;
            }
            else
                return string.Empty;
        }

        public static string GetResellerName(this NancyContext context)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                return user.SelectedResellerName;
            }
            else
                return string.Empty;
        }

        public static void SetCompanyCode(this NancyContext context, string companyCode)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                user.SelectedCompanyCode = companyCode;

                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Setting the selected company name");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var companyName = (from d in db.Companies
                                       where !d.IsReseller
                                       where d.CompanyCode == companyCode
                                       select d.CompanyName).FirstOrDefault();

                    user.SelectedCompanyName = companyName;
                }
                catch (Exception ex)
                {
                    logger.DebugFormat("Error getting company name for {0}: {1}", companyCode, ex.ToString());
                    user.SelectedCompanyName = "Unknown";
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            }
        }

        public static void SetResellerCode(this NancyContext context, string companyCode)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                user.SelectedResellerCode = companyCode;

                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Setting the selected reseller name");
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var companyName = (from d in db.Companies
                                       where d.IsReseller
                                       where d.CompanyCode == companyCode
                                       select d.CompanyName).FirstOrDefault();

                    user.SelectedResellerName = companyName;
                }
                catch (Exception ex)
                {
                    logger.DebugFormat("Error getting reseller name for {0}: {1}", companyCode, ex.ToString());
                    user.SelectedResellerName = "Unknown";
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            }
        }
    }
}