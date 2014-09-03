﻿//
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
using log4net;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;

namespace CloudPanel.code
{
    public class Users
    {
       private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Users));

        private CloudPanelContext db = null;

        public Users()
        {
            this.db = new CloudPanelContext(Settings.ConnectionString);
        }

        public List<User> GetUsers(string companyCode)
        {
            var users = (from u in db.Users
                         where u.CompanyCode == companyCode
                         orderby u.DisplayName
                         select u).ToList();

            return users;
        }

        public User GetUserFromSql(string companyCode, Guid userGuid)
        {
            var user = (from u in db.Users
                        where u.CompanyCode == companyCode
                        where u.UserGuid == userGuid
                        select u).First();

            return user;
        }

        public User GetUserFromAD(string companyCode, Guid userGuid)
        {
            ActiveDirectory.ADUsers users = null;

            try
            {
                users = new ActiveDirectory.ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                var sqlUser = GetUserFromSql(companyCode, userGuid);
                var adUser = users.GetUserWithoutGroups(sqlUser.UserPrincipalName);

                // Merge sqlUser into adUser
                adUser.CompanyCode = sqlUser.CompanyCode;
                adUser.IsResellerAdmin = sqlUser.IsResellerAdmin;
                adUser.IsCompanyAdmin = sqlUser.IsCompanyAdmin;
                adUser.MailboxPlan = sqlUser.MailboxPlan;
                adUser.TSPlan = sqlUser.TSPlan;
                adUser.LyncPlan = sqlUser.LyncPlan;
                adUser.AdditionalMB = sqlUser.AdditionalMB;
                adUser.ActiveSyncPlan = sqlUser.ActiveSyncPlan;
                adUser.ExchArchivePlan = sqlUser.ExchArchivePlan;

                return adUser;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error retrieving user {0}. Error: {1}", userGuid.ToString(), ex.ToString());
                throw;
            }
            finally
            {
                if (users != null)
                    users.Dispose();
            }
        }
    }
}