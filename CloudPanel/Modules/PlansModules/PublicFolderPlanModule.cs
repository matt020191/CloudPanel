using CloudPanel.Base.Config;
using CloudPanel.Base.Models.Database;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Security;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy.Responses.Negotiation;

namespace CloudPanel.Modules.PlansModules
{
    public class PublicFolderPlanModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PublicFolderPlanModule() : base("/plans/exchange/publicfolder")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/new"] = _ =>
            {
                return Negotiate.WithModel(new { plan = new Plans_ExchangePublicFolders() })
                                .WithView("plans/publicfolders.cshtml");
            };

            Post["/new"] = _ =>
            {
                #region Creates a new public folder plan
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    var newPlan = this.Bind<Plans_ExchangePublicFolders>(new[] { "Cost", "Price" });
                    newPlan.Cost = decimal.Parse(Request.Form.Cost.Value);
                    newPlan.Price = decimal.Parse(Request.Form.Price.Value);

                    db.Plans_ExchangePublicFolder.Add(newPlan);
                    db.SaveChanges();

                    return Negotiate.WithModel(new { plan = new Plans_ExchangePublicFolders() })
                                    .WithMediaRangeModel("application/json", HttpStatusCode.OK)
                                    .WithView("plans/publicfolders.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error creating public folder plan: {0}", ex.ToString());
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
                #region Gets an existing public folder plan
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var plan = (from d in db.Plans_ExchangePublicFolder
                                where d.ID == id
                                select d).Single();

                    return Negotiate.WithModel(new { plan = plan })
                                    .WithView("plans/publicfolders.cshtml");
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error getting public folder plan {0}: {1}", _.ID, ex.ToString());
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
                #region Updates an existing public folder plan
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var oldPlan = (from d in db.Plans_ExchangePublicFolder
                                   where d.ID == id
                                   select d).FirstOrDefault();

                    var updatedPlan = this.Bind<Plans_ExchangePublicFolders>(new[] { "Cost", "Price" });
                    oldPlan.Name = updatedPlan.Name;
                    oldPlan.Description = updatedPlan.Description;
                    oldPlan.MailboxSizeMB = updatedPlan.MailboxSizeMB;
                    oldPlan.Cost = updatedPlan.Cost;
                    oldPlan.Price = updatedPlan.Price;
                    oldPlan.CompanyCode = updatedPlan.CompanyCode;
                    oldPlan.Cost = decimal.Parse(Request.Form.Cost.Value);
                    oldPlan.Price = decimal.Parse(Request.Form.Price.Value);
                    db.SaveChanges();

                    return Negotiate.WithMediaRangeResponse(new MediaRange("text/html"),
                                                            this.Response.AsRedirect("~/plans/exchange/publicfolder/new"));
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error updating public folder plan {0}: {1}", _.ID, ex.ToString());
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
                #region Deletes a public folder plan if its not in use
                CloudPanelContext db = null;
                try
                {
                    db = new CloudPanelContext(Settings.ConnectionString);

                    int id = _.ID;
                    var plan = db.Plans_ExchangePublicFolder
                                 .Where(x => x.ID == id)
                                 .Single();

                    var companiesWithPlan = db.PublicFolderMailboxes
                                              .Where(x => x.PlanID == plan.ID)
                                              .Count();

                    if (companiesWithPlan > 0)
                    {
                        return Negotiate.WithModel(new { plan = plan, error = "Unable to delete plan because it is in use." })
                                        .WithView("plans/publicfolders.cshtml");
                    }
                    else
                    {
                        db.Plans_ExchangePublicFolder.Remove(plan);
                        db.SaveChanges();

                        return Negotiate.WithMediaRangeResponse(new MediaRange("text/html"),
                                                                this.Response.AsRedirect("~/plans/publicfolders/new"));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error deleting public folder plan {0}: {1}", _.ID, ex.ToString());
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

        public static List<Plans_ExchangePublicFolders> GetPublicFolderPlans(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                List<Plans_ExchangePublicFolders> plans = null;
                if (string.IsNullOrEmpty(companyCode))
                    plans = (from d in db.Plans_ExchangePublicFolder
                             orderby d.Name
                             select d).ToList();
                else
                    plans = (from d in db.Plans_ExchangePublicFolder
                             where d.CompanyCode == companyCode || string.IsNullOrEmpty(d.CompanyCode)
                             select d).ToList();

                return plans;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting public folder plans with company code [{0}]: {1}", companyCode, ex.ToString());
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