using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public class CPStaticHelpers
    {
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
    }
}