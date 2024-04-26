using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGenStatistics.classes
{
    /// <summary>
    /// The database sandbox 
    /// </summary>
    internal class DatabaseSandbox
    {
        public static DatabaseSandbox Instance = new DatabaseSandbox();
        internal Database database = new Database();
        internal Database databaseDelta = new Database();
        /// <summary>
        /// Initiating the sandbox
        /// </summary>
        public void Init()
        {
            //DropAll();
            //SeedDatabaseToMSQLServer();
            FetchAllTablesFromSQL();
            //GenerateAdditionalData(0,0,0,0,0,0,0);
            //PutDeltaIntoDB();
        }
        /// <summary>
        /// Creating tables in database matching the sandbox
        /// </summary>
        private static void SeedDatabaseToMSQLServer()
        {
            DBClass.CreateNewTable(Database.defaultLibrariesName,
                                    new List<string> { "ID", "CREATION_DATE", "ARCHIVES_INFO", "ALL_TIME_USERS_INFO", "LIBRARY_USAGES_INFO" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "DATETIME NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "NVARCHAR(MAX) NULL"},
                                              new List<string> { "", "", "", "", "" });

            DBClass.CreateNewTable(Database.defaultUsersName,
                                    new List<string> { "ID", "NAME", "TECHNICAL_SPECIFICATIONS", "USER_IP", "USER_INFO", "USER_STATUS", "LIBRARY_ID" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "VARCHAR(45) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(45) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(45) NULL",
                                                                 "INT NOT NULL"},
                                              new List<string> { "", "", "", "", "", "", Database.defaultLibrariesName + "(ID)" });
            DBClass.CreateNewTable(Database.defaultPlayersName,
                                    new List<string> { "ID", "NICKNAME", "PLAYER_STATS", "PLAYER_INVENTORY", "PLAYER_STATUS", "USER_ID" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "VARCHAR(45) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(45) NULL",
                                                                 "INT NOT NULL"},
                                              new List<string> { "", "", "", "", "", "[" + Database.defaultUsersName + "](ID)" });

            DBClass.CreateNewTable(Database.defaultArchivesName,
                                    new List<string> { "ID", "LIBRARY_ID", "INITIALIZATION_DATE",
                                                                 "SUSPENSION_DATE", "VOLUME", "ARCHIVED_INFO", "REGION"},
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "DATETIME NULL",
                                                                 "DATETIME NULL",
                                                                 "INT NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(10)"},
                                              new List<string> { "", Database.defaultLibrariesName + "(ID)", "", "", "", "", "" });

            DBClass.CreateNewTable(Database.defaultServersName,
                                    new List<string> { "ID", "ARCHIVE_ID", "LOCATION",
                                                                 "PAST_SESSIONS_INFO", "SERVER_AVAILABILITY", "SERVER_CAPACITY"},
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "VARCHAR(500) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "BINARY(1) NULL",
                                                                 "INT NULL"},
                                              new List<string> { "", Database.defaultArchivesName + "(ID)", "", "", "", "" });
            DBClass.CreateNewTable(Database.defaultSessionsName,
                                    new List<string> { "ID", "DEDICATED_SERVER_ID", "START_DATETIME",
                                                                 "END_DATETIME", "SESSION_INFO" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "DATETIME NULL",
                                                                 "DATETIME NULL",
                                                                 "NVARCHAR(MAX) NULL"},
                                              new List<string> { "", Database.defaultServersName + "(ID)", "", "", "" });
            DBClass.CreateNewTable(Database.defaultLobbiesName,
                                    new List<string> { "ID", "SESSION_ID", "NUMBER_OF_PARTICIPANTS",
                                                                 "CREATION_DATE", "PARTICIPANTS_INFO" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "INT NULL",
                                                                 "DATETIME NULL",
                                                                 "NVARCHAR(MAX) NULL"},
                                              new List<string> { "", Database.defaultSessionsName + "(ID)", "", "", "" });
        }
        /// <summary>
        /// Deleting all known tables from database
        /// </summary>
        private static void DropAll()
        {
            DBClass.DropTable("player");
            DBClass.DropTable("user");
            DBClass.DropTable("lobby");
            DBClass.DropTable("session");
            DBClass.DropTable("dedicated_server");
            DBClass.DropTable("archive");
            DBClass.DropTable("library");
        }
        /// <summary>
        /// Initiation of a database to data sandbox data fetch for all tables
        /// </summary>
        private void FetchAllTablesFromSQL()
        {
            foreach (string tableName in DBClass.GetTableNames())
            {
                //Trace.WriteLine(tableName);
                FetchDataFromTable(tableName);
            }
        }
        /// <summary>
        /// Initiation of a database to data sandbox data fetch for a specified table
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to fetch data from
        /// </param>
        private void FetchDataFromTable(string tableName)
        {
            if (tableName == Database.defaultLibrariesName)
                FetchDataOfType(tableName, database.libraryData);
            else if (tableName == Database.defaultUsersName)
                FetchDataOfType<UserData>(tableName, database.userData);
            else if (tableName == Database.defaultPlayersName)
                FetchDataOfType<PlayerData>(tableName, database.playerData);
            else if (tableName == Database.defaultArchivesName)
                FetchDataOfType<ArchiveData>(tableName, database.archiveData);
            else if (tableName == Database.defaultServersName)
                FetchDataOfType<ServerData>(tableName, database.serverData);
            else if (tableName == Database.defaultSessionsName)
                FetchDataOfType<SessionData>(tableName, database.sessionData);
            else if (tableName == Database.defaultLobbiesName)
                FetchDataOfType<LobbyData>(tableName, database.lobbyData);
            else throw new Exception("Non-existent table");
        }
        /// <summary>
        /// Initiation of a database to data sandbox data fetch from a specified table to a specified sandbox type
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to fetch data from
        /// </param>
        /// <param name="whereDataIsGoing">
        /// Specified sandbox list (preferrably one within the existing sandbox)
        /// </param>
        private static void FetchDataOfType<T>(string tableName, List<T> whereDataIsGoing)
        {
            DataTable table = DBClass.GetDataTableByName(tableName);
            foreach (DataRow row in table.Rows)
            {
                List<string> rowData = new List<string>();
                foreach (object? item in row.ItemArray)
                {
                    //Trace.WriteLine(item?.ToString());
                    if (item.GetType() == typeof(Byte[]))
                        rowData.Add(BitConverter.ToBoolean((Byte[])item).ToString());
                    else
                        rowData.Add(item?.ToString());
                }
                Data d = (Data)(T)Activator.CreateInstance(typeof(T));
                whereDataIsGoing.Add((T)d.ToData(rowData));
            }
        }
        /// <summary>
        /// Puts all newly created and added to databaseDelta values into the real database
        /// </summary>
        private void PutDeltaIntoDB()
        {
            database.Add(databaseDelta);
            DBClass.DBInsertMultiple(Database.defaultLibrariesName, databaseDelta.libraryData.Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultUsersName, databaseDelta.userData.Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultPlayersName, databaseDelta.playerData.Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultArchivesName, databaseDelta.archiveData.Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultServersName, databaseDelta.serverData.Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultSessionsName, databaseDelta.sessionData.Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultLobbiesName, databaseDelta.lobbyData.Cast<Data>().ToList());
            databaseDelta.Clear();

        }
        /// <summary>
        /// Generates and puts specified numbers of new data tuples into the sandbox
        /// </summary>
        /// <param name="libs">
        /// Number of libraries to generate
        /// </param>
        /// <param name="users">
        /// Number of users to generate
        /// </param>
        /// <param name="players">
        /// Number of players to generate
        /// </param>
        /// <param name="archives">
        /// Number of archives to generate
        /// </param>
        /// <param name="servers">
        /// Number of servers to generate
        /// </param>
        /// <param name="sessions">
        /// Number of sessions to generate
        /// </param>
        /// <param name="lobbies">
        /// Number of lobbies to generate
        /// </param>
        public void GenerateAdditionalData(int libs = 0, int users = 0, int players = 0, int archives = 0, int servers = 0,
                                            int sessions = 0, int lobbies = 0)
        {
            databaseDelta.libraryData.AddRange(GenerateLibraries(libs));
            databaseDelta.userData.AddRange(GenerateUsers(users));
            databaseDelta.playerData.AddRange(GeneratePlayers(players));
            databaseDelta.archiveData.AddRange(GenerateArchives(archives));
            databaseDelta.serverData.AddRange(GenerateServers(servers));
            databaseDelta.sessionData.AddRange(GenerateSessions(sessions));
            databaseDelta.lobbyData.AddRange(GenerateLobbies(lobbies));
        }
        /// <summary>
        /// Puts specified numbers of sandbox data tuples into the real database
        /// </summary>
        /// <param name="libs">
        /// Number of libraries to put into the database
        /// </param>
        /// <param name="users">
        /// Number of users to put into the database
        /// </param>
        /// <param name="players">
        /// Number of players to put into the database
        /// </param>
        /// <param name="archives">
        /// Number of archives to put into the database
        /// </param>
        /// <param name="servers">
        /// Number of servers to put into the database
        /// </param>
        /// <param name="sessions">
        /// Number of sessions to put into the database
        /// </param>
        /// <param name="lobbies">
        /// Number of lobbies to put into the database
        /// </param>
        public void PutDeltaPartIntoDB(int libs = 0, int users = 0, int players = 0, int archives = 0, int servers = 0,
                                            int sessions = 0, int lobbies = 0)
        {
            if (libs > databaseDelta.libraryData.Count || users > databaseDelta.userData.Count ||
                players > databaseDelta.playerData.Count || archives > databaseDelta.archiveData.Count ||
                servers > databaseDelta.serverData.Count || sessions > databaseDelta.sessionData.Count ||
                lobbies > databaseDelta.lobbyData.Count)
                throw new Exception("Length of database delta's part exceeds that of the delta itself");
            database.libraryData.AddRange(databaseDelta.libraryData.GetRange(0, libs));
            database.userData.AddRange(databaseDelta.userData.GetRange(0, users));
            database.playerData.AddRange(databaseDelta.playerData.GetRange(0, players));
            database.archiveData.AddRange(databaseDelta.archiveData.GetRange(0, archives));
            database.serverData.AddRange(databaseDelta.serverData.GetRange(0, servers));
            database.sessionData.AddRange(databaseDelta.sessionData.GetRange(0, sessions));
            database.lobbyData.AddRange(databaseDelta.lobbyData.GetRange(0, lobbies));
            DBClass.DBInsertMultiple(Database.defaultLibrariesName, databaseDelta.libraryData.GetRange(0, libs).Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultUsersName, databaseDelta.userData.GetRange(0, users).Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultPlayersName, databaseDelta.playerData.GetRange(0, players).Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultArchivesName, databaseDelta.archiveData.GetRange(0, archives).Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultServersName, databaseDelta.serverData.GetRange(0, servers).Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultSessionsName, databaseDelta.sessionData.GetRange(0, sessions).Cast<Data>().ToList());
            DBClass.DBInsertMultiple(Database.defaultLobbiesName, databaseDelta.lobbyData.GetRange(0, lobbies).Cast<Data>().ToList());
            databaseDelta.libraryData.RemoveRange(0, libs);
            databaseDelta.userData.RemoveRange(0, users);
            databaseDelta.playerData.RemoveRange(0, players);
            databaseDelta.archiveData.RemoveRange(0, archives);
            databaseDelta.serverData.RemoveRange(0, servers);
            databaseDelta.sessionData.RemoveRange(0, sessions);
            databaseDelta.lobbyData.RemoveRange(0, lobbies);
        }

        #region Library
        /// <summary>
        /// Selection of a specified number of random library ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of library ids
        /// </returns>
        public List<int> GetRandomLibraryIds(int n) => DataGenerator.GetRandomIds(database.libraryData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox library tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox library tuples to generate
        /// </param>
        /// <returns>
        /// List of library tuple structures
        /// </returns>
        public List<LibraryData> GenerateLibraries(int amount)
        {
            List<LibraryData> libraries = new List<LibraryData>();
            for (int i = 0; i < amount; i++)
            {
                LibraryData library = GenerateLibrary();
                library.id = library.id + i;
                libraries.Add(library);
            }
            return libraries;
        }
        /// <summary>
        /// Generation of a new sandbox library tuple structure
        /// </summary>
        /// <returns>
        /// New sandbox library tuple structure
        /// </returns>
        private LibraryData GenerateLibrary()
        {
            LibraryData newLibrary = new LibraryData();
            int newId;
            if (databaseDelta.libraryData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultLibrariesName);
            else
                newId = databaseDelta.libraryData.Max(l => l.id) + 1;
            newLibrary.id = newId;
            newLibrary.creationDate = DataGenerator.GenerateCreationDate();
            newLibrary.archivesInfo = new ArchivesInfo
            {
                archiveIDs = new List<int>()
            };
            newLibrary.usersInfo = new UsersInfo
            {
                userIDs = new List<int>()
            };
            newLibrary.libraryUsages = DataGenerator.GenerateRandomNumberOfLibraryUsages(DataGenerator.maxNumberOfLibraryUsagesOnStart);
            return newLibrary;
        }
        #endregion

        #region User
        /// <summary>
        /// Selection of a specified number of random user ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of user ids
        /// </returns>
        public List<int> GetRandomUserIds(int n) => DataGenerator.GetRandomIds(database.userData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox user tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox user tuples to generate
        /// </param>
        /// <returns>
        /// List of user tuple structures
        /// </returns>
        public List<UserData> GenerateUsers(int amount)
        {
            List<UserData> users = new List<UserData>();
            for (int i = 0; i < amount; i++) users.Add(GenerateUser(DataGenerator.RandomMaster<LibraryData>(database.libraryData, databaseDelta.libraryData).id));
            return users;
        }
        /// <summary>
        /// Generation of a new sandbox user tuple structure
        /// </summary>
        /// <param name="libraryID">
        /// This user's foreign key master library's id in database's hierarchy  
        /// </param>
        /// <returns>
        /// New sandbox user tuple structure
        /// </returns>
        private UserData GenerateUser(int libraryID)
        {
            UserData newUser = new UserData();
            List<LibraryData> tb = new List<LibraryData>();
            tb.AddRange(database.libraryData); tb.AddRange(databaseDelta.libraryData);
            LibraryData master = tb.Find(library => library.id == libraryID);
            int newId;
            if (databaseDelta.userData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultUsersName);
            else
                newId = databaseDelta.userData.Max(l => l.id) + 1;
            newUser.id = newId;
            newUser.name = DataGenerator.GenerateUsername();
            newUser.userIP = DataGenerator.GenerateUserIP();
            newUser.technicalSpecifications = DataGenerator.GenerateTechnicalSpecifications();
            newUser.userInfo = DataGenerator.GenerateUserInfo();
            newUser.userInfo.registrationDateTime = DataGenerator.GenerateDatetimeAfter(database.libraryData.Find(library => library.id == libraryID).creationDate);
            newUser.userStatus = DataGenerator.GenerateUserStatus();
            newUser.libraryID = libraryID;
            master.usersInfo.userIDs.Add(newUser.id);
            return newUser;
        }

        #endregion

        #region Player
        /// <summary>
        /// Selection of a specified number of random player ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of player ids
        /// </returns>
        public List<int> GetRandomPlayerIds(int n) => DataGenerator.GetRandomIds(database.playerData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox player tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox player tuples to generate
        /// </param>
        /// <returns>
        /// List of player tuple structures
        /// </returns>
        public List<PlayerData> GeneratePlayers(int amount)
        {
            List<PlayerData> players = new List<PlayerData>();
            for (int i = 0; i < amount; i++) players.Add(GeneratePlayer(DataGenerator.RandomMaster<UserData>(database.userData, databaseDelta.userData).id));
            return players;
        }
        /// <summary>
        /// Generation of a new sandbox player tuple structure
        /// </summary>
        /// <param name="userID">
        /// This player's foreign key master user's id in database's hierarchy  
        /// </param>
        /// <returns>
        /// New sandbox player tuple structure
        /// </returns>
        private PlayerData GeneratePlayer(int userID)
        {
            PlayerData newPlayer = new PlayerData();
            int newId;
            if (databaseDelta.playerData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultPlayersName);
            else
                newId = databaseDelta.playerData.Max(l => l.id) + 1;
            newPlayer.id = newId;
            newPlayer.nickname = DataGenerator.GeneratePlayerNickname();
            newPlayer.userID = userID;
            newPlayer.playerInventory = DataGenerator.GenerateInventory();
            newPlayer.playerStats = DataGenerator.GenerateStats();
            newPlayer.playerStatus = DataGenerator.GenerateStatus();
            return newPlayer;
        }

        #endregion

        #region Archive
        /// <summary>
        /// Selection of a specified number of random archive ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of archive ids
        /// </returns>
        public List<int> GetRandomArchiveIds(int n) => DataGenerator.GetRandomIds(database.archiveData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox archive tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox archive tuples to generate
        /// </param>
        /// <returns>
        /// List of archive tuple structures
        /// </returns>
        public List<ArchiveData> GenerateArchives(int amount)
        {
            List<ArchiveData> archives = new List<ArchiveData>();
            for (int i = 0; i < amount; i++) archives.Add(GenerateArchive(DataGenerator.RandomMaster<LibraryData>(database.libraryData, databaseDelta.libraryData).id));
            return archives;
        }
        /// <summary>
        /// Generation of a new sandbox archive tuple structure
        /// </summary>
        /// <param name="libraryID">
        /// This archive's foreign key master library's id in database's hierarchy 
        /// </param>
        /// <returns>
        /// New sandbox archive tuple structure
        /// </returns>
        private ArchiveData GenerateArchive(int libraryID)
        {
            ArchiveData newArchive = new ArchiveData();
            List<LibraryData> tb = new List<LibraryData>();
            tb.AddRange(database.libraryData); tb.AddRange(databaseDelta.libraryData);
            LibraryData master = tb.Find(library => library.id == libraryID);
            int newId;
            if (databaseDelta.archiveData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultArchivesName);
            else
                newId = databaseDelta.archiveData.Max(l => l.id) + 1;
            newArchive.id = newId;
            newArchive.libraryID = libraryID;
            newArchive.initializationDateTime = DataGenerator.GenerateDatetimeAfter(master.creationDate);
            newArchive.suspensionDateTime = DataGenerator.GenerateDatetimeAfter(newArchive.initializationDateTime);
            newArchive.serverIDs = new List<int>();
            newArchive.volume = newArchive.serverIDs.Count;
            newArchive.region = DataGenerator.GenerateRegion();
            master.archivesInfo.archiveIDs.Add(newArchive.id);
            return newArchive;
        }

        #endregion

        #region Server
        /// <summary>
        /// Selection of a specified number of random server ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of server ids
        /// </returns>
        public List<int> GetRandomServerIds(int n) => DataGenerator.GetRandomIds(database.serverData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox server tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox server tuples to generate
        /// </param>
        /// <returns>
        /// List of server tuple structures
        /// </returns>
        public List<ServerData> GenerateServers(int amount)
        {
            List<ServerData> servers = new List<ServerData>();
            for (int i = 0; i < amount; i++) servers.Add(GenerateServer(DataGenerator.RandomMaster<ArchiveData>(database.archiveData, databaseDelta.archiveData).id));
            return servers;
        }
        /// <summary>
        /// Generation of a new sandbox server tuple structure
        /// </summary>
        /// <param name="archiveID">
        /// This server's foreign key master archive's id in database's hierarchy 
        /// </param>
        /// <returns>
        /// New sandbox server tuple structure
        /// </returns>
        private ServerData GenerateServer(int archiveID)
        {
            ServerData newServer = new ServerData();
            int masterIndex; List<ArchiveData> masterHolder;
            if (database.archiveData.Any(archive => archive.id == archiveID))
            {
                masterHolder = database.archiveData;
                masterIndex = database.archiveData.FindIndex(archive => archive.id == archiveID);
            }
            else
            {
                masterHolder = databaseDelta.archiveData;
                masterIndex = databaseDelta.archiveData.FindIndex(archive => archive.id == archiveID);
            }
            int newId;
            if (databaseDelta.serverData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultServersName);
            else
                newId = databaseDelta.serverData.Max(l => l.id) + 1;
            newServer.id = newId;
            newServer.archiveID = archiveID;
            newServer.location = DataGenerator.GenerateLocation();
            newServer.sessionIDs = new List<int>();
            newServer.serverAvailability = DataGenerator.GenerateServerAvailability();
            newServer.serverCapacity = DataGenerator.GenerateServerCapacity();
            masterHolder[masterIndex].serverIDs.Add(newServer.id);
            return newServer;
        }

        #endregion

        #region Session
        /// <summary>
        /// Selection of a specified number of random session ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of session ids
        /// </returns>
        public List<int> GetRandomSessionIds(int n) => DataGenerator.GetRandomIds(database.sessionData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox session tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox session tuples to generate
        /// </param>
        /// <returns>
        /// List of session tuple structures
        /// </returns>
        public List<SessionData> GenerateSessions(int amount)
        {
            List<SessionData> sessions = new List<SessionData>();
            for (int i = 0; i < amount; i++) sessions.Add(GenerateSession(DataGenerator.RandomMaster<ServerData>(database.serverData, databaseDelta.serverData).id));
            return sessions;
        }
        /// <summary>
        /// Generation of a new sandbox session tuple structure
        /// </summary>
        /// <param name="serverID">
        /// This session's foreign key master server's id in database's hierarchy 
        /// </param>
        /// <returns>
        /// New sandbox session tuple structure
        /// </returns>
        private SessionData GenerateSession(int serverID)
        {
            SessionData newSession = new SessionData();
            List<ServerData> tb = new List<ServerData>();
            tb.AddRange(database.serverData); tb.AddRange(databaseDelta.serverData);
            ServerData master = tb.Find(server => server.id == serverID);
            List<ArchiveData> tb2 = new List<ArchiveData>();
            tb2.AddRange(database.archiveData); tb2.AddRange(databaseDelta.archiveData);
            ArchiveData mastersMaster = tb2.Find(archive => master.archiveID == archive.id);
            int newId;
            if (databaseDelta.sessionData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultSessionsName);
            else
                newId = databaseDelta.sessionData.Max(l => l.id) + 1;
            newSession.id = newId;
            newSession.serverID = serverID;
            newSession.startDateTime = DataGenerator.GenerateDatetime(mastersMaster.initializationDateTime, mastersMaster.suspensionDateTime);
            newSession.endDateTime = DataGenerator.GenerateDatetime(newSession.startDateTime, mastersMaster.suspensionDateTime);
            newSession.sessionInfo = DataGenerator.GenerateSessionInfo();
            master.sessionIDs.Add(newSession.id);
            return newSession;
        }

        #endregion

        #region Lobby
        /// <summary>
        /// Selection of a specified number of random lobby ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of lobby ids
        /// </returns>
        public List<int> GetRandomLobbyIds(int n) => DataGenerator.GetRandomIds(database.lobbyData.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox lobby tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox lobby tuples to generate
        /// </param>
        /// <returns>
        /// List of lobby tuple structures
        /// </returns>
        public List<LobbyData> GenerateLobbies(int amount)
        {
            List<LobbyData> lobbies = new List<LobbyData>();
            for (int i = 0; i < amount; i++) lobbies.Add(GenerateLobby(DataGenerator.RandomMaster<SessionData>(database.sessionData, databaseDelta.sessionData).id));
            return lobbies;
        }
        /// <summary>
        /// Generation of a new sandbox lobby tuple structure
        /// </summary>
        /// <param name="sessionID">
        /// This lobby's foreign key master session's id in database's hierarchy 
        /// </param>
        /// <returns>
        /// New sandbox lobby tuple structure
        /// </returns>
        private LobbyData GenerateLobby(int sessionID)
        {
            LobbyData newLobby = new LobbyData();
            List<SessionData> tb = new List<SessionData>();
            tb.AddRange(database.sessionData); tb.AddRange(databaseDelta.sessionData);
            SessionData master = tb.Find(session => session.id == sessionID);
            int newId;
            if (databaseDelta.lobbyData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultLobbiesName);
            else
                newId = databaseDelta.lobbyData.Max(l => l.id) + 1;
            newLobby.id = newId;
            newLobby.sessionID = sessionID;
            newLobby.playerIDs = RandomPlayerIds();
            newLobby.numberOfParticipants = newLobby.playerIDs.Count;
            newLobby.creationDate = DataGenerator.GenerateDatetime(master.startDateTime, master.endDateTime);
            master.sessionInfo.participatingLobbies.Add(newLobby.id);
            return newLobby;
        }
        /// <summary>
        /// Selection of a random number of random player id's to include into lobby's player list
        /// </summary>
        /// <returns>
        /// List of player ids
        /// </returns>
        private List<int> RandomPlayerIds() 
        { 
            List<PlayerData> allPlayers = database.playerData.Concat(databaseDelta.playerData).ToList();
            return DataGenerator.GetRandomIds(
                            allPlayers.Cast<Data>().ToList(),
                                 Math.Min(DataGenerator.maxPlayersInLobby, DataGenerator.rand.Next(0, allPlayers.Count)));
        }
        #endregion
    }
}
