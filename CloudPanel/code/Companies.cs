//
// Copyright (c) 2014, Jacob Dixon
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by KnowMoreIT and Compsys.
// 4. Neither the name of KnowMoreIT and Compsys nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY Jacob Dixon ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL Jacob Dixon BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudPanel.ActiveDirectory;
using CloudPanel.Base.AD;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Rollback;

namespace CloudPanel.code
{
    public class Companies
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Companies));

        private CloudPanelContext db = null;

        public Companies()
        {
            this.db = new CloudPanelContext(Settings.ConnectionString);
        }

        /// <summary>
        /// Creates a new company in AD and the database
        /// </summary>
        /// <param name="newCompany"></param>
        /// <param name="domainName"></param>
        /// <param name="resellerCode"></param>
        public void Create(Company newCompany, string domainName, string resellerCode)
        {
            OrganizationalUnits organizationalUnits = null;
            Groups groups = null;

            ReverseActions reverse = new ReverseActions();
            try
            {
                if (string.IsNullOrEmpty(domainName))
                    throw new MissingFieldException("", "DomainName");

                if (string.IsNullOrEmpty(resellerCode))
                    throw new MissingFieldException("", "ResellerCode");

                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                groups = new Groups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Check if the user provided a company code or not
                // If they didn't then we will automatically generate one
                if (string.IsNullOrEmpty(newCompany.CompanyCode))
                    newCompany.CompanyCode = OtherStatics.FindAvailableCompanyCode(newCompany.CompanyName, this.db);

                OrganizationalUnit newOrg = new OrganizationalUnit();
                newOrg.Name = newCompany.CompanyCode;
                newOrg.DisplayName = newCompany.CompanyName;
                newOrg.City = newCompany.City;
                newOrg.State = newCompany.State;
                newOrg.PostalCode = newCompany.ZipCode;
                newOrg.Country = newCompany.Country;
                newOrg.UPNSuffixes = new[] { domainName };
                newOrg.Description = newCompany.Description;

                var createdCompany = organizationalUnits.Create(Settings.HostingOU, newOrg);
                reverse.AddAction(Actions.CreateOrganizationalUnit, createdCompany.DistinguishedName);

                //
                // Create security groups
                //
                string strippedCompanyCode = newCompany.CompanyCode.Replace(" ", string.Empty);

                // Create Admins@ group
                SecurityGroup newGroup = new SecurityGroup();
                newGroup.Name = string.Format("Admins@", strippedCompanyCode);
                newGroup.SamAccountName = newGroup.Name.Length > 19 ? newGroup.Name.Substring(0, 18) : newGroup.Name;
                groups.Create(createdCompany.DistinguishedName, newGroup);
                reverse.AddAction(Actions.CreateSecurityGroup, newGroup.Name);

                // Create AllUsers@ group
                newGroup.Name = string.Format("AllUsers@", strippedCompanyCode);
                newGroup.SamAccountName = newGroup.Name.Length > 19 ? newGroup.Name.Substring(0, 18) : newGroup.Name;
                groups.Create(createdCompany.DistinguishedName, newGroup);
                reverse.AddAction(Actions.CreateSecurityGroup, newGroup.Name);

                // Create AllTSUsers@ group
                newGroup.Name = string.Format("AllTSUsers@", strippedCompanyCode);
                newGroup.SamAccountName = newGroup.Name.Length > 19 ? newGroup.Name.Substring(0, 18) : newGroup.Name;
                groups.Create(createdCompany.DistinguishedName, newGroup);
                reverse.AddAction(Actions.CreateSecurityGroup, newGroup.Name);
                groups.AddGroup("GPOAccess@" + resellerCode, newGroup.Name); // Add group to the GPOAccess group in resellers OU

                //
                // Create Exchange and Applications OU
                //
                newOrg = new OrganizationalUnit();
                newOrg.Name = Settings.ExchangeOU;
                newOrg.DisplayName = Settings.ExchangeOU;
                newOrg.UPNSuffixes = new[] { domainName };

                var createdOrg = organizationalUnits.Create(createdCompany.DistinguishedName, newOrg);
                reverse.AddAction(Actions.CreateOrganizationalUnit, createdOrg.DistinguishedName);

                newOrg = new OrganizationalUnit();
                newOrg.Name = Settings.ApplicationsOU;
                newOrg.DisplayName = Settings.ApplicationsOU;
                newOrg.UPNSuffixes = new[] { domainName };

                createdOrg = organizationalUnits.Create(createdCompany.DistinguishedName, newOrg);
                reverse.AddAction(Actions.CreateOrganizationalUnit, createdOrg.DistinguishedName);
                
                // Add to SQL
                log.DebugFormat("Saving new company {0} to the database.", newCompany.CompanyName);
                newCompany.Created = DateTime.Now;
                newCompany.DistinguishedName = createdCompany.DistinguishedName;
                newCompany.IsReseller = false;

                db.Companies.Add(newCompany);
                db.SaveChanges();

                log.InfoFormat("Successfully created new company {0}", newCompany.CompanyName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating new company: {0}", ex.ToString());
                reverse.RollbackNow();
                throw;
            }
            finally
            {
                if (groups != null)
                    groups.Dispose();

                if (organizationalUnits != null)
                    organizationalUnits.Dispose();
            }
        }

        /// <summary>
        /// Updates an existing company in AD and the database
        /// </summary>
        /// <param name="existingCompany"></param>
        public void Update(Company existingCompany)
        {
            OrganizationalUnits organizationalUnits = null;

            try
            {
                if (string.IsNullOrEmpty(existingCompany.CompanyCode))
                    throw new MissingFieldException("Company", "CompanyCode");

                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Find the company from SQL
                var company = (from c in db.Companies
                                where !c.IsReseller
                                where c.CompanyCode == existingCompany.CompanyCode
                                select c).First();

                if (company == null)
                    throw new ArgumentNullException(existingCompany.CompanyCode);
                else
                {
                    // Set the new values
                    company.CompanyName = existingCompany.CompanyName;
                    company.AdminName = existingCompany.AdminName;
                    company.AdminEmail = existingCompany.AdminEmail;
                    company.PhoneNumber = existingCompany.PhoneNumber;
                    company.Street = existingCompany.Street;
                    company.City = existingCompany.City;
                    company.State = existingCompany.State;
                    company.ZipCode = existingCompany.ZipCode;
                    company.Country = existingCompany.Country;

                    // Update the OU
                    log.DebugFormat("Updating organizational unit for company {0}", existingCompany.CompanyName);
                    organizationalUnits.Update(new OrganizationalUnit()
                    {
                        DistinguishedName = company.DistinguishedName,
                        DisplayName = company.CompanyName,
                        Description = company.Description,
                        Street = company.Street,
                        City = company.City,
                        State = company.State,
                        PostalCode = company.ZipCode,
                        Country = company.Country
                    });

                    // Save SQL changes
                    db.SaveChanges();

                    log.InfoFormat("Successfully updated existing company {0}. New name if changed: {1}", existingCompany.CompanyName, company.CompanyName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error updating company: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (organizationalUnits != null)
                    organizationalUnits.Dispose();
            }
        }

        /// <summary>
        /// Updates the plan associated with the company (Organization Plan / Company Plan)
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="newPlanID"></param>
        public void UpdatePlan(string companyCode, int newPlanID)
        {
            try
            {
                var company = (from c in db.Companies
                               where !c.IsReseller
                               where c.CompanyCode == companyCode
                               select c).First();

                company.OrgPlanID = newPlanID;
                db.SaveChanges();                
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error updating company plan for {0}. Error: {1}", companyCode, ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Deletes a company. Does not check for existing users. Is destructive
        /// </summary>
        /// <param name="companyCode"></param>
        public void Delete(string companyCode)
        {
            OrganizationalUnits organizationalUnits = null;

            try
            {
                if (string.IsNullOrEmpty(companyCode))
                    throw new MissingFieldException("", "CompanyCode");

                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Find the resellers from SQL
                var company = (from c in db.Companies
                                where c.IsReseller
                                where c.CompanyCode == companyCode
                                select c).First();

                if (company == null)
                    throw new ArgumentNullException(companyCode);
                else
                {
                    // SAFE DELETE OFF! DESTRUCTIVE! WILL DELETE ALL USERS AND OBJECTS IN COMPANY!!
                    organizationalUnits.Delete(company.DistinguishedName, false);

                    // Remove all companiesby calling stored procedure
                    db.spDeleteCompany(company.CompanyCode);

                    log.InfoFormat("Successfully deleted company {0}.", company.CompanyName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error deleting company: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (organizationalUnits != null)
                    organizationalUnits.Dispose();
            }
        }

        /// <summary>
        /// Retrieves a company from the database
        /// </summary>
        /// <param name="resellerCode"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public Company GetCompany(string companyCode)
        {
            var company = (from c in db.Companies
                            where !c.IsReseller
                            where c.CompanyCode == companyCode
                            select c).First();

            return company;
        }

        #region Domains

        /// <summary>
        /// Retrieves a single domain from the database for a specific company
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="domainID"></param>
        /// <returns></returns>
        public Domain GetDomain(string companyCode, int domainID)
        {
            var domain = (from d in db.Domains
                           where d.CompanyCode == companyCode
                           where d.DomainID == domainID
                           orderby d.Domain1
                           select d).FirstOrDefault();

            return domain;
        }

        /// <summary>
        /// Retrieves a list of domains from the database for a specific company
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public List<Domain> GetDomains(string companyCode)
        {
            var domains = (from d in db.Domains
                           where d.CompanyCode == companyCode
                           orderby d.Domain1
                           select d).ToList();

            return domains;
        }

        /// <summary>
        /// Adds a new domain
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="domainName"></param>
        public void AddDomain(string companyCode, string domainName)
        {
            if (string.IsNullOrEmpty(companyCode))
                throw new MissingFieldException("Companies", "companyCode");

            if (string.IsNullOrEmpty(domainName))
                throw new MissingFieldException("Companies", "domainName");

            // Get company
            var company = (from c in db.Companies 
                           where !c.IsReseller 
                           where c.CompanyCode == companyCode 
                           select c).First();

            // Check if domain exists
            var domain = (from d in db.Domains 
                          where d.Domain1 == domainName 
                          select d).Count();

            if (domain > 0)
                throw new Exception("Domain already exists in the system.");

            // Check for company limits
            // TODO

            OrganizationalUnits organizationalUnits = null;
            ReverseActions reverse = new ReverseActions();
            try
            {
                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                organizationalUnits.AddDomains(company.DistinguishedName, new[] { domainName });
                reverse.AddAction(Actions.AddDomains, new object[] { 
                    company.DistinguishedName, 
                    domainName
                });

                // Add domain to the system
                Domain newDomain = new Domain();
                newDomain.Domain1 = domainName;
                newDomain.CompanyCode = companyCode;
                newDomain.IsAcceptedDomain = false;
                newDomain.IsLyncDomain = false;
                newDomain.IsDefault = false;

                db.Domains.Add(newDomain);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error adding domain {0} to {1}. Error {2}", domainName, companyCode, ex.ToString());
                reverse.RollbackNow();
                throw;
            }
            finally
            {
                if (organizationalUnits != null)
                    organizationalUnits.Dispose();
            }
        }

        /// <summary>
        /// Gets a list of mailbox plans for the company
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public List<Plans_ExchangeMailbox> GetMailboxPlans(string companyCode)
        {
            var plans = (from p in db.Plans_ExchangeMailbox
                         where p.CompanyCode == companyCode || string.IsNullOrEmpty(p.CompanyCode)
                         orderby p.MailboxPlanName
                         select p).ToList();

            return plans;
        }

        /// <summary>
        /// Gets a list of activesync plans
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public List<Plans_ExchangeActiveSync> GetActiveSyncPlans(string companyCode)
        {
            var plans = (from a in db.Plans_ExchangeActiveSync
                         where a.CompanyCode == companyCode || string.IsNullOrEmpty(a.CompanyCode)
                         orderby a.DisplayName
                         select a).ToList();

            return plans;
        }

        /// <summary>
        /// Compiles a list of users with their SamAccountName
        /// This is mostly used for FullACcess, SendAs, SendOnBehalf permissions
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetUsersListAndSamAccountName(string companyCode)
        {
            var users = (from u in db.Users
                         where u.CompanyCode == companyCode
                         where !string.IsNullOrEmpty(u.sAMAccountName)
                         orderby u.DisplayName
                         select u).ToList();

            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (var u in users)
            {
                if (!values.ContainsKey(u.sAMAccountName))
                {
                    values.Add(u.sAMAccountName, u.DisplayName);
                }
            }

            return values;
        }

        #endregion

        #region Exchange

        /// <summary>
        /// Checks if a company is enabled for Exchange or not
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public bool IsExchangeEnabled(string companyCode)
        {
            var isEnabled = (from c in db.Companies
                             where c.CompanyCode == companyCode
                             select c.ExchEnabled).First();

            return isEnabled;
        }

        /// <summary>
        /// Gets a list of Exchange contacts
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public List<Contact> GetContacts(string companyCode)
        {
            var contacts = (from c in db.Contacts
                            where c.CompanyCode == companyCode
                            select c).ToList();

            return contacts;
        }

        /// <summary>
        /// Gets a specific contact
        /// </summary>
        /// <param name="contactID"></param>
        /// <returns></returns>
        public Contact GetContact(int contactID)
        {
            var contact = (from c in db.Contacts
                           where c.ID == contactID
                           select c).First();

            return contact;
        }

        #endregion
    }
}