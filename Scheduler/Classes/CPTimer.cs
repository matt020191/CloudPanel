using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Classes
{
    public class CPTimer : System.Timers.Timer
    {
        public readonly string Name;

        public CPTimer(string name)
        {
            this.Name = name;
        }
    }
}
