using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace RandomDataGenerator.classes
{
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
}
