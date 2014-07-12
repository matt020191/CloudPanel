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
using CloudPanel.code;
using CloudPanel.Base.Database.Models;
using System.Threading;
using log4net;

namespace CloudPanel.modules
{
    public class ResellersModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(ResellersModule));

        public ResellersModule() : base("/Resellers")
        {
            this.RequiresAuthentication();

            #region Resellers

            Get["/"] = _ =>
                {
                    //this.RequiresAuthentication();
                    //this.RequiresClaims(new[] { "SuperAdmin" });

                    try
                    {
                        Resellers resellers = new Resellers();
                        return View["resellers.cshtml", resellers.GetAll()];
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        log.ErrorFormat("Failed to retrieve resellers. Exception: {0}", ex.ToString());

                        return View["resellers.cshtml"];
                    }
                };

            Get["/{ResellerCode}"] = _ =>
                {
                    //this.RequiresAuthentication();
                    //this.RequiresClaims(new[] { "SuperAdmin" });

                    try
                    {
                        Resellers resellers = new Resellers();
                        Company foundReseller = resellers.GetReseller(_.ResellerCode);

                        return Response.AsJson(foundReseller, HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return View["resellers.cshtml", GetResellers()];
                    }
                };

            Put["/"] = _ =>
            {
                //this.RequiresAuthentication();
                //this.RequiresClaims(new[] { "SuperAdmin" });

                try
                {
                    var reseller = this.Bind<Company>();

                    Resellers resellers = new Resellers();
                    resellers.Update(reseller);

                    return View["resellers.cshtml", resellers.GetAll()];
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.Message;
                    return View["resellers.cshtml", GetResellers()];
                }
            };

            Delete["/"] = _ =>
                {
                    try
                    {
                        string companyName = Request.Form.CompanyNameValidation;
                        if (string.IsNullOrEmpty(companyName))
                            throw new MissingFieldException("Resellers", "CompanyName");

                        string companyCode = Request.Form.CompanyCode;
                        if (string.IsNullOrEmpty(companyCode))
                            throw new MissingFieldException("Resellers", "CompanyCode");

                        Resellers resellers = new Resellers();
                        resellers.Delete(companyCode);

                        return View["resellers.cshtml", resellers.GetAll()];
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return View["resellers.cshtml", GetResellers()];
                    }
                };

            Post["/"] = _ =>
                {
                    Resellers resellers = new Resellers();

                    try
                    {
                        var data = this.Bind<Company>();
                        resellers.Create(data);

                        return View["resellers.cshtml", resellers.GetAll()];
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return View["resellers.cshtml", GetResellers()];
                    }
                };

            #endregion

            
        }

        /// <summary>
        /// This is used for the catch station to pull the 
        /// resellers so we don't send back a blank page
        /// It doesn't throw an exception which is why we can use it 
        /// </summary>
        /// <returns></returns>
        private dynamic GetResellers()
        {
            try
            {
                Resellers resellers = new Resellers();
                return resellers.GetAll();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to retrieve resellers from the database. Error: {0}", ex.ToString());
                return null;
            }
        }
    }
}