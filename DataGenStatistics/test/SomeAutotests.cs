using DataGenStatistics.classes;
using DataGenStatistics.investigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DataGenStatistics.test
{
    internal class SomeAutotests
    {
        /// <summary>
        /// Testing if some data is generating
        /// </summary>
        public static void TestGenerator()
        {
            DatabaseSandbox.Instance.GenerateAdditionalData(1, 2, 3, 4, 5, 6, 7);
            if (DatabaseSandbox.Instance.databaseDelta.libraryData.Count != 1 ||
                DatabaseSandbox.Instance.databaseDelta.userData.Count != 2 ||
                DatabaseSandbox.Instance.databaseDelta.playerData.Count != 3 ||
                DatabaseSandbox.Instance.databaseDelta.archiveData.Count != 4 ||
                DatabaseSandbox.Instance.databaseDelta.serverData.Count != 5 ||
                DatabaseSandbox.Instance.databaseDelta.sessionData.Count != 6 ||
                DatabaseSandbox.Instance.databaseDelta.lobbyData.Count != 7)
                throw new Exception("Something's wrong with the generator!");
            DatabaseSandbox.Instance.databaseDelta.Clear();
        }

        /// <summary>
        /// Testing process timers and db response by extracting database's table names
        /// </summary>
        public static void TestDBResponse()
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
            foreach (long l in ProcessTimers.SeveralProcessesTimeInMilliseconds(delegates))
            {
                Trace.WriteLine(l);
            }
        }

        /// <summary>
        /// Testing process timers by iterating
        /// </summary>
        public static void TimerTest()
        {
            Func<Task> millionIterations = async () =>
            {
                for (int i = 0; i < 1000000; i++) ;
            };
            long ms = ProcessTimers.SeveralProcessesTimeInMilliseconds(new List<Func<Task>> { millionIterations })[0];
            if (ms > 10)
                throw new Exception("Your device is too slow");
            else
                Trace.WriteLine("You're welcome chad");
        }
    }
}
