namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AuditTrace
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Username { get; set; }

        public string IPAddress { get; set; }

        public string Method { get; set; }

        public string Route { get; set; }

        public string Parameters { get; set; }
    }
}
