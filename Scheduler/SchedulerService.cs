using log4net;
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
                // Start our Exchange timers
                ExchangeTasks.StartTimers();

                // Start the history timers
                HistoryTasks.StartTimers();
            }
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Stopping the CloudPanel service");

            if (ExchangeTasks._databaseSizesTimer != null)
                ExchangeTasks._databaseSizesTimer.Dispose();

            if (ExchangeTasks._mailboxSizesTimer != null)
                ExchangeTasks._mailboxSizesTimer.Dispose();

            if (ExchangeTasks._messageLogCountTimer != null)
                ExchangeTasks._messageLogCountTimer.Dispose();

            if (HistoryTasks._companyStats != null)
                HistoryTasks._companyStats.Dispose();
        }
    }
}
