using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Base.Models.ViewModels;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules
{
    public class ImportModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ImportModule));

        public ImportModule(): base("/company/{CompanyCode}/import")
        {
            Get["/users"] = _ =>
                {
                    #region Get a list of users not in the database
                    string companyCode = _.CompanyCode;

                    ADOrganizationalUnits org = null;
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Querying a list of users for import for {0}...", companyCode);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Getting company from database: {0}", companyCode);
                        var company = (from d in db.Companies
                                       where d.CompanyCode == companyCode
                                       select d).Single();

                        logger.DebugFormat("Getting current users from database: {0}", companyCode);
                        var sqlUsers = (from d in db.Users
                                     where d.CompanyCode == companyCode
                                     select d.UserGuid).ToList();

                        logger.DebugFormat("Getting users from database for {0}", companyCode);
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                        var users = org.GetUsers(company.DistinguishedName);
                        users = users.Where(x => sqlUsers.Contains(x.UserGuid)).ToList();

                        logger.DebugFormat("Found a total of {0} users", users.Count);
                        return Negotiate.WithModel(new { users = users })
                                        .WithView("Admin/import_users.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error querying a list of users for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();

                        if (org != null)
                            org.Dispose();
                    }
                    #endregion
                };

            Post["/users"] = _ =>
                {
                    #region Imports a list of users
                    string companyCode = _.CompanyCode;

                    ADUsers users = null;
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Importing users for {0}", companyCode);
                        var checkedUsers = this.Bind<List<ImportUserViewModel>>();

                        logger.DebugFormat("Importing {0} user(s) for {1}", checkedUsers.Count, companyCode);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        logger.DebugFormat("Checking if company {0} is enabled for Exchange", companyCode);
                        var isExchangeEnabled = CloudPanel.CPStaticHelpers.IsExchangeEnabled(companyCode);

                        users = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        foreach (var u in checkedUsers)
                        {
                            logger.DebugFormat("Preparing to import user {0}", u.UserPrincipalName);
                            var tmpADUser = users.GetUserWithoutGroups(u.UserGuid.ToString());
                            tmpADUser.UserGuid = u.UserGuid;
                            tmpADUser.CompanyCode = companyCode;
                            tmpADUser.MailboxPlan = isExchangeEnabled ? u.MailboxPlan : 0;

                            db.Users.Add(tmpADUser);
                        }

                        db.SaveChanges();

                        string redirect = string.Format("~/company/{0}/import/users", companyCode);
                        return Negotiate.WithModel(new { success = "Successfully imported users" })
                                        .WithMediaRangeResponse(new MediaRange("text/html"), Response.AsRedirect(redirect));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error importing a list of users for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();

                        if (users != null)
                            users.Dispose();
                    }
                    #endregion
                };
        }
    }
}