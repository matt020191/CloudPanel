using CloudPanel.Base.Security;
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
using System.Text;

namespace CloudPanel.Base.Config
{
    public static class Settings
    {
        public static string ConnectionString
        {
            get 
            {
                // Need ;Persist Security Info=True for EF6 connection open (when handling opening and closing connetions ourselves
                return "server=DXN-PC\\SQLEXPRESS;database=CloudPanel;uid=CloudPanel;password=password;";
                //return @"server=JDIXON-PC;database=CloudPanel;uid=CloudPanel;password=Password1";
            }
        }

        #region Basic Settings

        public static string HostingOU { get; set; }

        public static string UsersOU { get; set; }

        public static string PrimaryDC { get; set; }

        public static string Username { get; set; }

        public static string EncryptedPassword { get; set; }

        public static string[] SuperAdmins { get; set; }

        public static string SuperAdminsAsString
        {
            get
            {
                if (SuperAdmins == null || SuperAdmins.Length < 1)
                    return "";
                else
                    return String.Join(",", SuperAdmins);
            }
        }

        public static string[] BillingAdmins { get; set; }

        public static string BillingAdminsAsString
        {
            get
            {
                if (BillingAdmins == null || BillingAdmins.Length < 1)
                    return "";
                else
                    return String.Join(",", BillingAdmins);
            }
        }

        public static string CompanyName { get; set; }

        public static bool ResellersEnabled { get; set; }

        public static bool UseNameInsteadOfCompanyCode { get; set; }

        #endregion

        #region Billing

        public static string CurrencyName { get; set; }

        public static string CurrencySymbol { get; set; }

        #endregion

        #region Exchange Settings

        public static string ExchangeRoleAssignment { get; set; }

        public static string ExchangeServer { get; set; }

        public static string ExchangePFServer { get; set; }

        public static int ExchangeVersion { get; set; }

        public static bool ExchangePFEnabled { get; set; }

        public static string ExchangeConnection { get; set; }

        public static string[] ExchangeDatabases { get; set; }

        public static string ExchangeDatabasesAsString
        {
            get
            {
                if (ExchangeDatabases == null || ExchangeDatabases.Length < 1)
                    return "";
                else
                    return String.Join(",", ExchangeDatabases);
            }
        }

        private static string _exchangegalname;
        public static string ExchangeGALName
        {
            get { return string.IsNullOrEmpty(_exchangegalname) ? "{0} GAL" : _exchangegalname; }
            set { _exchangegalname = value; }
        }

        private static string _exchangegalfilter;
        public static string ExchangeGALFilter
        {
            get { return string.IsNullOrEmpty(_exchangegalfilter) ? "{{(Alias -ne $null) -and (CustomAttribute1 -eq '{0}')}}" : _exchangegalfilter; }
            set { _exchangegalfilter = value; }
        }

        private static string _exchangeabpname;
        public static string ExchangeABPName
        {
            get { return string.IsNullOrEmpty(_exchangeabpname) ? "{0} ABP" : _exchangeabpname; }
            set { _exchangeabpname = value; }
        }

        private static string _exchangeoalname;
        public static string ExchangeOALName
        {
            get { return string.IsNullOrEmpty(_exchangeoalname) ? "{0} OAL" : _exchangeoalname; }
            set { _exchangeoalname = value; }
        }

        private static string _exchangeusersname;
        public static string ExchangeUSERSName
        {
            get { return string.IsNullOrEmpty(_exchangeusersname) ? "{0} - All Users" : _exchangeusersname; }
            set { _exchangeusersname = value; }
        }

        private static string _exchangeusersfilter;
        public static string ExchangeUSERSFilter
        {
            get { return string.IsNullOrEmpty(_exchangeusersfilter) ? "{{(RecipientType -eq 'UserMailbox') -and (CustomAttribute1 -eq '{0}')}}" : _exchangeusersfilter; }
            set { _exchangeusersfilter = value; }
        }

        private static string _exchangecontactsname;
        public static string ExchangeCONTACTSName
        {
            get { return string.IsNullOrEmpty(_exchangecontactsname) ? "{0} - All Contacts" : _exchangecontactsname; }
            set { _exchangecontactsname = value; }
        }

        private static string _exchangecontactsfilter;
        public static string ExchangeCONTACTSFilter
        {
            get { return string.IsNullOrEmpty(_exchangecontactsfilter) ? "{{((RecipientType -eq 'MailContact') -or (RecipientType -eq 'MailUser')) -and (CustomAttribute1 -eq '{0}')}}" : _exchangecontactsfilter; }
            set { _exchangecontactsfilter = value; }
        }

        private static string _exchangeroomsname;
        public static string ExchangeROOMSName
        {
            get { return string.IsNullOrEmpty(_exchangeroomsname) ? "{0} - All Rooms" : _exchangeroomsname; }
            set { _exchangeroomsname = value; }
        }

        private static string _exchangeroomsfilter;
        public static string ExchangeROOMSFilter
        {
            get { return string.IsNullOrEmpty(_exchangeroomsfilter) ? "{{(Alias -ne $null) -and ((RecipientDisplayType -eq 'ConferenceRoomMailbox') -or (RecipientDisplayType -eq 'SyncedConferenceRoomMailbox')) -and (CustomAttribute1 -eq '{0}')}}" : _exchangeroomsfilter; }
            set { _exchangeroomsfilter = value; }
        }

        private static string _exchangegroupsname;
        public static string ExchangeGROUPSName
        {
            get { return string.IsNullOrEmpty(_exchangegroupsname) ? "{0} - All Users" : _exchangegroupsname; }
            set { _exchangegroupsname = value; }
        }

        private static string _exchangegroupsfilter;
        public static string ExchangeGROUPSFilter
        {
            get { return string.IsNullOrEmpty(_exchangegroupsfilter) ? "{{((RecipientType -eq 'MailUniversalDistributionGroup') -or (RecipientType -eq 'DynamicDistributionGroup')) -and (CustomAttribute1 -eq '{0}')}}" : _exchangegroupsfilter; }
            set { _exchangegroupsfilter = value; }
        }

        private static string _exchangeouname;
        public static string ExchangeOUName
        {
            get { return string.IsNullOrEmpty(_exchangeouname) ? "Exchange" : _exchangeouname; }
            set { _exchangeouname = value; }
        }

        private static string _exchangegroupsou;
        public static string ExchangeGroupsOU
        {
            get { return string.IsNullOrEmpty(_exchangegroupsou) ? "Exchange" : _exchangegroupsou; }
            set { _exchangegroupsou = value; }
        }

        private static string _exchangecontactsou;
        public static string ExchangeContactsOU
        {
            get { return string.IsNullOrEmpty(_exchangecontactsou) ? "Exchange" : _exchangecontactsou; }
            set { _exchangecontactsou = value; }
        }

        private static string _exchangeroomsou;
        public static string ExchangeRoomsOU
        {
            get { return string.IsNullOrEmpty(_exchangeroomsou) ? "Exchange" : _exchangeroomsou; }
            set { _exchangeroomsou = value; }
        }

        private static string _exchangeresourceou;
        public static string ExchangeResourceOU
        {
            get { return string.IsNullOrEmpty(_exchangeresourceou) ? "Exchange" : _exchangeresourceou; }
            set { _exchangeresourceou = value; }
        }

        #endregion

        #region Modules

        public static bool ExchangeModule { get; set; }

        public static bool CitrixModule { get; set; }

        public static bool LyncModule { get; set; }

        #endregion

        #region Security

        public static string SaltKey { get; set; }

        #endregion

        #region Support Notifications

        public static bool SNEnabled { get; set; }

        public static string SNFrom { get; set; }

        public static string SNTo { get; set; }

        public static string SNServer { get; set; }

        public static int SNPort { get; set; }

        public static string SNUsername { get; set; }

        public static string SNEncryptedPassword { get; set; }

        #endregion

        #region Advanced Customization

        private static string _applicationsouname;
        public static string ApplicationsOUName
        {
            get { return string.IsNullOrEmpty(_applicationsouname) ? "Applications" : _applicationsouname; }
            set { _applicationsouname = value; }
        }

        #endregion

        #region Other Getters & Setters

        public static string ExchangeUri
        {
            get
            {
                return string.Format("https://{0}/powershell", ExchangeServer);
            }
        }

        public static string OUPath(string childOUName, string parentOu)
        {
            return string.Format("OU={0},{1}",childOUName, parentOu);
        }

        public static string ExchangeOuPath(string parentOu)
        {
            return string.Format("OU={0},{1}", ExchangeOUName, parentOu);
        }

        public static string ApplicationOuPath(string parentOu)
        {
            return string.Format("OU={0},{1}", ApplicationsOUName, parentOu);
        }

        public static string UsersOuPath(string parentOu)
        {
            if (string.IsNullOrEmpty(UsersOU))
                return parentOu;
            else
                return string.Format("OU={0},{1}", UsersOU, parentOu);
        }

        public static string EncryptPassword
        {
            set
            {
                EncryptedPassword = DataProtection.Encrypt(value, SaltKey);
            }
        }
        public static string DecryptedPassword
        {
            get
            {
                return DataProtection.Decrypt(EncryptedPassword, SaltKey);
            }
            set
            {
                EncryptPassword = value;
            }
        }

        #endregion
    }
}
