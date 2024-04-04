using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace DataGenStatistics.classes
{
    /// <summary>
    /// The data values generator for a sandbox
    /// </summary>
    internal static class DataGenerator
    {
        public static Random rand = new Random();
        #region Generatable Items
        public const int maxNumberOfLibraryUsagesOnStart = 3;
        public static readonly string[] causes = new string[] { "Suspicious activity", "Check up" };
        public static readonly string[] results = new string[] { "Abnormalities found", "Nothing out of the ordinary" };
        public static readonly string[] usernames = new string[] { "Binko", "Bonko" };
        public static readonly string[] OSs = new string[] { "Windows", "MacOS", "Linux" };
        public static readonly string[] RAMs = new string[] { "8 GB", "16 GB" };
        public static readonly string[] GPUs = new string[] { "GeForce RTX 4060", "AMD Radeon RX 6800 XT" };
        public static readonly string[] CPUs = new string[] { "Intel Core i5-11400F OEM", "AMD Ryzen 7 5800X" };
        public static readonly string[] additionalInfos = new string[] { "", "Unidentified plugins" };
        public static readonly string[] statuses = new string[] { "banned", "suspended", "active" };
        public static readonly string[] userLocations = new string[] { "Serbia", "Spain" };
        public static readonly string[] realNames = new string[] { "Lublislav", "Pedro" };
        public static readonly string[] customURLs = new string[] { "awawawawo", "un_pedrero" };
        public const int maxNumberOfAchievmentsOnStart = 3;
        public static readonly string[] achievments = new string[] { "Dragonbuster", "Pull Up Maniac" };
        public static readonly string[] playerNames = new string[] { "Sprinkler", "Wafflemaker" };
        public const int maxNumberOfItemsOnStart = 3;
        public const int maxQuantityOfItemOnStart = 10;
        public static readonly string[] items = new string[] { "Stinky rag", "Slinky bat" };
        public const int maxNumberOfPerksOnStart = 3;
        public static readonly string[] perks = new string[] { "Power Power", "Quirkiness" };
        public const int maxNumberOfSkillsOnStart = 2;
        public const int maxLevelOfSkillOnStart = 999;
        public static readonly string[] skills = new string[] { "Game of Tag Proficiency", "Broom Swiping" };
        public const int minExperienceForSkillLevel = 10000;
        public static readonly string[] regions = new string[] { "EU", "NA", "EA" };
        public const int serverCapacityDenominator = 10;
        public const int minServerCap = 2;
        public const int maxServerCap = 200;
        public const int maxLogVolume = 20;
        public static readonly string[] possibleLogQuieries = new string[] { "MONSTER KILL", "RAMPAGE", "ULTRA KILL" };
        public static readonly string[] gameMaps = new string[] { "Castle", "Desert", "Meadows", "Forest" };
        public static readonly string[] gameModes = new string[] { "Tug of War", "Deathmatch", "3v3" };
        public static int maxPlayersInLobby = 4;
        #endregion

        #region Library
        /// <summary>
        /// Generation of a random creation date for library data
        /// </summary>
        /// <returns>
        /// Any DateTime from Unix Epoch up till now
        /// </returns>
        public static DateTime GenerateCreationDate() => GenerateDatetime();
        /// <summary>
        /// Generates no more than a specified amount of library usages
        /// </summary>
        /// <returns>
        /// New LibraryUsages structure containing a list of library usages
        /// </returns>
        public static LibraryUsages GenerateRandomNumberOfLibraryUsages(int upTo)
        {
            return GenerateLibraryUsagesInfo(rand.Next(0, upTo + 1));
        }
        /// <summary>
        /// Generates specified amount of library usages
        /// </summary>
        /// <param name="n">
        /// Number of library usages to generate
        /// </param>
        /// <returns>
        /// New LibraryUsages structure containing a list of library usages
        /// </returns>
        public static LibraryUsages GenerateLibraryUsagesInfo(int n)
        {
            LibraryUsages libraryUsages = new LibraryUsages();
            libraryUsages.libraryUsages = new List<LibraryUsage>();
            for (int i = 0; i < n; i++) libraryUsages.libraryUsages.Add(GenerateLibraryUsage());
            return libraryUsages;
        }
        /// <summary>
        /// Generates random library usage
        /// </summary>
        /// <returns>
        /// New LibraryUsage structure
        /// </returns>
        public static LibraryUsage GenerateLibraryUsage()
        {
            LibraryUsage libraryUsage = new LibraryUsage();
            libraryUsage.cause = GenerateCause();
            libraryUsage.result = GenerateResult();
            return libraryUsage;
        }
        /// <summary>
        /// Selects random cause for a library usage
        /// </summary>
        /// <returns>
        /// Selected usage cause string
        /// </returns>
        public static string GenerateCause()
        {
            return causes[rand.Next(0, causes.Length)];
        }
        /// <summary>
        /// Selects random result of a library usage
        /// </summary>
        /// <returns>
        /// Selected usage result string
        /// </returns>
        public static string GenerateResult()
        {
            return results[rand.Next(0, causes.Length)];
        }
        #endregion

        #region User
        /// <summary>
        /// Generates random user nickname
        /// </summary>
        /// <returns>
        /// Selected nickname with random numerical ending 
        /// </returns>
        public static string GenerateUsername()
        {
            StringBuilder username = new StringBuilder();
            username.Append(usernames[rand.Next(0, usernames.Length)]);
            username.Append((int)Math.Pow(DBClass.GetNextId(Database.defaultUsersName)*2, 3)+7);
            return username.ToString();
        }
        /// <summary>
        /// Generates random user's device's technical specifications 
        /// </summary>
        /// <returns>
        /// New TechnicalSpecifications structure
        /// </returns>
        public static TechnicalSpecifications GenerateTechnicalSpecifications()
        {
            TechnicalSpecifications technicalSpecifications = new TechnicalSpecifications();
            technicalSpecifications.OS = OSs[rand.Next(0, OSs.Length)];
            technicalSpecifications.RAM = RAMs[rand.Next(0, RAMs.Length)];
            technicalSpecifications.GPU = GPUs[rand.Next(0, GPUs.Length)];
            technicalSpecifications.CPU = CPUs[rand.Next(0, CPUs.Length)];
            technicalSpecifications.additionalInfo = additionalInfos[rand.Next(0, additionalInfos.Length)];
            return technicalSpecifications;
        }
        /// <summary>
        /// Generates random user IP
        /// </summary>
        /// <returns>
        /// IP address string
        /// </returns>
        public static string GenerateUserIP()
        {
            var data = new byte[4];
            rand.NextBytes(data);
            IPAddress ip = new IPAddress(data);
            return ip.ToString();
        }
        /// <summary>
        /// Generates random user info 
        /// </summary>
        /// <returns>
        /// New UserInfo structure
        /// </returns>
        public static UserInfo GenerateUserInfo()
        {
            UserInfo newUserInfo = new UserInfo();
            newUserInfo.location = userLocations[rand.Next(0, userLocations.Length)];
            newUserInfo.realName = usernames[rand.Next(0, realNames.Length)];
            newUserInfo.customURL = customURLs[rand.Next(0, customURLs.Length)];
            newUserInfo.achievments = new List<string>();
            for (int i = 0; i < maxNumberOfAchievmentsOnStart; i++)
                newUserInfo.achievments.Add(achievments[rand.Next(0, achievments.Length)]);
            return newUserInfo;
        }
        /// <summary>
        /// Selects random user status from preset array of statuses
        /// </summary>
        /// <returns>
        /// Status string (standard: banned, active, suspended)
        /// </returns>
        public static string GenerateUserStatus() 
        {
            return statuses[rand.Next(0, statuses.Length)];
        }
        #endregion

        #region Player
        /// <summary>
        /// Generates random player nickname
        /// </summary>
        /// <returns>
        /// Selected nickname with random numerical ending 
        /// </returns>
        public static string GeneratePlayerNickname()
        {
            
            StringBuilder name = new StringBuilder();
            name.Append(playerNames[rand.Next(0, playerNames.Length)]);
            name.Append((int)Math.Pow(DBClass.GetNextId(Database.defaultUsersName) * 3, 2) + 31);
            return name.ToString();
        }
        /// <summary>
        /// Generates random new player inventory contents
        /// </summary>
        /// <returns>
        /// List of items names and quantities
        /// </returns>
        public static List<Item> GenerateInventory()
        {
            
            List<Item> inv = new List<Item>();
            List<string> itemsNotYetInInv = items.ToList();
            for (int i = 0; i < rand.Next(0, Math.Min(items.Length, maxNumberOfItemsOnStart)+1) & itemsNotYetInInv.Count > 0; i++)
            {
                Item item = new Item();
                int index = rand.Next(0, itemsNotYetInInv.Count);
                item.name = itemsNotYetInInv[index];
                itemsNotYetInInv.RemoveAt(index);
                item.amount = rand.Next(0, maxQuantityOfItemOnStart+1);
                inv.Add(item);
            }
            return inv;
        }
        /// <summary>
        /// Generates random new set of stats for a player from preset arrays of parameters
        /// </summary>
        /// <returns>
        /// New PlayerStats structure
        /// </returns>
        public static PlayerStats GenerateStats()
        {
            PlayerStats stats = new PlayerStats();
            stats.perks = new List<string>();
            stats.skills = new List<Skill>();
            List<string> unusedPerks = perks.ToList();
            List<string> unusedSkills = skills.ToList();
            for (int i = 0; i < rand.Next(0, Math.Min(perks.Length,maxNumberOfPerksOnStart) + 1); i++)
            {
                int index = rand.Next(0, unusedPerks.Count);
                stats.perks.Add(unusedPerks[index]);
                unusedPerks.RemoveAt(index);
            }
            int totalLevel = 0;
            for (int i = 0; i < rand.Next(0, Math.Min(skills.Length, maxNumberOfSkillsOnStart) + 1); i++)
            {
                int index = rand.Next(0, unusedSkills.Count);
                Skill skill = new Skill();
                skill.name = unusedSkills[index];
                unusedSkills.RemoveAt(index);
                skill.level = rand.Next(0, maxLevelOfSkillOnStart+1);
                totalLevel += skill.level;
                stats.skills.Add(skill); 
            }
            stats.totalExperience = rand.Next(totalLevel * minExperienceForSkillLevel, maxLevelOfSkillOnStart * skills.Count() * minExperienceForSkillLevel + 1);
            return stats;
        }
        /// <summary>
        /// Selects random player status from preset array of statuses
        /// </summary>
        /// <returns>
        /// Status string (standard: banned, active, suspended)
        /// </returns>
        public static string GenerateStatus()
        {
            return statuses[rand.Next(0, statuses.Length)];
        }
        #endregion

        #region Archive
        /// <summary>
        /// Selects random world region from preset array of regions
        /// </summary>
        /// <returns>
        /// Region abbr string
        /// </returns>
        public static string GenerateRegion()
        {
            return regions[rand.Next(0, regions.Length)];
        }
        #endregion

        #region Server
        /// <summary>
        /// Generates random location IP
        /// </summary>
        /// <returns>
        /// IP address string
        /// </returns>
        public static string GenerateLocation()
        {
            var data = new byte[4];
            rand.NextBytes(data);
            IPAddress ip = new IPAddress(data);
            return ip.ToString();
        }
        /// <summary>
        /// Randomly chooses if the server is avaliable as of now
        /// </summary>
        /// <returns>
        /// Server avaliability boolean
        /// </returns>
        public static bool GenerateServerAvailability()
        {
            
            return rand.Next(0,2)==0;
        }
        /// <summary>
        /// Generates random serever capacity within preset capacity limits
        /// </summary>
        /// <returns>
        /// Server capacity integer
        /// </returns>
        public static int GenerateServerCapacity()
        {
            return rand.Next(minServerCap, maxServerCap+1)*serverCapacityDenominator;
        }
        #endregion

        #region Session
        /// <summary>
        /// Generates random new session info all related data
        /// </summary>
        /// <returns>
        /// Generated SessionInfo structure
        /// </returns>
        public static SessionInfo GenerateSessionInfo()
        {
            SessionInfo newSession = new SessionInfo();
            newSession.gameLog = new List<string>();
            for (int i = 0; i < rand.Next(0,maxLogVolume+1); i++) newSession.gameLog?.Add(GenerateGameLogQuery());
            newSession.gameMap = GenerateGameMap();
            newSession.gameMode = GenerateGameMode();
            newSession.participatingLobbies = new List<int>();
            return newSession;
        }
        /// <summary>
        /// Selects random game log query from possible game log queries
        /// </summary>
        /// <returns>
        /// Game log query
        /// </returns>
        public static string GenerateGameLogQuery()
        {            
            return possibleLogQuieries[rand.Next(0, possibleLogQuieries.Length)];
        }
        /// <summary>
        /// Selects random game map name from possible game maps
        /// </summary>
        /// <returns>
        /// Game map name
        /// </returns>
        public static string GenerateGameMap()
        {
            
            return gameMaps[rand.Next(0, gameMaps.Length)];
        }
        /// <summary>
        /// Selects random game mode from possible game modes
        /// </summary>
        /// <returns>
        /// Game mode
        /// </returns>
        public static string GenerateGameMode()
        {
            
            return gameModes[rand.Next(0, gameModes.Length)];
        }
        #endregion

        #region Lobby
        #endregion

        #region General Generators and Randomizers
        /// <summary>
        /// Generation of a random datetime
        /// </summary>
        /// <returns>
        /// Any DateTime from Unix Epoch up till the present date and time 
        /// </returns>
        public static DateTime GenerateDatetime() => GenerateDatetime(DateTime.UnixEpoch, DateTime.Now);
        /// <summary>
        /// Generation of a random datetime within set time period
        /// </summary>
        /// <param name="startDate">
        /// Earliest datetime possible to be generated
        /// </param>
        /// <param name="endDate">
        /// Latest datetime possible to be generated
        /// </param> 
        /// <returns>
        /// DateTime between startDate and endDate
        /// </returns>
        public static DateTime GenerateDatetime(DateTime startDate, DateTime endDate)
        {
            if ((startDate - DateTime.UnixEpoch).TotalSeconds < 0) startDate = DateTime.UnixEpoch;
            TimeSpan timeSpan = endDate - startDate;
            TimeSpan newSpan = new TimeSpan(0, 0, rand.Next(0, (int)(timeSpan.TotalSeconds + 1)));
            return startDate + newSpan;
        }
        /// <summary>
        /// Generation of a random datetime after set datetime and before the present
        /// </summary>
        /// <param name="dt">
        /// Earliest datetime possible to be generated
        /// </param>
        /// <returns>
        /// DateTime after dt and earlier than present
        /// </returns>
        public static DateTime GenerateDatetimeAfter(DateTime dt) => GenerateDatetime(dt, DateTime.Now);
        /// <summary>
        /// Selection of a random "master" object from two lists of a specified type.
        /// </summary>
        /// <param name="possibeMastersInDB">
        /// First list to select master from
        /// </param>
        /// <param name="possibleMastersInDBDelta">
        /// Second list to select master from
        /// </param>
        /// <returns>
        /// Master object of specific type 
        /// </returns>
        public static T RandomMaster<T>(List<T> possibeMastersInDB, List<T> possibleMastersInDBDelta)
        {
            List<T> list = possibeMastersInDB.Concat(possibleMastersInDBDelta).ToList();
            
            return list[rand.Next(0, list.Count)];
        }
        /// <summary>
        /// Selection of a batch of random ids from the list of data.
        /// </summary>
        /// <param name="data">
        /// List of Data structures to select master from
        /// </param>
        /// <param name="n">
        /// How many ids to select
        /// </param>
        /// <returns>
        /// List of selected ids
        /// </returns>
        public static List<int> GetRandomIds(List<Data> data, int n)
        {
            List<int> ids = new();
            int index;
            for (int i = 0; i < n; i++)
            {
                index = rand.Next(0, data.Count);
                ids.Add(data[index].Id);
                data.RemoveAt(index);
            }
            return ids;
        }
        /// <summary>
        /// List shuffler
        /// </summary>
        /// <param name="list">
        /// List to shuffle
        /// </param>
        /// <returns>
        /// Shuffled list
        /// </returns>
        public static List<T> Shuffle<T>(List<T> list)
        {
            int length = list.Count;
            while (length > 1)
            {
                length--;
                int randomNumber = rand.Next(length + 1);
                T value = list[randomNumber];
                list[randomNumber] = list[length];
                list[length] = value;
            }
            return list;
        }
        #endregion

    }
}
