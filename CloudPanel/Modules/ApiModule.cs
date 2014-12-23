using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Authentication.Token;
using Nancy.Security;
using log4net;
using Nancy;

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

        }
    }
}