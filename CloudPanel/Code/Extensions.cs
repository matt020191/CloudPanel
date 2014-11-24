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

        /// <summary>
        /// Gets the selected company code
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the company name the user has selected
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the selected reseller code
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the selected reseller name
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Sets the company code the current user selected
        /// </summary>
        /// <param name="context"></param>
        /// <param name="companyCode"></param>
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

        /// <summary>
        /// Sets the reseller code the current user selected
        /// </summary>
        /// <param name="context"></param>
        /// <param name="companyCode"></param>
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

        /// <summary>
        /// Returns the display name of the user
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string DisplayName(this NancyContext context)
        {
            if (context.CurrentUser == null)
                return string.Empty;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.DisplayName;
        }

        /// <summary>
        /// Determines if the user is a company admin
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsCompanyAdmin(this NancyContext context)
        {
            if (context.CurrentUser == null)
                return false;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.Claims.Any(x => x.Contains("CompanyAdmin"));
        }

        /// <summary>
        /// Gets the company code the user belongs to (if any)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetCompanyCodeMembership(this NancyContext context)
        {
            if (context.CurrentUser == null)
                return string.Empty;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.CompanyCode;
        }

        /// <summary>
        /// Checks if the logged in user is a super admin or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsSuperAdmin(this NancyContext context)
        {
            if (context.CurrentUser == null)
                return false;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.Claims.Any(x => x.Contains("SuperAdmin"));
        }

        /// <summary>
        /// Checks if the logged in user is a reseller admin or not
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsResellerAdmin(this NancyContext context)
        {
            if (context.CurrentUser == null)
                return false;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.Claims.Any(x => x.Contains("ResellerAdmin"));
        }

        /// <summary>
        /// Checks if the logged in user is a reseller admin OR a super admin
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool IsSuperOrResellerAdmin(this NancyContext context)
        {
            if (context.CurrentUser == null)
                return false;

            var user = context.CurrentUser as AuthenticatedUser;
            return user.Claims.Any(x => (x.Contains("SuperAdmin") || x.Contains("ResellerAdmin")));
        }

        public static bool HasPermission(this NancyContext context)
        {
            return false;
        }
    }
}