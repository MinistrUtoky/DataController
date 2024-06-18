using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

///<summary>
/// Here are general functions for counting the time required to complete a process or several processes
/// </summary>
namespace DataGenStatistics.investigation
{
    /// <summary>
    /// Timers for processes and related utilities
    /// </summary>
    internal class ProcessTimers
    {
        private static Stopwatch masterWatch = new Stopwatch();
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
        