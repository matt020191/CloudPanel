namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PublicFolderMailboxes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MailboxID { get; set; }

        public int CompanyID { get; set; }

        public int PlanID { get; set; }

        public string Identity { get; set; }

        [ForeignKey("CompanyID")]
        public virtual Companies Company { get; set; }

        [ForeignKey("PlanID")]
        public virtual Plans_ExchangePublicFolders Plan { get; set; }
    }
}
