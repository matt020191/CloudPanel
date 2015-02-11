using CloudPanel.Base.Models.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Reports.Exchange
{
    public class ExchangeSummaryData
    {
        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        //
        // User Data
        //
        public Guid UserGuid { get; set; }

        public string UserPrincipalName { get; set; }

        public string UserDisplayName { get; set; }

        //
        // Mailbox Data
        //
        public int MailboxPlan { get; set; }

        public string MailboxPlanName { get; set; }

        public int MailboxPlanSizeInMB { get; set; }

        public int MailboxAdditionalMB { get; set; }

        public decimal MailboxPlanCost { get; set; }

        public decimal MailboxPlanPrice { get; set; }

        public decimal? MailboxPlanPriceCustom { get; set; }

        public decimal MailboxPlanAdditionalPrice { get; set; }

        public long MailboxSizeInBytes { get; set; }

        //
        // Archiving Data
        //
        public int ArchivePlan { get; set; }

        public string ArchivePlanName { get; set; }

        public decimal ArchivePlanCost { get; set; }

        public decimal ArchivePlanPrice { get; set; }

        public long ArchiveSizeInBytes { get; set; }

        public decimal? ArchivePlanPriceCustom { get; set; }

        //
        // Getters only
        //
        public int MailboxAllocated
        {
            get
            {
                return MailboxPlanSizeInMB + MailboxAdditionalMB;
            }
        }

        public string MailboxAllocatedReadable
        {
            get
            {
                return ByteSize.ByteSize.FromMegaBytes(MailboxAllocated).ToString("#.##");
            }
        }

        public string MailboxSizeReadable
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(MailboxSizeInBytes).ToString("#.##");
            }
        }
    }
}
