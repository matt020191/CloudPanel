using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Extensions
{
    public static class ExtensionMethods
    {
        public static string GetPercentageAsInvariantString(this int x, double percentage)
        {
            return (x * 0.90).ToString("#.##", CultureInfo.InvariantCulture);
        }

        public static string GetNumberAsInvariantString(this int x)
        {
            return x.ToString("#.##", CultureInfo.InvariantCulture);
        }
    }
}
