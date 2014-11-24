using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Exchange;
using log4net;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules
{
    public class MailboxModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MailboxModule));

        public MailboxModule() : base("/company/{CompanyCode}/exchange/mailbox")
        {
            this.RequiresAuthentication();

            Get["/size/{UserPrincipalName}"] = _ =>
            {
                this.RequiresValidatedClaims(c => ValidateClaims.AllowCompanyAdmin(Context.CurrentUser, _.CompanyCode, "vUsers"));

                #region Gets the mailbox size for this specific user
                string upn = _.UserPrincipalName;
                logger.DebugFormat("Getting mailbox size for {0}", upn);

                dynamic powershell = null;
                CloudPanelContext db = null;
                try
                {
                    powershell = ExchPowershell.GetClass();

                    // Get the size
                    SvcMailboxSizes size = powershell.Get_MailboxSize(upn);

                    // Add to database
                    if (!string.IsNullOrEmpty(size.UserPrincipalName))
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.SvcMailboxSizes.Add(size);
                        db.SaveChanges();
                    }

                    // Return
                    return Negotiate.WithModel(new { size = size });
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting mailbox size for {0}: {1}", upn, ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithStatusCode(HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();

                    if (powershell != null)
                        powershell.Dispose();
                }

                #endregion
            };
        }
    }
}