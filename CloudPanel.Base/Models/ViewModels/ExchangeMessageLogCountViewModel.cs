using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Base.Models.ViewModels
{
    public class ExchangeMessageLogCountViewModel
    {
        public DateTime? Retrieved { get; set; }

        public int TotalSent { get; set; }

        public int TotalReceived { get; set; }

        public long TotalBytesSent { get; set; }

        public long TotalBytesReceived { get; set; }

        public double TotalBytesSentInMB
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(TotalBytesSent).MegaBytes;
            }
        }

        public double TotalBytesReceivedInMB
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(TotalBytesReceived).MegaBytes;
            }
        }

        public double TotalBytesSentInGB
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(TotalBytesSent).GigaBytes;
            }
        }

        public double TotalBytesReceivedInGB
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(TotalBytesReceived).GigaBytes;
            }
        }
    }
}
