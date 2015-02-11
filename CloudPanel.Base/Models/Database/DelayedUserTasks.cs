namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class DelayedUserTasks
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TaskID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int Status { get; set; }

        [Column(TypeName="ntext")]
        public string LastMessage { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime DelayedUntil { get; set; }

        public DateTime? LastUpdated { get; set; }

        [ForeignKey("UserID")]
        public virtual Users User { get; set; }
    }
}
