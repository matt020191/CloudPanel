using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using CloudPanel.Rollback;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.Responses.Negotiation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CloudPanel.Code;

namespace CloudPanel.Modules.CompanyModules.Exchange
{
    public class PublicFolderModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PublicFolderModule() : base("/company/{CompanyCode}/exchange/publicfolders")
        {
            Get["/"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangePublicFolders"));

                    #region Returns the public folder view
                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var company = db.Companies
                                          .Include(x => x.PublicFolderMailboxes)
                                          .Where(x => x.CompanyCode == companyCode)
                                          .Single();

                        if (company.PublicFolderMailboxes.Count > 0)
                        {
                            return Negotiate.WithModel(new { PlanID = company.PublicFolderMailboxes.First().PlanID })
                                            .WithView("Company/Exchange/publicfolders_modify.cshtml");
                        }
                        else
                        {
                            return Negotiate.WithView("Company/Exchange/publicfolders_enable.cshtml");
                        }
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
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "cExchangePublicFolders"));

                    #region Enables the company for public folders

                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    ADGroups groups = null;
                    dynamic powershell = null;

                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        #region Required values
                        if (!Request.Form.Username.HasValue)
                            throw new ArgumentNullException("Username");

                        if (!Request.Form.Domain.HasValue)
                            throw new ArgumentNullException("Domain");

                        if (!Request.Form.PlanID.HasValue)
                            throw new ArgumentNullException("PlanID");
                        #endregion

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
                            powershell = ExchPowershell.GetClass();
                            groups = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                            var newPFMailbox = new PublicFolderMailboxes();
                            newPFMailbox.CompanyID = sqlCompany.CompanyId;
                            newPFMailbox.PlanID = Request.Form.PlanID;
                            newPFMailbox.Identity = string.Format("{0}@{1}", username.Trim(), domain.Trim());

                            // Get the exchange path
                            var exchangePath = Settings.ExchangeOuPath(sqlCompany.DistinguishedName);

                            // Create the security groups
                            #region Create Security Groups
                            logger.DebugFormat("Creating new public folder admin group");
                            var adminGroup = powershell.New_SecurityGroup(new DistributionGroups()
                                                                     {
                                                                         DisplayName = "PublicFolderAdmins@" + companyCode,
                                                                         Email = "PublicFolderAdmins@" + domain,
                                                                         IsSecurityGroup = true
                                                                     }, exchangePath);
                            reverse.AddAction(Actions.CreateDistributionGroup, adminGroup.DistinguishedName);
                            powershell.Set_SecurityGroupCustomAttribute(adminGroup.Email, companyCode); // Set the custom attribute to the company code
                            groups.AddGroup(adminGroup.DisplayName, "Admins@" + companyCode); // Add PublicFolderAdmins@ to the Admins@ security group
                            db.DistributionGroups.Add(adminGroup); // Add the distribution group to the database with the IsSecurityGroup flag

                            logger.DebugFormat("Creating new public folder users group");
                            var userGroup = powershell.New_SecurityGroup(new DistributionGroups() 
                                                                     { 
                                                                         DisplayName = "PublicFolderUsers@" + companyCode,
                                                                         Email = "PublicFolderUsers@" + domain,
                                                                         IsSecurityGroup = true
                                                                     }, exchangePath);
                            reverse.AddAction(Actions.CreateDistributionGroup, userGroup.DistinguishedName);
                            powershell.Set_SecurityGroupCustomAttribute(userGroup.Email, companyCode); // Set the custom attribute to the company code
                            groups.AddGroup(userGroup.DisplayName, "AllUsers@" + companyCode); // Add PublicFolderUsers@ to the AllUsers@ security group
                            db.DistributionGroups.Add(userGroup); // Add the distribution group to the database with the IsSecurityGroup flag
                            #endregion

                            // Create the public folders
                            #region Create Public Folders
                            powershell.New_PublicFolderMailbox(newPFMailbox.Identity, newPFMailbox.Identity, exchangePath, newPFMailbox.Identity, string.Format(Settings.ExchangeABPName, companyCode));
                            reverse.AddAction(Actions.CreatePublicFolderMailbox, newPFMailbox.Identity);

                            logger.DebugFormat("Setting size on the public folder mailbox");
                            powershell.Set_PublicFolderMailbox(newPFMailbox.Identity, companyCode, sqlPlan);

                            logger.DebugFormat("Creating first public folder");
                            powershell.New_PublicFolder(companyCode, newPFMailbox.Identity, @"\");
                            reverse.AddAction(Actions.CreatePublicFolder, @"\" + companyCode);

                            logger.DebugFormat("Removing default permissions");
                            powershell.Remove_PublicFolderClientPermission(@"\" + companyCode, newPFMailbox.Identity, new[] { "Default" });

                            logger.DebugFormat("Adding public folder admin permissions");
                            var newPermissions = new Dictionary<string, string>();
                            newPermissions.Add("Owner", adminGroup.Email);
                            newPermissions.Add("Reviewer", userGroup.Email);

                            powershell.Add_PublicFolderClientPermission(@"\" + companyCode, newPFMailbox.Identity, newPermissions);
                            #endregion

                            // Add all mailbox users to the new public folder
                            logger.DebugFormat("Associating all mailbox users with the new public folder mailbox");
                            powershell.Set_DefaultPublicFolderMailbox(companyCode, newPFMailbox.Identity);

                            // Add to database
                            db.PublicFolderMailboxes.Add(newPFMailbox);
                            db.SaveChanges();
                        }

                        return Negotiate.WithModel(new { success = "Successfully enabled public folders" })
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
                        if (powershell != null)
                            powershell.Dispose();

                        if (groups != null)
                            groups.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/modify"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangePublicFolders"));

                    #region Modifies the public folders

                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    dynamic powershell = null;
                    try
                    {
                        #region Required values
                        if (!Request.Form.PlanID.HasValue)
                            throw new ArgumentNullException("PlanID");
                        #endregion

                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        // Form Values
                        int planId = Request.Form.PlanID;

                        var pfMailbox = db.Companies
                                          .Include(x => x.PublicFolderMailboxes)
                                          .Where(x => x.CompanyCode == companyCode)
                                          .Single()
                                          .PublicFolderMailboxes
                                          .First();

                        var sqlPlan = db.Plans_ExchangePublicFolder
                                        .Where(x => x.ID == planId)
                                        .Single();

                        powershell = ExchPowershell.GetClass();
                        logger.DebugFormat("Setting size on the public folder mailbox");
                        powershell.Set_PublicFolderMailbox(pfMailbox.Identity, companyCode, sqlPlan);

                        return Negotiate.WithModel(new { success = "Successfully updated public folder plan" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating public folder plan for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Post["/delete"] = _ =>
                {
                    this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "dExchangePublicFolders"));

                    #region Disables public folders for a company

                    string companyCode = _.CompanyCode;
                    CloudPanelContext db = null;
                    dynamic powershell = null;

                    ReverseActions reverse = new ReverseActions();
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var pfMailbox = db.Companies
                                          .Include(x => x.PublicFolderMailboxes)
                                          .Where(x => x.CompanyCode == companyCode)
                                          .Single()
                                          .PublicFolderMailboxes
                                          .First();

                        var sqlGroups = db.DistributionGroups
                                          .Where(x => x.CompanyCode == companyCode && x.IsSecurityGroup == true)
                                          .ToList();

                        powershell = ExchPowershell.GetClass();

                        // Delete the security groups
                        if (sqlGroups.Count > 0)
                        {
                            sqlGroups.ForEach(x =>
                            {
                                logger.DebugFormat("Removing security group {0}", x.DistinguishedName);
                                powershell.Remove_DistributionGroup(x.DistinguishedName);
                                db.DistributionGroups.Remove(x);
                            });
                        }

                        // Delete public folder mailbox
                        logger.DebugFormat("Removing public folders for {0}", companyCode);
                        powershell.Remove_PublicFolders(pfMailbox.Identity, companyCode);

                        logger.DebugFormat("Removing public folder mailbox {0}", pfMailbox.Identity);
                        powershell.Remove_PublicFolderMailbox(pfMailbox.Identity);
                        db.PublicFolderMailboxes.Remove(pfMailbox);

                        // Add to database
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully disabled public folders" })
                                        .WithStatusCode(HttpStatusCode.OK);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error disabling public folders for {0}: {1}", companyCode, ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (powershell != null)
                            powershell.Dispose();

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };
        }
    }
}