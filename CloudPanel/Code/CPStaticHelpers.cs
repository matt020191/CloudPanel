using CloudPanel.Base;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Enums;
using CloudPanel.Base.Exchange;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
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
                    int codeExist = (from d in db.Companies where d.CompanyCode == validCompanyCode select d).Count();
                    if (codeExist > 0)
                    {
                        validCompanyCode = string.Format("{0}{1}", companyCode, count);
                        count = count + 1;
                    }
                    else
                        break;
                }

                return validCompanyCode;
            }
            catch (Exception ex)
            {
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
                logger.ErrorFormat("Error getting");
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