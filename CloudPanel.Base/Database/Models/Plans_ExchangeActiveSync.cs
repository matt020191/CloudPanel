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
    public partial class Plans_ExchangeActiveSync
    {
        public int ASID { get; set; }
        public string CompanyCode { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ExchangeName { get; set; }
        public Nullable<bool> AllowNonProvisionableDevices { get; set; }
        public Nullable<int> RefreshIntervalInHours { get; set; }
        public Nullable<bool> RequirePassword { get; set; }
        public Nullable<bool> RequireAlphanumericPassword { get; set; }
        public Nullable<bool> EnablePasswordRecovery { get; set; }
        public Nullable<bool> RequireEncryptionOnDevice { get; set; }
        public Nullable<bool> RequireEncryptionOnStorageCard { get; set; }
        public Nullable<bool> AllowSimplePassword { get; set; }
        public Nullable<int> NumberOfFailedAttempted { get; set; }
        public Nullable<int> MinimumPasswordLength { get; set; }
        public Nullable<int> InactivityTimeoutInMinutes { get; set; }
        public Nullable<int> PasswordExpirationInDays { get; set; }
        public Nullable<int> EnforcePasswordHistory { get; set; }
        public string IncludePastCalendarItems { get; set; }
        public string IncludePastEmailItems { get; set; }
        public Nullable<int> LimitEmailSizeInKB { get; set; }
        public Nullable<bool> AllowDirectPushWhenRoaming { get; set; }
        public Nullable<bool> AllowHTMLEmail { get; set; }
        public Nullable<bool> AllowAttachmentsDownload { get; set; }
        public Nullable<int> MaximumAttachmentSizeInKB { get; set; }
        public Nullable<bool> AllowRemovableStorage { get; set; }
        public Nullable<bool> AllowCamera { get; set; }
        public Nullable<bool> AllowWiFi { get; set; }
        public Nullable<bool> AllowInfrared { get; set; }
        public Nullable<bool> AllowInternetSharing { get; set; }
        public Nullable<bool> AllowRemoteDesktop { get; set; }
        public Nullable<bool> AllowDesktopSync { get; set; }
        public string AllowBluetooth { get; set; }
        public Nullable<bool> AllowBrowser { get; set; }
        public Nullable<bool> AllowConsumerMail { get; set; }
        public Nullable<bool> IsEnterpriseCAL { get; set; }
        public Nullable<bool> AllowTextMessaging { get; set; }
        public Nullable<bool> AllowUnsignedApplications { get; set; }
        public Nullable<bool> AllowUnsignedInstallationPackages { get; set; }
    }
}
