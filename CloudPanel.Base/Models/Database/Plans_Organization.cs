namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Plans_Organization
    {
        [Key]
        public int OrgPlanID { get; set; }

        [Required]
        [StringLength(50)]
        public string OrgPlanName { get; set; }

        public int ProductID { get; set; }

        [StringLength(255)]
        public string ResellerCode { get; set; }

        public int MaxUsers { get; set; }

        public int MaxDomains { get; set; }

        public int MaxExchangeMailboxes { get; set; }

        public int MaxExchangeContacts { get; set; }

        public int MaxExchangeDistLists { get; set; }

        public int MaxExchangePublicFolders { get; set; }

        public int MaxExchangeMailPublicFolders { get; set; }

        public int MaxExchangeKeepDeletedItems { get; set; }

        public int? MaxExchangeActivesyncPolicies { get; set; }

        public int MaxTerminalServerUsers { get; set; }

        public int? MaxExchangeResourceMailboxes { get; set; }
    }
}
