using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace CloudPanel.Citrix
{
    public class CitrixPowershell : IDisposable
    {
        private bool _disposed = false;

        internal WSManConnectionInfo _connection;
        internal Runspace _runspace;
        internal PowerShell _powershell;

        private readonly ILog logger = LogManager.GetLogger("Citrix");

        public CitrixPowershell(string uri, string username, string password)
        {
            Console.WriteLine("Running");
            this._connection = GetConnection(uri, username, password);

            Console.WriteLine("Got connection");
            PSSnapInException snapinException;

            Console.WriteLine("Create runspace");
            _runspace = RunspaceFactory.CreateRunspace(_connection);
            _runspace.Open();

            //Console.WriteLine("add PSSnapin");
            //_runspace.RunspaceConfiguration.AddPSSnapIn("Citrix.*.Admin.V*", out snapinException);

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
                    logger.ErrorFormat("PSERROR Reason: {0}", reason);

                    var exception = _powershell.Streams.Error[0].Exception;
                    _powershell.Streams.Error.Clear();

                    throw exception;
                }
            }
        }

        private WSManConnectionInfo GetConnection(string uri, string username, string password)
        {
            Console.WriteLine("{0} / {1} / {2}", uri, username, password);
            SecureString pwd = new SecureString();
            foreach (char x in password)
                pwd.AppendChar(x);

            PSCredential ps = new PSCredential(username, pwd);

            WSManConnectionInfo wsinfo = new WSManConnectionInfo(new Uri(uri), "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", ps);
            wsinfo.AuthenticationMechanism = AuthenticationMechanism.Default;
            wsinfo.ProxyAuthentication = AuthenticationMechanism.Negotiate;

            return wsinfo;
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
