using Nancy;
using Nancy.ErrorHandling;
using Nancy.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Code
{
    public class InternalErrorHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public InternalErrorHandler(IViewFactory factory) : base(factory)
        {
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var response = RenderView(context, "Error/500");
            response.StatusCode = HttpStatusCode.InternalServerError;
            context.Response = response;
        }
    }
}