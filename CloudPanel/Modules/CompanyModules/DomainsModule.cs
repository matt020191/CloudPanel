using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Enums;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CloudPanel.Modules
{
    public class DomainsModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DomainsModule));

        public DomainsModule() : base("/company/{CompanyCode}/domains")
        {
            this.RequiresAuthentication();

            Get["/"] = _ =>
            {
                #region Returns the domains view with model or json data based on the request
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Opening connection to retrieve domains for {0}", _.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = _.CompanyCode;
                    logger.DebugFormat("Reading domains from database for {0}", companyCode);
                    var domains = (from d in db.Domains
                                   where d.CompanyCode == companyCode
                                   orderby d.Domain
                                   select d).ToList();

                    int draw = 0, start = 0, length = 0, recordsTotal = domains.Count, recordsFiltered = domains.Count, orderColumn = 0;
                    string searchValue = "", orderColumnName = "";
                    bool isAscendingOrder = true;

                    // Check for dataTables and process the values
                    logger.DebugFormat("Checking if this is jQuery dataTables");
                    if (Request.Query.draw.HasValue)
                    {
                        logger.DebugFormat("We are using jQuery dataTables.. Gathering information");
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
                            logger.DebugFormat("Search value for domains was not empty: {0}", searchValue);
                            domains = (from d in domains
                                       where d.Domain.IndexOf(searchValue, StringComparison.InvariantCultureIgnoreCase) != -1 
                                       select d).ToList();
                            recordsFiltered = domains.Count;
                        }

                        logger.DebugFormat("Checking if ascending order: {0}", isAscendingOrder);
                        if (isAscendingOrder)
                            domains = domains.OrderBy(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                        else
                            domains = domains.OrderByDescending(x => x.GetType()
                                                    .GetProperty(orderColumnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).GetValue(x, null))
                                                    .Skip(start)
                                                    .Take(length)
                                                    .ToList();
                    }

                    return Negotiate.WithModel(new { domains = domains })
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        draw = draw,
                                        recordsTotal = recordsTotal,
                                        recordsFiltered = recordsFiltered,
                                        data = domains
                                    })
                                    .WithView("Company/company_domains.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting domains for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/company_domains.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/"] = _ =>
            {
                #region Creates a new domain for the company
                CloudPanelContext db = null;
                ADOrganizationalUnits org = null;
                try
                {
                    logger.DebugFormat("Opening connection to add new domain for {0}", _.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting company code and new domain name for {0}", _.CompanyCode);
                    string companyCode = _.CompanyCode;
                    string domainName = Request.Form.DomainName;

                    // Validate
                    logger.DebugFormat("Validating domain name {0}", domainName);
                    domainName = domainName.Replace(" ", string.Empty);
                    if (string.IsNullOrEmpty(domainName))
                        throw new Exception("Domain name was not provided. Please providate the domain name to add");
                    else if (Regex.IsMatch(domainName, "^(?!-)[a-zA-Z0-9-]{1,63}(?<!-)$", RegexOptions.IgnoreCase))
                        throw new Exception("Domain name was invalid");
                    else
                    {
                        logger.DebugFormat("Domain name {0} passed validation. Checking if it already exists in the system", domainName);
                        bool exist = (from d in db.Domains where d.Domain == domainName select d).Count() > 0;
                        if (exist)
                            throw new Exception("The domain name you entered is already in use.");
                        else
                        {
                            logger.DebugFormat("All validations passed for domain name {0}. Adding to Active Directory and the database", domainName);
                            org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                            logger.DebugFormat("Getting company {0} from the database", companyCode);
                            var distinguishedName = (from d in db.Companies where !d.IsReseller where d.CompanyCode == companyCode select d.DistinguishedName).FirstOrDefault();

                            logger.DebugFormat("Adding domain to Active Directory");
                            org.AddDomains(distinguishedName, new[] { domainName });

                            logger.DebugFormat("Adding domain {0} to the database", domainName);
                            var newDomain = new Domains();
                            newDomain.Domain = domainName;
                            newDomain.CompanyCode = companyCode;
                            newDomain.IsDefault = false;
                            newDomain.IsLyncDomain = false;
                            newDomain.IsAcceptedDomain = false;
                            newDomain.DomainType = DomainType.Default;

                            db.Domains.Add(newDomain);
                            db.SaveChanges();

                            return Negotiate.WithView("Company/company_domains.cshtml");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error adding new domain for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/company_domains.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (org != null)
                        org.Dispose();

                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Delete["/"] = _ =>
            {
                #region Deletes a domain from the company
                CloudPanelContext db = null;
                ADOrganizationalUnits org = null;
                try
                {
                    logger.DebugFormat("Opening connection to delete domain for {0}", _.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting company code and domain name for {0}", _.CompanyCode);
                    string companyCode = _.CompanyCode;
                    int domainId = Request.Form.DomainID;

                    // Get domain
                    logger.DebugFormat("Retrieving domain {0} from the database", domainId);
                    var domain = (from d in db.Domains
                                  where d.CompanyCode == companyCode
                                  where d.DomainID == domainId
                                  select d).First();

                    // Validate
                    logger.DebugFormat("Making sure the domain {0} is not an accepted domain", domain.Domain);
                    if (domain.IsAcceptedDomain)
                        throw new Exception("This domain is enabled for Exchange. Please remove the accepted domain before deleting this domain.");
                    else
                    {
                        logger.DebugFormat("Making sure the domain is not in use.");
                        string atDomain = string.Format("@{0}", domain.Domain);
                        var domainUsed = (from d in db.Users
                                          where d.UserPrincipalName.Contains(atDomain) || d.Email.Contains(atDomain)
                                          select d).Count();

                        if (domainUsed > 0)
                            throw new Exception("Unable to remove domain because it is in use");
                        else
                        {
                            logger.DebugFormat("Initializing Active Directory organizational unit class");
                            org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                            logger.DebugFormat("Getting distinguished name from company");
                            var distinguishedName = (from d in db.Companies where !d.IsReseller where d.CompanyCode == companyCode select d.DistinguishedName).First();

                            logger.DebugFormat("Removing domain from Active Directory");
                            org.RemoveDomains(distinguishedName, new[] { domain.Domain });

                            logger.DebugFormat("Removing domain from the database");
                            db.Domains.Remove(domain);
                            db.SaveChanges();

                            return Negotiate.WithView("Company/company_domains.cshtml");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting domain for {0}: {1}", _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Company/company_domains.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (org != null)
                        org.Dispose();

                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Get["/{ID:int}"] = _ =>
            {
                #region Returns the edit page or json data for a specific domain
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Opening connection to retrieve domain {0} for {1}", _.ID, _.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    int id = _.ID;
                    string companyCode = _.CompanyCode;

                    logger.DebugFormat("Reading domain {0} from database for {1}", id, companyCode);
                    var domain = (from d in db.Domains
                                   where d.CompanyCode == companyCode
                                   where d.DomainID == id
                                   select d).First();

                    return Negotiate.WithModel(new { domain = domain })
                                    .WithView("Company/company_domains_edit.cshtml");
                }
                catch (Exception ex)
                {
                    ViewBag.error = ex.ToString();
                    logger.ErrorFormat("Error getting domain {0} for {1}: {2}", _.ID, _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Put["/{ID:int}"] = _ =>
            {
                #region Updates an existing domain
                CloudPanelContext db = null;
                dynamic exchangePowershell = null;
                try
                {
                    logger.DebugFormat("Opening connection to edit domain for {0}", _.CompanyCode);
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    logger.DebugFormat("Getting company code and domain {0} for {1}", _.ID, _.CompanyCode);
                    string companyCode = _.CompanyCode;
                    int domainId = _.ID;

                    logger.DebugFormat("Retrieving domain {0} from the database", domainId);
                    var domain = (from d in db.Domains
                                  where d.CompanyCode == companyCode
                                  where d.DomainID == domainId
                                  select d).First();


                    //
                    // Update domain in Exchange only if this company is enabled for Exchange
                    //
                    if (CPStaticHelpers.IsExchangeEnabled(companyCode))
                    {
                        logger.DebugFormat("Checking if domain type was changed");
                        int originalDomainType = domain.DomainType == null ? 0 : (int)domain.DomainType;
                        int newDomainType = Request.Form.DomainType.HasValue ? Request.Form.DomainType : 0;
                        domain.DomainType = newDomainType;

                        logger.DebugFormat("Original domain type was {0} and now is {1}", originalDomainType, newDomainType);
                        if (originalDomainType != newDomainType)
                        {
                            logger.DebugFormat("Domain type was changed");
                            exchangePowershell = ExchPowershell.GetClass();

                            if (originalDomainType == 0)
                            {
                                // Enabling accepted domain
                                logger.DebugFormat("Enabling domain {0} for Exchange", domain.Domain);
                                exchangePowershell.New_AcceptedDomain(new Domains() { Domain = domain.Domain, DomainType = newDomainType });
                                domain.IsAcceptedDomain = true;
                            }
                            else if (newDomainType == 0)
                            {
                                // Disabling accepted domain
                                logger.DebugFormat("Removing domain {0} from Exchange", domain.Domain);
                                exchangePowershell.Remove_AcceptedDomain(new Domains() { Domain = domain.Domain });
                                domain.IsAcceptedDomain = false;
                            }
                            else
                            {
                                // Update domain type
                                logger.DebugFormat("Setting domain {0} type to {1} in Exchange", domain.Domain, newDomainType);
                                exchangePowershell.Update_AcceptedDomain(new Domains() { Domain = domain.Domain, DomainType = newDomainType });
                                domain.IsAcceptedDomain = true;
                            }
                        }
                    }
                    else
                    {
                        domain.DomainType = DomainType.Default;
                        domain.IsAcceptedDomain = false;
                    }
                    db.SaveChanges();
                    
                    //
                    // See if this domain is the default
                    //
                    logger.DebugFormat("Checking if this domain was set to the default");
                    bool newIsDefault = Request.Form.IsDefault;
                    if (domain.IsDefault == false && newIsDefault)
                    {
                        // See if the default domain was changed
                        logger.DebugFormat("Domain {0} is being changed to the default", domain.Domain);
                        var allDomains = from d in db.Domains where d.CompanyCode == companyCode select d;
                        foreach (var d in allDomains)
                        {
                            logger.DebugFormat("Setting domain {0} default to false", d.Domain);
                            d.IsDefault = false;
                        }

                        logger.DebugFormat("Updating domain {0} to be the new default", domain.Domain);
                        domain.IsDefault = true;
                        db.SaveChanges();
                    }

                    string redirectUrl = string.Format("~/company/{0}/domains", companyCode);
                    return Response.AsRedirect(redirectUrl);
                }
                catch (Exception ex)
                {
                    ViewBag.error = ex.ToString();
                    logger.ErrorFormat("Error updating domain {0} for {1}: {2}", _.ID, _.CompanyCode, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("error.cshtml")
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (exchangePowershell != null)
                        exchangePowershell.Dispose();

                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }

        public static List<Domains> GetAcceptedDomains(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Getting company accepted domains for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);

                // Generate code
                var domains = (from d in db.Domains
                              where d.IsAcceptedDomain
                              where d.CompanyCode == companyCode
                              select d).ToList();

                return domains;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting accepted domains for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IHtmlString GetAcceptedDomainsOptions(string companyCode, int selectedValue)
        {
            var returnString = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Getting company accepted domains for {0}", companyCode);
                db = new CloudPanelContext(Settings.ConnectionString);

                // Generate code
                var domains = (from d in db.Domains
                               where d.IsAcceptedDomain
                               where d.CompanyCode == companyCode
                               select d).ToList();

                if (domains != null)
                {
                    domains.ForEach(x =>
                    {
                        returnString.AppendFormat("<option value=\"{0}\" {1}>{2}</option>", 
                            x.DomainID,
                            x.DomainID == selectedValue ? "selected" : "",
                            x.Domain);
                    });
                }

                return new NonEncodedHtmlString(returnString.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting accepted domains for {0}: {1}", companyCode, ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}