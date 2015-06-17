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
        public string progressLogPATH = sysPath() + "\\SayingsBotLog.txt";

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
            hardList.Add(new hardCom("!alltimefix", 0, 0));

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

            try
            {
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch { }
        }

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
            catch{} // ignore this if it fails, because i'm lazy --bob

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
            string[] str, tempVar3;
            bool done = false; int auth = pullAuth(user);
            bool fail; int tempVar1 = 0; string tempVar2 = "";
            string User = user.Substring(0, 1).ToUpper() + user.Substring(1);
            user = user.ToLower();
            message = message.ToLower();
            foreach (hardCom h in hardList)//hardcoded command
            {
                if (h.hardMatch(user,message,auth))
                {
                    done = true;
                    str = h.returnPars(message);
                    if (logLevel == 1) { logger.WriteLine("IRC:<- <"+user +"> " + message); }

                    if (h.returnKeyword() != "!sbgetuseraliases")
                    {
                    for (int it = 0; it < str.Length; it++)
                    {
                        if (getUserFromAlias(str[it]) != null)
                        {
                            string tmp = getUserFromAlias(str[it]);
                            str[it] = tmp;
                        }
                    }
                    }
                    switch (h.returnKeyword())
                    {
                            case "!alltimefix":
                            SQLiteDataReader reader = new SQLiteCommand("SELECT * FROM users", dbConn).ExecuteReader();
            while (reader.Read())
            {
                string name = reader.GetString(0);
                int points = reader.GetInt32(3);
                int alltime = reader.GetInt32(4);

                if (points > alltime)
                {
                    new SQLiteCommand("UPDATE users SET alltime='" + points + "' WHERE name='" + name + "';", dbConn);
                   logger.logAppendLine("Updated " + name);
                }
                else
                {
                    logger.logAppendLine("No update for " + name);
                }
            }
                                break;
                            case "!sbaddcom":
                                fail = false;

                                foreach (command c in comlist) { if (c.doesMatch(str[1])) { fail = true; break; } }
                                foreach (hardCom c in hardList) { if (c.doesMatch(str[1]) || fail) { fail = true; break; } }
                                foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; } }
                                if (fail) { sendMess(channel, "I'm sorry, " + User + ". A command or alias with the same name exists already."); }
                                else
                                {
                                    tempVar1 = 0;
                                    if (Regex.Match(str[2], @"@level(\d)@").Success) { tempVar1 = int.Parse(Regex.Match(str[2], @"@level(\d)@").Groups[1].Captures[0].Value); tempVar2 = str[3]; if (tempVar1 >= 5) { tempVar1 = 5; } }
                                    else { tempVar2 = str[2] + " " + str[3]; }
                                    tempVar3 = tempVar2.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    comlist.Add(new command(str[1], tempVar3, tempVar1));
                                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO commands (keyword,authlevel,count,response) VALUES (@par1,'" + tempVar1 + "','" + 0 + "',@par2);", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", str[1].ToLower());
                                    cmd.Parameters.AddWithValue("@par2", tempVar2);
                                    cmd.ExecuteNonQuery();
                                    sendMess(channel, User + " -> command \"" + str[1] + "\" added. Please try it out to make sure it's correct.");
                                    this.appendFile(progressLogPATH, "Command \"" + str[1] + "\" added.");
                                }
                                break;
                            case "!sbeditcom":
                                fail = true;
                                for (int a = 0; a < comlist.Count() && fail; a++)
                                {
                                    if (comlist[a].doesMatch(str[1]))
                                    {
                                        tempVar1 = 0;
                                        if (Regex.Match(str[2], @"@level(\d)@").Success) { tempVar1 = int.Parse(Regex.Match(str[2], @"@level(\d)@").Groups[1].Captures[0].Value); tempVar2 = str[3]; if (tempVar1 >= 5) { tempVar1 = 5; } }
                                        else { tempVar2 = str[2] + " " + str[3]; }
                                        tempVar3 = tempVar2.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                                        comlist[a].setResponse(tempVar3);
                                        comlist[a].setAuth(tempVar1);
                                        SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET response = @par1 authlevel=@par3 WHERE keyword=@par2;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", tempVar2); cmd.Parameters.AddWithValue("@par2", str[1]); cmd.Parameters.AddWithValue("@par3", tempVar1);
                                        cmd.ExecuteNonQuery();
                                        sendMess(channel, User + "-> command \"" + str[1] + "\" has been edited.");
                                        this.appendFile(progressLogPATH, "Command \"" + str[1] + "\" has been edited.");
                                        safe();
                                        fail = false;
                                    }
                                }
                                if (fail)
                                {
                                    sendMess(channel, "I'm sorry, " + User + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                                break;
                            case "!sbdelcom"://delete command
                                fail = true;
                                for (int a = 0; a < comlist.Count() && fail; a++)
                                {
                                    if (comlist[a].doesMatch(str[1]))
                                    {
                                        comlist.RemoveAt(a);
                                        fail = false;
                                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM commands WHERE keyword=@par1;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", str[1]);
                                        cmd.ExecuteNonQuery();
                                        sendMess(channel, User + "-> command \"" + str[1] + "\" has been deleted.");
                                        this.appendFile(progressLogPATH, "Command \"" + str[1] + "\" has been deleted.");
                                        safe();
                                        break;
                                    }

                                }
                                if (fail)
                                {
                                    sendMess(channel, "I'm sorry, " + User + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                                break;
                            case "!sbaddalias": //add alias
                                fail = false;
                                foreach (command c in comlist) { if (c.doesMatch(str[1])) { fail = true; break; } }
                                foreach (hardCom c in hardList) { if (c.doesMatch(str[1]) || fail) { fail = true; break; } }
                                foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; } }
                                if (fail) { sendMess(channel, "I'm sorry, " + user + ". A command or alias with the same name exists already."); }
                                else
                                {
                                    fail = true;
                                    foreach (ali c in aliList)
                                    {
                                        if (c.getTo() == str[2]) { c.addFrom(str[1]); fail = false; break; }
                                    }
                                    if (fail) { aliList.Add(new ali(str[1], str[2])); }
                                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO aliases (keyword,toword) VALUES (@par1,@par2);", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", str[1]); cmd.Parameters.AddWithValue("@par2", str[2]);
                                    cmd.ExecuteNonQuery();
                                    sendMess(channel, User + " -> alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                                    this.appendFile(progressLogPATH, "Alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                                    safe();
                                }
                                break;
                            case "!sbdelalias":
                                fail = true;
                                foreach (ali c in aliList)
                                {
                                    if (c.delFrom(str[1]))
                                    {
                                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM aliases WHERE keyword=@par1;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", str[1]); cmd.ExecuteNonQuery();
                                        sendMess(channel, user + " -> Alias \"" + str[1] + "\" removed.");
                                        this.appendFile(progressLogPATH, "Alias \"" + str[1] + "\" removed.");
                                        if (c.getFroms().Count() == 0) { aliList.Remove(c); }
                                        fail = false;
                                        safe();
                                        break;
                                    }
                                }
                                if (fail) { sendMess(channel, "I'm sorry, " + User + ". I couldn't find any aliases that match it. (maybe it's a command?)"); }
                                break;
                            case "!sbset"://!set <name> <level>
                                if (!Regex.Match(str[1].ToLower(), @"^[a-z0-9_]+$").Success) { sendMess(channel, "I'm sorry, " + User + ". That's not a valid name."); }
                                else
                                {
                                    if (Regex.Match(str[2], "^([0-" + auth + "])|(-1)$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
                                    {
                                        setAuth(str[1].ToLower(), int.Parse(str[2]));
                                        sendMess(channel, user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ".");
                                        this.appendFile(progressLogPATH, str[1] + " was given auth level " + str[2] + ".");
                                        safe();
                                    }
                                    else
                                    {
                                        sendMess(channel, "I'm sorry, " + User + ". You either lack the authorisation to give such levels to that person, or that level is not a valid number.");
                                    }
                                }
                                break;
                            case "!sbeditcount":
                                fail = true;
                                if (!Regex.Match(str[2], @"^\d+$").Success)
                                {
                                    break;
                                }
                                foreach (command c in comlist)
                                {
                                    if (c.doesMatch(str[1]))
                                    {
                                        fail = false;
                                        c.setCount(int.Parse(str[2]));
                                        SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET count='" + str[2] + "' WHERE keyword = @par1;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", str[1]);
                                        cmd.ExecuteNonQuery();
                                        sendMess(channel, user + "-> the count of \"" + str[1] + "\" has been updated to " + str[2] + ".");
                                        safe();
                                    }
                                }
                                break;
                            case "!banuser":
                                if (auth > pullAuth(str[1]))//should prevent mods from banning other mods, etc.
                                {
                                    setAuth(str[1], -1);
                                    sendMess(channel, User + "-> \"" + str[1] + "\" has been banned from using bot commands.");
                                    this.appendFile(progressLogPATH, str[1] + " has been banned from using bot commands.");
                                }

                                break;
                            case "!unbanuser":
                                if (pullAuth(str[1]) == -1)
                                {
                                    setAuth(str[1], 0);
                                    sendMess(channel, User + "-> \"" + str[1] + "\" has been unbanned.");
                                    this.appendFile(progressLogPATH, str[1] + "has been unbanned.");
                                }

                                break;
                            case "!sbsilence":
                                if (Regex.Match(str[1], "(on)|(off)|1|0|(true)|(false)|(yes)|(no)", RegexOptions.IgnoreCase).Success) { sendMess(channel, "Silence has been set to: " + str[1]); }
                                if (Regex.Match(str[1], "(on)|1|(true)|(yes)", RegexOptions.IgnoreCase).Success) { silence = true; new SQLiteCommand("UPDATE settings SET silence=1;", dbConn).ExecuteNonQuery(); }
                                if (Regex.Match(str[1], "(off)|0|(false)|(no)", RegexOptions.IgnoreCase).Success) { silence = false; new SQLiteCommand("UPDATE settings SET silence=0;", dbConn).ExecuteNonQuery(); }

                                break;

                            case "!sbrank":
                                string text = "";
                                switch (auth)
                                {
                                    case 0: text = "an user"; break;
                                    case 1: text = "a regular"; break;
                                    case 2: text = "trusted"; break;
                                    case 3: text = "a moderator"; break;
                                    case 4: text = "the broadcaster"; break;
                                    case 5: text = "an administrator of " + bot_name; break;
                                    default: text = "special"; break;
                                }
                                sendMess(channel, User + ", you are " + text + ".");
                                break;
                            /*
                            case "!permit":
                                if (antispam)
                                {
                                    permits.Add(new intStr(str[1], getNow()));
                                    sendMess(channel, str[1].Substring(0, 1).ToUpper() + str[1].Substring(1) + ", you have been granted permission to post a link by " + User+". This permit expires in "+permitTime+" seconds.");
                                }
                                break;

                            case "!whitelist":
                                if (auth == 3 && message.Split(' ').Count()>=3)
                                {
                                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO aswhitelist (name,regex) VALUES (@par1,@par2) VALUES",dbConn);
                                    cmd.Parameters.AddWithValue("@par1", message.Split(' ')[1]);
                                    cmd.Parameters.AddWithValue("@par2", message.Split(new string[] { " " }, 3, StringSplitOptions.None)[2]);
                                    cmd.ExecuteNonQuery();
                                    asWhitelist.Add(message.Split(new string[] { " " }, 3, StringSplitOptions.None)[2]);
                                    asWhitelist2.Add(message.Split(' ')[1]);
                                    sendMess(channel, User + "-> I've added it to the whitelist, I can't guarantee any results.");
                                }
                                else
                                {
                                    if(asWhitelist.Count == 0)
                                    {
                                        tempVar2 = "There are no whitelisted links.";
                                    }
                                    if (asWhitelist.Count == 1)
                                    {
                                        tempVar2 = "The only whitelisted website is " + asWhitelist2[0];
                                    }
                                    if (asWhitelist.Count > 1)
                                    {
                                        tempVar2 = "Whitelisted websites are: ";
                                        foreach (string tempStr1 in asWhitelist2)
                                        {

                                            tempVar2 += tempStr1 + ", ";
                                        }
                                        tempVar2 = tempVar2.Substring(0, tempVar2.Length - 2);
                                        tempVar2 += ".";
                                    }
                                    sendMess(channel, tempVar2);
                                }
                                break;*/
                            case "!commands":
                                sendMess(channel, User + " --> SayingsBot Commands at http://moddedbydude.net76.net/wiki/index.php/SayingsBot#Commands");
                                break;
                            case "!givecake":
                                sendMess(channel, User + " gives " + str[1] + " some cake!");
                                break;
                            case "!givepie":
                                sendMess(channel, User + " gives " + str[1] + " some pie!");
                                break;
                            case "!givea":
                                sendMess(channel, User + " gives " + str[1] + " a " + str[2] + "!");
                                break;
                            case "!givesome":
                                sendMess(channel, User + " gives " + str[1] + " some " + str[2] + "!");
                                break;
                            case "!classicwhoisuser":
                                sendMess(channel, getClassicWhoIs(str[1]));
                                break;
                            case "!whoisuser":
                                SQLiteCommand userCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '0';", dbConn);
                                userCommand.Parameters.AddWithValue("@par1", str[1].ToLower());
                                SQLiteDataReader userReader = userCommand.ExecuteReader();
                                if (userReader.Read()) { sendMess(channel, userReader.GetString(0)); } else { sendMess(channel, str[1] + " does not have a !whoisuser."); }
                                break;
                            case "!editme":
                                string newText = str[1];
                                setWhoIsUser(user, newText);
                                SQLiteCommand usersCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '0';", dbConn);
                                usersCommand.Parameters.AddWithValue("@par1", str[1].ToLower());
                                SQLiteDataReader usersReader = usersCommand.ExecuteReader();
                                if (usersReader.Read())
                                {
                                    sendMess(channel, User + " your !whoisuser now reads as: " + usersReader.GetString(0));
                                    this.appendFile(progressLogPATH, User + " your !whoisuser now reads as: " + usersReader.GetString(0));
                                }
                                break;
                            case "!edituser":
                                string newUser = str[1];
                                string newTextEU = str[2];
                                setWhoIsUser(newUser, newTextEU);
                                SQLiteCommand userssCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '0';", dbConn);
                                userssCommand.Parameters.AddWithValue("@par1", newUser.ToLower());
                                SQLiteDataReader userssReader = userssCommand.ExecuteReader();
                                if (userssReader.Read())
                                {
                                    sendMess(channel, newUser + "'s !whoisuser now reads as: " + userssReader.GetString(0));
                                    this.appendFile(progressLogPATH, User + " your !whoisuser now reads as: " + userssReader.GetString(0));
                                }
                                break;
                            case "!classic":
                                //TODO: Need to SQL this
                                SQLiteCommand classicCmd = new SQLiteCommand("SELECT data FROM userdata WHERE user = @par1 AND dataType = '4';", dbConn);
                                classicCmd.Parameters.AddWithValue("@par1", str[1]);
                                SQLiteDataReader classicReader = classicCmd.ExecuteReader();
                                if (classicReader.Read())
                                {
                                    sendMess(channel, classicReader.GetString(0));
                                }
                                else
                                {
                                    sendMess(channel, "Classic " + str[1] + " does not exist!");
                                }
                                break;
                            case "!addclassic":
                                string classicAdd = str[1];
                                string classicMessage = str[2];
                                SQLiteCommand classicAddCommand = new SQLiteCommand("INSERT INTO userdata (user, datatype, data) VALUES (@par1, '4', @par2);", dbConn);
                                classicAddCommand.Parameters.AddWithValue("@par1", classicAdd);
                                classicAddCommand.Parameters.AddWithValue("@par2", classicMessage);
                                classicAddCommand.ExecuteNonQuery();
                                sendMess(channel, "Classic command " + classicAdd + " appears as " + classicMessage);
                                this.appendFile(progressLogPATH, "Classic command " + classicAdd + " added.");
                                break;
                            case "!kill":
                                sendMess(channel, User + " politley murders " + str[1] + ".");
                                break;
                            case "!calluser":
                                sendMess(channel, "CALLING " + str[1].ToUpper() + "! WOULD " + str[1].ToUpper() + " PLEASE REPORT TO THE CHAT!");
                                break;
                            case "!count":
                                SQLiteDataReader getCountCmd = new SQLiteCommand("SELECT DATA FROM misc WHERE ID = 'CountGame';", dbConn).ExecuteReader(); //needs to be put into int count
                                int count;
                                if (getCountCmd.Read())
                                {
                                    count = int.Parse(getCountCmd.GetString(0));
                                    count++;
                                    new SQLiteCommand("UPDATE misc SET DATA = '" + count + "' WHERE ID='CountGame';", dbConn).ExecuteNonQuery();
                                }
                                else//countgame doesn't exist?
                                {
                                    count = 1;
                                    new SQLiteCommand("INSERT INTO misc (ID,DATA) VALUES ('CountGame','1');", dbConn).ExecuteNonQuery();
                                }
                                sendMess(channel, cstr(count));
                                break;
                            case "!newcount":
                                new SQLiteCommand("UPDATE misc SET DATA = '0' WHERE ID='CountGame';", dbConn).ExecuteNonQuery();
                                sendMess(channel, "Counting Game Reset");
                                break;
                            case "!points":
                                sendMess(channel, user + " you have " + getPoints(user) + " points. (" + getAllTime(user) + ")");
                                break;
                            case "!seepoints":
                                sendMess(channel, str[1] + " has " + getPoints(str[1]) + " points. (" + getAllTime(user) + ")");
                                break;
                            case "!setpoints":
                                setPoints(str[1], cint(str[2]));
                                sendMess(channel, str[1] + "'s points set to " + str[2] + " points.");
                                break;
                            case "!addpoints":
                                addPoints(str[1], cint(str[2]), "Manual Add");
                                addAllTime(str[1], cint(str[2]));
                                sendMess(channel, str[1] + " gained " + str[2] + " points.");
                                break;
                            case "!nc":
                                sendMess(channel, str[1] + "! Please change the color of your name, neon colors hurt some peoples eyes! (If you don't know how type \".color\")");
                                break;
                            case "!sbversion":
                                sendMess(channel, "/me is currently HB" + Application.ProductVersion + " bassed off of SB2.8.2. Changelog at http://moddedbydude.net76.net/wiki/index.php/SayingsBot#ChangeLog");
                                break;
                            case "!sbleaderboard":
                                SQLiteDataReader sqldr;
                                string messString = "Leaderboard: ";
                                sqldr = new SQLiteCommand("SELECT name,alltime FROM users ORDER BY alltime DESC,name LIMIT 5;", dbConn).ExecuteReader();
                                while (sqldr.Read())
                                {
                                    messString += sqldr.GetString(0) + " with " + sqldr.GetInt32(1) + " points. ";
                                }
                                sendMess(channel, messString);
                                break;
                            case "!sqlquery":
                                break;
                            case "sayingsbot":
                                sendMess(channel, "/me reporting, " + user + "!");
                                break;


                            case "!sbadduseralias":
                                setUserAlias(str[1], str[2]);
                                sendMess(channel, "Gave user " + str[1] + " the alias " + str[2] + ".");
                                break;
                            case "!sbgetuseraliases":
                                if (getUserAlias(str[1]) != null)
                                {
                                    sendMess(channel, "User " + str[1] + " has the aliases " + getUserAlias(str[1]) + ".");
                                }
                                else
                                {
                                    sendMess(channel, "User " + str[1] + " has no aliases.");
                                }
                                break;
                            case "!swearjar":
                                SQLiteDataReader tmp;
                                tmp = new SQLiteCommand("SELECT data FROM userdata WHERE user='swearJar' AND datatype='6';", dbConn).ExecuteReader();
                                if (tmp.Read())
                                {
                                    int tmpI = cint(tmp.GetString(0));
                                    sendMess(channel, "There is currently " +tmpI + " points in the swear jar.");
                                }
                                break;
                            case "!quotes":
                                Random rnd = new Random();
                                string function = str[1];
                                string quser = str[2];
                                string fParam = str[3];
                                Boolean success = false;
                                SQLiteDataReader quotesReader;
                                SQLiteCommand quotesCommand;
                                quser = quser.ToLower();

                                if (function == "info")
                                {
                                    sendMess(channel, "Infomation about quotes avalible on the !sbcommands page.");
                                    break;
                                }
                                else if (function == "say")
                                {
                                    if (quser == "random")
                                    {
                                        //randomquote
                                        quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = 'overallRandom' AND dataType = '5' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                        if (quotesReader.Read())
                                        {
                                            int ubound = quotesReader.GetInt32(0);
                                            int rand = rnd.Next(1, ubound + 1);
                                            quotesReader = new SQLiteCommand("SELECT data FROM userdata WHERE user = 'overallRandom' AND dataType = '5' AND dataID ='" + rand + "';", dbConn).ExecuteReader();
                                            if (quotesReader.Read())
                                            {
                                                string theuser = quotesReader.GetString(0);
                                                quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = '" + theuser + "' AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                                if (quotesReader.Read())
                                                {
                                                    rand = rnd.Next(1, quotesReader.GetInt32(0) + 1);
                                                    quotesReader = new SQLiteCommand("SELECT data FROM userdata WHERE user='" + theuser + "' AND datatype = '1' AND dataID = '" + rand + "';", dbConn).ExecuteReader();
                                                    if (quotesReader.Read()) { sendMess(channel, theuser + "(" + rand + "): " + quotesReader.GetString(0)); } else { sendMess(channel, "Quote " + rand + " does not exist for " + quser); }
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        if (fParam == "r")
                                        {
                                            //random user quote
                                            quotesCommand = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = @par1 AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn);
                                            quotesCommand.Parameters.AddWithValue("@par1", quser);
                                            quotesReader = quotesCommand.ExecuteReader();

                                            if (quotesReader.Read())
                                            {
                                                int rand = rnd.Next(1, quotesReader.GetInt32(0) + 1);
                                                quotesCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '1' AND dataID = '" + rand + "';", dbConn);
                                                quotesCommand.Parameters.AddWithValue("@par1", quser);
                                                quotesReader = quotesCommand.ExecuteReader();
                                                if (quotesReader.Read()) { sendMess(channel, "(" + rand + "): " + quotesReader.GetString(0)); } else { sendMess(channel, "Quote " + rand + " does not exist for " + quser); }
                                                break;
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            //specific user quote
                                            try { int.Parse(fParam); success = true; }
                                            catch { }
                                            if (success)
                                            {
                                                quotesCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '1' AND dataID = @par2;", dbConn);
                                                quotesCommand.Parameters.AddWithValue("@par1", quser);
                                                quotesCommand.Parameters.AddWithValue("@par2", fParam);
                                                quotesReader = quotesCommand.ExecuteReader();
                                                if (quotesReader.Read()) { sendMess(channel, quotesReader.GetString(0)); } else { sendMess(channel, "Quote " + fParam + " does not exist for " + quser); }
                                                break;
                                            }
                                            else
                                            {
                                                sendMess(channel, "Quote " + fParam + " does not exist for " + quser + ".");
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (function == "add")
                                {
                                    int length;
                                    int newLength;
                                    quotesCommand = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = @par1 AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn);
                                    quotesCommand.Parameters.AddWithValue("@par1", quser);
                                    quotesReader = quotesCommand.ExecuteReader();
                                    if (quotesReader.Read())
                                    {
                                        length = quotesReader.GetInt32(0);
                                        newLength = length + 1;
                                    }
                                    else
                                    {
                                        //No quotes yet exsist
                                        newLength = 1;
                                    }
                                    quotesCommand = new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES (@par1, '1', '" + newLength + "',  @par2);", dbConn);
                                    quotesCommand.Parameters.AddWithValue("@par1", quser);
                                    quotesCommand.Parameters.AddWithValue("@par2", fParam);
                                    quotesCommand.ExecuteNonQuery();
                                    sendMess(channel, "Quote " + cstr(newLength) + " for " + quser + " has been added as: " + fParam);
                                    this.appendFile(progressLogPATH, "Quote " + cstr(newLength) + " for " + quser + " has been added as: " + fParam);

                                    //If it's the user's first quote
                                    //Originally addusertorandom
                                    if (newLength == 1)
                                    {
                                        quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = 'overallRandom' AND datatype = '5' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                        if (quotesReader.Read())
                                        {
                                            length = quotesReader.GetInt32(0);
                                            newLength = length + 1;
                                            quotesCommand = new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES ('overallRandom', '5', '" + newLength + "', @par1);", dbConn);
                                            quotesCommand.Parameters.AddWithValue("@par1", quser);
                                            quotesCommand.ExecuteNonQuery();
                                            sendMess(channel, "Added " + quser + " to overall random list. They are user " + cstr(newLength) + ".");
                                            this.appendFile(progressLogPATH, "Added " + quser + " to overall random list. They are user " + cstr(newLength) + ".");
                                        }
                                        break;
                                    }
                                    else { break; }
                                }
                                else if (function == "edit")
                                {
                                    sendMess(channel, "Editing quotes unimplimented. Bug dude to change it.");
                                    break;
                                }
                                /*else if (function == "addusertorandom")
                                {
                                    quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = 'overallRandom' AND datatype = '5' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                    if (quotesReader.Read())
                                    {
                                        int length = quotesReader.GetInt32(0);
                                        int newLength = length + 1;
                                        new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES ('overallRandom', '5', '" + newLength + "', '"+quser+"');", dbConn).ExecuteNonQuery();
                                        sendMess(channel, "Added " + quser + " to overall random list. They are user " + cstr(newLength) + ".");
                                        this.writeFile(progressLogPATH, "Added " + quser + " to overall random list. They are user " + cstr(newLength) + ".");
                                    }
                                    break;
                                }*/
                                else
                                {
                                    sendMess(channel, "Incorrect use of !quotes.");
                                    break;
                                }
                            //insert your commands here.
                        }
                    break;


                }
            }
            //I guess you'd want to put something like this for classics, so I left it.
            if (!done)
            {
                foreach (command c in comlist)//flexible commands
                {
                    if (c.doesMatch(message) && c.canTrigger() && c.getAuth() <= auth)
                    {
                        if (logLevel == 1) { logger.WriteLine("IRC:<- <" + user +">" +message); }
                        str = c.getResponse(message, user);
                        c.addCount(1);
                        SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET count = '" + c.getCount() + "' WHERE keyword = @par1;", dbConn);
                        cmd.Parameters.AddWithValue("@par1",c.getKey());
                        cmd.ExecuteNonQuery();
                        if (str.Count() != 0) { if (str[0] != "") { c.updateTime(); } }
                        foreach (string b in str)
                        {
                            sendMess(channel, b);
                        }
                    }
                }
             }
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
        /// <summary>
        /// Set the !whoisuser message of a user.
        /// </summary>
        /// <param name="user">The user to set the message for.</param>
        /// <param name="message">The message to set to the user.</param>
        public void setWhoIsUser(string user, string message)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM userdata WHERE user='" + user + "' AND datatype='0';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE userdata SET data='" + message + "' WHERE user='" + user + "' AND datatype='0';", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO userdata (user,dataType,data) VALUES ('" + user + "','0','" + message + "');", dbConn).ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Gets the user's current points.
        /// </summary>
        /// <param name="name">The user to get the points for.</param>
        /// <returns>The user's current points.</returns>
        public int getPoints(string name)
        {
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

        /// <summary>
        /// Manually sets the user's points to a specific amount.
        /// </summary>
        /// <param name="user">The user to set points for.</param>
        /// <param name="amount">The amount to set.</param>
        public void setPoints(string user, int amount)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET points='" + amount + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + user + "','" + amount + "','FORCED CHANGE TO AMOUNT','"+sqldr.GetInt32(0)+"','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + user + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds to the user's current points.
        /// </summary>
        /// <param name="name">The user to add points to.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Amount added.</returns>
        public int addPoints(string name, int amount)
        {
            return addPoints(name, amount, null);
        }

        /// <summary>
        /// Adds to the user's current points.
        /// </summary>
        /// <param name="name">The user to add points to.</param>
        /// <param name="amount">The amount to add.</param>
        /// <param name="why">The reason for adding.</param>
        /// <returns>Amount added.</returns>
        public int addPoints(string name, int amount,string why)
        {
            int things;
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                new SQLiteCommand("UPDATE users SET points='"+ (things+amount)+"' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                if (why != null)
                {
                    new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + name + "','" + amount + "','" + why + "','" + things + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                }
                return things;
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','"+amount+"');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        /// <summary>
        /// Adds points to the user's all time amount.
        /// </summary>
        /// <param name="name">The user to preform the addition on.</param>
        /// <param name="amount">The ammount to add.</param>
        /// <returns>The amount added.</returns>
        public int addAllTime(string name, int amount)
        {
            int things;
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                new SQLiteCommand("UPDATE users SET alltime=alltime+"+amount+" WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return things;
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','"+amount+"');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        /// <summary>
        /// Gets the user's all-time points.
        /// </summary>
        /// <param name="name">The user to check for.</param>
        /// <returns>The user's all-time points in an int.</returns>
        public int getAllTime(string name)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                return sqldr.GetInt32(0);
            }
            else return 0;
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
            string message = e.Data.Message;
            storeMessage(nick, message);
            if (message.StartsWith("!")) { } else { addPoints(nick, 2); addAllTime(nick, 2); }
            try
            {
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
            }
            catch (Exception eee)
            {
                logger.WriteLine("IRC: Crisis adverted: <"+nick+"> "+message);
                this.appendFile(progressLogPATH, "IRC: Crisis adverted: <" + nick + "> " + message);
                Console.Write(eee);
            }
        }

        public void ircChanActi(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            addPoints(nick, 2); addAllTime(nick, 2);
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
        static string sysPath()
        {
            return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        }
        /// <summary>
        /// Converts an int to a string.
        /// </summary>
        string cstr(int i)
        {
            return Convert.ToString(i);
        }
        /// <summary>
        /// Converts a string to an int.
        /// </summary>
        Int32 cint(string i)
        {
            return Convert.ToInt32(i);
        }

        /// <summary>
        /// Sets a user's alias.
        /// </summary>
        /// <param name="user">The user to set the alias for.</param>
        /// <param name="alias">The alias to set.</param>
        void setUserAlias(string user, string alias)
        {
            string tmp;
            SQLiteCommand getUserAliasesCommand = new SQLiteCommand("SELECT alias FROM userAliases WHERE user=@par1;", dbConn);
            getUserAliasesCommand.Parameters.AddWithValue("@par1", user);
            SQLiteDataReader readAliases = getUserAliasesCommand.ExecuteReader();
            if (readAliases.Read())
            {
                tmp = readAliases.GetString(0);
            }
            else
            {
                tmp = null;
            }

            SQLiteCommand setAliasCommand;
            if (tmp != null) {
                setAliasCommand = new SQLiteCommand("UPDATE userAliases SET alias=@par2 WHERE user=@par1; ", dbConn);
            } else {
                setAliasCommand = new SQLiteCommand("INSERT INTO userAliases (user, alias) VALUES (@par1, @par2); ", dbConn);
            }
            setAliasCommand.Parameters.AddWithValue("@par1", user);
            setAliasCommand.Parameters.AddWithValue("@par2", alias);
            setAliasCommand.ExecuteNonQuery();
        }
        /// <summary>
        /// Get a user from an alias
        /// </summary>
        /// <param name="alias">The alias to check for.</param>
        /// <returns>The user, or null if it's not an alias.</returns>
        string getUserFromAlias(string alias)
        {
            SQLiteCommand getUserFromAliasCommand = new SQLiteCommand("SELECT user FROM userAliases WHERE alias=@par1;", dbConn);
            getUserFromAliasCommand.Parameters.AddWithValue("@par1", alias);
            SQLiteDataReader readAliases = getUserFromAliasCommand.ExecuteReader();
            if (readAliases.Read())
            {
                return readAliases.GetString(0);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Get's the user's alias.
        /// </summary>
        /// <param name="user">User to check for.</param>
        /// <returns>The alias, or null if there is none.</returns>
        string getUserAlias(string user)
        {
            SQLiteCommand getUserAliasesCommand = new SQLiteCommand("SELECT alias FROM userAliases WHERE user=@par1;", dbConn);
            getUserAliasesCommand.Parameters.AddWithValue("@par1", user);
            SQLiteDataReader readAliases = getUserAliasesCommand.ExecuteReader();
            if (readAliases.Read())
            {
                return readAliases.GetString(0);
            }
            else
            {
                return null;
            }
            
        }
        /// <summary>
        /// Checks through the classic !whoisuser's. (April/May 2014)
        /// </summary>
        /// <param name="user">The user to check for.</param>
        /// <returns>Classic !whoisuser message.</returns>
        string getClassicWhoIs(string user)
        {
            string[] lines = FileLines(sysPath() + "\\people.txt");
            int line = getClassicWhoIsLine(user, lines);
            if (line == -1)
            {
                return "That user did not classically have a !whoisuser";
            }
            else
            {
                return lines[line].Substring(user.Length + 1, lines[line].Length - (user.Length + 1));
            }
        }

        /// <summary>
        /// The line in the txt file that contains the user
        /// </summary>
        /// <param name="user">The user to check for.</param>
        /// <param name="lines">String[] of the text document.</param>
        /// <returns>The line, or -1 is there is none for the user.</returns>
        int getClassicWhoIsLine(string user, string[] lines) {
            for (int I = 0; I < lines.Length; I++)
            {
                if (lines[I].StartsWith(user))
                {
                    return I;
                }
            }
            return -1;
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
                this.addPoints(name, -10, "Profanity");
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