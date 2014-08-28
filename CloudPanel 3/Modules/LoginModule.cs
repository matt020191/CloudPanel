﻿using Nancy;
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

                Guid? usersGuid = UserMapper.ValidateUser(username, password);
                if (usersGuid == null)
                {
                    ViewBag.LoginError = "Invalid username or password.";
                    return View["login.cshtml"];
                }
                else
                {
                    return this.LoginAndRedirect(usersGuid.Value, null, "/dashboard");
                }
            };

            Get["/logout"] = _ =>
                {
                    return this.Logout("~/login");
                };
        }
    }
}