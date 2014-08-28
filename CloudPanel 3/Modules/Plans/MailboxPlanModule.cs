using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using System;
using System.Linq;

namespace CloudPanel.Modules.Plans
{
    public class MailboxPlanModule : NancyModule
    {
        public MailboxPlanModule() : base("/plans/exchange/mailbox")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin" });

            Get["/new"] = _ =>
            {
                return Negotiate.WithModel(new
                                {
                                    selectedPlan = new Plans_ExchangeMailbox()
                                })
                                .WithView("Plans/plans_mailbox.cshtml");
            };

            Post["/new"] = _ =>
            {
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var newPlan = this.Bind<Plans_ExchangeMailbox>();
                    db.Plans_ExchangeMailbox.Add(newPlan);
                    db.SaveChanges();

                    return Negotiate.WithModel(new
                                {
                                    selectedPlan = new Plans_Organization()
                                })
                                .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                .WithView("Plans/plans_mailbox.cshtml");
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Get["/{ID:int}"] = _ =>
            {
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
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Post["/{ID:int}"] = _ =>
            {
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
                    oldPlan.MaxMailboxSizeMB = updatedPlan.MaxMailboxSizeMB;
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
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };

            Delete["/{ID:int}"] = _ =>
            {
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
                    throw;
                }
                finally
                {
                    if (db != null)
                        db.Dispose();
                }
            };
        }
    }
}