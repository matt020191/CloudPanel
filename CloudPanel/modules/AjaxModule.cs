using Nancy;
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
using System.Web;
using Nancy.Security;
using CloudPanel.Database.EntityFramework;
using CloudPanel.code;
using CloudPanel.Base.Database.Models;
using log4net;
using Nancy.Localization;
using System.Threading;
using CloudPanel.Base.Charting;
using System.Data.SqlClient;
using CloudPanel.Base.Config;

namespace CloudPanel.modules
{
    public class AjaxModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(AjaxModule));

        private readonly CloudPanelContext db;

        public AjaxModule(CloudPanelContext db) : base("/ajax")
        {
            this.db = db;

            #region Validation

            Post["/validation/domain"] = _ =>
                {
                    try
                    {
                        string domain = Request.Form.DomainName;

                        // Validate the domain
                        if (domain.Contains(" ") || !domain.Contains("."))
                            return Response.AsJson("Domain is not in correct format");
                        else
                        {
                            int exist = (from d in db.Domains where d.Domain1 == domain select d).Count();
                            if (exist == 0)
                                return Response.AsJson(true);
                            else
                                return Response.AsJson("Domain is not available");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error checking if domain is valid. Exception: {0}", ex.ToString());
                        return Response.AsJson("Unknown error. Contact support.");
                    }
                };

            #endregion

            #region Company

            Get["/company/{CompanyCode}/Plans/Mailbox/{MailboxPlanID}"] = _ =>
            {
                try
                {
                    int id = int.Parse(_.MailboxPlanID);

                    var plan = (from p in db.Plans_ExchangeMailbox
                                where p.MailboxPlanID == id
                                select p).FirstOrDefault();

                    return Response.AsJson(plan, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error retrieving mailbox plan {0}. Exception: {1}", _.MailboxPlanID, ex.ToString());
                    return Response.AsJson("Unknown error. Contact support.", HttpStatusCode.InternalServerError);
                }
            };

            Get["/company/{CompanyCode}/Users"] = _ =>
            {
                try
                {
                    Users users = new Users();
                    List<User> foundUsers = users.GetUsers(_.CompanyCode);

                    var search = Request.Query.sSearch.HasValue ? (string)Request.Query.sSearch : "";

                    // This is if we are searching..
                    if (!string.IsNullOrEmpty(search))
                        foundUsers = foundUsers.Where(c => (
                                                    c.DisplayName.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1 ||
                                                    c.Email.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1 ||
                                                    c.UserPrincipalName.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1 ||
                                                    c.Department.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1
                                                )).ToList();

                    int start = Convert.ToInt32(Request.Query.iDisplayStart.ToString());
                    int length = Convert.ToInt32(Request.Query.iDisplayLength.ToString());
                    var totalRecords = foundUsers.Count();
                    var secho = Request.Query.sEcho;
                    var sorting = Request.Query.sSortDir_0;

                    if (sorting == "asc")
                    {
                        return Response.AsJson(new
                        {
                            aaData = foundUsers.OrderBy(x => x.DisplayName).Skip(start).Take(length),
                            sEcho = secho,
                            iTotalRecords = totalRecords,
                            iTotalDisplayRecords = totalRecords
                        });
                    }
                    else
                    {
                        return Response.AsJson(new
                        {
                            aaData = foundUsers.OrderByDescending(x => x.DisplayName).Skip(start).Take(length),
                            sEcho = secho.ToString(),
                            iTotalRecords = totalRecords,
                            iTotalDisplayRecords = totalRecords
                        });
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error retrieving users under company {0}. Exception: {1}", _.CompanyCode, ex.ToString());
                    return Response.AsJson(new
                    {
                        aaData = new List<User>(),
                        sEcho = "0",
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0
                    }, HttpStatusCode.InternalServerError);
                }
            };

            #endregion

            #region Companies

            Get["/reseller/{resellercode}/companies"] = _ =>
                {
                    try
                    {
                        Resellers resellers = new Resellers();
                        List<Company> companies = resellers.GetCompanies(_.resellercode);

                        var search = Request.Query.sSearch.HasValue ? ((string)Request.Query.sSearch).ToLower() : "";

                        // This is if we are searching..
                        if (!string.IsNullOrEmpty(search))
                            companies = companies.Where(c => (
                                                    c.CompanyName.ToLower().Contains(search)
                                                )).ToList();

                        int start = Convert.ToInt32(Request.Query.iDisplayStart.ToString());
                        int length = Convert.ToInt32(Request.Query.iDisplayLength.ToString());
                        var totalRecords = companies.Count();
                        var secho = Request.Query.sEcho;
                        var sorting = Request.Query.sSortDir_0;

                        if (sorting == "asc")
                        {
                            return Response.AsJson(new { 
                                aaData = companies.OrderBy(x => x.CompanyName).Skip(start).Take(length), 
                                sEcho = secho, 
                                iTotalRecords = totalRecords, 
                                iTotalDisplayRecords = totalRecords 
                            });
                        }
                        else
                        {
                            return Response.AsJson(new { 
                                aaData = companies.OrderByDescending(x => x.CompanyName).Skip(start).Take(length), sEcho = secho.ToString(), 
                                iTotalRecords = totalRecords, 
                                iTotalDisplayRecords = totalRecords 
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving companies under reseller {0}. Exception: {1}", _.resellercode, ex.ToString());
                        return Response.AsJson(new
                        {
                            aaData = new List<Company>(),
                            sEcho = "0",
                            iTotalRecords = 0,
                            iTotalDisplayRecords = 0
                        }, HttpStatusCode.InternalServerError);
                    }
                };

            #endregion

            #region Charting

            Get["/charts/customers/top/{x}"] = _ =>
                {
                    List<TopXCustomers> topx = new List<TopXCustomers>();

                    SqlConnection sql = null;
                    SqlCommand cmd = null;
                    try
                    {
                        string query = string.Format(@"SELECT TOP {0} u.CompanyCode, c.CompanyName, COUNT(*) as TotalUsers FROM Users u 
                                                       INNER JOIN Companies c ON u.CompanyCode=c.CompanyCode 
                                                       GROUP BY u.CompanyCode, c.CompanyName 
                                                       ORDER BY COUNT(*) 
                                                       DESC", _.x);

                        sql = new SqlConnection(Settings.ConnectionString);
                        cmd = new SqlCommand(query, sql);
                        sql.Open();

                        SqlDataReader r = cmd.ExecuteReader();
                        while (r.Read())
                        {
                            topx.Add(new TopXCustomers()
                            {
                                CompanyCode = r["CompanyCode"].ToString(),
                                CompanyName = r["CompanyName"].ToString(),
                                TotalUsers = int.Parse(r["TotalUsers"].ToString()),
                                TotalMailboxes = 0,
                                TotalExchInMBUsed = 0,
                                TotalExchInMBAllocated = 0
                            });
                        }
                        r.Dispose();

                        // We have our top X customers now we must retrieve data.
                        foreach (var top in topx)
                        {
                            var mailboxUsers = (from u in db.Users where u.CompanyCode == top.CompanyCode where u.MailboxPlan > 0 select u).ToList();
                            top.TotalMailboxes = mailboxUsers.Count;

                            var plans = from p in db.Plans_ExchangeMailbox select p;
                            foreach (var u in mailboxUsers)
                            {
                                int planTotal = (from p in plans where p.MailboxPlanID == u.MailboxPlan select p.MailboxSizeMB).First();
                                int addedToPlan = u.AdditionalMB > 0 ? (int)u.AdditionalMB : 0;
                                top.TotalExchInMBAllocated = planTotal + addedToPlan;

                                cmd.Parameters.Clear();
                                cmd.CommandText = "SELECT TOP 1 TotalItemSizeInKB FROM SvcMailboxSizes WHERE UserPrincipalName=@UserPrincipalName ORDER BY Retrieved DESC";
                                cmd.Parameters.AddWithValue("UserPrincipalName", u.UserPrincipalName);

                                object val = cmd.ExecuteScalar();
                                if (val != DBNull.Value && val != null)
                                {
                                    top.TotalExchInMBUsed += decimal.Round(decimal.Parse(val.ToString()) / 1024);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving Top X Customers. Error: {0}", ex.ToString());
                    }
                    finally
                    {
                        if (cmd != null)
                            cmd.Dispose();

                        if (sql != null)
                            sql.Dispose();
                    }

                    return Response.AsJson(topx, HttpStatusCode.OK);
                };

            Get["/charts/{CompanyCode}/overview"] = _ =>
                {
                    // Set default values
                    Dictionary<string, int> data = new Dictionary<string, int>();
                    data.Add("MaxUsers", 0);
                    data.Add("Users", 0);
                    data.Add("Mailboxes", 0);
                    data.Add("Citrix Users", 0);
                    data.Add("Lync Users", 0);
                    data.Add("Distribution Groups", 0);
                    data.Add("Contacts", 0);

                    // Query database for actual values
                    try
                    {
                        string companyCode = _.CompanyCode;
                        if (string.IsNullOrEmpty(companyCode))
                            throw new Exception("Company code was null");

                        var company = (from c in db.Companies where !c.IsReseller where c.CompanyCode == companyCode select c).First();
                        var companyUsers = from u in db.Users where u.CompanyCode == companyCode select u;
                        var userIds = from u in companyUsers select u.ID;

                        data["MaxUsers"] = (from p in db.Plans_Organization where p.OrgPlanID == company.OrgPlanID select p.MaxUsers).First();
                        data["Users"] = companyUsers.Count();
                        data["Mailboxes"] = (from u in companyUsers where u.MailboxPlan > 0 select u).Count();
                        data["Citrix Users"] = (from u in db.UserPlansCitrix where userIds.Contains(u.UserID) select u.UserID).Distinct().Count();
                        data["Lync Users"] = 0;
                        data["Distribution Groups"] = (from d in db.DistributionGroups where d.CompanyCode == companyCode select d).Count();
                        data["Contacts"] = (from c in db.Contacts where c.CompanyCode == companyCode select c).Count();
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving company overview column chart. Error: {0}", ex.ToString());
                    }

                    return Response.AsJson(data, HttpStatusCode.OK);
                };

            #endregion

            #region Plans

            Get["/plans/company/{OrgPlanID}"] = _ =>
                {
                    try
                    {
                        int planId = _.OrgPlanID;

                        var plan = (from p in db.Plans_Organization
                                    where p.OrgPlanID == planId
                                    select p).FirstOrDefault();

                        return Response.AsJson(plan, HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving company plans. Error: {0}", ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                };

            #endregion
        }
    }
}