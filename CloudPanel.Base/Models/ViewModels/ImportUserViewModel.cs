using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Models.ViewModels
{
    public class ImportUserViewModel
    {
        public bool IsChecked { get; set; }
        public Guid UserGuid { get; set; }
        public string UserPrincipalName { get; set; }
        public string Email { get; set; }
        public int MailboxPlan { get; set; }
    }
}
