﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public class RazorConfig
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return "CloudPanel.Base";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return "Nancy.Validation";
            yield return "System.Globalization";
            yield return "System.Collections.Generic";
            yield return "System.Linq";
            yield return "CloudPanel_3";
            yield return "CloudPanel";
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }
}