using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using System;
using System.Linq;

namespace CloudPanel.Modules
{
    public class CompanyPlanModule : NancyModule
    {
        public CompanyPlanModule() : base("/plans/company")
        {
            this.RequiresAuthentication();
            this.RequiresAnyClaim(new[] { "SuperAdmin" } );

            Get["/new"] = _ =>
                {
                    return Negotiate.WithModel(new
                                    {
                                        selectedPlan = new Plans_Organization()
                                    })
                                    .WithView("Plans/company.cshtml");
                };

            Post["/new"] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        var newPlan = this.Bind<Plans_Organization>();

                        db.Plans_Organization.Add(newPlan);
                        db.SaveChanges();

                        return Negotiate.WithModel(new
                                    {
                                        selectedPlan = new Plans_Organization()
                                    })
                                    .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                    .WithView("Plans/company.cshtml");
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
                        var plan = (from d in db.Plans_Organization
                                    where d.OrgPlanID == id
                                    select d).FirstOrDefault();

                        return Negotiate.WithModel(new { 
                                        selectedPlan = plan
                                    })
                                    .WithMediaRangeModel("application/json", new
                                    {
                                        selectedPlan = plan
                                    })
                                    .WithView("Plans/company.cshtml");
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
                        var oldPlan = (from d in db.Plans_Organization
                                       where d.OrgPlanID == id
                                       select d).FirstOrDefault();
                        var updatedPlan = this.Bind<Plans_Organization>();
                        oldPlan.OrgPlanName = updatedPlan.OrgPlanName;
                        oldPlan.MaxUsers = updatedPlan.MaxUsers;
                        oldPlan.MaxDomains = updatedPlan.MaxDomains;
                        oldPlan.MaxExchangeMailboxes = updatedPlan.MaxExchangeMailboxes;
                        oldPlan.MaxExchangeContacts = updatedPlan.MaxExchangeContacts;
                        oldPlan.MaxExchangeDistLists = updatedPlan.MaxExchangeDistLists;
                        oldPlan.MaxExchangeResourceMailboxes = updatedPlan.MaxExchangeResourceMailboxes;
                        oldPlan.MaxExchangeMailPublicFolders = updatedPlan.MaxExchangeMailPublicFolders;
                        oldPlan.MaxTerminalServerUsers = updatedPlan.MaxTerminalServerUsers;
                        db.SaveChanges();

                        return Negotiate
                                    .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                    .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect("~/plans/company/new"));
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
                    var oldPlan = (from d in db.Plans_Organization
                                   where d.OrgPlanID == id
                                   select d).FirstOrDefault();

                    if (oldPlan != null)
                    {
                        var companiesUsingPlan = (from d in db.Companies where d.OrgPlanID == id select d.CompanyId).Count();
                        if (companiesUsingPlan == 0)
                        {
                            db.Plans_Organization.Remove(oldPlan);
                            db.SaveChanges();
                        }
                        else
                            return Negotiate
                                            .WithMediaRangeModel("application/json", new { error = "Unable to delete plan because it is in use." })
                                            .WithModel(new { error = "Unable to delete plan because it is in use.", selectedPlan = oldPlan })
                                            .WithView("Plans/company.cshtml");
                    }

                    return Negotiate
                                .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect("~/plans/company/new"));
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