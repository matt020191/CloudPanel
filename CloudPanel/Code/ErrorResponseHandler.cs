using Nancy;
using Nancy.ErrorHandling;
using Nancy.Responses.Negotiation;
using Nancy.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Code
{
    public class ErrorResponseHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public ErrorResponseHandler(IViewFactory factory) : base(factory)
        {
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.InternalServerError ||
                   statusCode == HttpStatusCode.Unauthorized ||
                   statusCode == HttpStatusCode.Forbidden ||
                   statusCode == HttpStatusCode.NotFound;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var wantsHtml = CPStaticHelpers.ClientWantsHtml(context);
            if (!wantsHtml)
                return;
            else
            {
                switch (statusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        var exception = context.Items[NancyEngine.ERROR_EXCEPTION] as Exception;
                        context.Response = RenderView(context, "Error/500", exception); 
                        break;
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.Unauthorized:
                        context.Response = RenderView(context, "Error/401");
                        break;
                    case HttpStatusCode.NotFound:
                        context.Response = RenderView(context, "Error/404");
                        break;
                    default:
                        return;
                }

                context.Response.StatusCode = statusCode;
            }
        }
    }
}