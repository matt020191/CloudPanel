using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Modules.PlansModules
{
    public class ArchivePlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger("Default");

        public ArchivePlanModule() : base("/plans/exchange/archiving")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/new"] = _ =>
                {
                    return Negotiate.WithModel(new { plan = new Plans_ExchangeArchiving() })
                                    .WithView("Plans/archiving.cshtml");
                };

            Post["/new"] = _ =>
                {
                    #region Creates a new archive plan
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Creating new archive plan");
                        var plan = this.Bind<Plans_ExchangeArchiving>();

                        logger.DebugFormat("Validating required data");
                        if (!Request.Form.DisplayName.HasValue)
                            throw new MissingFieldException("", "DisplayName");

                        if (!Request.Form.Description.HasValue)
                            throw new MissingFieldException("", "Description");

                        if (!Request.Form.ArchiveSizeMB.HasValue)
                            throw new MissingFieldException("", "ArchiveSizeMB");

                        if (!Request.Form.Cost.HasValue)
                            throw new MissingFieldException("", "Cost");

                        if (!Request.Form.Price.HasValue)
                            throw new MissingFieldException("", "Price");

                        if (plan.CompanyCode.Equals("0"))
                            plan.CompanyCode = string.Empty;

                        if (plan.ArchiveSizeMB < 1)
                            plan.ArchiveSizeMB = 0;

                        logger.DebugFormat("Data has been validated. Adding to SQL");
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Plans_ExchangeArchiving.Add(plan);
                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully added new plan.", plan = new Plans_ExchangeArchiving() })
                                        .WithView("Plans/archiving.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error creating new archive plan: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
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
                    #region Gets a specific Archive Plan
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Getting archive plan {0}", _.ID);
                        db = new CloudPanelContext(Settings.ConnectionString);

                        int id = _.ID;
                        var plan = (from d in db.Plans_ExchangeArchiving
                                    where d.ArchivingID == id
                                    select d).First();

                        return Negotiate.WithModel(new { plan = plan })
                                        .WithView("Plans/archiving.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error getting archiving plan {0}: {1}", _.ID, ex.ToString());

                        ViewBag.error = ex.Message;
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
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
                    #region Updates an existing archive plan
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Updating archive plan {0}", _.ID);
                        var plan = this.Bind<Plans_ExchangeArchiving>();

                        logger.DebugFormat("Validating required data");
                        if (!Request.Form.DisplayName.HasValue)
                            throw new MissingFieldException("", "DisplayName");

                        if (!Request.Form.Description.HasValue)
                            throw new MissingFieldException("", "Description");

                        if (!Request.Form.ArchiveSizeMB.HasValue)
                            throw new MissingFieldException("", "ArchiveSizeMB");

                        if (!Request.Form.Cost.HasValue)
                            throw new MissingFieldException("", "Cost");

                        if (!Request.Form.Price.HasValue)
                            throw new MissingFieldException("", "Price");

                        logger.DebugFormat("Data has been validated. Updating to SQL");
                        db = new CloudPanelContext(Settings.ConnectionString);

                        int id = _.ID;
                        var sqlPlan = (from d in db.Plans_ExchangeArchiving
                                       where d.ArchivingID == id
                                       select d).First();

                        logger.DebugFormat("Looping through properties and updating values");
                        foreach (var p in plan.GetType().GetProperties())
                        {
                            if (!p.Name.Equals("ArchivingID"))
                            {
                                sqlPlan.GetType()
                                       .GetProperties()
                                       .Where(x => x.Name.Equals(p.Name)).First()
                                       .SetValue(sqlPlan, p.GetValue(plan, null), null);
                            }
                                   
                        }

                        if (plan.CompanyCode.Equals("0"))
                            sqlPlan.CompanyCode = string.Empty;

                        if (plan.ArchiveSizeMB < 1)
                            plan.ArchiveSizeMB = 0;

                        db.SaveChanges();

                        return Negotiate.WithModel(new { success = "Successfully updated plan.", plan = plan })
                                        .WithView("Plans/archiving.cshtml");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error updating archive plan: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
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
                    #region Deletes an archive plan 
                    CloudPanelContext db = null;
                    try
                    {
                        logger.DebugFormat("Deleting archiving plan {0}", _.ID);
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        int id = _.ID;
                        var planInUse = (from d in db.Users
                                         where d.ArchivePlan == id
                                         select d).Count();

                        if (planInUse > 0)
                            throw new Exception("Unable to delete plan because it is in use by " + planInUse + " user(s)");
                        else
                        {
                            logger.DebugFormat("Plan {0} is not in use. Attempting to remove", id);
                            var plan = (from d in db.Plans_ExchangeArchiving
                                        where d.ArchivingID == id
                                        select d).First();
                            db.Plans_ExchangeArchiving.Remove(plan);
                            db.SaveChanges();

                            return Negotiate.WithModel(new { success = "Successfully delete plan." })
                                            .WithMediaRangeResponse("text/html", this.Response.AsRedirect("~/plans/exchange/archiving/new"));
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error deleting archive plan: {0}", ex.ToString());
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithView("Error/500.cshtml")
                                        .WithStatusCode(HttpStatusCode.InternalServerError);
                    }
                    finally
                    {
                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };
        }

        public static List<Plans_ExchangeArchiving> GetArchivePlans(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                List<Plans_ExchangeArchiving> plans = null;
                if (string.IsNullOrEmpty(companyCode))
                    plans = (from d in db.Plans_ExchangeArchiving
                             orderby d.DisplayName
                             select d).ToList();
                else
                    plans = (from d in db.Plans_ExchangeArchiving
                             where string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode
                             orderby d.DisplayName
                             select d).ToList();

                return plans;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting archive plans for companycode {0}: {1}", companyCode, ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static IHtmlString GetArchivePlansWithOptions(string companyCode, int? selectedId = null)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                List<Plans_ExchangeArchiving> plans = null;
                if (string.IsNullOrEmpty(companyCode))
                    plans = (from d in db.Plans_ExchangeArchiving
                             orderby d.DisplayName
                             select d).ToList();
                else
                    plans = (from d in db.Plans_ExchangeArchiving
                             where string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode
                             orderby d.DisplayName
                             select d).ToList();


                var sb = new StringBuilder();
                foreach (var p in plans)
                {
                    string append = string.Format("<option value='{0}' data-description='{1}' {2}>{3}</option>",
                                                   p.ArchivingID,
                                                   p.Description,
                                                   (selectedId != null && p.ArchivingID == selectedId) ? "selected" : "",
                                                   p.DisplayName);
                    sb.AppendLine(append);
                }

                return new NonEncodedHtmlString(sb.ToString());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting archive plans with options for companycode {0}: {1}", companyCode, ex.ToString());
                return new NonEncodedHtmlString("");
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

    }
}