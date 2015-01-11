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

    public partial class CitrixApplications
    {
        [Key]
        public int ApplicationID { get; set; }

        [Required]
        public int Uid { get; set; }

        [Required]
        public Guid UUID { get; set; }

        public string Name { get; set; }

        public string PublishedName { get; set; }

        public string ApplicationName { get; set; }

        public string Description { get; set; }

        public string SecurityGroup { get; set; }

        public string CommandLineExecutable { get; set; }

        public string CommandLineArguments { get; set; }

        public bool ShortcutAddedToDesktop { get; set; }

        public bool ShortcutAddedToStartMenu { get; set; }

        public bool IsEnabled { get; set; }

        public bool UserFilterEnabled { get; set; }

        public DateTime LastRetrieved { get; set; }

        public virtual ICollection<CitrixDesktopGroups> DesktopGroups { get; set; }

        public virtual ICollection<Users> Users { get; set; }
    }
}
