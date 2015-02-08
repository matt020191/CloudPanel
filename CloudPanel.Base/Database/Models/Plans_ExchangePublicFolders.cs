namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Plans_ExchangePublicFolders
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int MailboxSizeMB { get; set; }

        public string CompanyCode { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal Cost { get; set; }

        public virtual ICollection<PublicFolderMailboxes> PublicFolderMailboxes { get; set; }
    }
}
