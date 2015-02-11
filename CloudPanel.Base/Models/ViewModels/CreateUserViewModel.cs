using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Models.ViewModels
{
    public class CreateUserViewModel
    {
        public string DisplayName { get; set; }

        public string Department { get; set; }

        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Middlename { get; set; }

        public string Lastname { get; set; }

        public string Pwd { get; set; }

        public int DomainID { get; set; }

        public bool ChangePasswordNextLogin { get; set; }
    }
}
