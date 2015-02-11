using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using CloudPanel.Exchange;
using log4net;
using System;
using System.Collections.Generic;

namespace CloudPanel.Rollback
{
    public enum Actions
    {
        CreateOrganizationalUnit,
        CreateSecurityGroup,
        AddDomains,
        AddUsers,
        CreateGlobalAddressList,
        CreateAddressList,
        CreateOfflineAddressBook,
        CreateAddressBookPolicy,
        CreateMailContact,
        CreateDistributionGroup,
        CreateRoomMailbox,
        CreateEquipmentMailbox,
        CreateSharedMailbox,
        CreateMailbox,
        CreateArchiveMailbox,
        CreatePublicFolderMailbox,
        CreatePublicFolder
    }

    public class ReverseActionValue
    {
        public Actions _PerformedAction { get; set; }

        public object _ActionAttribute { get; set; }

        public object[] _ActionAttributes { get; set; }
    }

    public class ReverseActions
    {
        private static readonly ILog log = log4net.LogManager.GetLogger("Rollback");

        private List<ReverseActionValue> _currentActions;

        public ReverseActions()
        {
            _currentActions = new List<ReverseActionValue>();
        }

        public void AddAction(Actions action, object attribute)
        {
            _currentActions.Add(new ReverseActionValue()
                {
                    _PerformedAction = action,
                    _ActionAttribute = attribute
                });

            if (attribute is string)
                log.DebugFormat("Inserted rollback action {0} with attribute {1} in the event of failure", action.ToString(), attribute.ToString());
        }

        public void AddAction(Actions action, object[] attribute)
        {
            _currentActions.Add(new ReverseActionValue()
            {
                _PerformedAction = action,
                _ActionAttribute = attribute
            });
        }

        public void RollbackNow()
        {
            // Reverse collection to remove in reverse order
            _currentActions.Reverse();

            foreach (var a in _currentActions)
            {
                switch (a._PerformedAction)
                {
                    case Actions.CreateOrganizationalUnit:
                    case Actions.CreateSecurityGroup:
                    case Actions.AddUsers:
                        RollbackADAction(a._PerformedAction, new[] { a._ActionAttribute });
                        break;
                    case Actions.AddDomains:
                        RollbackADAction(a._PerformedAction, a._ActionAttributes);
                        break;
                    case Actions.CreateAddressBookPolicy:
                    case Actions.CreateAddressList:
                    case Actions.CreateGlobalAddressList:
                    case Actions.CreateOfflineAddressBook:
                    case Actions.CreateRoomMailbox:
                    case Actions.CreateEquipmentMailbox:
                    case Actions.CreateSharedMailbox:
                    case Actions.CreatePublicFolderMailbox:
                    case Actions.CreatePublicFolder:
                    case Actions.CreateDistributionGroup:
                        RollbackExchangeAction(a._PerformedAction, new[] { a._ActionAttribute });
                        break;
                    default:
                        break;
                }
            }
        }

        private void RollbackADAction(Actions action, object[] attribute)
        {
            ADOrganizationalUnits org = null;
            ADGroups grp = null;
            ADUsers usr = null;

            try
            {
                log.InfoFormat("Rolling back action {0}...", action.ToString());

                switch (action)
                {
                    case Actions.CreateOrganizationalUnit:
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        org.Delete(attribute[0].ToString(), true);
                        log.DebugFormat("Successfully rolled back action {0} at path {1}", action.ToString(), attribute[0].ToString());
                        break;
                    case Actions.CreateSecurityGroup:
                        grp = new ADGroups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        grp.Delete(attribute[0].ToString());
                        log.DebugFormat("Successfully rolled back action {0} for group {1}", action.ToString(), attribute[0].ToString());
                        break;
                    case Actions.AddDomains:
                        org = new ADOrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        org.RemoveDomains(attribute[0].ToString(), new string[] { attribute[1].ToString() });
                        log.DebugFormat("Successfully rolled back action {0} for org {1}", action.ToString(), attribute[0].ToString());
                        break;
                    case Actions.AddUsers:
                        usr = new ADUsers(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        usr.Delete((Guid)attribute[0]);
                        log.DebugFormat("Successfully rolled back action {0} for user {1}", action.ToString(), attribute[0].ToString());
                        break;
                    default:
                        log.DebugFormat("Unknown action {0}... Skipping...", action.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to rollback action {0}. Exception: {1}", action.ToString(), ex.ToString());
            }
            finally
            {
                if (usr != null)
                    usr.Dispose();

                if (grp != null)
                    grp.Dispose();

                if (org != null)
                    org.Dispose();
            }
        }

        private void RollbackExchangeAction(Actions action, object[] attribute)
        {
            dynamic powershell = null;
            try
            {
                log.InfoFormat("Rolling back action {0}...", action.ToString());
                powershell = ExchPowershell.GetClass();

                switch (action)
                {
                    case Actions.CreateGlobalAddressList:
                        powershell.Remove_GlobalAddressList(attribute[0].ToString());
                        break;
                    case Actions.CreateOfflineAddressBook:
                        powershell.Remove_OfflineAddressBook(attribute[0].ToString());
                        break;
                    case Actions.CreateAddressBookPolicy:
                        powershell.Remove_AddressBookPolicy(attribute[0].ToString());
                        break;
                    case Actions.CreateAddressList:
                        powershell.Remove_AddressList(attribute[0].ToString());
                        break;
                    case Actions.CreateMailContact:
                        powershell.Remove_MailContact(attribute[0].ToString());
                        break;
                    case Actions.CreateRoomMailbox:
                        powershell.Remove_RoomMailbox(attribute[0].ToString());
                        break;
                    case Actions.CreateEquipmentMailbox:
                        powershell.Remove_EquipmentMailbox(attribute[0].ToString());
                        break;
                    case Actions.CreateSharedMailbox:
                        powershell.Remove_SharedMailbox(attribute[0].ToString());
                        break;
                    case Actions.CreateMailbox:
                        powershell.Disable_Mailbox(attribute[0].ToString(), true);
                        break;
                    case Actions.CreateArchiveMailbox:
                        powershell.Disable_ArchiveMailbox(attribute[0].ToString());
                        break;
                    case Actions.CreatePublicFolderMailbox:
                        powershell.Remove_PublicFolderMailbox(attribute[0].ToString());
                        break;
                    case Actions.CreatePublicFolder:
                        powershell.Remove_PublicFolder(attribute[0].ToString(), true);
                        break;
                    case Actions.CreateDistributionGroup:
                        powershell.Remove_DistributionGroup(attribute[0].ToString());
                        break;
                    default:
                        log.DebugFormat("Unknown action {0}... Skipping...", action.ToString());
                        break;
                }

                log.DebugFormat("Successfully rolled back action {0} for {1}", action.ToString(), attribute[0].ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to rollback action {0}. Exception: {1}", action.ToString(), ex.ToString());
            }
            finally
            {
                if (powershell != null)
                    powershell.Dispose();
            }
        }
    }
}
