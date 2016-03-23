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
            SQLiteDataReader sqdlr = db.Reader(db.main, "SELECT Count(*) FROM users WHERE name = '" + channel.Substring(1) + "' OR name = '" + bot_name + "';");
            if (sqdlr.Read() && sqdlr.GetInt32(0) == ((bot_name == channel.Substring(1)) ? 1 : 2))
            {
                db.Execute("UPDATE users SET rank = 4  WHERE name='" + channel.Substring(1) + "';");
                if (bot_name != channel.Substring(1))
                    db.Execute("UPDATE users SET rank = -1 WHERE name = '" + bot_name + "'");
            }
            else
            {
                db.Execute("INSERT INTO users (name,rank,lastseen,isnew) VALUES ('" + channel.Substring(1) + "','4','" + getNowSQL() + "',0);");
                if (bot_name != channel.Substring(1))
                    db.Execute("INSERT INTO users (name,rank,lastseen,isnew) VALUES ('" + bot_name + "','-1','" + getNowSQL() + "',0);");
            }
            
        }

        public void loadSettings()
        {
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT variable, type ,value FROM newsettings GROUP BY variable;");
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
                    case "votingenabled": if (!irc.IsConnected) { voteStatus = ((int)a - 1); }; break;
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
                    case "antistreambot": antistreambot = bitToBool(a); break;
                    case "goal": goal = sqldr.GetString(2); break;
                    case "ircServer": server = sqldr.GetString(2); break;
                }
            }
            sqldr = db.Reader(db.main,"SELECT name,choice FROM poll;");
            while (sqldr.Read())
            {
                poll_votes.Add(new intStr(sqldr.GetInt32(1), sqldr.GetString(0)));
            }
        }

        bool setSetting(string variable, string type, string value, bool force=false)
        {
            return db.setSetting(variable, type, value, force);
        }

        bool bitToBool(double i)
        {
            return (i == 0) ? false : true;
        }

        void insertIntoSettings(string variable, string type, string value)//escapes values, woo! Except for types, but those really shouldn't be able to.
        {
            db.addSetting(variable, type, value);
        }

        void loadHardComs()
        {
            //Here we add some hardcoded commands and stuff (while we do have to write out their responses hardocded too, it's a small price to pay for persistency)
            hardList.Add(new hardCom("!ac", 3, 2));
            hardList.Add(new hardCom("!dc", 3, 1));
            hardList.Add(new hardCom("!ec", 3, 2));
            hardList.Add(new hardCom("!addalias", 3, 2));
            hardList.Add(new hardCom("!delalias", 3, 1));
            hardList.Add(new hardCom("!set", 2, 2));
            hardList.Add(new hardCom("!editcount", 3, 2));
            hardList.Add(new hardCom("!banuser", 3, 1));
            hardList.Add(new hardCom("!unbanuser", 4, 1));
            hardList.Add(new hardCom("!silence", 3, 1));
            hardList.Add(new hardCom("!rank", 0, 0, 60));
            hardList.Add(new hardCom("!permit", 2, 1));
            hardList.Add(new hardCom("!whitelist", 0, 0));
            hardList.Add(new hardCom("!rngppcommands", 0, 0, 120));
            hardList.Add(new hardCom("!calculate", 0, 1));
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
            hardList.Add(new hardCom("!background", 0, 1));
            hardList.Add(new hardCom("!save", 3, 1));
            hardList.Add(new hardCom("!funmode", 3, 0));//   >:)
            hardList.Add(new hardCom("!givemoney", 0, 0));
            hardList.Add(new hardCom("!giveball", 0, 0));
            hardList.Add(new hardCom("!addbias", 3, 8));
            hardList.Add(new hardCom("!delbias", 3, 1));
            hardList.Add(new hardCom("!expall", 0, 1));
            hardList.Add(new hardCom("!repel", 3, 1));
            hardList.Add(new hardCom("!reloadsettings", 3, 0));
            hardList.Add(new hardCom("!changesetting", 4, 2));
            hardList.Add(new hardCom("!poll", 3, 1));
            hardList.Add(new hardCom("!vote", 0, 0));
            hardList.Add(new hardCom("!addsetting", 4, 2));
            hardList.Add(new hardCom("!goal", 0, 0));
            hardList.Add(new hardCom("!setgoal", 2, 1));
            hardList.Add(new hardCom("!bet", 0, 2));
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
            
            try
            {
                voteTimer.Stop();
                voteTimer2.Stop();
                saveTimer.Stop();
                reconTimer.Stop();
                exp_allTimer.Stop();
            }
            catch { }
            if (voteStatus != -1)
            {
                voteTimer = new System.Timers.Timer(timeBetweenVotes * 1000);
                voteTimer.Elapsed += voteTimer_Elapsed;
                voteTimer.AutoReset = false;

                voteTimer2 = new System.Timers.Timer(timeToVote * 1000);
                voteTimer2.AutoReset = false;
                voteTimer2.Elapsed += voteTimer_Elapsed;
                voteTimer2.Start();
            }

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

            bettingChatters = new List<string>();
            betters = new List<string>();
            betAmounts = new List<int[]>();
        }

        void loadAliases()
        {
            SQLiteDataReader rdr = db.Reader(db.main,"SELECT * FROM aliases;");
            while (rdr.Read())
            {
                string[] a = rdr.GetString(0).Split(' ');
                ali k = new ali(a, rdr.GetString(1));
                aliList.Add(k);
            }
        }

       void loadCommands()
        {
            SQLiteDataReader rdr = db.Reader(db.main,"SELECT * FROM commands;");
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
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT * FROM biases;");
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
            SQLiteDataReader sql  = db.Reader(db.main,"SELECT * FROM biases WHERE keyword LIKE '% "+keyword +" %';");
            if(sql.Read()){
                return false;
            }
            else
            {
                sql = db.Reader(db.main,"SELECT keyword FROM biases WHERE numbers  = '"+ numbers +"';");
                if (sql.Read())
                {
                    string str = sql.GetString(0) +" "+ keyword + " ";
                    db.Execute("UPDATE biases SET keyword = '" + str + "' WHERE numbers  = '" + numbers + "';");
                }
                else
                {
                    db.Execute("INSERT INTO biases (keyword,numbers) VALUES (' " + keyword + " ', '" + numbers + "');");
                }
                return true;
               
            }
        }

        bool delBias(string keyword)
        {
            SQLiteDataReader sql = db.Reader(db.main,"SELECT keyword,numbers FROM biases WHERE keyword LIKE '% "+keyword+" %';");
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
                   db.Execute("UPDATE biases SET keyword = '"+newAli+"' WHERE numbers = '"+numbers+"';");
                }
                else
                {
                    db.Execute("DELETE FROM biases WHERE numbers = '" + numbers + "';");
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
            db.Execute("UPDATE users SET isnew = 0 WHERE name = '" + user + "';");
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
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT rank FROM users WHERE name='" + name + "';");
            if (sqldr.Read())
            {
                db.Execute(db.main,"UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';");
                return sqldr.GetInt32(0);
            }
            else
            {

                db.Execute("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');");
                return 0;
            }
        }
        public void setAuth(string user, int level)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT * FROM users WHERE name='" + user + "';");
            if (sqldr.Read())
            {
                db.Execute("UPDATE users SET rank='" + level + "' WHERE name='" + user + "';");
            }
            else
            {
                db.Execute("INSERT INTO users (name,lastseen,rank) VALUES ('" + user + "','" + getNowSQL() + "','" + level + "');");
            }
        }
        public int getPoints(string name)
        {
            name = name.ToLower();
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT points FROM users WHERE name='" + name + "';");
            if (sqldr.Read())
            {
                db.Execute("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';");
                return sqldr.GetInt32(0);
            }
            else
            {

                db.Execute("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');");
                return 0;
            }
        }
        public void setPoints(string user, int amount)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT points FROM users WHERE name='" + user + "';");
            if (sqldr.Read())
            {
                db.Execute("UPDATE users SET points='" + amount + "' WHERE name='" + user + "';");
                db.Execute("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + user + "','" + amount + "','FORCED CHANGE TO AMOUNT','" + sqldr.GetInt32(0) + "','" + getNowSQL() + "');");
            }
            else
            {
                db.Execute("INSERT INTO users (name,lastseen,points) VALUES ('" + user + "','" + getNowSQL() + "','" + amount + "');");
            }
        }
        public int addPoints(string name, int amount, string why)
        {
            name = name.ToLower();
            if (amount != 0)
            {
                int things;
                SQLiteDataReader sqldr = db.Reader(db.main,"SELECT points FROM users WHERE name='" + name + "';");
                if (sqldr.Read())
                {
                    things = sqldr.GetInt32(0);
                    db.Execute("UPDATE users SET points='" + (things + amount) + "' WHERE name='" + name + "';");
                    db.Execute("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + name + "','" + amount + "','" + why + "','" + things + "','" + getNowSQL() + "');");
                    return things;
                }
                else
                {
                    db.Execute("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','" + amount + "');");
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

            db.Execute("DELETE FROM poll WHERE 1=1;");
            poll_votes.Clear();
        }

        bool pollVote(string user, int value)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT choice FROM poll WHERE name='" + user + "';");
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
                    db.Execute("UPDATE poll SET choice = '"+value+"' WHERE name='" + user + "';");
                    poll_votes.Add(new intStr(value, user));
                    return true;
                }
            }
            else
            {
                db.Execute("INSERT INTO poll (name,choice) VALUES ('" + user + "','" + value + "');");
                poll_votes.Add(new intStr(value, user));
                return true;
            }
        }

        bool isNew(string user)
        {
            user = user.ToLower();
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT isnew FROM users WHERE name='" + user + "';");
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
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT alltime, rank FROM users WHERE name='" + name + "';");
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                rank = sqldr.GetInt32(1);
                if (rank == 0 && things + amount > 2500) { setAuth(name, 1); }
                db.Execute("UPDATE users SET alltime=alltime+" + amount + " WHERE name='" + name + "';");
                return things;
            }
            else
            {
                db.Execute("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','" + amount + "');");
                return 0;
            }
        }

        int getAllTime(string name)
        {
            name = name.ToLower();
            SQLiteDataReader sqldr = db.Reader(db.main,"SELECT alltime FROM users WHERE name='" + name + "';");
            if (sqldr.Read())
            {
                db.Execute("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';");
                return sqldr.GetInt32(0);
            }
            else
            {

                db.Execute("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');");
                return 0;
            }
        }

        void storeMessage(string user, string message,int type = 0)
        {
            if (chatter != null)
            {
              //  chatter.Add(user, pullAuth(user), message, type);
            }
            user = user.ToLower();

            db.safeExecute(db.chat, "INSERT INTO messages (name,message,time) VALUES ('" + user + "',@par0," + getNowExtended() + ");",new object[] {message});
            SQLiteDataReader sqldr = db.Reader(db.chat, "SELECT * FROM users WHERE name= '" + user + "';");
            if (sqldr.Read())
            {
                db.Execute(db.chat,"UPDATE users SET lines = lines+1 WHERE name = '" + user + "';");
            }
            else
            {
                db.Execute(db.chat, "INSERT INTO users (name) VALUES ('" + user + "');");
            }

        }
    }
}
