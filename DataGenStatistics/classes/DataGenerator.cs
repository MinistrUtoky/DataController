using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Documents;
using System.Windows.Markup;
using System.IO;

namespace DataGenStatistics.classes
{
    internal class DataGenerator
    {
        //
        // Add ids to parent lists 
        //

        public static DataGenerator Instance = new DataGenerator();
        private Database database = new Database();
        private Database databaseDelta = new Database();

        public void Init()
        {
            InitDBSandbox();
            FetchAllTablesFromSQL();
            GenerateAdditionalData(0,0,0,0,0,0,0);
            PutDeltaIntoDB();
        }
        private void InitDBSandbox()
        {
            List<string> tableNames = DBClass.GetTableNames();
            database.libraryData = new List<LibraryData>();
            database.archiveData = new List<ArchiveData>();
            database.serverData =  new List<ServerData>();
            database.sessionData = new List<SessionData>();
            database.lobbyData = new List<LobbyData>();
            database.userData = new List<UserData>();
            database.playerData = new List<PlayerData>();
            databaseDelta.libraryData = new List<LibraryData>();
            databaseDelta.archiveData = new List<ArchiveData>();
            databaseDelta.serverData = new List<ServerData>();
            databaseDelta.sessionData = new List<SessionData>();
            databaseDelta.lobbyData = new List<LobbyData>();
            databaseDelta.userData = new List<UserData>();
            databaseDelta.playerData = new List<PlayerData>();
        }
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
        private void FetchAllTablesFromSQL()
        {
            foreach (string tableName in DBClass.GetTableNames())
            {
                Trace.WriteLine(tableName);
                FetchDataFromTable(tableName);
            }
        }
        private void FetchDataFromTable(string tableName)
        {
            if (tableName == Database.defaultLibrariesName)
            {
                FetchDataOfType(tableName, database.libraryData);
            }
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
        private static void FetchDataOfType<T>(string tableName, List<T> whereDataIsGoing)
        {
            DataTable table = DBClass.GetDataTableByName(tableName);
            foreach (DataRow row in table.Rows)
            {
                List<string> rowData = new List<string>();
                foreach (object? item in row.ItemArray)
                {
                    string s;
                    Trace.WriteLine(item?.ToString());
                    if (item.GetType() == typeof(Byte[]))
                        rowData.Add(BitConverter.ToBoolean((Byte[])item).ToString());
                    else
                        rowData.Add(item?.ToString());
                }
                Data d = (Data)(T)Activator.CreateInstance(typeof(T));
                whereDataIsGoing.Add((T)d.ToData(rowData));
            }
        }
        private void GenerateAdditionalData(int libs=0, int users = 0, int players = 0, int archives=0, int servers=0, 
                                            int sessions=0, int lobbies = 0)
        {
            List<string> tableNames = DBClass.GetTableNames();
            for (int i = 0; i < libs; i++)
                databaseDelta.libraryData.Add(GenerateLibrary());
            for (int i = 0; i < users; i++)
                databaseDelta.userData.Add(GenerateUser(RandomMaster<LibraryData>(database.libraryData, databaseDelta.libraryData).id));
            for (int i = 0; i < players; i++)
                databaseDelta.playerData.Add(GeneratePlayer(RandomMaster<UserData>(database.userData, databaseDelta.userData).id));
            for (int i = 0; i < archives; i++)
                databaseDelta.archiveData.Add(GenerateArchive(RandomMaster<LibraryData>(database.libraryData, databaseDelta.libraryData).id));
            for (int i = 0; i < servers; i++)
                databaseDelta.serverData.Add(GenerateServer(RandomMaster<ArchiveData>(database.archiveData, databaseDelta.archiveData).id));
            for (int i = 0; i < sessions; i++)
                databaseDelta.sessionData.Add(GenerateSession(RandomMaster<ServerData>(database.serverData, databaseDelta.serverData).id));
            for (int i = 0; i < lobbies; i++)
                databaseDelta.lobbyData.Add(GenerateLobby(RandomMaster<SessionData>(database.sessionData, databaseDelta.sessionData).id));
        }
        public void PutDeltaIntoDB()
        {
            using (StreamWriter outputFile = new StreamWriter(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + @"\data\log.txt"))
            {
                foreach (LibraryData d in databaseDelta.libraryData)
                {
                    DBClass.DBInsert(Database.defaultLibrariesName, d.ToList());
                }
                foreach (UserData d in databaseDelta.userData)
                {
                    DBClass.DBInsert(Database.defaultUsersName, d.ToList());
                }
                foreach (PlayerData d in databaseDelta.playerData)
                {
                    DBClass.DBInsert(Database.defaultPlayersName, d.ToList());
                }
                foreach (ArchiveData d in databaseDelta.archiveData)
                {
                    DBClass.DBInsert(Database.defaultArchivesName, d.ToList());
                }
                foreach (ServerData d in databaseDelta.serverData)
                {
                    DBClass.DBInsert(Database.defaultServersName, d.ToList());
                }
                foreach (SessionData d in databaseDelta.sessionData)
                {
                    DBClass.DBInsert(Database.defaultSessionsName, d.ToList());
                }
                foreach (LobbyData d in databaseDelta.lobbyData)
                {
                    DBClass.DBInsert(Database.defaultLobbiesName, d.ToList());
                }
            }
        }
        #region Generatable Items
        private const int maxNumberOfLibraryUsagesOnStart = 3;
        private static readonly string[] causes = new string[] { "Suspicious activity", "Check up" };
        private static readonly string[] results = new string[] { "Abnormalities found", "Nothing out of the ordinary" };
        private static readonly string[] usernames = new string[] { "Binko", "Bonko" };
        private static readonly string[] OSs = new string[] { "Windows", "MacOS", "Linux" };
        private static readonly string[] RAMs = new string[] { "8 GB", "16 GB" };
        private static readonly string[] GPUs = new string[] { "GeForce RTX 4060", "AMD Radeon RX 6800 XT" };
        private static readonly string[] CPUs = new string[] { "Intel Core i5-11400F OEM", "AMD Ryzen 7 5800X" };
        private static readonly string[] additionalInfos = new string[] { "", "Unidentified plugins" };
        private static readonly string[] statuses = new string[] { "banned", "suspended", "active" };
        private static readonly string[] userLocations = new string[] { "Serbia", "Spain" };
        private static readonly string[] realNames = new string[] { "Lublislav", "Pedro" };
        private static readonly string[] customURLs = new string[] { "awawawawo", "un_pedrero" };
        private const int maxNumberOfAchievmentsOnStart = 3;
        private static readonly string[] achievments = new string[] { "Dragonbuster", "Pull Up Maniac" };
        private static readonly string[] playerNames = new string[] { "Sprinkler", "Wafflemaker" };
        private const int maxNumberOfItemsOnStart = 3;
        private const int maxQuantityOfItemOnStart = 10;
        private static readonly string[] items = new string[] { "Stinky rag", "Slinky bat" };
        private const int maxNumberOfPerksOnStart = 3;
        private static readonly string[] perks = new string[] { "Power Power", "Quirkiness" };
        private const int maxNumberOfSkillsOnStart = 2;
        private const int maxLevelOfSkillOnStart = 999;
        private static readonly string[] skills = new string[] { "Game of Tag Proficiency", "Broom Swiping" };
        private const int minExperienceForSkillLevel = 10000;
        private static readonly string[] regions = new string[] { "EU", "NA", "EA" };
        private const int serverCapacityDenominator = 10;
        private const int minServerCap = 2;
        private const int maxServerCap = 200;
        private const int maxLogVolume = 20;
        private static readonly string[] possibleLogQuieries = new string[] { "MONSTER KILL", "RAMPAGE", "ULTRA KILL" };
        private static readonly string[] gameMaps = new string[] { "Castle", "Desert", "Meadows", "Forest" };
        private static readonly string[] gameModes = new string[] { "Tug of War", "Deathmatch", "3v3" };
        private static int maxPlayersInLobby = 4;
        #endregion

        #region Library
        public List<LibraryData> GenerateLibraries(int n)
        {
            List<LibraryData> libraries = new List<LibraryData>();
            for (int i = 0; i < n; i++)
            {
                LibraryData library = GenerateLibrary();
                library.id = library.id + i;
                libraries.Add(library);
            }
            return libraries;
        }
        public LibraryData GenerateLibrary() {
            LibraryData newLibrary = new LibraryData();
            int newId;
            if (databaseDelta.libraryData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultLibrariesName);
            else
                newId = databaseDelta.libraryData.Max(l => l.id) + 1;
            newLibrary.id = newId;
            newLibrary.creationDate = GenerateCreationDate();
            newLibrary.archivesInfo = new ArchivesInfo
            {
                archiveIDs = new List<int>()
            };
            newLibrary.usersInfo = new UsersInfo
            {
                userIDs = new List<int>()
            };
            newLibrary.libraryUsages = GenerateRandomNumberOfLibraryUsages(maxNumberOfLibraryUsagesOnStart);
            return newLibrary;
        }
        private static DateTime GenerateCreationDate() => GenerateDatetime();
        private static LibraryUsages GenerateRandomNumberOfLibraryUsages(int upTo)
        {
            Random r = new Random();
            return GenerateLibraryUsagesInfo(r.Next(0, upTo + 1));
        }
        private static LibraryUsages GenerateLibraryUsagesInfo(int n)
        {
            LibraryUsages libraryUsages = new LibraryUsages();
            libraryUsages.libraryUsages = new List<LibraryUsage>();
            for (int i = 0; i < n; i++) libraryUsages.libraryUsages.Add(GenerateLibraryUsage());
            return libraryUsages;
        }
        private static LibraryUsage GenerateLibraryUsage()
        {
            LibraryUsage libraryUsage = new LibraryUsage();
            libraryUsage.cause = GenerateCause();
            libraryUsage.result = GenerateResult();
            return libraryUsage;
        }
        private static string GenerateCause()
        {
            Random r = new Random();
            return causes[r.Next(0, causes.Length)];
        }
        private static string GenerateResult()
        {
            Random r = new Random();
            return results[r.Next(0, causes.Length)];
        }
        #endregion

        #region User
        public List<UserData> GenerateUsers(int amount)
        {
            List<UserData> users = new List<UserData>();
            for (int i = 0; i < amount; i++) users.Add(GenerateUser(RandomMaster<LibraryData>(database.libraryData, databaseDelta.libraryData).id));
            return users;
        }
        public List<UserData> GenerateUsers(int amount, int libraryID)
        {
            List<UserData> users = new List<UserData>();
            for (int i = 0; i < amount; i++) users.Add(GenerateUser(libraryID));
            return users;
        }
        public UserData GenerateUser(int libraryID)
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
            newUser.name = GenerateUsername();
            newUser.userIP = GenerateUserIP();
            newUser.technicalSpecifications = GenerateTechnicalSpecifications();
            newUser.userInfo = GenerateUserInfo();
            newUser.userInfo.registrationDateTime = GenerateDatetimeAfter(database.libraryData.Find(library => library.id == libraryID).creationDate);
            newUser.userStatus = GenerateUserStatus();
            newUser.libraryID = libraryID;
            master.usersInfo.userIDs.Add(newUser.id);
            return newUser;
        }
        private static string GenerateUsername()
        {
            Random r = new Random();
            StringBuilder username = new StringBuilder();
            username.Append(usernames[r.Next(0, usernames.Length)]);
            r = new Random(DBClass.GetNextId(Database.defaultUsersName));
            username.Append(r.Next(0, 1000000));
            return username.ToString();
        }
        private static TechnicalSpecifications GenerateTechnicalSpecifications()
        {
            TechnicalSpecifications technicalSpecifications = new TechnicalSpecifications();
            Random r = new Random();
            technicalSpecifications.OS = OSs[r.Next(0, OSs.Length)];
            technicalSpecifications.RAM = RAMs[r.Next(0, RAMs.Length)];
            technicalSpecifications.GPU = GPUs[r.Next(0, GPUs.Length)];
            technicalSpecifications.CPU = CPUs[r.Next(0, CPUs.Length)];
            technicalSpecifications.additionalInfo = additionalInfos[r.Next(0, additionalInfos.Length)];
            return technicalSpecifications;
        }
        private static string GenerateUserIP()
        {
            Random r = new Random();
            var data = new byte[4];
            r.NextBytes(data);
            IPAddress ip = new IPAddress(data);
            return ip.ToString();
        }
        private static UserInfo GenerateUserInfo()
        {
            UserInfo newUserInfo = new UserInfo();
            Random r = new Random();
            newUserInfo.location = userLocations[r.Next(0, userLocations.Length)];
            newUserInfo.realName = usernames[r.Next(0, realNames.Length)];
            newUserInfo.customURL = customURLs[r.Next(0, customURLs.Length)];
            newUserInfo.achievments = new List<string>();
            for (int i = 0; i < maxNumberOfAchievmentsOnStart; i++)
                newUserInfo.achievments.Add(achievments[r.Next(0, achievments.Length)]);
            return newUserInfo;
        }
        private static string GenerateUserStatus() 
        {
            Random r = new Random();
            return statuses[r.Next(0, statuses.Length)];
        }
        #endregion

        #region Player
        public List<PlayerData> GeneratePlayers(int amount)
        {
            List<PlayerData> players = new List<PlayerData>();
            for (int i = 0; i < amount; i++) players.Add(GeneratePlayer(RandomMaster<UserData>(database.userData, databaseDelta.userData).id));
            return players;
        }
        public List<PlayerData> GeneratePlayers(int amount, int userID)
        {
            List<PlayerData> players = new List<PlayerData>();
            for (int i = 0; i < amount; i++) players.Add(GeneratePlayer(userID));
            return players;
        }
        public PlayerData GeneratePlayer(int userID)
        {
            PlayerData newPlayer = new PlayerData();
            int newId;
            if (databaseDelta.playerData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultPlayersName);
            else
                newId = databaseDelta.playerData.Max(l => l.id) + 1;
            newPlayer.id = newId; 
            newPlayer.nickname = GeneratePlayerNickname();
            newPlayer.userID = userID;
            newPlayer.playerInventory = GenerateInventory();
            newPlayer.playerStats = GenerateStats();
            newPlayer.playerStatus = GenerateStatus();
            return newPlayer;
        }
        private static string GeneratePlayerNickname()
        {
            Random r = new Random();
            StringBuilder name = new StringBuilder();
            name.Append(playerNames[r.Next(0, playerNames.Length)]);
            r = new Random(DBClass.GetNextId(Database.defaultPlayersName));
            name.Append(r.Next(0, 1000000));
            return name.ToString();
        }
        private static List<Item> GenerateInventory()
        {
            Random r = new Random();
            List<Item> inv = new List<Item>();
            List<string> itemsNotYetInInv = items.ToList();
            for (int i = 0; i < r.Next(0, Math.Min(items.Length, maxNumberOfItemsOnStart)+1) & itemsNotYetInInv.Count > 0; i++)
            {
                Item item = new Item();
                int index = r.Next(0, itemsNotYetInInv.Count);
                item.name = itemsNotYetInInv[index];
                itemsNotYetInInv.RemoveAt(index);
                item.amount = r.Next(0, maxQuantityOfItemOnStart+1);
                inv.Add(item);
            }
            return inv;
        }
        private static PlayerStats GenerateStats()
        {
            PlayerStats stats = new PlayerStats();
            stats.perks = new List<string>();
            stats.skills = new List<Skill>();
            Random r = new Random();
            List<string> unusedPerks = perks.ToList();
            List<string> unusedSkills = skills.ToList();
            for (int i = 0; i < r.Next(0, Math.Min(perks.Length,maxNumberOfPerksOnStart) + 1); i++)
            {
                int index = r.Next(0, unusedPerks.Count);
                stats.perks.Add(unusedPerks[index]);
                unusedPerks.RemoveAt(index);
            }
            int totalLevel = 0;
            for (int i = 0; i < r.Next(0, Math.Min(skills.Length, maxNumberOfSkillsOnStart) + 1); i++)
            {
                int index = r.Next(0, unusedSkills.Count);
                Skill skill = new Skill();
                skill.name = unusedSkills[index];
                unusedSkills.RemoveAt(index);
                skill.level = r.Next(0, maxLevelOfSkillOnStart+1);
                totalLevel += skill.level;
                stats.skills.Add(skill); 
            }
            stats.totalExperience = r.Next(totalLevel * minExperienceForSkillLevel, maxLevelOfSkillOnStart * skills.Count() * minExperienceForSkillLevel + 1);
            return stats;
        }
        private static string GenerateStatus()
        {
            Random r = new Random();
            return statuses[r.Next(0, statuses.Length)];
        }
        #endregion

        #region Archive
        public List<ArchiveData> GenerateArchives(int amount)
        {
            List<ArchiveData> archives = new List<ArchiveData>();
            for (int i = 0; i < amount; i++) archives.Add(GenerateArchive(RandomMaster<LibraryData>(database.libraryData, databaseDelta.libraryData).id));
            return archives;
        }
        public List<ArchiveData> GenerateArchives(int amount, int libraryID)
        {
            List<ArchiveData> archives = new List<ArchiveData>();
            for (int i = 0; i < amount; i++) archives.Add(GenerateArchive(libraryID));
            return archives;
        }
        public ArchiveData GenerateArchive(int libraryID)
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
            newArchive.initializationDateTime = GenerateDatetimeAfter(master.creationDate);
            newArchive.suspensionDateTime = GenerateDatetimeAfter(newArchive.initializationDateTime);
            newArchive.serverIDs = new List<int>();
            newArchive.volume = newArchive.serverIDs.Count;
            newArchive.region = GenerateRegion();
            master.archivesInfo.archiveIDs.Add(newArchive.id);
            return newArchive;
        }
        private static string GenerateRegion()
        {
            Random r = new Random();
            return regions[r.Next(0, regions.Length)];
        }
        #endregion

        #region Server
        public List<ServerData> GenerateServers(int amount)
        {
            List<ServerData> servers = new List<ServerData>();
            for (int i = 0; i < amount; i++) servers.Add(GenerateServer(RandomMaster<ArchiveData>(database.archiveData, databaseDelta.archiveData).id));
            return servers;
        }
        public List<ServerData> GenerateServers(int amount, int archiveID)
        {
            List<ServerData> servers = new List<ServerData>();
            for (int i = 0; i < amount; i++) servers.Add(GenerateServer(archiveID));
            return servers;
        }
        public ServerData GenerateServer(int archiveID)
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
            newServer.location = GenerateLocation();
            newServer.sessionIDs = new List<int>();
            newServer.serverAvailability = GenerateServerAvailability();
            newServer.serverCapacity = GenerateServerCapacity();
            masterHolder[masterIndex].serverIDs.Add(newServer.id);
            return newServer;
        }
        private static string GenerateLocation()
        {
            Random r = new Random();
            var data = new byte[4];
            r.NextBytes(data);
            IPAddress ip = new IPAddress(data);
            return ip.ToString();
        }
        private static bool GenerateServerAvailability()
        {
            Random r = new Random();
            return r.Next(0,2)==0;
        }
        private static int GenerateServerCapacity()
        {
            Random r = new Random();
            return r.Next(minServerCap, maxServerCap+1)*serverCapacityDenominator;
        }
        #endregion

        #region Session
        public List<SessionData> GenerateSessions(int amount)
        {
            List<SessionData> sessions = new List<SessionData>();
            for (int i = 0; i < amount; i++) sessions.Add(GenerateSession(RandomMaster<ServerData>(database.serverData, databaseDelta.serverData).id));
            return sessions;
        }
        public List<SessionData> GenerateSessions(int amount, int serverID)
        {
            List<SessionData> sessions = new List<SessionData>();
            for (int i = 0; i < amount; i++) sessions.Add(GenerateSession(serverID));
            return sessions;
        }
        public SessionData GenerateSession(int serverID)
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
            newSession.startDateTime = GenerateDatetime(mastersMaster.initializationDateTime, mastersMaster.suspensionDateTime);
            newSession.endDateTime = GenerateDatetime(newSession.startDateTime, mastersMaster.suspensionDateTime);
            newSession.sessionInfo = GenerateSessionInfo();
            master.sessionIDs.Add(newSession.id);
            return newSession;
        }
        private static SessionInfo GenerateSessionInfo()
        {
            SessionInfo newSession = new SessionInfo();
            newSession.gameLog = new List<string>();
            Random random = new Random();
            for (int i = 0; i < random.Next(0,maxLogVolume+1); i++) newSession.gameLog?.Add(GenerateGameLogQuery());
            newSession.gameMap = GenerateGameMap();
            newSession.gameMode = GenerateGameMode();
            newSession.participatingLobbies = new List<int>();
            return newSession;
        }
        private static string GenerateGameLogQuery()
        {
            Random r = new Random();
            return possibleLogQuieries[r.Next(0, possibleLogQuieries.Length)];
        }
        private static string GenerateGameMap()
        {
            Random r = new Random();
            return gameMaps[r.Next(0, gameMaps.Length)];
        }
        private static string GenerateGameMode()
        {
            Random r = new Random();
            return gameModes[r.Next(0, gameModes.Length)];
        }
        #endregion

        #region Lobby
        public List<LobbyData> GenerateLobbies(int amount)
        {
            List<LobbyData> lobbies = new List<LobbyData>();
            for (int i = 0; i < amount; i++) lobbies.Add(GenerateLobby(RandomMaster<SessionData>(database.sessionData, databaseDelta.sessionData).id));
            return lobbies;
        }
        public List<LobbyData> GenerateLobbies(int amount, int sessionID)
        {
            List<LobbyData> lobbies = new List<LobbyData>();
            for (int i = 0; i < amount; i++) lobbies.Add(GenerateLobby(sessionID));
            return lobbies;
        }
        public LobbyData GenerateLobby(int sessionID)
        {
            LobbyData newLobby = new LobbyData();
            List<SessionData> tb = new List<SessionData>();
            tb.AddRange(database.sessionData); tb.AddRange(databaseDelta.sessionData);
            SessionData master = tb.First(session => session.id == sessionID);
            int newId;
            if (databaseDelta.lobbyData.Count == 0)
                newId = DBClass.GetNextId(Database.defaultLobbiesName);
            else
                newId = databaseDelta.lobbyData.Max(l => l.id) + 1;
            newLobby.id = newId; 
            newLobby.sessionID = sessionID;
            newLobby.playerIDs = RandomPlayerIds();
            newLobby.numberOfParticipants = newLobby.playerIDs.Count;
            newLobby.creationDate = GenerateDatetime(master.startDateTime, master.endDateTime);
            master.sessionInfo.participatingLobbies.Add(newLobby.id);
            return newLobby;
        }
        public List<int> RandomPlayerIds() {
            List<int> someIds = new List<int>();
            List<PlayerData> players = database.playerData.Concat(databaseDelta.playerData).ToList();
            Random r = new Random();
            for (int i = 0; i < Math.Min(maxPlayersInLobby, r.Next(0, players.Count)); i++)
                someIds.Add(players[r.Next(0, players.Count)].id);
            return someIds;
        }
        #endregion

        #region General Generators
        private static DateTime GenerateDatetime() => GenerateDatetime(DateTime.UnixEpoch, DateTime.Now);
        private static DateTime GenerateDatetime(DateTime startDate, DateTime endDate)
        {
            if ((startDate - DateTime.UnixEpoch).TotalSeconds < 0) startDate = DateTime.UnixEpoch;
            var random = new Random();
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, 0, random.Next(0, (int)(timeSpan.TotalSeconds + 1)));
            return startDate + newSpan;
        }
        private static DateTime GenerateDatetimeAfter(DateTime dt) => GenerateDatetime(dt, DateTime.Now);

        public T RandomMaster<T>(List<T> possibeMastersInDB, List<T> possibleMastersInDBDelta)
        {
            List<T> list = possibeMastersInDB.Concat(possibleMastersInDBDelta).ToList();
            Random r = new Random();
            return list[r.Next(0, list.Count)];
        }
        #endregion

    }
}
