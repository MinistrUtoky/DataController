using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DataGenStatistics.classes
{
    internal class ProcessTimers
    {
        private static Stopwatch masterWatch = Stopwatch.StartNew();
        public static void TimerTest()
        {
            List<Func<Task>> delegates = new List<Func<Task>>()
            {
                async () => { DBClass.GetDataTableByName("library"); },
                async () => { DBClass.GetDataTableByName("user"); },
                async () => { DBClass.GetDataTableByName("player"); },
                async () => { DBClass.GetDataTableByName("archive"); },
                async () => { DBClass.GetDataTableByName("dedicated_server"); },
                async () => { DBClass.GetDataTableByName("session"); },
                async () => { DBClass.GetDataTableByName("lobby"); }
            };
            foreach (long l in SeveralProcessesTimeInMilliseconds(delegates))
            {
                Trace.WriteLine(l);
            }
        }
        public static List<long> SeveralProcessesTimeInMilliseconds(List<Func<Task>> processes)
        {
            List<long> times = new();
            foreach (Func<Task> d in processes)
                times.Add(ProcessTimeInMilliseconds(d));
            return times;
        }
        public static long ProcessTimeInMilliseconds(Func<Task> process)
        {
            masterWatch.Restart();
            process.Invoke();
            masterWatch.Stop();
            return masterWatch.ElapsedMilliseconds;
        }
    }
}
