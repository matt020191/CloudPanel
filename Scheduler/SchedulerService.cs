using log4net;
using log4net.Config;
using Scheduler.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Scheduler
{
    public partial class SchedulerService : ServiceBase
    {
        private readonly ILog logger = LogManager.GetLogger("Default");

        public static Timer _ExchangeMailboxSizesTimer;
        public static Timer _ExchangeMailboxDatabaseSizesTimer;

        public SchedulerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            logger.DebugFormat("Loading settings");
            bool success = StaticSettings.LoadSettings(Settings.Default.Settings_Path);
            if (!success)
                this.Stop();
            else
            {
                logger.DebugFormat("Configuring exchange mailbox sizes timer");
                _ExchangeMailboxSizesTimer = new Timer(Settings.Default.Exchange_RetrieveMailboxSizes);
                _ExchangeMailboxSizesTimer.Elapsed += _ExchangeMailboxSizesTimer_Elapsed;
                _ExchangeMailboxSizesTimer.Start();

                logger.DebugFormat("Configuring exchange mailbox database sizes timer");
                _ExchangeMailboxDatabaseSizesTimer = new Timer(Settings.Default.Exchange_RetrieveDatabaseSizes);
                _ExchangeMailboxDatabaseSizesTimer.Elapsed += _ExchangeMailboxDatabaseSizesTimer_Elapsed;
                _ExchangeMailboxDatabaseSizesTimer.Start();
            }
        }

        void _ExchangeMailboxDatabaseSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        void _ExchangeMailboxSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
        }

        protected override void OnStop()
        {
            logger.DebugFormat("Stopping CloudPanel Scheduler service");
        }
    }
}
