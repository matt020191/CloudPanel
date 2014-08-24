using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public class CPStaticHelpers
    {
        public static string FormatBytes(long bytes)
        {
            if (bytes < 0)
                return "0 Bytes";
            else
            {
                var prefixOrder = new string[] { "TB", "GB", "MB", "KB", "Bytes" };
                var max = (long)Math.Pow(1024, prefixOrder.Length - 1);

                foreach (string p in prefixOrder)
                {
                    if (bytes > max)
                        return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), p);

                    max /= 1024;
                }

                return "0 Bytes";
            }
        }
    }
}