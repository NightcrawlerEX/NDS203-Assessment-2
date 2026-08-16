/* 
* NDS203 Assessment 3
* Student ID: A00125081
* Student Name: James Simpson
* Repository: https://github.com/NightcrawlerEX/NDS203-Assessment-2
*/

//reference: https://zetcode.com/csharp/sqlite/
using System.Data.SQLite;

//New namespace just because
namespace Ass3
{
    public class Database
    {
        private const string _connectionString = "Data Source=Users.db;Version=3;";

        /// <summary>
        /// Open and create the table
        /// </summary>
        public void CreateTable()
        {
            SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string commandString = "CREATE TABLE IF NOT EXISTS Users (" +
            "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
            "Username TEXT NOT NULL UNIQUE, " +
            "Password TEXT NOT NULL, " +
            "Wins INTEGER NOT NULL DEFAULT 0, " +
            "Losses INTEGER NOT NULL DEFAULT 0, " +
            "Draws INTEGER NOT NULL DEFAULT 0" +
            ");";

            SQLiteCommand command = new SQLiteCommand(commandString, connection);
            command.ExecuteNonQuery();
            command.Dispose();
            connection.Close();
            connection.Dispose();
        }//end constructor
        
    }//end class
}//end namespace