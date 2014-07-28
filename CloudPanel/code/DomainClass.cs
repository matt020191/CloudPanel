using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
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

namespace CloudPanel.code
{
    public class DomainClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(DomainClass));

        private CloudPanelContext db = null;

        public DomainClass()
        {
            this.db = new CloudPanelContext(Settings.ConnectionString);
        }

        public void Update_Domain(Domain updateDomain)
        {
            if (string.IsNullOrEmpty(updateDomain.CompanyCode))
                throw new MissingFieldException("Domain", "CompanyCode");

            if (updateDomain.DomainID <= 0)
                throw new MissingFieldException("Domain", "ID");

            // Retrieve our domain that we are updating from the database
            var domain = (from d in db.Domains
                          where d.DomainID == updateDomain.DomainID
                          where d.CompanyCode == updateDomain.CompanyCode
                          select d).First();

            // Lets see if it is default
            if (updateDomain.IsDefault)
            {
                domain.IsDefault = true;

                // Remove default attribute from the rest since this is the new default
                var domains = from d in db.Domains
                              where d.CompanyCode == updateDomain.CompanyCode
                              select d;

                foreach (var d in domains)
                {
                    if (d.DomainID != domain.DomainID)
                        d.IsDefault = false;
                }
            }
            db.SaveChanges();

            //
            // Exchange settings
            //
            EmailClass email = new EmailClass();
            if (!domain.IsAcceptedDomain && updateDomain.IsAcceptedDomain)
                email.New_AcceptedDomain(updateDomain);
            else if (domain.IsAcceptedDomain && !updateDomain.IsAcceptedDomain)
                email.Remove_AcceptedDomain(updateDomain);
            else if (domain.IsAcceptedDomain && updateDomain.IsAcceptedDomain)
                email.Update_AcceptedDomain(updateDomain);
        }
    }
}