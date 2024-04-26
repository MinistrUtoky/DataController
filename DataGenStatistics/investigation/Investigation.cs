using DataGenStatistics.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataGenStatistics.investigation
{
    public static class Investigation
    {
        public const int maxSteps = 10,
                  numberOfRowsPerStep = 2;
        /// <summary>
        /// Calculates generation time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        public static Dictionary<string, List<long>> PlotGenerators(int[] numberOfRows)
        {            
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => DatabaseSandbox.Instance.GenerateLibraries(n));
                userDelegates.Add(async () => DatabaseSandbox.Instance.GenerateUsers(n));
                playerDelegates.Add(async () => DatabaseSandbox.Instance.GeneratePlayers(n));
                archiveDelegates.Add(async () => DatabaseSandbox.Instance.GenerateArchives(n));
                serverDelegates.Add(async () => DatabaseSandbox.Instance.GenerateServers(n));
                sessionDelegates.Add(async () => DatabaseSandbox.Instance.GenerateSessions(n));
                lobbyDelegates.Add(async () => DatabaseSandbox.Instance.GenerateLobbies(n));
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates selection time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        public static Dictionary<string, List<long>> PlotSelectQueries(int[] numberOfRows)
        {
            
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [library]");
                });
                userDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [user]");
                });
                playerDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [player]");
                });
                archiveDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [archive]");
                });
                serverDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [dedicated_server]");
                });
                sessionDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [session]");
                });
                lobbyDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [lobby]");
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates insertion time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        public static Dictionary<string, List<long>> PlotInsertQueries(int[] numberOfRows)
        {
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            int totalDataAmount = numberOfRows.Sum();
            DatabaseSandbox.Instance.GenerateAdditionalData(totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount,
                                                          totalDataAmount, totalDataAmount, totalDataAmount);
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(n, 0, 0, 0, 0, 0, 0);
                });
                userDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, n, 0, 0, 0, 0, 0);
                });
                playerDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, n, 0, 0, 0, 0);
                });
                archiveDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, n, 0, 0, 0);
                });
                serverDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, 0, n, 0, 0);
                });
                sessionDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, 0, 0, n, 0);
                });
                lobbyDelegates.Add(async () => {
                    DatabaseSandbox.Instance.PutDeltaPartIntoDB(0, 0, 0, 0, 0, 0, n);
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates removal time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        public static Dictionary<string, List<long>> PlotRemoveQueries(int[] numberOfRows)
        {
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            List<int> ids = new();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomLibraryIds(n).ToList().ForEach(id => DBClass.DBRemove("library", id));
                });

                userDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomUserIds(n).ToList().ForEach(id => DBClass.DBRemove("user", id));
                });

                playerDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomPlayerIds(n).ToList().ForEach(id => DBClass.DBRemove("player", id));
                });

                archiveDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomArchiveIds(n).ToList().ForEach(id => DBClass.DBRemove("archive", id));
                });

                serverDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomServerIds(n).ToList().ForEach(id => DBClass.DBRemove("dedicated_server", id));
                });

                sessionDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomSessionIds(n).ToList().ForEach(id => DBClass.DBRemove("session", id));
                });

                lobbyDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomLobbyIds(n).ToList().ForEach(id => DBClass.DBRemove("lobby", id));
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates update time result data for globally given numbers of rows
        /// Plots with Plot()
        /// </summary>
        public static Dictionary<string, List<long>> PlotUpdateQueries(int[] numberOfRows)
        {
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            List<int> ids = new();
            List<Data> newValues = new();
            int totalDataAmount = numberOfRows.Sum();
            //DatabaseSandbox.Instance.GenerateAdditionalData(totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount, totalDataAmount);
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("library", DatabaseSandbox.Instance.GenerateLibraries(n).Cast<Data>().ToList(),
                                            DatabaseSandbox.Instance.GetRandomLibraryIds(n).ToList());
                });
                userDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("user", DatabaseSandbox.Instance.GenerateUsers(n).Cast<Data>().ToList(),
                                            DatabaseSandbox.Instance.GetRandomUserIds(n).ToList());
                });
                playerDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("player", DatabaseSandbox.Instance.GeneratePlayers(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomPlayerIds(n).ToList());
                });
                archiveDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("archive", DatabaseSandbox.Instance.GenerateArchives(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomArchiveIds(n).ToList());
                });
                serverDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("dedicated_server", DatabaseSandbox.Instance.GenerateServers(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomServerIds(n).ToList());
                });
                sessionDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("session", DatabaseSandbox.Instance.GenerateSessions(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomSessionIds(n).ToList());
                });
                lobbyDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple("lobby", DatabaseSandbox.Instance.GenerateLobbies(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomLobbyIds(n).ToList());
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {"library", ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {"user", ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {"player", ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {"archive", ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {"server", ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {"session", ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {"lobby", ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            //DatabaseSandbox.Instance.DumpDelta();
            return results;
        }
    }
}
