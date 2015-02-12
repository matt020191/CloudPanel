using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Reports.Citrix
{
    public class CitrixDetails
    {
        private List<CitrixAppsData> e_appsdata;
        private List<CitrixDesktopGroupData> e_desktopgroupdata;

        public CitrixDetails()
        {

        }

        public CitrixDetails(List<CitrixDesktopGroupData> desktopGroupData, List<CitrixAppsData> appsData)
        {
            e_desktopgroupdata = desktopGroupData;
            e_appsdata = appsData;
        }

        public List<CitrixDesktopGroupData> GetDesktopGroupData()
        {
            return e_desktopgroupdata;
        }

        public List<CitrixAppsData> GetAppData()
        {
            return e_appsdata;
        }
    }

    public class CitrixAppsData
    {
        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        public string DesktopGroupName { get; set; }

        public Guid UserGuid { get; set; }

        public string UserPrincipalName { get; set; }

        public string UserDisplayName { get; set; }

        public string ApplicationName { get; set; }

        public string Cost { get; set; }

        public string Price { get; set; }
    }

    public class CitrixDesktopGroupData
    {
        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        public Guid UserGuid { get; set; }

        public string UserPrincipalName { get; set; }

        public string UserDisplayName { get; set; }

        public string DesktopGroupName { get; set; }

        public string Cost { get; set; }

        public string Price { get; set; }
    }
}
