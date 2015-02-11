
namespace CloudPanel.Base.Models.Database
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class CitrixSecurityGroups
    {
        [Key]
        public int GroupID { get; set; }

        [Required]
        public string GroupName { get; set; }

        public string Description { get; set; }

        [Required]
        public string CompanyCode { get; set; }

        [Required]
        public int DesktopGroupID { get; set; }

        public int? ApplicationID { get; set; }

        [ForeignKey("DesktopGroupID")]
        public virtual CitrixDesktopGroups DesktopGroup { get; set; }
    }
}
