using Nancy;
using Nancy.ErrorHandling;
using Nancy.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Code
{
    public class UnauthorizedHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public UnauthorizedHandler(IViewFactory factory)
            : base(factory)
        {
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden);
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var response = RenderView(context, "Error/401");
            response.StatusCode = HttpStatusCode.Unauthorized;
            context.Response = response;
        }
    }
}