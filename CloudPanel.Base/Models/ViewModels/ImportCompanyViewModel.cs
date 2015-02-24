using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Models.ViewModels
{
    public class ImportCompanyViewModel
    {
        public bool IsChecked { get; set; }
        public bool IsExchangeEnabled { get; set; }
        public int CompanyPlan { get; set; }
        public string CompanyCode { get; set; }
        public string DistinguishedName { get; set; }
    }
}
