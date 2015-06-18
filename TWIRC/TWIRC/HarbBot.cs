using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using Meebey.SmartIrc4net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;
using System.Windows.Forms;

namespace SayingsBot
{
    public class HarbBot
    {
        //really important stuff
        public static IrcClient irc = new IrcClient();
        public bool running = true;
        public SQLiteConnection dbConn,chatDbConn;
        public static Logger logger;

        //important stuff
        public string bot_name, oauth, channels;

        //commands and aliases
        public List<command> comlist = new List<command>();
        public List<ali> aliList = new List<ali>();
        public List<hardCom> hardList = new List<hardCom>();
        public int globalCooldown;
        public Commands commands;

        //antispam
        public bool antispam; public List<intStr> permits = new List<intStr>(); public int asCooldown = 60, permitTime = 300;
        public List<asUser> asUsers = new List<asUser>();
        public List<intStr> asCosts = new List<intStr>();
        public List<string> asTLDs = new List<string>(), asWhitelist = new List<string>(),asWhitelist2 = new List<string>();
        List<List<string>> asResponses = new List<List<string>>();

        //defines the output level of our connection
        public int logLevel;
        
        //some settings
        public bool silence,isMod = false;
        public string progressLogPATH = sysPathStatic() + "\\SayingsBotLog.txt";

        System.Timers.Timer saveTimer = null;
        System.Timers.Timer reconTimer = null;
        System.Timers.Timer colourTimer = null;

        public Thread one;

        ProfanityFilter pf = new ProfanityFilter();

        public HarbBot(Logger logLogger)
        {
            logger = logLogger;
            irc.Encoding = System.Text.Encoding.UTF8;//twitch's encoding
            irc.SendDelay = 1500;
            irc.ActiveChannelSyncing = true;

            //write these Methods
            irc.OnConnected += ircConnected;
            irc.OnDisconnected += ircDisconnected;
            irc.OnDisconnecting += ircDisconnecting;
            irc.OnConnecting += ircConnecting;
            irc.OnConnectionError += ircConError;
            irc.OnError += ircError;
            irc.OnQueryNotice += ircNotice;

            irc.OnQueryMessage += ircQuery;
            irc.OnRawMessage += ircRaw;
            irc.OnChannelAction += ircChanActi;
            irc.OnChannelMessage += ircChanMess;

            //LoadCommands
            if (logLevel != 0)
            {
                logger.WriteLine("IRC: Booting up, shouldn't take long!");
            }
            if (!File.Exists("db.sqlite"))
            {
                if (logLevel != 0)
                {
                    logger.WriteLine("IRC: First time setup detected, making database");
                }
                bot_name = "sayingsbot";
                channels = "#rngplayspokemon";
                
                globalCooldown = 20; 
                antispam = false;
                oauth = "oauth:tpjq7tyuevo3nyre0ywdlklznmkh0r"; //TODO: not the latest OAUTH
                logLevel = 2;
                //progressLogPATH = sysPath() + "\\SayingsBotLog.txt";

                //TODO: Add sayingsbotty things

                short temp2 = 0; if (antispam) { temp2 = 1; }
                SQLiteConnection.CreateFile("db.sqlite");
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7), points INT DEFAULT 0, alltime INT DEFAULT 0);", dbConn).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE settings (name VARCHAR(25) NOT NULL, channel VARCHAR(26) NOT NULL, antispam TINYINT(1) DEFAULT 1, silence TINYINT(1) DEFAULT 0, oauth VARCHAR(200), cooldown INT DEFAULT 20,loglevel TINYINT(1) DEFAULT 2,logPATH VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE transactions (name VARCHAR(25) NOT NULL, amount INT NOT NULL,item VARCHAR(1024) NOT NULL,prevMoney INT NOT NULL,date VARCHAR(7) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE ascostlist (type VARCHAR(25), costs INT DEFAULT 0, message VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aswhitelist (name VARCHAR(50),regex VARCHAR(50));", dbConn).ExecuteNonQuery();
                //SayingsBot Tables
                new SQLiteCommand("CREATE TABLE misc (ID VARCHAR(50) NOT NULL, DATA VARCHAR(50) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE userAliases (user VARCHAR(100) NOT NULL, alias VARCHAR(1024) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE userdata (user VARCHAR(25) NOT NULL, datatype INT NOT NULL, dataID INT, data VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();

                new SQLiteCommand("INSERT INTO settings (name,channel,antispam,silence,oauth,cooldown,loglevel,logPATH) VALUES ('" + bot_name + "','" + channels + "','" + temp2 + "',0,'" + oauth + "','" + globalCooldown + "','"+logLevel+"','"+progressLogPATH+"');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen) VALUES ('" + channels.Substring(1) + "','4','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen) VALUES ('"+bot_name+"','-1','"+getNowSQL()+"');",dbConn).ExecuteNonQuery();
                //SayingsBot Data
                new SQLiteCommand("INSERT INTO misc (ID, DATA) VALUES ('CountGame', '0');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO userdata (user, datatype, data) VALUES ('ExampleUser', '0', 'This is ExampleUser\'s !whoisuser message!');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES ('ExampleUser', '1', '1', 'This is an example quote from an example user!');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES ('overallRandom', '5', '1', 'ExampleUser');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO userdata (user, datatype, data) VALUES (swearJar, 6, 0);", dbConn).ExecuteNonQuery();

                SQLiteCommand cmd;
                new SQLiteCommand("INSERT INTO ascostlist (type,costs,message) VALUES ('link','5','Google Those Nudes!\nWe are not buying your shoes!\nThe stuff people would have to put up with...');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO ascostlist (type,costs,message) VALUES ('emote spam','3','Images say more than a thousand words, so stop writing essays.\nHow is a timeout for a twitch feature?\nI dislike emotes, they are all text to me.');", dbConn).ExecuteNonQuery();
                cmd = new SQLiteCommand("INSERT INTO ascostlist (type,costs,message) VALUES ('letter spam','1',@par1);", dbConn);
                cmd.Parameters.AddWithValue("@par1", "There's no need to type that way.\nI do not take kindly upon that.\nStop behaving like a spoiled little RNG!");cmd.ExecuteNonQuery();
                cmd =new SQLiteCommand("INSERT INTO ascostlist (type,costs,message) VALUES ('ASCII','7',@par1);", dbConn);
                cmd.Parameters.AddWithValue("@par1", "Whatever that was, it's gone now.\nOak's words echo: This is not the time for that!\nWoah, you typed all of that? Who am I kidding, get out!"); cmd.ExecuteNonQuery();
                cmd = new SQLiteCommand("INSERT INTO ascostlist (type,costs,message) VALUES ('tpp',2,@par1);",dbConn);
                cmd.Parameters.AddWithValue("@par1", "Don't you love how people just tend to disregard the multiple texts, saying this isn't TPP?\nI'm not Twippy, stop acting like a slave to him.\nTry !what."); cmd.ExecuteNonQuery();

                //I'll leave adding the whitelist to manual typing
            }
            else
            {
                    dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                    dbConn.Open();
                    SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM settings;", dbConn).ExecuteReader();
                    sqldr.Read();
                    bot_name = sqldr.GetString(0);
                    channels = sqldr.GetString(1);
                    antispam = false; if (sqldr.GetInt32(2) == 1) { antispam = true; }
                    silence = false; if (sqldr.GetInt32(3) == 1) { silence = true; }
                    oauth = sqldr.GetString(4);
                    globalCooldown = sqldr.GetInt32(5);
                    logLevel = sqldr.GetInt32(6);
                    //progressLogPATH = sqldr.GetString(7);
            }
            if(!File.Exists("chat.sqlite")){
                SQLiteConnection.CreateFile("chat.sqlite");
                chatDbConn = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
                chatDbConn.Open();
                new SQLiteCommand("CREATE TABLE messages (name VARCHAR(25) NOT NULL, message VARCHAR(1024) NOT NULL, time INT(13) NOT NULL);",chatDbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, lines INT DEFAULT 1);",chatDbConn).ExecuteNonQuery();
            }
            else
            {
                chatDbConn = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
                chatDbConn.Open();
            }

            if(!File.Exists("TLDs.twirc"))
            {
                writeFile("TLDs.twirc","com\nnl\nde\nnet\nbiz\nuk");
            }
            asTLDs = FileLines("TLDs.twirc").ToList();

            SQLiteDataReader rdr = new SQLiteCommand("SELECT * FROM commands;", dbConn).ExecuteReader();
            while (rdr.Read())
            {
                string[] a = rdr.GetString(3).Split(new string[] {@"\n"},StringSplitOptions.RemoveEmptyEntries);
                command k = new command(rdr.GetString(0), a, rdr.GetInt32(1));
                k.setCount(rdr.GetInt32(2));
                k.setCooldown(globalCooldown);
                comlist.Add(k);
            }
            if (logLevel != 0)
            {
                logger.WriteLine("IRC: Loaded " + comlist.Count() + " commands!");
            }

            rdr = new SQLiteCommand("SELECT * FROM aliases;", dbConn).ExecuteReader();
            while (rdr.Read())
            {
                string[] a = rdr.GetString(0).Split(' ');
                ali k = new ali(a, rdr.GetString(1));
                aliList.Add(k);
            }
            if (logLevel != 0)
            {
                logger.WriteLine("IRC: Loaded " + aliList.Count() + " aliases!");
            }

            rdr = new SQLiteCommand("SELECT * FROM ascostlist", dbConn).ExecuteReader(); int tempInt = 0;
            while(rdr.Read())
            {
                asResponses.Add(new List<string>());
                asCosts.Add(new intStr(rdr.GetString(0), rdr.GetInt32(1)));
                asResponses[tempInt] = rdr.GetString(2).Split(new string[] { "\n" },StringSplitOptions.None).ToList();
                tempInt++;
            }

            rdr = new SQLiteCommand("SELECT * FROM aswhitelist", dbConn).ExecuteReader();
            while(rdr.Read())
            {
                asWhitelist2.Add(rdr.GetString(0));
                asWhitelist.Add(rdr.GetString(1));
            }

            hardList.Add(new hardCom("!sbaddcom", 3, 2));//addcom (reduced now, so it doesn't conflict with nightbot)
            hardList.Add(new hardCom("!sbdelcom", 3, 1));//delcom
            hardList.Add(new hardCom("!sbeditcom", 3, 2));//editcom
            hardList.Add(new hardCom("!sbaddalias", 3, 2));//addalias
            hardList.Add(new hardCom("!sbdelalias", 3, 1));//delete alias
            
            hardList.Add(new hardCom("!sbset", 2, 2));//elevate another user
            hardList.Add(new hardCom("!sbeditcount", 3, 2));
            hardList.Add(new hardCom("!banuser", 3, 1));
            hardList.Add(new hardCom("!unbanuser", 4, 1));
            hardList.Add(new hardCom("!sbsilence",3,1));
            hardList.Add(new hardCom("!sbrank", 0, 0,60));
            hardList.Add(new hardCom("!commands", 0, 0, 120));
          
            hardList.Add(new hardCom("!whoisuser",0,1,20));
            hardList.Add(new hardCom("!classicwhoisuser", 0, 1, 20));
            hardList.Add(new hardCom("!editme",0,0));
            hardList.Add(new hardCom("!edituser",3,1));
            hardList.Add(new hardCom("!classic",0,1,20));
            hardList.Add(new hardCom("!addclassic",2,1));
            hardList.Add(new hardCom("!delclassic",2,1));
            hardList.Add(new hardCom("!givecake",0,1,20));
            hardList.Add(new hardCom("!givepie", 0,1,20));
            hardList.Add(new hardCom("!givea", 0, 1, 20));
            hardList.Add(new hardCom("!givesome", 0, 1, 20));
            hardList.Add(new hardCom("!kill", 0, 0, 20));
            hardList.Add(new hardCom("!calluser", 0, 1, 20));
            hardList.Add(new hardCom("!quotes",0,2,20));
            hardList.Add(new hardCom("!count",3,0));
            hardList.Add(new hardCom("!newcount",3,0));
            hardList.Add(new hardCom("!points",0,0));
            hardList.Add(new hardCom("!seepoints", 0, 1));
            hardList.Add(new hardCom("!setpoints", 5, 2));
            hardList.Add(new hardCom("!addpoints", 3, 2));
            hardList.Add(new hardCom("!nc", 0, 1));
            hardList.Add(new hardCom("!sbversion",0,0));
            hardList.Add(new hardCom("!sbleaderboard",0,0));
            hardList.Add(new hardCom("!sqlquery",5,0));
            hardList.Add(new hardCom("sayingsbot",0,0,20));
            hardList.Add(new hardCom("!sbadduseralias", 2, 2, 20));
            hardList.Add(new hardCom("!sbgetuseraliases", 2, 1, 20));
            hardList.Add(new hardCom("!swearjar", 0, 0));
            hardList.Add(new hardCom("!nightbotisdown", 0, 0));
            hardList.Add(new hardCom("!logbotisdown", 0, 0));

            one = new Thread(connection);
            one.Name = "SAYINGSBOT IRC CONNECTION";
            one.IsBackground = true;

            
            //saveTimer = new System.Timers.Timer(5 * 60 * 1000);
            //saveTimer.AutoReset = true;
            //saveTimer.Elapsed += saveTimer_Elapsed;
            //saveTimer.Start();

            colourTimer = new System.Timers.Timer(10000);
            colourTimer.AutoReset = true;
            colourTimer.Elapsed += colourTimer_Elapsed;
            colourTimer.Start();

            reconTimer = new System.Timers.Timer(5000);
            reconTimer.AutoReset = true;
            reconTimer.Elapsed += reconTimer_Elapsed;
            reconTimer.Start();

            commands = new Commands(this, this.dbConn, this.hardList, this.comlist, this.aliList, this.logLevel, logger);

            try
            {
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch { }
        }
        /// <summary>
        /// The time that randomly changes SayingsBot's Colour
        /// </summary>
        void colourTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Random rnd = new Random();
            RNGWindow rngwin = Program.mainWindow;
            int I;
            I = rnd.Next(11);
            if (I == 1)
            {
                I = rnd.Next(11);
                switch (I)
                {
                    case 1: irc.RfcPrivmsg(channels, ".color blue"); rngwin.setColourText("Blue"); break;
                    case 2: irc.RfcPrivmsg(channels, ".color green"); rngwin.setColourText("Green"); break;
                    case 3: irc.RfcPrivmsg(channels, ".color orangered"); rngwin.setColourText("OrangeRed"); break;
                    case 4: irc.RfcPrivmsg(channels, ".color red"); rngwin.setColourText("Red"); break;
                    case 5: irc.RfcPrivmsg(channels, ".color GoldenRod"); rngwin.setColourText("GoldenRod"); break;
                    case 6: irc.RfcPrivmsg(channels, ".color HotPink"); rngwin.setColourText("HotPink"); break;
                    case 7: irc.RfcPrivmsg(channels, ".color SeaGreen"); rngwin.setColourText("SeaGreen"); break;
                    case 8: irc.RfcPrivmsg(channels, ".color Chocolate"); rngwin.setColourText("Chocolate"); break;
                    case 9: irc.RfcPrivmsg(channels, ".color FireBrick"); rngwin.setColourText("FireBrick"); break;
                    case 10: irc.RfcPrivmsg(channels, ".color Coral"); rngwin.setColourText("Coral"); break;
                }
            }
            
            
        }
        [System.Obsolete("Unused")]
        void saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            /*write here what you want to save, I've left the previous one here.
             * 
            writeFile(commandsPATH, "<DOCTYPE html><head><title>RNGPPBot commands</title><h1>RNGPPBot commands</h1></head>If this page looks sloppy, it is because it is. I've paid no attention to any standards whatsoever.<table border='1px' cellspacing='0px'><tr><td><b>keyword</b></td><td><b>level required</b>(0 = user, 1 = regular, 2 = trusted, 3 = mod, 4 = broadcaster, 5 = secret)</td><td><b>output<b></td></tr>");
            foreach (command c in comlist)
            {
                appendFile(commandsPATH, "<tr><td>" + c.getKey() + "</td><td>" + c.getAuth() + "</td><td>" + c.getResponse() + "</td></tr>");
            }
            appendFile(commandsPATH, "</table>\nBOOTIFUL!");

            string s = "INSERT INTO buttons (left,down,up,right,a,b,start) VALUES (";
            foreach (int a in biasControl.stats)
            {
                s += a + ",";
            }
            s = s.Substring(0, s.Length - 1);
            s += ");";
            new SQLiteCommand(s, butDbConn).ExecuteNonQuery();
            biasControl.stats = new int[] { 0, 0, 0, 0, 0, 0, 0 };
             */
        }
        /// <summary>
        /// The timer that checks connection status and attempts to reconnect acordingly.
        /// </summary>
        void reconTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!irc.IsConnected)
            {
                logger.WriteLine("HOLY AWEPRLFPVREA NOT CONNECTED.. RECONNECTING NOW!~");
                doReconnect();
            }

        }
        void connection()
        {
            irc.RfcJoin(channels);
            irc.Listen();

        }
        [System.Obsolete("Unused")]
        public void say(string message)
        {   
            sendMess(channels, message);
            checkCommand(channels, channels.Substring(1), filter(message));//I guess?
        }
        [System.Obsolete("Unused, but I have fuuture plans...")]
        public bool checkSpam(string channel, string user, string message)
        {
            List<asUser> temp = new List<asUser>();List<intStr> temp2 = new List<intStr>();
            foreach(asUser person in asUsers)
            {
                if (person.lastUpdate < getNow() - asCooldown) { temp.Add(person); }
                if(person.points<1){person.points=2;}//resets the person's limit if they misused it, but keeps it within quick timeout range.
            }
            foreach(asUser person in temp)
            {
                asUsers.Remove(person);
            }
            foreach (intStr person in permits)
            {
                if (person.Int < getNow() - permitTime)
                {
                    temp2.Add(person);
                }
            }
            foreach (intStr person in temp2)
            {
                permits.Remove(person);
            }

            if (pullAuth(user) < 2)
            {
                int a = asUsers.FindIndex(x => x.name == user);
                int type = -1;
                if (a == -1)
                {
                    a = asUsers.Count;
                    asUsers.Add(new asUser(user, pullAuth(user)));
                }
                if (message != "")
                {
                    message = message.ToLower();
                    if (Regex.Match(message, @"^.$").Success || Regex.Match(message,@"([a-zA-Z])\1\1").Success || Regex.Match(message,@"([0-9])\1\1\1").Success || Regex.Match(message,@"([^[0-9a-zA-Z]]){4}").Success) { asUsers[a].update(asCosts[2].Int); type=2; }//either a single letter, 3 same letters in a row, 4 not alphanumerical characters in a row,
                    if (message.Length > 40 && Regex.Match(message, @"^[^[a-zA-Z]]*$").Success) { asUsers[a].update(asCosts[3].Int); type=3; }

                    MatchCollection mc = Regex.Matches(message, @"[^ ]+\.([a-z]{2,})[\/\?\#]?".ToLower());
                    MatchCollection me = Regex.Matches(message, @"([^ ]+\.[a-z]{2,})[\/\?\#]?".ToLower());
                    int b = mc.Count; int d = 0; ;
                    if (b > 0)
                    {
                        foreach (Match c in mc)
                        {
                            if (asTLDs.Contains(c.Groups[1].Value.ToUpper())) { d++; }
                        }
                        foreach (Match c in me)
                        {
                            if (Regex.Match(message, @"twitch\.tv\/" + channel.Substring(1) + @"\/c\/").Success) { d--; continue; }
                            foreach(string e in asWhitelist)
                            {
                                if (Regex.Match(c.Value, e).Success) { d--; continue; }

                            }
                        }
                        foreach (intStr f in permits)
                        {
                            if (f.Str == user) { b = 0; permits.Remove(f); break; }
                        }
                        if(d>0){ asUsers[a].update(asCosts[0].Int); type = 0; }
                    }
                }
                if(type!=-1 && asUsers[a].points<1){
                    irc.RfcPrivmsg(channel,".timeout "+user+" 1");//overrides the send delay (hopefully)
                    int c = new Random().Next(0,4);
                    sendMess(channel,user+" -> "+asResponses[type][c]+" ("+asCosts[type].Str+")");
                    return true;
                }
            }
            if (user == "zackattack9909" && Regex.Match(message,"wix[1-4]").Success) {irc.RfcPrivmsg(channel,".clear");sendMess(channel,"Zack, please don't.");}
            return false;
        }
        public string filter(string message)
        {
            string result = message;
            foreach (ali alias in aliList)
            {
                result = alias.filter(message);//shouldn't matter much
            }
            return result;
        }
        public void checkCommand(string channel, string user, string message)
        {
            //Yup
            commands.checkCommand(channel, user, message);
        }
        public void checkAlias(string channel, string user, string message)
        {
            string[] str, tempVar3;
            bool done = false; int auth = pullAuth(user);
            bool fail; int tempVar1 = 0; string tempVar2 = "";
            string User = user.Substring(0, 1).ToUpper() + user.Substring(1);
            if (!done)
            {
                foreach (ali c in aliList)//flexible commands
                {
                    if (c.doesMatch(message) && c.canTrigger() && c.getAuth() <= auth)
                    {
                        if (logLevel == 1) { logger.WriteLine("IRC:<- <" + user + ">" + message); }
                        str = c.getResponse(message, user);
                        c.addCount(1);
                        SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET count = '" + c.getCount() + "' WHERE keyword = @par1;", dbConn);
                        cmd.Parameters.AddWithValue("@par1", c.getKey());
                        cmd.ExecuteNonQuery();
                        //if (str.Count() != 0) { if (str[0] != "") { c.updateTime(); } }
                        foreach (string b in str)
                        {
                            sendMess(channel, b);
                        }
                    }
                }
            }
        }
        public void Close()
        {
            try
            {
                one.Abort();
                irc.RfcQuit();
            }
            catch { }
        }
        public void sendMess(string channel, string message)
        {
            if (logLevel > 0)
            {
                logger.WriteLine("IRC: ->" + channel + ": " + message);
            }
            if (!silence)
            {
                irc.SendMessage(SendType.Message, channel, message);
                storeMessage(bot_name, message);
            }
        }
        public int getNow()
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }
        public string getNowSQL()
        {
            string str = DateTime.Now.Year.ToString();
            if (DateTime.Now.DayOfYear < 100) { str += "0"; }
            if (DateTime.Now.DayOfYear < 10) { str += "0"; }
            str += DateTime.Now.DayOfYear.ToString();
            //(int)DateTime.Now.TimeOfDay.TotalSeconds;
            return str;
        }
        public string getNowExtended(){
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
        public void storeMessage(string user, string message) {
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO messages (name,message,time) VALUES ('" + user + "',@par1," + getNowExtended() + ");", chatDbConn);
            cmd.Parameters.AddWithValue("@par1", message); cmd.ExecuteNonQuery();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM users WHERE name= '"+user+"';",chatDbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lines = lines+1 WHERE name = '" + user + "';", chatDbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name) VALUES ('" + user + "');", chatDbConn).ExecuteNonQuery();
            }
        }
        #region IRC
        public void doDisconnect()
        {

            logger.Write("IRC Disconnecting");
            reconTimer.Stop();
            colourTimer.Stop();
            if (!irc.IsConnected)
            {
                logger.WriteLine("... already disconnected.");
                return;
            }

            try
            {
                irc.Disconnect();
            }
            catch (Exception ex)
            {
                logger.WriteLine("... IRC DISCONNECT FAILED: " + ex.Message);
            }

            if (!irc.IsConnected)
            {
                logger.WriteLine("... disconnected.");
                return;
            }

        }
        public void doConnect()
        {


            logger.Write("IRC Connecting ");
            reconTimer.Start();
            colourTimer.Start();

            if (irc.IsConnected)
            {
                logger.WriteLine("...  already connected.");
                return;
            }

            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (Exception ex) { logger.WriteLine("IRC CONNECT FAILED: " + ex.Message); }

            if (!irc.IsConnected)
            {
                logger.WriteLine("... IRC seems to have failed to connect :( ;~; D: ");
            }
            else
            {
                logger.WriteLine("... Connected! Timers resuming...");
                reconTimer.Start();

            }
        }
        public void doReconnect()
        {
            doDisconnect();
            doConnect();
        }
        public void doReconnect2()
        {
            irc.Reconnect(true, true);
        }
        public void ircConnecting(object sender, EventArgs e)
        {
            logger.WriteLine("ircConnecting()");
            try
            {
                one.Abort();
            }
            catch { } // ignore this if it fails, because i'm lazy --bob

            one = new Thread(connection);
            one.Name = "SAYINGSBOT IRC CONNECTION";
            one.IsBackground = true;

            logger.WriteLine("Thread \"one\" recreated...");

        }
        public void ircConnected(object sender, EventArgs e)
        {
            logger.WriteLine("ircConnected()");
            logger.WriteLine("IRC: Joining Twitch chat");
            irc.Login(bot_name, "SAYINGSBOT", 0, bot_name, oauth);
            one.Start();
        }
        public void ircDisconnecting(object sender, EventArgs e)
        {
            logger.WriteLine("ircDisconnecting()");

        }
        public void ircDisconnected(object sender, EventArgs e)
        {
            logger.WriteLine("ircDisconnected()");
            try
            {
                one.Abort();
            }
            catch { }
        }
        [System.Obsolete("Unused - No Code")]
        public void ircConError(object sender, EventArgs e)
        {
        }
        [System.Obsolete("Unused - No Code")]
        public void ircError(object sender, EventArgs e)
        {
        }
        public void ircRaw(object sender, IrcEventArgs e)
        {
            if (logLevel == 3)
            {
                logger.WriteLine("IRC RAW:<- " + e.Data.RawMessage);
            }
        }
        public void ircNotice(object sender, IrcEventArgs e)
        {
            if (logLevel < 3 && logLevel > 0)
            {
                logger.WriteLine("IRC NOTICE: " + e.Data.Message);
            }
            if (e.Data.Message == "Error logging in")
            {
                logger.WriteLine("IRC: SEVERE: Unsuccesful login, please check the username and oauth.");
            }
        }
        public void ircChanMess(object sender, IrcEventArgs e)
        {
            bool a = false;
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            nick = nick.ToLower();
            string message = e.Data.Message;
            message = message.ToLower();
            storeMessage(nick, message);
            if (message.StartsWith("!")) { } else { commands.addPoints(nick, 2); commands.addAllTime(nick, 2); }
            //try
            //{
                if (logLevel == 2) { logger.WriteLine(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + " IRC: <-" + channel + ": <" + nick + "> " + message); }
                message = message.TrimEnd();
                if (antispam) { if (isMod) { a = checkSpam(channel, nick, message); } };
                if (!a)
                {
                    message = filter(message);
                    if (Regex.Match(message, "^wh?at'?s?( is)? the point", RegexOptions.IgnoreCase).Success)
                    {
                        sendMess(channel, "No point, only play.");
                    }
                    else
                    {
                        this.checkProfanity(message, nick);
                        this.checkCommand(channel, nick, message);
                        this.checkAlias(channel, nick, message);
                    }
                }
            /*}
            catch (Exception eee)
            {
                logger.WriteLine("IRC: Crisis adverted: <"+nick+"> "+message);
                this.appendFile(progressLogPATH, "IRC: Crisis adverted: <" + nick + "> " + message);
                Console.Write(eee);
            }*/
        }
        public void ircChanActi(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            nick = nick.ToLower();
            string message = e.Data.Message;
            message = message.ToLower();
            commands.addPoints(nick, 2); commands.addAllTime(nick, 2);
            message = message.Remove(0, 8);
            message = message.Remove(message.Length - 1);
            if (logLevel == 2) { logger.WriteLine("<-" + channel + ": " + nick + " " + message); }
        }
        public void ircQuery(object sender, IrcEventArgs e)
        {
            string str = e.Data.Message;
            if (str.StartsWith("The moderators"))
            {
                str = str.Substring("the moderators of this room are: ".Length);
                string[] splt = str.Split(new string[] { ", " }, StringSplitOptions.None);
                isMod = false;
                foreach (string moderator in splt)
                {
                    if (moderator == bot_name)
                    {
                        isMod = true;
                    }
                    else
                    {
                        if (pullAuth(moderator) < 4)
                        {
                            setAuth(moderator, 3);
                        }
                    }
                }

            }
        }
        #endregion
        [System.Obsolete("Unused")]
        public void safe()//saves all data
        {
            /* No clue what this is, should have been gone ages ago.
            string temp;
            temp = "";
            foreach (command c in comlist)
            {
                temp += c.ToString() + "\n";
            }
            writeFile("Commands.twirc", temp);
            temp = "";
            foreach (ali c in aliList)
            {
                temp += c.ToString() + "\n";
            }
            writeFile("Aliases.twirc", temp);

            //rewriting it
            foreach (command c in comlist)
            {
                string str = "UPDATE commands SET authlevel='" + c.getAuth() + "',count='" + c.getCount() + "',response='" + MySqlEscape(c.getResponse()) + "' WHERE keyword='" + c.getKey() + "'; ";
                new SQLiteCommand(str , dbConn).ExecuteNonQuery();
            }
            */
        }
        public static string MySqlEscape(string usString)
        {
            if (usString == null)
            {
                return null;
            }
            // it escapes \r, \n, \x00, \x1a, baskslash, single quotes, double quotes and semi colons
            return Regex.Replace(usString, "([\\r\\n\\x00\\x1a\\\'\";])", "\\$1");
        }
        public string[] FileLines(string path)
        {
            try
            {
                StreamReader a = File.OpenText(path);
                List<string> lst = new List<string>();
                string str;
                while ((str = a.ReadLine()) != null)
                {
                    lst.Add(str);
                }
                a.Close();
                string[] ret = lst.ToArray();
                return ret;
            }
            catch { return null; }
        }
        public bool writeFile(string path, string stuff)
        {
            try
            {
                File.WriteAllText(path, stuff);

                return true;
            }
            catch { return false; }
        }
        public bool appendFile(string path, string stuff)
        {
            try
            {
                File.AppendAllText(path, Environment.NewLine + stuff);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Gets the system path of the EXE.
        /// </summary>
        /// <returns>A system path.</returns>
        public string sysPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }
        /// <summary>
        /// Gets the system path of the EXE.
        /// </summary>
        /// <returns>A system path.</returns>
        public static string sysPathStatic()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }
        /// <summary>
        /// Converts an int to a string.
        /// </summary>
        public string cstr(int i)
        {
            return Convert.ToString(i);
        }
        /// <summary>
        /// Converts a string to an int.
        /// </summary>
        public Int32 cint(string i)
        {
            return Convert.ToInt32(i);
        }

        /// <summary>
        /// Checks for profanity, then subtracts points accordingly.
        /// </summary>
        /// <param name="message">The message to check for swears in,</param>
        /// <param name="name">The user sending the message.</param>
        private void checkProfanity(string message, string name)
        {
            if (pf.isProfanity(message))
            {
                //sendMess(channels, "PROFANITY!");
                commands.addPoints(name, -10, "Profanity");
                SQLiteDataReader read = new SQLiteCommand("SELECT data FROM userdata WHERE user='swearJar' AND datatype = '6';", dbConn).ExecuteReader();
                if (read.Read())
                {
                    int currentJar = cint(read.GetString(0));
                    currentJar += 10;
                    new SQLiteCommand("UPDATE userdata SET data='"+currentJar+"' WHERE user='swearJar' AND datatype='6';", dbConn).ExecuteNonQuery();
                }
                else
                {
                    new SQLiteCommand("INSERT INTO userdata (user, datatype, data) VALUES ('swearJar', '6', '10');", dbConn).ExecuteNonQuery();
                }
            }
        }
    }

}