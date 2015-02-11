using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CloudPanel.Exchange
{
    public class Exch2013 : Exch2010
    {
        private readonly ILog logger = LogManager.GetLogger("Exchange");

        public Exch2013(string uri, string username, string password, bool kerberos, string domainController) :
            base(uri, username, password, kerberos, domainController)
        {
        }

        #region Mailboxes

        public void Set_DefaultPublicFolderMailbox(string companyCode, string publicFolderMailbox)
        {
            logger.DebugFormat("Associate all the company's mailbox for {0} with public folder mailbox {1}", companyCode, publicFolderMailbox);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-Mailbox");
            cmd.AddParameter("Filter", string.Format("CustomAttribute1 -eq \"{0}\"", companyCode));
            cmd.AddParameter("ResultSize", "Unlimited");
            cmd.AddParameter("DomainController", this._domainController);
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("DefaultPublicFolderMailbox", publicFolderMailbox);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        #endregion

        #region GAL / OAL / Address Book Policies

        public override string New_OfflineAddressBook(string companyCode)
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

        #endregion

        #region Activesync

        /// <summary>
        /// Creates a new activesync policy in Exchange
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="companyCode"></param>
        public override void New_ActiveSyncPolicy(Plans_ExchangeActiveSync policy)
        {
            logger.DebugFormat("Creating new activesync policy named {0}", policy.DisplayName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-MobileDeviceMailboxPolicy");
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
            cmd.AddParameter("AllowSimplePassword", policy.AllowSimplePassword);
            cmd.AddParameter("AllowStorageCard", policy.AllowRemovableStorage);
            cmd.AddParameter("AllowTextMessaging", policy.AllowTextMessaging);
            cmd.AddParameter("AllowUnsignedApplications", policy.AllowUnsignedApplications);
            cmd.AddParameter("AllowUnsignedInstallationPackages", policy.AllowUnsignedInstallationPackages);
            cmd.AddParameter("AllowWiFi", policy.AllowWiFi);
            cmd.AddParameter("AlphanumericPasswordRequired", policy.RequireAlphanumericPassword);
            cmd.AddParameter("AttachmentsEnabled", policy.AllowAttachmentsDownload);
            cmd.AddParameter("DeviceEncryptionEnabled", policy.RequireEncryptionOnDevice);
            cmd.AddParameter("PasswordEnabled", policy.RequirePassword);
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
                cmd.AddParameter("MaxPasswordFailedAttempts", policy.NumberOfFailedAttempted);
            else
                cmd.AddParameter("MaxPasswordFailedAttempts", "Unlimited");

            if (policy.LimitEmailSizeInKB > 0)
                cmd.AddParameter("MaxEmailBodyTruncationSize", policy.LimitEmailSizeInKB + "KB");
            else
                cmd.AddParameter("MaxEmailBodyTruncationSize", "Unlimited");

            if (policy.InactivityTimeoutInMinutes > 0)
                cmd.AddParameter("MaxInactivityTimeLock", new TimeSpan(0, (int)policy.InactivityTimeoutInMinutes, 0).ToString());
            else
                cmd.AddParameter("MaxInactivityTimeLock", "Unlimited");

            if (policy.MinDevicePasswordComplexCharacters > 0)
                cmd.AddParameter("MinPasswordComplexCharacters", policy.MinDevicePasswordComplexCharacters);
            else
                cmd.AddParameter("MinPasswordComplexCharacters", 1);

            if (policy.MinimumPasswordLength > 0)
                cmd.AddParameter("MinPasswordLength", policy.MinimumPasswordLength);
            else
                cmd.AddParameter("MinPasswordLength", null);

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
        public override void Set_ActiveSyncPolicy(string oldDisplayName, Plans_ExchangeActiveSync policy)
        {
            logger.DebugFormat("Updating activeysnc policy with old name {0} and new name is {1}", oldDisplayName, policy.DisplayName);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-MobileDeviceMailboxPolicy");
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
            cmd.AddParameter("AllowSimplePassword", policy.AllowSimplePassword);
            cmd.AddParameter("AllowStorageCard", policy.AllowRemovableStorage);
            cmd.AddParameter("AllowTextMessaging", policy.AllowTextMessaging);
            cmd.AddParameter("AllowUnsignedApplications", policy.AllowUnsignedApplications);
            cmd.AddParameter("AllowUnsignedInstallationPackages", policy.AllowUnsignedInstallationPackages);
            cmd.AddParameter("AllowWiFi", policy.AllowWiFi);
            cmd.AddParameter("AlphanumericPasswordRequired", policy.RequireAlphanumericPassword);
            cmd.AddParameter("AttachmentsEnabled", policy.AllowAttachmentsDownload);
            cmd.AddParameter("DeviceEncryptionEnabled", policy.RequireEncryptionOnDevice);
            cmd.AddParameter("PasswordEnabled", policy.RequirePassword);
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
                cmd.AddParameter("MaxPasswordFailedAttempts", policy.NumberOfFailedAttempted);
            else
                cmd.AddParameter("MaxPasswordFailedAttempts", "Unlimited");

            if (policy.LimitEmailSizeInKB > 0)
                cmd.AddParameter("MaxEmailBodyTruncationSize", policy.LimitEmailSizeInKB + "KB");
            else
                cmd.AddParameter("MaxEmailBodyTruncationSize", "Unlimited");

            if (policy.InactivityTimeoutInMinutes > 0)
                cmd.AddParameter("MaxInactivityTimeLock", new TimeSpan(0, (int)policy.InactivityTimeoutInMinutes, 0).ToString());
            else
                cmd.AddParameter("MaxInactivityTimeLock", "Unlimited");

            if (policy.MinDevicePasswordComplexCharacters > 0)
                cmd.AddParameter("MinPasswordComplexCharacters", policy.MinDevicePasswordComplexCharacters);
            else
                cmd.AddParameter("MinPasswordComplexCharacters", 1);

            if (policy.MinimumPasswordLength > 0)
                cmd.AddParameter("MinPasswordLength", policy.MinimumPasswordLength);
            else
                cmd.AddParameter("MinPasswordLength", null);

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
        public override void Remove_ActiveSyncPolicy(string identity)
        {
            logger.DebugFormat("Removing activesync policy {0}", identity);

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-MobileDeviceMailboxPolicy");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("Confirm", false);
            cmd.AddParameter("DomainController", this._domainController);

            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors(true);
        }

        /// <summary>
        /// Gets all activesync policy
        /// </summary>
        /// <returns></returns>
        public override List<Plans_ExchangeActiveSync> Get_ActiveSyncPolicies()
        {
            var foundList = new List<Plans_ExchangeActiveSync>();

            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-MobileDeviceMailboxPolicy");
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
                    bool isDefault = (bool)ps.Members["IsDefault"].Value;
                    if (isDefault)
                    {
                        logger.DebugFormat("Policy {0} was found but it was the default. Not importing", newItem.DisplayName);
                    }
                    else
                    {
                        logger.DebugFormat("Policy {0} was found and it was not the default policy.", newItem.DisplayName);

                        newItem.AllowNonProvisionableDevices = (bool)ps.Members["AllowNonProvisionableDevices"].Value;
                        logger.DebugFormat("Allow non provisionable devices is {0}", newItem.AllowNonProvisionableDevices);

                        newItem.RefreshIntervalInHours = GetHours(ps.Members["DevicePolicyRefreshInterval"].Value.ToString());
                        logger.DebugFormat("Refresh interval is {0}", newItem.RefreshIntervalInHours);

                        newItem.RequirePassword = (bool)ps.Members["PasswordEnabled"].Value;
                        logger.DebugFormat("Device password required {0}", newItem.RequirePassword);

                        newItem.RequireAlphanumericPassword = (bool)ps.Members["AlphanumericPasswordRequired"].Value;
                        logger.DebugFormat("Require alphanumeric password required {0}", newItem.RequireAlphanumericPassword);

                        newItem.EnablePasswordRecovery = (bool)ps.Members["PasswordRecoveryEnabled"].Value;
                        logger.DebugFormat("Password Recovery", newItem.EnablePasswordRecovery);

                        newItem.RequireEncryptionOnDevice = (bool)ps.Members["DeviceEncryptionEnabled"].Value;
                        logger.DebugFormat("Encryption required {0}", newItem.RequireEncryptionOnDevice);

                        newItem.RequireEncryptionOnStorageCard = (bool)ps.Members["RequireStorageCardEncryption"].Value;
                        logger.DebugFormat("Encryption on storage card required {0}", newItem.RequireEncryptionOnStorageCard);

                        newItem.AllowSimplePassword = (bool)ps.Members["AllowSimplePassword"].Value;
                        logger.DebugFormat("Simple password allowed {0}", newItem.AllowSimplePassword);

                        newItem.MinDevicePasswordComplexCharacters = (int)ps.Members["MinPasswordComplexCharacters"].Value;
                        logger.DebugFormat("Minimum character sets {0}", newItem.MinDevicePasswordComplexCharacters);

                        newItem.NumberOfFailedAttempted = GetNumber(ps.Members["MaxPasswordFailedAttempts"].Value.ToString());
                        logger.DebugFormat("Failed password attempts {0}", newItem.NumberOfFailedAttempted);

                        newItem.MinimumPasswordLength = GetNumber(ps.Members["MinPasswordLength"].Value);
                        logger.DebugFormat("Minimum password length {0}", newItem.MinimumPasswordLength);

                        newItem.InactivityTimeoutInMinutes = GetMinutes(ps.Members["MaxInactivityTimeLock"].Value.ToString());
                        logger.DebugFormat("Inactivity device lock in minutes {0}", newItem.InactivityTimeoutInMinutes);

                        newItem.PasswordExpirationInDays = GetDays(ps.Members["PasswordExpiration"].Value.ToString());
                        logger.DebugFormat("Password expiration in days {0}", newItem.PasswordExpirationInDays);

                        newItem.EnforcePasswordHistory = GetNumber(ps.Members["PasswordHistory"].Value.ToString());
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

        #region Public Folders

        /// <summary>
        /// Creates a new public folder mailbox
        /// </summary>
        /// <param name="publicFolderName"></param>
        /// <param name="displayName"></param>
        /// <param name="organizationalUnit"></param>
        /// <param name="primarySmtpAddress"></param>
        public void New_PublicFolderMailbox(string name, string displayName, string organizationalUnit, string primarySmtpAddress, string addressBookPolicy)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-Mailbox");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("PublicFolder");
            cmd.AddParameter("DisplayName", displayName);
            cmd.AddParameter("OrganizationalUnit", organizationalUnit);
            cmd.AddParameter("PrimarySmtpAddress", primarySmtpAddress);
            cmd.AddParameter("AddressBookPolicy", addressBookPolicy);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Creates a new public folder
        /// </summary>
        /// <param name="name"></param>
        /// <param name="mailbox"></param>
        /// <param name="path"></param>
        public void New_PublicFolder(string name, string mailbox, string path)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("New-PublicFolder");
            cmd.AddParameter("Name", name);
            cmd.AddParameter("Mailbox", mailbox);
            cmd.AddParameter("Path", path);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Updates a public folder mailbox with plan settings
        /// </summary>
        /// <param name="publicFolderName"></param>
        /// <param name="companyCode"></param>
        /// <param name="p"></param>
        public void Set_PublicFolderMailbox(string identity, string companyCode, Plans_ExchangePublicFolders p)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Set-Mailbox");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("PublicFolder");
            cmd.AddParameter("EmailAddressPolicyEnabled", false);
            cmd.AddParameter("IssueWarningQuota", string.Format("{0}MB", p.MailboxSizeMB * 0.90));
            cmd.AddParameter("OfflineAddressBook", string.Format(Settings.ExchangeOALName, companyCode));
            cmd.AddParameter("ProhibitSendQuota", string.Format("{0}MB", p.MailboxSizeMB));
            cmd.AddParameter("ProhibitSendReceiveQuota", string.Format("{0}MB", p.MailboxSizeMB));
            cmd.AddParameter("UseDatabaseQuotaDefaults", false);
            cmd.AddParameter("UseDatabaseRetentionDefaults", false);
            cmd.AddParameter("RetainDeletedItemsUntilBackup", true);
            cmd.AddParameter("CustomAttribute1",companyCode);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Removes a public folder mailbox
        /// </summary>
        /// <param name="publicFolderName"></param>
        public void Remove_PublicFolderMailbox(string identity)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-Mailbox");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("PublicFolder");
            cmd.AddParameter("Confirm", false);
            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Removes a public folder
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="isRecurse"></param>
        public void Remove_PublicFolder(string identity, bool isRecurse)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Remove-PublicFolder");
            cmd.AddParameter("Identity", identity);
            cmd.AddParameter("DomainController", this._domainController);
            cmd.AddParameter("Confirm", false);

            if (isRecurse)
                cmd.AddParameter("Recurse");

            _powershell.Commands = cmd;
            _powershell.Invoke();

            HandleErrors();
        }

        /// <summary>
        /// Adds public folder client permissions
        /// </summary>
        /// <param name="publicFolderPath"></param>
        /// <param name="permissions">Key is the permission and value is the user</param>
        public void Add_PublicFolderClientPermission(string publicFolderPath, string publicFolderMailbox, Dictionary<string, string> permissions)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-PublicFolderClientPermission");
            cmd.AddParameter("Identity", publicFolderPath);
            cmd.AddParameter("Mailbox", publicFolderMailbox);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            Collection<PSObject> obj = _powershell.Invoke();
            HandleErrors(); // Check for errors

            // Add the permissions
            var currentUsers = from o in obj select o.Members["User"].Value;
            foreach (var kvp in permissions)
            {
                if (!currentUsers.Any(x => x.ToString().Equals(kvp.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    cmd = new PSCommand();
                    cmd.AddCommand("Add-PublicFolderClientPermission");
                    cmd.AddParameter("Identity", publicFolderPath);
                    cmd.AddParameter("User", kvp.Value);
                    cmd.AddParameter("AccessRights", kvp.Key);
                    cmd.AddParameter("Confirm", false);
                    cmd.AddParameter("DomainController", this._domainController);
                    _powershell.Commands = cmd;
                    _powershell.Invoke();

                    HandleErrors(); // Check for new errors
                }
            }
        }

        /// <summary>
        /// Removes public folder client permissions
        /// </summary>
        /// <param name="publicFolderPath"></param>
        /// <param name="permissions"></param>
        public void Remove_PublicFolderClientPermission(string publicFolderPath, string publicFolderMailbox, string[] users)
        {
            PSCommand cmd = new PSCommand();
            cmd.AddCommand("Get-PublicFolderClientPermission");
            cmd.AddParameter("Identity", publicFolderPath);
            cmd.AddParameter("Mailbox", publicFolderMailbox);
            cmd.AddParameter("DomainController", this._domainController);
            _powershell.Commands = cmd;

            Collection<PSObject> obj = _powershell.Invoke();
            HandleErrors(); // Check for errors

            // Add the permissions
            var currentUsers = from o in obj select o.Members["User"].Value;
            foreach (var user in users)
            {
                if (!currentUsers.Any(x => x.ToString().Equals(user, StringComparison.InvariantCultureIgnoreCase)))
                {
                    cmd = new PSCommand();
                    cmd.AddCommand("Remove-PublicFolderClientPermission");
                    cmd.AddParameter("Identity", publicFolderPath);
                    cmd.AddParameter("Mailbox", publicFolderMailbox);
                    cmd.AddParameter("User", user);
                    cmd.AddParameter("Confirm", false);
                    cmd.AddParameter("DomainController", this._domainController);
                    _powershell.Commands = cmd;
                    _powershell.Invoke();

                    HandleErrors(); // Check for new errors
                }
            }
        }

        #endregion
    }
}
