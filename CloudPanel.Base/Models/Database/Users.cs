namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Users
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public Guid UserGuid { get; set; }

        [Required]
        public string CompanyCode { get; set; }

        [Required]
        public string sAMAccountName { get; set; }

        [Required]
        public string UserPrincipalName { get; set; }

        [Required]
        public string DistinguishedName { get; set; }

        [Required]
        public string DisplayName { get; set; }

        public string Firstname { get; set; }

        public string Middlename { get; set; }

        public string Lastname { get; set; }

        public string Email { get; set; }

        public string Department { get; set; }

        public string Skype { get; set; }

        public string Facebook { get; set; }

        public string Twitter { get; set; }

        public string Dribbble { get; set; }

        public string Tumblr { get; set; }

        public string LinkedIn { get; set; }

        /// <summary>
        /// If the user is a reseller admin. Only super admins should be able to modify this value
        /// </summary>
        public bool? IsResellerAdmin { get; set; }

        /// <summary>
        /// Is the user is a company admin and can manage their company (should have permissions associated with their account
        /// </summary>
        public bool? IsCompanyAdmin { get; set; }

        /// <summary>
        /// If the user is enabled or disabled
        /// </summary>
        public bool? IsEnabled { get; set; }

        #region Plans

        /// <summary>
        /// Exchange mailbox plan the user is associated with
        /// </summary>
        public int? MailboxPlan { get; set; }

        /// <summary>
        /// Exchange archive plan the user is associated with
        /// </summary>
        public int? ArchivePlan { get; set; }

        /// <summary>
        /// CURRENTLY NOT USED
        /// </summary>
        public int? TSPlan { get; set; }

        /// <summary>
        /// CURRENTLY NOT USED
        /// </summary>
        public int? LyncPlan { get; set; }

        /// <summary>
        /// Activesync plan the user is associated with
        /// </summary>
        public int? ActiveSyncPlan { get; set; }

        #endregion

        /// <summary>
        /// When the user was created
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// How many additional MB that was added to the mailbox plan
        /// </summary>
        public int? AdditionalMB { get; set; }

        /// <summary>
        /// Security role the user belongs to
        /// </summary>
        public int? RoleID { get; set; }

        public virtual ApiKeys ApiKey { get; set; }

        [ForeignKey("RoleID")]
        public virtual UserRoles Role { get; set; }

        public virtual ICollection<StatMessageTrackingCount> UserMessageTrackingCount { get; set; }

        public virtual ICollection<CitrixDesktopGroups> CitrixDesktopGroups { get; set; }

        public virtual ICollection<CitrixApplications> CitrixApplications { get; set; }

        public virtual ICollection<UserActiveSyncDevices> ActiveSyncDevices { get; set; }

        public virtual ICollection<DelayedUserTasks> DelayedUserTasks { get; set; }

        #region AD Settings

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
        public string Street { get; set; }

        /// <summary>
        /// Property: l
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Property: st
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Property: postalCode
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Property: postOfficeBox
        /// </summary>
        [NotMapped]
        public string POBox { get; set; }

        /// <summary>
        /// Property: co
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Property: c
        /// </summary>
        [NotMapped]
        public string CountryCode { get; set; }

        /// <summary>
        /// Property: Company
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Property: title
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Property: telephoneNumber
        /// </summary>
        public string TelephoneNumber { get; set; }

        /// <summary>
        /// Property: facsimileTelephoneNumber
        /// </summary>
        public string Fax { get; set; }

        /// <summary>
        /// Property: homePhone
        /// </summary>
        public string HomePhone { get; set; }

        /// <summary>
        /// Property: mobile
        /// </summary>
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

        [NotMapped]
        public bool IsLyncEnabled
        {
            get { return (LyncPlan != null && LyncPlan > 0) ? true : false; }
        }

        [NotMapped]
        public bool ChangePasswordNextLogin { get; set; }

        #endregion

        #region Exchange Settings

        [NotMapped]
        public bool IsEmailEnabled
        {
            get { return (MailboxPlan != null && MailboxPlan > 0) ? true : false; }
        }

        [NotMapped]
        public bool IsArchivingEnabled
        {
            get { return (MailboxPlan != null && MailboxPlan > 0 && ArchivePlan > 0) ? true : false; }
        }

        [NotMapped]
        public string ArchiveName { get; set; }

        [NotMapped]
        public bool ArchivingEnabled { get; set; }

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

        [NotMapped]
        public int SizeInMB { get; set; }

        [NotMapped]
        public int DomainID { get; set; }

        [NotMapped]
        public string[] EmailAddressesProcessed { get; set; }

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
        public string[] EmailSendOnBehalf { get; set; }

        [NotMapped]
        public bool LitigationHoldEnabled { get; set; }

        [NotMapped]
        public string LitigationHoldDate { get; set; }

        [NotMapped]
        public string LitigationHoldOwner { get; set; }

        [NotMapped]
        public string RetentionComment { get; set; }

        [NotMapped]
        public string RetentionUrl { get; set; }

        [NotMapped]
        public string ForwardTo { get; set; }

        [NotMapped]
        public string CanoncialName { get; set; }

        [NotMapped]
        public string MailboxDatabase { get; set; }

        [NotMapped]
        public bool AutoMapping { get; set; }

        [NotMapped]
        public bool DeliverToMailboxAndForward { get; set; }

        [NotMapped] // Used on reports
        public long MailboxSizeInBytes { get; set; }

        [NotMapped] // Used on reports
        public long ArchiveSizeInBytes { get; set; }

        [NotMapped]
        public Guid msExchMailboxGuid { get; set; }

        #endregion

        #region Modified notifications

        [NotMapped]
        public bool IsMailboxEnabled { get; set; }

        [NotMapped]
        public bool IsEmailModified { get; set; }

        [NotMapped]
        public bool IsLitigationHoldModified { get; set; }

        [NotMapped]
        public bool IsArchivingModified { get; set; }

        #endregion
    }
}
