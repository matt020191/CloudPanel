namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Statistics
    {
        [Key]
        public int ID { get; set; }

        public DateTime Retrieved { get; set; }

        public int UserCount { get; set; }

        public int MailboxCount { get; set; }

        public int CitrixCount { get; set; }

        public string ResellerCode { get; set; }

        public string CompanyCode { get; set; }
    }
}