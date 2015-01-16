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

    public partial class UserActiveSyncDevices
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int UserID { get; set; }

        public Guid DeviceGuid { get; set; }

        public DateTime? FirstSyncTime { get; set; }

        public DateTime? LastPolicyUpdateTime { get; set; }

        public DateTime? LastSyncAttemptTime { get; set; }

        public DateTime? LastSuccessSync { get; set; }

        public DateTime? DeviceWipeSentTime { get; set; }

        public DateTime? DeviceWipeRequestTime { get; set; }

        public DateTime? DeviceWipeAckTime { get; set; }

        public int LastPingHeartbeat { get; set; }

        public string Identity { get; set; }

        public string DeviceType { get; set; }

        public string DeviceID { get; set; }

        public string DeviceUserAgent { get; set; }

        public string DeviceModel { get; set; }

        public string DeviceImei { get; set; }

        public string DeviceFriendlyName { get; set; }

        public string DeviceOS { get; set; }

        public string DevicePhoneNumber { get; set; }

        public string Status { get; set; }

        public string StatusNote { get; set; }

        public string DevicePolicyApplied { get; set; }

        public string DevicePolicyApplicationStatus { get; set; }

        public string DeviceActiveSyncVersion { get; set; }

        public string NumberOfFoldersSynced { get; set; }

        public virtual Users User { get; set; }

    }
}
