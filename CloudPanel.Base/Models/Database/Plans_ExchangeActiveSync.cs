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
namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Plans_ExchangeActiveSync
    {
        [Key]
        public int ASID { get; set; }

        [StringLength(255)]
        public string CompanyCode { get; set; }

        [Required]
        [StringLength(150)]
        public string DisplayName { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [StringLength(75)]
        public string ExchangeName { get; set; }

        public bool AllowNonProvisionableDevices { get; set; }

        public int? RefreshIntervalInHours { get; set; }

        public bool RequirePassword { get; set; }

        public bool RequireAlphanumericPassword { get; set; }

        public bool EnablePasswordRecovery { get; set; }

        public bool RequireEncryptionOnDevice { get; set; }

        public bool RequireEncryptionOnStorageCard { get; set; }

        public bool AllowSimplePassword { get; set; }

        public int? MinDevicePasswordComplexCharacters { get; set; }

        public int? NumberOfFailedAttempted { get; set; }

        public int? MinimumPasswordLength { get; set; }

        public int? InactivityTimeoutInMinutes { get; set; }

        public int? PasswordExpirationInDays { get; set; }

        public int? EnforcePasswordHistory { get; set; }

        [StringLength(20)]
        public string IncludePastCalendarItems { get; set; }

        [StringLength(20)]
        public string IncludePastEmailItems { get; set; }

        public int? LimitEmailSizeInKB { get; set; }

        public bool AllowDirectPushWhenRoaming { get; set; }

        public bool AllowHTMLEmail { get; set; }

        public bool AllowAttachmentsDownload { get; set; }

        public int? MaximumAttachmentSizeInKB { get; set; }

        public bool AllowRemovableStorage { get; set; }

        public bool AllowCamera { get; set; }

        public bool AllowWiFi { get; set; }

        public bool AllowInfrared { get; set; }

        public bool AllowInternetSharing { get; set; }

        public bool AllowRemoteDesktop { get; set; }

        public bool AllowDesktopSync { get; set; }

        [StringLength(10)]
        public string AllowBluetooth { get; set; }

        public bool AllowBrowser { get; set; }

        public bool AllowConsumerMail { get; set; }

        public bool? IsEnterpriseCAL { get; set; }

        public bool AllowTextMessaging { get; set; }

        public bool AllowUnsignedApplications { get; set; }

        public bool AllowUnsignedInstallationPackages { get; set; }
    }
}
