using log4net;
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
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof(ErrorResponseHandler));

        public ErrorResponseHandler(IViewFactory factory) : base(factory)
        {
        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            logger.DebugFormat("Response Handler status code {0}", statusCode);

            return (statusCode == HttpStatusCode.InternalServerError ||
                    statusCode == HttpStatusCode.Unauthorized ||
                    statusCode == HttpStatusCode.Forbidden ||
                    statusCode == HttpStatusCode.NotFound) && context.Request.Accept("text/html");
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var wantsHtml = CPStaticHelpers.ClientWantsHtml(context);
            if (!wantsHtml)
                return;
            else
            {
                logger.DebugFormat("Client wanted HTML back");
                switch (statusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        if (context.Items.ContainsKey(NancyEngine.ERROR_EXCEPTION)) {
                            var exception = context.Items[NancyEngine.ERROR_EXCEPTION] as Exception;
                            context.Response = RenderView(context, "Error/500", exception);
                        } 
                        else {
                            context.Response = RenderView(context, "Error/500", context.NegotiationContext.DefaultModel);
                        }
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