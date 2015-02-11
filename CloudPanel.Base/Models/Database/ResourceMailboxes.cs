namespace CloudPanel.Base.Models.Database
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

        public Guid ResourceGuid { get; set; }

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

        [NotMapped]
        public string CanonicalName { get; set; }

        #endregion
    }
}
