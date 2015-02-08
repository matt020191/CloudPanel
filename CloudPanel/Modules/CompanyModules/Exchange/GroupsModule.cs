using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Enums;
using CloudPanel.Base.Exchange;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CloudPanel.Modules
{
    public class GroupsModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GroupsModule() : base("/company/{CompanyCode}/exchange/groups")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeGroups"));

                #region Returns the groups view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    var groups = (from d in db.DistributionGroups
                                  where d.CompanyCode == companyCode
                                  orderby d.DisplayName
                                  select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = groups.Count, recordsFiltered = groups.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    // Check for dataTables and process the values
                    if (Request.Query.draw.HasValue)
                    {
                        draw = Request.Query.draw;
                        start = Request.Query.start;
                        length = Request.Query.length;
                        orderColumn = Request.Query["order[0][column]"];
                        searchValue = Request.Query["search[value]"].HasValue ? Request.Query["search[value]"] : string.Empty;
                        isAscendingOrder = Request.Query["order[0][dir]"] == "asc" ? true : false;
                        orderColumnName = Request.Query["columns[" + orderColumn + "][data]"];

                        // See if we are using dataTables to search
                        if (!string.IsNullOrEmpty(searchValue))
                        {
                            groups = (from d in groups
                                       where d.DisplayName.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 || 
                                             d.Email.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1
                                       select d).ToList();
                            recordsFiltered = groups.Count;
                        }

                        if (isAscendingOrder)
                            groups = groups.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            groups = groups.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(new { groups = groups })
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = groups
                                    })
                                    .WithView("Company/Exchange/groups.cshtml");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithMediaRangeModel("application/json", new { error = ex.Message })
                                    .WithView("Company/Exchange/groups.cshtml");
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Delete["/"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "dExchangeGroups"));

                #region Updates existing group
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    int id = Request.Form.ID;
                    string companyCode = _.CompanyCode;

                    // Get the group from the database
                    logger.DebugFormat("Getting the group from the database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    var dbGroup = (from d in db.DistributionGroups
                                   where d.CompanyCode == companyCode
                                   where d.ID == id
                                   select d).FirstOrDefault();
                    if (dbGroup == null)
                        throw new Exception("Unable to find distribution group in the dabase.");
                    else
                    {
                        logger.DebugFormat("Removing group {0} from Exchange", dbGroup.Email);
                        powershell = ExchPowershell.GetClass();
                        powershell.Remove_DistributionGroup(dbGroup.Email);

                        logger.DebugFormat("Removing group {0} from the database", dbGroup.Email);
                        db.DistributionGroups.Remove(dbGroup);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully deleted group" })
                                        .WithView("Company/Exchange/groups.cshtml");
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error removing group for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
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

            Get["/{ID:int}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeGroups"));

                #region Gets an existing group for editing
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    // Get group from database first
                    logger.DebugFormat("Retrieving distribution group {0} from the database", _.ID);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    var dbGroup = (from d in db.DistributionGroups
                                   where d.CompanyCode == companyCode
                                   where d.ID == id
                                   select d).FirstOrDefault();
                    if (dbGroup == null)
                        throw new Exception("Unable to find group in database.");
                    else
                    {
                        logger.DebugFormat("Retrieving distribution group from Exchange");
                        powershell = ExchPowershell.GetClass();

                        DistributionGroups group = powershell.Get_DistributionGroup(dbGroup.DistinguishedName);
                        string[] emailSplit = group.Email.Split('@');
                        group.EmailFirst = emailSplit[0];
                        group.EmailLast = emailSplit[1];

                        // Get managed but since Exchange gives the results to us in
                        // canoncial format we must query our database for all users
                        // and convert distinguished name to canoncial on the fly.
                        // Thank you Microsoft!!
                        var mailboxUsers = (from d in db.Users
                                            where d.CompanyCode == companyCode
                                            where d.MailboxPlan > 0
                                            select d).ToList();

                        var managedByOriginal = new List<string>();
                        var selectors = new List<GroupObjectSelector>();
                        if (mailboxUsers != null && group.ManagedByOriginal != null)
                        {
                            foreach (var s in group.ManagedByOriginal)
                            {
                                var m = mailboxUsers.FirstOrDefault(
                                    x => LdapConverters.ToCanonicalName(x.DistinguishedName)
                                        .Equals(s, StringComparison.InvariantCultureIgnoreCase));

                                if (m != null)
                                {
                                    selectors.Add(new GroupObjectSelector()
                                    {
                                        DisplayName = m.DisplayName,
                                        Email = m.Email,
                                        Identifier = m.UserGuid.ToString(),
                                        ObjectType = ExchangeValues.User
                                    });
                                    managedByOriginal.Add(m.UserGuid.ToString());
                                }
                            }
                            selectors = selectors.OrderBy(x => x.DisplayName).ToList();
                        }
                        group.ManagedByOriginalObject = selectors;
                        group.ManagedByOriginal = managedByOriginal.ToArray(); // This contains the converted canoncial name to email address from the database

                        return Negotiate.WithModel(new { group = group })
                                        .WithView("Company/Exchange/groups_edit.cshtml");
                    }  
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting group {0} for {1}: {2}", _.ID, _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
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

            Get["/{ID:int}/members"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vExchangeGroups"));

                #region Gets members of a distribution group
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Getting distribution group {0} from database for {1}", id, companyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var group = (from d in db.DistributionGroups
                                 where d.CompanyCode == companyCode
                                 where d.ID == id
                                 select d).FirstOrDefault();
                    if (group == null)
                        throw new Exception("Unable to find distribution group in the database");
                    else
                    {
                        logger.DebugFormat("Retrieving members of distribution group {0}", group.DistinguishedName);
                        powershell = ExchPowershell.GetClass();

                        List<GroupObjectSelector> members = powershell.Get_DistributionGroupMembers(group.DistinguishedName);
                        members = members.OrderBy(x => x.DisplayName).ToList();

                        return Response.AsJson(new { members = members });
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting members for group {0}: {1}", _.ID, ex.ToString());
                    return Response.AsJson(new { error = ex.ToString() }, HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (powershell != null)
                        powershell.Dispose();
                }
                #endregion
            };

            Post["/{ID:int}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangeGroups"));

                #region Updates existing group
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    // Bind our class to the form
                    logger.DebugFormat("Binding class for updating group for {0}", companyCode);
                    DistributionGroups updateGroup = this.Bind<DistributionGroups>();

                    // Get the group from the database
                    logger.DebugFormat("Getting the group from the database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    var dbGroup = (from d in db.DistributionGroups
                                   where d.CompanyCode == companyCode
                                   where d.ID == id
                                   select d).FirstOrDefault();
                    if (dbGroup == null)
                        throw new Exception("Unable to find distribution group in the dabase.");
                    else
                    {
                        // Get the domain from the database
                        logger.DebugFormat("Getting selected domain");
                        var domain = (from d in db.Domains
                                      where d.CompanyCode == companyCode
                                      where d.DomainID == updateGroup.DomainID
                                      select d).FirstOrDefault();
                        if (domain == null)
                            throw new Exception("Unable to find domain in the database");
                        else
                        {
                            // Format the attributes
                            logger.DebugFormat("Formatting the email and other attributes..");
                            updateGroup.Email = string.Format("{0}@{1}", updateGroup.EmailFirst.Replace(" ", string.Empty), domain.Domain);
                            updateGroup.CompanyCode = companyCode;
                            updateGroup.ManagedByAdded = updateGroup.ManagedByAdded.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            updateGroup.MembersAdded = updateGroup.MembersAdded.Except(updateGroup.MembersOriginal).Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            updateGroup.MembersRemoved = updateGroup.MembersRemoved == null ? updateGroup.MembersRemoved : updateGroup.MembersRemoved.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            
                            logger.DebugFormat("Initializing Exchange powershell");
                            powershell = ExchPowershell.GetClass();

                            logger.DebugFormat("Updating group in Exchange");
                            var createdGroup = powershell.Update_DistributionGroup(updateGroup, dbGroup.Email);

                            logger.DebugFormat("Updating group in database");
                            dbGroup.Hidden = updateGroup.Hidden;
                            dbGroup.DisplayName = updateGroup.DisplayName;
                            dbGroup.Email = updateGroup.Email;
                            db.SaveChanges();
                        }
                    }

                    string redirectUrl = string.Format("~/company/{0}/exchange/groups", companyCode);
                    return Negotiate.WithModel(new { success = "Successfully updated group" })
                                    .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating group for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
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

            Get["/new"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "eExchangeGroups"));

                return View["Company/Exchange/groups_edit.cshtml", 
                    new { 
                        companyCode = _.CompanyCode,
                        group = new DistributionGroups()
                    }];
            };

            Post["/new"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "cExchangeGroups"));

                #region Create a new group
                CloudPanelContext db = null;
                dynamic powershell = null;
                try
                {
                    string companyCode = _.CompanyCode;

                    // Bind our class to the form
                    logger.DebugFormat("Binding class for new group for {0}", companyCode);
                    DistributionGroups newGroup = this.Bind<DistributionGroups>();

                    // Get the company from the database so we can find the OU to put the group in
                    logger.DebugFormat("Getting company from database");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    var company = (from d in db.Companies
                                   where !d.IsReseller
                                   where d.CompanyCode == companyCode
                                   select d).FirstOrDefault();
                    if (company == null)
                        throw new Exception("Unable to find company in the dabase.");
                    else
                    {
                        // Get the domain from the database
                        logger.DebugFormat("Getting selected domain");
                        var domain = (from d in db.Domains
                                      where d.CompanyCode == companyCode
                                      where d.DomainID == newGroup.DomainID
                                      select d).FirstOrDefault();
                        if (domain == null)
                            throw new Exception("Unable to find domain in the database");
                        else
                        {
                            // Format the attributes


                            logger.DebugFormat("Formatting the email and other attributes..");
                            newGroup.Email = string.Format("{0}@{1}", newGroup.EmailFirst.Replace(" ", string.Empty), domain.Domain);
                            newGroup.CompanyCode = companyCode;
                            newGroup.ManagedByAdded = newGroup.ManagedByAdded.Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            newGroup.MembersAdded = newGroup.MembersAdded.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                            logger.DebugFormat("Initializing Exchange powershell");
                            powershell = ExchPowershell.GetClass();

                            logger.DebugFormat("Creating new group in Exchange");
                            var createdGroup = powershell.New_DistributionGroup(newGroup, Settings.OUPath(Settings.ExchangeGroupsOU, company.DistinguishedName));

                            logger.DebugFormat("Adding group to database");
                            db.DistributionGroups.Add(createdGroup);
                            db.SaveChanges();
                        }
                    }

                    string redirectUrl = string.Format("~/company/{0}/exchange/groups", companyCode);
                    return Negotiate.WithModel(new { success = "Successfully created new group" })
                                    .WithMediaRangeResponse("text/html", this.Response.AsRedirect(redirectUrl));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating new group for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
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

        public static List<DistributionGroups> GetGroups(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Retrieving Exchange groups for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);

                var groups = (from d in db.DistributionGroups
                              where d.CompanyCode == companyCode
                              orderby d.DisplayName
                              select d).ToList();

                if (groups != null)
                {
                    groups.ForEach(x =>
                        {
                            x.CanonicalName = LdapConverters.ToCanonicalName(x.DistinguishedName);
                        });
                }

                return groups;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error retrieving Exchange groups for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static List<GroupObjectSelector> GetAll(string companyCode)
        {
            var returnObject = new List<GroupObjectSelector>();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                db.Database.Connection.Open();

                logger.DebugFormat("Getting mailbox users for {0}", companyCode);
                var users = (from u in db.Users where u.CompanyCode == companyCode 
                             where u.MailboxPlan > 0
                             orderby u.DisplayName select u).ToList();

                if (users != null)
                {
                    logger.DebugFormat("Found a total of {0} mailbox users", users.Count());
                    users.ForEach(x =>
                        {
                            returnObject.Add(new GroupObjectSelector()
                                {
                                    ObjectType = ExchangeValues.User,
                                    DisplayName = x.DisplayName,
                                    Email = x.Email,
                                    Identifier = x.UserGuid.ToString()
                                });
                        });
                }

                logger.DebugFormat("Getting distribution groups for {0}", companyCode);
                var groups = (from d in db.DistributionGroups 
                              where d.CompanyCode == companyCode
                              where !string.IsNullOrEmpty(d.Email)
                              orderby d.DisplayName select d).ToList();

                if (groups != null)
                {
                    logger.DebugFormat("Found a total of {0} groups", groups.Count());
                    groups.ForEach(x =>
                        {
                            returnObject.Add(new GroupObjectSelector()
                                {
                                    ObjectType = ExchangeValues.Group,
                                    DisplayName = x.DisplayName,
                                    Email = x.Email,
                                    Identifier = x.Email
                                });
                        });
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting mailbox users for permissions: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }

            return returnObject;
        }
    }
}