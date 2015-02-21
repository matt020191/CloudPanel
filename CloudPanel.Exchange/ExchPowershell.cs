
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;
using CloudPanel.Base.Config;
using log4net;
using System;

namespace CloudPanel.Exchange
{
    public class ExchPowershell : IDisposable
    {
        private bool _disposed = false;

        internal WSManConnectionInfo _connection;
        internal Runspace _runspace;
        internal PowerShell _powershell;

        internal string _domainController;

        private readonly ILog logger = LogManager.GetLogger("Exchange");

        public ExchPowershell(string uri, string username, string password, bool kerberos, string domainController)
        {
            this._connection = GetConnection(uri, username, password, kerberos);
            this._domainController = domainController;

            _runspace = RunspaceFactory.CreateRunspace(_connection);
            _runspace.Open();

            _powershell = PowerShell.Create();
            _powershell.Runspace = _runspace;
        }

        internal void HandleErrors(bool ignoredNotFound = false, bool ignoreAlreadyExist = false)
        {
            if (_powershell != null)
            {
                // Log the debug messages
                if (_powershell.Streams.Debug.Count > 0)
                {
                    foreach (var debug in _powershell.Streams.Debug)
                        logger.DebugFormat("PSDEBUG: {0}", debug.ToString());

                    _powershell.Streams.Debug.Clear();
                }

                // Log the warnings
                if (_powershell.Streams.Warning.Count > 0)
                {
                    foreach (var warn in _powershell.Streams.Warning)
                        logger.WarnFormat("PSWARN: {0}", warn.ToString());

                    _powershell.Streams.Warning.Clear();
                }

                // Log the errors
                if (_powershell.Streams.Error.Count > 0)
                {
                    foreach (var err in _powershell.Streams.Error)
                        logger.ErrorFormat("PSERROR: {0}", err.Exception.ToString());

                    // Get first reason
                    var reason = _powershell.Streams.Error[0].CategoryInfo.Reason;
                    if (reason.Equals("ManagementObjectNotFoundException") && ignoredNotFound)
                        logger.InfoFormat("PSHANDLE: Error was found in exception but was ignored due to setting: {0}. Continuing...", reason);
                    else if (reason.Equals("MemberAlreadyExistsException") && ignoreAlreadyExist)
                        logger.InfoFormat("PSHANDLE: Error was found in exception but was ignored due to setting: {0}. Continuing...", reason);
                    else
                    {
                        logger.ErrorFormat("PSERROR Reason: {0}", reason);

                        var exception = _powershell.Streams.Error[0].Exception;
                        _powershell.Streams.Error.Clear();

                        throw exception;
                    }
                }
            }
        }

        private WSManConnectionInfo GetConnection(string uri, string username, string password, bool kerberos)
        {
            SecureString pwd = new SecureString();
            foreach (char x in password)
                pwd.AppendChar(x);

            PSCredential ps = new PSCredential(username, pwd);

            WSManConnectionInfo wsinfo = new WSManConnectionInfo(new Uri(uri), "http://schemas.microsoft.com/powershell/Microsoft.Exchange", ps);
            wsinfo.SkipCACheck = true;
            wsinfo.SkipCNCheck = true;
            wsinfo.SkipRevocationCheck = true;
            wsinfo.OpenTimeout = 9000;
            wsinfo.MaximumConnectionRedirectionCount = 1;

            if (kerberos)
                wsinfo.AuthenticationMechanism = AuthenticationMechanism.Kerberos;
            else
                wsinfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

            return wsinfo;
        }

        // Returns the appropriate class based on their Exchange version
        public static dynamic GetClass()
        {
            if (Settings.ExchangeVersion == 2010)
                return new Exch2010(Settings.ExchangeUri, Settings.Username, Settings.DecryptedPassword, false, Settings.PrimaryDC);
            else if (Settings.ExchangeVersion == 2013)
                return new Exch2013(Settings.ExchangeUri, Settings.Username, Settings.DecryptedPassword, false, Settings.PrimaryDC);
            else if (Settings.ExchangeVersion == 20135)
                return new Exch2013(Settings.ExchangeUri, Settings.Username, Settings.DecryptedPassword, false, Settings.PrimaryDC);
            else
                throw new Exception("Unable to determine Exchange version");
        }

        #region Dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection = null;

                    if (_powershell != null)
                        _powershell.Dispose();

                    if (_runspace != null)
                        _runspace.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
