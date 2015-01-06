﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Reports
{
    public class ExchangeOverviewUserReport
    {
        public Guid UserGuid { get; set; }

        public string UserPrincipalName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public int MailboxPlan { get; set; }

        public int ArchivePlan { get; set; }

        public long MailboxSizeInBytes { get; set; }

        public long ArchiveSizeInBytes { get; set; }
    }
}
