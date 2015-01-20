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
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

        /// <summary>
        /// If we allow super or reseller admins
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public static bool AllowSuperOrReseller(IUserIdentity currentUser, string companyCode)
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
                if (authUser.Claims.Contains("SuperAdmin"))
                    return true;
                else
                {
                    return authUser.Claims.Contains("ResellerAdmin") &&
                           new CloudPanelContext(Settings.ConnectionString)
                                .Companies
                                .Where(x => x.CompanyCode.Equals(companyCode) && x.ResellerCode.Equals(authUser.ResellerCode))
                                .Count() > 0;
                }
            }
        }

        /// <summary>
        /// If we allow the company admin or not. It compares the user's company code to the company code being accessed to make sure they match.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="companyCode"></param>
        /// <param name="role"></param>
        /// <returns></returns>
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
                    // Check if the user is a company admin and belongs to the company with the right role
                    logger.DebugFormat("User was not a superadmin, or reselleradmin, checking company admin rights");
                    return authUser.Claims
                                   .Contains("CompanyAdmin") &&
                           authUser.CompanyCode
                                   .Equals(companyCode) && 
                           authUser.Claims
                                   .Contains(role);
                }
            }
        }

        /// <summary>
        /// Allows everyone.. company admin, reseller admins, and super admin. Does not check company code
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool AllowCompanyAdmin(IUserIdentity currentUser, string role)
        {
            logger.DebugFormat("Validating if user is super admin, reseller admin, or company admin and contains the permission {0}", role);
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
                    return true;
                }
                else
                {
                    // Check if the user is a company admin and belongs to the company with the right role
                    logger.DebugFormat("User was not a superadmin, or reselleradmin, checking company admin rights");
                    return authUser.Claims
                                   .Contains("CompanyAdmin") &&
                           authUser.Claims
                                   .Contains(role);
                }
            }
        }
    }
}