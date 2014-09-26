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
    using CloudPanel.Base.Enums;
    using CloudPanel.Base.Exchange;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DistributionGroups
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [StringLength(255)]
        public string DistinguishedName { get; set; }

        [StringLength(255)]
        public string CompanyCode { get; set; }

        [Required]
        [StringLength(255)]
        public string DisplayName { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        public bool Hidden { get; set; }

        #region Not Mapped

        [NotMapped]
        public string EmailFirst { get; set; }

        [NotMapped]
        public string EmailLast { get; set; }

        [NotMapped]
        public int DomainID { get; set; }

        [NotMapped]
        public string[] ManagedByOriginal { get; set; }

        [NotMapped]
        public List<GroupObjectSelector> ManagedByOriginalObject { get; set; }

        [NotMapped]
        public string[] ManagedByAdded { get; set; }

        [NotMapped]
        public string[] ManagedByRemoved { get; set; }

        [NotMapped]
        public string[] MembersOriginal { get; set; }

        [NotMapped]
        public List<GroupObjectSelector> MembersOriginalObject { get; set; }

        [NotMapped]
        public string[] MembersAdded { get; set; }

        [NotMapped]
        public string[] MembersRemoved { get; set; }

        [NotMapped]
        public int MemberJoinRestriction { get; set; }

        [NotMapped]
        public int MemberDepartRestriction { get; set; }

        [NotMapped]
        public int RequireSenderAuthenticationEnabled { get; set; }

        [NotMapped]
        public string[] AcceptMessagesOnlyFromSendersOrMembers { get; set; }

        [NotMapped]
        public bool ModerationEnabled { get; set; }

        [NotMapped]
        public string[] ModeratedBy { get; set; }

        [NotMapped]
        public string[] BypassModerationFromSendersOrMembers { get; set; }

        [NotMapped]
        public int SendModerationNotifications { get; set; }

        #endregion
    }
}
