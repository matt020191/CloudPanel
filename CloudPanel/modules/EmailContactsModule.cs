using CloudPanel.Base.Database.Models;
using CloudPanel.code;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
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

namespace CloudPanel.modules
{
    public class EmailContactsModule : NancyModule
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(EmailContactsModule));

        public EmailContactsModule() : base("/Company/Email/Contacts")
        {
            this.RequiresAuthentication();

            Get["/{CompanyCode}"] = _ =>
                {
                    return GetContacts(_.CompanyCode);
                };

            Get["/{CompanyCode}/{ContactID}"] = _ =>
                {
                    return GetContact(_.ContactID);
                };

            Post["/{CompanyCode}"] = _ =>
                {
                    try
                    {
                        var contact = this.Bind<Contact>();
                        contact.CompanyCode = _.CompanyCode;

                        EmailClass email = new EmailClass();
                        email.New_MailContact(contact);                        

                        return GetContacts(_.CompanyCode);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return GetContacts(_.CompanyCode);
                    }
                };

            Put["/{CompanyCode}/{ContactID}"] = _ =>
                {
                    try
                    {
                        var contact = this.Bind<Contact>();
                        contact.CompanyCode = _.CompanyCode;
                        contact.ID = _.ContactID;

                        EmailClass email = new EmailClass();
                        email.Update_MailContact(contact);

                        string redirect = string.Format("~/Company/Email/Contacts/{0}", _.CompanyCode);
                        return this.Response.AsRedirect(redirect);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return View["error.cshtml", ex.ToString()];
                    }
                };

            Delete["/{CompanyCode}"] = _ =>
                {
                    try
                    {
                        var contact = this.Bind<Contact>();
                        contact.CompanyCode = _.CompanyCode;

                        EmailClass email = new EmailClass();
                        email.Remove_MailContact(contact.CompanyCode, contact.ID);

                        return GetContacts(_.CompanyCode);
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Error = ex.Message;
                        return GetContacts(_.CompanyCode);
                    }
                };
        }

        /// <summary>
        /// Gets all contacts and returns the contacts page
        /// or the error page if there was an error
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        private Negotiator GetContacts(string companyCode)
        {
            try
            {
                Companies companies = new Companies();
                List<Contact> contacts = companies.GetContacts(companyCode);

                return View["c_exchange_contacts.cshtml", contacts];
            }
            catch (Exception ex)
            {
                return View["error.cshtml", ex.ToString()];
            }
        }

        /// <summary>
        /// Gets a specific contact and returns the contacts edit page
        /// or the error page if there was an error
        /// </summary>
        /// <param name="contactID"></param>
        /// <returns></returns>
        private Negotiator GetContact(int contactID)
        {
            try
            {
                Companies companies = new Companies();
                Contact contact = companies.GetContact(contactID);

                return View["c_exchange_contactsedit.cshtml", contact];
            }
            catch (Exception ex)
            {
                return View["error.cshtml", ex.ToString()];
            }
        }
    }
}