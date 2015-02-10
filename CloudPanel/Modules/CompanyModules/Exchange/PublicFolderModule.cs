using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using CloudPanel.Rollback;
using System.Data.Entity;
using log4net;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class PublicFolderModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PublicFolderModule() : base("/company/{CompanyCode}/exchange/publicfolders")
        {
            Get["/"] = _ =>
                {
                    #region Returns the public folder view
                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var pfMailbox = db.Companies
                                          .Where(x => x.CompanyCode == companyCode)
                                          .FirstOrDefault();

                        if (pfMailbox == null)
                            return Negotiate.WithView("Company/Exchange/publicfolders_enable.cshtml");
                        else
                            return Negotiate.WithView("Company/Exchange/publicfolders_enable.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting public folder view for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/enable"] = _ =>
                {
                    #region Enables the company for public folders
                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        if (!Request.Form.Username.HasValue)
                            throw new ArgumentNullException("Username");

                        if (!Request.Form.Domain.HasValue)
                            throw new ArgumentNullException("Domain");

                        if (!Request.Form.PlanID.HasValue)
                            throw new ArgumentNullException("PlanID");

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Form Values
                        string username = Request.Form.Username.Value;
                        string domain = Request.Form.Domain.Value;
                        int planId = Request.Form.PlanID;

                        var sqlCompany = db.Companies.Include(x => x.PublicFolderMailboxes).Where(x => x.CompanyCode == companyCode).Single();
                        var sqlDomain = db.Domains.Where(x => x.Domain == domain).Where(x => x.CompanyCode == companyCode).Single();
                        var sqlPlan = db.Plans_ExchangePublicFolder.Where(x => x.ID == planId).Single();

                        if (sqlCompany.PublicFolderMailboxes == null || sqlCompany.PublicFolderMailboxes.Count == 0)
                        {
                            var newPFMailbox = new PublicFolderMailboxes();
                            newPFMailbox.CompanyID = sqlCompany.CompanyId;
                            newPFMailbox.PlanID = Request.Form.PlanID;
                            newPFMailbox.Identity = string.Format("{0}@{1}", username.Trim(), domain.Trim());

                            // Create the security groups
                            CreateSecurityGroups(sqlCompany.CompanyCode, sqlCompany.DistinguishedName, ref reverse);

                            // Create the public folders
                            CreatePublicFolders(sqlCompany.CompanyCode, sqlCompany.DistinguishedName, newPFMailbox.Identity, sqlPlan, ref reverse);

                            // Add to database
                            db.PublicFolderMailboxes.Add(newPFMailbox);
                            db.SaveChanges();
                        }

                        return Negotiate.WithModel(new { success = "Successfully enabled public folders" })
                                        .WithView("Company/Exchange/publicfolders_enable.cshtml")
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        reverse.RollbackNow();
                        logger.ErrorFormat("Error getting public folder view for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };
        }

        /// <summary>
        /// Creates the required security groups for public folders
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="companyDistinguishedName"></param>
        private void CreateSecurityGroups(string companyCode, string companyDistinguishedName, ref ReverseActions reverse)
        {
            ADGroups groups = null;
            try
            {
                groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                logger.DebugFormat("Generating Exchange path for {0}", companyDistinguishedName);
                var exchangeOUPath = Settings.ExchangeOuPath(companyDistinguishedName);

                logger.DebugFormat("Creating new public folder admin security group");
                var adminPfGroup = groups.Create(exchangeOUPath, new SecurityGroup()
                {
                    Name = "PublicFolderAdmins@" + companyCode,
                    DisplayName = "PublicFolderAdmins@" + companyCode,
                    SamAccountName = "PublicFolderAdmins_" + companyCode,
                    IsUniversalGroup = true
                });
                reverse.AddAction(Actions.CreateSecurityGroup, adminPfGroup.Name);
                groups.AddGroup(adminPfGroup.Name, "Admins@" + companyCode);

                logger.DebugFormat("Creating new public folder users security group");
                var userPfGroup = groups.Create(exchangeOUPath, new SecurityGroup()
                {
                    Name = "PublicFolderUsers@" + companyCode,
                    DisplayName = "PublicFolderUsers@" + companyCode,
                    SamAccountName = "PublicFolderUsers_" + companyCode,
                    IsUniversalGroup = true
                });
                reverse.AddAction(Actions.CreateSecurityGroup, userPfGroup.Name);
                groups.AddGroup(userPfGroup.Name, "AllUsers@" + companyCode);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error creating public folder secuity groups: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (groups != null)
                    groups.Dispose();
            }
        }

        /// <summary>
        /// Creates the public folder mailbox and parent public folder
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="companyDistinguishedName"></param>
        /// <param name="email"></param>
        /// <param name="plan"></param>
        /// <param name="reverse"></param>
        private void CreatePublicFolders(string companyCode, string companyDistinguishedName, string email, Plans_ExchangePublicFolders plan, ref ReverseActions reverse)
        {
            Exch2013 powershell = null;
            try
            {
                logger.DebugFormat("Generating Exchange path for {0}", companyDistinguishedName);
                var exchangeOUPath = Settings.ExchangeOuPath(companyDistinguishedName);

                powershell = ExchPowershell.GetClass();
                powershell.New_PublicFolderMailbox(email, email, exchangeOUPath, email, string.Format(Settings.ExchangeABPName, companyCode));
                reverse.AddAction(Actions.CreatePublicFolderMailbox, email);

                logger.DebugFormat("Setting size on the public folder mailbox");
                powershell.Set_PublicFolderMailbox(email, companyCode, plan);

                logger.DebugFormat("Creating first public folder");
                powershell.New_PublicFolder(companyCode, email, @"\");
                reverse.AddAction(Actions.CreatePublicFolder, @"\" + companyCode);

                logger.DebugFormat("Removing default permissions");
                powershell.Remove_PublicFolderClientPermission(@"\" + companyCode, email, new[] { "Default" });

                logger.DebugFormat("Adding public folder admin permissions");
                var newPermissions = new Dictionary<string, string>();
                newPermissions.Add("Owner", "PublicFolderAdmins@" + companyCode);
                newPermissions.Add("Reviewer", "PublicFolderUsers@"+companyCode);

                powershell.Add_PublicFolderClientPermission(@"\" + companyCode, email, newPermissions);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error creating public folders: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }
    }
}