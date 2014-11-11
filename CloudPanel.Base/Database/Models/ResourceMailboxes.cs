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
namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ResourceMailboxes
    {
        [Key]
        public int ResourceID { get; set; }

        public string DistinguishedName { get; set; }

        [Required]
        [StringLength(255)]
        public string DisplayName { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyCode { get; set; }

        [Required]
        [StringLength(255)]
        public string UserPrincipalName { get; set; }

        [Required]
        [StringLength(255)]
        public string PrimarySmtpAddress { get; set; }

        [Required]
        [StringLength(10)]
        public string ResourceType { get; set; }

        public int MailboxPlan { get; set; }

        public int AdditionalMB { get; set; }

        #region Not Mapped

        [NotMapped]
        public int SizeInMB { get; set; }

        [NotMapped]
        public string EmailDomain
        {
            get
            {
                if (string.IsNullOrEmpty(PrimarySmtpAddress))
                    return string.Empty;
                else
                    return PrimarySmtpAddress.Split('@')[1];
            }
        }

        [NotMapped]
        public string EmailFirst
        {
            get
            {
                if (string.IsNullOrEmpty(PrimarySmtpAddress))
                    return string.Empty;
                else
                    return PrimarySmtpAddress.Split('@')[0];
            }
        }

        [NotMapped]
        public string[] EmailAliases { get; set; }

        [NotMapped]
        public string[] EmailFullAccess { get; set; }

        [NotMapped]
        public string[] EmailFullAccessOriginal { get; set; }

        [NotMapped]
        public string[] EmailSendAs { get; set; }

        [NotMapped]
        public string[] EmailSendAsOriginal { get; set; }

        [NotMapped]
        public bool AutoMapping { get; set; }

        #endregion
    }
}
