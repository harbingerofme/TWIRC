using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace Run_me_once
{
    class Program
    {
        SQLiteConnection dbConn;
        
        static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
            dbConn.Open();
            new SQLiteCommand("CREATE TABLE IF NOT EXISTS newsettings (variable VARCHAR(128), type VARCHAR(64), value VARCHAR(128));", dbConn).ExecuteNonQuery();
            insertIntoSettings("name", "string", "rngppbot");
            insertIntoSettings("channel", "string", "#harbbot");
            insertIntoSettings("antispam", "bit", "0");
            insertIntoSettings("silence", "bit", "1");
            insertIntoSettings("oauth", "string", "oauth:67h2n5dny6xf2ho6j7oj3xugu7uurd");
            insertIntoSettings("cooldown", "int", "20");
            insertIntoSettings("loglevel", "int", "2");
            insertIntoSettings("logpath", "string", @"C:\Users\Zack\Dropbox\Public\rnglog.txt");
            insertIntoSettings("timebetweenvote", "calc", "15*60");
            insertIntoSettings("timetovote", "calc", "4*60");
            insertIntoSettings("defaultbias", "string", "1.00:1.00:1.00:1.00:0.96:0.92:0.82");
            insertIntoSettings("biasmaxdiff", "double", "0.05");
            insertIntoSettings("moneypervote", "int", "50");
            insertIntoSettings("moneyconversionrate", "double", "0.5");
            insertIntoSettings("expallfunction", "string", "8X");
            insertIntoSettings("welcomemessagecd", "int", "60");
            insertIntoSettings("backgroundspath", "string", @"C:\Users\Zack\Desktop\rngpp\backgrounds\");
            insertIntoSettings("commandsurl", "string", @"https://dl.dropboxusercontent.com/u/273135957/commands.html");
            insertIntoSettings("commandspath", "string", @"C:\Users\Zack\Desktop\RNGPPDropbox\Dropbox\Public\commands.html");
            insertIntoSettings("biaspointspread", "string", "10:10:10:10:9:8:6.5");
        }


        public void insertIntoSettings(string variable, string type, string value)//escapes values, woo! Except for types, but those really shouldn't be able to.
        {
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO newsettings (variable,type , value) VALUES ( @par0, '" + type + "', @par1);", dbConn);
            cmd.Parameters.AddWithValue("@par0", variable);
            cmd.Parameters.AddWithValue("@par1", value);
            cmd.ExecuteNonQuery();
        }
    }
}
