using log4net;
using Scheduler.Properties;
using System;
using System.ServiceProcess;
using System.Timers;

namespace Scheduler
{
    public partial class SchedulerService : ServiceBase
    {
        private readonly ILog logger = LogManager.GetLogger("Default");

        public static Timer _ExchangeMailboxSizesTimer;
        public static Timer _ExchangeMailboxDatabaseSizesTimer;

        static void Main(string[] args)
        {
            SchedulerService service = new SchedulerService();
            if (Environment.UserInteractive)
            {
                service.OnStart(args);
                Console.WriteLine("Press any key to stop the program");
                Console.Read();
                service.OnStop();
            }
            else
            {
                ServiceBase.Run(service);
            }
        }

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
                logger.DebugFormat("Retrieving minute variables from configuration");
                double _exchMbxSizesInMin = Settings.Default.Exchange_RetrieveMailboxSizes;
                double _exchMbxDbSizesInMin = Settings.Default.Exchange_RetrieveDatabaseSizes;

                logger.DebugFormat("Configuring exchange mailbox sizes timer for {0} minutes", _exchMbxSizesInMin);
                _ExchangeMailboxSizesTimer = new Timer( GetMillisecondsFromMinutes(_exchMbxSizesInMin) );
                _ExchangeMailboxSizesTimer.Elapsed += _ExchangeMailboxSizesTimer_Elapsed;
                _ExchangeMailboxSizesTimer.Start();

                logger.DebugFormat("Configuring exchange mailbox database sizes timer for {0} minutes", _exchMbxDbSizesInMin);
                _ExchangeMailboxDatabaseSizesTimer = new Timer( GetMillisecondsFromMinutes(_exchMbxDbSizesInMin) );
                _ExchangeMailboxDatabaseSizesTimer.Elapsed += _ExchangeMailboxDatabaseSizesTimer_Elapsed;
                _ExchangeMailboxDatabaseSizesTimer.Start();
            }
        }

        private void _ExchangeMailboxDatabaseSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.DebugFormat("Timer elapsed for mailbox database sizes.");
            _ExchangeMailboxDatabaseSizesTimer.Stop();

            _ExchangeMailboxDatabaseSizesTimer.Start();
        }

        private void _ExchangeMailboxSizesTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.DebugFormat("Timer elasped for mailbox sizes.");
            _ExchangeMailboxSizesTimer.Stop();

            ExchangeTasks.GetMailboxSizes();

            _ExchangeMailboxSizesTimer.Start();
        }

        private static double GetMillisecondsFromMinutes(double minutes)
        {
            return TimeSpan.FromMinutes(minutes).TotalMilliseconds;
        }

        protected override void OnStop()
        {
            logger.DebugFormat("Stopping CloudPanel Scheduler service");
        }
    }
}
