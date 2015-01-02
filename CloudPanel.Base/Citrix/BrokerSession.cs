using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Citrix
{
    public class BrokerSession
    {
        public Int64 Uid { get; set; }

        public string UserFullName { get; set; }

        public string UserName { get; set; }

        public string UserSID { get; set; }

        public string UserUPN { get; set; }

        public string UntrustedUserName { get; set; }

        public int? BrokeringDuration { get; set; }

        public DateTime? BrokeringTime { get; set; }

        public string BrokeringUserName { get; set; }

        public string BrokeringUserSID { get; set; }

        public string ClientName { get; set; }

        public string ClientAddress { get; set; }

        public string ClientPlatform { get; set; }

        public string ClientVersion { get; set; }

        public string ConnectedViaHostName { get; set; }

        public string SessionState { get; set; }

        public DateTime StartTime { get; set; }

        public Guid SessionKey { get; set; }
    }
}
