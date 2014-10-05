using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel
{
    public static class Extensions
    {
        public static bool Accept(this Request request, string contentType)
        {
            return request.Headers.Keys.Contains("Accept")
                && request.Headers["Accept"]
                            .Any(c => c.Contains(contentType));
        }
    }
}