using Database_Interface.classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Intrinsics.X86;

namespace RandomDataGenerator
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
            List<string> tableNames = DBClass.GetTableNames();
            database.libraryData = new Tuple<string, List<LibraryData>>(tableNames[0], new List<LibraryData>());
            database.archiveData = new Tuple<string, List<ArchiveData>>(tableNames[1], new List<ArchiveData>());
            database.serverData = new Tuple<string, List<ServerData>>(tableNames[2], new List<ServerData>());
            database.sessionData = new Tuple<string, List<SessionData>>(tableNames[3], new List<SessionData>());
            database.lobbyData = new Tuple<string, List<LobbyData>>(tableNames[4], new List<LobbyData>());
            database.userData = new Tuple<string, List<UserData>>(tableNames[5], new List<UserData>());
            database.playerData = new Tuple<string, List<PlayerData>>(tableNames[6], new List<PlayerData>());
            FetchAllTablesFromSQL();
        }
        private static void SeedDatabaseToMSQLServer()
        {
            DBClass.CreateNewTable(Database.defaultLibrariesName, new List<string> { "ID", "CREATION_DATE", "ARCHIVES_INFO", "ALL_TIME_USERS_INFO", "LIBRARY_USAGES_INFO" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "DATETIME NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "NVARCHAR(MAX) NULL"},
                                              new List<string> { "", "", "", "", "" });

            DBClass.CreateNewTable(Database.defaultUsersName, new List<string> { "ID", "NAME", "TECHNICAL_SPECIFICATIONS", "USER_IP", "USER_INFO", "USER_STATUS", "LIBRARY_ID" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "VARCHAR(45) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(45) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(45) NULL",
                                                                 "INT NOT NULL"},
                                              new List<string> { "", "", "", "", "", "", Database.defaultLibrariesName + "(ID)" });
            DBClass.CreateNewTable(Database.defaultPlayersName, new List<string> { "ID", "NICKNAME", "PLAYER_STATS", "PLAYER_INVENTORY", "PLAYER_STATUS", "USER_ID" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "VARCHAR(45) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(45) NULL",
                                                                 "INT NOT NULL"},
                                              new List<string> { "", "", "", "", "", "[" + Database.defaultUsersName + "](ID)" });

            DBClass.CreateNewTable(Database.defaultArchivesName, new List<string> { "ID", "LIBRARY_ID", "INITIALIZATION_DATE",
                                                                 "SUSPENSION_DATE", "VOLUME", "ARCHIVED_INFO", "REGION"},
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "DATETIME NULL",
                                                                 "DATETIME NULL",
                                                                 "INT NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "VARCHAR(10)"},
                                              new List<string> { "", Database.defaultLibrariesName + "(ID)", "", "", "", "", "" });

            DBClass.CreateNewTable(Database.defaultServersName, new List<string> { "ID", "ARCHIVE_ID", "LOCATION",
                                                                 "PAST_SESSIONS_INFO", "SERVER_AVAILABILITY", "SERVER_CAPACITY"},
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "VARCHAR(500) NULL",
                                                                 "NVARCHAR(MAX) NULL",
                                                                 "BINARY(1) NULL",
                                                                 "INT NULL"},
                                              new List<string> { "", Database.defaultArchivesName + "(ID)", "", "", "", "" });
            DBClass.CreateNewTable(Database.defaultSessionsName, new List<string> { "ID", "DEDICATED_SERVER_ID", "START_DATETIME",
                                                                 "END_DATETIME", "SESSION_INFO" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "DATETIME NULL",
                                                                 "DATETIME NULL",
                                                                 "NVARCHAR(MAX) NULL"},
                                              new List<string> { "", Database.defaultServersName + "(ID)", "", "", "" });
            DBClass.CreateNewTable(Database.defaultLobbiesName, new List<string> { "ID", "SESSION_ID", "NUMBER_OF_PARTICIPANTS",
                                                                 "CREATION_DATE", "PARTICIPANTS_INFO" },
                                              new List<string> { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                                                 "INT NOT NULL",
                                                                 "INT NULL",
                                                                 "DATETIME NULL",
                                                                 "NVARCHAR(MAX) NULL"},
                                              new List<string> { "", Database.defaultSessionsName + "(ID)", "", "", "" });
        }
        public void FetchAllTablesFromSQL()
        {
            foreach (string tableName in DBClass.GetTableNames())
            {
                Console.WriteLine(tableName);
                DataTable table = DBClass.GetDataTableByName(tableName);
                foreach (DataRow row in table.Rows)
                {
                    foreach (object? item in row.ItemArray)
                    {
                        Console.Write(item?.ToString() + ", ");
                    }
                    Console.WriteLine();
                }
            }
        }
        public void GenerateAdditionadata(int libs, int archs, int servers, int sessions,
                                           int lobbies, int users, int players)
        {
            List<string> tableNames = DBClass.GetTableNames();
            databaseDelta.libraryData = new Tuple<string, List<LibraryData>>(tableNames[0], GenerateLibraries(libs));
            
            databaseDelta.archiveData = new Tuple<string, List<ArchiveData>>(tableNames[1], 
                                                                            GenerateArchives(archs, 
                                                                            ((ArchiveData)RandomMaster(database.libraryData.Item2, databaseDelta.libraryData.Item2)).id));
            databaseDelta.serverData = new Tuple<string, List<ServerData>>(tableNames[2], 
                                                                            GenerateServers(servers,
                                                                            ((ServerData)RandomMaster(database.archiveData.Item2, databaseDelta.archiveData.Item2)).id));
            databaseDelta.sessionData = new Tuple<string, List<SessionData>>(tableNames[3], 
                                                                            GenerateSessions(sessions,
                                                                            ((SessionData)RandomMaster(database.serverData.Item2, databaseDelta.serverData.Item2)).id));
            databaseDelta.lobbyData = new Tuple<string, List<LobbyData>>(tableNames[4], 
                                                                            GenerateLobbies(lobbies,
                                                                            ((LobbyData)RandomMaster(database.sessionData.Item2, databaseDelta.sessionData.Item2)).id));
            databaseDelta.userData = new Tuple<string, List<UserData>>(tableNames[5], 
                                                                            GenerateUsers(users,
                                                                            ((UserData)RandomMaster(database.libraryData.Item2, databaseDelta.libraryData.Item2)).id));
            databaseDelta.playerData = new Tuple<string, List<PlayerData>>(tableNames[6], 
                                                                            GeneratePlayers(players,
                                                                            ((PlayerData)RandomMaster(database.userData.Item2, databaseDelta.userData.Item2)).id));
        }
        public void PutDeltaIntoDB()
        {
            foreach (LibraryData d in database.libraryData.Item2)
            {

            }
            foreach (ArchiveData d in databaseDelta.archiveData.Item2)
            {

            }
            foreach (ServerData d in databaseDelta.serverData.Item2)
            {

            }
            foreach (SessionData d in databaseDelta.sessionData.Item2)
            {

            }
            foreach (LobbyData d in databaseDelta.lobbyData.Item2)
            {

            }
            foreach (UserData d in databaseDelta.userData.Item2)
            {

            }
            foreach (PlayerData d in databaseDelta.playerData.Item2)
            {

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
        private const int maxNumberOfSkillsOnStart = 3;
        private const int maxLevelOfSkillOnStart = 999;
        private static readonly string[] skills = new string[] { "Game of Tag Proficiency", "Broom Swiping" };
        private const int minExperienceForSkillLevel = 10000;
        private const int maxExperience = 10000000;
        private static readonly string[] regions = new string[] { "EU", "NA", "EA" };
        private const int serverCapacityDenominator = 10;
        private const int minServerCap = 2;
        private const int maxServerCap = 200;
        private const int maxLogVolume = 20;
        private static readonly string[] possibleLogQuieries = new string[] { "", "", "" };
        private static readonly string[] gameMaps = new string[] { "Castle", "Desert", "Meadows", "Forest" };
        private static readonly string[] gameModes = new string[] { "Tug of War", "Deathmatch", "3v3" };
        #endregion
        #region Data Structures
        public struct Database
        {
            public const string defaultLibrariesName = "library";
            public const string defaultArchivesName = "archive";
            public const string defaultServersName = "dedicated_server";
            public const string defaultSessionsName = "session";
            public const string defaultLobbiesName = "lobby";
            public const string defaultUsersName = "user";
            public const string defaultPlayersName = "player";

            public Tuple<string, List<LibraryData>> libraryData;
            public Tuple<string, List<UserData>> userData;
            public Tuple<string, List<PlayerData>> playerData;
            public Tuple<string, List<ArchiveData>> archiveData;
            public Tuple<string, List<ServerData>> serverData;
            public Tuple<string, List<SessionData>> sessionData;
            public Tuple<string, List<LobbyData>> lobbyData;
        }
        public struct PlayerData
        {
            public int id;
            public string nickname;
            public List<Item> playerInventory;
            public PlayerStats playerStats;
            public string playerStatus;
            public int userID;
            public List<string> ToList() { return new List<string>(); }
        }
        public struct Item
        {
            public string name;
            public int amount;
        }
        public struct PlayerStats
        {
            public List<string> perks;
            public List<Skill> skills;
            public long totalExperience;
        }
        public struct Skill
        {
            public string name;
            public int level;
        }
        public struct UserData
        {
            public int id;
            public string name;
            public TechnicalSpecifications technicalSpecifications;
            public string userIP;
            public UserInfo userInfo;
            public string userStatus;
            public int libraryID;
            public List<string> ToList() { return new List<string>(); }
        }
        public struct TechnicalSpecifications
        {
            public string OS;
            public string RAM;
            public string GPU;
            public string CPU;
            public string additionalInfo;
        }
        public struct UserInfo
        {
            public string location;
            public string realName;
            public string customURL;
            public List<string> achievments;
            public DateTime registrationDateTime;
        }
        public struct LobbyData
        {
            public int id;
            public int sessionID;
            public int numberOfParticipants;
            public DateTime creationDate;
            public List<int> playerIDs;
            public List<string> ToList() { return new List<string>(); }
        }
        public struct SessionData
        {
            public int id;
            public int serverID;
            public DateTime startDateTime;
            public DateTime endDateTime;
            public SessionInfo sessionInfo;
            public List<string> ToList() { return new List<string>(); }
        }
        public struct SessionInfo
        {
            public List<string> gameLog;
            public string gameMap;
            public string gameMode;
            public List<int> participatingLobbies;
        }
        public struct ServerData
        {
            public int id;
            public int archiveID;
            public string location;
            public List<int> sessionIDs;
            public bool serverAvailability;
            public int serverCapacity;
            public List<string> ToList() { return new List<string>(); }
        }
        public struct ArchiveData
        {
            public int id;
            public int libraryID;
            public DateTime initializationDateTime;
            public DateTime suspensionDateTime;
            public int volume;
            public List<int> serverIDs;
            public string region;
            public List<string> ToList() { return new List<string>(); }
        }
        public struct LibraryData
        {
            public int id;
            public DateTime creationDate { get; set; }
            public ArchivesInfo archivesInfo { get; set; }
            public UsersInfo usersInfo { get; set; }
            public LibraryUsages libraryUsages { get; set; }
            public List<string> ToList() 
            {
                return new List<string>() { id.ToString(), creationDate.ToString("yyyy-MM-dd HH:mm:ss"), JsonSerializer.Serialize(archivesInfo),
                                            JsonSerializer.Serialize(usersInfo), JsonSerializer.Serialize(libraryUsages)};
            }
        }
        public struct LibraryUsages
        {
            public List<LibraryUsage> libraryUsages { get; set; }
        }
        public struct ArchivesInfo
        {
            public List<int> archiveIDs { get; set; }
        }
        public struct UsersInfo 
        {
            public List<int> userIDs { get; set; }
        }
        public struct LibraryUsage
        {
            public string cause { get; set; }
            public string result { get; set; }
        }
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
            int newId = Math.Max(DBClass.GetNextId(database.libraryData.Item1), databaseDelta.libraryData.Item2.Max(l => l.id));
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
        public List<UserData> GenerateUsers(int amount, int libraryID)
        {
            List<UserData> users = new List<UserData>();
            for (int i = 0; i < amount; i++) users.Add(GenerateUser(libraryID));
            return users;
        }
        public UserData GenerateUser(int libraryID)
        {
            UserData newUser = new UserData();
            LibraryData master = database.libraryData.Item2.Find(library => library.id == libraryID);
            int newId = Math.Max(DBClass.GetNextId(database.userData.Item1), databaseDelta.userData.Item2.Max(l => l.id));
            newUser.id = newId;
            newUser.name = GenerateUsername();
            newUser.userIP = GenerateUserIP();
            newUser.technicalSpecifications = GenerateTechnicalSpecifications();
            newUser.userInfo = GenerateUserInfo();
            newUser.userInfo.registrationDateTime = GenerateDatetimeAfter(database.libraryData.Item2.Find(library => library.id == libraryID).creationDate);
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
            newUserInfo.realName = userLocations[r.Next(0, realNames.Length)];
            newUserInfo.customURL = userLocations[r.Next(0, customURLs.Length)];
            newUserInfo.achievments = new List<string>();
            for (int i = 0; i < maxNumberOfAchievmentsOnStart; i++)
                newUserInfo.achievments.Add(userLocations[r.Next(0, achievments.Length)]);
            return newUserInfo;
        }
        private static string GenerateUserStatus() 
        {
            Random r = new Random();
            return statuses[r.Next(0, statuses.Length)];
        }
        #endregion
        #region Player
        public List<PlayerData> GeneratePlayers(int amount, int userID)
        {
            List<PlayerData> players = new List<PlayerData>();
            for (int i = 0; i < amount; i++) players.Add(GeneratePlayer(userID));
            return players;
        }
        public PlayerData GeneratePlayer(int userID)
        {
            PlayerData newPlayer = new PlayerData();
            int newId = Math.Max(DBClass.GetNextId(database.playerData.Item1), databaseDelta.playerData.Item2.Max(l => l.id));
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
            for (int i = 0; i < r.Next(0, maxNumberOfItemsOnStart+1) & itemsNotYetInInv.Count > 0; i++)
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
            for (int i = 0; i < r.Next(0, maxNumberOfPerksOnStart + 1); i++)
            {
                int index = r.Next(0, unusedPerks.Count);
                stats.perks.Add(unusedPerks[index]);
                unusedPerks.RemoveAt(index);
            }
            int totalLevel = 0;
            for (int i = 0; i < r.Next(0, maxNumberOfSkillsOnStart + 1); i++)
            {
                int index = r.Next(0, unusedSkills.Count);
                Skill skill = new Skill();
                skill.name = unusedSkills[index];
                unusedSkills.RemoveAt(index);
                skill.level = r.Next(0, maxLevelOfSkillOnStart+1);
                totalLevel += skill.level;
                stats.skills.Add(skill); 
            }
            stats.totalExperience = r.Next(totalLevel * minExperienceForSkillLevel, maxExperience);
            return stats;
        }
        private static string GenerateStatus()
        {
            Random r = new Random();
            return statuses[r.Next(0, statuses.Length)];
        }
        #endregion
        #region Archive
        public List<ArchiveData> GenerateArchives(int amount, int libraryID)
        {
            List<ArchiveData> archives = new List<ArchiveData>();
            for (int i = 0; i < amount; i++) archives.Add(GenerateArchive(libraryID));
            return archives;
        }
        public ArchiveData GenerateArchive(int libraryID)
        {
            ArchiveData newArchive = new ArchiveData();
            LibraryData master = database.libraryData.Item2.Find(library => library.id == libraryID);
            int newId = Math.Max(DBClass.GetNextId(database.archiveData.Item1), databaseDelta.archiveData.Item2.Max(l => l.id));
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
        public List<ServerData> GenerateServers(int amount, int archiveID)
        {
            List<ServerData> servers = new List<ServerData>();
            for (int i = 0; i < amount; i++) servers.Add(GenerateServer(archiveID));
            return servers;
        }
        public ServerData GenerateServer(int archiveID)
        {
            ServerData newServer = new ServerData();
            ArchiveData master = database.archiveData.Item2.Find(archive => archive.id == archiveID);
            int newId = Math.Max(DBClass.GetNextId(database.serverData.Item1), databaseDelta.serverData.Item2.Max(l => l.id));
            newServer.id = newId;
            newServer.archiveID = archiveID;
            newServer.location = GenerateLocation();
            newServer.sessionIDs = new List<int>();
            newServer.serverAvailability = GenerateServerAvailability();
            newServer.serverCapacity = GenerateServerCapacity();
            master.serverIDs.Add(newServer.id);
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
        public List<SessionData> GenerateSessions(int amount, int serverID)
        {
            List<SessionData> sessions = new List<SessionData>();
            for (int i = 0; i < amount; i++) sessions.Add(GenerateSession(serverID));
            return sessions;
        }
        public SessionData GenerateSession(int serverID)
        {
            SessionData newSession = new SessionData();
            ServerData master = database.serverData.Item2.Find(server => server.id == serverID);
            ArchiveData mastersMaster = database.archiveData.Item2.Find(archive => master.archiveID == archive.id);
            int newId = Math.Max(DBClass.GetNextId(database.sessionData.Item1), databaseDelta.sessionData.Item2.Max(l => l.id));
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
            for (int i = 0; i < maxLogVolume; i++) newSession.gameLog?.Add(GenerateGameLogQuery());
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
        public List<LobbyData> GenerateLobbies(int amount, int sessionID)
        {
            List<LobbyData> lobbies = new List<LobbyData>();
            for (int i = 0; i < amount; i++) lobbies.Add(GenerateLobby(sessionID));
            return lobbies;
        }
        public LobbyData GenerateLobby(int sessionID)
        {
            LobbyData newLobby = new LobbyData();
            SessionData master = database.sessionData.Item2.Find(session => session.id == sessionID);
            int newId = Math.Max(DBClass.GetNextId(database.lobbyData.Item1), databaseDelta.lobbyData.Item2.Max(l => l.id));
            newLobby.id = newId; 
            newLobby.sessionID = sessionID;
            newLobby.playerIDs = new List<int>();
            newLobby.numberOfParticipants = newLobby.playerIDs.Count;
            newLobby.creationDate = GenerateDatetime(master.startDateTime, master.endDateTime);
            master.sessionInfo.participatingLobbies.Add(newLobby.id);
            return newLobby;
        }
        #endregion
       
        private static DateTime GenerateDatetime() => GenerateDatetime(DateTime.UnixEpoch, DateTime.Now);
        private static DateTime GenerateDatetime(DateTime startDate, DateTime endDate)
        {
            var random = new Random();
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, 0, random.Next(0, (int)(timeSpan.TotalSeconds + 1)));
            return startDate + newSpan;
        }
        private static DateTime GenerateDatetimeAfter(DateTime dt) => GenerateDatetime(dt, DateTime.Now);

        public object RandomMaster<T>(List<T> possibeMastersInDB, List<T> possibleMastersInDBDelta)
        {
            List<T> list = possibeMastersInDB.Concat(possibleMastersInDBDelta).ToList();
            Random r = new Random();
            return list[r.Next(0, list.Count)];
        }
    }
}
