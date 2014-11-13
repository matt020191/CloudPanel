namespace CloudPanel.Base.Database.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RoleID { get; set; }

        public string CompanyCode { get; set; }

        #region View Permissions

        public bool vDomains { get; set; }

        public bool vUsers { get; set; }

        public bool vUsersEdit { get; set; }

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

        public bool eExchangeContacts { get; set; }

        public bool eExchangeGroups { get; set; }

        public bool eExchangeResources { get; set; }

        public bool eExchangePublicFolders { get; set; }

        public bool eCitrix { get; set; }

        public bool eLync { get; set; }

        #endregion

        #region Delete Permissions

        public bool dDomains { get; set; }

        public bool dUsers { get; set; }

        public bool dExchangeContacts { get; set; }

        public bool dExchangeGroups { get; set; }

        public bool dExchangeResources { get; set; }

        public bool dExchangePublicFolders { get; set; }

        public bool dCitrix { get; set; }

        public bool dLync { get; set; }

        #endregion
    }
}
