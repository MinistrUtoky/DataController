using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace DataGenStatistics.classes
{
    /// <summary>
    /// The instance that is used to directly manage the database
    /// </summary>
    public static class DBClass
    {
        private static string cn_String = "";
        private static string dbName = "myDB.mdf";

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
        /// Getting connection to database 
        /// </summary>
        /// <returns>
        /// Database connection object
        /// </returns>
        public static SqlConnection GetDBConnection()
        {
            cn_String = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + @"\data\" + dbName + "; Integrated Security=True;";
            SqlConnection cn_connection = new SqlConnection(cn_String);
            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();
            return cn_connection;
        }
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
        /// Execution of anys SQL command without anything in return
        /// </summary>
        /// <param name="SQL_Text">
        /// SQL command text
        /// </param>
        public static void ExecuteSQL(string SQL_Text)
        {
            using (SqlConnection cn_connection = GetDBConnection())
            {
                SqlCommand cmd_Command = new SqlCommand(SQL_Text, cn_connection);
                cmd_Command.ExecuteNonQuery();
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
        public static void DBInsert(string tableName, List<string> new_element)
        {
            StringBuilder values = new StringBuilder();
            for (int i = 1; i < new_element.Count; i++)
            {
                if (new_element[i].ToUpper() == "TRUE" || new_element[i].ToUpper() == "FALSE")
                {
                    values.Append("CAST('"); values.Append(new_element[i]); values.Append("' as bit)");
                }
                else
                {
                    values.Append("'"); values.Append(new_element[i]); values.Append("'");
                }
                if (i != new_element.Count - 1) values.Append(",");
            }
            string sql_Add = "INSERT INTO \"" + tableName + "\" VALUES(" + values + ")";
            ExecuteSQL(sql_Add);
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
        public static void DBInsertMultiple(string tableName, List<Data> data)
        {
            foreach (Data d in data)
                DBInsert(tableName, d.ToList());
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
            string sql_Remove = "DELETE FROM " + tableName + " WHERE " + where + ";";
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
        /// </param>
        /// <param name="foreignKeysOrEmpty">
        /// List of empty strings or string table references matching "[*tableName*].*columnName*"
        /// </param>
        public static void CreateNewTable(string name, List<string> columnNames, List<string> types, List<string> foreignKeysOrEmpty)
        {
            if (GetTableNames().Contains(name)) throw new Exception("Table with the name " + name + " already exists");
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
            for (int i = 0; i < foreignKeysOrEmpty.Count; i++)
            {
            }
            string sql_Add_New_Table = "CREATE TABLE [dbo].[" + name + "] ( " + sb.ToString() + ");";
            ExecuteSQL(sql_Add_New_Table);
        }
        /// <summary>
        /// Truncating the specific table
        /// </summary>
        /// <param name="tableName">
        /// Name of a table to truncate
        /// </param>
        public static void ClearTable(string tableName) => ExecuteSQL("TRUNCATE TABLE [" + tableName +"]");
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
        public static void DropTable(string tableName) => ExecuteSQL("DROP TABLE [" + tableName + "]");
    }
}
