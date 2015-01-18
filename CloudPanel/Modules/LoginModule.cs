using Nancy;
using Nancy.Security;
using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;

namespace CloudPanel.Modules
{
    public class LoginModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                        if (identity.Claims.Contains("SuperAdmin") || identity.Claims.Contains("ResellerAdmin"))
                            return this.LoginAndRedirect(identity.UserGuid, null, "~/dashboard");
                        else
                            return this.LoginAndRedirect(identity.UserGuid, null, string.Format("~/company/{0}/overview", identity.CompanyCode));
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.LoginError = "Error: " + ex.Message;
                    return View["login.cshtml"];
                }
            };

            Post["/login/simple"] = _ =>
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

            Get["/logout"] = _ =>
            {
                return this.LogoutAndRedirect("~/login");
            };
        }
    }
}