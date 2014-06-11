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
using System.Data.Entity.Validation;
using CloudPanel.Rollback;
using System.Threading;

namespace CloudPanel.code
{
    public class Resellers
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Resellers));

        private CloudPanelContext db = null;

        public Resellers()
        {
            this.db = new CloudPanelContext(Settings.ConnectionString);
        }

        public List<Company> GetAll()
        {
            var resellers = (from c in db.Companies
                            where c.IsReseller
                            orderby c.CompanyName
                            select c).ToList();

            // Count the total companies under the reseller
            foreach (var reseller in resellers)
                reseller.TotalCompanies = (from c in db.Companies where !c.IsReseller where c.ResellerCode == reseller.CompanyCode select c).Count();

            return resellers;
        }

        public Company GetReseller(string resellerCode)
        {
            var reseller = (from c in db.Companies
                             where c.IsReseller
                             where c.CompanyCode == resellerCode
                             select c).First();

            // Count the total companies under the reseller
            reseller.TotalCompanies = (from c in db.Companies where !c.IsReseller where c.ResellerCode == reseller.CompanyCode select c).Count();

            return reseller;
        }

        public List<Company> GetCompanies(string resellerCode)
        {
            var companies = from c in db.Companies
                            where !c.IsReseller
                            where c.ResellerCode == resellerCode
                            orderby c.CompanyName
                            select c;

            return companies.ToList();
        }

        public void Create(Company newReseller)
        {
            OrganizationalUnits organizationalUnits = null;
            Groups groups = null;

            ReverseActions reverse = new ReverseActions();
            try
            {
                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                groups = new Groups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Check if the user provided a company code or not
                // If they didn't then we will automatically generate one
                if (string.IsNullOrEmpty(newReseller.CompanyCode))
                    newReseller.CompanyCode = OtherStatics.FindAvailableCompanyCode(newReseller.CompanyName, this.db);

                OrganizationalUnit newOrg = new OrganizationalUnit();
                newOrg.Name = newReseller.CompanyCode;
                newOrg.DisplayName = newReseller.CompanyName;
                newOrg.City = newReseller.City;
                newOrg.State = newReseller.State;
                newOrg.PostalCode = newReseller.ZipCode;
                newOrg.Country = newReseller.Country;
                newOrg.UPNSuffixes = null; // Do not allow UPNSuffixes on the reseller OU
                newOrg.Description = newReseller.Description;
                
                var createdReseller = organizationalUnits.Create(Settings.HostingOU, newOrg);
                reverse.AddAction(Actions.CreateOrganizationalUnit, createdReseller.DistinguishedName);

                //
                // Create GPOAccess Group
                //
                SecurityGroup newGroup = new SecurityGroup();
                newGroup.Name = string.Format("GPOAccess@{0}", newReseller.CompanyCode.Replace(" ", string.Empty));
                newGroup.SamAccountName = newGroup.Name.Length > 19 ? newGroup.Name.Substring(0, 18) : newGroup.Name;

                groups.Create(createdReseller.DistinguishedName, newGroup);
                reverse.AddAction(Actions.CreateSecurityGroup, newGroup.Name);

                //
                // Add group to hoster GPOAccess group
                //
                groups.AddGroup("GPOAccess@Hosting", newGroup.Name);

                // Add to SQL
                log.DebugFormat("Saving new reseller {0} to the database.", newReseller.CompanyName);
                newReseller.Created = DateTime.Now;
                newReseller.DistinguishedName = createdReseller.DistinguishedName;
                newReseller.IsReseller = true;

                db.Companies.Add(newReseller);
                db.SaveChanges();

                log.InfoFormat("Successfully created new reseller {0}", newReseller.CompanyName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating new reseller: {0}", ex.ToString());
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

        public void Update(Company existingReseller)
        {
            OrganizationalUnits organizationalUnits = null;

            try
            {
                if (string.IsNullOrEmpty(existingReseller.CompanyCode))
                    throw new MissingFieldException("Company", "CompanyCode");

                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Find the resellers from SQL
                var reseller = (from c in db.Companies
                                where c.IsReseller
                                where c.CompanyCode == existingReseller.CompanyCode
                                select c).First();

                // Set the new values
                reseller.CompanyName = existingReseller.CompanyName;
                reseller.AdminName = existingReseller.AdminName;
                reseller.AdminEmail = existingReseller.AdminEmail;
                reseller.PhoneNumber = existingReseller.PhoneNumber;
                reseller.Street = existingReseller.Street;
                reseller.City = existingReseller.City;
                reseller.State = existingReseller.State;
                reseller.ZipCode = existingReseller.ZipCode;
                reseller.Country = existingReseller.Country;

                // Update the OU
                log.DebugFormat("Updating organizational unit for reseller {0}", existingReseller.CompanyName);
                organizationalUnits.Update(new OrganizationalUnit()
                    {
                        DistinguishedName = reseller.DistinguishedName,
                        DisplayName = reseller.CompanyName,
                        Description = reseller.Description,
                        Street = reseller.Street,
                        City = reseller.City,
                        State = reseller.State,
                        PostalCode = reseller.ZipCode,
                        Country = reseller.Country
                    });

                // Save SQL changes
                db.SaveChanges();

                log.InfoFormat("Successfully updated existing reseller {0}. New name if changed: {1}", existingReseller.CompanyName, reseller.CompanyName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error updating reseller: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (organizationalUnits != null)
                    organizationalUnits.Dispose();
            }
        }

        public void Delete(string resellerCode)
        {
            OrganizationalUnits organizationalUnits = null;

            try
            {
                if (string.IsNullOrEmpty(resellerCode))
                    throw new MissingFieldException("", "ResellerCode");

                organizationalUnits = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);

                // Find the resellers from SQL
                var reseller = (from c in db.Companies
                                where c.IsReseller
                                where c.CompanyCode == resellerCode
                                select c).First();

                // See if any companies belong to the reseller
                var companyCount = (from c in db.Companies where c.ResellerCode == resellerCode select c).Count();
                if (companyCount > 0)
                    throw new Exception("Unable to delete reseller because it contains companies.");

                organizationalUnits.Delete(reseller.DistinguishedName, true);
                db.Companies.Remove(reseller);
                db.SaveChanges();

                log.InfoFormat("Successfully deleted reseller {0}.", reseller.CompanyName);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error deleting reseller: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (organizationalUnits != null)
                    organizationalUnits.Dispose();
            }
        }
    }
}