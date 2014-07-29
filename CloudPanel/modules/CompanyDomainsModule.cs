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
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using log4net;
using CloudPanel.code;
using CloudPanel.Base.Database.Models;

namespace CloudPanel.modules
{
    public class CompanyDomainsModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(CompanyDomainsModule));

        public CompanyDomainsModule() : base("/Company/Domains")
        {
            this.RequiresAuthentication();

            // Get list of domains
            Get["/{CompanyCode}"] = _ =>
                {
                    return View["c_domains.cshtml", _.CompanyCode];
                };

            Get["/{CompanyCode}/json"] = _ =>
                {
                    try
                    {
                        Companies companies = new Companies();
                        List<Domain> domains = companies.GetDomains(_.CompanyCode);

                        var search = Request.Query.sSearch.HasValue ? ((string)Request.Query.sSearch).ToLower() : "";

                        // This is if we are searching..
                        if (!string.IsNullOrEmpty(search))
                            domains = domains.Where(c => (
                                                    c.Domain1.ToLower().Contains(search)
                                                )).ToList();

                        int start = Convert.ToInt32(Request.Query.iDisplayStart.ToString());
                        int length = Convert.ToInt32(Request.Query.iDisplayLength.ToString());
                        var totalRecords = domains.Count();
                        var secho = Request.Query.sEcho;
                        var sorting = Request.Query.sSortDir_0;

                        if (sorting == "asc")
                        {
                            return Response.AsJson(new
                            {
                                aaData = domains.OrderBy(x => x.Domain1).Skip(start).Take(length),
                                iTotalRecords = totalRecords,
                                iTotalDisplayRecords = totalRecords
                            });
                        }
                        else
                        {
                            return Response.AsJson(new
                            {
                                aaData = domains.OrderByDescending(x => x.Domain1).Skip(start).Take(length),
                                iTotalRecords = totalRecords,
                                iTotalDisplayRecords = totalRecords
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving domains under company {0}. Exception: {1}", _.CompanyCode, ex.ToString());
                        return HttpStatusCode.InternalServerError;
                    }
                };

            // Add a new domain
            Post["/{CompanyCode}"] = _ =>
                {
                    try
                    {
                        Companies companies = new Companies();
                        companies.AddDomain(_.CompanyCode, Request.Form.DomainName);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error adding domain {0} for company {1}. Error: {2}", Request.Form.DomainName, _.CompanyCode, ex.ToString());
                        ViewBag.Error = ex.Message;
                    }
                    
                    return View["c_domains.cshtml", _.CompanyCode];
                };

            // Get a specific domain
            Get["/{CompanyCode}/{DomainID}"] = _ =>
                {
                    try
                    {
                        Companies companies = new Companies();
                        Domain domain = companies.GetDomain(_.CompanyCode, _.DomainID);

                        return View["c_domainsedit.cshtml", new { Domain = domain, IsExchangeEnabled = companies.IsExchangeEnabled(_.CompanyCode) }];
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error retrieving company domain for {0} with id {1}. Error: {2}", _.CompanyCode, _.DomainID, ex.ToString());
                        return View["error.cshtml", ex.ToString()];
                    }
                };

            Put["/{CompanyCode}/{DomainID}"] = _ =>
                {
                    try
                    {
                        Domain updateDomain = this.Bind<Domain>();
                        updateDomain.CompanyCode = _.CompanyCode;

                        DomainClass domain = new DomainClass();
                        domain.Update_Domain(updateDomain);

                        string redirect = string.Format("~/Company/Domains/{0}", _.CompanyCode);
                        return this.Response.AsRedirect(redirect);
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error updating company domain for {0} with id {1}. Error: {2}", _.CompanyCode, _.DomainID, ex.ToString());
                        return View["error.cshtml", ex.ToString()];
                    }
                };
        }
    }
}