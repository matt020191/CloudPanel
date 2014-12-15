using CloudPanel.Base.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Reports
{
    public class ExchangeOverview
    {
        public string CompanyName { get; set; }

        public string CompanyCode { get; set; }

        public int UserCount { get; set; }

        public int MailboxCount { get; set; }

        public int ArchiveCount { get; set; }

        public decimal MailboxCost
        {
            get
            {
                if (MailboxPlans != null)
                {
                    decimal total = 0;
                    MailboxPlans.ForEach(x =>
                        {
                            total += decimal.Parse(x.Cost.Replace("$", string.Empty)) * x.UserCount;
                        });

                    return total;
                }
                else
                    return 0;
            }
        }

        public decimal MailboxPrice
        {
            get
            {
                if (MailboxPlans != null)
                {
                    decimal total = 0;
                    MailboxPlans.ForEach(x =>
                        {
                            if (string.IsNullOrEmpty(x.CustomPrice))
                                total += decimal.Parse(x.Price.Replace("$", string.Empty)) * x.UserCount;
                            else
                                total += decimal.Parse(x.CustomPrice.Replace("$", string.Empty)) * x.UserCount;
                        });

                    return total;
                }
                else
                    return 0;
            }
        }

        public decimal Profit
        {
            get
            {
                return MailboxPrice - MailboxCost;
            }
        }

        public List<Plans_ExchangeMailbox> MailboxPlans { get; set; }

        public List<Plans_ExchangeArchiving> ArchivePlans { get; set; }
    }
}
