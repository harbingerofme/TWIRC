using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SQLite;



namespace RNGBot
{
    class DBHandler
    {
        Logger RNGLogger;
        SQLiteConnection dbConnection;
        
        public DBHandler(string dbFilename, Logger newlogger)
        {
            RNGLogger = newlogger;

            //if the database does not exist, or if it is empty, it will be
            //created, along with the following table
            string query = @"
                CREATE TABLE IF NOT EXISTS users (UserName VARCHAR(255), Points INTEGER, PointsUsed INTEGER, PRIMARY KEY (UserName));
                CREATE TABLE IF NOT EXISTS vote (UserName VARCHAR(255), Ballot INTEGER, PRIMARY KEY (UserName));
            ";
            try
            {
                RNGLogger.addLog("DB", 0, "Setting up database");
                dbConnection = new SQLiteConnection(string.Format("Data Source={0}", dbFilename));
                dbConnection.Open();
                SQLiteCommand sqlStartup = new SQLiteCommand(query, dbConnection);
                sqlStartup.ExecuteNonQuery();
            }
            catch (Exception fail)
            {
                RNGLogger.addLog("DB", 1, fail.Message);
            }
 

            addUser("bob");
            addUser("boob");


        }

        public void addUser(string username)
        {
            Dictionary<String,String> data = new Dictionary<string,string>();
            data.Add("UserName", username);
            if (Insert("users", data))
            {
                RNGLogger.addLog("DB", 0, "Added user \"" + username + "\"");
            }
            else
            {
                RNGLogger.addLog("DB", 0, "Add failed, user \"" + username + "\" probably already exists");
            }

        }


        //some of the following code originates from a tutorial no longer present on the internet
        // defunct: http://www.mikeduncan.com/page/3/
        // referenced by http://www.dreamincode.net/forums/topic/157830-using-sqlite-with-c%23/
        //

        public bool Insert(String tableName, Dictionary<String, String> data)
	    {
	        String columns = "";
	        String values = "";
	        Boolean returnCode = true;
	        foreach (KeyValuePair<String, String> val in data)
	        {
	            columns += String.Format(" {0},", val.Key.ToString());
	            values += String.Format(" '{0}',", val.Value);
	        }
	        columns = columns.Substring(0, columns.Length - 1);
	        values = values.Substring(0, values.Length - 1);
	        try
	        {
	           SQLiteCommand thecommand = new SQLiteCommand(string.Format("insert into {0}({1}) values({2});", tableName, columns, values),dbConnection);
               thecommand.ExecuteNonQuery();
	        }
	        catch(Exception fail)
	        {
	            RNGLogger.addLog("DB",0,fail.Message);
	            returnCode = false;
	        }
	        return returnCode;
	    }

        
    }
}
