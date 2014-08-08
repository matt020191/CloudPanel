using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;

namespace CloudPanel.code
{
    public class Auditor : IDisposable
    {
        public Auditor()
        {
            db = new CloudPanelContext(Settings.ConnectionString);
        }

        public void Audit(string username, string companycode, string method, string path, string host, string identifyingInfo = null, DateTime? timestamp = null)
        {
            db.Audits.Add(new Audit
            {
                Username = username,
                CompanyCode = companycode,
                Method = method,
                Path = path,
                UserHostAddress = host,
                Timestamp = timestamp ?? DateTime.Now,
                IdentifyingInfo = identifyingInfo
            });

            db.SaveChanges();
        }

        private readonly CloudPanelContext db;

        #region IDisposable Members

        public void Dispose()
        {
            db.Dispose();
        }

        #endregion
    }
}