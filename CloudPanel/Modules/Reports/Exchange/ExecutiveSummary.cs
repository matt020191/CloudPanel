using CloudPanel.Base.Config;
using CloudPanel.Base.Database.Models;
using CloudPanel.Base.Reports;
using CloudPanel.Database.EntityFramework;
using Nancy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace CloudPanel.Modules.Reports.Exchange
{
    public class ExecutiveSummary : NancyModule
    {
        public ExecutiveSummary() : base("/reports/exchange/executivesummary")
        {
            Get["/", c => c.Request.Accept("text/html")] = _ =>
                {
                    CloudPanelContext db = null;
                    try
                    {
                        db = new CloudPanelContext(Settings.ConnectionString);
                        db.Database.Connection.Open();

                        var companies = (from d in db.Companies
                                         orderby d.CompanyName
                                         select new ExchangeOverviewReport()
                                         {
                                             CompanyCode = d.CompanyCode,
                                             CompanyName = d.CompanyName
                                         }).ToList();

                        // Loop through each company gathering information
                        companies.ForEach(x =>
                        {
                            // Get the users from SQL for this company
                            x.Users = (from d in db.Users
                                       join s in db.StatMailboxSize on d.UserGuid equals s.UserGuid into d2
                                       from mailboxinfo in d2.DefaultIfEmpty().OrderByDescending(m => m.Retrieved).Take(1)
                                       join s2 in db.StatMailboxArchiveSize on d.UserGuid equals s2.UserGuid into d3
                                       from archiveinfo in d3.DefaultIfEmpty().OrderByDescending(a => a.Retrieved).Take(1)
                                       where d.CompanyCode == x.CompanyCode
                                       orderby d.DisplayName
                                       select new UserForReport()
                                       {
                                           UserGuid = d.UserGuid,
                                           UserPrincipalName = d.UserPrincipalName,
                                           Email = d.Email,
                                           DisplayName = d.DisplayName,
                                           MailboxPlan = d.MailboxPlan == null ? 0 : (int)d.MailboxPlan,
                                           ArchivePlan = d.ArchivePlan == null ? 0 : (int)d.ArchivePlan,
                                           MailboxSizeInBytes = mailboxinfo == null ? 0 : mailboxinfo.TotalItemSizeInBytes,
                                           ArchiveSizeInBytes = archiveinfo == null ? 0 : archiveinfo.TotalItemSizeInBytes,
                                           AdditionalMB = d.AdditionalMB == null ? 0 : (int)d.AdditionalMB
                                       }).ToList();

                            x.TotalUsers = x.Users.Count;
                            x.TotalMailboxes = x.Users.Where(m => m.MailboxPlan > 0).Count();
                            x.TotalArchiveMailboxes = x.Users.Where(m => m.ArchivePlan > 0).Count();
                            x.TotalArchiveSizeAllocatedInMB = 0;
                            x.TotalMailboxSizeAllocated = 0;
                            x.TotalMailboxSizeUsed = 0;
                            x.TotalArchiveSizeUsed = 0;

                            x.MailboxPlans = new List<Plans_ExchangeMailbox>();
                            x.ArchivePlans = new List<Plans_ExchangeArchiving>();

                            // Loop through each user gathering further information
                            x.Users.ForEach(u =>
                                {
                                    // Check if the user has a mailbox plan
                                    #region Mailbox Plan
                                    if (u.MailboxPlan > 0)
                                    {
                                        // Get the mailbox plan and update the counts
                                        var plan = x.MailboxPlans.Where(m => m.MailboxPlanID == u.MailboxPlan).FirstOrDefault();
                                        if (plan == null)
                                        {
                                            var dbPlan = (from d in db.Plans_ExchangeMailbox
                                                          where d.MailboxPlanID == u.MailboxPlan
                                                          select d).First();

                                            dbPlan.CustomPrice = (from d in db.PriceOverride
                                                                  where d.CompanyCode == x.CompanyCode
                                                                  where d.Product == "Exchange"
                                                                  where d.PlanID == dbPlan.MailboxPlanID
                                                                  select d.Price).SingleOrDefault();
                                            
                                            dbPlan.UserCount = 1;
                                            x.MailboxPlans.Add(dbPlan);
                                        }
                                        else
                                        {
                                            plan.UserCount++;
                                        }

                                        x.TotalMailboxSizeUsed += u.MailboxSizeInBytes;
                                        x.TotalMailboxSizeAllocated += Math.BigMul(u.AdditionalMB, 1048576);
                                    }
                                    #endregion

                                    // Check if the user has an archive plan
                                    #region Archive Plan
                                    if (u.ArchivePlan > 0)
                                    {
                                        // Get the archive plan and update the counts
                                        var plan = x.ArchivePlans.Where(m => m.ArchivingID == u.ArchivePlan).FirstOrDefault();
                                        if (plan == null)
                                        {
                                            var dbPlan = (from d in db.Plans_ExchangeArchiving
                                                          where d.ArchivingID == u.ArchivePlan
                                                          select d).First();

                                            dbPlan.CustomPrice = (from d in db.PriceOverride
                                                                  where d.CompanyCode == x.CompanyCode
                                                                  where d.Product == "Archive"
                                                                  where d.PlanID == dbPlan.ArchivingID
                                                                  select d.Price).SingleOrDefault();

                                            dbPlan.UserCount = 1;
                                            x.ArchivePlans.Add(dbPlan);
                                        }
                                        else
                                        {
                                            plan.UserCount++;
                                        }

                                        x.TotalArchiveSizeUsed += u.ArchiveSizeInBytes;
                                    }
                                    #endregion
                                });

                            // Update the total cost and prices
                            x.MailboxPlans.ForEach(p => {
                                x.TotalCost += decimal.Parse(p.Cost, CultureInfo.InvariantCulture) * p.UserCount;
                                x.TotalPrice += string.IsNullOrEmpty(p.CustomPrice) ?
                                                decimal.Parse(p.Price, CultureInfo.InvariantCulture) * p.UserCount :
                                                decimal.Parse(p.CustomPrice, CultureInfo.InvariantCulture) * p.UserCount;
                                
                                x.TotalMailboxSizeAllocated += Math.BigMul( (p.MailboxSizeMB * 1048576), p.UserCount);
                            });
                            x.ArchivePlans.ForEach(p =>
                            {
                                x.TotalCost += decimal.Parse(p.Cost, CultureInfo.InvariantCulture) * p.UserCount;
                                x.TotalPrice += string.IsNullOrEmpty(p.CustomPrice) ?
                                                decimal.Parse(p.Price, CultureInfo.InvariantCulture) * p.UserCount :
                                                decimal.Parse(p.CustomPrice, CultureInfo.InvariantCulture) * p.UserCount;

                                x.TotalArchiveSizeAllocatedInMB += p.ArchiveSizeMB * p.UserCount;
                            });
                        });

                        return View["Reports/Exchange/ExecutiveSummary.cshtml", companies];
                    }
                    catch (Exception ex)
                    {
                        return Negotiate.WithModel(new { error = ex.Message })
                                        .WithStatusCode(HttpStatusCode.InternalServerError)
                                        .WithView("Error/500.cshtml");
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