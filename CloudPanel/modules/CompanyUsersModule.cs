using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Other;
using CloudPanel.code;
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

namespace CloudPanel.modules
{
    public class CompanyUsersModule : NancyModule
    {
        public CompanyUsersModule() : base("/Company/Users")
        {
            this.RequiresAuthentication();
            
            Get["/{CompanyCode}"] = _ =>
            {
                return View["c_users.cshtml", _.CompanyCode];
            };

            Get["/{CompanyCode}/{UserGuid}"] = _ =>
            {
                try
                {
                    UsersEditPage uep = new UsersEditPage();

                    Companies companies = new Companies();
                    Users users = new Users();

                    // Get the user from Active Directory
                    User adUser = users.GetUserFromAD(_.CompanyCode, _.UserGuid);
                    uep.User = adUser;

                    // Get a list of domains
                    uep.Domains = companies.GetDomains(_.CompanyCode);

                    // Get list of mailbox plans
                    uep.MailboxPlans = companies.GetMailboxPlans(_.CompanyCode);

                    // Get a list of activesync plans
                    uep.ActiveSyncPlans = companies.GetActiveSyncPlans(_.CompanyCode);

                    // Get list of users and their sAMAccountName
                    uep.UsersPermissionList = companies.GetUsersListAndSamAccountName(_.CompanyCode);

                    return View["c_usersedit.cshtml", uep];
                }
                catch (Exception ex)
                {
                    return View["error.cshtml", ex.ToString()];
                }
            };
        }
    }
}