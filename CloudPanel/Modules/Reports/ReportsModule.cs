using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Reports.Exchange;
using log4net;
using Microsoft.Reporting.WebForms;
using Nancy;
using Nancy.Security;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using CloudPanel.Reports.Citrix;

namespace CloudPanel.Modules.Reports
{
    public class ReportsModule : NancyModule
    {
        private readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ReportsModule() : base("/reports")
        {
            this.RequiresClaims(new[] { "SuperAdmin" });

            Get["/"] = _ =>
                {
                    return View["Reports/reports.cshtml"];
                };

            Get["/exchange/summary"] = _ =>
                {
                    #region Exchange global summary report

                    CloudPanelContext db = null;
                    Assembly assembly = null;
                    Stream stream = null;

                    ReportViewer reportViewer = null;
                    try
                    { 
                        logger.DebugFormat("Generating Exchange summary report..");
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        string companyCode = string.Empty;
                        if (Request.Query.CompanyCode.HasValue)
                            companyCode = Request.Query.CompanyCode.Value;

                        var users = (from d in db.Users
                                     join c in db.Companies on d.CompanyCode equals c.CompanyCode into c1
                                     from company in c1.DefaultIfEmpty()
                                     join m in db.Plans_ExchangeMailbox on d.MailboxPlan equals m.MailboxPlanID into d1
                                     from mailboxplan in d1.DefaultIfEmpty()
                                     join a in db.Plans_ExchangeArchiving on d.ArchivePlan equals a.ArchivingID into d2
                                     from archiveplan in d2.DefaultIfEmpty()
                                     join s in db.StatMailboxSize on d.UserGuid equals s.UserGuid into d3
                                     from mailboxsize in d3.OrderByDescending(x => x.Retrieved).DefaultIfEmpty().Take(1)
                                     join s2 in db.StatMailboxArchiveSize on d.UserGuid equals s2.UserGuid into d4
                                     from archivesize in d4.OrderByDescending(x => x.Retrieved).DefaultIfEmpty().Take(1)
                                     where d.MailboxPlan > 0
                                     select new ExchangeSummaryData()
                                     {
                                         CompanyCode = d.CompanyCode,
                                         CompanyName = company.CompanyName,
                                         UserGuid = d.UserGuid,
                                         UserPrincipalName = d.UserPrincipalName,
                                         UserDisplayName = d.DisplayName,
                                         MailboxPlan = d.MailboxPlan == null ? 0 : (int)d.MailboxPlan,
                                         ArchivePlan = d.ArchivePlan == null ? 0 : (int)d.ArchivePlan,
                                         MailboxAdditionalMB = d.AdditionalMB == null ? 0 : (int)d.AdditionalMB,
                                         MailboxPlanName = mailboxplan.MailboxPlanName,
                                         MailboxPlanSizeInMB = mailboxplan.MailboxSizeMB,
                                         MailboxPlanCost = mailboxplan.Cost,
                                         MailboxPlanPrice = mailboxplan.Price,
                                         MailboxSizeInBytes = mailboxsize == null ? 0 : mailboxsize.TotalItemSizeInBytes,
                                         ArchivePlanName = archiveplan == null ? string.Empty : archiveplan.DisplayName,
                                         ArchivePlanCost = archiveplan == null ? 0 : archiveplan.Cost,
                                         ArchivePlanPrice = archiveplan == null ? 0 : archiveplan.Price,
                                         ArchiveSizeInBytes = archivesize == null ? 0 : archivesize.TotalItemSizeInBytes
                                     }).ToList();

                        logger.DebugFormat("Found a total of {0} users for the Exchange summary report.. checking if we are limiting results", users.Count);
                        if (!string.IsNullOrEmpty(companyCode))
                        {
                            logger.DebugFormat("Limiting results to {0}", companyCode);
                            users = users.Where(x => x.CompanyCode == companyCode).ToList();
                        }

                        logger.DebugFormat("Continuing with {0} users", users.Count);
                        var priceoverride = db.PriceOverride.ToList();
                        users.ForEach(x =>
                        {
                            if (priceoverride != null)
                            {
                                var customMailboxPrice = priceoverride.Where(p => p.CompanyCode == x.CompanyCode &&
                                                                                      p.PlanID == x.MailboxPlan &&
                                                                                      p.Product == "Exchange").FirstOrDefault();
                                x.MailboxPlanPriceCustom = customMailboxPrice == null ? null : (decimal?)customMailboxPrice.Price;

                                if (x.ArchivePlan > 0)
                                {
                                    var customArchivePrice = priceoverride.Where(p => p.CompanyCode == x.CompanyCode &&
                                                                                      p.PlanID == x.ArchivePlan &&
                                                                                      p.Product == "Archive").FirstOrDefault();
                                    x.ArchivePlanPriceCustom = customArchivePrice == null ? null : (decimal?)customArchivePrice.Price;
                                }
                            }
                        });

                        //stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CloudPanel.Reports.Exchange.RDLC.ExchangeSummaryReport.rdlc");

                        reportViewer = new ReportViewer();
                        reportViewer.LocalReport.ReportEmbeddedResource = "CloudPanel.Reports.Exchange.ExchangeSummaryReport.rdlc";
                        reportViewer.LocalReport.DataSources.Add(new ReportDataSource()
                        {
                            Name = "Users",
                            Value = users
                        });

                        reportViewer.LocalReport.Refresh();
                        byte[] reportData = reportViewer.LocalReport.Render("pdf");

                        return Response.FromByteArray(reportData, "application/pdf");
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithModel(new { error = ex.ToString() })
                                        .WithStatusCode(HttpStatusCode.InternalServerError)
                                        .WithView("Error/500.cshtml");
                    }
                    finally
                    {
                        if (reportViewer != null)
                            reportViewer.Dispose();

                        if (stream != null)
                            stream.Dispose();

                        if (assembly != null)
                            assembly = null;

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/exchange/detailed"] = _ =>
                {
                    #region Exchange detailed report

                    CloudPanelContext db = null;
                    Assembly assembly = null;
                    Stream stream = null;

                    ReportViewer reportViewer = null;
                    try
                    {
                        logger.DebugFormat("Generating Exchange detailed report..");
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        string companyCode = string.Empty;
                        if (Request.Query.CompanyCode.HasValue)
                            companyCode = Request.Query.CompanyCode.Value;

                        var users = (from d in db.Users
                                     join c in db.Companies on d.CompanyCode equals c.CompanyCode into c1
                                     from company in c1.DefaultIfEmpty()
                                     join m in db.Plans_ExchangeMailbox on d.MailboxPlan equals m.MailboxPlanID into d1
                                     from mailboxplan in d1.DefaultIfEmpty()
                                     join a in db.Plans_ExchangeArchiving on d.ArchivePlan equals a.ArchivingID into d2
                                     from archiveplan in d2.DefaultIfEmpty()
                                     join s in db.StatMailboxSize on d.UserGuid equals s.UserGuid into d3
                                     from mailboxsize in d3.OrderByDescending(x => x.Retrieved).DefaultIfEmpty().Take(1)
                                     join s2 in db.StatMailboxArchiveSize on d.UserGuid equals s2.UserGuid into d4
                                     from archivesize in d4.OrderByDescending(x => x.Retrieved).DefaultIfEmpty().Take(1)
                                     where d.MailboxPlan > 0
                                     select new ExchangeDetailedData()
                                     {
                                         CompanyCode = d.CompanyCode,
                                         CompanyName = company.CompanyName,
                                         UserGuid = d.UserGuid,
                                         UserPrincipalName = d.UserPrincipalName,
                                         UserDisplayName = d.DisplayName,
                                         MailboxPlan = d.MailboxPlan == null ? 0 : (int)d.MailboxPlan,
                                         ArchivePlan = d.ArchivePlan == null ? 0 : (int)d.ArchivePlan,
                                         MailboxAdditionalMB = d.AdditionalMB == null ? 0 : (int)d.AdditionalMB,
                                         MailboxPlanName = mailboxplan.MailboxPlanName,
                                         MailboxPlanSizeInMB = mailboxplan.MailboxSizeMB,
                                         MailboxPlanCost = mailboxplan.Cost,
                                         MailboxPlanPrice = mailboxplan.Price,
                                         MailboxSizeInBytes = mailboxsize == null ? 0 : mailboxsize.TotalItemSizeInBytes,
                                         ArchivePlanName = archiveplan == null ? string.Empty : archiveplan.DisplayName,
                                         ArchivePlanCost = archiveplan == null ? 0 : archiveplan.Cost,
                                         ArchivePlanPrice = archiveplan == null ? 0 : archiveplan.Price,
                                         ArchiveSizeInBytes = archivesize == null ? 0 : archivesize.TotalItemSizeInBytes
                                     }).ToList();

                        logger.DebugFormat("Found a total of {0} users for the Exchange detailed report.. checking if we are limiting results", users.Count);
                        if (!string.IsNullOrEmpty(companyCode))
                        {
                            logger.DebugFormat("Limiting results to {0}", companyCode);
                            users = users.Where(x => x.CompanyCode == companyCode).ToList();
                        }

                        logger.DebugFormat("Continuing with {0} users", users.Count);
                        var priceoverride = db.PriceOverride.ToList();
                        users.ForEach(x =>
                        {
                            if (priceoverride != null)
                            {
                                var customMailboxPrice = priceoverride.Where(p => p.CompanyCode == x.CompanyCode &&
                                                                                      p.PlanID == x.MailboxPlan &&
                                                                                      p.Product == "Exchange").FirstOrDefault();
                                x.MailboxPlanPriceCustom = customMailboxPrice == null ? null : (decimal?)customMailboxPrice.Price;

                                if (x.ArchivePlan > 0)
                                {
                                    var customArchivePrice = priceoverride.Where(p => p.CompanyCode == x.CompanyCode &&
                                                                                      p.PlanID == x.ArchivePlan &&
                                                                                      p.Product == "Archive").FirstOrDefault();
                                    x.ArchivePlanPriceCustom = customArchivePrice == null ? null : (decimal?)customArchivePrice.Price;
                                }
                            }
                        });

                        //stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CloudPanel.Reports.Exchange.RDLC.ExchangeSummaryReport.rdlc");

                        reportViewer = new ReportViewer();
                        reportViewer.LocalReport.ReportEmbeddedResource = "CloudPanel.Reports.Exchange.ExchangeDetails.rdlc";
                        reportViewer.LocalReport.DataSources.Add(new ReportDataSource()
                        {
                            Name = "Users",
                            Value = users
                        });

                        reportViewer.LocalReport.Refresh();
                        byte[] reportData = reportViewer.LocalReport.Render("pdf");

                        return Response.FromByteArray(reportData, "application/pdf");
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithModel(new { error = ex.ToString() })
                                        .WithStatusCode(HttpStatusCode.InternalServerError)
                                        .WithView("Error/500.cshtml");
                    }
                    finally
                    {
                        if (reportViewer != null)
                            reportViewer.Dispose();

                        if (stream != null)
                            stream.Dispose();

                        if (assembly != null)
                            assembly = null;

                        if (db != null)
                            db.Dispose();
                    }
                    #endregion
                };

            Get["/citrix/summary"] = _ =>
            {
                #region Citrix summary report

                CloudPanelContext db = null;
                Assembly assembly = null;
                Stream stream = null;

                ReportViewer reportViewer = null;
                try
                {
                    logger.DebugFormat("Generating Citrix summary report..");
                    db = new CloudPanelContext(Settings.ConnectionString);
                    db.Database.Connection.Open();

                    string companyCode = string.Empty;
                    if (Request.Query.CompanyCode.HasValue)
                        companyCode = Request.Query.CompanyCode.Value;


                    var desktopGroups = new List<CitrixDesktopGroupData>();
                    var applications = new List<CitrixAppsData>();

                    var citrixDesktopGroups = (from d in db.CitrixDesktopGroup orderby d.Name select d).ToList();
                    citrixDesktopGroups.ForEach(x =>
                        {
                            var data = (from d in db.Users
                                                     .Include(c => c.CitrixDesktopGroups)
                                         join c in db.Companies on d.CompanyCode equals c.CompanyName into c1
                                         from company in c1.DefaultIfEmpty()
                                         where d.CitrixDesktopGroups.Any(a => a.Name == x.Name)
                                         select new CitrixDesktopGroupData()
                                         {
                                              CompanyCode = d.CompanyCode,
                                              CompanyName = company.CompanyName,
                                              DesktopGroupName = x.Name,
                                              UserGuid = d.UserGuid,
                                              UserDisplayName = d.DisplayName,
                                              UserPrincipalName = d.UserPrincipalName
                                         }).ToList();

                            if (data.Count > 0) 
                            {
                                desktopGroups.AddRange(data);
                            }
                        });

                    var citrixApplications = (from d in db.CitrixApplication.Include(x => x.DesktopGroups) orderby d.Name select d).ToList();
                    citrixApplications.ForEach(x =>
                        {
                            var data = (from d in db.Users
                                                    .Include(c => c.CitrixApplications)
                                        join c in db.Companies on d.CompanyCode equals c.CompanyName into c1
                                        from company in c1.DefaultIfEmpty()
                                        where d.CitrixApplications.Any(a => a.Name == x.Name)
                                        select new CitrixAppsData()
                                        {
                                            CompanyCode = d.CompanyCode,
                                            CompanyName = company.CompanyName,
                                            ApplicationName = x.ApplicationName,
                                            UserDisplayName = d.DisplayName,
                                            UserGuid = d.UserGuid,
                                            UserPrincipalName = d.UserPrincipalName
                                        }).ToList();

                            if (data.Count > 0)
                            {
                                applications.AddRange(data);
                            }
                        });

                    logger.DebugFormat("Found a total of {0} desktop groups for the Citrix summary report", desktopGroups.Count);
                    logger.DebugFormat("Found a total of {0} applications for the Citrix summary report", applications.Count);

                    reportViewer = new ReportViewer();
                    reportViewer.LocalReport.ReportEmbeddedResource = "CloudPanel.Reports.Citrix.CitrixSummary.rdlc";
                    reportViewer.LocalReport.DataSources.Add(new ReportDataSource() { Name = "DesktopGroups", Value = desktopGroups });
                    reportViewer.LocalReport.DataSources.Add(new ReportDataSource() { Name = "Applications", Value = applications });

                    reportViewer.LocalReport.Refresh();
                    byte[] reportData = reportViewer.LocalReport.Render("pdf");
                    return Response.FromByteArray(reportData, "application/pdf");
                }
                catch (Exception ex)
                {
                    return Negotiate.WithModel(new { error = ex.ToString() })
                                    .WithStatusCode(HttpStatusCode.InternalServerError)
                                    .WithView("Error/500.cshtml");
                }
                finally
                {
                    if (reportViewer != null)
                        reportViewer.Dispose();

                    if (stream != null)
                        stream.Dispose();

                    if (assembly != null)
                        assembly = null;

                    if (db != null)
                        db.Dispose();
                }
                #endregion
            };
        }
    }
}