using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Xml.Linq;

///<summary>
/// This document is made for declaring all the classes related to sandbox database sourcing the ORM
/// </summary>
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
    /// Abstraction of table's properties
    /// </summary>
    public interface ITable
    {
        public int Count { get; }
        public string Name { get; set; }
        public bool[] PrimaryKeys { get; set; }
        public string[] ForeignKeys { get; set; }
        public string[] ColumnTypes { get; set; }
        public string[] ColumnNames { get; set; }
        /// <summary>
        /// INSERT INTO thisTableName SELECT ... abstraction
        /// </summary>
        /// <param name="abstractInsertion">
        /// Sub-table rows to insert
        /// </param>
        public void InsertAbstract(List<object> abstractInsertion);
        /// <summary>
        /// INSERT INTO thisTableName SELECT ... abstraction (But it's table this time around)
        /// </summary>
        /// <param name="abstractInsertion">
        /// Sub-table rows to insert
        /// </param>
        public void InsertAbstract(Table<object> abstractInsertion);
        /// <summary>
        /// SELECT *  FROM thisTable abstraction
        /// </summary>
        /// <returns>
        /// Returns the copy of this table's contents
        /// </returns>
        public List<object> SelectAllAbstract();
        /// <summary>
        /// SELECT TOP n FROM thisTable abstraction
        /// </summary>
        /// <param name="n">
        /// Number of top rows to select
        /// </param>
        /// <returns>
        /// Returns n first items from this table's contents
        /// </returns>
        public List<object> SelectTopAbstract(int n);
        /// <summary>
        /// Deletion command 1 (deleting top rows) (DELETE TOP n FROM thisTable)
        /// </summary>
        /// <param name="n">
        /// Number of top rows to delete
        /// </param>
        public void DeleteTop(int n);
        /// <summary>
        /// Deletion command 2 (deleting all rows) (DELETE *  FROM thisTable)
        /// </summary>
        public void DeleteAll();
        /// <summary>
        /// Casts all the table's contents against an object type for generalization purposes
        /// </summary>
        /// <returns>
        /// This table's clone, but each item is of object type
        /// </returns>
        public Table<object> CastAbstract();
    }

    [Serializable]
    /// <summary>
    /// Sandbox table iterator with main comands
    /// </summary>
    /// <typeparam name="SomeData">
    /// Data type of table's contents
    /// </typeparam>
    public class Table<SomeData> : IEnumerator<SomeData>, ITable
    {
        int position = -1;
        [JsonIgnore]
        public int Count => Rows.Count;
        public string Name { get; set; }
        public bool[] PrimaryKeys { get; set; }
        public string[] ForeignKeys { get; set; }
        public string[] ColumnNames { get; set; }
        public string[] ColumnTypes { get; set; }
        public List<SomeData> Rows { get; set; }

        public Table()
        {
            Name = ""; ColumnNames = new string[0]; ColumnTypes = new string[0]; Rows = new List<SomeData>(); PrimaryKeys = new bool[0]; ForeignKeys = new string[0];
        }
        /// <summary>
        /// Constructor of a new empty table with all it's components, including name, column names, primary keys and foreign keys
        /// </summary>
        /// <param name="name">
        /// The name of a new table, mirrors the name in the relational database
        /// </param>
        /// <param name="columnNames">
        /// Names of all the table's columns mirroring those of the relational DB
        /// </param>
        /// <param name="columnTypes">
        /// Types without constraint information according to column names
        /// </param>
        /// <param name="primaryKeys">
        /// Signals if the column with the same index is a primary key 
        /// </param>
        /// <param name="foreignKeys">
        /// Signals if the column with the same index is a primary key 
        /// </param>
        public Table(string name, string[] columnNames, string[] columnTypes, bool[] primaryKeys, string[] foreignKeys)
        {
            Name = name; ColumnNames = columnNames; ColumnTypes = columnTypes; Rows = new List<SomeData>(); PrimaryKeys = primaryKeys; ForeignKeys = foreignKeys;
        }
        /// <summary>
        /// Constructor of an empty table with the name only
        /// </summary>
        /// <param name="name">
        /// Set name of the table
        /// </param>
        public Table(string name)
        {
            Name = name; ColumnNames = new string[0]; ColumnTypes = new string[0]; Rows = new List<SomeData>(); PrimaryKeys = new bool[0]; ForeignKeys = new string[0];
        }
        /// <summary>
        /// Insertion command 1 (inserting sub-table of rows) (INSERT INTO thisTableName SELECT ...)
        /// </summary>
        /// <param name="rows">
        /// Sub-table rows to insert
        /// </param>
        public void Insert(List<SomeData> rows) => Rows.AddRange(rows);
        /// <summary>
        /// Insertion command 2 (inserting individual item) (INSERT INTO thisTableName VALUES(...))
        /// </summary>
        /// <param name="item">
        /// Sub-table rows to insert
        /// </param>
        public void Insert(SomeData item) => Rows.Add(item);
        /// <summary>
        /// Insertion command 3 (inserting another table's rows) (INSERT INTO thisTableName SELECT * FROM anotherTable)
        /// </summary>
        /// <param name="table">
        /// Table, holding rows to insert
        /// </param>
        public void Insert(Table<SomeData> table) => Rows.AddRange(table.SelectAll());
        /// <summary>
        /// Selection command 1 (selecting top rows) (SELECT TOP n FROM thisTable)
        /// </summary>
        /// <param name="n">
        /// Number of top rows to select
        /// </param>
        public List<SomeData> SelectTop(int n) => Rows.GetRange(0, n).ToList();
        /// <summary>
        /// Selection command 2 (selecting all rows) (SELECT *  FROM thisTable)
        /// </summary>
        public List<SomeData> SelectAll() => Rows.ToList();
        /// <summary>
        /// INSERT INTO thisTableName SELECT ... abstraction
        /// </summary>
        /// <param name="abstractInsertion">
        /// Sub-table rows to insert
        /// </param>
        public void InsertAbstract(List<object> abstractInsertion) => Rows.AddRange(abstractInsertion.Cast<SomeData>().ToList());
        /// <summary>
        /// INSERT INTO thisTableName SELECT ... abstraction (But it's table this time around)
        /// </summary>
        /// <param name="abstractInsertion">
        /// Sub-table rows to insert
        /// </param>
        public void InsertAbstract(Table<object> abstractInsertion) => Rows.AddRange(abstractInsertion.Rows.Cast<SomeData>().ToList());
        /// <summary>
        /// SELECT TOP n FROM thisTable abstraction
        /// </summary>
        /// <param name="n">
        /// Number of top rows to select
        /// </param>
        public List<object> SelectTopAbstract(int n) => SelectTop(n).Cast<object>().ToList();
        /// <summary>
        /// SELECT * FROM thisTable abstraction
        /// </summary>
        public List<object> SelectAllAbstract() => Rows.ToList().Cast<object>().ToList();
        /// <summary>
        /// Deletion command 1 (deleting top rows) (DELETE TOP n FROM thisTable)
        /// </summary>
        /// <param name="n">
        /// Number of top rows to delete
        /// </param>
        public void DeleteTop(int n) => Rows.RemoveRange(0, n);
        /// <summary>
        /// Deletion command 2 (deleting all rows) (DELETE *  FROM thisTable)
        /// </summary>
        public void DeleteAll() => Rows.Clear();
        /// <summary>
        /// Tries to cast table of .NET objects against another, resulting in table of data of set generic type
        /// </summary>
        /// <param name="objectTable">
        /// Object type table to cast against table of the set generic type 
        /// </param>
        /// <returns>
        /// Returns table of set generic type or sets up an exception
        /// </returns>
        public static Table<SomeData> TryCast(Table<object> objectTable)
        {
            try
            {
                Table<SomeData> table = new Table<SomeData>(objectTable.Name, objectTable.ColumnNames,
                    objectTable.ColumnTypes, objectTable.PrimaryKeys, objectTable.ForeignKeys);
                table.Insert(objectTable.Cast<SomeData>().SelectAll());
                return table;
            }
            catch
            {
                throw new Exception("Unfitting type of table to cast against");
            }
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public Table<T1> Cast<T1>()
        {
            Table<T1> table = new Table<T1>(Name, ColumnNames, ColumnTypes, PrimaryKeys, ForeignKeys);
            table.Insert(Rows.Cast<T1>().ToList());
            return table;
        }

        public Table<object> CastAbstract() => Cast<object>();

        [JsonIgnore]
        public object Current
        {
            get
            {
                if (position != -1 & position < Rows.Count) return Rows[position];
                throw new ArgumentException();
            }
        }

        SomeData IEnumerator<SomeData>.Current => (SomeData)Current;

        public void Dispose() { }
        public void Reset() { position = -1; }
        public bool MoveNext()
        {
            if (position < Rows.Count - 1)
            {
                position++; return true;
            }
            else return false;
        }
    }

    /// <summary>
    /// Sandbox junction table data tuple
    /// </summary>
    public struct Junction : Data
    {
        private int id;
        public int firstTableId;
        public int secondTableId;
        public int Id { get { return id; } set { id = value; }}
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            firstTableId = int.Parse(data[1]);
            secondTableId = int.Parse(data[2]);
            return this;
        }

        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), firstTableId.ToString(), secondTableId.ToString() };
        }
    }

    /// <summary>
    /// Sandbox database
    /// </summary>
    public struct Database
    {
        public List<string> schema { get; set; }
        public List<Table<Junction>> junctionTables { get; set; }

        public Table<LibraryData> libraryData { get ; set; }
        public Table<UserData> userData { get; set; }
        public Table<PlayerData> playerData { get; set; }
        public Table<ArchiveData> archiveData { get; set; }
        public Table<ServerData> serverData { get; set; }
        public Table<SessionData> sessionData { get; set; }
        public Table<LobbyData> lobbyData { get; set; }

        /// <summary>
        /// ORM database constructor, where the table properties of this class are declared and the new tables in relational database fitting table properties of this class are created
        /// </summary>
        /// <param name="main">
        /// Indexes if the database should de
        /// </param>
        public Database(bool main=false)
        {
            schema = new List<string>();
            junctionTables = new List<Table<Junction>>();
            libraryData = new Table<LibraryData>("library", new string[] { "ID", "CREATION_DATE", "ARCHIVES_INFO", "ALL_TIME_USERS_INFO", "LIBRARY_USAGES_INFO" },
                new string[] { "INT", // NOT NULL PRIMARY KEY IDENTITY(1,1)
                                "DATETIME NULL",
                                "NVARCHAR(MAX) NULL",
                                "NVARCHAR(MAX) NULL",
                                "NVARCHAR(MAX) NULL"},
                new bool[] { true,false,false,false,false },
                new string[] { "", "", "", "", "" }); 
            archiveData = new Table<ArchiveData>("archive", new string[] { "ID", "LIBRARY_ID", "INITIALIZATION_DATE", "SUSPENSION_DATE", "VOLUME", "ARCHIVED_INFO", "REGION" },
                new string[] { "INT",// NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                "INT NOT NULL",
                                "DATETIME NULL",
                                "DATETIME NULL",
                                "INT NULL",
                                "NVARCHAR(MAX) NULL",
                                "VARCHAR(10)"},
                new bool[] { true,false,false,false,false, false, false }, 
                new string[] { "", libraryData.Name + "(ID)", "", "", "", "", "" });
            serverData = new Table<ServerData>("dedicated_server", 
                new string[] { "ID", "ARCHIVE_ID", "LOCATION", "PAST_SESSIONS_INFO", "SERVER_AVAILABILITY", "SERVER_CAPACITY" },
                new string[] { "INT",// NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                "INT NOT NULL",
                                "VARCHAR(500) NULL",
                                "NVARCHAR(MAX) NULL",
                                "BINARY(1) NULL",
                                "INT NULL"},
                new bool[] { true,false,false,false,false, false }, 
                new string[] { "", archiveData.Name + "(ID)", "", "", "", "" });
            sessionData = new Table<SessionData>("session", 
                new string[] { "ID", "DEDICATED_SERVER_ID", "START_DATETIME", "END_DATETIME", "SESSION_INFO" },
                new string[] { "INT",// NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                "INT NOT NULL",
                                "DATETIME NULL",
                                "DATETIME NULL",
                                "NVARCHAR(MAX) NULL"},
                new bool[] { true,false,false,false,false },
                new string[] { "", serverData.Name + "(ID)", "", "", "" });
            lobbyData = new Table<LobbyData>("lobby", new string[] { "ID", "SESSION_ID", "NUMBER_OF_PARTICIPANTS", "CREATION_DATE", "PARTICIPANTS_INFO" },
                new string[] { "INT",// NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                "INT NOT NULL",
                                "INT NULL",
                                "DATETIME NULL",
                                "NVARCHAR(MAX) NULL"},
                new bool[] { true,false,false,false,false }, 
                new string[] { "", sessionData.Name + "(ID)", "", "", "" });
            userData = new Table<UserData>("users", new string[] { "ID", "NAME", "TECHNICAL_SPECIFICATIONS", "USER_IP", "USER_INFO", "USER_STATUS", "LIBRARY_ID" },
                new string[] { "INT",// NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                "VARCHAR(45) NULL",
                                "NVARCHAR(MAX) NULL",
                                "VARCHAR(45) NULL",
                                "NVARCHAR(MAX) NULL",
                                "VARCHAR(45) NULL",
                                "INT NOT NULL"},
                new bool[] { true,false,false,false,false, false, false }, 
                new string[] { "", "", "", "", "", "", libraryData.Name + "(ID)" });
            playerData = new Table<PlayerData>("player", new string[] { "ID", "NICKNAME", "PLAYER_STATS", "PLAYER_INVENTORY", "PLAYER_STATUS", "USER_ID" },
                new string[] { "INT",// NOT NULL PRIMARY KEY IDENTITY(1,1)",
                                "VARCHAR(45) NULL",
                                "NVARCHAR(MAX) NULL",
                                "NVARCHAR(MAX) NULL",
                                "VARCHAR(45) NULL",
                                "INT NOT NULL"},
                new bool[] { true,false,false,false,false, false }, 
                new string[] { "", "", "", "", "", "[" + userData.Name + "](ID)" });

            foreach (var property in typeof(Database).GetProperties().ToList())
                if (property.GetValue(this) is ITable)
                    schema.Add(((ITable)property.GetValue(this)).Name);

            if (main)
                DatabaseSandbox.SeedDatabaseToMSQLServer(this);
        }

        /// <summary>
        /// Player stats bit representing value of a certain in-game skill
        /// </summary>
        public void Clear()
        {
            foreach (var property in typeof(Database).GetProperties().ToList())
                if (property.GetValue(this) is ITable)
                    ((ITable)property.GetValue(this)).DeleteAll();
        }
        /// <summary>
        /// Addition of another sandbox's values to this sandbox instance 
        /// </summary>
        /// <param name="anotherDatabase">
        /// Database which values will be added
        /// </param>
        public void Add(Database anotherDatabase)
        {
            foreach (var property in typeof(Database).GetProperties().ToList())
                if (property.GetValue(this) is ITable)
                    ((ITable)property.GetValue(this)).InsertAbstract(((ITable)property.GetValue(anotherDatabase)).SelectAllAbstract());           
        }
        public List<Table<object>> TablesList() 
        {
            List<Table<object>> tablesList = new List<Table<object>>();
            foreach (var property in typeof(Database).GetProperties().ToList())
                if (property.GetValue(this) is ITable)
                    tablesList.Add(((ITable)property.GetValue(this)).CastAbstract());
            return tablesList;
        }
    }
    #region Player
    /// <summary>
    /// Sandbox player data tuple
    /// </summary>
    public struct PlayerData : Data
    {
        private int id;
        public string nickname{ get; set; }
        public List<Item> playerInventory{ get; set; }
        public PlayerStats playerStats{ get; set; }
        public string playerStatus{ get; set; }
        public int userID{ get; set; }

        public int Id { get { return id; } set { id = value; }}
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
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
        public string name{ get; set; }
        public int amount { get; set; }
    }
    /// <summary>
    /// Player data bit representing all player's in-game stats
    /// </summary>
    public struct PlayerStats 
    {
        public List<string> perks { get; set; }
        public List<Skill> skills { get; set; }
        public long totalExperience { get; set; }
    }
    /// <summary>
    /// Player stats bit representing value of a certain in-game skill
    /// </summary>
    public struct Skill
    {
        public string name { get; set; }
        public int level { get; set; }
    }
    #endregion
    #region User
    /// <summary>
    /// Sandbox user data tuple
    /// </summary>
    public struct UserData : Data
    {
        private int id;
        public string name { get; set; }
        public TechnicalSpecifications technicalSpecifications { get; set; }
        public string userIP { get; set; }
        public UserInfo userInfo { get; set; }
        public string userStatus { get; set; }
        public int libraryID { get; set; }

        public int Id { get { return id; } set { id = value; }}
        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), name, JsonSerializer.Serialize(technicalSpecifications), userIP,
                                            JsonSerializer.Serialize(userInfo), userStatus, libraryID.ToString() };
        }
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
            id = int.Parse(data[0]);
            name = data[1];
            technicalSpecifications = JsonSerializer.Deserialize<TechnicalSpecifications>(data[2]);
            userIP = data[3];
            userInfo = JsonSerializer.Deserialize<UserInfo>(data[4]);
            userStatus = data[5];
            libraryID = int.Parse(data[6]);
            return this;
        }
    }
    /// <summary>
    /// User data bit representing tech specifications for user's device
    /// </summary>
    public struct TechnicalSpecifications
    {
        public string OS { get; set; }
        public string RAM { get; set; }
        public string GPU { get; set; }
        public string CPU { get; set; }
        public string additionalInfo { get; set; }
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
        private int id;
        public int sessionID { get; set; }
        public int numberOfParticipants { get; set; }
        public DateTime creationDate { get; set; }
        public List<int> playerIDs { get; set; }

        public int Id { get { return id; } set { id = value; }}
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
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
        private int id;
        public int serverID { get; set; }
        public DateTime startDateTime { get; set; }
        public DateTime endDateTime { get; set; }
        public SessionInfo sessionInfo { get; set; }

        public int Id { get { return id; } set { id = value; }}
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
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
        private int id;
        public int archiveID { get; set; }
        public string location { get; set; }
        public List<int> sessionIDs { get; set; }
        public bool serverAvailability { get; set; }
        public int serverCapacity { get; set; }

        public int Id { get { return id; } set { id = value; }}
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
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
        private int id;
        public int libraryID { get; set; }
        public DateTime initializationDateTime { get; set; }
        public DateTime suspensionDateTime { get; set; }
        public int volume { get; set; }
        public List<int> serverIDs { get; set; }
        public string region { get; set; }

        public int Id { get { return id; } set { id = value; }}
        public Data ToData(List<string> data)
        {
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
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
        private int id;
        public DateTime creationDate { get; set; } 
        public ArchivesInfo archivesInfo { get; set; }
        public UsersInfo usersInfo { get; set; }
        public LibraryUsages libraryUsages { get; set; }
        public int Id { get { return id; } set { id = value; }}
        public List<string> ToList()
        {
            return new List<string>() { id.ToString(), creationDate.ToString("yyyy-MM-dd HH:mm:ss"), JsonSerializer.Serialize(archivesInfo),
                                            JsonSerializer.Serialize(usersInfo), JsonSerializer.Serialize(libraryUsages)};
        }
        public Data ToData(List<string> data)
        {
            if (data.Any(item => item == "")) throw new Exception("Can't parse empty data");
            if (data.Count != GetType().GetFields().Count() + GetType().GetProperties().Count()) throw new Exception("Wrong data size format");
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
