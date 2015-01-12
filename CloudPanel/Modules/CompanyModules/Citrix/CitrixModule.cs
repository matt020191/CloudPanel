using CloudPanel.Base.Citrix;
using CloudPanel.Base.Config;
using CloudPanel.Citrix;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Citrix
{
    public class CitrixModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixModule() : base("/company/{CompanyCode}/citrix")
        {
            Get["/sessions", c => c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));

                    return View["Company/Citrix/sessions.cshtml"];
                };

            Get["/sessions", c => !c.Request.Accept("text/html")] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vCitrix"));

                    string companyCode = _.CompanyCode;

                    #region Gets sessions for all destop groups the user is assigned
                    CloudPanelContext db = null;
                    XenDesktop7 xd7 = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var users = db.Users
                                      .Where(x => x.CompanyCode == companyCode)
                                      .ToList();

                        var desktopGroups = db.CitrixDesktopGroup
                                              .Where(x =>
                                                     x.Companies.Any(c =>
                                                     c.CompanyCode == companyCode))
                                              .ToList();

                        var brokerSessions = new List<BrokerSession>();
                        xd7 = new XenDesktop7(Settings.CitrixUri, Settings.Username, Settings.DecryptedPassword);
                        desktopGroups.ForEach(x =>
                            {
                                var sessions = xd7.GetSessionsByDesktopGroup(x.Uid);
                                if (sessions != null)
                                {
                                    sessions.ForEach(s =>
                                        {
                                            //if (users.Any(u => u.UserPrincipalName.Equals(s.UserUPN, StringComparison.CurrentCultureIgnoreCase)))
                                            //{
                                                brokerSessions.Add(s);
                                            //}
                                        });
                                }
                            });

                        return Negotiate.WithModel(new { sessions = brokerSessions })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting sessions for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (xd7 != null)
                            xd7.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };
        }
    }
}