using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;

namespace TWIRC
{
    public partial class HarbBot //contains various sql methods, and loading functions.
    {
        public void initialiseDatabase()
        {
            if (!File.Exists("db.sqlite"))
            {
                SQLiteConnection.CreateFile("db.sqlite");
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                new SQLiteCommand("PRAGMA auto_vacuum = \"1\";",dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7), points INT DEFAULT 0, alltime INT DEFAULT 0, isnew INTEGER DEFAULT 1);", dbConn).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE transactions (name VARCHAR(25) NOT NULL, amount INT NOT NULL,item VARCHAR(1024) NOT NULL,prevMoney INT NOT NULL,date VARCHAR(7) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE luacoms (keyword VARCHAR(60) NOT NULL, command VARCHAR(60) NOT NULL, defult VARCHAR(60), response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE biases (keyword VARCHAR(50),numbers VARCHAR(50));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE IF NOT EXISTS 'poll' ('name' TEXT(25), 'choice' INTEGER(1));", dbConn).ExecuteNonQuery();

                new SQLiteCommand("INSERT INTO biases (keyword,numbers) VALUES (' left ', '10 0 0 0 0 0 0'),(' up ','0 0 10 0 0 0 0'),(' down ', '0 10 0 0 0 0 0'),(' right ', '0 0 0 10 0 0 0'),(' start ', '0 0 0 0 0 0 10')", dbConn).ExecuteNonQuery();
                SQLiteCommand cmd;

                new SQLiteCommand("CREATE TABLE IF NOT EXISTS newsettings (variable VARCHAR(128), type VARCHAR(64), value VARCHAR(128));", dbConn).ExecuteNonQuery();
                insertIntoSettings("name", "string", "rngppbot");
                insertIntoSettings("channel", "string", "#harbbot");
                insertIntoSettings("antispam", "bit", "1");
                insertIntoSettings("silence", "bit", "1");
                insertIntoSettings("oauth", "string", "oauth:67h2n5dny6xf2ho6j7oj3xugu7uurd");
                insertIntoSettings("cooldown", "int", "20");
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
                insertIntoSettings("poll", "string", "");

                loadSettings();

                new SQLiteCommand("INSERT INTO users (name,rank,lastseen,isnew) VALUES ('" + channel.Substring(1) + "','4','" + getNowSQL() + "',0);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen,isnew) VALUES ('" + bot_name + "','-1','" + getNowSQL() + "',0);", dbConn).ExecuteNonQuery();

            }
            else
            {
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                loadSettings();
            }
            
        }

        public void loadSettings()
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT variable, type ,value FROM newsettings GROUP BY variable;",dbConn).ExecuteReader();
            while (sqldr.Read())
            {
                double a = 0; string debug = sqldr.GetString(0);
                if(sqldr.GetString(1) == "calc"){
                    a = calculator.Parse(sqldr.GetString(2)).Answer;
                }
                else if(sqldr.GetString(1) != "string"){
                    a = double.Parse(sqldr.GetString(2));
                }
                List<double> tempDoubleArray; string[] tempStringArray;
                switch(sqldr.GetString(0))
                {
                    case "name": bot_name = sqldr.GetString(2); break;
                    case "channel": channel = sqldr.GetString(2); break;
                    case "antispam": antispam = bitToBool(a);break;
                    case "silence": silence = bitToBool(a); break;
                    case "oauth": oauth = sqldr.GetString(2); break;
                    case "cooldown": globalCooldown = (int)a; break;
                    case "logpath": progressLogPATH = sqldr.GetString(2); break;
                    case "timebetweenvote": timeBetweenVotes = (int)a; break;
                    case "timetovote": timeToVote = (int)a; break;
                    case "moneypervote": moneyPerVote = (int)a; break;
                    case "moneyconversionrate": moneyconversionrate = (int)a; break;
                    case "expallfunction": expAllFunc = sqldr.GetString(2); break;
                    case "defaultbias": tempDoubleArray = new List<double>(); tempStringArray = sqldr.GetString(2).Split(':'); foreach (string s in tempStringArray){tempDoubleArray.Add(double.Parse(s));}biasControl.setDefaultBias(tempDoubleArray.ToArray()); break;
                    case "biasmaxdiff": maxBiasDiff = a; break;
                    case "welcomemessagecd": welcomeMessageCD = (int)a; break;
                    case "backgroundspath": backgroundPATH = sqldr.GetString(2); break;
                    case "commandsurl": commandsURL = sqldr.GetString(2); break;
                    case "commandspath": commandsPATH = sqldr.GetString(2); break;
                    case "biaspointspread": tempDoubleArray = new List<double>(); tempStringArray = sqldr.GetString(2).Split(':'); foreach (string s in tempStringArray) { tempDoubleArray.Add(double.Parse(s)); } tempDoubleArray.CopyTo(newBias,0); break;
                    case "poll": if (sqldr.GetString(2) != "") { tempStringArray = sqldr.GetString(2).Split('|'); poll_name = tempStringArray[0]; poll = new string[tempStringArray.Length - 1]; for (int i = 1; i < tempStringArray.Length; i++) { poll[i - 1] = tempStringArray[i]; } } break;
                }
            }
            sqldr = new SQLiteCommand("SELECT name,choice FROM poll;", dbConn).ExecuteReader();
            while (sqldr.Read())
            {
                poll_votes.Add(new intStr(sqldr.GetInt32(1), sqldr.GetString(0)));
            }
        }

        bool setSetting(string variable, string type, string value, bool force=false)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT variable FROM newsettings WHERE variable='" + variable + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE newsettings SET type = '"+type+"', value = '"+value+"' WHERE variable='" + variable + "';", dbConn).ExecuteNonQuery();
                return true;
            }
            else
            {
                if (force)
                {
                    new SQLiteCommand("INSERT INTO newsettings (variable,type,value) VALUES ('" + variable + "','" + type + "','" + value + "');", dbConn).ExecuteNonQuery();
                    return true;
                }
                return false;
            }
        }

        bool bitToBool(double i)
        {
            if (i == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }

        void insertIntoSettings(string variable, string type, string value)//escapes values, woo! Except for types, but those really shouldn't be able to.
        {
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO newsettings (variable, type, value) VALUES ( @par0, '"+type+"', @par1);", dbConn);
            cmd.Parameters.AddWithValue("@par0", variable);
            cmd.Parameters.AddWithValue("@par1", value);
            cmd.ExecuteNonQuery();
        }

        public void initialiseButtons()
        {
            if (!File.Exists("buttons.sqlite"))
            {
                SQLiteConnection.CreateFile("buttons.sqlite");
                butDbConn = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
                butDbConn.Open();
                new SQLiteCommand("PRAGMA auto_vacuum = \"1\";", butDbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE buttons (id INT, left INT, down INT, up INT, right INT, a INT, b INT, start INT);", butDbConn).ExecuteNonQuery();
            }
            else
            {
                butDbConn = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
                butDbConn.Open();
            }
        }

        void initialiseChat()
        {
            if(!File.Exists("chat.sqlite"))
            {
            SQLiteConnection.CreateFile("chat.sqlite");
            chatDbConn = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
            chatDbConn.Open();
            new SQLiteCommand("PRAGMA auto_vacuum = \"1\";", chatDbConn).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE messages (name VARCHAR(25) NOT NULL, message VARCHAR(1024) NOT NULL, time INT(13) NOT NULL);", chatDbConn).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, lines INT DEFAULT 1);", chatDbConn).ExecuteNonQuery();
        
            }
            else
            {
                chatDbConn = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
                chatDbConn.Open();
            }
        }

        void loadHardComs()
        {
            //Here we add some hardcoded commands and stuff (while we do have to write out their responses hardocded too, it's a small price to pay for persistency)

            hardList.Add(new hardCom("!ac", 3, 2));//addcom (reduced now, so it doesn't conflict with nightbot)
            hardList.Add(new hardCom("!dc", 3, 1));//delcom
            hardList.Add(new hardCom("!ec", 3, 2));//editcom
            hardList.Add(new hardCom("!addalias", 3, 2));//addalias
            hardList.Add(new hardCom("!delalias", 3, 1));//delete alias

            hardList.Add(new hardCom("!set", 2, 2));//elevate another user
            hardList.Add(new hardCom("!editcount", 3, 2));
            hardList.Add(new hardCom("!banuser", 3, 1));
            hardList.Add(new hardCom("!unbanuser", 4, 1));
            hardList.Add(new hardCom("!silence", 3, 1));
            hardList.Add(new hardCom("!rank", 0, 0, 60));
            hardList.Add(new hardCom("!permit", 2, 1));
            hardList.Add(new hardCom("!whitelist", 0, 0));
            hardList.Add(new hardCom("!rngppcommands", 0, 0, 120));
            hardList.Add(new hardCom("!calculate", 0, 1));

            //RNGPP catered commands, commented out means no way of implementing that yet or no idea.
            hardList.Add(new hardCom("!setbias", 4, 7));
            hardList.Add(new hardCom("!setdefaultbias", 4, 7));
            hardList.Add(new hardCom("!setbiasmaxdiff", 4, 1));
            hardList.Add(new hardCom("!resetbias", 4, 0));
            hardList.Add(new hardCom("!bias", 0, 1));
            hardList.Add(new hardCom("!balance", 0, 0, 60));
            hardList.Add(new hardCom("!check", 3, 1));
            hardList.Add(new hardCom("!addlog", 0, 1, 5));
            hardList.Add(new hardCom("!setpoints", 4, 2));
            hardList.Add(new hardCom("!voting", 3, 1));
            //hardList.Add(new hardCom("!maintenance", 3, 1));
            hardList.Add(new hardCom("!background", 0, 1));
            //hardList.Add(new hardCom("!song",0,1));
            //hardList.Add(new hardCom("!seriousmode",3,1);
            hardList.Add(new hardCom("!save", 3, 1));
            hardList.Add(new hardCom("!funmode", 3, 0));//   >:)
            hardList.Add(new hardCom("!givemoney", 0, 0));
            hardList.Add(new hardCom("!giveball", 0, 0));
            hardList.Add(new hardCom("!addbias", 3, 8));
            hardList.Add(new hardCom("!delbias", 3, 1));
            hardList.Add(new hardCom("!expall", 0, 1));
            hardList.Add(new hardCom("!repel", 3, 1));
            hardList.Add(new hardCom("!reloadsettings", 3, 0));
            hardList.Add(new hardCom("!changesetting", 5, 2));
            hardList.Add(new hardCom("!poll", 3, 1));
            hardList.Add(new hardCom("!vote", 0, 0));
        }

        void setUpIRC()
        {
            irc.Encoding = System.Text.Encoding.UTF8;//twitch's encoding
            irc.SendDelay = 1500;
            irc.ActiveChannelSyncing = true;
            irc.OnConnected += ircConnected;
            irc.OnDisconnected += ircDisconnected;
            irc.OnDisconnecting += ircDisconnecting;
            irc.OnConnecting += ircConnecting;
            irc.OnConnectionError += ircConError;
            irc.OnError += ircError;
            irc.OnQueryNotice += ircNotice;


            irc.OnQueryMessage += ircQuery;
            irc.OnRawMessage += ircRaw;
            irc.OnChannelAction += ircChanActi; //defined in harbbot_split
            irc.OnChannelMessage += ircChanMess; //defined in harbbot_split
        }

        void prepareTimers()
        {
            voteTimer = new System.Timers.Timer(timeBetweenVotes*1000);
            voteTimer.Elapsed += voteTimer_Elapsed;
            voteTimer.AutoReset = false;
            voteTimer.Start();

            voteTimer2 = new System.Timers.Timer(timeToVote*1000);
            voteTimer2.AutoReset = false;
            voteTimer2.Elapsed += voteTimer_Elapsed;

            saveTimer_Elapsed(null, null);

            saveTimer = new System.Timers.Timer(30 * 60 * 1000);
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += saveTimer_Elapsed;
            saveTimer.Start();

            reconTimer = new System.Timers.Timer(5000);
            reconTimer.AutoReset = true;
            reconTimer.Elapsed += reconTimer_Elapsed;
            reconTimer.Start();

            exp_allTimer = new System.Timers.Timer(1);
            exp_allTimer.AutoReset = false;
            exp_allTimer.Enabled = false;

            pollTimer = new System.Timers.Timer(1000 * 60 * 60 * 2);
            pollTimer.AutoReset = true;
            pollTimer.Elapsed += pollTimer_Elapsed;
            pollTimer.Start();
        }

        void loadAliases()
        {
            SQLiteDataReader rdr = new SQLiteCommand("SELECT * FROM aliases;", dbConn).ExecuteReader();
            while (rdr.Read())
            {
                string[] a = rdr.GetString(0).Split(' ');
                ali k = new ali(a, rdr.GetString(1));
                aliList.Add(k);
            }
        }

       void loadCommands()
        {
            SQLiteDataReader rdr = new SQLiteCommand("SELECT * FROM commands;", dbConn).ExecuteReader();
            while (rdr.Read())
            {
                string[] a = rdr.GetString(3).Split(new string[] { @"\n" }, StringSplitOptions.RemoveEmptyEntries);
                command k = new command(rdr.GetString(0), a, rdr.GetInt32(1));
                k.setCount(rdr.GetInt32(2));
                k.setCooldown(globalCooldown);
                comlist.Add(k);
            }
        }

       void loadBiases()
        {
            List<Bias> biases = new List<Bias>();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM biases;",dbConn).ExecuteReader();
            while(sqldr.Read())
            {
                foreach(string s in sqldr.GetString(0).Trim().Split(' '))
                {
                    if(s.Length>=1)
                    biases.Add(new Bias(s, sqldr.GetString(1)));
                }
            }
            biasList = biases;
        }

        bool addBias(string keyword, string numbers)
        {
            SQLiteDataReader sql  = new SQLiteCommand("SELECT * FROM biases WHERE keyword LIKE '% "+keyword +" %';",dbConn).ExecuteReader();
            if(sql.Read()){
                return false;
            }
            else
            {
                sql = new SQLiteCommand("SELECT keyword FROM biases WHERE numbers  = '"+ numbers +"';",dbConn).ExecuteReader();
                if (sql.Read())
                {
                    string str = sql.GetString(0) +" "+ keyword + " ";
                    new SQLiteCommand("UPDATE biases SET keyword = '" + str + "' WHERE numbers  = '" + numbers + "';",dbConn).ExecuteNonQuery();
                }
                else
                {
                    new SQLiteCommand("INSERT INTO biases (keyword,numbers) VALUES (' " + keyword + " ', '" + numbers + "');",dbConn).ExecuteNonQuery();
                }
                return true;
               
            }
        }

        bool delBias(string keyword)
        {
            SQLiteDataReader sql = new SQLiteCommand("SELECT keyword,numbers FROM biases WHERE keyword LIKE '% "+keyword+" %';",dbConn).ExecuteReader();
            if (sql.Read())
            {
                string numbers = sql.GetString(1);
                string oldAli = sql.GetString(0).Trim();
                string[] split = oldAli.Split(' ');
                string newAli = " ";
                foreach (string str in split)
                {
                    if(str != keyword)
                    {
                        newAli += str + " ";
                    }
                }
                if(newAli.Length > 1)
                {
                    new SQLiteCommand("UPDATE biases SET keyword = '"+newAli+"' WHERE numbers = '"+numbers+"';", dbConn).ExecuteNonQuery();
                }
                else
                {
                    new SQLiteCommand("DELETE FROM biases WHERE numbers = '" + numbers + "';", dbConn).ExecuteNonQuery();
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        void notNew(string user)
        {
            user = user.ToLower();
            new SQLiteCommand("UPDATE users SET isnew = 0 WHERE name = '" + user + "';", dbConn).ExecuteNonQuery();
        }

        int getNow()
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        string getNowSQL()
        {
            string str = DateTime.Now.Year.ToString();
            if (DateTime.Now.DayOfYear < 100) { str += "0"; }
            if (DateTime.Now.DayOfYear < 10) { str += "0"; }
            str += DateTime.Now.DayOfYear.ToString();
            //(int)DateTime.Now.TimeOfDay.TotalSeconds;
            return str;
        }
        string getNowExtended()
        {
            string str = DateTime.Now.Year.ToString();
            if (DateTime.Now.DayOfYear < 100) { str += "0"; }
            if (DateTime.Now.DayOfYear < 10) { str += "0"; }
            str += DateTime.Now.DayOfYear.ToString();
            if (DateTime.Now.Hour < 10) { str += "0"; }
            str += DateTime.Now.Hour;
            if (DateTime.Now.Minute < 10) { str += "0"; }
            str += DateTime.Now.Minute;
            if (DateTime.Now.Second < 10) { str += "0"; }
            str += DateTime.Now.Second;
            return str;
        }

        public int pullAuth(string name)
        {
            name = name.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT rank FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        public void setAuth(string user, int level)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET rank='" + level + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,rank) VALUES ('" + user + "','" + getNowSQL() + "','" + level + "');", dbConn).ExecuteNonQuery();
            }
        }
        public int getPoints(string name)
        {
            name = name.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        public void setPoints(string user, int amount)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET points='" + amount + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + user + "','" + amount + "','FORCED CHANGE TO AMOUNT','" + sqldr.GetInt32(0) + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + user + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
            }
        }
        public int addPoints(string name, int amount, string why)
        {
            name = name.ToLower();
            if (amount != 0)
            {
                int things;
                SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
                if (sqldr.Read())
                {
                    things = sqldr.GetInt32(0);
                    new SQLiteCommand("UPDATE users SET points='" + (things + amount) + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                    new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + name + "','" + amount + "','" + why + "','" + things + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                    return things;
                }
                else
                {
                    new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
                    return 0;
                }
            }
            return 0;
        }

        public void pollOpen()
        {
            string temp = poll_name + "|";
            foreach (string s in poll)
            {
                temp += s + "|";
            }
            temp = temp.Substring(0, temp.Length - 1);//remove last delimiter
            setSetting("poll", "string", temp, true);

            new SQLiteCommand("DELETE FROM poll WHERE 1=1;", dbConn).ExecuteNonQuery();
            poll_votes.Clear();
        }

        bool pollVote(string user, int value)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT choice FROM poll WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                int a = sqldr.GetInt32(0);
                if (a == value)
                {
                    return false;
                }
                else
                {
                    int index = poll_votes.FindIndex(delegate(intStr InSt) { return InSt.Str == user; });
                    poll_votes.RemoveAt(index);
                    new SQLiteCommand("UPDATE poll SET choice = '"+value+"' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
                    poll_votes.Add(new intStr(value, user));
                    return true;
                }
            }
            else
            {
                new SQLiteCommand("INSERT INTO poll (name,choice) VALUES ('" + user + "','" + value + "');", dbConn).ExecuteNonQuery();
                poll_votes.Add(new intStr(value, user));
                return true;
            }
        }

        bool isNew(string user)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT isnew FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                int a = sqldr.GetInt32(0);
                if (sqldr.GetInt32(0) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        int addAllTime(string name, int amount)
        {
            int things, rank;
            name = name.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime, rank FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                rank = sqldr.GetInt32(1);
                if (rank == 0 && things + amount > 2500) { setAuth(name, 1); }
                new SQLiteCommand("UPDATE users SET alltime=alltime+" + amount + " WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return things;
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }

        int getAllTime(string name)
        {
            name = name.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }

        void storeMessage(string user, string message,int type = 0)
        {
            if (chatter != null)
            {
                chatter.Add(user, pullAuth(user), message, type);
            }
            user = user.ToLower();
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO messages (name,message,time) VALUES ('" + user + "',@par1," + getNowExtended() + ");", chatDbConn);
            cmd.Parameters.AddWithValue("@par1", message); cmd.ExecuteNonQuery();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM users WHERE name= '" + user + "';", chatDbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lines = lines+1 WHERE name = '" + user + "';", chatDbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name) VALUES ('" + user + "');", chatDbConn).ExecuteNonQuery();
            }

        }
    }
}
