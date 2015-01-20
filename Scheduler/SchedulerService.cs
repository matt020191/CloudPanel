﻿using log4net;
using Scheduler.Classes;
using Scheduler.Properties;
using System;
using System.ServiceProcess;
using System.Timers;

namespace Scheduler
{
    public partial class SchedulerService : ServiceBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static Timer _ExchangeMailboxSizesTimer = null;
        public static Timer _ExchangeMailboxDatabaseSizesTimer = null;
        public static Timer _HistoryStatisticsTimer = null;

        public SchedulerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            bool success = StaticSettings.LoadSettings(Settings.Default.Settings_Path);
            if (!success)
            {
                EventLog.WriteEntry("Failed to load the settings", System.Diagnostics.EventLogEntryType.Error);
                this.Stop();
            }
            else
            {
                StartTimers();
            }
        }

        private void StartTimers()
        {
            int _exchMbxSizesInMin = Settings.Default.Exchange_RetrieveMailboxSizes;
            int _exchMbxDbSizesInMin = Settings.Default.Exchange_RetrieveDatabaseSizes;
            int _historyStatsInMin = Settings.Default.History_Statistics;

            _ExchangeMailboxSizesTimer = new Timer(GetMillisecondsFromMinutes(_exchMbxSizesInMin));
            _ExchangeMailboxSizesTimer.Elapsed += _ExchangeMailboxSizesTimer_Elapsed;
            _ExchangeMailboxSizesTimer.Start();
            logger.DebugFormat("Querying mailbox sizes in {0}", TimeSpan.FromMilliseconds(_ExchangeMailboxSizesTimer.Interval));

            _ExchangeMailboxDatabaseSizesTimer = new Timer(GetMillisecondsFromMinutes(_exchMbxDbSizesInMin));
            _ExchangeMailboxDatabaseSizesTimer.Elapsed += _ExchangeMailboxDatabaseSizesTimer_Elapsed;
            _ExchangeMailboxDatabaseSizesTimer.Start();
            logger.DebugFormat("Querying mailbox database sizes in {0}", TimeSpan.FromMilliseconds(_ExchangeMailboxDatabaseSizesTimer.Interval));

            _HistoryStatisticsTimer = new Timer(GetMillisecondsFromMinutes(_historyStatsInMin));
            _HistoryStatisticsTimer.Elapsed += _HistoryStatisticsTimer_Elapsed;
            _HistoryStatisticsTimer.Start();
            logger.DebugFormat("Querying history statistics in {0}", TimeSpan.FromMilliseconds(_HistoryStatisticsTimer.Interval));
        }

        #region Timer Elapsed

        private void _ExchangeMailboxDatabaseSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.Debug("Time elasped for mailbox database sizes... querying...");
            _ExchangeMailboxDatabaseSizesTimer.Stop();

            // Query the mailbox database sizes
            ExchangeTasks.GetMailboxDatabaseSizes();

            // Start the timer agin
            _ExchangeMailboxDatabaseSizesTimer.Start();
        }

        private void _ExchangeMailboxSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.Debug("Time elasped for mailbox sizes... querying...");
            _ExchangeMailboxSizesTimer.Stop();

            // Query the mailbox sizes
            ExchangeTasks.GetMailboxSizes();

            // Start the timer again
            _ExchangeMailboxSizesTimer.Start();
        }

        private void _HistoryStatisticsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.Debug("Time elasped for mailbox sizes... querying...");
            _HistoryStatisticsTimer.Stop();

            // Query statistics
            HistoryTasks.UpdateCompanyStatistics();

            // Start the timer again
            _HistoryStatisticsTimer.Start();
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

            if (_HistoryStatisticsTimer != null)
                _HistoryStatisticsTimer.Dispose();
        }
    }
}