using CloudPanel.Base.Billing;
using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Code;
using CloudPanel.Database.EntityFramework;
using log4net;
using Nancy;
using Nancy.Responses.Negotiation;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.CompanyModules
{
    public class BillingModule : NancyModule
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(BillingModule));

        public BillingModule() : base("/company/{CompanyCode}/billing")
        {
            this.RequiresAuthentication();

            Get["/", c => c.Request.Accept("text/html")] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.CompanyCode));

                return View["Company/billing.cshtml"];
            };

            Post["/mailbox"] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.CompanyCode));

                return Update(Request.Form.id, Request.Form.value.Value, "Exchange", _.CompanyCode);
            };

            Post["/archive"] = _ =>
            {
                this.RequiresValidatedClaims(x => ValidateClaims.AllowSuperOrReseller(Context.CurrentUser, _.CompanyCode));

                return Update(Request.Form.id, Request.Form.value.Value, "Archive", _.CompanyCode);
            };
        }

        private Negotiator Update(int id, string value, string product, string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                logger.DebugFormat("Updating company prices for {0}. ID: {1}, Value: {2}, Product: {3}", companyCode, id, value, product);
                db = new CloudPanelContext(Settings.ConnectionString);

                var price = (from d in db.PriceOverride
                             where d.CompanyCode == companyCode
                             where d.Product == product
                             where d.PlanID == id
                             select d).FirstOrDefault();

                if (price != null)
                {
                    logger.DebugFormat("Found existing custom price in database. Updating...");
                    price.Price = value.Replace(" ", string.Empty);
                    db.SaveChanges();
                }
                else
                {
                    logger.DebugFormat("Custom price not found. Creating new...");

                    var newPrice = new PriceOverride();
                    newPrice.CompanyCode = companyCode;
                    newPrice.Product = product;
                    newPrice.PlanID = id;
                    newPrice.Price = value.Replace(" ", string.Empty);

                    db.PriceOverride.Add(newPrice);
                    db.SaveChanges();
                }

                return Negotiate.WithModel(new { success = "Updated custom pricing for the plan" })
                                .WithView("Company/billing.cshtml")
                                .WithStatusCode(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Negotiate.WithModel(new { error = ex.Message })
                                .WithView("error/500.cshtml")
                                .WithStatusCode(HttpStatusCode.InternalServerError);
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static List<CustomPrice> GetExchange(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plan = (from d in db.Plans_ExchangeMailbox
                            join o in db.PriceOverride on d.MailboxPlanID equals o.PlanID into p1
                            from s in p1.Where(x => (string.IsNullOrEmpty(companyCode) || x.CompanyCode == companyCode) && x.Product == "Exchange").DefaultIfEmpty()
                            where (string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode)
                            orderby d.MailboxPlanName
                            select new CustomPrice
                            {
                                ID = d.MailboxPlanID,
                                Name = d.MailboxPlanName,
                                DefaultPrice = d.Price,
                                Custom = s.Price == null ? d.Price : s.Price
                            }).ToList();

                return plan;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting Exchange billing: {0}", ex.ToString());
                return null;
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        public static List<CustomPrice> GetExchangeArchive(string companyCode)
        {
            CloudPanelContext db = null;
            try
            {
                db = new CloudPanelContext(Settings.ConnectionString);

                var plan = (from d in db.Plans_ExchangeArchiving
                            join o in db.PriceOverride on d.ArchivingID equals o.PlanID into p1
                            from s in p1.Where(x => (string.IsNullOrEmpty(companyCode) || x.CompanyCode == companyCode) && x.Product == "Archive").DefaultIfEmpty()
                            where (string.IsNullOrEmpty(d.CompanyCode) || d.CompanyCode == companyCode)
                            orderby d.DisplayName
                            select new CustomPrice
                            {
                                ID = d.ArchivingID,
                                Name = d.DisplayName,
                                DefaultPrice = d.Price,
                                Custom = s.Price == null ? d.Price : s.Price
                            }).ToList();

                return plan;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error getting Exchange billing: {0}", ex.ToString());
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