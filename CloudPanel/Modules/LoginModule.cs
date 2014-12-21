using Nancy;
using Nancy.Security;
using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace CloudPanel.Modules
{
    public class LoginModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger("Default");

        public LoginModule()
        {
            Get["/"] = _ =>
                {
                    return View["login.cshtml"];
                };

            Get["/login"] = _ =>
                {
                    return View["login.cshtml"];
                };

            Post["/login"] = _ =>
            {
                var username = this.Request.Form.username;
                var password = this.Request.Form.password;

                try
                {
                    AuthenticatedUser identity = UserMapper.ValidateUser(username, password);
                    if (identity == null)
                    {
                        ViewBag.LoginError = "Login failed. Please try again.";
                        return View["login.cshtml"];
                    }
                    else
                    {
                        return this.LoginAndRedirect(identity.UserGuid, null, "~/dashboard");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.LoginError = "Error: " + ex.Message;
                    return View["login.cshtml"];
                }
            };

            Get["/logout"] = _ =>
            {
                return this.LogoutAndRedirect("~/login");
            };
        }
    }
}