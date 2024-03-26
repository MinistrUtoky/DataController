using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Reflection;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DataGenStatistics.classes
{
    public static class DBClass
    {
        private static string cn_String = "";

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
        public static List<DataTable> GetDataTables()
        {
            List<DataTable> tables = new List<DataTable>();
            using (SqlConnection con = GetDBConnection())
            {
                foreach (DataRow row in con.GetSchema("Tables").Rows)
                    tables.Add(GetDataTable((string)row["TABLE_NAME"]));
                return tables;
            }
        }

        public static SqlConnection GetDBConnection()
        {
            cn_String = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=" + Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName + @"\data\myDB.mdf; Integrated Security=True;"; 
            SqlConnection cn_connection = new SqlConnection(cn_String);
            if (cn_connection.State != ConnectionState.Open) cn_connection.Open();
            return cn_connection;
        }

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

        public static void ExecuteSQL(string SQL_Text)
        {
            using (SqlConnection cn_connection = GetDBConnection())
            {
                SqlCommand cmd_Command = new SqlCommand(SQL_Text, cn_connection);
                cmd_Command.ExecuteNonQuery();
            }
        }

        public static void CloseDBConnection()
        {
            SqlConnection cn_connection = new SqlConnection(cn_String);
            if (cn_connection.State != ConnectionState.Closed) cn_connection.Close();
        }

        public static int GetNextId(string table)
        {
            int id = 0;
            Int32.TryParse(GetDataTable("SELECT IDENT_CURRENT('" + table + "');").Rows[0].ItemArray[0]?.ToString(), out id);
            return id;
        }
        public static List<string> GetColumnNames(string tableName)
        {
            List<string> names = new List<string>();
            DataTable dt = GetDataTable("SELECT  * FROM " + tableName);
            foreach (DataColumn column in dt.Columns) 
                names.Add(column.ColumnName);
            return names;
        }
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
        public static void DBRemove(string tableName, int id) => DBRemove(tableName, "ID=" + id);
        public static void DBRemove(string tableName, string where)
        {
            string sql_Remove = "DELETE FROM " + tableName + " WHERE " + where + ";";
            ExecuteSQL(sql_Remove);
        }
        public static void DBUpdate(string tableName, List<string> columnNames, List<string> newValues, int id)
            => DBUpdate(tableName, columnNames, newValues, "ID=" + id);
        public static void DBUpdate(string tableName, List<string> columnNames, List<string> newValues, string condition)
        {
            if (columnNames.Count != newValues.Count) throw new Exception("Number of columns and new values are not the same");
            StringBuilder values = new StringBuilder();
            for (int i = 1; i < columnNames.Count; i++)
            {
                values.Append(columnNames[i]); values.Append(" = ");
                Console.WriteLine(columnNames[i].ToString());
                Console.WriteLine(newValues[i].ToString());
                values.Append('\'');
                values.Append(newValues[i]);
                values.Append('\'');
                if (i != columnNames.Count-1) values.Append(", ");
            }
            string sql_Update = "UPDATE " + tableName + " SET " + values + " WHERE " + condition;
            ExecuteSQL(sql_Update);
        }

        public static DataTable GetDataTableByName(string tableName) => GetDataTable("SELECT * FROM [" + tableName + "]");
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
            Console.WriteLine(sql_Add_New_Table);
            ExecuteSQL(sql_Add_New_Table);
        }
        
    }
}
