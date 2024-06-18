using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using JsonSerializer = System.Text.Json.JsonSerializer;

///<summary>
/// This document is made for general interaction between main wpf application and other 3 data-related documents
/// </summary>
namespace DataGenStatistics.classes
{
    /// <summary>
    /// The database sandbox 
    /// </summary>
    internal class DatabaseSandbox
    {
        public static DatabaseSandbox Instance = new DatabaseSandbox();
        internal Database database = new Database(true);
        internal Database databaseDelta = new Database(false);
        /// <summary>
        /// Initiating the sandbox
        /// </summary>
        public void Init()
        {
            database = new Database(true);
            databaseDelta = new Database(false);
            //DBClass.DropAll();
            //SeedDatabaseToMSQLServer(database);
            //DBClass.ClearAllTables();

            FetchAllTablesFromSQL();
            //MakeDBBackup();//MakeDBBackupJsonToStandardLocalFolder();
            RestoreBackupIntoDB(); //RestoreBackupIntoDBDeltaFromStandardFolderJson();
        }

        /*
        private Type CreateDocumentationUsingDatabaseProxy()
        {
            AppDomain domain = Thread.GetDomain();
            AssemblyName asmName = new AssemblyName();
            asmName.Name = "DatabaseAssembly";
            AssemblyBuilder myAsmBuilder = domain.DefineDynamicAssembly(asmName,
                                                            AssemblyBuilderAccess.RunAndSave);
            // Generate a persistable single-module assembly.
            ModuleBuilder modBuilder =
                myAsmBuilder.DefineDynamicModule(asmName.Name, asmName.Name + ".dll");

            TypeBuilder typeBuilder = modBuilder.DefineType("Database",
                                                            TypeAttributes.Public);
            FieldBuilder customerNameBldr = typeBuilder.DefineField("someData",
                                                            typeof(string),
                                                            FieldAttributes.Private);
            PropertyBuilder custNamePropBldr = typeBuilder.DefineProperty("SomeData",
                                                             PropertyAttributes.HasDefault,
                                                             typeof(string),
                                                             null);
            MethodAttributes getSetAttr =
                MethodAttributes.Public | MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig;
            MethodBuilder custNameGetPropMthdBldr =
                typeBuilder.DefineMethod("get_CustomerName",
                                           getSetAttr,
                                           typeof(string),
                                           Type.EmptyTypes);

            ILGenerator custNameGetIL = custNameGetPropMthdBldr.GetILGenerator();

            custNameGetIL.Emit(OpCodes.Ldarg_0);
            custNameGetIL.Emit(OpCodes.Ldfld, customerNameBldr);
            custNameGetIL.Emit(OpCodes.Ret);

            MethodBuilder custNameSetPropMthdBldr =
                typeBuilder.DefineMethod("set_CustomerName",
                                           getSetAttr,
                                           null,
                                           new Type[] { typeof(string) });

            ILGenerator custNameSetIL = custNameSetPropMthdBldr.GetILGenerator();

            custNameSetIL.Emit(OpCodes.Ldarg_0);
            custNameSetIL.Emit(OpCodes.Ldarg_1);
            custNameSetIL.Emit(OpCodes.Stfld, customerNameBldr);
            custNameSetIL.Emit(OpCodes.Ret);

            custNamePropBldr.SetGetMethod(custNameGetPropMthdBldr);
            custNamePropBldr.SetSetMethod(custNameSetPropMthdBldr);

            Type retval = typeBuilder.CreateType();

            myAsmBuilder.Save(asmName.Name + ".dll");
            return retval;
        }*/

        /// <summary>
        /// Making the database backup into a local database
        /// </summary>
        private void MakeDBBackup()
        {
            DBClass.DropDatabase(DBClass.dbName.Split(".")[0] + "Copy");
            if (File.Exists(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + DBClass.dbHeadFolder + DBClass.dbName.Split(".")[0] + "Copy.mdf"))
                File.Delete(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + DBClass.dbHeadFolder + DBClass.dbName.Split(".")[0] + "Copy.mdf");
            if (File.Exists(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + DBClass.dbHeadFolder + DBClass.dbName.Split(".")[0] + "Copy_log.ldf"))
                File.Delete(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + DBClass.dbHeadFolder + DBClass.dbName.Split(".")[0] + "Copy_log.ldf");
            DBClass.CreateNewDatabase(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName, DBClass.dbName.Split(".")[0] + "Copy");
            DBClass.dbName = DBClass.dbName.Split(".")[0] + "Copy.mdf";
            SeedDatabaseToMSQLServer(database);
            databaseDelta.Add(database);
            database.Clear();
            PutDeltaIntoDB();
            DBClass.dbName = string.Join("", DBClass.dbName.Split("Copy"));
        }

        /// <summary>
        /// Restoring the database from local database file backup from
        /// </summary>
        private void RestoreBackupIntoDB()
        {
            DBClass.dbName = DBClass.dbName.Split(".")[0] + "Copy.mdf";
            database.Clear();
            FetchAllTablesFromSQL();
            DBClass.dbName = string.Join("", DBClass.dbName.Split("Copy"));
            DBClass.ClearAllTables();
            databaseDelta.Add(database);
            database.Clear();
            DBClass.UncheckAllConstraintsInDB();
            PutDeltaIntoDB();
            DBClass.CheckAllConstraintsInDB();
        }

        /// <summary>
        /// Making the database backup into a local JSON text file (Derpecated since you can't serialize Reflection properties)
        /// </summary>
        /// <param name="where">
        /// Full name of a folder to write database file in
        /// </param>
        private void MakeDBBackupJson(string where) => File.WriteAllText(where, JsonSerializer.Serialize(database));

        /// <summary>
        /// Making the database backup into a local JSON text file ...\data\dbJsonBackup.json  (Derpecated since you can't serialize Reflection properties)
        /// </summary>
        private void MakeDBBackupJsonToStandardLocalFolder() => MakeDBBackupJson(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + DBClass.dbHeadFolder + "dbJsonBackup.json");

        /// <summary>
        /// Restoring the database from local JSON text file backup from the stated folder
        /// </summary>
        /// <param name="fromJsonAdress">
        /// Full name of a folder to read database from
        /// </param>
        private void RestoreBackupIntoDBJson(string fromJsonAdress)
        {
            string jsonContents = File.ReadAllText(fromJsonAdress);
            Database backup = new Database(false);
            backup = JsonConvert.DeserializeObject<Database>(jsonContents);
            JObject backupObj = (JObject)JsonConvert.DeserializeObject(jsonContents);
            backupObj.Properties().Select(p => p.Name).ToList().ForEach(name => {
                if (typeof(Database).GetProperty(name) == null)
                    throw new Exception("Database does not contain properties that are in the backup");
            });
            foreach (var property in typeof(Database).GetProperties().ToList()) 
                if (!backupObj.Properties().Select(p => p.Name).ToList().Contains(property.Name))
                    throw new Exception("Backups database does not contain properties that are in database now");
            DBClass.ClearAllTables();
            SeedDatabaseToMSQLServer(database);
            databaseDelta.Add(backup);
            PutDeltaIntoDB();
        }

        /// <summary>
        /// Restoring the database from the standard local JSON text file backup ...\data\dbJsonBackup.json
        /// </summary>
        private void RestoreBackupIntoDBFromStandardFolderJson() 
            => RestoreBackupIntoDBJson(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + DBClass.dbHeadFolder + "dbJsonBackup.json");

        /// <summary>
        /// Creating tables in database matching the sandbox
        /// </summary>
        /// <param name="database">
        /// Database sandbox for the seed function to get tables and their column information from
        /// </param>
        public static void SeedDatabaseToMSQLServer(Database database)
        {
            foreach (var property in typeof(Database).GetProperties().ToList())
                if (property.GetValue(database) is ITable)
                {
                    ITable table = (ITable)property.GetValue(database);
                    List<string> columnTypesModified = new List<string>();
                    for (int i = 0; i < table.ColumnTypes.Length; i++) {
                        if (table.PrimaryKeys[i])
                            columnTypesModified.Add(table.ColumnTypes[i] + (table.ColumnTypes[i].ToLower()=="int"? " NOT NULL PRIMARY KEY IDENTITY(1,1)" : " NOT NULL PRIMARY KEY") );
                        else
                            columnTypesModified.Add(table.ColumnTypes[i]);
                    }
                    DBClass.CreateNewTable(table.Name, table.ColumnNames.ToList(), columnTypesModified, table.ForeignKeys.ToList(), DBClass.junctionTablesRequired);
                }
        }

        /// <summary>
        /// Initiation of a database to data sandbox data fetch for all tables
        /// </summary>
        private void FetchAllTablesFromSQL()
        {
            database.Clear(); databaseDelta.Clear();
            foreach (string tableName in DBClass.GetTableNames())
            {
                Trace.WriteLine(tableName + ": " + DBClass.GetDataTableByName(tableName).Rows.Count);
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
            foreach (var property in typeof(Database).GetProperties().ToList())
                if (property.GetValue(database) is ITable)
                    if (tableName.ToLower() == ((ITable)property.GetValue(database)).Name.ToLower())
                    {
                        Table<object> table = ((ITable)property.GetValue(database)).CastAbstract();
                        Type tableItemType = property.PropertyType.GetGenericArguments()[0];
                        Table<object> endTable = FetchDataOfType(tableName, table, tableItemType);
                        MethodInfo methodInfo = property.PropertyType.GetMethod("TryCast", BindingFlags.Public | BindingFlags.Static);
                        var finalTable = methodInfo.Invoke(null, new object[] { endTable });
                        property.SetValue(database, finalTable);
                        (property.GetValue(database) as ITable).InsertAbstract(endTable);
                    }
            if (tableName.Contains(DBClass.junctionTableEnding))
            {
                Table<Junction> newJunctionTable = new Table<Junction>(tableName);
                FetchDataOfType<Junction>(tableName, newJunctionTable, typeof(Junction));
                database.junctionTables.Add(newJunctionTable);
            }
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
        private static Table<T> FetchDataOfType<T>(string tableName, Table<T> whereDataIsGoing, Type realType)
        {
            List<string> columnNames = DBClass.GetColumnNames(tableName),
                         columnTypes = DBClass.GetColumnTypes(tableName);
            bool[] primaryKeyColumns = new bool[columnNames.Count];
            string[] foreignKeyColumns = new string[columnNames.Count]; for (int i = 0; i < foreignKeyColumns.Length; i++) foreignKeyColumns[i] = "";
            foreach (string primaryKey in DBClass.GetPrimaryKeys(tableName))
                primaryKeyColumns[columnNames.IndexOf(primaryKey)] = true;
            List<string[]> foreignKeys = DBClass.GetForeignKeys(tableName);
            for (int i = 0; i < foreignKeys.Count; i++)
                foreignKeyColumns[columnNames.IndexOf(foreignKeys[i][0])] = foreignKeys[i][1];
            whereDataIsGoing = new Table<T>(tableName, columnNames.ToArray(), columnTypes.ToArray(), primaryKeyColumns, foreignKeyColumns);

            DataTable table = DBClass.GetDataTableByName(tableName);
            foreach (DataRow row in table.Rows)
            {
                List<string> rowData = new List<string>();
                foreach (object? item in row.ItemArray)
                {
                    if (item.GetType() == typeof(Byte[]))
                        rowData.Add(BitConverter.ToBoolean((Byte[])item).ToString());
                    else
                        rowData.Add(item?.ToString());
                }
                Data d = (Data)(T)Activator.CreateInstance(realType);
                whereDataIsGoing.Insert((T)d.ToData(rowData));
            }
            return whereDataIsGoing;
        }
        /// <summary>
        /// Puts all newly created and added to databaseDelta values into the real database
        /// </summary>
        public void PutDeltaIntoDB()
        {
            database.Add(databaseDelta);
            bool perseveranceFlag = true;
            Queue<PropertyInfo> commitQueue = new Queue<PropertyInfo>();
            typeof(Database).GetProperties().ToList().ForEach(p => commitQueue.Enqueue(p));
            while (perseveranceFlag|| commitQueue.Count > 0)
            {
                perseveranceFlag = false;
                PropertyInfo property = commitQueue.Dequeue();
                Trace.WriteLine(commitQueue.Count);
                if (property.GetValue(database) is ITable)
                {
                    List<Data> abstractRows = ((ITable)property.GetValue(databaseDelta)).SelectAllAbstract().Cast<Data>().ToList();
                    abstractRows.ForEach(row => Trace.WriteLine(string.Join("", row.ToList())));
                    if (DBClass.TryDBInsertMultiple(((ITable)property.GetValue(database)).Name, abstractRows))
                    {
                        if (DBClass.junctionTablesRequired)
                            foreach (Data data1 in abstractRows)
                                foreach (Data data2 in ((ITable)property.GetValue(database)).SelectAllAbstract().Cast<Data>().ToList())
                                    if (!data1.GetType().Equals(data2.GetType()))
                                        for (int i = 0; i < database.junctionTables.Count; i++)
                                            database.junctionTables[i].Insert(new Junction() { firstTableId = data1.Id, secondTableId = data2.Id });
                    }
                    else
                    {
                        commitQueue.Enqueue(property);
                        perseveranceFlag = true;
                    }
                }
                else perseveranceFlag = true;
            }
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
            databaseDelta.libraryData.Insert(GenerateLibraries(libs));
            databaseDelta.userData.Insert(GenerateUsers(users));
            databaseDelta.playerData.Insert(GeneratePlayers(players));
            databaseDelta.archiveData.Insert(GenerateArchives(archives));
            databaseDelta.serverData.Insert(GenerateServers(servers));
            databaseDelta.sessionData.Insert(GenerateSessions(sessions));
            databaseDelta.lobbyData.Insert(GenerateLobbies(lobbies));
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
        public List<int> GetRandomLibraryIds(int n) => DataGenerator.GetRandomIds(database.libraryData.Rows.Cast<Data>().ToList(), n);
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
                library.Id = library.Id + i;
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
                newId = database.libraryData.Count == 0 ? 1 : DBClass.GetNextId(database.libraryData.Name) + 1;
            else
                newId = databaseDelta.libraryData.Rows.Max(l => l.Id) + 1;
            newLibrary.Id = newId;
            newLibrary.creationDate = DataGenerator.GenerateDatetime();
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
        /// Selection of a specified number of random users ids 
        /// </summary>
        /// <param name="n">
        /// Number of ids to select
        /// </param>
        /// <returns>
        /// List of users ids
        /// </returns>
        public List<int> GetRandomUserIds(int n) => DataGenerator.GetRandomIds(database.userData.Rows.Cast<Data>().ToList(), n);
        /// <summary>
        /// Generation of several sandbox users tuple structures
        /// </summary>
        /// <param name="amount">
        /// Number of sandbox users tuples to generate
        /// </param>
        /// <returns>
        /// List of users tuple structures
        /// </returns>
        public List<UserData> GenerateUsers(int amount)
        {
            List<UserData> users = new List<UserData>();
            for (int i = 0; i < amount; i++) users.Add(GenerateUser(DataGenerator.RandomMaster<LibraryData>(database.libraryData.Rows, databaseDelta.libraryData.Rows).Id));
            return users;
        }
        /// <summary>
        /// Generation of a new sandbox users tuple structure
        /// </summary>
        /// <param name="libraryID">
        /// This users's foreign key master library's id in database's hierarchy  
        /// </param>
        /// <returns>
        /// New sandbox users tuple structure
        /// </returns>
        private UserData GenerateUser(int libraryID)
        {
            UserData newUser = new UserData();
            List<LibraryData> tb = new List<LibraryData>();
            tb.AddRange(database.libraryData.Rows); tb.AddRange(databaseDelta.libraryData.Rows);
            LibraryData master = tb.Find(library => library.Id == libraryID);
            int newId;
            if (databaseDelta.userData.Count == 0)
                newId = database.userData.Count == 0 ? 1 : DBClass.GetNextId(database.userData.Name) + 1;
            else
                newId = databaseDelta.userData.Rows.Max(l => l.Id) + 1;
            newUser.Id = newId;
            newUser.name = DataGenerator.GenerateUsername(newId);
            newUser.userIP = DataGenerator.GenerateUserIP();
            newUser.technicalSpecifications = DataGenerator.GenerateTechnicalSpecifications();
            UserInfo uInfo = DataGenerator.GenerateUserInfo();
            uInfo.registrationDateTime = DataGenerator.GenerateDatetimeAfter(database.libraryData.Rows.Find(library => library.Id == libraryID).creationDate);
            newUser.userInfo = uInfo;
            newUser.userStatus = DataGenerator.GenerateUserStatus();
            newUser.libraryID = libraryID;
            master.usersInfo.userIDs.Add(newUser.Id);
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
        public List<int> GetRandomPlayerIds(int n) => DataGenerator.GetRandomIds(database.playerData.Rows.Cast<Data>().ToList(), n);
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
            for (int i = 0; i < amount; i++) 
                players.Add(GeneratePlayer(DataGenerator.RandomMaster<UserData>(database.userData.Rows, databaseDelta.userData.Rows).Id));
            return players;
        }
        /// <summary>
        /// Generation of a new sandbox player tuple structure
        /// </summary>
        /// <param name="userID">
        /// This player's foreign key master users's id in database's hierarchy  
        /// </param>
        /// <returns>
        /// New sandbox player tuple structure
        /// </returns>
        private PlayerData GeneratePlayer(int userID)
        {
            PlayerData newPlayer = new PlayerData();
            int newId;
            if (databaseDelta.playerData.Count == 0)
                newId = database.playerData.Count == 0 ? 1 : DBClass.GetNextId(database.playerData.Name) + 1;
            else
                newId = databaseDelta.playerData.Rows.Max(l => l.Id) + 1;
            newPlayer.Id = newId;
            newPlayer.nickname = DataGenerator.GeneratePlayerNickname(newId);
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
        public List<int> GetRandomArchiveIds(int n) => DataGenerator.GetRandomIds(database.archiveData.Rows.Cast<Data>().ToList(), n);
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
            for (int i = 0; i < amount; i++) archives.Add(GenerateArchive(DataGenerator.RandomMaster<LibraryData>(database.libraryData.Rows, databaseDelta.libraryData.Rows).Id));
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
            tb.AddRange(database.libraryData.Rows); tb.AddRange(databaseDelta.libraryData.Rows);
            LibraryData master = tb.Find(library => library.Id == libraryID);
            int newId;
            if (databaseDelta.archiveData.Count == 0)
                newId = database.archiveData.Count == 0 ? 1 : DBClass.GetNextId(database.archiveData.Name) + 1;
            else
                newId = databaseDelta.archiveData.Rows.Max(l => l.Id) + 1;

            newArchive.Id = newId;
            newArchive.libraryID = libraryID;
            newArchive.initializationDateTime = DataGenerator.GenerateDatetimeAfter(master.creationDate);
            newArchive.suspensionDateTime = DataGenerator.GenerateDatetimeAfter(newArchive.initializationDateTime);
            newArchive.serverIDs = new List<int>();
            newArchive.volume = newArchive.serverIDs.Count;
            newArchive.region = DataGenerator.GenerateRegion();
            master.archivesInfo.archiveIDs.Add(newArchive.Id);
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
        public List<int> GetRandomServerIds(int n) => DataGenerator.GetRandomIds(database.serverData.Rows.Cast<Data>().ToList(), n);
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
            for (int i = 0; i < amount; i++) servers.Add(GenerateServer(DataGenerator.RandomMaster<ArchiveData>(database.archiveData.Rows, databaseDelta.archiveData.Rows).Id));
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
            if (database.archiveData.Rows.Any(archive => archive.Id == archiveID))
            {
                masterHolder = database.archiveData.Rows;
                masterIndex = database.archiveData.Rows.FindIndex(archive => archive.Id == archiveID);
            }
            else
            {
                masterHolder = databaseDelta.archiveData.Rows;
                masterIndex = databaseDelta.archiveData.Rows.FindIndex(archive => archive.Id == archiveID);
            }
            int newId;
            if (databaseDelta.serverData.Count == 0)
                newId = database.serverData.Count == 0 ? 1 : DBClass.GetNextId(database.serverData.Name) + 1;
            else
                newId = databaseDelta.serverData.Rows.Max(l => l.Id) + 1;
            newServer.Id = newId;
            newServer.archiveID = archiveID;
            newServer.location = DataGenerator.GenerateLocation();
            newServer.sessionIDs = new List<int>();
            newServer.serverAvailability = DataGenerator.GenerateServerAvailability();
            newServer.serverCapacity = DataGenerator.GenerateServerCapacity();
            masterHolder[masterIndex].serverIDs.Add(newServer.Id);
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
        public List<int> GetRandomSessionIds(int n) => DataGenerator.GetRandomIds(database.sessionData.Rows.Cast<Data>().ToList(), n);
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
            for (int i = 0; i < amount; i++) 
                sessions.Add(GenerateSession(DataGenerator.RandomMaster<ServerData>(database.serverData.Rows, databaseDelta.serverData.Rows).Id));
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
            tb.AddRange(database.serverData.Rows); tb.AddRange(databaseDelta.serverData.Rows);
            ServerData master = tb.Find(server => server.Id == serverID);
            List<ArchiveData> tb2 = new List<ArchiveData>();
            tb2.AddRange(database.archiveData.Rows); tb2.AddRange(databaseDelta.archiveData.Rows);
            ArchiveData mastersMaster = tb2.Find(archive => master.archiveID == archive.Id);
            if (mastersMaster.Id == 0)
            {
                mastersMaster = tb2[new Random().Next(0, tb2.Count)];
                master.archiveID = mastersMaster.Id;
                mastersMaster.serverIDs.Add(master.Id);
            }
            int newId;
            if (databaseDelta.sessionData.Count == 0)
                newId = database.sessionData.Count == 0 ? 1 : DBClass.GetNextId(database.sessionData.Name) + 1;
            else
                newId = databaseDelta.sessionData.Rows.Max(l => l.Id) + 1;
            newSession.Id = newId;
            newSession.serverID = serverID;
            newSession.startDateTime = DataGenerator.GenerateDatetime(mastersMaster.initializationDateTime, mastersMaster.suspensionDateTime);
            newSession.endDateTime = DataGenerator.GenerateDatetime(newSession.startDateTime, mastersMaster.suspensionDateTime);
            newSession.sessionInfo = DataGenerator.GenerateSessionInfo();
            master.sessionIDs.Add(newSession.Id);
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
        public List<int> GetRandomLobbyIds(int n) => DataGenerator.GetRandomIds(database.lobbyData.Rows.Cast<Data>().ToList(), n);
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
            for (int i = 0; i < amount; i++) lobbies.Add(GenerateLobby(DataGenerator.RandomMaster<SessionData>(database.sessionData.Rows, databaseDelta.sessionData.Rows).Id));
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
            tb.AddRange(database.sessionData.Rows); tb.AddRange(databaseDelta.sessionData.Rows);
            SessionData master = tb.Find(session => session.Id == sessionID);
            int newId;
            if (databaseDelta.lobbyData.Count == 0)
                newId = database.lobbyData.Count == 0 ? 1 : DBClass.GetNextId(database.lobbyData.Name) + 1;
            else
                newId = databaseDelta.lobbyData.Rows.Max(l => l.Id) + 1;
            newLobby.Id = newId;
            newLobby.sessionID = sessionID;
            newLobby.playerIDs = RandomPlayerIds();
            newLobby.numberOfParticipants = newLobby.playerIDs.Count;
            newLobby.creationDate = DataGenerator.GenerateDatetime(master.startDateTime, master.endDateTime);
            master.sessionInfo.participatingLobbies.Add(newLobby.Id);
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
            List<PlayerData> allPlayers = database.playerData.Rows.Concat(databaseDelta.playerData.Rows).ToList();
            return DataGenerator.GetRandomIds(
                            allPlayers.Cast<Data>().ToList(),
                                 Math.Min(DataGenerator.maxPlayersInLobby, DataGenerator.rand.Next(0, allPlayers.Count)));
        }
        #endregion
    }
}
