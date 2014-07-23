using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
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

namespace CloudPanel.code
{
    public class EmailClass
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(EmailClass));

        private CloudPanelContext db = null;

        public EmailClass()
        {
            this.db = new CloudPanelContext(Settings.ConnectionString);
        }

        public List<DistributionGroup> GetDistribtionGroups(string companyCode)
        {
            var groups = (from g in db.DistributionGroups
                          where g.CompanyCode == companyCode
                          orderby g.DisplayName
                          select g).ToList();

            return groups;
        }

        #region Contacts

        public void New_MailContact(Contact newContact)
        {
            dynamic exchange = null;
            
            try
            {
                var dn = (from c in db.Companies
                          where c.CompanyCode == newContact.CompanyCode
                          where !c.IsReseller
                          select c.DistinguishedName).First();

                string exchangeOU = string.Format("OU={0},{1}", Settings.ExchangeOU, dn);

                exchange = GetExchangeClass();
                
                var contact = exchange.New_MailContact(newContact, exchangeOU);
                db.Contacts.Add(contact);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error creating mail contact for {0} with email {1}. Error: {2}", newContact.CompanyCode, newContact.Email, ex.ToString());
                throw;
            }
            finally
            {
                exchange.Dispose();
            }
        }

        public void Update_MailContact(Contact updateContact)
        {
            Exch2010 exchange = null;

            try
            {
                var contact = (from c in db.Contacts
                               where c.ID == updateContact.ID
                               where c.CompanyCode == updateContact.CompanyCode
                               select c).First();

                // Set parameters for exchange class
                updateContact.DistinguishedName = contact.DistinguishedName;

                // Update Exchange
                exchange = GetExchangeClass();
                Contact updatedContact = exchange.Update_MailContact(updateContact);

                // Update database
                contact.DistinguishedName = updatedContact.DistinguishedName;
                contact.DisplayName = updateContact.DisplayName;
                contact.Hidden = updateContact.Hidden;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error updating mail contact for {0} with email {1}. Error: {2}", updateContact.CompanyCode, updateContact.Email, ex.ToString());
                throw;
            }
            finally
            {
                exchange.Dispose();
            }
        }

        public void Remove_MailContact(string companyCode, int contactID)
        {
            Exch2010 exchange = null;

            try
            {
                var contact = (from c in db.Contacts
                               where c.ID == contactID
                               where c.CompanyCode == companyCode
                               select c).First();

                if (contact != null)
                {
                    exchange = GetExchangeClass();
                    exchange.Remove_MailContact(contact.DistinguishedName);

                    db.Contacts.Remove(contact);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error removing mail contact {0}. Error: {1}", contactID, ex.ToString());
                throw;
            }
            finally
            {
                if (exchange != null)
                    exchange.Dispose();
            }
        }

        #endregion

        private dynamic GetExchangeClass()
        {
            return new Exch2010(string.Format("https://{0}/powershell", Settings.ExchangeServer),
                                Settings.Username,
                                Settings.DecryptedPassword,
                                Settings.ExchangeConnection.Equals("Kerberos", StringComparison.CurrentCultureIgnoreCase) ? true : false,
                                Settings.PrimaryDC);
        }
    }
}