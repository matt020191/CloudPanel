using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base
{
    public class DynamicHandlers
    {
        public static object GetPropValue(object target, string propName)
        {
            return target.GetType().GetProperty(propName).GetValue(target, null);
        }
    }
}
