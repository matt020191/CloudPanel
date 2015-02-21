namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class StatMessageTrackingCount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        [Required]
        public int TotalSent { get; set; }

        [Required]
        public int TotalReceived { get; set; }

        [Required]
        public long TotalBytesSent { get; set; }

        [Required]
        public long TotalBytesReceived { get; set; }

        public virtual Users User { get; set; }
    }
}
