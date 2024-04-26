using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DataGenStatistics.classes;

namespace DataGenStatistics.investigation
{
    /// <summary>
    /// Timers for processes and related utilities
    /// </summary>
    internal class ProcessTimers
    {
        private static Stopwatch masterWatch = Stopwatch.StartNew();
        /// <summary>
        /// Testing process timers by extracting database's table names
        /// </summary>
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
        /// <summary>
        /// Calculating time for several processes
        /// </summary>
        /// <param name="processes">
        /// Processes to calculate time for
        /// </param>
        /// <returns>
        /// List of milliseconds elapsed for several processes
        /// </returns>
        public static List<long> SeveralProcessesTimeInMilliseconds(List<Func<Task>> processes)
        {
            List<long> times = new();
            foreach (Func<Task> d in processes)
                times.Add(ProcessTimeInMilliseconds(d));
            return times;
        }

        /// <summary>
        /// Calculating time for a process
        /// </summary>
        /// <param name="process">
        /// Process to calculate time for
        /// </param>
        /// <returns>
        /// Time in milliseconds spent for a process to complete
        /// </returns>
        public static long ProcessTimeInMilliseconds(Func<Task> process)
        {
            masterWatch.Restart();
            process.Invoke();
            masterWatch.Stop();
            return masterWatch.ElapsedMilliseconds;
        }
    }
}
