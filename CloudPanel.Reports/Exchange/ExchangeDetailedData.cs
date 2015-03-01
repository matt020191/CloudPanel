using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CloudPanel.Reports.Exchange
{

    public class ExchangeDetails
    {
        private List<ExchangeDetailedData> e_detaileddata;
        private List<MessageLogDataGlobal> e_messagelogdataglobal;

        public ExchangeDetails()
        {

        }

        public ExchangeDetails(List<ExchangeDetailedData> detaileddata, List<MessageLogDataGlobal> messagelogdataglobal)
        {
            e_detaileddata = detaileddata;
            e_messagelogdataglobal = messagelogdataglobal;
        }

        public List<ExchangeDetailedData> GetDetails()
        {
            return e_detaileddata;
        }

        public List<MessageLogDataGlobal> GetMessageLogDataGlobal()
        {
            return e_messagelogdataglobal;
        }
    }

    public class ExchangeDetailedData
    {
        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        //
        // User Data
        //
        public int UserID { get; set; }

        public Guid UserGuid { get; set; }

        public bool IsEnabled { get; set; }

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

        public double MailboxSizeInBytes { get; set; }

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
        // Messages Sent / Received
        //
        public int MessageLogSent { get; set; }

        public int MessageLogReceived { get; set; }

        public long MessageLogSentBytes { get; set; }

        public double MessageLogSentInMB { get { return ByteSize.ByteSize.FromBytes(MessageLogSentBytes).MegaBytes; } }

        public long MessageLogReceivedBytes { get; set; }

        public double MessageLogReceivedInMB { get { return ByteSize.ByteSize.FromBytes(MessageLogReceivedBytes).MegaBytes; } }

        public DateTime MessageLogStart { get; set; }

        public DateTime MessageLogEnd { get; set; }

        //
        // Getters only
        //

        public double MailboxAllocatedInBytes
        {
            get
            {
                int total = MailboxPlanSizeInMB + MailboxAdditionalMB;
                return ByteSize.ByteSize.FromMegaBytes(total).Bytes;
            }
        }

        public double MailboxLeftOverInBytes
        {
            get
            {
                double leftOver = MailboxAllocatedInBytes - MailboxSizeInBytes;
                return leftOver;
            }
        }

        public string MailboxAllocatedReadable
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(MailboxAllocatedInBytes).ToString("#.##");
            }
        }

        public string MailboxAllocatedLeftReadable
        {
            get
            {
                double leftOver = MailboxAllocatedInBytes - MailboxSizeInBytes;
                return ByteSize.ByteSize.FromBytes(leftOver).ToString("#.##");
            }
        }

        public string MailboxSizeReadable
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(MailboxSizeInBytes).ToString("#.##");
            }
        }

        public string MessageLogSentReadable
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(MessageLogSentBytes).ToString("#.##");
            }
        }

        public string MessageLogReceivedReadable
        {
            get
            {
                return ByteSize.ByteSize.FromBytes(MessageLogReceivedBytes).ToString("#.##");
            }
        }
    }

    public class MessageLogDataGlobal
    {
        public DateTime Retrieved { get; set; }

        public int TotalSent {get; set; }

        public int TotalReceived {get; set; }

        public long TotalBytesSent {get; set; }

        public double TotalBytesSentInGB { get { return ByteSize.ByteSize.FromBytes(TotalBytesSent).GigaBytes; } }

        public long TotalBytesReceived { get; set; }

        public double TotalBytesReceivedInGB { get { return ByteSize.ByteSize.FromBytes(TotalBytesReceived).GigaBytes; } }
    }
}
