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

namespace CloudPanel.Base.Database.Models
{
    public partial class Setting
    {
        public string BaseOU { get; set; }
        public string PrimaryDC { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SuperAdmins { get; set; }
        public string BillingAdmins { get; set; }
        public string ExchangeFqdn { get; set; }
        public string ExchangePFServer { get; set; }
        public int ExchangeVersion { get; set; }
        public bool ExchangeSSLEnabled { get; set; }
        public string ExchangeConnectionType { get; set; }
        public int PasswordMinLength { get; set; }
        public int PasswordComplexityType { get; set; }
        public bool CitrixEnabled { get; set; }
        public bool PublicFolderEnabled { get; set; }
        public bool LyncEnabled { get; set; }
        public bool WebsiteEnabled { get; set; }
        public bool SQLEnabled { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyEnglishName { get; set; }
        public Nullable<bool> ResellersEnabled { get; set; }
        public string CompanysName { get; set; }
        public Nullable<bool> AllowCustomNameAttrib { get; set; }
        public Nullable<bool> ExchStats { get; set; }
        public Nullable<bool> IPBlockingEnabled { get; set; }
        public Nullable<int> IPBlockingFailedCount { get; set; }
        public Nullable<int> IPBlockingLockedMinutes { get; set; }
        public string ExchDatabases { get; set; }
        public string UsersOU { get; set; }
        public string BrandingLoginLogo { get; set; }
        public string BrandingCornerLogo { get; set; }
        public Nullable<bool> LockdownEnabled { get; set; }
        public string LyncFrontEnd { get; set; }
        public string LyncUserPool { get; set; }
        public string LyncMeetingUrl { get; set; }
        public string LyncDialinUrl { get; set; }
        public string CompanysLogo { get; set; }
        public Nullable<bool> SupportMailEnabled { get; set; }
        public string SupportMailAddress { get; set; }
        public string SupportMailServer { get; set; }
        public Nullable<int> SupportMailPort { get; set; }
        public string SupportMailUsername { get; set; }
        public string SupportMailPassword { get; set; }
        public string SupportMailFrom { get; set; }
        public int ID { get; set; }
    }
}
