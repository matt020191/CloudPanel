using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CitrixDesktopGroups
    {
        [Key]
        public int DesktopGroupID { get; set; }

        [Required]
        public int Uid { get; set; }

        [Required]
        public Guid UUID { get; set; }

        [Required]
        public string Name { get; set; }

        public string PublishedName { get; set; }

        public string Description { get; set; }

        public bool IsEnabled { get; set; }

        public DateTime LastRetrieved { get; set; }

        public virtual ICollection<CitrixDesktops> Desktops { get; set; }

        public virtual ICollection<CitrixApplications> Applications { get; set; }

        public virtual ICollection<Users> Users { get; set; }

        public int ApplicationId { get; set; }

        public int DesktopId { get; set; }
        #region Not Mapped

        [NotMapped]
        public int CurrentSessions { get; set; }

        #endregion
    }
}
