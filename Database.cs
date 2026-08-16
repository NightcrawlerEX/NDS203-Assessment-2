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
        }//end CreateTable

        /// <summary>
        /// CreateUser
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>false if user already exists. True if successful</returns>
        public bool CreateUser(string username, string password)
        {
            SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string commandString =
                "INSERT OR IGNORE INTO Users (Username, Password) " +
                "VALUES (@username, @password);";
            SQLiteCommand command =
                new SQLiteCommand(commandString, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            int rowsAdded = command.ExecuteNonQuery();

            command.Dispose();
            connection.Close();
            connection.Dispose();

            //if rows added is 1 then it was successful. If its 0 then there was 
            //a problem
            if(rowsAdded > 0) return true;
            else return false;
        }//end CreateUser

        /// <summary>
        /// TryLogin
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>false on fail. True on success</returns>
        public bool TryLogin(string username, string password)
        {
            SQLiteConnection connection =
                new SQLiteConnection(_connectionString);

            connection.Open();
            string commandString =
                "SELECT COUNT(*) FROM Users " +
                "WHERE Username = @username " +
                "AND Password = @password;";
            SQLiteCommand command =
                new SQLiteCommand(commandString, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            long numberOfUsers = (long)command.ExecuteScalar();
            command.Dispose();
            connection.Close();
            connection.Dispose();
            if(numberOfUsers > 0) return true;
            else return false;
        }//end TryLogin

        public string GetScores()
        {
            SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();
            string commandString =
                "SELECT Username, Wins, Losses, Draws " +
                "FROM Users " +
                "ORDER BY Wins DESC;";

            SQLiteCommand command =
                new SQLiteCommand(commandString, connection);
            SQLiteDataReader reader = command.ExecuteReader();

            string scoreString = "Scores: \n";

            while (reader.Read())
            {
                scoreString += reader["Username"].ToString() + ", ";
                scoreString += "W: " + reader["Wins"].ToString() + ", ";
                scoreString += "L: " + reader["Losses"].ToString() + ", ";
                scoreString += "D: " + reader["Draws"].ToString() + ", ";
            }//end while

            reader.Close();
            reader.Dispose();
            command.Dispose();
            connection.Close();
            connection.Dispose();

            return scoreString;
        }//end GetScores

        /// <summary>
        /// Adds one win to the specified user.
        /// </summary>
        /// <param name="username"></param>
        public void RecordWin(string username)
        {
            SQLiteConnection connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string commandString =
                "UPDATE Users " +
                "SET Wins = Wins + 1 " +
                "WHERE Username = @username;";

            SQLiteCommand command = new SQLiteCommand(commandString, connection);

            command.Parameters.AddWithValue("@username", username);
            command.ExecuteNonQuery();

            command.Dispose();
            connection.Close();
            connection.Dispose();
        }//end RecordWin
        
    }//end class
}//end namespace