using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Exceptions;
using CloudPanel.Database.EntityFramework;
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
    public class Plans
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Plans));

        private CloudPanelContext db = null;

        public Plans()
        {
            this.db = new CloudPanelContext(Settings.ConnectionString);
        }

        public void Update_Organization(int planId, Plans_Organization newPlan)
        {
            var existingPlan = (from p in db.Plans_Organization
                                where p.OrgPlanID == planId
                                select p).First();

            existingPlan.OrgPlanName = newPlan.OrgPlanName;
            existingPlan.MaxUsers = newPlan.MaxUsers;
            existingPlan.MaxDomains = newPlan.MaxDomains;
            existingPlan.MaxExchangeMailboxes = newPlan.MaxExchangeMailboxes;
            existingPlan.MaxExchangeContacts = newPlan.MaxExchangeContacts;
            existingPlan.MaxExchangeDistLists = newPlan.MaxExchangeDistLists;
            existingPlan.MaxExchangeResourceMailboxes = newPlan.MaxExchangeResourceMailboxes;
            existingPlan.MaxExchangeMailPublicFolders = newPlan.MaxExchangeMailPublicFolders;
            existingPlan.MaxTerminalServerUsers = newPlan.MaxTerminalServerUsers;

            db.SaveChanges();
        }

        public void Create_Organization(Plans_Organization newPlan)
        {
            if (string.IsNullOrEmpty(newPlan.OrgPlanName))
                throw new MissingDataException("OrgPlanName");

            db.Plans_Organization.Add(newPlan);
            db.SaveChanges();
        }

        public void Delete_Organization(int planId)
        {
            // Check for companies using the plan
            var count = (from u in db.Companies
                         where u.OrgPlanID == planId
                         select u.CompanyId).Count();

            if (count > 0)
                throw new MatchFoundException(count.ToString());
            else
            {
                var plan = (from p in db.Plans_Organization
                         where p.OrgPlanID == planId
                         select p).First();

                db.Plans_Organization.Remove(plan);
                db.SaveChanges();
            }
        }

        public Plans_ExchangeMailbox Get_MailboxPlan(int planId)
        {
            var plan = (from p in db.Plans_ExchangeMailbox
                        where p.MailboxPlanID == planId
                        select p).First();

            return plan;
        }
    }
}