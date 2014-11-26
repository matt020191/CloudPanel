using Nancy;
using Nancy.Security;
using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class LoginModule : NancyModule
    {
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
                    Guid? usersGuid = UserMapper.ValidateUser(username, password);
                    return this.LoginAndRedirect(usersGuid.Value, null, "~/dashboard");
                }
                catch (UnauthorizedAccessException)
                {
                    ViewBag.LoginError = "Login failed. Please try again.";
                    return View["login.cshtml"];
                }
                catch (Exception ex)
                {
                    ViewBag.LoginError = "Error: " + ex.Message;
                    return View["login.cshtml"];
                }
            };

            Get["/logout"] = _ =>
            {
                    return this.Logout("~/login");
            };
        }
    }
}