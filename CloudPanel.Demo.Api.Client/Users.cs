using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Demo.Api.Client
{
    public class Users
    {
        public int draw { get; set; }

        public int recordsTotal { get; set; }

        public int recordsFiltered { get; set; }

        public List<UserObject> data { get; set; }
    }

    public class UserObject
    {
        public Guid UserGuid {get;set; }

        public string CompanyCode {get ;set; }

        public string DisplayName {get ;set; }
    }
}
