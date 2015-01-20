using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Authentication.Token;
using Nancy.Security;
using log4net;
using Nancy;
using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;

namespace CloudPanel.Modules
{
    public class ApiModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ApiModule(Nancy.Authentication.Token.ITokenizer tokenizer) : base("/login")
        {
            Post["/token"] = _ =>
            {
                var username = this.Request.Form.username;
                var password = this.Request.Form.password;

                try
                {
                    IUserIdentity identity = UserMapper.ValidateUser(username, password);
                    var token = tokenizer.Tokenize(identity, Context);

                    return new { Token = token };
                }
                catch (UnauthorizedAccessException)
                {
                    return HttpStatusCode.Unauthorized;
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error performing api login: {0}", ex.ToString());
                    return HttpStatusCode.InternalServerError;
                }
            };

            Post["/stateless"] = _ =>
            {
                var username = this.Request.Form.username;
                var password = this.Request.Form.password;

                ADUsers ad = null;
                try
                {
                    ad = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                    bool isValid = ad.AuthenticateSimple(username, password);
                    if (isValid)
                    {
                        return Negotiate.WithModel(new { success = "Successfully authenticated user." })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    else
                    {
                        return Negotiate.WithModel(new { error = "Failed authentication." })
                                        .WithStatusCode(HttpStatusCode.Unauthorized);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error authenticating user {0}: {1}", username, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
            };
        }
    }
}