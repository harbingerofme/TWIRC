using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.IO;

namespace TWIRC
{
    public class DatabaseConnector
    {
        public SQLiteConnection main,chat,buttons;
        private Logger log;
        private string name;

        public DatabaseConnector(Logger Log)
        {
            initButtons();
            initChat();
            initMain();

            log = Log;
            name = "DBCONN";
        }

        private void initMain()
        {
            if (!File.Exists("db.sqlite"))
            {
                SQLiteConnection.CreateFile("db.sqlite");
                main = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                main.Open();
                new SQLiteCommand("PRAGMA auto_vacuum = \"1\";", main).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7), points INT DEFAULT 0, alltime INT DEFAULT 0, isnew INTEGER DEFAULT 1);", main).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", main).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", main).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE transactions (name VARCHAR(25) NOT NULL, amount INT NOT NULL,item VARCHAR(1024) NOT NULL,prevMoney INT NOT NULL,date VARCHAR(7) NOT NULL);", main).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE luacoms (keyword VARCHAR(60) NOT NULL, command VARCHAR(60) NOT NULL, defult VARCHAR(60), response VARCHAR(1000));", main).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE biases (keyword VARCHAR(50),numbers VARCHAR(50));", main).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'poll' ('name' TEXT(25), 'choice' INTEGER(1));", main).ExecuteNonQuery();

                new SQLiteCommand("INSERT INTO biases (keyword,numbers) VALUES (' left ', '10 0 0 0 0 0 0'),(' up ','0 0 10 0 0 0 0'),(' down ', '0 10 0 0 0 0 0'),(' right ', '0 0 0 10 0 0 0'),(' start ', '0 0 0 0 0 0 10')", main).ExecuteNonQuery();

                new SQLiteCommand("CREATE TABLE IF NOT EXISTS newsettings (variable VARCHAR(128), type VARCHAR(64), value VARCHAR(128));", main).ExecuteNonQuery();
                setSetting("name", "string", "rngppbot");
                setSetting("channel", "string", "#harbbot");
                setSetting("antispam", "bit", "1");
                setSetting("silence", "bit", "1");
                setSetting("oauth", "string", "oauth:67h2n5dny6xf2ho6j7oj3xugu7uurd");
                setSetting("votingenabled", "bit", "0");
                setSetting("cooldown", "int", "20");
                setSetting("logpath", "string", @"C:\Users\Zack\Dropbox\Public\rnglog.txt");
                setSetting("timebetweenvote", "calc", "15*60");
                setSetting("timetovote", "calc", "4*60");
                setSetting("defaultbias", "string", "1.00:1.00:1.00:1.00:0.96:0.92:0.82");
                setSetting("biasmaxdiff", "double", "0.05");
                setSetting("moneypervote", "int", "50");
                setSetting("moneyconversionrate", "double", "0.5");
                setSetting("expallfunction", "string", "8X");
                setSetting("welcomemessagecd", "int", "60");
                setSetting("backgroundspath", "string", @"C:\Users\Zack\Desktop\rngpp\backgrounds\");
                setSetting("commandsurl", "string", @"https://dl.dropboxusercontent.com/u/273135957/commands.html");
                setSetting("commandspath", "string", @"C:\Users\Zack\Desktop\RNGPPDropbox\Dropbox\Public\commands.html");
                setSetting("biaspointspread", "string", "10:10:10:10:9:8:6.5");
                setSetting("poll", "string", "");
                setSetting("antistreambot", "string", "1");
                setSetting("goal", "string", "Set a goal!");
                setSetting("ircserver", "string", "irc.chat.twitch.tv");
            }
            else
            {
                main = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                main.Open();
            }
        }

        private void initChat()
        {
            if (!File.Exists("chat.sqlite"))
                SQLiteConnection.CreateFile("chat.sqlite");
            chat = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
            chat.Open();
            new SQLiteCommand("PRAGMA auto_vacuum = \"1\";", chat).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE IF NOT EXISTS messages (name VARCHAR(25) NOT NULL, message VARCHAR(1024) NOT NULL, time INT(13) NOT NULL);", chat).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE IF NOT EXISTS users (name VARCHAR(25) NOT NULL, lines INT DEFAULT 1);", chat).ExecuteNonQuery();
        }

        private void initButtons()
        {
            if (!File.Exists("buttons.sqlite"))
                SQLiteConnection.CreateFile("buttons.sqlite");
            buttons = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
            buttons.Open();
            new SQLiteCommand("PRAGMA auto_vacuum = \"1\";", buttons).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE IF NOT EXISTS buttons (id INT, left INT, down INT, up INT, right INT, a INT, b INT, start INT);", buttons).ExecuteNonQuery();
        }

        public SQLiteCommand Command(SQLiteConnection conn, string text)
        {
            return new SQLiteCommand(text, conn);
        }

        public SQLiteCommand safeCommand(SQLiteConnection conn, string text,object[] value)
        {
            SQLiteCommand cmd = Command(conn, text);
            for(int i = 0; i< value.Length; i++)
            {
                cmd.Parameters.AddWithValue("@par" + i, value[i]);
            }
            return cmd;
        }

        public bool Execute(SQLiteConnection conn, string text)
        {
            try
            {
                Command(conn, text).ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                log.addLog(name,0,"ERROR: "+e.Message+" Command was: "+text);
                return false;
            }
        }

        public bool Execute(SQLiteCommand command)
        {
            try
            {
                string debug = command.CommandText + command.Parameters.Count;
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                log.addLog(name, 0, "ERROR: " + e.Message);
                return false;
            }
        }

        public bool safeExecute(SQLiteConnection conn, string text, object[] value)
        {
            SQLiteCommand cmd = safeCommand(conn, text,value);
            return Execute(cmd);
        }


        public bool Execute(string text)
        {
            return Execute(main, text);
        }

        public object getSetting(string name)
        {
            SQLiteCommand cmd = safeCommand(main, "SELECT type,value FROM newsettings WHERE variable = @par0;",new object[] {name});
            SQLiteDataReader rdr = cmd.ExecuteReader();
            object output = null;
            if(rdr==null)
            {
                return null;
            }

            if(rdr.Read())
            {
                string raw = rdr.GetString(1);
                switch(rdr.GetString(0))
                {
                    case "bit":
                        if (raw == "0")
                            output = false;
                        if (raw == "1")
                            output =  true;
                        break;
                    case "int": 
                        int i;
                        if (int.TryParse(raw, out i))
                            output = i;
                        else
                            output = null;
                        break;
                    default:
                        output = raw;
                        break;
                }
            }
            return output;
        }

        public bool setSetting(string name, string type, object value, bool addIfNotExists = true)
        {
            if(getSetting(name) != null)
            {
                return safeExecute(main, "UPDATE newsettings SET value = @par0, type = @par1 WHERE variable = @par2;", new object[] {value,type,name});
            }
            else if (addIfNotExists)
            {
                return safeExecute(main, "INSERT INTO newsettings (variable,type,value) VALUES (@par0,@par1,@par2);", new object[] { name, type, value });
            }
            else
            {
                log.addLog(name, 1, "WARNING: setting '" + name + "' does NOT exist, nor the override is enabled.");
                return false;
            }
        }

        public bool addSetting(string name, string type, object value)
        {
            if (getSetting(name) != null)
                return false;
            return setSetting(name, type, value);
        }

        public SQLiteDataReader Reader(SQLiteConnection conn, string text)
        {
            try
            {
                return Command(conn, text).ExecuteReader();
            }
            catch
            {
                return null;
            }
        }

        public List<object[]> Read(SQLiteConnection conn, string text)
        {
            SQLiteDataReader sqldr = Reader(conn, text);
            List<object[]> list = new List<object[]>();
            if(sqldr == null)
            {
                list.Add(new object[] {-2});
                return list;
            }
            
            while(sqldr.Read())
            {
                object[] obj = new object[sqldr.FieldCount];
                for(int i = 0; i< sqldr.FieldCount; i++)
                {
                    obj[i] = sqldr.GetValue(i);
                }
                list.Add(obj);
            }
            if(list.Count == 0)
            {
                list.Add(new object[] { -1 });
            }
            return list;
        }
    }
}
