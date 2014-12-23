using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Authentication;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace CloudPanel.Modules.Admin
{
    public class MassEmailModule : NancyModule
    {
        private static readonly ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MassEmailModule() : base("/admin/email")
        {
            this.RequiresAnyClaim(new[] { "SuperAdmin", "ResellerAdmin" });

            Get["/"] = _ =>
            {
                #region Gets a list of companies the user has access to
                CloudPanelContext db = null;
                try
                {
                    logger.DebugFormat("Loading mass email section");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    if (this.Context.IsSuperAdmin())
                    {
                        var companies = (from d in db.Companies
                                         orderby d.CompanyName
                                         select d).ToList();
                        return View["Admin/massemail.cshtml", new { companies = companies }];
                    }
                    else
                    {
                        // Has to be a reseller admin in this case (only return companies for the reseller)
                        string companyCode = this.Context.GetCompanyCodeMembership();
                        string resellerCode = (from d in db.Companies
                                               where d.CompanyCode == companyCode
                                               select d.ResellerCode).First();

                        var companies = (from d in db.Companies
                                         where d.ResellerCode == resellerCode
                                         orderby d.CompanyName
                                         select d).ToList();

                        return View["Admin/massemail.cshtml", new { companies = companies }];
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error loading mass email page: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(Nancy.HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/"] = _ =>
            {
                CloudPanelContext db = null;
                SmtpClient sc = null;
                try
                {
                    #region Validation
                    if (!Request.Form.To.HasValue)
                        throw new MissingFieldException("", "To");

                    if (!Request.Form.Subject.HasValue)
                        throw new MissingFieldException("", "Subject");

                    if (!Request.Form.Message.HasValue)
                        throw new MissingFieldException("", "Message");
                    #endregion

                    #region Load form values
                    string sendTo = Request.Form.To.Value;
                    logger.DebugFormat("Send to raw: {0}", sendTo);

                    string subject = Request.Form.Subject.Value;
                    logger.DebugFormat("Subject raw: {0}", subject);

                    string message = Request.Form.Message.Value;
                    logger.DebugFormat("Message raw: {0}", message);
                    #endregion

                    string resellerCode = string.Empty;
                    if (this.Context.IsResellerAdmin())
                    {
                        logger.DebugFormat("User is a reseller admin");
                        string companyCode = this.Context.GetCompanyCodeMembership();
                        resellerCode = (from d in db.Companies where d.CompanyCode == companyCode select d.ResellerCode).First();
                    }

                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    List<Users> allUsers = null;
                    switch (sendTo.ToLower())
                    {
                        case "all": // Sending to all users
                            allUsers = (from d in db.Users
                                        where !string.IsNullOrEmpty(d.Email)
                                        select d).ToList();

                            if (this.Context.IsResellerAdmin())
                                allUsers = (from d in allUsers
                                            join c in db.Companies on d.CompanyCode equals c.CompanyCode into d1
                                            from company in d1.DefaultIfEmpty()
                                            where company.ResellerCode == resellerCode
                                            select d).ToList();
                            break;
                        case "admins": // Sending to only company admins
                            allUsers = (from d in db.Users
                                        where !string.IsNullOrEmpty(d.Email)
                                        where d.IsCompanyAdmin == true
                                        select d).ToList();

                            if (this.Context.IsResellerAdmin())
                                allUsers = (from d in allUsers
                                            join c in db.Companies on d.CompanyCode equals c.CompanyCode into d1
                                            from company in d1.DefaultIfEmpty()
                                            where company.ResellerCode == resellerCode
                                            select d).ToList();
                            break;
                        default: // Sending to a specific company
                            allUsers = (from d in db.Users
                                        where d.CompanyCode == sendTo
                                        where !string.IsNullOrEmpty(d.Email)
                                        select d).ToList();

                            if (this.Context.IsResellerAdmin())
                                allUsers = (from d in allUsers
                                            join c in db.Companies on d.CompanyCode equals c.CompanyCode into d1
                                            from company in d1.DefaultIfEmpty()
                                            where company.ResellerCode == resellerCode
                                            select d).ToList();
                            break;
                    }

                    Dictionary<string, string> results = new Dictionary<string, string>();
                    sc = new SmtpClient(Settings.SNServer, Settings.SNPort);

                    if (!string.IsNullOrEmpty(Settings.SNUsername) && !string.IsNullOrEmpty(Settings.SNEncryptedPassword))
                        sc.Credentials = new NetworkCredential(Settings.SNUsername, Settings.SNDecryptedPassword);

                    SendMessage(ref sc, allUsers, subject, message, ref results);

                    return Negotiate.WithModel(new { success = "Successfully sent messages", results = results })
                                    .WithView("success.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error sending messages: {0}", ex.ToString());
                    return Negotiate.WithModel(new { error = ex.Message })
                                    .WithView("Error/500.cshtml")
                                    .WithStatusCode(Nancy.HttpStatusCode.InternalServerError);
                }
                finally
                {
                    if (sc != null)
                        sc.Dispose();

                    if (db != null)
                        db.Dispose();
                }
            };
        }

        private void SendMessage(ref SmtpClient sc, List<Users> to, string subject, string message, ref Dictionary<string, string> results)
        {
            try
            {
                foreach (var u in to)
                {
                    string s = subject.Replace("{DisplayName}", u.DisplayName);
                    string m = message.Replace("{DisplayName}", u.DisplayName)
                                      .Replace("{FirstName}", u.Firstname)
                                      .Replace("{LastName}", u.Lastname);

                    MailMessage mm = null;
                    try
                    {
                        mm = new MailMessage(Settings.SNFrom, u.Email);
                        mm.Subject = s;
                        mm.Body = m;
                        mm.IsBodyHtml = true;

                        sc.Send(mm);
                        results.Add(u.Email, "Message submitted successfully");

                        logger.InfoFormat("Message to [{0}] with subject [{1}] successfully sent", u.Email, s);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Message to [{0}] with subject [{1}] FAILED to send with status [{2}]", u.Email, s, ex.ToString());
                        results.Add(u.Email, "FAILED: " + ex.Message);
                    }
                    finally
                    {
                        if (mm != null)
                            mm.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error sending messages: {0}", ex.ToString());
                throw;
            }
        }
    }
}