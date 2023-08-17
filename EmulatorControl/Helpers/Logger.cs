using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmulatorControl.Helpers
{
    class Logger
    {
        private static Logger? instance;
        private ICollection<string> logs;

        private Logger(ICollection<string> logs)
        {
            this.logs = logs;
        }

        public static Logger Create(ICollection<string> logs)
        {
            if(instance == null)
            {
                instance = new Logger(logs);
            }
            return instance;
        }

        public void Log(string message)
        {
            if (logs != null)
            {
                logs.Add(message);
            }
        }
    }
}
