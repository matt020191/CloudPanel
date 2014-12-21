using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.PlansModules
{
    public class CompanyPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Default");

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
                                    .WithView("Plans/plans_company.cshtml");
                };

            Post["/new"] = _ =>
                {
                    #region Creates a new company plan
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
                                    .WithView("Plans/plans_company.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error creating new company plan: {0}", ex.ToString());
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
                    #region Gets a specific company plan
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
                                    .WithView("Plans/plans_company.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting company plan {0}: {1}", _.ID, ex.ToString());
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
                    #region Updates an existing company plan
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);

                        int id = _.ID;
                        var oldPlan = (from d in db.Plans_Organization
                                       where d.OrgPlanID == id
                                       select d).FirstOrDefault();

                        var updatedPlan = this.Bind<Plans_Organization>();
                        foreach (var p in updatedPlan.GetType().GetProperties())
                        {
                            if (!p.Name.Equals("OrgPlanID"))
                            {
                                object value = p.GetValue(updatedPlan, null);
                                oldPlan.GetType()
                                       .GetProperty(p.Name)
                                       .SetValue(oldPlan, value, null);
                            }
                        }
                        db.SaveChanges();

                        return Negotiate
                                    .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                    .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect("~/plans/company/new"));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating company plan {0}: {1}", _.ID, ex.ToString());
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
                    #region Deletes a specific company plan
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
                                return Negotiate.WithMediaRangeModel("application/json", new { error = "Unable to delete plan because it is in use." })
                                                .WithModel(new { error = "Unable to delete plan because it is in use.", selectedPlan = oldPlan })
                                                .WithView("Plans/plans_company.cshtml");
                        }

                        return Negotiate
                                    .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                    .WithMediaRangeResponse(new MediaRange("text/html"), this.Response.AsRedirect("~/plans/company/new"));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error deleting comapny plan {0}: {1}", _.ID, ex.ToString());
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

        public static List<Plans_Organization> GetCompanyPlans()
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);
                var plans = (from d in db.Plans_Organization
                            orderby d.OrgPlanName
                            select d).ToList();

                return plans;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting company plans: {0}", ex.ToString());
                throw;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

    }
}