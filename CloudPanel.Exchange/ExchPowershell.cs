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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Security;
using System.Text;

namespace CloudPanel.Exchange
{
    public class ExchPowershell : IDisposable
    {
        private bool _disposed = false;

        internal WSManConnectionInfo _connection;
        internal Runspace _runspace;
        internal PowerShell _powershell;

        internal string _domainController;

        private readonly ILog logger = LogManager.GetLogger(typeof(ExchPowershell));

        public ExchPowershell(string uri, string username, string password, bool kerberos, string domainController)
        {
            this._connection = GetConnection(uri, username, password, kerberos);
            this._domainController = domainController;

            _runspace = RunspaceFactory.CreateRunspace(_connection);
            _runspace.Open();

            _powershell = PowerShell.Create();
            _powershell.Runspace = _runspace;
        }

        internal void HandleErrors(bool ignoredNotFound = false)
        {
            if (_powershell != null && _powershell.HadErrors)
            {
                // Log the debug messages
                if (_powershell.Streams.Debug.Count > 0)
                {
                    foreach (var debug in _powershell.Streams.Debug)
                        logger.DebugFormat("PSDEBUG: {0}", debug.Message);

                    _powershell.Streams.Debug.Clear();
                }

                // Log the warnings
                if (_powershell.Streams.Warning.Count > 0)
                {
                    foreach (var warn in _powershell.Streams.Warning)
                        logger.WarnFormat("PSWARN: {0}", warn.Message);

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
                        logger.InfoFormat("PSHANDLE: Error was found in exception but was ignored due to setting. Continuing...");
                    else
                        throw _powershell.Streams.Error[0].Exception;
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

            if (kerberos)
                wsinfo.AuthenticationMechanism = AuthenticationMechanism.Kerberos;
            else
                wsinfo.AuthenticationMechanism = AuthenticationMechanism.Basic;

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
