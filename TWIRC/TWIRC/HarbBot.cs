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
        #region vars and stuff
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
        public List<string> swearList = new List<string>();
        public List<string> classicList = new List<string>();
        public List<string[]> quotesList = new List<string[]>();
        public List<string[]> whoisList = new List<string[]>();
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

        ProfanityFilter pf;

#if DEBUG
        string commandsPATH = @"D:\SBcommands.html";
#else
        string commandsPATH = @"C:\Documents and Settings\Administrator\My Documents\Dropbox\New Folder 2\SBcommands.html";
#endif

        public bool shouldRebuildProf = false;
        public NetComm.Host Server;
        #endregion
        public HarbBot(Logger logLogger, NetComm.Host netCommServer)
        {
            Server = netCommServer;
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
            irc.OnJoin += new JoinEventHandler(ircWhoJoined);
            irc.OnPart += new PartEventHandler(ircWhoParted);



            //LoadCommands
            if (logLevel != 0)
            {
                writeLogger("IRC: Booting up, shouldn't take long!");
            }
            if (!File.Exists("db.sqlite"))
            {
                if (logLevel != 0)
                {
                    writeLogger("IRC: First time setup detected, making database");
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
                new SQLiteCommand("INSERT INTO userdata (user, datatype, data) VALUES ('ExampleUser', '0', 'This is ExampleUser !whoisuser message!');", dbConn).ExecuteNonQuery();
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
                writeLogger("IRC: Loaded " + comlist.Count() + " commands!");
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
                writeLogger("IRC: Loaded " + aliList.Count() + " aliases!");
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

            hardList.Add(new hardCom("!dudesmagiccommand", 5, 0));

            hardList.Add(new hardCom("!sbaddcom", 3, 2));//addcom (reduced now, so it doesn't conflict with nightbot)
            hardList.Add(new hardCom("!sbdelcom", 3, 1));//delcom
            hardList.Add(new hardCom("!sbeditcom", 3, 2));//editcom
            hardList.Add(new hardCom("!sbaddalias", 3, 2));//addalias
            hardList.Add(new hardCom("!sbdelalias", 3, 1));//delete alias
            
            hardList.Add(new hardCom("!sbset", 3, 2));//elevate another user
            hardList.Add(new hardCom("!sbeditcount", 3, 2));
            hardList.Add(new hardCom("!banuser", 3, 1));
            hardList.Add(new hardCom("!unbanuser", 4, 1));
            hardList.Add(new hardCom("!sbsilence",3,1));
            hardList.Add(new hardCom("!sbrank", -5, 0,60));
          
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
            hardList.Add(new hardCom("sayingsbot",0,0,20));
            hardList.Add(new hardCom("!sbadduseralias", 2, 2, 20));
            hardList.Add(new hardCom("!sbgetuseraliases", 2, 1, 20));
            hardList.Add(new hardCom("!swearjar", 0, 0));
            hardList.Add(new hardCom("!nightbotisdown", 0, 0));
            hardList.Add(new hardCom("!logbotisdown", 0, 0));
            hardList.Add(new hardCom("!addswear", 0, 1));
            hardList.Add(new hardCom("!lolcounter", 0, 0));
            hardList.Add(new hardCom("!howmanytimes", 0, 0));
            hardList.Add(new hardCom("!howfar", 0, 0));

            one = new Thread(connection);
            one.Name = "SAYINGSBOT IRC CONNECTION";
            one.IsBackground = true;

            
            saveTimer = new System.Timers.Timer(60 *1000);
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += saveTimer_Elapsed;
            saveTimer.Start();

            colourTimer = new System.Timers.Timer(10000);
            colourTimer.AutoReset = true;
            colourTimer.Elapsed += colourTimer_Elapsed;
            colourTimer.Start();

            reconTimer = new System.Timers.Timer(5000);
            reconTimer.AutoReset = true;
            reconTimer.Elapsed += reconTimer_Elapsed;
            reconTimer.Start();

            commands = new Commands(this, this.dbConn, this.hardList, this.comlist, this.aliList, this.logLevel, logger);
            pf = new ProfanityFilter(this);
            this.loadProfanity();
            commands.loadClassics(this.classicList);
            this.loadQuotesForHTML();
            this.loadWhoIsForHTML();

            if (logLevel != 0)
            {
                writeLogger("IRC: Loaded " + hardList.Count() + " hard-codded sayings!");
                writeLogger("IRC: Loaded " + swearList.Count() + " profaine sayings!");
                writeLogger("IRC: Loaded " + classicList.Count() + " classic sayings!");
                writeLogger("IRC: Loaded " + quotesList.Count() + " quoted sayings!");
                writeLogger("IRC: Loaded " + whoisList.Count() + " !Whoisuser sayings!");
            }

            try
            {
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch { }

            Server.StartConnection();
            Server.onConnection += new NetComm.Host.onConnectionEventHandler(Server_onConnection);
            Server.lostConnection += new NetComm.Host.lostConnectionEventHandler(Server_lostConnection);
            Server.DataReceived += new NetComm.Host.DataReceivedEventHandler(Server_DataReceived);

            saveTimer_Elapsed(null, null);
        }
        #region Sayingsbot Remote NetComm Server
        void Server_DataReceived(string ID, byte[] Data)
        {
            string rcvdData = ASCIIEncoding.ASCII.GetString(Data);
            string recived = (ID + ": " + rcvdData);
            writeLogger("Sayingsbot Remote: " + recived); //Updates the log when a new message arrived, converting the Data bytes to a string
            if (rcvdData.StartsWith("CHAT:"))
            {
                rcvdData = rcvdData.Remove(0, 5);
                this.say(rcvdData);
            }

        }
        void Server_lostConnection(string id)
        {
            
        }
        void Server_onConnection(string id)
        {
            
        }
        public void serverMessage(string send)
        {
            if (Server.Listening)
            {
                Server.Brodcast(ASCIIEncoding.ASCII.GetBytes(send));
            }

        }
        public void writeLogger(string write)
        {
            logger.WriteLine(write);
            serverMessage(write);
        }
        #endregion
        #region Timers
        /// <summary>
        /// The time that randomly changes SayingsBot's Colour
        /// </summary>
        void colourTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
#if DEBUG
#else
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
#endif
            
        }
        void saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            writeFile(commandsPATH, "<DOCTYPE html>\n<head>\n<title>Sayingsbot Commands and Such</title>\n<script src=\"https://dl.dropboxusercontent.com/s/qwvnaeigartecp2/sorttable.js\" type=\"text/javascript\"></script>\n<style>\ntr:nth-of-type(odd) {\nbackground-color:#ccc;\n}\ntr:nth-of-type(even) {\nbackground-color:#aaa;\n}\n</style>\n</head>\n<h1>Sayingsbot</h1>\nSayingsbot Version "+Application.ProductVersion+".<br>If this page looks sloppy, it is because it is. I've paid no attention to any standards whatsoever.\n<ul>\n    <li><a href=\"#commands\">Commands</a></li>\n    <li><a href=\"#aliases\">Aliases</a></li>\n    <li><a href=\"#classics\">Classics</a></li>\n    <li><a href=\"#quotes\">Quotes</a></li>\n    <li><a href=\"#leaderboard\">Leaderboard</a></li>\n    <li><a href=\"#whoisuser\">!whoisuser Responses</a></li>\n</ul>\n<h2 id=\"commands\">Commands</h2>\n<table border='1px' cellspacing='0px' class=\"sortable\">\n<thead><tr>\n    <td><b>Keyword</b></td>\n    <td><b>Level required</b>(0 = user, 1 = regular, 2 = trusted, 3 = mod, 4 = broadcaster, 5 = secret)</td>\n    <td><b>Output<b></td>\n</tr></thead>\n");
            foreach (hardCom h in hardList)
            {
                #region  HardComm
                string keyword = h.returnKeyword();
                string response = null;
                if (h.returnAuthLevel() < 5)
                {
                    if (keyword == "!sbset" || keyword == "!sbsilence" || keyword == "!delclassic" || keyword == "!count" || keyword == "!newcount"|| keyword == "!serpoints"|| keyword == "!addpoints") { }
                    else
                    {
                        switch (keyword)
                        {
                            case "!sbaddcom":
                                response = "Adds a command to SayingsBot.";
                                break;
                            case "!sbdelcom":
                                response = "Deletes a command from SayingsBot.";
                                break;
                            case "!sbeditcom":
                                response = "Edits a SayingsBot command.";
                                break;
                            case "!sbaddalias":
                                response = "Adds a command alais to SayingsBot.";
                                break;
                            case "!sbdelalias":
                                response = "Deletes a command alias from sayingsbot.";
                                break;
                            case "!sbeditcount":
                                response = "Edit's the count of a SayingsBot command.";
                                break;
                            case "!banuser":
                                response = "\"Ban\" a user, making them unable to use commands.\nThis also bans them from RNGPPBot. To only ban from SayingsBot, use \"!sbbanuser\"";
                                break;
                            case "!unbanuser":
                                response = "\"UnBan\" a user, making them able to use commands again.\nThis also unbans them from RNGPPBot. To only unban from SayingsBot, use \"!sbunbanuser\"";
                                break;
                            case "!sbrank":
                                response = "Returns the rank (Auth Level) SayingsBot reconises you as.";
                                break;
                            case "!whoisuser":
                                response = "Returns a reponse of who a user is (if they, or a mod, have provided one).";
                                break;
                            case "!classicwhoisuser":
                                response = "Returns a !whoisuser message from around April/May 2014.";
                                break;
                            case "!editme":
                                response = "Edit your !whoisuser message.";
                                break;
                            case "!edituser":
                                response = "Edit a user's !whoisuser message.";
                                break;
                            case "!classic":
                                response = "Classic commands no longer on nightbot. See options bellow.";
                                break;
                            case "!addclassic":
                                response = "Used to add responses to !classic.";
                                break;
                            case "!quotes":
                                response = "";
                                break;
                            case "!points":
                                response = "See how many points you have.";
                                break;
                            case "!seepoints":
                                response = "See how many points another user has.";
                                break;
                            case "!sbadduseralias":
                                response = "Adds a user alias.";
                                break;
                            case "!sbgetuseraliases":
                                response = "Get's the aliases for a user.";
                                break;
                            case "!addswear":
                                response = "Adds a swear to the swear jar.";
                                break;
                            default:
                                try //Jut in case....
                                {
                                    response = commands.checkCommand(channels, "testuser", keyword + " testvar1 testvar2 testvar3");
                                }
                                catch
                                {
                                    response = "ERROR";
                                }
                                break;
                        }
                        appendFile(commandsPATH, "<tr>\n    <td>" + keyword + "</td>\n    <td>" + h.returnAuthLevel() + "</td>\n    <td>" + response + "</td>\n</tr>");
                    }
                }
                #endregion
            }
            foreach (command c in comlist)
            {
                appendFile(commandsPATH, "<tr>\n    <td>" + c.getKey() + "</td>\n    <td>" + c.getAuth() + "</td>\n    <td>" + c.getResponse() + "</td>\n</tr>");
            }
            appendFile(commandsPATH, "</table>\n<h2 id=\"aliases\">Aliases</h2><table border='1px' cellspacing='0px' class=\"sortable\">\n<thead><tr>\n    <td><b>Alias</b></td>\n    <td><b>Command<b></td>\n</tr></thead>\n");
            foreach (ali a in aliList)
            {
                appendFile(commandsPATH, "<tr>\n    <td>" + a.getFroms()[0] + "</td>\n    <td>" + a.getTo() + "</td>\n</tr>");
            }
            appendFile(commandsPATH, "</table>\n<h2 id=\"classics\">Classics</h2>\nYou can't beat the classics!<table border='1px' cellspacing='0px' class=\"sortable\">\n<thead><tr>\n    <td><b>Classic</b></td>\n    <td><b>Response<b></td>\n</tr></thead>\n");
            foreach (string s in classicList)
            {
                appendFile(commandsPATH, "<tr>\n    <td>" + s + "</td>\n    <td>" + commands.getClassic(s) + "</td>\n</tr>");
            }
            appendFile(commandsPATH, "</table>\n<h2 id=\"quotes\">Quotes</h2><table border='1px' cellspacing='0px' class=\"sortable\">\n<thead><tr>\n    <td><b>User</b></td>\n    <td><b>Quote Number<b></td>\n    <td><b>Quote</b></td>\n</tr></thead>\n");
            foreach (string[] q in quotesList)
            {
                appendFile(commandsPATH, "<tr>\n    <td>" + q[0] + "</td>\n    <td>" + q[1] + "</td>\n    <td>" + q[2] + "</td>\n</tr>");
            }
            appendFile(commandsPATH, "</table>\n<h2 id=\"leaderboard\">Leaderboard</h2>\n<table border='1px' cellspacing='0px' class=\"sortable\">\n<thead><tr>\n    <td><b>User</b></td>\n    <td><b>Points<b></td>\n</tr></thead>\n");
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT name,alltime FROM users ORDER BY alltime DESC LIMIT 25;", dbConn).ExecuteReader();
            while (sqldr.Read())
            {
                appendFile(commandsPATH, "<tr>\n    <td>" + sqldr.GetString(0) + "</td>\n    <td>" + cstr(sqldr.GetInt32(1)) + "</td>\n</tr>\n");
            }
            appendFile(commandsPATH, "</table>\n<h2 id=\"whoisuser\">!whoisuser responses</h2>\n<table border='1px' cellspacing='0px' class=\"sortable\">\n<thead><tr>\n    <td><b>User</b></td>\n    <td><b>Response<b></td>\n</tr></thead>\n");
            foreach (string[] w in whoisList)
            {
                appendFile(commandsPATH, "<tr>\n    <td>"+w[0]+"</td>\n    <td>"+w[1]+"</td>\n</tr>\n");
            }
            appendFile(commandsPATH, "</table>\nBOOTIFUL!");
        }
        /// <summary>
        /// The timer that checks connection status and attempts to reconnect acordingly.
        /// </summary>
        void reconTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!irc.IsConnected)
            {
                writeLogger("HOLY AWEPRLFPVREA NOT CONNECTED.. RECONNECTING NOW!~");
                doReconnect();
            }

        }
        #endregion
        void connection()
        {
            irc.WriteLine("CAP REQ :twitch.tv/membership", Priority.Critical);
            irc.RfcJoin(channels);
            irc.Listen();

        }
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
            string response = commands.checkCommand(channel, user, message);
            if (response != "ERROR" && response != null)
            {
                sendMess(channel, response);
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
                writeLogger("IRC: ->" + channel + ": " + message);
            }
            if (!silence)
            {
                irc.SendMessage(SendType.Message, channel, message);
                storeMessage(bot_name, message);
            }
        }
        #region getNow
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
        #endregion
        #region Auth
        public int pullAuth(string name)
        {
            if (commands.getPoints(name) < 0) { return -2; }

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
        #endregion
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

            writeLogger("IRC Disconnecting");
            reconTimer.Stop();
            colourTimer.Stop();
            if (!irc.IsConnected)
            {
                writeLogger("... already disconnected.");
                return;
            }

            try
            {
                irc.Disconnect();
            }
            catch (Exception ex)
            {
                writeLogger("... IRC DISCONNECT FAILED: " + ex.Message);
            }

            if (!irc.IsConnected)
            {
                writeLogger("... disconnected.");
                return;
            }

        }
        public void doConnect()
        {


            writeLogger("IRC Connecting ");
            reconTimer.Start();
            colourTimer.Start();

            if (irc.IsConnected)
            {
                writeLogger("...  already connected.");
                return;
            }

            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (Exception ex) { writeLogger("IRC CONNECT FAILED: " + ex.Message); }

            if (!irc.IsConnected)
            {
                writeLogger("... IRC seems to have failed to connect :( ;~; D: ");
            }
            else
            {
                writeLogger("... Connected! Timers resuming...");
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
            writeLogger("ircConnecting()");
            try
            {
                one.Abort();
            }
            catch { } // ignore this if it fails, because i'm lazy --bob

            one = new Thread(connection);
            one.Name = "SAYINGSBOT IRC CONNECTION";
            one.IsBackground = true;

            writeLogger("Thread \"one\" recreated...");

        }
        public void ircConnected(object sender, EventArgs e)
        {
            writeLogger("ircConnected()");
            writeLogger("IRC: Joining Twitch chat");
            irc.Login(bot_name, "SAYINGSBOT", 0, bot_name, oauth);
            one.Start();
        }
        public void ircDisconnecting(object sender, EventArgs e)
        {
            writeLogger("ircDisconnecting()");

        }
        public void ircDisconnected(object sender, EventArgs e)
        {
            writeLogger("ircDisconnected()");
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
                writeLogger("IRC RAW:<- " + e.Data.RawMessage);
            }
        }
        public void ircNotice(object sender, IrcEventArgs e)
        {
            if (logLevel < 3 && logLevel > 0)
            {
                writeLogger("IRC NOTICE: " + e.Data.Message);
            }
            if (e.Data.Message == "Error logging in")
            {
                writeLogger("IRC: SEVERE: Unsuccesful login, please check the username and oauth.");
            }
        }
        public void ircChanMess(object sender, IrcEventArgs e)
        {
            bool a = false;
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            nick = nick.ToLower();
            string message = e.Data.Message;
            //message = message.ToLower(); //Need to figure out something to lowercase usernames.
            storeMessage(nick, message);
            if (message.StartsWith("!")) { } else { commands.addPoints(nick, 2); commands.addAllTime(nick, 2); }
#if DEBUG
                if (logLevel == 2) { writeLogger(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + " IRC: <-" + channel + ": <" + nick + "> " + message); }
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
                    }
                }
#else
            try
            {
                if (logLevel == 2) { writeLogger(DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + " IRC: <-" + channel + ": <" + nick + "> " + message); }
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
                    }
                }
            }
            catch (Exception eee)
            {
                writeLogger("IRC: Crisis adverted: <" + nick + "> " + message);
                this.appendFile(progressLogPATH, "IRC: Crisis adverted: <" + nick + "> " + message);
                this.appendFile(progressLogPATH, eee.ToString());
                Console.Write(eee);
            }
#endif
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
            if (logLevel == 2) { writeLogger("<-" + channel + ": " + nick + " " + message); }
        }

        public void ircWhoJoined(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            writeLogger("<-" + channel + ": " + nick + " " + " has JOINed");
        }

        public void ircWhoParted(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            writeLogger("<-" + channel + ": " + nick + " " + " has PARTed");
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
                        irc.SendDelay = 60000 / 50;//if we are modded, we can send 50 messages a minute.
                    }
                    else
                    {
                        if (pullAuth(moderator) < 4)
                        {
                            setAuth(moderator, 3);
                        }
                    }
                }
                if (!isMod) { irc.SendDelay = 60000 / 20; }//We are allowed to send 20 messages a minute to channels we are not modded in.
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
        #region Files
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
        #endregion
        #region conversions
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
        #endregion
        #region profanity
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
        /// <summary>
        /// Loads the profanity stored in the database into a list.
        /// </summary>
        public void loadProfanity()
        {
            SQLiteDataReader profanityReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user='swear' AND datatype='7' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
            if (profanityReader.Read())
            {
                int ubound = profanityReader.GetInt32(0);
                for (int i = 0; i < (ubound + 1); i++)
                {
                    profanityReader = new SQLiteCommand("SELECT data FROM userdata WHERE user='swear' AND datatype='7' AND dataID='" + i + "';", dbConn).ExecuteReader();
                    if(profanityReader.Read())
                    {
                        swearList.Add(profanityReader.GetString(0));
                    }
                }
                shouldRebuildProf = true;
            }
        }
        #endregion
        public void loadQuotesForHTML()
        {
            SQLiteDataReader quotesReader;
            Random rnd = new Random();
            quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = 'overallRandom' AND dataType = '5' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
            if (quotesReader.Read())
            {
                int ubound = quotesReader.GetInt32(0);
                for (int i = 1; i <= ubound; i++ )
                {
                    quotesReader = new SQLiteCommand("SELECT data FROM userdata WHERE user = 'overallRandom' AND dataType = '5' AND dataID='"+i+"';", dbConn).ExecuteReader();
                    if (quotesReader.Read())
                    {
                        string user = quotesReader.GetString(0);
                        quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = '"+user+"' AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                        if (quotesReader.Read())
                        {
                            int userUbound = quotesReader.GetInt32(0);
                            for (int j = 1; j <= userUbound; j++)
                            {
                                quotesReader = new SQLiteCommand("SELECT data FROM userdata WHERE user = '"+user+"' AND dataType = '1' AND dataID='" + j + "';", dbConn).ExecuteReader();
                                if (quotesReader.Read())
                                {
                                    string quote = quotesReader.GetString(0);
                                    quotesList.Add(new string[] { user, cstr(j), quote });
                                }
                            }
                        }
                    }
                }
            }
        }
        public void loadWhoIsForHTML()
        {
            SQLiteDataReader userReader = new SQLiteCommand("SELECT user,data FROM userdata WHERE datatype = '0';", dbConn).ExecuteReader();
            while (userReader.Read())
            {
                this.whoisList.Add(new string[] { userReader.GetString(0), userReader.GetString(1)});
            }
        }
    }

}