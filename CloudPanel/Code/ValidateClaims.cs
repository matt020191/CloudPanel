using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Code
{
    public class ValidateClaims
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(ValidateClaims));

        /// <summary>
        /// Checks if the user is a super admin
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public static bool AllowSuper(IUserIdentity currentUser)
        {
            logger.DebugFormat("Validating if user is super admin");
            return currentUser == null ? false : currentUser.Claims.Contains("SuperAdmin");
        }

        /// <summary>
        /// Checks if the user is a super admin or a reseller admin (making sure the companyCode belongs to the reseller)
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static bool AllowReseller(IUserIdentity currentUser, string companyCode)
        {
            logger.DebugFormat("Validating if user is super admin or reseller admin for company code {0}", companyCode);
            if (currentUser == null)
            {
                logger.DebugFormat("Context was null when validating");
                return false;
            }
            else
            {
                var authUser = currentUser as AuthenticatedUser;
                return currentUser.Claims.Contains("ResellerAdmin") &&
                       new CloudPanelContext(Settings.ConnectionString)
                            .Companies
                            .Where(x => x.CompanyCode.Equals(companyCode) && x.ResellerCode.Equals(authUser.ResellerCode))
                            .Count() > 0;
            }
        }

        public static bool AllowCompanyAdmin(IUserIdentity currentUser, string companyCode, string role)
        {
            logger.DebugFormat("Validating if user is super admin, reseller admin, or company admin for company code {0} and contains the permission {1}", companyCode, role);
            if (currentUser == null)
            {
                logger.DebugFormat("Context was null when validating");
                return false;
            }
            else
            {
                var authUser = currentUser as AuthenticatedUser;

                if (currentUser.Claims.Contains("SuperAdmin"))
                {
                    logger.DebugFormat("Claims contain SuperAdmin");
                    return true;
                }
                else if (currentUser.Claims.Contains("ResellerAdmin"))
                {
                    // Check if reseller admin and company belongs to the reseller
                    logger.DebugFormat("Claims contains ResellerAdmin");
                    return new CloudPanelContext(Settings.ConnectionString)
                            .Companies
                            .Where(x => x.CompanyCode.Equals(companyCode) && x.ResellerCode.Equals(authUser.ResellerCode))
                            .Count() > 0;
                }
                else
                {
                    // Chheck if the user is a company admin and belongs to the company with the right role
                    logger.DebugFormat("User was not a superadmin, or reselleradmin, checking company admin rights");
                    var db = new CloudPanelContext(Settings.ConnectionString);
                    var user = (from d in db.Users
                                join p in db.UserRoles on d.RoleID equals p.RoleID into d1
                                from permission in d1.DefaultIfEmpty().Take(1)
                                where d.CompanyCode.Equals(companyCode)
                                where d.UserPrincipalName.Equals(currentUser.UserName)
                                select d).FirstOrDefault();

                    if (user == null)
                    {
                        logger.DebugFormat("User was not found in database {0}", currentUser.UserName);
                        return false;
                    }
                    else
                    {
                        logger.DebugFormat("Validating the role for {0}", currentUser.UserName);

                        var permission = (from d in db.UserRoles
                                          where d.RoleID == user.RoleID
                                          where d.CompanyCode == companyCode
                                          select d).FirstOrDefault();

                        logger.DebugFormat("Permissions was {0}", permission == null ? "not found" : "found");
                        return permission == null ? false :
                               permission.GetType()
                                         .GetProperties()
                                         .Where(x => (x.Name.Equals(role, StringComparison.InvariantCultureIgnoreCase) && x.PropertyType == typeof(bool))).FirstOrDefault()
                                         .GetValue(permission, null)
                                         .Equals((bool)true);
                    }
                }
            }
        }
    }
}