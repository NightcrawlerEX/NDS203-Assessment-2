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
        /// Constructor
        /// </summary>
        public Database()
        {
            SQLiteConnection connection = new SQLiteConnection(_connectionString);
            string command = "CREATE TABLE IF NOT EXISTS Users (" +
            ");";
        }//end constructor
        
    }//end class
}//end namespace