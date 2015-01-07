using CloudPanel.Base.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Reports
{
    public class ExchangeOverviewReport
    {
        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        public int TotalUsers { get; set; }

        public int TotalMailboxes { get; set; }

        public int TotalArchiveMailboxes { get; set; }

        public decimal TotalCost { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal TotalProfit { get; set; }

        public long TotalMailboxSizeUsed { get; set; }

        public long TotalMailboxSizeAllocated { get; set; }

        public long TotalArchiveSizeUsed { get; set; }

        public int TotalMailboxSizeAllocatedInMB { get; set; }

        public int TotalArchiveSizeAllocatedInMB { get; set; }

        public List<UserForReport> Users { get; set; }

        public List<Plans_ExchangeMailbox> MailboxPlans { get; set; }

        public List<Plans_ExchangeArchiving> ArchivePlans { get; set; }
    }
}
