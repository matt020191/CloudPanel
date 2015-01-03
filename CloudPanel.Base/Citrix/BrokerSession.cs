using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Citrix
{
    public class BrokerSession
    {
        public Int64 Uid { get; set; }

        public string AgentVersion { get; set; }

        public string ApplicationInUse { get; set; }

        public Boolean AutonomouslyBrokered { get; set; }

        public Int32 BrokeringDuration { get; set; }

        public DateTime BrokeringTime { get; set; }

        public string BrokeringUserName { get; set; }

        public string BrokeringUserSID { get; set; }

        public string CatalogName { get; set; }

        public string ClientAddress { get; set; }

        public string ClientName { get; set; }

        public string ClientVersion { get; set; }

        public string ConnectedViaHostName { get; set; }

        public string ConnectedViaIP { get; set; }

        public string ControllerDNSName { get; set; }

        public string DesktopGroupName { get; set; }

        public Int32 DesktopGroupUid { get; set; }

        public string DesktopSID { get; set; }

        public Int32 DesktopUid { get; set; }

        public string DeviceId { get; set; }

        public string DNSName { get; set; }

        public Int32 EstablishmentDuration { get; set; }

        public DateTime EstablishmentTime { get; set; }

        public string HardwareId { get; set; }

        public Boolean Hidden { get; set; }

        public string HostedMachineName { get; set; }

        public string HostingServerName { get; set; }

        public string HypervisorConnectedName { get; set; }

        public Boolean ImageOutOfDate { get; set; }

        public Boolean InMaintenanceMode { get; set; }

        public string IPAddress { get; set; }

        public Boolean IsPhysical { get; set; }

        public string LaunchedViaHostName { get; set; }

        public string LaunchedViaIP { get; set; }

        public string MachineName { get; set; }

        public Int32 MachineUid { get; set; }

        public string Metadata { get; set; }

        public string OSType { get; set; }

        public string Protocol { get; set; }

        public Boolean SecureIcaActive { get; set; }

        /// <summary>
        /// Unique identifier Remote Desktop Services use but only on that machine
        /// </summary>
        public Int32 SessionId { get; set; }

        public Guid SessionKey { get; set; }

        public DateTime SessionStateChangeTime { get; set; }

        public DateTime StartTime { get; set; }

        public string UserFullName { get; set; }

        public string UserName { get; set; }

        public string UserSID { get; set; }

        public string UserUPN { get; set; }

        public Int32 ApplicationUid { get; set; }

        public Int32 SharedDesktopUid { get; set; }

        public string AdminAddress { get; set; }

        public string SessionState { get; set; }
    }
}
