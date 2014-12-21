using System.ServiceProcess;

namespace Scheduler
{
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new SchedulerService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
