namespace CloudPanel.Base.Database.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserRoles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }

        public string CompanyCode { get; set; }

        public string DisplayName { get; set; }

        #region Create Permissions

        public bool cDomains { get; set; }

        public bool cUsers { get; set; }

        public bool cExchangeActiveSyncPlans { get; set; }

        public bool cExchangeContacts { get; set; }

        public bool cExchangeGroups { get; set; }

        public bool cExchangeResources { get; set; }

        public bool cExchangePublicFolders { get; set; }

        public bool cCitrix { get; set; }

        public bool cLync { get; set; }

        #endregion

        #region View Permissions

        public bool vDomains { get; set; }

        public bool vUsers { get; set; }

        public bool vUsersEdit { get; set; }

        public bool vExchangeActiveSyncPlans { get; set; }

        public bool vExchangeContacts { get; set; }

        public bool vExchangeGroups { get; set; }

        public bool vExchangeResources { get; set; }

        public bool vExchangePublicFolders { get; set; }

        public bool vCitrix { get; set; }

        public bool vLync { get; set; }

        #endregion

        #region Edit Permissions

        public bool eDomains { get; set; }

        public bool eUsers { get; set; }

        public bool eExchangeActiveSyncPlans { get; set; }

        public bool eExchangeContacts { get; set; }

        public bool eExchangeGroups { get; set; }

        public bool eExchangeResources { get; set; }

        public bool eExchangePublicFolders { get; set; }

        public bool eCitrix { get; set; }

        public bool eLync { get; set; }

        public bool ePermissions { get; set; }

        #endregion

        #region Delete Permissions

        public bool dDomains { get; set; }

        public bool dUsers { get; set; }

        public bool dExchangeActiveSyncPlans { get; set; }

        public bool dExchangeContacts { get; set; }

        public bool dExchangeGroups { get; set; }

        public bool dExchangeResources { get; set; }

        public bool dExchangePublicFolders { get; set; }

        public bool dCitrix { get; set; }

        public bool dLync { get; set; }

        public bool dPermissions { get; set; }

        #endregion
    }
}
