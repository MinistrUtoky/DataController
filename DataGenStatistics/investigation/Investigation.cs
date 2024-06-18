using DataGenStatistics.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

///<summary>
/// Main file for investigations where information about timings for different processes is calculated and packaged for plotting
/// </summary>
namespace DataGenStatistics.investigation
{
    public static class Investigation
    {
        public const int maxSteps = 10,
                  numberOfRowsPerStep = 1;
        /// <summary>
        /// Calculates generation time result data for globally given numbers of rows
        /// Gets plotted with Plot()
        /// </summary>
        /// <param name="numberOfRows">
        /// Number of rows to generate  
        /// </param>
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
                {DatabaseSandbox.Instance.database.libraryData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {DatabaseSandbox.Instance.database.userData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {DatabaseSandbox.Instance.database.playerData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {DatabaseSandbox.Instance.database.archiveData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {DatabaseSandbox.Instance.database.serverData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {DatabaseSandbox.Instance.database.sessionData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {DatabaseSandbox.Instance.database.lobbyData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates selection time result data for globally given numbers of rows
        /// Gets plotted with Plot()
        /// </summary>
        /// <param name="numberOfRows">
        /// Number of rows to select  
        /// </param>
        /// <param name="condition">
        /// Selection condition (Example: WHERE a=b)
        /// </param>
        public static Dictionary<string, List<long>> PlotSelectQueries(int[] numberOfRows, string condition)
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
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [library] " + condition);
                });
                userDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [users] " + condition);
                });
                playerDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [player] " + condition);
                });
                archiveDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [archive] " + condition);
                });
                serverDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [dedicated_server] " + condition);
                });
                sessionDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [session] " + condition);
                });
                lobbyDelegates.Add(async () => {
                    DBClass.GetDataTable("SELECT TOP " + n + " * FROM [lobby] " + condition);
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {DatabaseSandbox.Instance.database.libraryData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {DatabaseSandbox.Instance.database.userData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {DatabaseSandbox.Instance.database.playerData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {DatabaseSandbox.Instance.database.archiveData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {DatabaseSandbox.Instance.database.serverData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {DatabaseSandbox.Instance.database.sessionData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {DatabaseSandbox.Instance.database.lobbyData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates insertion time result data for globally given numbers of rows;
        /// Gets plotted with Plot()
        /// </summary>
        /// <param name="numberOfRows">
        /// Number of rows to insert  
        /// </param>
        /// <param name="selectStatement">
        /// *tableName* gets inserted automatically at the end of quiery if selectStatement=true
        /// </param>
        public static Dictionary<string, List<long>> PlotInsertQueries(int[] numberOfRows, bool selectStatement=false, string whereStatement="")
        {
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            int totalDataAmount = numberOfRows.Sum();
            DBClass.UncheckAllConstraintsInDB();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.libraryData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.libraryData.Name, columnNames.GetRange(1, columnNames.Count-1), "SELECT TOP " + n + " " + columnNamesExceptIdString + " FROM  library " + whereStatement);   
                });
                userDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.userData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.userData.Name, columnNames.GetRange(1, columnNames.Count-1), "SELECT TOP " + n + " " + columnNamesExceptIdString + " FROM  users " + whereStatement);
                });
                playerDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.playerData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.playerData.Name, columnNames.GetRange(1, columnNames.Count - 1), "SELECT TOP " + n + " " + columnNamesExceptIdString + " FROM  player " + whereStatement);                  
                });
                archiveDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.archiveData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.archiveData.Name, columnNames.GetRange(1, columnNames.Count-1), "SELECT TOP " + n + " " + columnNamesExceptIdString + " FROM  archive " + whereStatement);
                });
                serverDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.serverData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.serverData.Name, columnNames.GetRange(1, columnNames.Count-1), "SELECT TOP " + n + columnNamesExceptIdString + " FROM dedicated_server " + whereStatement);
                });
                sessionDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.sessionData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.sessionData.Name, columnNames.GetRange(1, columnNames.Count-1), "SELECT TOP " + n + " " + columnNamesExceptIdString + " FROM  session " + whereStatement);
                });
                lobbyDelegates.Add(async () => {
                    List<string> columnNames = DBClass.GetColumnNames(DatabaseSandbox.Instance.database.lobbyData.Name);
                    StringBuilder columnNamesExceptIdString = new StringBuilder();
                    for (int i = 1; i < columnNames.Count - 1; i++)
                    {
                        columnNamesExceptIdString.Append(columnNames[i]);
                        columnNamesExceptIdString.Append(",");
                    }
                    columnNamesExceptIdString.Append(columnNames[columnNames.Count - 1]);

                    DBClass.DBInsertSelect(DatabaseSandbox.Instance.database.lobbyData.Name, columnNames.GetRange(1, columnNames.Count-1), "SELECT TOP " + n + " " + columnNamesExceptIdString + " FROM  lobby " + whereStatement);
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {DatabaseSandbox.Instance.database.libraryData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {DatabaseSandbox.Instance.database.userData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {DatabaseSandbox.Instance.database.playerData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {DatabaseSandbox.Instance.database.archiveData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {DatabaseSandbox.Instance.database.serverData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {DatabaseSandbox.Instance.database.sessionData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {DatabaseSandbox.Instance.database.lobbyData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            DBClass.CheckAllConstraintsInDB();
            return results;
        }

        /// <summary>
        /// Calculates insertion time result data for globally given numbers of rows;
        /// Gets plotted with Plot()
        /// </summary>
        /// <param name="numberOfRows">
        /// Number of rows to insert  
        /// </param>
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
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.libraryData.Name, DatabaseSandbox.Instance.databaseDelta.libraryData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.libraryData.DeleteTop(n);
                });
                userDelegates.Add(async () => {
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.userData.Name, DatabaseSandbox.Instance.databaseDelta.userData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.userData.DeleteTop(n);
                });
                playerDelegates.Add(async () => {
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.playerData.Name, DatabaseSandbox.Instance.databaseDelta.playerData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.playerData.DeleteTop(n);
                });
                archiveDelegates.Add(async () => {
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.archiveData.Name, DatabaseSandbox.Instance.databaseDelta.archiveData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.archiveData.DeleteTop(n);
                });
                serverDelegates.Add(async () => {
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.serverData.Name, DatabaseSandbox.Instance.databaseDelta.serverData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.serverData.DeleteTop(n);
                });
                sessionDelegates.Add(async () => {
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.sessionData.Name, DatabaseSandbox.Instance.databaseDelta.sessionData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.sessionData.DeleteTop(n);
                });
                lobbyDelegates.Add(async () => {
                    DBClass.TryDBInsertMultiple(DatabaseSandbox.Instance.database.lobbyData.Name, DatabaseSandbox.Instance.databaseDelta.lobbyData.SelectAll().Cast<Data>().ToList().GetRange(0, n));
                    DatabaseSandbox.Instance.databaseDelta.lobbyData.DeleteTop(n);
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {DatabaseSandbox.Instance.database.libraryData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {DatabaseSandbox.Instance.database.userData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {DatabaseSandbox.Instance.database.playerData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {DatabaseSandbox.Instance.database.archiveData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {DatabaseSandbox.Instance.database.serverData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {DatabaseSandbox.Instance.database.sessionData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {DatabaseSandbox.Instance.database.lobbyData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            return results;
        }

        /// <summary>
        /// Calculates removal time result data for globally given numbers of rows
        /// Gets plotted with Plot()
        /// </summary>
        /// <param name="numberOfRows">
        /// Number of rows to remove  
        /// </param>
        /// <param name="condition">
        /// Removal condition (Example: WHERE a=b)
        /// </param>
        public static Dictionary<string, List<long>> PlotRemoveQueries(int[] numberOfRows, string condition)
        {
            List<Func<Task>> libraryDelegates = new(),
                            userDelegates = new(),
                            playerDelegates = new(),
                            archiveDelegates = new(),
                            serverDelegates = new(),
                            sessionDelegates = new(),
                            lobbyDelegates = new();
            List<int> ids = new();
            DBClass.UncheckAllConstraintsInDB();
            foreach (int n in numberOfRows)
            {
                libraryDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomLibraryIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.libraryData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); });
                });

                userDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomUserIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.userData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); });
                });

                playerDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomPlayerIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.playerData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); }); 
                });

                archiveDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomArchiveIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.archiveData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); });
                });

                serverDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomServerIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.serverData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); }); 
                });

                sessionDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomSessionIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.sessionData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); }); 
                });

                lobbyDelegates.Add(async () => {
                    DatabaseSandbox.Instance.GetRandomLobbyIds(n).ForEach(
                        id => { DBClass.DBRemove(DatabaseSandbox.Instance.database.lobbyData.Name, condition == "" ? "ID=" + id : condition + " AND ID=" + id); }); 
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {DatabaseSandbox.Instance.database.libraryData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {DatabaseSandbox.Instance.database.userData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {DatabaseSandbox.Instance.database.playerData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {DatabaseSandbox.Instance.database.archiveData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {DatabaseSandbox.Instance.database.serverData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {DatabaseSandbox.Instance.database.sessionData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {DatabaseSandbox.Instance.database.lobbyData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            DBClass.CheckAllConstraintsInDB();
            return results;
        }

        /// <summary>
        /// Calculates update time result data for globally given numbers of rows
        /// Gets plotted with Plot()
        /// </summary>
        /// <param name="numberOfRows">
        /// Number of rows to update  
        /// </param>
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
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.libraryData.Name, DatabaseSandbox.Instance.GenerateLibraries(n).Cast<Data>().ToList(),
                                            DatabaseSandbox.Instance.GetRandomLibraryIds(n).ToList());
                });
                userDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.userData.Name, DatabaseSandbox.Instance.GenerateUsers(n).Cast<Data>().ToList(),
                                            DatabaseSandbox.Instance.GetRandomUserIds(n).ToList());
                });
                playerDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.playerData.Name, DatabaseSandbox.Instance.GeneratePlayers(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomPlayerIds(n).ToList());
                });
                archiveDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.archiveData.Name, DatabaseSandbox.Instance.GenerateArchives(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomArchiveIds(n).ToList());
                });
                serverDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.serverData.Name, DatabaseSandbox.Instance.GenerateServers(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomServerIds(n).ToList());
                });
                sessionDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.sessionData.Name, DatabaseSandbox.Instance.GenerateSessions(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomSessionIds(n).ToList());
                });
                lobbyDelegates.Add(async () => {
                    DBClass.DBUpdateMultiple(DatabaseSandbox.Instance.database.lobbyData.Name, DatabaseSandbox.Instance.GenerateLobbies(n).Cast<Data>().ToList(),
                        DatabaseSandbox.Instance.GetRandomLobbyIds(n).ToList());
                });
            }
            Dictionary<string, List<long>> results = new Dictionary<string, List<long>>
            {
                {DatabaseSandbox.Instance.database.libraryData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(libraryDelegates) },
                {DatabaseSandbox.Instance.database.userData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(userDelegates) },
                {DatabaseSandbox.Instance.database.playerData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(playerDelegates) },
                {DatabaseSandbox.Instance.database.archiveData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(archiveDelegates) },
                {DatabaseSandbox.Instance.database.serverData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(serverDelegates) },
                {DatabaseSandbox.Instance.database.sessionData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(sessionDelegates) },
                {DatabaseSandbox.Instance.database.lobbyData.Name, ProcessTimers.SeveralProcessesTimeInMilliseconds(lobbyDelegates) }
            };
            //DatabaseSandbox.Instance.DumpDelta();
            return results;
        }
    }
}
