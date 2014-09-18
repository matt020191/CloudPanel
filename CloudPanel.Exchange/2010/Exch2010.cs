using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Enums;
using CloudPanel.Base.Exchange;
using log4net;
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace CloudPanel.Exchange
{
    public class Exch2010 : ExchPowershell
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(Exch2010));

        public Exch2010(string uri, string username, string password, bool kerberos, string domainController) :
            base(uri, username, password, kerberos, domainController)
        {
        }

        #region Address Lists

        public string New_AddressListForUsers(string companyCode, string organizationalUnit)
        {
            string name = string.Format(Settings.ExchangeUSERSName, companyCode);
            string filter = string.Format(Settings.ExchangeUSERSFilter, companyCode);

            logger.DebugFormat("Creating new address list '{0}' with filter '{1}' for {2}", name, filter, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-AddressList");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("RecipientFilter", filter);
            cmd.AddParameter("RecipientContainer", organizationalUnit);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public string New_AddressListForContacts(string companyCode, string organizationalUnit)
        {
            string name = string.Format(Settings.ExchangeCONTACTSName, companyCode);
            string filter = string.Format(Settings.ExchangeCONTACTSFilter, companyCode);

            logger.DebugFormat("Creating new address list '{0}' with filter '{1}' for {2}", name, filter, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-AddressList");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("RecipientFilter", filter);
            cmd.AddParameter("RecipientContainer", organizationalUnit);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public string New_AddressListForGroups(string companyCode, string organizationalUnit)
        {
            string name = string.Format(Settings.ExchangeGROUPSName, companyCode);
            string filter = string.Format(Settings.ExchangeGROUPSFilter, companyCode);

            logger.DebugFormat("Creating new address list '{0}' with filter '{1}' for {2}", name, filter, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-AddressList");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("RecipientFilter", filter);
            cmd.AddParameter("RecipientContainer", organizationalUnit);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public string New_AddressListForRooms(string companyCode, string organizationalUnit)
        {
            string name = string.Format(Settings.ExchangeROOMSName, companyCode);
            string filter = string.Format(Settings.ExchangeROOMSFilter, companyCode);

            logger.DebugFormat("Creating new address list '{0}' with filter '{1}' for {2}", name, filter, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-AddressList");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("RecipientFilter", filter);
            cmd.AddParameter("RecipientContainer", organizationalUnit);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public void Remove_AddressList(string name)
        {
            logger.DebugFormat("Attempting to remove address list {0}", name);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-AddressList");
            cmd.AddParameter("Identity", name);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        #endregion

        #region GAL / OAL / Address Book Policies

        public string New_GlobalAddressList(string companyCode, string organizationalUnit)
        {
            string name = string.Format(Settings.ExchangeGALName, companyCode);
            string filter = string.Format(Settings.ExchangeGALFilter, companyCode);

            logger.DebugFormat("Creating new address list '{0}' with filter '{1}' for {2}", name, filter, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-GlobalAddressList");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("RecipientFilter", filter);
            cmd.AddParameter("RecipientContainer", organizationalUnit);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public void Remove_GlobalAddressList(string name)
        {
            logger.DebugFormat("Attempting to remove global address list {0}", name);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-GlobalAddressList");
            cmd.AddParameter("Identity", name);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        public virtual string New_OfflineAddressBook(string companyCode)
        {
            string name = string.Format(Settings.ExchangeOALName, companyCode);
            string gal = string.Format(Settings.ExchangeGALName, companyCode);

            logger.DebugFormat("Creating new offline address list '{0}' for {1}", name, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-OfflineAddressBook");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("AddressLists", gal);
            cmd.AddParameter("GlobalWebDistributionEnabled", true);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public void Remove_OfflineAddressBook(string name)
        {
            logger.DebugFormat("Attempting to remove offline address list {0}", name);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-OfflineAddressBook");
            cmd.AddParameter("Identity", name);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        public string New_AddressBookPolicy(string companyCode)
        {
            string name = string.Format(Settings.ExchangeABPName, companyCode);

            logger.DebugFormat("Creating new address book policy '{0}' for {1}", name, companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-AddressBookPolicy");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("GlobalAddressList", string.Format(Settings.ExchangeGALName, companyCode));
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, companyCode));
            cmd.AddParameter("RoomList", string.Format(Settings.ExchangeROOMSName, companyCode));
            cmd.AddParameter("AddressLists", new string[] {
                                                string.Format(Settings.ExchangeCONTACTSName, companyCode),
                                                string.Format(Settings.ExchangeGROUPSName, companyCode),
                                                string.Format(Settings.ExchangeUSERSName, companyCode)
                                             });
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            return name;
        }

        public void Remove_AddressBookPolicy(string name)
        {
            logger.DebugFormat("Attempting to remove address book policy {0}", name);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-AddressBookPolicy");
            cmd.AddParameter("Identity", name);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        #endregion

        #region Contacts

        public Contacts New_MailContact(Contacts mailContact, string defaultDomain, string organizationalUnit)
        {
            string[] emailSplit = mailContact.Email.Split('@');
            string primarySmtpAddress = string.Format("{0}{{at}}{1}.{{contact}}@{2}", emailSplit[0], emailSplit[1], defaultDomain);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-MailContact");
            cmd.AddParameter("Name", mailContact.Email);
            cmd.AddParameter("PrimarySmtpAddress", primarySmtpAddress);
            cmd.AddParameter("ExternalEmailAddress", mailContact.Email);
            cmd.AddParameter("OrganizationalUnit", organizationalUnit);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                // Get the distinguished name of the new object
                foreach (var o in psObjects)
                {
                    if (o.Properties["DistinguishedName"] != null)
                    {
                        mailContact.DistinguishedName = o.Properties["DistinguishedName"].Value.ToString();
                        break;
                    }
                }

                cmd.Clear();
                cmd.AddCommand("Set-MailContact");
                cmd.AddParameter("Identity", mailContact.DistinguishedName);
                cmd.AddParameter("CustomAttribute1", mailContact.CompanyCode);
                cmd.AddParameter("HiddenFromAddressListsEnabled", mailContact.Hidden);
                cmd.AddParameter("EmailAddressPolicyEnabled", false);
                cmd.AddParameter("DomainController", this._domainController);
                _powershell.Commands = cmd;
                _powershell.Invoke();

                HandleErrors();
            }

            return mailContact;
        }

        public Contacts Update_MailContact(Contacts mailContact)
        {
            logger.DebugFormat("Updating mail contact {0} with values: {1}, {2}, {3}", mailContact.DistinguishedName, mailContact.DisplayName, mailContact.CompanyCode, mailContact.Hidden);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-MailContact");
            cmd.AddParameter("Identity", mailContact.DistinguishedName);
            cmd.AddParameter("Name", mailContact.Email);
            cmd.AddParameter("DisplayName", mailContact.DisplayName);
            cmd.AddParameter("HiddenFromAddressListsEnabled", mailContact.Hidden);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();

            // Handle any errors first
            HandleErrors();

            foreach (var o in psObjects)
            {
                if (o.Properties["DistinguishedName"] != null)
                    mailContact.DistinguishedName = o.Properties["DistinguishedName"].Value.ToString();
            }
            return mailContact;
        }

        public void Remove_MailContact(string distinguishedName)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-MailContact");
            cmd.AddParameter("Identity", distinguishedName);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        public void Remove_AllContacts(string companyCode)
        {
            logger.DebugFormat("Removing all mail contacts where CustomAttribute1 equals {0}", companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-MailContact");
            cmd.AddParameter("Filter", string.Format("CustomAttribute1 -eq \"{0}\"", companyCode));
            cmd.AddParameter("ResultSize", "Unlimited");
            cmd.AddCommand("Remove-MailContact");
            cmd.AddParameter("Confirm", false);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion

        #region Domains

        public void New_AcceptedDomain(Domains newDomain)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-AcceptedDomain");
            cmd.AddParameter("Name", newDomain.Domain);
            cmd.AddParameter("DomainName", newDomain.Domain);

            switch (newDomain.DomainType)
            {
                case 2:
                    cmd.AddParameter("DomainType", "InternalRelay");
                    break;
                case 3:
                    cmd.AddParameter("DomainType", "ExternalRelay");
                    break;
                default:
                    cmd.AddParameter("DomainType", "Authoritative");
                    break;
            }

            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            logger.InfoFormat("Created new accepted domain {0}", newDomain.Domain);
        }

        public void Update_AcceptedDomain(Domains updateDomain)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-AcceptedDomain");
            cmd.AddParameter("Identity", updateDomain.Domain);

            switch (updateDomain.DomainType)
            {
                case 2:
                    cmd.AddParameter("DomainType", "InternalRelay");
                    break;
                case 3:
                    cmd.AddParameter("DomainType", "ExternalRelay");
                    break;
                default:
                    cmd.AddParameter("DomainType", "Authoritative");
                    break;
            }

            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

            logger.DebugFormat("Removed accepted domain {0}", updateDomain.Domain);
        }

        public void Remove_AcceptedDomain(Domains deleteDomain)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-AcceptedDomain");
            cmd.AddParameter("Identity", deleteDomain.Domain);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);

            logger.InfoFormat("Removed accepted domain {0}", deleteDomain.Domain);
        }

        #endregion

        #region Groups

        public DistributionGroups New_DistributionGroup(DistributionGroups group, string organizationalUnit)
        {
            logger.DebugFormat("Creating new distribution group {0} for {1}", group.DisplayName, group.CompanyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-DistributionGroup");
            cmd.AddParameter("Name", group.DisplayName);
            cmd.AddParameter("DisplayName", group.DisplayName);
            cmd.AddParameter("ManagedBy", group.ManagedByAdded);
            cmd.AddParameter("Members", group.MembersAdded);
            cmd.AddParameter("ModeratedBy", group.ModeratedBy);
            cmd.AddParameter("ModerationEnabled", group.ModerationEnabled);
            cmd.AddParameter("OrganizationalUnit", organizationalUnit);
            cmd.AddParameter("PrimarySmtpAddress", group.Email);
            cmd.AddParameter("DomainController", this._domainController);

            switch (group.MemberDepartRestriction)
            {
                case ExchangeValues.Closed:
                    cmd.AddParameter("MemberDepartRestriction", "Closed");
                    break;
                case ExchangeValues.ApprovalRequired:
                    cmd.AddParameter("MemberDepartRestriction", "ApprovalRequired");
                    break;
                default:
                    cmd.AddParameter("MemberDepartRestriction", "Open");
                    break;
            }

            switch (group.MemberJoinRestriction)
            {
                case ExchangeValues.Closed:
                    cmd.AddParameter("MemberJoinRestriction", "Closed");
                    break;
                case ExchangeValues.ApprovalRequired:
                    cmd.AddParameter("MemberJoinRestriction", "ApprovalRequired");
                    break;
                default:
                    cmd.AddParameter("MemberJoinRestriction", "Open");
                    break;
            }

            switch (group.SendModerationNotifications)
            {
                case ExchangeValues.Always:
                    cmd.AddParameter("SendModerationNotifications", "Always");
                    break;
                case ExchangeValues.Internal:
                    cmd.AddParameter("SendModerationNotifications", "Internal");
                    break;
                default:
                    cmd.AddParameter("SendModerationNotifications", "Never");
                    break;
            }
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                // Get the distinguished name of the new object
                foreach (var o in psObjects)
                {
                    if (o.Properties["DistinguishedName"] != null)
                    {
                        group.DistinguishedName = o.Properties["DistinguishedName"].Value.ToString();
                        break;
                    }
                }

                cmd.Clear();
                cmd.AddCommand("Set-DistributionGroup");
                cmd.AddParameter("Identity", group.DistinguishedName);
                cmd.AddParameter("CustomAttribute1", group.CompanyCode);
                cmd.AddParameter("HiddenFromAddressListsEnabled", group.Hidden);
                cmd.AddParameter("DomainController", this._domainController);
                _powershell.Commands = cmd;
                _powershell.Invoke();

                HandleErrors();
            }

            return group;
        }

        public DistributionGroups Get_DistributionGroup(string identity)
        {
            logger.DebugFormat("Retrieving distribution group {0}", identity);
            var returnGroup = new DistributionGroups();

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-DistributionGroup");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found group {0}. Retrieving values...", identity);

                // Get the distinguished name of the new object
                var foundGroup = psObjects[0];
                returnGroup.DisplayName = foundGroup.Properties["DisplayName"].Value.ToString();
                returnGroup.DistinguishedName = foundGroup.Properties["DistinguishedName"].Value.ToString();
                returnGroup.CompanyCode = foundGroup.Properties["CustomAttribute1"].Value.ToString();
                returnGroup.Email = foundGroup.Properties["PrimarySmtpAddress"].Value.ToString();
                returnGroup.ModerationEnabled = (bool)foundGroup.Properties["ModerationEnabled"].Value;
                returnGroup.Hidden = (bool)foundGroup.Properties["HiddenFromAddressListsEnabled"].Value;
                
                logger.DebugFormat("Checking if authentication is required to send to the group");
                bool requireAuthenticatedSenders = false;
                bool.TryParse(foundGroup.Properties["RequireSenderAuthenticationEnabled"].Value.ToString(), out requireAuthenticatedSenders);
                returnGroup.RequireSenderAuthenticationEnabled = requireAuthenticatedSenders ? ExchangeValues.Inside : ExchangeValues.InsideAndOutside;

                logger.DebugFormat("Getting the join restriction");
                int memberJoinRestriction = ExchangeValues.Open;
                if (foundGroup.Properties["MemberJoinRestriction"].Value.ToString() == "Closed")
                    memberJoinRestriction = ExchangeValues.Closed;
                else if (foundGroup.Properties["MemberJoinRestriction"].Value.ToString() == "ApprovalRequired")
                    memberJoinRestriction = ExchangeValues.ApprovalRequired;
                returnGroup.MemberJoinRestriction = memberJoinRestriction;

                logger.DebugFormat("Getting the departure restriction");
                int memberDepartRestriction = ExchangeValues.Open;
                if (foundGroup.Properties["MemberDepartRestriction"].Value.ToString() == "Closed")
                    memberDepartRestriction = ExchangeValues.Closed;
                else if (foundGroup.Properties["MemberDepartRestriction"].Value.ToString() == "ApprovalRequired")
                    memberDepartRestriction = ExchangeValues.ApprovalRequired;
                returnGroup.MemberDepartRestriction = memberDepartRestriction;

                logger.DebugFormat("Getting the ManagedBy list");
                if (foundGroup.Properties["ManagedBy"] != null)
                {
                    var multiValue = foundGroup.Properties["ManagedBy"].Value as PSObject;
                    var owners = multiValue.BaseObject as ArrayList;
                    returnGroup.ManagedByOriginal = owners.ToArray(typeof(string)) as string[];
                }

                logger.DebugFormat("Checking the AcceptMessagesOnlyFromSendersOrMembers values");
                if (foundGroup.Properties["AcceptMessagesOnlyFromSendersOrMembers"] != null)
                {
                    returnGroup.AcceptMessagesOnlyFromSendersOrMembers = foundGroup.Properties["AcceptMessagesOnlyFromSendersOrMembers"].Value.ToString().Split(',');
                }

                logger.DebugFormat("Checking the ModeratedBy values");
                if (foundGroup.Properties["ModeratedBy"] != null)
                {
                    returnGroup.ModeratedBy = foundGroup.Properties["ModeratedBy"].Value.ToString().Split(',');
                }

                logger.DebugFormat("Checking the BypassModerationFromSendersOrMembers values");
                if (foundGroup.Properties["BypassModerationFromSendersOrMembers"] != null)
                {
                    returnGroup.BypassModerationFromSendersOrMembers = foundGroup.Properties["BypassModerationFromSendersOrMembers"].Value.ToString().Split(',');
                }

                // Get members
                //returnGroup.MembersOriginalObject = Get_DistributionGroupMembers(identity);
                //returnGroup.MembersOriginal = returnGroup.MembersOriginalObject.Select(x => x.DistinguishedName).ToArray();

                return returnGroup;
            }
        }

        public List<ExchangeGroupSelectors> Get_DistributionGroupMembers(string identity)
        {
            var returnObject = new List<ExchangeGroupSelectors>();

            logger.DebugFormat("Getting distribution group members for {0}", identity);
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-DistributionGroupMember");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("ResultSize", "Unlimited");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var results = _powershell.Invoke();
            foreach (var r in results)
            {
                string recipientType = r.Properties["RecipientType"].Value.ToString().ToLower();

                var newObject = new ExchangeGroupSelectors();
                newObject.Name = r.Properties["DisplayName"].Value.ToString();
                newObject.Email = r.Properties["PrimarySmtpAddress"].Value.ToString();
                newObject.DistinguishedName = r.Properties["DistinguishedName"].Value.ToString();

                if (recipientType.Contains("distributiongroup"))
                    newObject.IdValue = ExchangeValues.Group;
                else if (recipientType.Contains("contact"))
                    newObject.IdValue = ExchangeValues.Contact;
                else
                    newObject.IdValue = ExchangeValues.User;

                returnObject.Add(newObject);
            }

            HandleErrors();

            return returnObject;
        }

        public void Remove_AllGroups(string companyCode)
        {
            logger.DebugFormat("Removing all distribution groups where CustomAttribute1 equals {0}", companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-DistributionGroup");
            cmd.AddParameter("Filter", string.Format("CustomAttribute1 -eq \"{0}\"", companyCode));
            cmd.AddParameter("ResultSize", "Unlimited");
            cmd.AddCommand("Remove-DistributionGroup");
            cmd.AddParameter("Confirm", false);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion

        #region Mailboxes

        public void Enable_Mailbox(string userPrincipalName, string companyCode, EmailInfo emailInfo, Plans_ExchangeMailbox p)
        {
            logger.DebugFormat("Enabling mailbox for {0}", userPrincipalName);

            int sizeInMB = 0;
            if (p.MailboxSizeMB > 0)
            {
                // If mailbox size for plan is greater than 0 then its not unlimited
                if (p.MaxMailboxSizeMB != null && emailInfo.SizeInMB > p.MaxMailboxSizeMB)
                    sizeInMB = (int)p.MaxMailboxSizeMB;

                if (p.MaxMailboxSizeMB == null && emailInfo.SizeInMB > p.MailboxSizeMB)
                    sizeInMB = p.MailboxSizeMB;
            }

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Enable-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("AddressBookPolicy", string.Format(Settings.ExchangeABPName, companyCode));
            cmd.AddParameter("PrimarySmtpAddress", string.Format("{0}@{1}", emailInfo.EmailFirst, emailInfo.EmailDomain));
            cmd.AddParameter("Alias", string.Format("{0}_{1}", emailInfo.EmailFirst, emailInfo.EmailDomain));
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Continuing to Set-Mailbox...");
            cmd.AddStatement();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("IssueWarningQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB * 0.90) : "Unlimited");
            cmd.AddParameter("MaxReceiveSize", p.MaxReceiveKB > 0 ? string.Format("{0}KB", p.MaxReceiveKB) : "Unlimited");
            cmd.AddParameter("MaxSendSize", p.MaxSendKB > 0 ? string.Format("{0}KB", p.MaxSendKB) : "Unlimited");
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, companyCode));
            cmd.AddParameter("ProhibitSendQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("ProhibitSendReceiveQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("RecipientLimits", p.MaxRecipients > 0 ? string.Format("{0}", p.MaxRecipients) : "Unlimited");
            cmd.AddParameter("RetainDeletedItemsFor", p.MaxKeepDeletedItems > 0 ? p.MaxKeepDeletedItems : 30);
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("CustomAttribute1", companyCode);
            //cmd.AddParameter("RoleAssignmentPolicy", "Alternate Assignment Policy");
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Continuing to Set-CASMailbox...");
            cmd.AddStatement();
            cmd.AddCommand("Set-CASMailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("ActiveSyncEnabled", p.EnableAS);
            cmd.AddParameter("ECPEnabled", p.EnableECP);
            cmd.AddParameter("ImapEnabled", p.EnableIMAP);
            cmd.AddParameter("MAPIEnabled", p.EnableMAPI);
            cmd.AddParameter("OWAEnabled", p.EnableOWA);
            cmd.AddParameter("PopEnabled", p.EnablePOP3);
            //cmd.AddParameter("ActiveSyncMailboxPolicy", "");
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Executing powershell commands...");
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        public void Disable_Mailbox(string userPrincipalName)
        {
            logger.DebugFormat("Removing mailbox {0}", userPrincipalName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Disable-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        public void Remove_AllMailboxes(string companyCode)
        {
            logger.DebugFormat("Removing all mailboxes where CustomAttribute1 equals {0}", companyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-Mailbox");
            cmd.AddParameter("Filter", string.Format("CustomAttribute1 -eq \"{0}\"", companyCode));
            cmd.AddParameter("ResultSize", "Unlimited");
            cmd.AddParameter("DomainController", this._domainController);
            cmd.AddCommand("Disable-Mailbox");
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion
    }
}
