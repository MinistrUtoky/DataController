using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenStatistics.classes
{
    internal class ProcessTimers
    {
        private static Stopwatch masterWatch = Stopwatch.StartNew();
        public static void TimerTest()
        {
            List<Delegate> delegates = new List<Delegate>()
            {
                () => DBClass.GetDataTableByName("library"),
                () => DBClass.GetDataTableByName("user"),
                () => DBClass.GetDataTableByName("player"),
                () => DBClass.GetDataTableByName("archive"),
                () => DBClass.GetDataTableByName("dedicated_server"),
                () => DBClass.GetDataTableByName("session"),
                () => DBClass.GetDataTableByName("lobby")
            };
            foreach (long l in SeveralProcessesTimeInMilliseconds(delegates))
            {
                Trace.WriteLine(l);
            }
        }
        public static List<long> SeveralProcessesTimeInMilliseconds(List<Delegate> processes)
        {
            List<long> times = new();
            foreach (Delegate d in processes)
                times.Add(ProcessTimeInMilliseconds(d));
            return times;
        }
        public static long ProcessTimeInMilliseconds(Delegate process)
        {
            masterWatch.Restart();
            process?.DynamicInvoke();
            masterWatch.Stop();
            return masterWatch.ElapsedMilliseconds;
        }
    }
}
