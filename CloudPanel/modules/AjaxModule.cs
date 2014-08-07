using CloudPanel.Base.Database.Models;
using CloudPanel.code;
using CloudPanel.Database.EntityFramework;
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

            Get["/validation/domain"] = _ =>
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