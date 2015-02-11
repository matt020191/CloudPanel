using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Models.ViewModels
{
    public enum ActionToTake
    {
        Enable = 0,
        Change = 2,
        Disable = 1
    }

    public enum EmailFormat
    {
        Username = 0,
        Firstname = 1,
        Lastname = 2,
        FirstNameDotLastname = 3,
        FirstnameLastname = 4,
        LastnameDotFirstname = 5,
        LastnameFirstname = 6,
        FirstInitialLastname = 7,
        LastnameFirstInitial = 8
    }

    public class ExchangeBulkEditViewModel
    {
        public string[] CheckedUsers { get; set; }

        public int MailboxPlan { get; set; }

        public int SizeInMB { get; set; }

        public int EmailDomain { get; set; }

        public int ActiveSyncPlan { get; set; }

        public EmailFormat EmailFormat { get; set; }
    }
}
