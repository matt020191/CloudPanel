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
    public class Settings
    {
        public static string ConnectionString
        {
            get 
            { 
                //return "server=DXN-PC\\SQLEXPRESS;database=CloudPanel;uid=CloudPanel;password=password;";
                return "server=JDIXON-PC\\SQLEXPRESS;database=CloudPanel;uid=sa;password=Password1";
            }
        }

        #region Basic Settings

        public static string HostingOU { get; set; }

        public static string UsersOU { get; set; }

        public static string PrimaryDC { get; set; }

        public static string Username { get; set; }

        public static string EncryptedPassword { get; set; }

        public static string[] SuperAdmins { get; set; }

        public static string[] BillingAdmins { get; set; }

        public static string CompanyName { get; set; }

        public static bool ResellersEnabled { get; set; }

        #endregion

        #region Billing

        public static string CurrencyName { get; set; }

        public static string CurrencySymbol { get; set; }

        #endregion

        #region Exchange Settings

        public static string ExchangeServer { get; set; }

        public static string ExchangePFServer { get; set; }

        public static int ExchangeVersion { get; set; }

        public static bool ExchangeSSL { get; set; }

        public static bool ExchangePFEnabled { get; set; }

        public static string ExchangeConnection { get; set; }

        public static string[] ExchangeDatabases { get; set; }

        #endregion

        #region Modules

        public static bool ExchangeModule { get; set; }

        public static bool CitrixModule { get; set; }

        public static bool LyncModule { get; set; }

        #endregion

        #region Security

        public static string SaltKey { get; set; }

        #endregion

        #region Advanced Customization

        private static string _exchangegalname;
        public static string ExchangeGALName
        {
            get { return string.IsNullOrEmpty(_exchangegalname) ? "{0} GAL" : _exchangegalname;  }
            set { _exchangegalname = value; }
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

        private static string _exchangecontactsname;
        public static string ExchangeCONTACTSName
        {
            get { return string.IsNullOrEmpty(_exchangecontactsname) ? "{0} - All Contacts" : _exchangecontactsname; }
            set { _exchangecontactsname = value; }
        }

        private static string _exchangeroomsname;
        public static string ExchangeROOMSName
        {
            get { return string.IsNullOrEmpty(_exchangeroomsname) ? "{0} - All Rooms" : _exchangeroomsname; }
            set { _exchangeroomsname = value; }
        }

        private static string _exchangegroupsname;
        public static string ExchangeGROUPSName
        {
            get { return string.IsNullOrEmpty(_exchangegroupsname) ? "{0} - All Users" : _exchangegroupsname; }
            set { _exchangegroupsname = value; }
        }

        private static string _exchangeou;
        public static string ExchangeOU
        {
            get { return string.IsNullOrEmpty(_exchangeou) ? "Exchange" : _exchangeou; }
            set { _exchangeou = value; }
        }

        private static string _applicationsou;
        public static string ApplicationsOU
        {
            get { return string.IsNullOrEmpty(_applicationsou) ? "Applications" : _applicationsou; }
            set { _applicationsou = value; }
        }

        #endregion

        #region Other Getters & Setters

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
        }

        #endregion
    }
}
