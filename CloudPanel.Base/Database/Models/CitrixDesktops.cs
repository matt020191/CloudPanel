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

    public partial class CitrixDesktops
    {
        [Key]
        public int DesktopID { get; set; }

        [Required]
        public int Uid { get; set; }

        [Required]
        public int DesktopGroupID { get; set; }

        [Required]
        public string SID { get; set; }

        [Required]
        public string AgentVersion { get; set; }

        public int CatalogUid { get; set; }

        public string CatalogName { get; set; }

        public string DNSName { get; set; }

        public string MachineName { get; set;  }

        public int MachineUid { get; set; }

        public string OSType { get; set; }

        public string OSVersion { get; set; }

        public string IPAddress { get; set; }

        public bool InMaintenanceMode { get; set; }

        public DateTime LastRetrieved { get; set; }

        public virtual CitrixDesktopGroups DesktopGroup { get; set; }
    }
}
