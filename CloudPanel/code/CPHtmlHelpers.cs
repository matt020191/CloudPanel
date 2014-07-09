//
// Copyright (c) 2014, Jacob Dixon
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by KnowMoreIT and Compsys.
// 4. Neither the name of KnowMoreIT and Compsys nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY Jacob Dixon ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL Jacob Dixon BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ViewEngines.Razor;
using System.Text;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using log4net;
using CloudPanel.Base.Database.Models;

namespace CloudPanel.code
{
    public class CPHtmlHelpers
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(CPHtmlHelpers));

        public static IHtmlString GetEmailDomains(string companyCode)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var domains = from d in db.Domains
                              where d.CompanyCode == companyCode
                              orderby d.Domain1
                              select d;

                foreach (var d in domains)
                {
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>",
                        d.DomainID,
                        d.IsDefault ? "selected" : "",
                        d.Domain1);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting domains for {0}. Error: {1}", companyCode, ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}</select>",
                "DomainName",
                "DomainName",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString GetEmailDomains(string companyCode, List<Domain> domains, string emailDomain)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                foreach (var d in domains)
                {
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>",
                        d.DomainID,
                        d.Domain1.Equals(emailDomain, StringComparison.CurrentCultureIgnoreCase) ? "selected" : "",
                        d.Domain1);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting domains for {0}. Error: {1}", companyCode, ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}</select>",
                "DomainName",
                "DomainName",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString GetCompanyPlans(string companyCode, int orgID)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plans = from p in db.Plans_Organization orderby p.OrgPlanName select p;
                foreach (var p in plans)
                {
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>",
                        p.OrgPlanID,
                        p.OrgPlanID == orgID ? "selected" : "",
                        p.OrgPlanName);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting company plans for {0}. Error: {1}", companyCode, ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}{3}</select>",
                "OrgPlanID",
                "OrgPlanID",
                "<option value='0'> --- Select Plan --- </option>",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString GetCompanyPlans()
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plans = (from p in db.Plans_Organization
                             orderby p.OrgPlanName
                             select p).ToList();

                foreach (var p in plans)
                {
                    sb.AppendFormat("<option value='{0}'>{1}</option>",
                        p.OrgPlanID,
                        p.OrgPlanName);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting company plans. Error: {0}", ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}{3}</select>",
                "OrgPlanID",
                "OrgPlanID",
                "<option value='0'> --- Create New --- </option>",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString GetMailboxPlans(string companyCode, List<Plans_ExchangeMailbox> plans, int selectedID)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                                
                foreach (var p in plans)
                {
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>",
                        p.MailboxPlanID,
                        p.MailboxPlanID == selectedID ? "selected" : "",
                        p.MailboxPlanName);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting mailbox plans for {0}. Error: {1}", companyCode, ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}{3}</select>",
                "MailboxPlan",
                "MailboxPlan",
                "<option value='0'> --- Select Plan --- </option>",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString GetActiveSyncPlans(string companyCode, List<Plans_ExchangeActiveSync> plans, int selectedID)
        {
            var sb = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                foreach (var p in plans)
                {
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>",
                        p.ASID,
                        p.ASID == selectedID ? "selected" : "",
                        p.DisplayName);
                }
            }
            catch (Exception ex)
            {
                sb.AppendFormat("<option value='0' selected>{1}: {2}</option>",
                    "ERROR",
                    ex.Message);

                log.ErrorFormat("Error getting activesync plans for {0}. Error: {1}", companyCode, ex.ToString());
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='form-control'>{2}{3}</select>",
                "ActiveSyncPlan",
                "ActiveSyncPlan",
                "<option value='0'> --- None --- </option>",
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString GetMultiSelectBox(string selectName, Dictionary<string, string> keyValuePair, List<string> selectedKeys)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var a in keyValuePair)
            {
                if (selectedKeys == null)
                    sb.AppendFormat("<option value='{0}' {1}>{2}</option>", a.Key, "", a.Value);
                else
                {
                    var match = selectedKeys.FirstOrDefault(s => s.Equals(a.Key, StringComparison.CurrentCultureIgnoreCase));
                    if (match == null)
                        sb.AppendFormat("<option value='{0}' {1}>{2}</option>", a.Key, "", a.Value);
                    else
                        sb.AppendFormat("<option value='{0}' {1}>{2}</option>", a.Key, "selected", a.Value);
                }
            }

            var htmlSelectBox = string.Format("<select id='{0}' name='{1}' class='chosen-select' multiple>{2}</select>",
                selectName,
                selectName,
                sb.ToString());

            return new NonEncodedHtmlString(htmlSelectBox);
        }

        public static IHtmlString IsChecked(object objValue)
        {
            NonEncodedHtmlString blank = new NonEncodedHtmlString("");

            if (objValue == null)
                return blank;
            else
            {
                if (objValue is Boolean && (bool)objValue == true)
                    return new NonEncodedHtmlString("checked");
                else
                    return blank;
            }
        }

        public static IHtmlString IsChecked(object objValue, int compareValue)
        {
            NonEncodedHtmlString blank = new NonEncodedHtmlString("");

            if (objValue == null)
                return blank;
            else
            {
                if (objValue is int && (int)objValue == compareValue)
                    return new NonEncodedHtmlString("checked");
                else
                    return blank;
            }
        }

        public static IHtmlString RandomCharacters()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return new NonEncodedHtmlString(result);
        }
    }
}