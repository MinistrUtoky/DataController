using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.Drawing;
using ScottPlot;
using System.Diagnostics;

namespace RandomDataGenerator.classes
{
    public interface Data
    {
        public List<string> ToList();
        public Data ToData(List<string> data);
    }
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
    }
    #region Player
    public struct PlayerData : Data
    {
        public int id;
        public string nickname;
        public List<Item> playerInventory;
        public PlayerStats playerStats;
        public string playerStatus;
        public int userID;

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
    public struct Item
    {
        [JsonInclude]
        public string name;
        [JsonInclude]
        public int amount;
    }
    public struct PlayerStats
    {
        [JsonInclude]
        public List<string> perks;
        [JsonInclude]
        public List<Skill> skills;
        [JsonInclude]
        public long totalExperience;
    }
    public struct Skill
    {
        [JsonInclude]
        public string name;
        [JsonInclude]
        public int level;
    }
    #endregion
    #region User
    public struct UserData : Data
    {
        public int id;
        public string name;
        public TechnicalSpecifications technicalSpecifications;
        public string userIP;
        public UserInfo userInfo;
        public string userStatus;
        public int libraryID;

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
    public struct LobbyData : Data
    {
        public int id;
        public int sessionID;
        public int numberOfParticipants;
        public DateTime creationDate;
        public List<int> playerIDs;

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
    #region Session
    public struct SessionData : Data
    {
        public int id;
        public int serverID;
        public DateTime startDateTime;
        public DateTime endDateTime;
        public SessionInfo sessionInfo;

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
    public struct ServerData : Data
    {
        public int id;
        public int archiveID;
        public string location;
        public List<int> sessionIDs;
        public bool serverAvailability;
        public int serverCapacity;

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
    #region Archive
    public struct ArchiveData : Data
    {
        public int id;
        public int libraryID;
        public DateTime initializationDateTime;
        public DateTime suspensionDateTime;
        public int volume;
        public List<int> serverIDs;
        public string region;

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
    public struct LibraryData : Data
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
    public struct LibraryUsages
    {
        [JsonInclude]
        public List<LibraryUsage> libraryUsages { get; set; }
    }
    public struct ArchivesInfo
    {
        [JsonInclude]
        public List<int> archiveIDs { get; set; }
    }
    public struct UsersInfo
    {
        [JsonInclude]
        public List<int> userIDs { get; set; }
    }
    public struct LibraryUsage
    {
        [JsonInclude]
        public string cause { get; set; }
        [JsonInclude]
        public string result { get; set; }
    }
    #endregion
}
