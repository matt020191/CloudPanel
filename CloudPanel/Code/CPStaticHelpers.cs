using CloudPanel.Base;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Enums;
using CloudPanel.Base.Exchange;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudPanel
{
    public class CPStaticHelpers
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(CPStaticHelpers));

        public static string FormatBytes(long bytes)
        {
            if (bytes < 0)
                return "0 Bytes";
            else
            {
                var prefixOrder = new string[] { "TB", "GB", "MB", "KB", "Bytes" };
                var max = (long)Math.Pow(1024, prefixOrder.Length - 1);

                foreach (string p in prefixOrder)
                {
                    if (bytes > max)
                        return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), p);

                    max /= 1024;
                }

                return "0 Bytes";
            }
        }

        public static string GenerateCompanyCode(string companyName)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                // Generate code
                char[] arr = companyName.ToCharArray();
                arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c))));

                string companyCode = new string(arr).ToUpper();
                if (companyCode.Length > 3)
                    companyCode = companyCode.Substring(0, 3);

                // Make sure it doesn't exist in the database
                int count = 1;
                string validCompanyCode = companyCode;
                while (true)
                {
                    logger.DebugFormat("Checking if code {0} exists", validCompanyCode);
                    int codeExist = (from d in db.Companies where d.CompanyCode == validCompanyCode select d).Count();
                    if (codeExist > 0)
                    {
                        validCompanyCode = string.Format("{0}{1}", companyCode, count);
                        count = count + 1;

                        logger.DebugFormat("Code already existed. Increasing number and trying {0}", validCompanyCode);
                    }
                    else
                        break;
                }

                return validCompanyCode;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error generating new company code for {0}: {1}", companyName, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static string StripCharacters(string dataToStrip)
        {
            char[] arr = dataToStrip.ToCharArray();
            arr = Array.FindAll<char>(arr, (c => (char.IsLetterOrDigit(c))));

            return new string(arr);
        }

        public static string GetCompanyName(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Getting company name for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);

                // Generate code
                var companyName = (from d in db.Companies
                                   where d.CompanyCode == companyCode
                                   select d.CompanyName).FirstOrDefault();

                logger.DebugFormat("Found company name {0} for {1}", companyName, companyCode);
                return companyName;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting company name for {0} form the database: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static string RandomCharacters()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return result;
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

        /// <summary>
        /// Checks if the clients wants HTML or not. This is for error handler classes
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool ClientWantsHtml(NancyContext context)
        {
            logger.DebugFormat("Checking if client wants HTML bck or not");

            var acceptHeaders = context.Request.Headers.Accept;
            var mediaRanges = acceptHeaders.OrderByDescending(x => x.Item2)
                                           .Select(x => new MediaRange(x.Item1))
                                           .ToList();

            logger.DebugFormat("Found a total of {0} media ranges", mediaRanges.Count);
            foreach (var item in mediaRanges)
            {
                logger.DebugFormat("Matching media range {0}", item.ToString());
                if (item.Matches("application/json"))
                    return false;
                if (item.Matches("text/json"))
                    return false;
                if (item.Matches("text/html"))
                    return true;
            }

            // Return true as fallback
            return true;
        }

        /// <summary>
        /// Checks if the current numbers are under the plan limit or not
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public static bool IsUnderLimit(string companyCode, string section)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                var companyPlan = (from d in db.Companies
                                    join p in db.Plans_Organization on d.OrgPlanID equals p.OrgPlanID into p1
                                    from plan in p1
                                    where d.CompanyCode == companyCode
                                    select plan).First();

                switch (section.ToLower())
                {
                    case "user":
                        var userCount = (from d in db.Users where d.CompanyCode == companyCode select d).Count();
                        return (companyPlan.MaxUsers > userCount);
                    case "mailbox":
                        var mailboxCount = (from d in db.Users where d.CompanyCode == companyCode where d.MailboxPlan > 0 select d).Count();
                        return (companyPlan.MaxExchangeMailboxes > mailboxCount);
                    case "distributiongroup":
                        var groupCount = (from d in db.DistributionGroups where d.CompanyCode == companyCode select d).Count();
                        return (companyPlan.MaxExchangeDistLists > groupCount);
                    case "contact":
                        var contactCount = (from d in db.Contacts where d.CompanyCode == companyCode select d).Count();
                        return (companyPlan.MaxExchangeContacts > contactCount);
                    case "resource":
                        var resourceCount = (from d in db.ResourceMailboxes where d.CompanyCode == companyCode select d).Count();
                        return (companyPlan.MaxExchangeResourceMailboxes > resourceCount);
                    case "domain":
                        var domainCount = (from d in db.Domains where d.CompanyCode == companyCode select d).Count();
                        return (companyPlan.MaxDomains > domainCount);
                    case "activesync":
                        var activesyncCount = (from d in db.Plans_ExchangeActiveSync where d.CompanyCode == companyCode select d).Count();
                        return (companyPlan.MaxExchangeActivesyncPolicies > activesyncCount);
                    default:
                        return false; // Return false that it is at the limits if it doesn't find the section
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error checking company plan limit for {0} and section {1}: {2}", companyCode, section, ex.ToString());
                return false; // Return false that it is at the limits if something fails
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        #region Alert Messages
        public static IHtmlString GetSuccess(string message)
        {
            string returnMsg = string.Format(@"<div class='alert alert-dismissable alert-success'>
								                <strong>Success</strong> {0}
								                <button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>
							                   </div>", message);

            return new NonEncodedHtmlString(returnMsg);
        }

        public static IHtmlString GetError(string message)
        {
            string returnMsg = string.Format(@"<div class='alert alert-dismissable alert-danger'>
								                <strong>Error</strong> {0}
								                <button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>
							                   </div>", message);

            return new NonEncodedHtmlString(returnMsg);
        }

        public static IHtmlString GetInfo(string message)
        {
            string returnMsg = string.Format(@"<div class='alert alert-dismissable alert-info'>
								                <strong>Info</strong> {0}
								                <button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>
							                   </div>", message);

            return new NonEncodedHtmlString(returnMsg);
        }

        public static IHtmlString GetWarning(string message)
        {
            string returnMsg = string.Format(@"<div class='alert alert-dismissable alert-warning'>
								                <strong>Warning</strong> {0}
								                <button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>
							                   </div>", message);

            return new NonEncodedHtmlString(returnMsg);
        }
        #endregion
    }
}