using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;

///<summary>
/// This document is made for universal relational database interaction
/// </summary>
namespace DataGenStatistics.classes
{
    /// <summary>
    /// The instance that is used to directly manage the database
    /// </summary>
    public static class DBClass
    {
        public static string dbHeadFolder = "\\data\\";
        public static string dbName = "myDB.mdf";
        public static string cn_String = "";
        public static string junctionTableEnding = "JunctionTable";
        public const bool junctionTablesRequired = false;

        /// <summary>
        /// Extracting table names from db
        /// </summary>
        /// <returns>
        /// List of table name strings 
        /// </returns>
        public static List<string> GetTableNames()
        {
            List<string> tableNames = new List<string>();
            using (SqlConnection con = GetDBConnection())
            {
                foreach (DataRow row in con.GetSchema("Tables").Rows)
                    tableNames.Add((string)row["TABLE_NAME"]);
                return tableNames;
            }
        }
        /// <summary>
        /// Extracting table objects from db
        /// </summary>
        /// <returns>
        /// List of DataTable objects
        /// </returns>
        public static List<DataTable> GetDataTables()
        {
            List<DataTable> tables = new List<DataTable>();
            using (SqlConnection con = GetDBConnection())
            {
                foreach (DataRow row in con.GetSchema("Tables").Rows)
                    tables.Add(GetDataTable("SELECT  * FROM [" + (string)row["TABLE_NAME"] + "]"));
                return tables;
            }
        }

        /// <summary>
        /// Getting connection to database by it's full filename 
        /// </summary>
        /// <param name="fullDBFilename">
        /// Full name of the database, including path, name of the file and it's extension
        /// </param>
        /// <returns>
        /// Database connection object
        /// </returns>
        public static SqlConnection GetDBConnection(string fullDBFilename)
        {
            cn_String = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + fullDBFilename + ";Integrated security=true;";
            SqlConnection cn_connection = new SqlConnection(cn_String);

            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();
            //Trace.WriteLine(cn_String);
            return cn_connection;
        }
        /// <summary>
        /// Getting connection to globally stated database database relative to the project folder 
        /// </summary>
        /// <returns>
        /// Database connection object
        /// </returns>
        public static SqlConnection GetDBConnection() => GetDBConnection(Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + dbHeadFolder + dbName);
        /// <summary>
        /// Getting data table with SELECT statement
        /// </summary>
        /// <param name="SQL_Text">
        /// SELECT statement to get table or table part 
        /// </param>
        /// <returns>
        /// DataTable object retrieved by SELECT statement
        /// </returns>
        public static DataTable GetDataTable(string SQL_Text)
        {
            using (SqlConnection cn_connection = GetDBConnection())
            {
                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(SQL_Text, cn_connection);
                adapter.Fill(table);
                return table;
            }
        }

        /// <summary>
        /// Execution of any SQL command using already established connection with a database
        /// </summary>
        /// <param name="SQL_Text">
        /// SQL command text
        /// </param>
        public static void ExecuteSQL(string SQL_Text)
        {
            using (SqlConnection cn_connection = GetDBConnection())
            {
                //Trace.WriteLine(SQL_Text);
                SqlCommand cmd_Command = new SqlCommand(SQL_Text, cn_connection);
                try
                {
                    cmd_Command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Trace.WriteLine(SQL_Text);
                    foreach (var error in ex.Errors)
                        Trace.WriteLine(error);
                    throw ex;
                }
            }
        }
        /// <summary>
        /// Forceful disruption of the database connection
        /// </summary>
        public static void CloseDBConnection()
        {
            SqlConnection cn_connection = new SqlConnection(cn_String);
            if (cn_connection.State != ConnectionState.Closed) cn_connection.Close();
        }
        /// <summary>
        /// Getting the next increment of the table's id 
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to get the next id from
        /// </param>
        /// <returns>
        /// Next id for the exact table
        /// </returns>
        public static int GetNextId(string tableName)
        {
            int id = 0;
            Int32.TryParse(GetDataTable("SELECT IDENT_CURRENT('" + tableName + "');").Rows[0].ItemArray[0]?.ToString(), out id);
            return id;
        }
        /// <summary>
        /// Retrieving names of the table's columns
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to get column names from
        /// </param>
        /// <returns>
        /// List of column name strings
        /// </returns>
        public static List<string> GetColumnNames(string tableName)
        {
            List<string> names = new List<string>();
            DataTable dt = GetDataTable("SELECT  * FROM [" + tableName + "]");
            foreach (DataColumn column in dt.Columns)
                names.Add(column.ColumnName);
            return names;
        }
        /// <summary>
        /// Insertion of an element to the table with specified content
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to insert into
        /// </param>
        /// <param name="new_element">
        /// List of insertion element values fitting the mask of all non-primary colimns
        /// </param>
        public static bool TryDBInsert(string tableName, Data new_element) 
        { 
        
            try{
                StringBuilder values = new StringBuilder();
                List<string> new_element_list = new_element.ToList();
                for (int i = 1; i < new_element.ToList().Count; i++)
                {
                    if (new_element_list[i].ToUpper() == "TRUE" || new_element_list[i].ToUpper() == "FALSE")
                    {
                        values.Append("CAST('"); values.Append(new_element_list[i]); values.Append("' as bit)");
                    }
                    else
                    {
                        values.Append("'"); values.Append(new_element_list[i]); values.Append("'");
                    }
                    if (i != new_element_list.Count - 1) values.Append(",");
                }
                string sql_Add = "INSERT INTO [" + tableName + "] VALUES(" + values + ")";
                //Trace.WriteLine(sql_Add);
                ExecuteSQL(sql_Add);
                if (junctionTablesRequired)
                    GetTableNames().ForEach(name => DBInsertIntoJunction(name, tableName, new_element.Id.ToString()));
                return true;
            }
            catch (SqlException ex) { return false; }
        }

        /// <summary>
        /// Insertion of an element to the table with content specified by select statement
        /// Example: INSERT INTO Table1(column11,column12,column13) SELECT column21, column22, column23 FROM Table2;
        /// </summary>
        /// <param name="name">
        /// Name of the table to insert into
        /// </param>
        /// <param name="columnNames">
        /// Names of the columns to insert into (the rest is default)
        /// </param>
        /// <param name="afterStatement">
        /// "SELECT ..." fitting the columns of the table the insertion is happening for or any other afterstatement like "DEFAULT VALUES" or "VALUES(...)"
        /// </param>
        public static void DBInsertSelect(string name, List<string> columnNames, string afterStatement)
        {
            string sql_Add;
            if (columnNames.Count > 0)
            {
                StringBuilder columnNamesThroughComma = new StringBuilder();
                columnNamesThroughComma.Append(columnNames[0]);
                for (int i = 1; i < columnNames.Count; i++)
                {
                    columnNamesThroughComma.Append(",");
                    columnNamesThroughComma.Append(columnNames[i]);
                }
                sql_Add = "INSERT INTO [" + name + "](" + columnNamesThroughComma + ") " + afterStatement + ";";
            }
            else
                sql_Add = "INSERT INTO [" + name + "] " + afterStatement + ";";
            //Trace.WriteLine(sql_Add);
            ExecuteSQL(sql_Add);
        }

        /// <summary>
        /// Insertion of a new id pair into the junction table
        /// </summary>
        /// <param name="junctionTableName">
        /// Name of the junction table to insert into
        /// </param>
        public static void DBInsertIntoJunction(string junctionTableName, string tableName, string id)
        {
            if (!junctionTableName.Contains(junctionTableEnding) || !junctionTableName.Contains(tableName)) return;
            List<string> tablesInJunction = GetColumnNames(junctionTableName).GetRange(1, 2);
            if (tablesInJunction[0] == tableName)
                ExecuteSQL("INSERT INTO " + junctionTableName +
                           "(" + tablesInJunction[0] + "," + tablesInJunction[1] +
                           ") SELECT " + id + " as someconst, ID FROM " + tablesInJunction[1] + ";");
            else
                ExecuteSQL("INSERT INTO " + junctionTableName +
                           "(" + tablesInJunction[1] + "," + tablesInJunction[0] +
                           ") SELECT " + id + " as someconst, ID FROM " + tablesInJunction[0] + ";");
        }

        /// <summary>
        /// Insertion of multiple elements to the table with set content
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to insert into
        /// </param>
        /// <param name="data">
        /// List of data structures convertable to lists of string insertion contents 
        /// </param>
        public static bool TryDBInsertMultiple(string tableName, List<Data> data)
        {
            Queue<Data> commitQueue = new Queue<Data>(); data.ForEach(row => commitQueue.Enqueue(row));
            bool perceveranceFlag = true; 
            while (commitQueue.Count > 0)
                if (!TryDBInsert(tableName, commitQueue.Dequeue()))
                    perceveranceFlag = false;
            return perceveranceFlag;
        }

        /// <summary>
        /// Removal of a tuple with specified id
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to remove from
        /// </param>
        /// <param name="id">
        /// Id of a tuple to remove
        /// </param>
        public static void DBRemove(string tableName, int id) => DBRemove(tableName, "ID=" + id);
        /// <summary>
        /// Removal of tuples by specified condition
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to remove from
        /// </param>
        /// <param name="where">
        /// Condition for removal
        /// </param>
        public static void DBRemove(string tableName, string where)
        {
            string sql_Remove = "DELETE FROM [" + tableName + "] WHERE " + where + ";";
            ExecuteSQL(sql_Remove);
        }

        /// <summary>
        /// Update of specified columns with certain values of a tuple with set id 
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to update
        /// </param>
        /// <param name="columnNames">
        /// Names of columns to update
        /// </param>
        /// <param name="newValues">
        /// Values to update the specified tuple's columns with
        /// </param>
        /// <param name="id">
        /// Id of a tuple to update
        /// </param>
        public static void DBUpdate(string tableName, List<string> columnNames, List<string> newValues, int id)
            => DBUpdate(tableName, columnNames, newValues, "ID=" + id);

        /// <summary>
        /// Update of specified columns with certain values of tuples by set condition
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to update
        /// </param>
        /// <param name="columnNames">
        /// Names of columns to update
        /// </param>
        /// <param name="newValues">
        /// Values to update the specified tuples' columns with
        /// </param>
        /// <param name="where">
        /// Update condition
        /// </param>
        public static void DBUpdate(string tableName, List<string> columnNames, List<string> newValues, string where)
        {
            if (columnNames.Count != newValues.Count) throw new Exception("Number of columns and new values are not the same");
            StringBuilder values = new StringBuilder();
            for (int i = 1; i < columnNames.Count; i++)
            {
                values.Append(columnNames[i]); values.Append(" = ");
                if (newValues[i].ToUpper() == "TRUE" || newValues[i].ToUpper() == "FALSE")
                {
                    values.Append("CAST('"); values.Append(newValues[i]); values.Append("' as bit)");
                }
                else
                {
                    values.Append("'"); values.Append(newValues[i]); values.Append("'");
                }
                if (i != columnNames.Count - 1) values.Append(", ");
            }
            string sql_Update = "UPDATE [" + tableName + "] SET " + values + " WHERE " + where;
            ExecuteSQL(sql_Update);
        }

        /// <summary>
        /// Update of multiple identified tuples with specified data.
        /// Number and sequence of ids must be exactly preset to the number and sequence of data.  
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to update
        /// </param>
        /// <param name="newValues">
        /// List of structures convertable to values to update the identified tuples' columns with
        /// </param>
        /// <param name="ids">
        /// Identifiers of the tuples to update 
        /// </param>
        public static void DBUpdateMultiple(string tableName, List<Data> newValues, List<int> ids)
        {
            if (newValues.Count != ids.Count) throw new Exception("Number of ids doesn't match number of value rows");
            if (newValues.Count > 0)
            {
                int valuesCount = newValues[0].ToList().Count;
                for (int i = 0; i < newValues.Count; i++)
                {
                    List<string> columnNames = GetColumnNames(tableName).GetRange(1, valuesCount - 1);
                    DBUpdate(tableName, columnNames, newValues[i].ToList().GetRange(1, valuesCount - 1), ids[i]);
                }
            }
        }

        /// <summary>
        /// Getting the whole table by it's name
        /// </summary>
        /// <param name="tableName">
        /// Name of the table to get
        /// </param>
        /// <returns>
        /// DataTable object that mirrors the specified table
        /// </returns>
        public static DataTable GetDataTableByName(string tableName) => GetDataTable("SELECT * FROM [" + tableName + "]");

        /// <summary>
        /// Creating a new table in the database
        /// </summary>
        /// <param name="name">
        /// Name of a new table
        /// </param>
        /// <param name="columnNames">
        /// Names for the new table's columns
        /// </param>
        /// <param name="types">
        /// Types of the new table's columns 
        /// (types are assigned in the same sequence as column names)
        /// (You can even choose the modified type such as primary key, identity, auto-increment etc.)
        /// </param>
        /// <param name="foreignKeysOrEmpty">
        /// List of empty strings or string table references matching "[*tableName*].*columnName*"
        /// </param>
        public static void CreateNewTable(string name, List<string> columnNames, List<string> types, List<string> foreignKeysOrEmpty, bool junctionTablesRequired)
        {
            if (GetTableNames().Contains(name.ToLower()) || GetTableNames().Contains(name.ToUpper())) { Trace.WriteLine("Table with the name " + name + " already exists"); return; }
            if (columnNames.Count != types.Count
                || columnNames.Count != foreignKeysOrEmpty.Count) throw new Exception("Numbers of columns, types and keys are not synchronized");
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < columnNames.Count; i++)
            {
                sb.Append(columnNames[i]);
                sb.Append(" ");
                if (foreignKeysOrEmpty[i] != "")
                    sb.Append(types[i] + " FOREIGN KEY REFERENCES " + foreignKeysOrEmpty[i]);
                else
                    sb.Append(types[i]);

                if (i != columnNames.Count - 1)
                    sb.Append(", ");
            }
            string sql_Add_New_Table = "CREATE TABLE [dbo].[" + name + "] ( " + sb.ToString() + ");";
            sql_Add_New_Table = string.Join("VARCHAR(MAX)", string.Join("VARCHAR", sql_Add_New_Table.ToUpper().Split("VARCHAR(MAX)")).Split("VARCHAR"));
            sql_Add_New_Table = string.Join("VARCHAR(", sql_Add_New_Table.ToUpper().Split("VARCHAR(MAX)("));
            ExecuteSQL(sql_Add_New_Table);
            if (junctionTablesRequired)
            {
                GetTableNames().ForEach(tableName =>
                {
                    CreateManyToManyConnectionTable(name, tableName);
                });
            }
        }

        /// <summary>
        /// Creating a junction table for two other tables in the database
        /// </summary>
        /// <param name="name">
        /// Name of a new junction table
        /// </param>
        /// <param name="tableName1">
        /// Name of the first table in the junction
        /// </param>
        /// <param name="tableName2">
        /// Name of the second table in the junction
        /// </param>
        public static void CreateManyToManyConnectionTable(string tableName1, string tableName2)
        {
            if (tableName1.ToLower().Equals(tableName2.ToLower()) || (tableName1 + tableName2).ToLower().Contains(junctionTableEnding.ToLower())) return;
            CreateNewTable(tableName1 + tableName2 + junctionTableEnding, new List<string>() { "ID", tableName1, tableName2 },
                                 new List<string>() { "INT NOT NULL PRIMARY KEY IDENTITY(1,1)", "INT NOT NULL", "INT NOT NULL" },
                                 new List<string>() { "", tableName1 + "(ID)", tableName2 + "(ID)" }, false);

            ExecuteSQL("CREATE TRIGGER dbo." + tableName1 + tableName2 + junctionTableEnding + "InsertionTrigger ON " + tableName1
                       + " AFTER INSERT AS BEGIN " +
                       "INSERT INTO " + tableName1 + tableName2 + junctionTableEnding + "(" + tableName1 + "," + tableName2 + ") " +
                       "SELECT i.id, t3.ID FROM inserted i CROSS JOIN " + tableName2 + " t3; " +
                       "END");
            ExecuteSQL("CREATE TRIGGER dbo." + tableName1 + tableName2 + junctionTableEnding + "DeletionTrigger1 ON "
                + tableName1 + " FOR DELETE AS DELETE FROM " + tableName1 + tableName2 + junctionTableEnding
                + " WHERE " + tableName1 + " IN(SELECT deleted.id FROM deleted)");
            ExecuteSQL("CREATE TRIGGER dbo." + tableName1 + tableName2 + junctionTableEnding + "DeletionTrigger2 ON "
                + tableName2 + " FOR DELETE AS DELETE FROM " + tableName1 + tableName2 + junctionTableEnding
                + " WHERE " + tableName2 + " IN(SELECT deleted.id FROM deleted)");
        }

        /// <summary>
        /// Truncating the specific table
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to truncate
        /// </param>
        public static void ClearTable(string tableName)
        {
            UncheckAllConstraintsInDB();
            ExecuteSQL("DELETE FROM [" + tableName + "];");
            CheckAllConstraintsInDB();
        }

        /// <summary>
        /// Truncating all the tables in database
        /// </summary>
        public static void ClearAllTables() => GetTableNames().ForEach(tableName => ClearTable(tableName));

        /// <summary>
        /// Deleting the specific table
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to drop
        /// </param>
        public static void DropTable(string tableName)
        {
            ExecuteSQL("DROP TABLE [dbo].[" + tableName + "]");
        }

        /// <summary>
        /// Deleting all tables in the database
        /// </summary>
        public static void DropAll()
        {
            ExecuteSQL("exec sp_MSforeachtable \"declare @name nvarchar(max); " +
                           "set @name = parsename('?', 1); " +
                           "exec sp_MSdropconstraints @name\"; " +
                           "exec sp_MSforeachtable \"drop table ?\";");
            GetTableNames().ForEach(tableName =>
            {
                DropTable(tableName);
            });
            if (junctionTablesRequired)
                foreach (DataRow trigger in GetDataTable("SELECT o.name AS trigger_name FROM sysobjects as o " +
                                                        "INNER JOIN sysobjects AS o2 ON o.parent_obj = o2.id " +
                                                        "INNER JOIN sysusers AS s ON o2.uid = s.uid " +
                                                        "WHERE o.type = 'TR';").Rows)
                {
                    ExecuteSQL("DROP TRIGGER " + trigger.ItemArray[0]);
                }
        }

        /// <summary>
        /// Truncates the specific table and fills it with new data
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to replace data in
        /// </param>
        /// <param name="newData">
        /// Data to replace table's contents with
        /// </param>
        public static void ReplaceAllDataInTableWithNew(string tableName, List<Data> newData)
        {
            ClearTable(tableName);
            TryDBInsertMultiple(tableName, newData);
        }

        /// <summary>
        /// Unchecks all constrains for a table allowing DML commands without restriction
        /// </summary>
        public static void UncheckAllConstraintsInDB() => GetTableNames().ForEach(name => { ExecuteSQL("ALTER TABLE [" + name + "] NOCHECK CONSTRAINT ALL;"); });

        /// <summary>
        /// Checks all constrains on for a table back again
        /// </summary>
        public static void CheckAllConstraintsInDB() => GetTableNames().ForEach(name => { ExecuteSQL("ALTER TABLE [" + name + "] CHECK CONSTRAINT ALL;"); });

        /// <summary>
        /// Gets all primary key column names from the table
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to get primary key column names from
        /// </param>
        public static List<string> GetPrimaryKeys(string tableName)
        {
            List<string> columnNames = new List<string>();

            DataRowCollection rows = GetDataTable("SELECT C.COLUMN_NAME " +
                                    "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T " +
                                    "JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE C " +
                                    "ON C.CONSTRAINT_NAME=T.CONSTRAINT_NAME " +
                                    "WHERE C.TABLE_NAME='" + tableName + "' " +
                                    "AND T.CONSTRAINT_TYPE='PRIMARY KEY'").Rows;
            if (rows.Count > 0)
                foreach (object? item in rows[0].ItemArray)
                    columnNames.Add(item.ToString());
            return columnNames;
        }

        /// <summary>
        /// Gets all foreign key column names from the table
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to get foreign key column names from
        /// </param>
        public static List<string[]> GetForeignKeys(string tableName)
        {
            List<string[]> referenceNames = new List<string[]>();
            DataRowCollection rows = GetDataTable("SELECT DISTINCT COL_NAME(fc.parent_object_id, fc.parent_column_id) col, OBJECT_NAME(f.referenced_object_id) TableName, COL_NAME(fc.referenced_object_id, fc.referenced_column_id) anotherCol " +
                                                "FROM sys.foreign_keys AS f " +
                                                "INNER JOIN sys.foreign_key_columns AS fc " +
                                                "ON f.OBJECT_ID = fc.constraint_object_id " +
                                                "INNER JOIN sys.tables t " +
                                                "ON t.OBJECT_ID = fc.referenced_object_id " +
                                                "WHERE OBJECT_NAME (f.parent_object_id) = '" + tableName + "'").Rows;
            /*GetDataTable("SELECT C.COLUMN_NAME " +
              "FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS T " +
              "JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE C " +
              "ON C.CONSTRAINT_NAME=T.CONSTRAINT_NAME " +
              "WHERE C.TABLE_NAME='" + tableName + "' " +
              "AND T.CONSTRAINT_TYPE='FOREIGN KEY'").Rows; - alternative without straight up reference for primary key*/
            if (rows.Count > 0)
                foreach (DataRow row in rows)
                    referenceNames.Add(new string[2] { row.ItemArray[0].ToString(), row.ItemArray[1] + "(" + row.ItemArray[2] + ")" });
            return referenceNames;
        }

        /// <summary>
        /// Gets all foreign key column names from the table
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to get foreign key column names from
        /// </param>
        public static List<string> GetColumnTypes(string tableName)
        {
            List<string> columnTypes = new List<string>();
            DataRowCollection rows = GetDataTable("SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tableName + "'").Rows;
            if (rows.Count > 0)
                foreach (DataRow row in rows)
                {
                    columnTypes.Add(row.ItemArray[0].ToString());
                }
            return columnTypes;
        }

        /// <summary>
        /// Creates the new database on the same server the main database is using
        /// </summary>
        /// <param name="folderAdress"></param>
        /// <param name="name"></param>
        public static void CreateNewDatabase(string folderAdress, string name) => ExecuteSQL(string.Format("CREATE DATABASE [{0}] ON PRIMARY " +
                                        "(NAME={0}_data, FILENAME = '{1}{0}.mdf') " +
                                        "LOG ON (NAME={0}_log, FILENAME = '{1}{0}_log.ldf')", name, folderAdress + dbHeadFolder));

        ///<summary>
        /// Deletes the database going by stated name from the server used by main database
        /// </summary>
        /// <param name="name">
        /// Name of the database dropped
        /// </param>
        public static void DropDatabase(string name)
        {
            if (GetDataTable("SELECT name FROM master.sys.databases WHERE name = N'" + name + "'").Rows.Count != 0)
            {
                ExecuteSQL("EXEC master.dbo.sp_detach_db '" + name + "', 'true'");
                //ExecuteSQL("DROP DATABASE [" + name + "]");
            }
        }
    }
}
