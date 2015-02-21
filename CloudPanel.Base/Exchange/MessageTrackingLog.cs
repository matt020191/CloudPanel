using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Exchange
{
    public class MessageTrackingLog
    {
        public DateTime Timestamp { get; set; }

        public string ServerHostname { get; set; }

        public string Source { get; set; }

        public string EventId { get; set; }

        public List<string> Users { get; set; }

        public long TotalBytes { get; set; }
    }
}
