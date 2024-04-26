using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataGenStatistics.classes
{
    /// <summary>
    /// Abstraction of a sandbox data tuple 
    /// </summary>
    public interface Data
    {
        public int Id { get; }
        /// <summary>
        /// Data to list conversion for later DBClass processing
        /// </summary>
        public List<string> ToList();
        /// <summary>
        /// List to data conversion for DBClass data extraction
        /// </summary>
        /// <param name="data">
        /// List of values to be cast into Data
        /// </param>
        public Data ToData(List<string> data);
    }
    /// <summary>
    /// Sandbox database
    /// </summary>
    public struct Database
    {
        public const string defaultLibrariesName = "library";
        public const string defaultArchivesName = "archive";
        public const string defaultServersName = "dedicated_server";
        public const string defaultSessionsName = "session";
        public const string defaultLobbiesName = "lobby";
        public const string defaultUsersName = "user";
        public const string defaultPlayersName = "player";

        public List<LibraryData> libraryData;
        public List<UserData> userData;
        public List<PlayerData> playerData;
        public List<ArchiveData> archiveData;
        public List<ServerData> serverData;
        public List<SessionData> sessionData;
        public List<LobbyData> lobbyData;

        public Database()
        {
            libraryData = new List<LibraryData>();
            archiveData = new List<ArchiveData>();
            serverData = new List<ServerData>();
            sessionData = new List<SessionData>();
            lobbyData = new List<LobbyData>();
            userData = new List<UserData>();
            playerData = new List<PlayerData>();
        }

        /// <summary>
        /// Player stats bit representing value of a certain in-game skill
        /// </summary>
        public void Clear()
        {
            libraryData.Clear();
            userData.Clear();
            playerData.Clear();
            archiveData.Clear();
            serverData.Clear();
            sessionData.Clear();
            lobbyData.Clear();
        }
        /// <summary>
        /// Addition of another sandbox's values to this sandbox instance 
        /// </summary>
        /// <param name="anotherDatabase">
        /// Database which values will be added
        /// </param>
        public void Add(Database anotherDatabase)
        {
            libraryData.AddRange(anotherDatabase.libraryData);
            userData.AddRange(anotherDatabase.userData);
            playerData.AddRange(anotherDatabase.playerData);
            archiveData.AddRange(anotherDatabase.archiveData);
            serverData.AddRange(anotherDatabase.serverData);
            sessionData.AddRange(anotherDatabase.sessionData);
            lobbyData.AddRange(anotherDatabase.lobbyData);
        }
    }
    #region Player
    /// <summary>
    /// Sandbox player data tuple
    /// </summary>
    public struct PlayerData : Data
    {
        public int id;
        public string nickname;
        public List<Item> playerInventory;
        public PlayerStats playerStats;
        public string playerStatus;
        public int userID;

        public int Id => id;
        public Data ToData(List<string> data)
        {
            if (data.Count != 6) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            nickname = data[1];
            playerInventory = JsonSerializer.Deserialize<List<Item>>(data[2]);
            playerStats = JsonSerializer.Deserialize<PlayerStats>(data[3]);
            playerStatus = data[4];
            userID = int.Parse(data[5]);
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), nickname, JsonSerializer.Serialize(playerInventory),
                                            JsonSerializer.Serialize(playerStats), playerStatus, userID.ToString() };
        }
    }
    /// <summary>
    /// Player data bit representing a certain item in player's inventory 
    /// </summary>
    public struct Item
    {
        [JsonInclude]
        public string name;
        [JsonInclude]
        public int amount;
    }
    /// <summary>
    /// Player data bit representing all player's in-game stats
    /// </summary>
    public struct PlayerStats
    {
        [JsonInclude]
        public List<string> perks;
        [JsonInclude]
        public List<Skill> skills;
        [JsonInclude]
        public long totalExperience;
    }
    /// <summary>
    /// Player stats bit representing value of a certain in-game skill
    /// </summary>
    public struct Skill
    {
        [JsonInclude]
        public string name;
        [JsonInclude]
        public int level;
    }
    #endregion
    #region User
    /// <summary>
    /// Sandbox user data tuple
    /// </summary>
    public struct UserData : Data
    {
        public int id;
        public string name;
        public TechnicalSpecifications technicalSpecifications;
        public string userIP;
        public UserInfo userInfo;
        public string userStatus;
        public int libraryID;

        public int Id => id;
        public Data ToData(List<string> data)
        {
            if (data.Count != 7) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            name = data[1];
            technicalSpecifications = JsonSerializer.Deserialize<TechnicalSpecifications>(data[2]);
            userIP = data[3];
            userInfo = JsonSerializer.Deserialize<UserInfo>(data[4]);
            userStatus = data[5];
            libraryID = int.Parse(data[6]);
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), name, JsonSerializer.Serialize(technicalSpecifications), userIP,
                                            JsonSerializer.Serialize(userInfo), userStatus, libraryID.ToString() };
        }
    }
    /// <summary>
    /// User data bit representing tech specifications for user's device
    /// </summary>
    public struct TechnicalSpecifications
    {
        [JsonInclude]
        public string OS;
        [JsonInclude]
        public string RAM;
        [JsonInclude]
        public string GPU;
        [JsonInclude]
        public string CPU;
        [JsonInclude]
        public string additionalInfo;
    }
    /// <summary>
    /// User data bit representing miscellanious user information
    /// </summary>
    public struct UserInfo
    {
        [JsonInclude]
        public string location;
        [JsonInclude]
        public string realName;
        [JsonInclude]
        public string customURL;
        [JsonInclude]
        public List<string> achievments;
        [JsonInclude]
        public DateTime registrationDateTime;
    }
    #endregion
    #region Lobby
    /// <summary>
    /// Sandbox lobby data tuple
    /// </summary>
    public struct LobbyData : Data
    {
        public int id;
        public int sessionID;
        public int numberOfParticipants;
        public DateTime creationDate;
        public List<int> playerIDs;

        public int Id => id;
        public Data ToData(List<string> data)
        {
            if (data.Count != 5) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            sessionID = int.Parse(data[1]);
            numberOfParticipants = int.Parse(data[2]);
            creationDate = DateTime.Parse(data[3]);
            playerIDs = JsonSerializer.Deserialize<List<int>>(data[4]);
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), sessionID.ToString(), numberOfParticipants.ToString(),
                                            creationDate.ToString("yyyy-MM-dd HH:mm:ss"), JsonSerializer.Serialize(playerIDs) };
        }
    }
    #endregion
    #region Session
    /// <summary>
    /// Sandbox session data tuple
    /// </summary>
    public struct SessionData : Data
    {
        public int id;
        public int serverID;
        public DateTime startDateTime;
        public DateTime endDateTime;
        public SessionInfo sessionInfo;

        public int Id => id;
        public Data ToData(List<string> data)
        {
            if (data.Count != 5) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            serverID = int.Parse(data[1]);
            startDateTime = DateTime.Parse(data[2]);
            endDateTime = DateTime.Parse(data[3]);
            sessionInfo = JsonSerializer.Deserialize<SessionInfo>(data[4]);
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), serverID.ToString(), startDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                            endDateTime.ToString("yyyy-MM-dd HH:mm:ss"), JsonSerializer.Serialize(sessionInfo) };
        }
    }
    /// <summary>
    /// User data bit representing miscellanious session information
    /// </summary>
    public struct SessionInfo
    {
        [JsonInclude]
        public List<string> gameLog;
        [JsonInclude]
        public string gameMap;
        [JsonInclude]
        public string gameMode;
        [JsonInclude]
        public List<int> participatingLobbies;
    }
    #endregion
    #region Server
    /// <summary>
    /// Sandbox dedicated_server data tuple
    /// </summary>
    public struct ServerData : Data
    {
        public int id;
        public int archiveID;
        public string location;
        public List<int> sessionIDs;
        public bool serverAvailability;
        public int serverCapacity;

        public int Id => id;
        public Data ToData(List<string> data)
        {
            if (data.Count != 6) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            archiveID = int.Parse(data[1]);
            location = data[2];
            sessionIDs = JsonSerializer.Deserialize<List<int>>(data[3]);
            serverAvailability = bool.Parse(data[4]);
            serverCapacity = int.Parse(data[5]);
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), archiveID.ToString(), location, JsonSerializer.Serialize(sessionIDs),
                                            serverAvailability.ToString(), serverCapacity.ToString() };
        }
    }
    #endregion
    #region Archive
    /// <summary>
    /// Sandbox archive data tuple
    /// </summary>
    public struct ArchiveData : Data
    {
        public int id;
        public int libraryID;
        public DateTime initializationDateTime;
        public DateTime suspensionDateTime;
        public int volume;
        public List<int> serverIDs;
        public string region;

        public int Id => id;
        public Data ToData(List<string> data)
        {
            if (data.Count != 7) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            libraryID = int.Parse(data[1]);
            initializationDateTime = DateTime.Parse(data[2]);
            suspensionDateTime = DateTime.Parse(data[3]);
            volume = int.Parse(data[4]);
            serverIDs = JsonSerializer.Deserialize<List<int>>(data[5]);
            region = data[6];
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), libraryID.ToString(), initializationDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                        suspensionDateTime.ToString("yyyy-MM-dd HH:mm:ss"), serverIDs.Count.ToString(), JsonSerializer.Serialize(serverIDs), region };
        }
    }
    #endregion
    #region Library
    /// <summary>
    /// Sandbox library data tuple
    /// </summary>
    public struct LibraryData : Data
    {
        public int id;
        public DateTime creationDate { get; set; }
        public ArchivesInfo archivesInfo { get; set; }
        public UsersInfo usersInfo { get; set; }
        public LibraryUsages libraryUsages { get; set; }
        public int Id => id;
        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), creationDate.ToString("yyyy-MM-dd HH:mm:ss"), JsonSerializer.Serialize(archivesInfo),
                                            JsonSerializer.Serialize(usersInfo), JsonSerializer.Serialize(libraryUsages)};
        }
        public Data ToData(List<string> data)
        {
            if (data.Count != 5) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            creationDate = DateTime.Parse(data[1]);
            archivesInfo = JsonSerializer.Deserialize<ArchivesInfo>(data[2]);
            usersInfo = JsonSerializer.Deserialize<UsersInfo>(data[3]);
            libraryUsages = JsonSerializer.Deserialize<LibraryUsages>(data[4]);
            return this;
        }
    }
    /// <summary>
    /// Library data bit representing list of data usage commentaries
    /// </summary>
    public struct LibraryUsages
    {
        [JsonInclude]
        public List<LibraryUsage> libraryUsages { get; set; }
    }
    /// <summary>
    /// Library data bit representing list of related archives
    /// </summary>
    public struct ArchivesInfo
    {
        [JsonInclude]
        public List<int> archiveIDs { get; set; }
    }
    /// <summary>
    /// Library data bit representing list of related users
    /// </summary>
    public struct UsersInfo
    {
        [JsonInclude]
        public List<int> userIDs { get; set; }
    }
    /// <summary>
    /// Library usage commentary
    /// </summary>
    public struct LibraryUsage
    {
        [JsonInclude]
        public string cause { get; set; }
        [JsonInclude]
        public string result { get; set; }
    }
    #endregion
}
