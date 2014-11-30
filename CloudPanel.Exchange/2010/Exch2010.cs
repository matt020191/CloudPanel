﻿using CloudPanel.Base.Config;
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
// 2. Redistributions in binary form must reproduce the above copyrighft
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement
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
using System.Globalization;
using System.Linq;
using System.Management.Automation;

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
            cmd.AddParameter("ModerationEnabled", group.ModerationEnabled);
            cmd.AddParameter("OrganizationalUnit", organizationalUnit);
            cmd.AddParameter("PrimarySmtpAddress", group.Email);
            cmd.AddParameter("DomainController", this._domainController);

            if (group.ModerationEnabled)
            {
                cmd.AddParameter("ModeratedBy", group.ModeratedBy);
            }

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

                bool requireSenderAuth = true;
                if (group.RequireSenderAuthenticationEnabled == ExchangeValues.InsideAndOutside)
                    requireSenderAuth = false;
                cmd.AddParameter("RequireSenderAuthenticationEnabled", requireSenderAuth);

                if (group.ModerationEnabled)
                {
                    if (group.BypassModerationFromSendersOrMembers != null && group.BypassModerationFromSendersOrMembers.Length > 0)
                    {
                        cmd.AddParameter("BypassModerationFromSendersOrMembers", group.BypassModerationFromSendersOrMembers);
                    }
                }

                if (group.AcceptMessagesOnlyFromSendersOrMembers != null && group.AcceptMessagesOnlyFromSendersOrMembers.Length > 0)
                {
                    cmd.AddParameter("AcceptMessagesOnlyFromSendersOrMembers", group.AcceptMessagesOnlyFromSendersOrMembers);
                }

                _powershell.Commands = cmd;
                _powershell.Invoke();

                HandleErrors();
            }

            return group;
        }

        public DistributionGroups Update_DistributionGroup(DistributionGroups group, string originalIdentity)
        {
            logger.DebugFormat("Updating distribution group {0} for {1}", group.DisplayName, group.CompanyCode);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-DistributionGroup");
            cmd.AddParameter("Identity", originalIdentity);
            cmd.AddParameter("DisplayName", group.DisplayName);
            cmd.AddParameter("ManagedBy", group.ManagedByAdded);
            cmd.AddParameter("ModeratedBy", group.ModeratedBy);
            cmd.AddParameter("ModerationEnabled", group.ModerationEnabled);
            cmd.AddParameter("PrimarySmtpAddress", group.Email);
            cmd.AddParameter("HiddenFromAddressListsEnabled", group.Hidden);
            cmd.AddParameter("AcceptMessagesOnlyFromSendersOrMembers", group.AcceptMessagesOnlyFromSendersOrMembers);
            cmd.AddParameter("DomainController", this._domainController);
            cmd.AddParameter("BypassSecurityGroupManagerCheck");

            bool requireSenderAuth = true;
            if (group.RequireSenderAuthenticationEnabled == ExchangeValues.InsideAndOutside)
                requireSenderAuth = false;
            cmd.AddParameter("RequireSenderAuthenticationEnabled", requireSenderAuth);

            if (group.BypassModerationFromSendersOrMembers != null && group.BypassModerationFromSendersOrMembers.Length > 0)
            {
                cmd.AddParameter("BypassModerationFromSendersOrMembers", group.BypassModerationFromSendersOrMembers);
            }
            else
                cmd.AddParameter("BypassModerationFromSendersOrMembers", null);

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
            _powershell.Invoke();

            HandleErrors();

            logger.DebugFormat("Processing members to add");
            if (group.MembersAdded != null && group.MembersAdded.Length > 0)
            {
                foreach (var i in group.MembersAdded)
                {
                    Add_DistributionGroupMembers(group.Email, i);
                }
            }

            logger.DebugFormat("Processing members to remove");
            if (group.MembersRemoved != null && group.MembersRemoved.Length > 0)
            {
                foreach (var i in group.MembersRemoved)
                {
                    Remove_DistributionGroupMembers(group.Email, i);
                }
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
                returnGroup.RequireSenderAuthenticationEnabled = foundGroup.Properties["RequireSenderAuthenticationEnabled"].Value.Equals(true) ? ExchangeValues.Inside : ExchangeValues.InsideAndOutside;

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

                logger.DebugFormat("Getting moderation notifications");
                int sendModerationNotifications = ExchangeValues.Never;
                if (foundGroup.Properties["SendModerationNotifications"].Value.ToString() == "Always")
                    sendModerationNotifications = ExchangeValues.Always;
                else if (foundGroup.Properties["SendModerationNotifications"].Value.ToString() == "Internal")
                    sendModerationNotifications = ExchangeValues.Internal;
                returnGroup.SendModerationNotifications = sendModerationNotifications;

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

        public void Remove_DistributionGroup(string identity)
        {
            logger.InfoFormat("Removing group {0}", identity);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-DistributionGroup");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true, false);
        }

        public List<GroupObjectSelector> Get_DistributionGroupMembers(string identity)
        {
            var returnObject = new List<GroupObjectSelector>();

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

                var newObject = new GroupObjectSelector();
                newObject.DisplayName = r.Properties["DisplayName"].Value.ToString();
                newObject.Email = r.Properties["PrimarySmtpAddress"].Value.ToString();
                newObject.Identifier = newObject.Email;

                if (recipientType.Contains("distributiongroup"))
                    newObject.ObjectType = ExchangeValues.Group;
                else if (recipientType.Contains("contact"))
                    newObject.ObjectType = ExchangeValues.Contact;
                else
                    newObject.ObjectType = ExchangeValues.User;

                returnObject.Add(newObject);
            }

            HandleErrors();

            return returnObject;
        }

        public void Add_DistributionGroupMembers(string groupIdentity, string addIdentity)
        {
            logger.DebugFormat("Adding {0} to group {1}", addIdentity, groupIdentity);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Add-DistributionGroupMember");
            cmd.AddParameter("Identity", groupIdentity);
            cmd.AddParameter("Member", addIdentity);
            cmd.AddParameter("BypassSecurityGroupManagerCheck");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(false, true);
        }

        public void Remove_DistributionGroupMembers(string groupIdentity, string removeIdentity)
        {
            logger.DebugFormat("Removing {0} from group {1}", removeIdentity, groupIdentity);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-DistributionGroupMember");
            cmd.AddParameter("Identity", groupIdentity);
            cmd.AddParameter("Member", removeIdentity);
            cmd.AddParameter("BypassSecurityGroupManagerCheck");
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
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

        public void Enable_Mailbox(Users user)
        {
            #region Required data
            if (string.IsNullOrEmpty(user.UserPrincipalName))
                throw new MissingFieldException("Users", "UserPrincipalName");

            if (string.IsNullOrEmpty(user.CompanyCode))
                throw new MissingFieldException("Users", "CompanyCode");

            if (string.IsNullOrEmpty(user.Email))
                throw new MissingFieldException("Users", "Email");
            #endregion

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Enable-Mailbox");
            cmd.AddParameter("Identity", user.UserPrincipalName);
            cmd.AddParameter("AddressBookPolicy", string.Format(Settings.ExchangeABPName, user.CompanyCode));
            cmd.AddParameter("PrimarySmtpAddress", user.Email);
            cmd.AddParameter("Alias", string.Format("{0}_{1}", user.EmailFirst, user.EmailDomain));
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Executing powershell commands...");
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Enable an Exchange mailbox
        /// </summary>
        /// <param name="user">User object containing all the required variables</param>
        /// <param name="p">The plans object containing the plan to apply to the user</param>
        /// <param name="emailAddresses">List of email addresses including the primary and aliases</param>
        public void Enable_Mailbox(Users user, Plans_ExchangeMailbox p, string[] emailAddresses)
        {
            logger.DebugFormat("Enabling mailbox for {0}", user.UserPrincipalName);

            #region Required data
            if (string.IsNullOrEmpty(user.UserPrincipalName))
                throw new MissingFieldException("Users", "UserPrincipalName");

            if (string.IsNullOrEmpty(user.CompanyCode))
                throw new MissingFieldException("Users", "CompanyCode");

            if (string.IsNullOrEmpty(user.Email))
                throw new MissingFieldException("Users", "Email");

            if (p == null)
                throw new ArgumentNullException("Plans_ExchangeMailbox");
            #endregion

            int sizeInMB = 0;
            if (p.MailboxSizeMB > 0)
            {
                // If mailbox size for plan is greater than 0 then its not unlimited
                if (p.MaxMailboxSizeMB != null && user.SizeInMB > p.MaxMailboxSizeMB)
                    sizeInMB = (int)p.MaxMailboxSizeMB;

                if (p.MaxMailboxSizeMB == null && user.SizeInMB > p.MailboxSizeMB)
                    sizeInMB = p.MailboxSizeMB;
            }

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Enable-Mailbox");
            cmd.AddParameter("Identity", user.UserPrincipalName);
            cmd.AddParameter("AddressBookPolicy", string.Format(Settings.ExchangeABPName, user.CompanyCode));
            cmd.AddParameter("PrimarySmtpAddress", user.Email);
            cmd.AddParameter("Alias", string.Format("{0}_{1}", user.EmailFirst, user.EmailDomain));
            cmd.AddParameter("DomainController", this._domainController);

            // Setting Mailbox
            logger.DebugFormat("Continuing to Set-Mailbox...");
            cmd.AddStatement();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", user.UserPrincipalName);
            cmd.AddParameter("EmailAddresses", emailAddresses);
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("IssueWarningQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB * 0.90) : "Unlimited");
            cmd.AddParameter("MaxReceiveSize", p.MaxReceiveKB > 0 ? string.Format("{0}KB", p.MaxReceiveKB) : "Unlimited");
            cmd.AddParameter("MaxSendSize", p.MaxSendKB > 0 ? string.Format("{0}KB", p.MaxSendKB) : "Unlimited");
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, user.CompanyCode));
            cmd.AddParameter("ProhibitSendQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("ProhibitSendReceiveQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("RecipientLimits", p.MaxRecipients > 0 ? string.Format("{0}", p.MaxRecipients) : "Unlimited");
            cmd.AddParameter("RetainDeletedItemsFor", p.MaxKeepDeletedItems > 0 ? p.MaxKeepDeletedItems : 30);
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("CustomAttribute1", user.CompanyCode);
            cmd.AddParameter("GrantSendOnBehalfTo", (user.EmailSendOnBehalf == null || user.EmailSendOnBehalf.Length == 0) ? null : user.EmailSendOnBehalf);
            cmd.AddParameter("RoleAssignmentPolicy", Settings.ExchangeRoleAssignment);
            cmd.AddParameter("DomainController", this._domainController);

            // CAS Mailbox
            logger.DebugFormat("Continuing to Set-CASMailbox...");
            cmd.AddStatement();
            cmd.AddCommand("Set-CASMailbox");
            cmd.AddParameter("Identity", user.UserPrincipalName);
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

        /// <summary>
        /// Updates an Exchange mailbox
        /// </summary>
        /// <param name="user">User object containing all the required variables</param>
        /// <param name="p">The plans object containing the plan to apply to the user</param>
        /// <param name="emailAddresses">List of email addresses including the primary and aliases</param>
        public void Set_Mailbox(Users user, Plans_ExchangeMailbox p, string[] emailAddresses)
        {
            logger.DebugFormat("Updating mailbox for {0}", user.UserPrincipalName);

            #region Required data
            if (string.IsNullOrEmpty(user.UserPrincipalName))
                throw new MissingFieldException("Users", "UserPrincipalName");

            if (string.IsNullOrEmpty(user.CompanyCode))
                throw new MissingFieldException("Users", "CompanyCode");

            if (string.IsNullOrEmpty(user.Email))
                throw new MissingFieldException("Users", "Email");

            if (p == null)
                throw new ArgumentNullException("Plans_ExchangeMailbox");
            #endregion

            int sizeInMB = 0;
            if (p.MailboxSizeMB > 0)
            {
                // If mailbox size for plan is greater than 0 then its not unlimited
                if (p.MaxMailboxSizeMB != null && user.SizeInMB > p.MaxMailboxSizeMB)
                    sizeInMB = (int)p.MaxMailboxSizeMB;

                if (p.MaxMailboxSizeMB == null && user.SizeInMB > p.MailboxSizeMB)
                    sizeInMB = p.MailboxSizeMB;
            }

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", user.UserPrincipalName);
            cmd.AddParameter("EmailAddresses", emailAddresses);
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("IssueWarningQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB * 0.90) : "Unlimited");
            cmd.AddParameter("MaxReceiveSize", p.MaxReceiveKB > 0 ? string.Format("{0}KB", p.MaxReceiveKB) : "Unlimited");
            cmd.AddParameter("MaxSendSize", p.MaxSendKB > 0 ? string.Format("{0}KB", p.MaxSendKB) : "Unlimited");
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, user.CompanyCode));
            cmd.AddParameter("ProhibitSendQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("ProhibitSendReceiveQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("RecipientLimits", p.MaxRecipients > 0 ? string.Format("{0}", p.MaxRecipients) : "Unlimited");
            cmd.AddParameter("RetainDeletedItemsFor", p.MaxKeepDeletedItems > 0 ? p.MaxKeepDeletedItems : 30);
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("CustomAttribute1", user.CompanyCode);
            cmd.AddParameter("GrantSendOnBehalfTo", (user.EmailSendOnBehalf == null || user.EmailSendOnBehalf.Length == 0) ? null : user.EmailSendOnBehalf);
            cmd.AddParameter("RoleAssignmentPolicy", Settings.ExchangeRoleAssignment);
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Executing powershell commands...");
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Updates the CAS mailbox settings
        /// </summary>
        /// <param name="user"></param>
        /// <param name="p"></param>
        public void Set_CASMailbox(string userPrincipalName, Plans_ExchangeMailbox p)
        {
            logger.DebugFormat("Updating CAS mailbox for {0}", userPrincipalName);

            #region Required data
            if (string.IsNullOrEmpty(userPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (p == null)
                throw new ArgumentNullException("Plans_ExchangeMailbox");
            #endregion

            PSCommand cmd = new PSCommand();
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

        /// <summary>
        /// Updates an Exchange mailbox litigation hold information
        /// </summary>
        /// <param name="userPrincipalName">UPN of the user to update</param>
        /// <param name="litigationHoldEnabled">If litigation hold is enabled or not</param>
        /// <param name="retentionUrl">The URL for information on the hold (Displayed to user)</param>
        /// <param name="retentionComment">The comment for the information on the hold (Displayed to user)</param>
        public void Set_LitigationHold(string userPrincipalName, bool litigationHoldEnabled, string retentionUrl = "", string retentionComment = "")
        {
            logger.DebugFormat("Updating litigation hold information for {0}", userPrincipalName);
            logger.DebugFormat("Litigation hold values are {0}, {1}, {2}, {3}", userPrincipalName, litigationHoldEnabled, retentionUrl, retentionComment);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("LitigationHoldEnabled", litigationHoldEnabled);

            if (litigationHoldEnabled)
            {
                if (!string.IsNullOrEmpty(retentionComment))
                    cmd.AddParameter("RetentionComment", retentionComment);

                if (!string.IsNullOrEmpty(retentionUrl))
                    cmd.AddParameter("RetentionUrl", retentionUrl);
            }
            else
            {
                cmd.AddParameter("RetentionComment", null);
                cmd.AddParameter("RetentionUrl", null);
            }

            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Disables an Exchange mailbox
        /// </summary>
        /// <param name="userPrincipalName">UserPrincipalName of the user to disable</param>
        public void Disable_Mailbox(string userPrincipalName, bool ignoreNotFound = false)
        {
            logger.DebugFormat("Removing mailbox {0}", userPrincipalName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Disable-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(ignoreNotFound, false);
        }

        /// <summary>
        /// Disables all Exchange mailboxes for a specific company
        /// </summary>
        /// <param name="companyCode"></param>
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

        /// <summary>
        /// Gets a specific mailbox
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Users Get_Mailbox(Users user)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-Mailbox");
            cmd.AddParameter("Identity", user.UserPrincipalName);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found mailbox {0}", user.UserPrincipalName);

                var foundUser = psObjects[0];
                user.Email = foundUser.Properties["PrimarySmtpAddress"].Value.ToString();
                user.DistinguishedName = foundUser.Properties["DistinguishedName"].Value.ToString();

                logger.DebugFormat("Parsing email aliases...");
                var parsedAliases = new List<string>();
                var emailAliasesValue = foundUser.Properties["EmailAddresses"].Value as PSObject;
                var emailAliases = emailAliasesValue.BaseObject as ArrayList;
                foreach (var i in emailAliases)
                {
                    string e = i.ToString();
                    if (!e.StartsWith("SMTP:")) // Skip primary email
                    {
                        parsedAliases.Add(e.Replace("smtp:", ""));
                    }
                }
                user.EmailAliases = parsedAliases.ToArray();

                logger.DebugFormat("Retrieving litigation hold information");
                if (foundUser.Properties["LitigationHoldEnabled"].Value != null)
                    user.LitigationHoldEnabled = bool.Parse(foundUser.Properties["LitigationHoldEnabled"].Value.ToString());

                if (foundUser.Properties["LitigationHoldDate"].Value != null)
                    user.LitigationHoldDate = foundUser.Properties["LitigationHoldDate"].Value.ToString();

                if (foundUser.Properties["LitigationHoldOwner"].Value != null)
                    user.LitigationHoldOwner = foundUser.Properties["LitigationHoldOwner"].Value.ToString();

                if (foundUser.Properties["RetentionComment"].Value != null)
                    user.RetentionComment = foundUser.Properties["RetentionComment"].Value.ToString();

                if (foundUser.Properties["RetentionUrl"].Value != null)
                    user.RetentionUrl = foundUser.Properties["RetentionUrl"].Value.ToString();

                logger.DebugFormat("Parsing send on behalf permissions...");
                PSObject multiValue = (PSObject)foundUser.Properties["GrantSendOnBehalfTo"].Value;
                ArrayList accessRights = (ArrayList)multiValue.BaseObject;
                user.EmailSendOnBehalf = (string[])accessRights.ToArray(typeof(string));

                // Get full access permissions
                logger.DebugFormat("Retrieving full access permissions");
                user.EmailFullAccess = Get_FullAccessPermissions(user.UserPrincipalName);

                // Get send as permissions
                logger.DebugFormat("Retrieving send as permissions");
                user.EmailSendAs = Get_SendAsPermissions(user.DistinguishedName);

            }

            return user;
        }

        /// <summary>
        /// Adds full access permissions to a mailbox
        /// </summary>
        /// <param name="userPrincipalName">UPN of the user to add permissions to</param>
        /// <param name="toAdd">string array of who we are adding to have full access permissions</param>
        /// <param name="autoMapping">If we are automapping for who is added</param>
        public void Add_FullAccessPermissions(string userPrincipalName, string[] toAdd, bool autoMapping = true)
        {
            if (toAdd != null && toAdd.Length > 0)
            {
                foreach (var s in toAdd)
                {
                    PSCommand cmd = new PSCommand();
                    cmd.AddCommand("Add-MailboxPermission");
                    cmd.AddParameter("Identity", userPrincipalName);
                    cmd.AddParameter("User", s);
                    cmd.AddParameter("AccessRights", "FullAccess");
                    cmd.AddParameter("InheritanceType", "All");
                    cmd.AddParameter("Automapping", autoMapping); // If it should automatically show up in the user that has rights mailbox
                    cmd.AddParameter("Confirm", false);
                    cmd.AddParameter("DomainController", this._domainController);

                    _powershell.Commands = cmd;
                    _powershell.Invoke();

                    HandleErrors(true, true);
                }
            }
            else
                logger.DebugFormat("Adding full access permissions method was called but toAdd parameter was null or empty");
        }

        /// <summary>
        /// Adds send as permissions to a mailbox
        /// </summary>
        /// <param name="distinguishedName">DistinguishedName of the user we are adding send as permission to</param>
        /// <param name="toAdd">string array of who we are adding to have full access permissions</param>
        public void Add_SendAsPermissions(string distinguishedName, string[] toAdd)
        {
            if (toAdd != null && toAdd.Length > 0)
            {
                foreach (var s in toAdd)
                {
                    PSCommand cmd = new PSCommand();
                    cmd.AddCommand("Add-ADPermission");
                    cmd.AddParameter("Identity", distinguishedName);
                    cmd.AddParameter("User", s);
                    cmd.AddParameter("AccessRights", "ExtendedRight");
                    cmd.AddParameter("ExtendedRights", "Send As");
                    cmd.AddParameter("Confirm", false);
                    cmd.AddParameter("DomainController", this._domainController);

                    _powershell.Commands = cmd;
                    _powershell.Invoke();

                    HandleErrors(true, true);
                }
            }
            else
                logger.DebugFormat("Adding send as access permissions method was called but toAdd parameter was null or empty");
        }

        /// <summary>
        /// Removes full access permissions from a mailbox
        /// </summary>
        /// <param name="userPrincipalName"></param>
        /// <param name="toRemove"></param>
        public void Remove_FullAccessPermissions(string userPrincipalName, string[] toRemove)
        {
            if (toRemove != null && toRemove.Length > 0)
            {
                foreach (var s in toRemove)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        PSCommand cmd = new PSCommand();
                        cmd.AddCommand("Remove-MailboxPermission");
                        cmd.AddParameter("Identity", userPrincipalName);
                        cmd.AddParameter("User", s);
                        cmd.AddParameter("AccessRights", "FullAccess");
                        cmd.AddParameter("InheritanceType", "All");
                        cmd.AddParameter("Confirm", false);
                        cmd.AddParameter("DomainController", this._domainController);

                        _powershell.Commands = cmd;
                        _powershell.Invoke();

                        HandleErrors(true, true);
                    }
                }
            }
            else
                logger.DebugFormat("Removing full access permissions method was called but toRemove parameter was null or empty");
        }

        /// <summary>
        /// Removes send as permissions from a mailbox
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="toRemove"></param>
        public void Remove_SendAsPermissions(string distinguishedName, string[] toRemove)
        {
            if (toRemove != null && toRemove.Length > 0)
            {
                foreach (var s in toRemove)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        PSCommand cmd = new PSCommand();
                        cmd.AddCommand("Remove-ADPermission");
                        cmd.AddParameter("Identity", distinguishedName);
                        cmd.AddParameter("User", s);
                        cmd.AddParameter("AccessRights", "ExtendedRight");
                        cmd.AddParameter("ExtendedRights", "Send As");
                        cmd.AddParameter("Confirm", false);
                        cmd.AddParameter("DomainController", this._domainController);

                        _powershell.Commands = cmd;
                        _powershell.Invoke();

                        HandleErrors(true, true);
                    }
                }
            }
            else
                logger.DebugFormat("Removing send as access permissions method was called but toAdd parameter was null or empty");
        }

        /// <summary>
        /// Retrieves a string array of who currently has full access permissions
        /// </summary>
        /// <param name="userPrincipalName">UPN of the user to retrieve permissions</param>
        /// <returns></returns>
        public string[] Get_FullAccessPermissions(string userPrincipalName)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-MailboxPermission");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found mailbox {0}... retrieving full access permissions", userPrincipalName);
                var listAccounts = new List<string>();
                foreach (PSObject ps in _powershell.Invoke())
                {
                    if (ps.Members["AccessRights"].Value != null)
                    {
                        string inheritanceType = ps.Members["InheritanceType"].Value.ToString();
                        bool deny = bool.Parse(ps.Members["Deny"].Value.ToString());
                        bool isInherited = bool.Parse(ps.Members["IsInherited"].Value.ToString());

                        if (ps.Members["User"].Value.ToString().IndexOf("\\") != -1)
                        {
                            // Need to make sure the value contains a black slash because if not then it is not a valid sAMAccountName (Could be like S-1-5-32-554)
                            string samAccountName = ps.Members["User"].Value.ToString().Split('\\')[1];

                            // Get the permissions that this user has
                            PSObject multiValue = (PSObject)ps.Members["AccessRights"].Value;
                            ArrayList accessRights = (ArrayList)multiValue.BaseObject;
                            string[] strAccessRights = (string[])accessRights.ToArray(typeof(string));

                            // Only add if it contains "FullAccess"
                            if (strAccessRights.Contains("FullAccess"))
                                listAccounts.Add(samAccountName);
                        }
                    }
                }

                return listAccounts.ToArray();
            }
        }

        /// <summary>
        /// Retrieves a string array of who currently has send as permissions
        /// </summary>
        /// <param name="userPrincipalName"></param>
        /// <returns></returns>
        public string[] Get_SendAsPermissions(string userPrincipalName)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-ADPermission");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found mailbox {0}... retrieving send-as permissions", userPrincipalName);
                var listAccounts = new List<string>();
                foreach (PSObject ps in _powershell.Invoke())
                {
                    if (ps.Members["ExtendedRights"].Value != null)
                    {
                        string inheritanceType = ps.Members["InheritanceType"].Value.ToString();
                        bool deny = bool.Parse(ps.Members["Deny"].Value.ToString());
                        bool isInherited = bool.Parse(ps.Members["IsInherited"].Value.ToString());

                        if (ps.Members["User"].Value.ToString().IndexOf("\\") != -1)
                        {
                            // Need to make sure the value contains a black slash because if not then it is not a valid sAMAccountName (Could be like S-1-5-32-554)
                            string samAccountName = ps.Members["User"].Value.ToString().Split('\\')[1];

                            // Get the permissions that this user has
                            PSObject multiValue = (PSObject)ps.Members["ExtendedRights"].Value;
                            ArrayList accessRights = (ArrayList)multiValue.BaseObject;
                            string[] strAccessRights = (string[])accessRights.ToArray(typeof(string));

                            // Only add if it contains "FullAccess"
                            if (strAccessRights.Contains("Send-As"))
                                listAccounts.Add(samAccountName);
                        }
                    }
                }

                return listAccounts.ToArray();
            }
        }

        /// <summary>
        /// Gets a specific users mailbox size
        /// </summary>
        /// <param name="userPrincipalName"></param>
        /// <returns></returns>
        public SvcMailboxSizes Get_MailboxSize(string userPrincipalName)
        {
            logger.DebugFormat("Getting mailbox size for {0}", userPrincipalName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-MailboxStatistics");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            
            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                var returnSize = new SvcMailboxSizes();
                foreach (PSObject obj in psObjects)
                {
                    returnSize.UserPrincipalName = userPrincipalName;
                    returnSize.MailboxDatabase = obj.Members["Database"].Value.ToString();
                    returnSize.TotalItemSize = obj.Members["TotalItemSize"].Value.ToString();
                    returnSize.TotalItemSizeInBytes = GetExchangeBytes(returnSize.TotalItemSize);
                    returnSize.TotalDeletedItemSize = obj.Members["TotalDeletedItemSize"].Value.ToString();
                    returnSize.TotalDeletedItemSizeInBytes = GetExchangeBytes(returnSize.TotalDeletedItemSize);

                    int itemCount = 0;
                    int.TryParse(obj.Members["ItemCount"].Value.ToString(), out itemCount);
                    returnSize.ItemCount = itemCount;

                    int deletedItemCount = 0;
                    int.TryParse(obj.Members["DeletedItemCount"].Value.ToString(), out deletedItemCount);
                    returnSize.DeletedItemCount = deletedItemCount;

                    returnSize.Retrieved = DateTime.Now;
                    break;
                }

                logger.DebugFormat("Successfully retrieves mailbox statistics for {0}: {1}, {2}, {3}, {4}, {5}, {6}, {7}",
                    userPrincipalName, returnSize.MailboxDatabase, returnSize.TotalItemSize, returnSize.TotalItemSizeInBytes,
                    returnSize.TotalDeletedItemSize, returnSize.TotalDeletedItemSizeInBytes, returnSize.ItemCount, returnSize.DeletedItemCount);

                return returnSize;
            }
        }

        /// <summary>
        /// Gets all mailbox sizes for the list of userprincipalnames passed
        /// </summary>
        /// <param name="userPrincipalNames"></param>
        /// <returns></returns>
        public List<SvcMailboxSizes> Get_AllMailboxSizes(string[] userPrincipalNames)
        {
            logger.DebugFormat("Getting all mailbox sizes");

            var allSizes = new List<SvcMailboxSizes>();
            foreach (var user in userPrincipalNames)
            {
                logger.DebugFormat("Processing mailbox size for {0}", user);
                try
                {
                    var size = Get_MailboxSize(user);
                    allSizes.Add(size);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Failed to process mailbox size for {0}: {1}", user, ex.ToString());
                }
            }

            return allSizes;
        }

        #endregion

        #region Archive Mailbox

        public virtual void Enable_ArchiveMailbox(string userPrincipalName, string archiveName, string database)
        {
            logger.DebugFormat("Enabling archive mailbox for {0}", userPrincipalName);

            #region Required data
            if (string.IsNullOrEmpty(userPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");
            #endregion

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Enable-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Archive");

            if (!string.IsNullOrEmpty(archiveName))
                cmd.AddParameter("ArchiveName", archiveName);

            if (!string.IsNullOrEmpty(database))
                cmd.AddParameter("ArchiveDatabase", database);

            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        public virtual void Set_ArchiveMailbox(string userPrincipalName, int archiveSizeMB)
        {
            logger.DebugFormat("Settings archive mailbox for {0}", userPrincipalName);

            #region Required data
            if (string.IsNullOrEmpty(userPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");
            #endregion

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("ArchiveQuota", string.Format("{0}MB", archiveSizeMB));
            cmd.AddParameter("ArchiveWarningQuota", string.Format("{0}MB", archiveSizeMB * .95));
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();

        }

        public virtual void Disable_ArchiveMailbox(string userPrincipalName)
        {
            logger.DebugFormat("Disabling archive mailbox for {0}", userPrincipalName);

            #region Required data
            if (string.IsNullOrEmpty(userPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");
            #endregion

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Disable-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Archive");
            cmd.AddParameter("DomainController", this._domainController);
            cmd.AddParameter("Confirm", false);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        #endregion

        #region Resource Mailboxes

        public ResourceMailboxes Get_ResourceMailbox(string userPrincipalName)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found mailbox {0}", userPrincipalName);

                var returnMailbox = new ResourceMailboxes();
                var foundMailbox = psObjects[0];

                returnMailbox.UserPrincipalName = userPrincipalName;
                returnMailbox.PrimarySmtpAddress = foundMailbox.Properties["PrimarySmtpAddress"].Value.ToString();
                returnMailbox.DistinguishedName = foundMailbox.Properties["DistinguishedName"].Value.ToString();

                logger.DebugFormat("Parsing email aliases...");
                var parsedAliases = new List<string>();
                var emailAliasesValue = foundMailbox.Properties["EmailAddresses"].Value as PSObject;
                var emailAliases = emailAliasesValue.BaseObject as ArrayList;
                foreach (var i in emailAliases)
                {
                    string e = i.ToString();
                    if (!e.StartsWith("SMTP:")) // Skip primary email
                    {
                        parsedAliases.Add(e.Replace("smtp:", ""));
                    }
                }
                returnMailbox.EmailAliases = parsedAliases.ToArray();

                // Get full access permissions
                logger.DebugFormat("Retrieving full access permissions");
                returnMailbox.EmailFullAccess = Get_FullAccessPermissions(userPrincipalName);

                // Get send as permissions
                logger.DebugFormat("Retrieving send as permissions");
                returnMailbox.EmailSendAs = Get_SendAsPermissions(returnMailbox.DistinguishedName);

                return returnMailbox;
            }
        }

        #endregion

        #region Room Mailboxes

        /// <summary>
        /// Creates a new room mailbox
        /// </summary>
        /// <param name="resourceMailbox"></param>
        /// <param name="parentOrganizationalUnit"></param>
        public void New_RoomMailbox(ResourceMailboxes resourceMailbox, string parentOrganizationalUnit)
        {
            logger.DebugFormat("Creating room mailbox {0}", resourceMailbox.DisplayName);

            if (string.IsNullOrEmpty(resourceMailbox.DisplayName))
                throw new MissingFieldException("", "DisplayName");

            if (string.IsNullOrEmpty(resourceMailbox.UserPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (string.IsNullOrEmpty(resourceMailbox.PrimarySmtpAddress))
                throw new MissingFieldException("", "PrimarySmtpAddress");

            if (string.IsNullOrEmpty(parentOrganizationalUnit))
                throw new MissingFieldException("", "ParentOrganizationalUnit");

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-Mailbox");
            cmd.AddParameter("Name", resourceMailbox.DisplayName);
            cmd.AddParameter("DisplayName", resourceMailbox.DisplayName);
            cmd.AddParameter("UserPrincipalName", resourceMailbox.UserPrincipalName);
            cmd.AddParameter("PrimarySmtpAddress", resourceMailbox.PrimarySmtpAddress);
            cmd.AddParameter("OrganizationalUnit", string.Format("OU={0},{1}", Settings.ExchangeRoomsOU, parentOrganizationalUnit));
            cmd.AddParameter("Room");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Updates the room mailbox
        /// </summary>
        /// <param name="resourceMailbox"></param>
        /// <param name="p"></param>
        public void Set_RoomMailbox(ResourceMailboxes resourceMailbox, Plans_ExchangeMailbox p, string[] emailAddresses)
        {
            logger.DebugFormat("Updating room mailbox {0}", resourceMailbox.DisplayName);

            if (string.IsNullOrEmpty(resourceMailbox.UserPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (string.IsNullOrEmpty(resourceMailbox.CompanyCode))
                throw new MissingFieldException("", "CompanyCode");

            int sizeInMB = 0;
            if (p.MailboxSizeMB > 0)
            {
                // If mailbox size for plan is greater than 0 then its not unlimited
                if (p.MaxMailboxSizeMB != null && resourceMailbox.SizeInMB > p.MaxMailboxSizeMB)
                    sizeInMB = (int)p.MaxMailboxSizeMB;

                if (p.MaxMailboxSizeMB == null && resourceMailbox.SizeInMB > p.MailboxSizeMB)
                    sizeInMB = p.MailboxSizeMB;
            }

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", resourceMailbox.UserPrincipalName);
            cmd.AddParameter("CustomAttribute1", resourceMailbox.CompanyCode);
            cmd.AddParameter("EmailAddresses", emailAddresses);
            cmd.AddParameter("IssueWarningQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB * 0.90) : "Unlimited");
            cmd.AddParameter("MaxReceiveSize", p.MaxReceiveKB > 0 ? string.Format("{0}KB", p.MaxReceiveKB) : "Unlimited");
            cmd.AddParameter("MaxSendSize", p.MaxSendKB > 0 ? string.Format("{0}KB", p.MaxSendKB) : "Unlimited");
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, resourceMailbox.CompanyCode));
            cmd.AddParameter("ProhibitSendQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("ProhibitSendReceiveQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("RecipientLimits", p.MaxRecipients > 0 ? string.Format("{0}", p.MaxRecipients) : "Unlimited");
            cmd.AddParameter("RetainDeletedItemsFor", p.MaxKeepDeletedItems > 0 ? p.MaxKeepDeletedItems : 30);
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("RoleAssignmentPolicy", Settings.ExchangeRoleAssignment);
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Deletes the room mailbox and AD object
        /// </summary>
        /// <param name="userPrincipalName"></param>
        public void Remove_RoomMailbox(string userPrincipalName)
        {
            logger.DebugFormat("Removing room mailbox {0}", userPrincipalName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion

        #region Equipment Mailbox

        /// <summary>
        /// Creates a new equipment mailbox
        /// </summary>
        /// <param name="resourceMailbox"></param>
        /// <param name="parentOrganizationalUnit"></param>
        public void New_EquipmentMailbox(ResourceMailboxes resourceMailbox, string parentOrganizationalUnit)
        {
            logger.DebugFormat("Creating equipment mailbox {0}", resourceMailbox.DisplayName);

            if (string.IsNullOrEmpty(resourceMailbox.DisplayName))
                throw new MissingFieldException("", "DisplayName");

            if (string.IsNullOrEmpty(resourceMailbox.UserPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (string.IsNullOrEmpty(resourceMailbox.PrimarySmtpAddress))
                throw new MissingFieldException("", "PrimarySmtpAddress");

            if (string.IsNullOrEmpty(parentOrganizationalUnit))
                throw new MissingFieldException("", "ParentOrganizationalUnit");

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-Mailbox");
            cmd.AddParameter("Name", resourceMailbox.DisplayName);
            cmd.AddParameter("DisplayName", resourceMailbox.DisplayName);
            cmd.AddParameter("UserPrincipalName", resourceMailbox.UserPrincipalName);
            cmd.AddParameter("PrimarySmtpAddress", resourceMailbox.PrimarySmtpAddress);
            cmd.AddParameter("OrganizationalUnit", string.Format("OU={0},{1}", Settings.ExchangeResourceOU, parentOrganizationalUnit));
            cmd.AddParameter("Equipment");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Updates the equipment mailbox
        /// </summary>
        /// <param name="resourceMailbox"></param>
        /// <param name="p"></param>
        public void Set_EquipmentMailbox(ResourceMailboxes resourceMailbox, Plans_ExchangeMailbox p, string[] emailAddresses)
        {
            logger.DebugFormat("Updating equipment mailbox {0}", resourceMailbox.DisplayName);

            if (string.IsNullOrEmpty(resourceMailbox.UserPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (string.IsNullOrEmpty(resourceMailbox.CompanyCode))
                throw new MissingFieldException("", "CompanyCode");

            int sizeInMB = 0;
            if (p.MailboxSizeMB > 0)
            {
                // If mailbox size for plan is greater than 0 then its not unlimited
                if (p.MaxMailboxSizeMB != null && resourceMailbox.SizeInMB > p.MaxMailboxSizeMB)
                    sizeInMB = (int)p.MaxMailboxSizeMB;

                if (p.MaxMailboxSizeMB == null && resourceMailbox.SizeInMB > p.MailboxSizeMB)
                    sizeInMB = p.MailboxSizeMB;
            }

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", resourceMailbox.UserPrincipalName);
            cmd.AddParameter("CustomAttribute1", resourceMailbox.CompanyCode);
            cmd.AddParameter("EmailAddresses", emailAddresses);
            cmd.AddParameter("IssueWarningQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB * 0.90) : "Unlimited");
            cmd.AddParameter("MaxReceiveSize", p.MaxReceiveKB > 0 ? string.Format("{0}KB", p.MaxReceiveKB) : "Unlimited");
            cmd.AddParameter("MaxSendSize", p.MaxSendKB > 0 ? string.Format("{0}KB", p.MaxSendKB) : "Unlimited");
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, resourceMailbox.CompanyCode));
            cmd.AddParameter("ProhibitSendQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("ProhibitSendReceiveQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("RecipientLimits", p.MaxRecipients > 0 ? string.Format("{0}", p.MaxRecipients) : "Unlimited");
            cmd.AddParameter("RetainDeletedItemsFor", p.MaxKeepDeletedItems > 0 ? p.MaxKeepDeletedItems : 30);
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("RoleAssignmentPolicy", Settings.ExchangeRoleAssignment);
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Deletes the equipment mailbox and AD object
        /// </summary>
        /// <param name="userPrincipalName"></param>
        public void Remove_EquipmentMailbox(string userPrincipalName)
        {
            logger.DebugFormat("Removing equipment mailbox {0}", userPrincipalName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion

        #region Shared Mailbox

        /// <summary>
        /// Creates a new shared mailbox
        /// </summary>
        /// <param name="resourceMailbox"></param>
        /// <param name="parentOrganizationalUnit"></param>
        public void New_SharedMailbox(ResourceMailboxes resourceMailbox, string parentOrganizationalUnit)
        {
            logger.DebugFormat("Creating shared mailbox {0}", resourceMailbox.DisplayName);

            if (string.IsNullOrEmpty(resourceMailbox.DisplayName))
                throw new MissingFieldException("", "DisplayName");

            if (string.IsNullOrEmpty(resourceMailbox.UserPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (string.IsNullOrEmpty(resourceMailbox.PrimarySmtpAddress))
                throw new MissingFieldException("", "PrimarySmtpAddress");

            if (string.IsNullOrEmpty(parentOrganizationalUnit))
                throw new MissingFieldException("", "ParentOrganizationalUnit");

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-Mailbox");
            cmd.AddParameter("Name", resourceMailbox.DisplayName);
            cmd.AddParameter("DisplayName", resourceMailbox.DisplayName);
            cmd.AddParameter("UserPrincipalName", resourceMailbox.UserPrincipalName);
            cmd.AddParameter("PrimarySmtpAddress", resourceMailbox.PrimarySmtpAddress);
            cmd.AddParameter("OrganizationalUnit", string.Format("OU={0},{1}", Settings.ExchangeResourceOU, parentOrganizationalUnit));
            cmd.AddParameter("Shared");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Updates the shared mailbox
        /// </summary>
        /// <param name="resourceMailbox"></param>
        /// <param name="p"></param>
        public void Set_SharedMailbox(ResourceMailboxes resourceMailbox, Plans_ExchangeMailbox p, string[] emailAddresses)
        {
            logger.DebugFormat("Updating shared mailbox {0}", resourceMailbox.DisplayName);

            if (string.IsNullOrEmpty(resourceMailbox.UserPrincipalName))
                throw new MissingFieldException("", "UserPrincipalName");

            if (string.IsNullOrEmpty(resourceMailbox.CompanyCode))
                throw new MissingFieldException("", "CompanyCode");

            int sizeInMB = 0;
            if (p.MailboxSizeMB > 0)
            {
                // If mailbox size for plan is greater than 0 then its not unlimited
                if (p.MaxMailboxSizeMB != null && resourceMailbox.SizeInMB > p.MaxMailboxSizeMB)
                    sizeInMB = (int)p.MaxMailboxSizeMB;

                if (p.MaxMailboxSizeMB == null && resourceMailbox.SizeInMB > p.MailboxSizeMB)
                    sizeInMB = p.MailboxSizeMB;
            }

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", resourceMailbox.UserPrincipalName);
            cmd.AddParameter("CustomAttribute1", resourceMailbox.CompanyCode);
            cmd.AddParameter("EmailAddresses", emailAddresses);
            cmd.AddParameter("IssueWarningQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB * 0.90) : "Unlimited");
            cmd.AddParameter("MaxReceiveSize", p.MaxReceiveKB > 0 ? string.Format("{0}KB", p.MaxReceiveKB) : "Unlimited");
            cmd.AddParameter("MaxSendSize", p.MaxSendKB > 0 ? string.Format("{0}KB", p.MaxSendKB) : "Unlimited");
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, resourceMailbox.CompanyCode));
            cmd.AddParameter("ProhibitSendQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("ProhibitSendReceiveQuota", sizeInMB > 0 ? string.Format("{0}MB", sizeInMB) : "Unlimited");
            cmd.AddParameter("RecipientLimits", p.MaxRecipients > 0 ? string.Format("{0}", p.MaxRecipients) : "Unlimited");
            cmd.AddParameter("RetainDeletedItemsFor", p.MaxKeepDeletedItems > 0 ? p.MaxKeepDeletedItems : 30);
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("RoleAssignmentPolicy", Settings.ExchangeRoleAssignment);
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Deletes the room mailbox and AD object
        /// </summary>
        /// <param name="userPrincipalName"></param>
        public void Remove_SharedMailbox(string userPrincipalName)
        {
            logger.DebugFormat("Removing shared mailbox {0}", userPrincipalName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-Mailbox");
            cmd.AddParameter("Identity", userPrincipalName);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion

        #region Databases

        public List<MailboxDatabase> Get_MailboxDatabases()
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-MailboxDatabase");
            cmd.AddParameter("Status");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found Exchange databases");

                var listDatabases = new List<MailboxDatabase>();
                foreach (PSObject ps in psObjects)
                {
                    var newDb = new MailboxDatabase();
                    newDb.Identity = ps.Members["Identity"].Value.ToString();
                    newDb.Guid = Guid.Parse(ps.Members["Guid"].Value.ToString());
                    newDb.IsMailboxDatabase = bool.Parse(ps.Members["IsMailboxDatabase"].Value.ToString());
                    newDb.LogFilePrefix = ps.Members["LogFilePrefix"].Value.ToString();
                    newDb.LogFolderPath = ps.Members["LogFolderPath"].Value.ToString();
                    newDb.Server = ps.Members["Server"].Value.ToString();
                    newDb.Retrieved = DateTime.Now;

                    string size = "0 MB (0 bytes)";
                    long sizeInBytes = 0;
                    if (ps.Members["DatabaseSize"].Value != null)
                    {
                        size = ps.Members["DatabaseSize"].Value.ToString();
                        sizeInBytes = GetExchangeBytes(ps.Members["DatabaseSize"].Value.ToString());
                    }
                    newDb.DatabaseSize = size;
                    newDb.DatabaseSizeInBytes = sizeInBytes;

                    listDatabases.Add(newDb);
                }

                return listDatabases;
            }
        }

        #endregion

        #region Activesync

        /// <summary>
        /// Creates a new activesync policy in Exchange
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="companyCode"></param>
        public virtual void New_ActiveSyncPolicy(Plans_ExchangeActiveSync policy)
        {
            logger.DebugFormat("Creating new activesync policy named {0}", policy.DisplayName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-ActiveSyncMailboxPolicy");
            cmd.AddParameter("Name", policy.ExchangeName);
            cmd.AddParameter("AllowBluetooth", policy.AllowBluetooth);
            cmd.AddParameter("AllowBrowser", policy.AllowBrowser);
            cmd.AddParameter("AllowCamera", policy.AllowCamera);
            cmd.AddParameter("AllowConsumerEmail", policy.AllowConsumerMail);
            cmd.AddParameter("AllowDesktopSync", policy.AllowDesktopSync);
            cmd.AddParameter("AllowHTMLEmail", policy.AllowHTMLEmail);
            cmd.AddParameter("AllowInternetSharing", policy.AllowInternetSharing);
            cmd.AddParameter("AllowIrDA", policy.AllowInfrared);
            cmd.AddParameter("AllowNonProvisionableDevices", policy.AllowNonProvisionableDevices);
            cmd.AddParameter("AllowRemoteDesktop", policy.AllowRemoteDesktop);
            cmd.AddParameter("AllowSimpleDevicePassword", policy.AllowSimplePassword);
            cmd.AddParameter("AllowStorageCard", policy.AllowRemovableStorage);
            cmd.AddParameter("AllowTextMessaging", policy.AllowTextMessaging);
            cmd.AddParameter("AllowUnsignedApplications", policy.AllowUnsignedApplications);
            cmd.AddParameter("AllowUnsignedInstallationPackages", policy.AllowUnsignedInstallationPackages);
            cmd.AddParameter("AllowWiFi", policy.AllowWiFi);
            cmd.AddParameter("AlphanumericDevicePasswordRequired", policy.RequireAlphanumericPassword);
            cmd.AddParameter("AttachmentsEnabled", policy.AllowAttachmentsDownload);
            cmd.AddParameter("DeviceEncryptionEnabled", policy.RequireEncryptionOnDevice);
            cmd.AddParameter("DevicePasswordEnabled", policy.RequirePassword);
            cmd.AddParameter("MaxCalendarAgeFilter", policy.IncludePastCalendarItems);
            cmd.AddParameter("MaxEmailAgeFilter", policy.IncludePastEmailItems);
            cmd.AddParameter("PasswordRecoveryEnabled", policy.EnablePasswordRecovery);
            cmd.AddParameter("RequireStorageCardEncryption", policy.RequireEncryptionOnStorageCard);
            cmd.AddParameter("RequireManualSyncWhenRoaming", policy.AllowDirectPushWhenRoaming);

            if (policy.RefreshIntervalInHours > 0)
                cmd.AddParameter("DevicePolicyRefreshInterval", new TimeSpan((int)policy.RefreshIntervalInHours, 0, 0).ToString());
            else
                cmd.AddParameter("DevicePolicyRefreshInterval", "Unlimited");

            if (policy.MaximumAttachmentSizeInKB > 0)
                cmd.AddParameter("MaxAttachmentSize", policy.MaximumAttachmentSizeInKB + "KB");
            else
                cmd.AddParameter("MaxAttachmentSize", "Unlimited");

            if (policy.NumberOfFailedAttempted > 0)
                cmd.AddParameter("MaxDevicePasswordFailedAttempts", policy.NumberOfFailedAttempted);
            else
                cmd.AddParameter("MaxDevicePasswordFailedAttempts", "Unlimited");

            if (policy.LimitEmailSizeInKB > 0)
                cmd.AddParameter("MaxEmailBodyTruncationSize", policy.LimitEmailSizeInKB + "KB");
            else
                cmd.AddParameter("MaxEmailBodyTruncationSize", "Unlimited");

            if (policy.InactivityTimeoutInMinutes > 0)
                cmd.AddParameter("MaxInactivityTimeDeviceLock", new TimeSpan(0, (int)policy.InactivityTimeoutInMinutes, 0).ToString());
            else
                cmd.AddParameter("MaxInactivityTimeDeviceLock", "Unlimited");

            if (policy.MinDevicePasswordComplexCharacters > 0)
                cmd.AddParameter("MinDevicePasswordComplexCharacters", policy.MinDevicePasswordComplexCharacters);
            else
                cmd.AddParameter("MinDevicePasswordComplexCharacters", 1);

            if (policy.MinimumPasswordLength > 0)
                cmd.AddParameter("MinDevicePasswordLength", policy.MinimumPasswordLength);
            else
                cmd.AddParameter("MinDevicePasswordLength", null);

            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Completed adding paramters.. executing powershell..");
            _powershell.Commands = cmd;
            _powershell.Invoke();

            // Check for errors
            HandleErrors();
        }

        /// <summary>
        /// Updates an existing activesync policy in Exchange
        /// </summary>
        /// <param name="oldDisplayName"></param>
        /// <param name="policy"></param>
        public virtual void Set_ActiveSyncPolicy(string oldDisplayName, Plans_ExchangeActiveSync policy)
        {
            logger.DebugFormat("Updating activeysnc policy with old name {0} and new name is {1}", oldDisplayName, policy.DisplayName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-ActiveSyncMailboxPolicy");
            cmd.AddParameter("Identity", oldDisplayName);
            cmd.AddParameter("Name", policy.ExchangeName);
            cmd.AddParameter("AllowBluetooth", policy.AllowBluetooth);
            cmd.AddParameter("AllowBrowser", policy.AllowBrowser);
            cmd.AddParameter("AllowCamera", policy.AllowCamera);
            cmd.AddParameter("AllowConsumerEmail", policy.AllowConsumerMail);
            cmd.AddParameter("AllowDesktopSync", policy.AllowDesktopSync);
            cmd.AddParameter("AllowHTMLEmail", policy.AllowHTMLEmail);
            cmd.AddParameter("AllowInternetSharing", policy.AllowInternetSharing);
            cmd.AddParameter("AllowIrDA", policy.AllowInfrared);
            cmd.AddParameter("AllowNonProvisionableDevices", policy.AllowNonProvisionableDevices);
            cmd.AddParameter("AllowRemoteDesktop", policy.AllowRemoteDesktop);
            cmd.AddParameter("AllowSimpleDevicePassword", policy.AllowSimplePassword);
            cmd.AddParameter("AllowStorageCard", policy.AllowRemovableStorage);
            cmd.AddParameter("AllowTextMessaging", policy.AllowTextMessaging);
            cmd.AddParameter("AllowUnsignedApplications", policy.AllowUnsignedApplications);
            cmd.AddParameter("AllowUnsignedInstallationPackages", policy.AllowUnsignedInstallationPackages);
            cmd.AddParameter("AllowWiFi", policy.AllowWiFi);
            cmd.AddParameter("AlphanumericDevicePasswordRequired", policy.RequireAlphanumericPassword);
            cmd.AddParameter("AttachmentsEnabled", policy.AllowAttachmentsDownload);
            cmd.AddParameter("DeviceEncryptionEnabled", policy.RequireEncryptionOnDevice);
            cmd.AddParameter("DevicePasswordEnabled", policy.RequirePassword);
            cmd.AddParameter("MaxCalendarAgeFilter", policy.IncludePastCalendarItems);
            cmd.AddParameter("MaxEmailAgeFilter", policy.IncludePastEmailItems);
            cmd.AddParameter("PasswordRecoveryEnabled", policy.EnablePasswordRecovery);
            cmd.AddParameter("RequireStorageCardEncryption", policy.RequireEncryptionOnStorageCard);
            cmd.AddParameter("RequireManualSyncWhenRoaming", policy.AllowDirectPushWhenRoaming);

            if (policy.RefreshIntervalInHours > 0)
                cmd.AddParameter("DevicePolicyRefreshInterval", new TimeSpan((int)policy.RefreshIntervalInHours, 0, 0).ToString());
            else
                cmd.AddParameter("DevicePolicyRefreshInterval", "Unlimited");

            if (policy.MaximumAttachmentSizeInKB > 0)
                cmd.AddParameter("MaxAttachmentSize", policy.MaximumAttachmentSizeInKB + "KB");
            else
                cmd.AddParameter("MaxAttachmentSize", "Unlimited");

            if (policy.NumberOfFailedAttempted > 0)
                cmd.AddParameter("MaxDevicePasswordFailedAttempts", policy.NumberOfFailedAttempted);
            else
                cmd.AddParameter("MaxDevicePasswordFailedAttempts", "Unlimited");

            if (policy.LimitEmailSizeInKB > 0)
                cmd.AddParameter("MaxEmailBodyTruncationSize", policy.LimitEmailSizeInKB + "KB");
            else
                cmd.AddParameter("MaxEmailBodyTruncationSize", "Unlimited");

            if (policy.InactivityTimeoutInMinutes > 0)
                cmd.AddParameter("MaxInactivityTimeDeviceLock", new TimeSpan(0, (int)policy.InactivityTimeoutInMinutes, 0).ToString());
            else
                cmd.AddParameter("MaxInactivityTimeDeviceLock", "Unlimited");

            if (policy.MinDevicePasswordComplexCharacters > 0)
                cmd.AddParameter("MinDevicePasswordComplexCharacters", policy.MinDevicePasswordComplexCharacters);
            else
                cmd.AddParameter("MinDevicePasswordComplexCharacters", 1);

            if (policy.MinimumPasswordLength > 0)
                cmd.AddParameter("MinDevicePasswordLength", policy.MinimumPasswordLength);
            else
                cmd.AddParameter("MinDevicePasswordLength", null);

            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);

            logger.DebugFormat("Completed adding paramters.. executing powershell..");
            _powershell.Commands = cmd;
            _powershell.Invoke();

            // Check for errors
            HandleErrors();
        }

        /// <summary>
        /// Removes an ActiveSync mailbox policy from Exchange
        /// </summary>
        /// <param name="identity"></param>
        public virtual void Remove_ActiveSyncPolicy(string identity)
        {
            logger.DebugFormat("Removing activesync policy {0}", identity);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-ActiveSyncMailboxPolicy");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);

            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        /// <summary>
        /// Gets a list of activesync policies
        /// </summary>
        /// <returns></returns>
        public virtual List<Plans_ExchangeActiveSync> Get_ActiveSyncPolicies()
        {
            var foundList = new List<Plans_ExchangeActiveSync>();

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-ActiveSyncMailboxPolicy");
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            var psObjects = _powershell.Invoke();
            if (_powershell.HadErrors)
                throw _powershell.Streams.Error[0].Exception;
            else
            {
                logger.DebugFormat("Found ActiveSync policies");

                foreach (PSObject ps in psObjects)
                {
                    var newItem = new Plans_ExchangeActiveSync();
                    newItem.DisplayName = ps.Members["Identity"].Value.ToString();
                    newItem.ExchangeName = newItem.DisplayName;
                    logger.DebugFormat("Found policy {0} in Exchange", newItem.DisplayName);

                    logger.DebugFormat("Checking if it is the default policy");
                    bool isDefault = (bool)ps.Members["IsDefaultPolicy"].Value;
                    if (isDefault)
                    {
                        logger.DebugFormat("Policy {0} was found but it was the default. Not importing", newItem.DisplayName);
                    }
                    else
                    {
                        newItem.AllowNonProvisionableDevices = (bool)ps.Members["AllowNonProvisionableDevices"].Value;
                        logger.DebugFormat("Allow non provisionable devices is {0}", newItem.AllowNonProvisionableDevices);

                        newItem.RefreshIntervalInHours = GetHours(ps.Members["DevicePolicyRefreshInterval"].Value.ToString());
                        logger.DebugFormat("Refresh interval is {0}", newItem.RefreshIntervalInHours);

                        newItem.RequirePassword = (bool)ps.Members["DevicePasswordEnabled"].Value;
                        logger.DebugFormat("Device password required {0}", newItem.RequirePassword);

                        newItem.RequireAlphanumericPassword = (bool)ps.Members["AlphanumericDevicePasswordRequired"].Value;
                        logger.DebugFormat("Require alphanumeric password required {0}", newItem.RequireAlphanumericPassword);

                        newItem.EnablePasswordRecovery = (bool)ps.Members["PasswordRecoveryEnabled"].Value;
                        logger.DebugFormat("Password Recovery", newItem.EnablePasswordRecovery);

                        newItem.RequireEncryptionOnDevice = (bool)ps.Members["DeviceEncryptionEnabled"].Value;
                        logger.DebugFormat("Encryption required {0}", newItem.RequireEncryptionOnDevice);

                        newItem.RequireEncryptionOnStorageCard = (bool)ps.Members["RequireStorageCardEncryption"].Value;
                        logger.DebugFormat("Encryption on storage card required {0}", newItem.RequireEncryptionOnStorageCard);

                        newItem.AllowSimplePassword = (bool)ps.Members["AllowSimpleDevicePassword"].Value;
                        logger.DebugFormat("Simple password allowed {0}", newItem.AllowSimplePassword);

                        newItem.MinDevicePasswordComplexCharacters = (int)ps.Members["MinDevicePasswordComplexCharacters"].Value;
                        logger.DebugFormat("Minimum character sets {0}", newItem.MinDevicePasswordComplexCharacters);

                        newItem.NumberOfFailedAttempted = GetNumber(ps.Members["MaxDevicePasswordFailedAttempts"].Value.ToString());
                        logger.DebugFormat("Failed password attempts {0}", newItem.NumberOfFailedAttempted);

                        newItem.MinimumPasswordLength = GetNumber(ps.Members["MinDevicePasswordLength"].Value);
                        logger.DebugFormat("Minimum password length {0}", newItem.MinimumPasswordLength);

                        newItem.InactivityTimeoutInMinutes = GetMinutes(ps.Members["MaxInactivityTimeDeviceLock"].Value.ToString());
                        logger.DebugFormat("Inactivity device lock in minutes {0}", newItem.InactivityTimeoutInMinutes);

                        newItem.PasswordExpirationInDays = GetDays(ps.Members["DevicePasswordExpiration"].Value.ToString());
                        logger.DebugFormat("Password expiration in days {0}", newItem.PasswordExpirationInDays);

                        newItem.EnforcePasswordHistory = GetNumber(ps.Members["DevicePasswordHistory"].Value.ToString());
                        logger.DebugFormat("Password history {0}", newItem.EnforcePasswordHistory);

                        newItem.IncludePastCalendarItems = ps.Members["MaxCalendarAgeFilter"].Value.ToString();
                        logger.DebugFormat("Calendar items {0}", newItem.IncludePastCalendarItems);

                        newItem.IncludePastEmailItems = ps.Members["MaxEmailAgeFilter"].Value.ToString();
                        logger.DebugFormat("Email items {0}", newItem.IncludePastEmailItems);

                        newItem.LimitEmailSizeInKB = GetKiloBytes(ps.Members["MaxEmailBodyTruncationSize"].Value.ToString());
                        logger.DebugFormat("Limit email size in KB {0}", newItem.LimitEmailSizeInKB);

                        newItem.MaximumAttachmentSizeInKB = GetKiloBytes(ps.Members["MaxAttachmentSize"].Value.ToString());
                        logger.DebugFormat("Max attachment size in KB {0}", newItem.MaximumAttachmentSizeInKB);

                        newItem.AllowDirectPushWhenRoaming = (bool)ps.Members["RequireManualSyncWhenRoaming"].Value;
                        logger.DebugFormat("Allow direct push {0}", newItem.AllowDirectPushWhenRoaming);

                        newItem.AllowHTMLEmail = (bool)ps.Members["AllowHTMLEmail"].Value;
                        logger.DebugFormat("Allow HTML email {0}", newItem.AllowHTMLEmail);

                        newItem.AllowAttachmentsDownload = (bool)ps.Members["AttachmentsEnabled"].Value;
                        logger.DebugFormat("Allow attachment download {0}", newItem.AllowAttachmentsDownload);

                        newItem.AllowRemovableStorage = (bool)ps.Members["AllowStorageCard"].Value;
                        logger.DebugFormat("Allow removable storage {0}", newItem.AllowRemovableStorage);

                        newItem.AllowCamera = (bool)ps.Members["AllowCamera"].Value;
                        logger.DebugFormat("Allow camera {0}", newItem.AllowCamera);

                        newItem.AllowWiFi = (bool)ps.Members["AllowWiFi"].Value;
                        logger.DebugFormat("Allow wifi {0}", newItem.AllowWiFi);

                        newItem.AllowInfrared = (bool)ps.Members["AllowIrDA"].Value;
                        logger.DebugFormat("Allow infrared {0}", newItem.AllowInfrared);

                        newItem.AllowInternetSharing = (bool)ps.Members["AllowInternetSharing"].Value;
                        logger.DebugFormat("Allow internet sharing {0}", newItem.AllowInternetSharing);

                        newItem.AllowRemoteDesktop = (bool)ps.Members["AllowRemoteDesktop"].Value;
                        logger.DebugFormat("Allow remote desktop {0}", newItem.AllowRemoteDesktop);

                        newItem.AllowDesktopSync = (bool)ps.Members["AllowDesktopSync"].Value;
                        logger.DebugFormat("Allow desktop sync {0}", newItem.AllowDesktopSync);

                        newItem.AllowTextMessaging = (bool)ps.Members["AllowTextMessaging"].Value;
                        logger.DebugFormat("Allow text messaging {0}", newItem.AllowTextMessaging);

                        newItem.AllowBluetooth = ps.Members["AllowBluetooth"].Value.ToString();
                        logger.DebugFormat("Allow bluetooth {0}", newItem.AllowBluetooth);

                        newItem.AllowBrowser = (bool)ps.Members["AllowBrowser"].Value;
                        logger.DebugFormat("Allow browser {0}", newItem.AllowBrowser);

                        newItem.AllowConsumerMail = (bool)ps.Members["AllowConsumerEmail"].Value;
                        logger.DebugFormat("Allow consumer email {0}", newItem.AllowConsumerMail);

                        newItem.AllowUnsignedApplications = (bool)ps.Members["AllowUnsignedApplications"].Value;
                        logger.DebugFormat("Unsigned apps {0}", newItem.AllowUnsignedApplications);

                        newItem.AllowUnsignedInstallationPackages = (bool)ps.Members["AllowUnsignedInstallationPackages"].Value;
                        logger.DebugFormat("Unsigned installation packages {0}", newItem.AllowUnsignedInstallationPackages);

                        logger.DebugFormat("Completed {0}", newItem.DisplayName);
                        foundList.Add(newItem);
                    }
                }
            }

            return foundList;
        }

        #endregion

        #region Private methods

        internal int? GetNumber(object number)
        {
            logger.DebugFormat("Parsing number for string {0}", number);

            if (number == null || number.Equals("Unlimited"))
                return null;
            else
                return int.Parse(number.ToString());
        }

        internal int? GetHours(string time)
        {
            logger.DebugFormat("Parsing hours for string {0}", time);

            if (string.IsNullOrEmpty(time) || time.Equals("Unlimited"))
                return null;
            else
            {
                return TimeSpan.Parse(time).Hours;
            }
        }

        internal int? GetMinutes(string time)
        {
            logger.DebugFormat("Parsing minutes for string {0}", time);

            if (string.IsNullOrEmpty(time) || time.Equals("Unlimited"))
                return null;
            else
            {
                return TimeSpan.Parse(time).Minutes;
            }
        }

        internal int? GetDays(string time)
        {
            logger.DebugFormat("Parsing days for string {0}", time);

            if (string.IsNullOrEmpty(time) || time.Equals("Unlimited"))
                return null;
            else
            {
                return TimeSpan.Parse(time).Days;
            }
        }

        internal int? GetKiloBytes(string bytes)
        {
            logger.DebugFormat("Parsing kilobytes for string {0}", bytes);

            if (string.IsNullOrEmpty(bytes) || bytes.Equals("Unlimited"))
                return null;
            else
                return int.Parse(bytes) / 1024;
        }

        internal long GetExchangeBytes(string data)
        {
            // Should be in this format: "768 MB (805,306,386 bytes)"
            logger.DebugFormat("Parsing Exchange bytes for {0}", data);
            int startIndex = data.IndexOf("(");
            int endIndex = data.LastIndexOf(")");

            logger.DebugFormat("Start index of {0} is {1} and end index of {2}", data, startIndex, endIndex);
            string subString = data.Substring(startIndex + 1, endIndex - startIndex - 1);

            logger.DebugFormat("Substring of {0} is {1}", data, subString);
            string[] numbersOnly = subString.Split(new[] { "bytes" }, StringSplitOptions.RemoveEmptyEntries);

            logger.DebugFormat("Numbers only is {0}", numbersOnly[0].Trim());
            return long.Parse(numbersOnly[0].Trim(), NumberStyles.AllowThousands, CultureInfo.InvariantCulture);            
        }

        #endregion

    }
}