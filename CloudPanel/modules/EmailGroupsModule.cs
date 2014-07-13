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
using CloudPanel.code;
using CloudPanel.Base.Database.Models;

namespace CloudPanel.modules
{
    public class EmailGroupsModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(EmailGroupsModule));

        public EmailGroupsModule() : base("/Company/Email/Groups")
        {
            this.RequiresAuthentication();

            Get["/{CompanyCode}"] = _ =>
                {
                    return View["c_exchange_groups.cshtml", _.CompanyCode];
                };

            Get["/{CompanyCode}/json"] = _ =>
                {
                    try
                    {
                        EmailClass email = new EmailClass();
                        List<DistributionGroup> groups = email.GetDistribtionGroups(_.CompanyCode);

                        var search = Request.Query.sSearch.HasValue ? (string)Request.Query.sSearch : "";

                        // This is if we are searching..
                        if (!string.IsNullOrEmpty(search))
                            groups = groups.Where(c => (
                                                        c.DisplayName.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1 ||
                                                        c.Email.IndexOf(search, 0, StringComparison.CurrentCultureIgnoreCase) != -1
                                                    )).ToList();

                        int start = Convert.ToInt32(Request.Query.iDisplayStart.ToString());
                        int length = Convert.ToInt32(Request.Query.iDisplayLength.ToString());
                        var totalRecords = groups.Count();
                        var secho = Request.Query.sEcho;
                        var sorting = Request.Query.sSortDir_0;

                        if (sorting == "asc")
                        {
                            return Response.AsJson(new
                            {
                                aaData = groups.OrderBy(x => x.DisplayName).Skip(start).Take(length),
                                sEcho = secho,
                                iTotalRecords = totalRecords,
                                iTotalDisplayRecords = totalRecords
                            });
                        }
                        else
                        {
                            return Response.AsJson(new
                            {
                                aaData = groups.OrderByDescending(x => x.DisplayName).Skip(start).Take(length),
                                sEcho = secho.ToString(),
                                iTotalRecords = totalRecords,
                                iTotalDisplayRecords = totalRecords
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving groups under company {0}. Exception: {1}", _.CompanyCode, ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                };
        }
    }
}