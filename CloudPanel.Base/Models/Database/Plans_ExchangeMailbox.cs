namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Plans_ExchangeMailbox
    {
        [Key]
        public int MailboxPlanID { get; set; }

        [Required]
        [StringLength(50)]
        public string MailboxPlanName { get; set; }

        public int? ProductID { get; set; }

        [StringLength(255)]
        public string ResellerCode { get; set; }

        [StringLength(255)]
        public string CompanyCode { get; set; }

        public int MailboxSizeMB { get; set; }

        public int? MaxMailboxSizeMB { get; set; }

        public int MaxSendKB { get; set; }

        public int MaxReceiveKB { get; set; }

        public int MaxRecipients { get; set; }

        public bool EnablePOP3 { get; set; }

        public bool EnableIMAP { get; set; }

        public bool EnableOWA { get; set; }

        public bool EnableMAPI { get; set; }

        public bool EnableAS { get; set; }

        public bool EnableECP { get; set; }

        public int MaxKeepDeletedItems { get; set; }

        [Column(TypeName = "ntext")]
        public string MailboxPlanDesc { get; set; }

        public decimal Price { get; set; }

        public decimal Cost { get; set; }

        public decimal AdditionalGBPrice { get; set; }

        #region Not Mapped

        [NotMapped]
        public string CustomPrice { get; set; }

        [NotMapped]
        public int UserCount { get; set; }

        #endregion
    }
}
