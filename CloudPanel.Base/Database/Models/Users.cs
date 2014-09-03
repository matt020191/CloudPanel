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

    public partial class Users
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        public Guid UserGuid { get; set; }

        [StringLength(255)]
        public string CompanyCode { get; set; }

        [StringLength(255)]
        public string sAMAccountName { get; set; }

        [Required]
        [StringLength(255)]
        public string UserPrincipalName { get; set; }

        [StringLength(255)]
        public string DistinguishedName { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; }

        [StringLength(50)]
        public string Firstname { get; set; }

        [StringLength(50)]
        public string Middlename { get; set; }

        [StringLength(50)]
        public string Lastname { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Department { get; set; }

        public bool? IsResellerAdmin { get; set; }

        public bool? IsCompanyAdmin { get; set; }

        public bool? IsEnabled { get; set; }

        public int? MailboxPlan { get; set; }

        public int? TSPlan { get; set; }

        public int? LyncPlan { get; set; }

        public DateTime? Created { get; set; }

        public int? AdditionalMB { get; set; }

        public int? ActiveSyncPlan { get; set; }

        #region Not Mapped

        /// <summary>
        /// Property: description
        /// </summary>
        [NotMapped]
        public string Description { get; set; }

        /// <summary>
        /// Property: name
        /// </summary>
        [NotMapped]
        public string Name { get; set; }

        /// <summary>
        /// Property: streetAddress
        /// </summary>
        [NotMapped]
        public string Street { get; set; }

        /// <summary>
        /// Property: l
        /// </summary>
        [NotMapped]
        public string City { get; set; }

        /// <summary>
        /// Property: st
        /// </summary>
        [NotMapped]
        public string State { get; set; }

        /// <summary>
        /// Property: postalCode
        /// </summary>
        [NotMapped]
        public string PostalCode { get; set; }

        /// <summary>
        /// Property: postOfficeBox
        /// </summary>
        [NotMapped]
        public string POBox { get; set; }

        /// <summary>
        /// Property: co
        /// </summary>
        [NotMapped]
        public string Country { get; set; }

        /// <summary>
        /// Property: c
        /// </summary>
        [NotMapped]
        public string CountryCode { get; set; }

        /// <summary>
        /// Property: Company
        /// </summary>
        [NotMapped]
        public string Company { get; set; }

        /// <summary>
        /// Property: title
        /// </summary>
        [NotMapped]
        public string JobTitle { get; set; }

        /// <summary>
        /// Property: telephoneNumber
        /// </summary>
        [NotMapped]
        public string TelephoneNumber { get; set; }

        /// <summary>
        /// Property: facsimileTelephoneNumber
        /// </summary>
        [NotMapped]
        public string Fax { get; set; }

        /// <summary>
        /// Property: homePhone
        /// </summary>
        [NotMapped]
        public string HomePhone { get; set; }

        /// <summary>
        /// Property: mobile
        /// </summary>
        [NotMapped]
        public string MobilePhone { get; set; }

        /// <summary>
        /// Property: pager
        /// </summary>
        [NotMapped]
        public string Pager { get; set; }

        /// <summary>
        /// Property: ipPhone
        /// </summary>
        [NotMapped]
        public string IPPhone { get; set; }

        /// <summary>
        /// Property: physicalDeliveryOfficeName
        /// </summary>
        [NotMapped]
        public string Office { get; set; }

        /// <summary>
        /// Property: info
        /// </summary>
        [NotMapped]
        public string Notes { get; set; }

        /// <summary>
        /// Property: profilePath
        /// </summary>
        [NotMapped]
        public string ProfilePath { get; set; }

        /// <summary>
        /// Property: scriptPath
        /// </summary>
        [NotMapped]
        public string ScriptPath { get; set; }

        /// <summary>
        /// Property: wWWHomePAge
        /// </summary>
        [NotMapped]
        public string Webpage { get; set; }

        /// <summary>
        /// Property: badPwdCount
        /// </summary>
        [NotMapped]
        public int BadPwdCount { get; set; }

        /// <summary>
        /// Property: userAccountControl
        /// </summary>
        [NotMapped]
        public int UserAccountControl { get; set; }

        /// <summary>
        /// Property: sAMAccountType
        /// </summary>
        [NotMapped]
        public int SamAccountType { get; set; }

        /// <summary>
        /// Property: badPasswordTime
        /// </summary>
        [NotMapped]
        public long BadPasswordTime { get; set; }

        /// <summary>
        /// Property: pwdLastSet
        /// </summary>
        [NotMapped]
        public long PwdLastSet { get; set; }

        /// <summary>
        /// Property: accountExpires
        /// </summary>
        [NotMapped]
        public long AccountExpires { get; set; }

        /// <summary>
        /// Property: memberOf
        /// </summary>
        [NotMapped]
        public string[] MemberOf { get; set; }

        /// <summary>
        /// Property: thumbnailPhoto
        /// </summary>
        [NotMapped]
        public byte[] ImageFromAD { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public bool PwdNeverExpires { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [NotMapped]
        public bool ResetPwdNextLogin { get; set; }

        [NotMapped]
        public bool IsLyncEnabled
        {
            get { return (LyncPlan != null && LyncPlan > 0) ? true : false; }
        }

        [NotMapped]
        public bool IsEmailEnabled
        {
            get { return (MailboxPlan != null && MailboxPlan > 0) ? true : false; }
        }

        [NotMapped]
        public string EmailDomain
        {
            get
            {
                if (string.IsNullOrEmpty(Email))
                    return string.Empty;
                else
                    return Email.Split('@')[1];
            }
        }

        [NotMapped]
        public string EmailFirst
        {
            get
            {
                if (string.IsNullOrEmpty(Email))
                    return string.Empty;
                else
                    return Email.Split('@')[0];
            }
        }

        #endregion
    }
}
