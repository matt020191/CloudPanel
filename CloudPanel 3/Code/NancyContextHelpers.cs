using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public class NancyContextHelpers
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(NancyContextHelpers));

        public static void SetSelectedCompanyCode(NancyContext context, string companyCode)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                user.SelectedCompanyCode = companyCode;
            }
        }

        public static string GetSelectedCompanyCode(NancyContext context)
        {
            if (context.CurrentUser == null)
                return string.Empty;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.SelectedCompanyCode;
        }

        public static void SetSelectedResellerCode(NancyContext context, string resellerCode)
        {
            if (context.CurrentUser != null)
            {
                var user = context.CurrentUser as AuthenticatedUser;
                user.SelectedResellerCode = resellerCode;
            }
        }

        public static string GetSelectedResellerCode(NancyContext context)
        {
            if (context.CurrentUser == null)
                return string.Empty;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.SelectedResellerCode;
        }

        public static bool IsSuperAdmin(NancyContext context)
        {
            if (context.CurrentUser == null)
                return false;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.Claims.Any(x => x.Contains("SuperAdmin"));
        }

        public static bool IsExchangeEnabled(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Checking if company {0} is enabled for Exchange", companyCode);

                if (string.IsNullOrEmpty(companyCode))
                    return false;
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var isEnabled = (from d in db.Companies where !d.IsReseller where d.CompanyCode == companyCode select d.ExchEnabled).FirstOrDefault();
                    return isEnabled;
                }
            }
            catch (Exception ex)
            {
                logger.DebugFormat("Error checking if company was enabled for Exchange. Returning false by default: {0}", ex.ToString());
                return false;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}