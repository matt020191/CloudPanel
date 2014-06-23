using CloudPanel.ActiveDirectory;
using CloudPanel.Base.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Rollback
{
    public enum Actions
    {
        CreateOrganizationalUnit,
        CreateSecurityGroup,
        AddDomains
    }

    public class ReverseActionValue
    {
        public Actions _PerformedAction { get; set; }

        public object _ActionAttribute { get; set; }

        public object[] _ActionAttributes { get; set; }
    }

    public class ReverseActions
    {
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(ReverseActions));

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
                        RollbackADAction(a._PerformedAction, new[] { a._ActionAttribute });
                        break;
                    case Actions.AddDomains:
                        RollbackADAction(a._PerformedAction, a._ActionAttributes);
                        break;
                    default:
                        break;
                }
            }
        }

        private void RollbackADAction(Actions action, object[] attribute)
        {
            OrganizationalUnits org = null;
            Groups grp = null;
            Users usr = null;

            try
            {
                log.DebugFormat("Rolling back action {0}...", action.ToString());

                switch (action)
                {
                    case Actions.CreateOrganizationalUnit:
                        org = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        org.Delete(attribute[0].ToString(), true);
                        log.DebugFormat("Successfully rolled back action {0} at path {1}", action.ToString(), attribute.ToString());
                        break;
                    case Actions.CreateSecurityGroup:
                        grp = new Groups(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        grp.Delete(attribute[0].ToString());
                        log.DebugFormat("Successfully rolled back action {0} for group {1}", action.ToString(), attribute.ToString());
                        break;
                    case Actions.AddDomains:
                        org = new OrganizationalUnits(Settings.Username, Settings.DecryptedPassword, Settings.PrimaryDC);
                        org.RemoveDomains(attribute[0].ToString(), (string[])attribute[1]);
                        log.DebugFormat("Successfully rolled back action {0} for org {1}", action.ToString(), attribute[0]);
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
    }
}
