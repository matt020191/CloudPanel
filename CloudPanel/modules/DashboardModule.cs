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
using Nancy.Authentication;
using Nancy.Security;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using System.Globalization;
using System.Data.SqlClient;
using System.Threading;

namespace CloudPanel.modules
{
    public class DashboardModule : NancyModule
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DashboardModule));

        public DashboardModule() : base("/dashboard")
        {
           // this.RequiresAuthentication();

            Get["/"] = _ =>
                {
                    return View["dashboard.cshtml"];
                };

            Get["/chart/progress/json"] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        // Get the total count of users
                        int users = (from u in db.Users select u.ID).Count();
                        int domains = (from d in db.Domains select d.DomainID).Count();

                        //
                        // Get Exchange information
                        // 
                        int mailboxes = 0;
                        int acceptedDomains = 0;
                        if (Settings.ExchangeModule)
                        {
                            mailboxes = (from u in db.Users where u.MailboxPlan > 0 select u.ID).Count();
                            acceptedDomains = (from d in db.Domains where d.IsAcceptedDomain select d.DomainID).Count();
                        }


                        return Response.AsJson(new
                        {
                            totalusers = users,
                            totalmailboxes = mailboxes,
                            totaldomains = domains,
                            totalaccepteddomains = acceptedDomains
                        });
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving area chart for dashboard. Error: {0}", ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                };

            Get["/chart/pie/json"] = _ =>
                {
                    SqlConnection sql = null;
                    SqlCommand cmd = null;
                    try
                    {
                        sql = new SqlConnection(Settings.ConnectionString);
                        cmd = new SqlCommand(@"SELECT TOP 5
                                               (SELECT CompanyName FROM Companies WHERE CompanyCode=u.CompanyCode) AS CompanyName,
                                               COUNT(ID) AS Total FROM Users u
                                               GROUP BY CompanyCode ORDER BY Total DESC", sql);

                        var array = new List<object>();
                        sql.Open();

                        SqlDataReader r = cmd.ExecuteReader();
                        while (r.Read())
                        {
                            array.Add(new
                            {
                                name = r["CompanyName"].ToString(),
                                y = (int)r["Total"]
                            });
                        }

                        return Response.AsJson(new
                        {
                            totals = array
                        });
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving pie chart for dashboard. Error: {0}", ex.ToString());
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

            Get["/chart/area/json"] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        string[] months = new string[12];

                        int?[] userCount = new int?[12];
                        int?[] exchCount = new int?[12];
                        int?[] citrixCount = new int?[12];

                        DateTime yearAgo = DateTime.Now.AddMonths(-11);
                        for (int i = 0; i < 12; i++)
                        {
                            months[i] = yearAgo.ToString("MMM");
                            userCount[i] = GetUserCount(ref db, yearAgo);

                            if (Settings.ExchangeModule)
                                exchCount[i] = GetMailboxCount(ref db, yearAgo);

                            if (Settings.CitrixModule)
                                citrixCount[i] = GetCitrixCount(ref db, yearAgo);

                            yearAgo = yearAgo.AddMonths(1);
                        }

                        return Response.AsJson(new
                            {
                                months = months,
                                users = userCount,
                                exchange = exchCount,
                                citrix = citrixCount
                            });
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving area chart for dashboard. Error: {0}", ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                };
        }

        private int? GetUserCount(ref CloudPanelContext db, DateTime pickDate)
        {
            int value = 0;

            if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                value = (from u in db.Users
                         select u.ID).Count();
            else
                value = (from d in db.Stats_UserCount
                         where d.StatDate.Month == pickDate.Month
                         where d.StatDate.Year == pickDate.Year
                         orderby d.StatDate descending
                         select d.UserCount).FirstOrDefault();

            if (value == 0)
                return null;
            else
                return value;
        }

        private int? GetMailboxCount(ref CloudPanelContext db, DateTime pickDate)
        {
            int value = 0;

            if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                value = (from u in db.Users
                         where u.MailboxPlan > 0
                         select u.ID).Count();
            else
                value = (from d in db.Stats_ExchCount
                         where d.StatDate.Month == pickDate.Month
                         where d.StatDate.Year == pickDate.Year
                         orderby d.StatDate descending
                         select d.UserCount).FirstOrDefault();

            if (value == 0)
                return null;
            else
                return value;
        }

        private int? GetCitrixCount(ref CloudPanelContext db, DateTime pickDate)
        {
            int value = 0;

            if (pickDate.Month == DateTime.Now.Month && pickDate.Year == DateTime.Now.Year)
                value = (from u in db.UserPlansCitrix
                         select u.UserID).Distinct().Count();
            else
                value = (from d in db.Stats_CitrixCount
                         where d.StatDate.Month == pickDate.Month
                         where d.StatDate.Year == pickDate.Year
                         orderby d.StatDate descending
                         select d.UserCount).FirstOrDefault();

            if (value == 0)
                return null;
            else
                return value;
        }
    }
}