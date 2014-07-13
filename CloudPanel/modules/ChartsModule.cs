using log4net;
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
using CloudPanel.Base.Charting;
using System.Data.SqlClient;
using CloudPanel.Base.Config;

namespace CloudPanel.modules
{
    public class ChartsModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(AjaxModule));

        private readonly CloudPanelContext db;

        public ChartsModule(CloudPanelContext db) : base("/charts")
        {
            this.RequiresAuthentication();
            this.db = db;

            Get["/column/overview/{CompanyCode}/json"] = _ =>
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

                        return Response.AsJson(data, HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving company overview column chart. Error: {0}", ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                };

            Get["/column/dashboard/top/{x}/json"] = _ =>
            {
                this.RequiresAnyClaim(new[] { "SuperAdmin" });

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

                    return Response.AsJson(topx, HttpStatusCode.OK);
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error retrieving Top X Customers. Error: {0}", ex.ToString());
                    return HttpStatusCode.InternalServerError;
                }
                finally
                {
                    if (cmd != null)
                        cmd.Dispose();

                    if (sql != null)
                        sql.Dispose();
                }
            };
        }
    }
}