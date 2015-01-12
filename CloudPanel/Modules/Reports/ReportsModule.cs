using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Reports.Exchange.Classes;
using Microsoft.Reporting.WebForms;
using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CloudPanel.Modules.Reports
{
    public class ReportsModule : NancyModule
    {
        public ReportsModule() : base("/reports")
        {
            Get["/exchange/summary"] = _ =>
                {
                    CloudPanelContext db = null;
                    Assembly assembly = null;
                    Stream stream = null;

                    ReportViewer reportViewer = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

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
                        reportViewer.LocalReport.ReportEmbeddedResource = "CloudPanel.Reports.Exchange.RDLC.ExchangeSummaryReport.rdlc";
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
                };
        }
    }
}