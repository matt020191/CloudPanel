namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public partial class StatMailboxSizes
    {
        public int ID { get; set; }

        [Required]
        public Guid UserGuid { get; set; }

        [Required]
        [StringLength(64)]
        public string UserPrincipalName { get; set; }

        [Required]
        [StringLength(255)]
        public string MailboxDatabase { get; set; }

        [Required]
        [StringLength(255)]
        public string TotalItemSize { get; set; }

        [Required]
        public long TotalItemSizeInBytes { get; set; }

        [Required]
        [StringLength(255)]
        public string TotalDeletedItemSize { get; set; }

        [Required]
        public long TotalDeletedItemSizeInBytes { get; set; }

        public int ItemCount { get; set; }

        public int DeletedItemCount { get; set; }

        public DateTime Retrieved { get; set; }
    }
}