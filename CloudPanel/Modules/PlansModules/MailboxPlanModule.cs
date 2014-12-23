using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Modules.PlansModules
{
    public class MailboxPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MailboxPlanModule() : base("/plans/exchange/mailbox")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/new"] = _ =>
            {
                return Negotiate.WithModel(new { selectedPlan = new Plans_ExchangeMailbox() })
                                .WithView("Plans/plans_mailbox.cshtml");
            };

            Post["/new"] = _ =>
            {
                #region Creates a new mailbox plan
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var newPlan = this.Bind<Plans_ExchangeMailbox>();

                    if (newPlan.MaxMailboxSizeMB == null || newPlan.MaxMailboxSizeMB < newPlan.MailboxSizeMB)
                        newPlan.MaxMailboxSizeMB = newPlan.MailboxSizeMB;

                    if (newPlan.CompanyCode == "0")
                        newPlan.CompanyCode = string.Empty;

                    db.Plans_ExchangeMailbox.Add(newPlan);
                    db.SaveChanges();

                    return Negotiate.WithModel(new
                                {
                                    selectedPlan = new Plans_ExchangeMailbox()
                                })
                                .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                .WithView("Plans/plans_mailbox.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating mailbox plan: {0}", ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Get["/{ID:int}"] = _ =>
            {
                #region Gets an existing mailbox plan
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var plan = (from d in db.Plans_ExchangeMailbox
                                where d.MailboxPlanID == id
                                select d).FirstOrDefault();

                    return Negotiate.WithModel(new
                                {
                                    selectedPlan = plan
                                })
                                .WithMediaRangeModel("application/json", new
                                {
                                    selectedPlan = plan
                                })
                                .WithView("Plans/plans_mailbox.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting mailbox plan {0}: {1}", _.ID, ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Post["/{ID:int}"] = _ =>
            {
                #region Updates an existing mailbox plan
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var oldPlan = (from d in db.Plans_ExchangeMailbox
                                   where d.MailboxPlanID == id
                                   select d).FirstOrDefault();
                    var updatedPlan = this.Bind<Plans_ExchangeMailbox>("ProductID", "MailboxPlanID", "ResellerCode");
                    oldPlan.MailboxPlanName = updatedPlan.MailboxPlanName;
                    oldPlan.MailboxPlanDesc = updatedPlan.MailboxPlanDesc;
                    oldPlan.MaxRecipients = Request.Form.cbMaxRecipients == true ? 0 : updatedPlan.MaxRecipients;
                    oldPlan.MaxKeepDeletedItems = updatedPlan.MaxKeepDeletedItems;
                    oldPlan.MailboxSizeMB = Request.Form.cbMailboxSizeMB == true ? 0 : updatedPlan.MailboxSizeMB;

                    oldPlan.MaxMailboxSizeMB = (updatedPlan.MaxMailboxSizeMB == null || updatedPlan.MaxMailboxSizeMB < oldPlan.MailboxSizeMB) ? oldPlan.MailboxSizeMB : updatedPlan.MaxMailboxSizeMB; // Make sure max mailbox size is greater than mailbox size or equal to.

                    oldPlan.MaxSendKB = Request.Form.cbMaxSendKB == true ? 0 : updatedPlan.MaxSendKB;
                    oldPlan.MaxReceiveKB = Request.Form.cbMaxReceiveKB == true ? 0 : updatedPlan.MaxReceiveKB;
                    oldPlan.EnablePOP3 = updatedPlan.EnablePOP3;
                    oldPlan.EnableIMAP = updatedPlan.EnableIMAP;
                    oldPlan.EnableOWA = updatedPlan.EnableOWA;
                    oldPlan.EnableMAPI = updatedPlan.EnableMAPI;
                    oldPlan.EnableAS = updatedPlan.EnableAS;
                    oldPlan.EnableECP = updatedPlan.EnableECP;
                    oldPlan.Cost = updatedPlan.Cost;
                    oldPlan.Price = updatedPlan.Price;
                    oldPlan.AdditionalGBPrice = updatedPlan.AdditionalGBPrice;
                    oldPlan.CompanyCode = updatedPlan.CompanyCode == "0" ? string.Empty : updatedPlan.CompanyCode;

                    db.SaveChanges();

                    return Negotiate
                                .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect("~/plans/exchange/mailbox/new"));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating mailbox plan {0}: {1}", _.ID, ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };

            Delete["/{ID:int}"] = _ =>
            {
                #region Deletes a mailbox plan if its not in use
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var oldPlan = (from d in db.Plans_ExchangeMailbox
                                   where d.MailboxPlanID == id
                                   select d).FirstOrDefault();

                    if (oldPlan != null)
                    {
                        var usersUsingPlan = (from u in db.Users where u.MailboxPlan == id select u.ID).Count();
                        if (usersUsingPlan == 0)
                        {
                            db.Plans_ExchangeMailbox.Remove(oldPlan);
                            db.SaveChanges();
                        }
                        else
                            return Negotiate
                                            .WithMediaRangeModel("application/json", new { error = "Unable to delete plan because it is in use." })
                                            .WithModel(new { error = "Unable to delete plan because it is in use.", selectedPlan = oldPlan })
                                            .WithView("Plans/plans_mailbox.cshtml");
                    }

                    return Negotiate
                                .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect("~/plans/mailbox/new"));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting mailbox plan {0}: {1}", _.ID, ex.ToString());
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }

        public static List<Plans_ExchangeMailbox> GetMailboxPlans(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                List<Plans_ExchangeMailbox> plans = null;
                if (string.IsNullOrEmpty(companyCode))
                    plans = (from d in db.Plans_ExchangeMailbox
                             orderby d.MailboxPlanName
                             select d).ToList();
                else
                    plans = (from d in db.Plans_ExchangeMailbox
                             where d.CompanyCode == companyCode || string.IsNullOrEmpty(d.CompanyCode)
                             select d).ToList();

                return plans;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting mailbox plans with company code [{0}]: {1}", companyCode, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IHtmlString GetMailboxPlansOptions(string companyCode, int selectedValue)
        {
            var returnString = new StringBuilder();

            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                List<Plans_ExchangeMailbox> plans = null;
                if (string.IsNullOrEmpty(companyCode))
                    plans = (from d in db.Plans_ExchangeMailbox
                             orderby d.MailboxPlanName
                             select d).ToList();
                else
                    plans = (from d in db.Plans_ExchangeMailbox
                             where d.CompanyCode == companyCode || string.IsNullOrEmpty(d.CompanyCode)
                             select d).ToList();

                logger.DebugFormat("Found a total of {0} mailbox plans", plans.Count());
                if (plans != null)
                {
                    plans.ForEach(x =>
                        {
                            returnString.AppendFormat("<option value=\"{0}\" data-size=\"{1}\" data-maxsize=\"{2}\" data-description=\"{3}\" data-price=\"{4}\" data-additionalprice=\"{5}\"  {6}>{7}</option>",
                                x.MailboxPlanID,
                                x.MailboxSizeMB,
                                x.MaxMailboxSizeMB,
                                x.MailboxPlanDesc.Replace("\"", "'"),
                                x.Price,
                                x.AdditionalGBPrice,
                                selectedValue.Equals(x.MailboxPlanID) ? "selected" : "",
                                x.MailboxPlanName);
                        });
                }

                return new NonEncodedHtmlString(returnString.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting mailbox plans with company code [{0}]: {1}", companyCode, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }
    }
}