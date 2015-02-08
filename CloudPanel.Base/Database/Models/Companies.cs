namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Companies
    {
        [Key]
        public int CompanyId { get; set; }

        public bool IsReseller { get; set; }

        [StringLength(255)]
        public string ResellerCode { get; set; }

        public int? OrgPlanID { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyCode { get; set; }

        [Required]
        [StringLength(255)]
        public string Street { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(50)]
        public string ZipCode { get; set; }

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        public string Website { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string AdminName { get; set; }

        [Required]
        [StringLength(255)]
        public string AdminEmail { get; set; }

        [Required]
        [StringLength(255)]
        public string DistinguishedName { get; set; }

        public DateTime Created { get; set; }

        public bool ExchEnabled { get; set; }

        public bool? LyncEnabled { get; set; }

        public bool? CitrixEnabled { get; set; }

        public int? ExchPFPlan { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        public bool? ExchPermFixed { get; set; }

        public ICollection<CitrixDesktopGroups> CitrixDesktopGroups { get; set; }

        public ICollection<PublicFolderMailboxes> PublicFolderMailboxes { get; set; }

        #region Not Mapped

        [NotMapped]
        public string FullAddressFormatted
        {
            get
            {
                return string.Format("{0}<br/>{1} {2}  {3}", Street, City, State, ZipCode);
            }

        }

        [NotMapped]
        public int TotalUsers { get; set; }

        #endregion
    }
}
