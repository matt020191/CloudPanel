using CloudPanel.Base.Config;
using CloudPanel.Database.EntityFramework;
using CloudPanel.Reports.Exchange;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Reports_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.reportViewer1.RefreshReport();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom("CloudPanel.Reports.dll");
            Stream stream = assembly.GetManifestResourceStream("CloudPanel.Reports.Exchange.RDLC.ExchangeSummaryReport.rdlc");
            reportViewer1.LocalReport.LoadReportDefinition(stream);

            CloudPanelContext db = new CloudPanelContext(Settings.ConnectionString);

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

            reportViewer1.LocalReport.DataSources.Add(new Microsoft.Reporting.WinForms.ReportDataSource()
            {
                Name = "Users",
                Value = users
            });

            reportViewer1.LocalReport.Refresh();
            reportViewer1.RefreshReport();
        }

        private void reportViewer1_Load(object sender, EventArgs e)
        {

        }
    }
}
