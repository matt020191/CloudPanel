using Scheduler.Properties;
using System;
using System.ServiceProcess;
using System.Timers;

namespace Scheduler
{
    public partial class SchedulerService : ServiceBase
    {
        public static Timer _ExchangeMailboxSizesTimer = null;
        public static Timer _ExchangeMailboxDatabaseSizesTimer = null;

        public SchedulerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("CloudPanel Scheduler is starting");

            bool success = StaticSettings.LoadSettings(Settings.Default.Settings_Path);
            if (!success)
            {
                EventLog.WriteEntry("Failed to load the settings", System.Diagnostics.EventLogEntryType.Error);
                this.Stop();
            }
            else
            {
                double _exchMbxSizesInMin = Settings.Default.Exchange_RetrieveMailboxSizes;
                double _exchMbxDbSizesInMin = Settings.Default.Exchange_RetrieveDatabaseSizes;

                _ExchangeMailboxSizesTimer = new Timer( GetMillisecondsFromMinutes(_exchMbxSizesInMin) );
                _ExchangeMailboxSizesTimer.Elapsed += _ExchangeMailboxSizesTimer_Elapsed;
                _ExchangeMailboxSizesTimer.Start();

                _ExchangeMailboxDatabaseSizesTimer = new Timer( GetMillisecondsFromMinutes(_exchMbxDbSizesInMin) );
                _ExchangeMailboxDatabaseSizesTimer.Elapsed += _ExchangeMailboxDatabaseSizesTimer_Elapsed;
                _ExchangeMailboxDatabaseSizesTimer.Start();
            }
        }

        #region Timer Elapsed

        private void _ExchangeMailboxDatabaseSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EventLog.WriteEntry("Time elasped for mailbox database sizes... querying...");
            _ExchangeMailboxDatabaseSizesTimer.Stop();

            // Query the mailbox database sizes
            ExchangeTasks.GetMailboxDatabaseSizes();

            // Start the timer agin
            _ExchangeMailboxDatabaseSizesTimer.Start();
        }

        private void _ExchangeMailboxSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            EventLog.WriteEntry("Time elasped for mailbox sizes... querying...");
            _ExchangeMailboxSizesTimer.Stop();

            // Query the mailbox sizes
            ExchangeTasks.GetMailboxSizes();

            // Start the timer again
            _ExchangeMailboxSizesTimer.Start();
        }

        #endregion

        #region Private Methods 

        private static double GetMillisecondsFromMinutes(double minutes)
        {
            return TimeSpan.FromMinutes(minutes).TotalMilliseconds;
        }

        #endregion

        protected override void OnStop()
        {
            EventLog.WriteEntry("Stopping the CloudPanel service");

            if (_ExchangeMailboxDatabaseSizesTimer != null)
                _ExchangeMailboxDatabaseSizesTimer.Dispose();

            if (_ExchangeMailboxSizesTimer != null)
                _ExchangeMailboxSizesTimer.Dispose();
        }
    }
}
