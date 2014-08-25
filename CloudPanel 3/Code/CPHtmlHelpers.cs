using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel
{
    public class CPHtmlHelpers
    {
        public static IHtmlString GetCompanyPlans(int selectedId, string insertBefore)
        {
            var stringBuilder = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var companyPlans = (from d in db.Plans_Organization
                                    orderby d.OrgPlanName
                                    select d).ToList();

                if (!string.IsNullOrEmpty(insertBefore))
                    stringBuilder.Append(insertBefore);

                companyPlans.ForEach(x =>
                {
                    stringBuilder.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", 
                            x.OrgPlanID,
                            x.OrgPlanID == selectedId ? "selected" : "",
                            x.OrgPlanName
                        );
                });

                string returnValue = string.Format("<select id=\"{0}\" name=\"{0}\" class=\"form-control\">{1}</select>", "OrgPlanId", stringBuilder.ToString());
                return new NonEncodedHtmlString(returnValue);
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

        public static IHtmlString GetMailboxPlans(int selectedId, string companyCode, string insertBefore)
        {
            var stringBuilder = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var mailboxPlans = (from d in db.Plans_ExchangeMailbox
                                    orderby d.MailboxPlanName
                                    select d).ToList();

                // If provided a company code limit the values to ones set to their company and set to all companies
                if (!string.IsNullOrEmpty(companyCode))
                    mailboxPlans = (from d in mailboxPlans
                                    where string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode
                                    select d).ToList();

                if (!string.IsNullOrEmpty(insertBefore))
                    stringBuilder.Append(insertBefore);

                mailboxPlans.ForEach(x =>
                {
                    stringBuilder.AppendFormat("<option value=\"{0}\" {1}>{2}</option>",
                            x.MailboxPlanID,
                            x.MailboxPlanID == selectedId ? "selected" : "",
                            x.MailboxPlanName
                        );
                });

                string returnValue = string.Format("<select id=\"{0}\" name=\"{0}\" class=\"form-control\">{1}</select>", "MailboxPlanID", stringBuilder.ToString());
                return new NonEncodedHtmlString(returnValue);
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

        public static IHtmlString GetCompanies(string companyCode, string insertBefore)
        {
            var stringBuilder = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var companies = (from d in db.Companies
                                 orderby d.CompanyName
                                 select d).ToList();

                if (!string.IsNullOrEmpty(insertBefore))
                    stringBuilder.Append(insertBefore);

                companies.ForEach(x =>
                {
                    stringBuilder.AppendFormat("<option value=\"{0}\" {1}>{2}</option>",
                            x.CompanyCode,
                            x.CompanyCode.Equals(companyCode, StringComparison.InvariantCultureIgnoreCase) ? "selected" : "",
                            x.CompanyName
                        );
                });

                string returnValue = string.Format("<select id=\"{0}\" name=\"{0}\" class=\"form-control\">{1}</select>", "CompanyCode", stringBuilder.ToString());
                return new NonEncodedHtmlString(returnValue);
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
    }
}