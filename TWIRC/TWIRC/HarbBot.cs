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

namespace TWIRC
{
    public partial class HarbBot
    {
        //really important stuff
        public static IrcClient irc = new IrcClient();
        public bool running = true;
        SQLiteConnection dbConn,chatDbConn,butDbConn;
        public Logger logger;
        public ButtonMasher biasControl;
        public LuaServer luaServer;

        //important stuff
        public string bot_name, oauth, channels;

        //commands and aliases
        public List<command> comlist = new List<command>();
        public List<ali> aliList = new List<ali>();
        public List<hardCom> hardList = new List<hardCom>();
        public List<luaCom> luaList = new List<luaCom>();
        public int globalCooldown;
        public int welcomeMessageCD = 60,lastWelcomeMessageTime = 0;

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
        public string progressLogPATH; public string backgroundPATH = @"C:\Users\Zack\Desktop\rngpp\backgrounds\"; int backgrounds;
        public string commandsURL = @"https://dl.dropboxusercontent.com/u/273135957/commands.html"; public string commandsPATH = @"C:\Users\Zack\Dropbox\Public\commands.html";//@"C:\Users\Zack\Dropbox\Public\commands.html"

        //voting and bias related stuff.
        public List<intIntStr> votingList = new List<intIntStr>();
        public int timeBetweenVotes = 1800, lastVoteTime, voteStatus = 0,timeToVote = 300; public System.Timers.Timer voteTimer = null,voteTimer2 = null,saveTimer = null,reconTimer = null;
        public List<double[]> newBias = new List<double[]>(); double maxBiasDiff;

        int moneyPerVote = 50;

        public Thread one;

        public HarbBot(Logger logLogger, ButtonMasher buttMuncher,LuaServer luaSurfer)
        {
            lastVoteTime = getNow();
            logger = logLogger;
            irc.Encoding = System.Text.Encoding.UTF8;//twitch's encoding
            irc.SendDelay = 1500;
            irc.ActiveChannelSyncing = true;
            //irc.AutoReconnect = true;
            //irc.AutoRejoin = true;
            //irc.AutoRelogin = true;
            //irc.SocketReceiveTimeout = 30;
            //irc.SocketSendTimeout = 10;


            biasControl = buttMuncher;
            luaServer = luaSurfer;

            newBias.Add(new double[7] { 0,0,0,0,0,0,6.5 });//0 (start)
            newBias.Add(new double[7] { 5,5,0,0,0,0,0 });//1
            newBias.Add(new double[7] { 0,10,0,0,0,0,0 });//2
            newBias.Add(new double[7] { 0,5,0,5,0,0,0 });//3
            newBias.Add(new double[7] { 10,0,0,0,0,0,0 });//4
            newBias.Add(new double[7] { 0,0,0,0,0,0,0 });//5
            newBias.Add(new double[7] { 0,0,0,10,0,0,0 });//6
            newBias.Add(new double[7] { 5,0,5,0,0,0,0 });//7
            newBias.Add(new double[7] { 0,0,10,0,0,0,0 });//8
            newBias.Add(new double[7] { 0,0,5,5,0,0,0 });//9
            newBias.Add(new double[7] { 0,0,0,0,9.5,0,0 });//10 (a)
            newBias.Add(new double[7] { 0,0,0,0,0,8.5,0 });//11 (b)
            newBias.Add(new double[7] { 2.5, 2.5, 2.5, 2.5, 0, 0, 0 });//12 (movement)
            newBias.Add(new double[7] { 0, 5, 5, 0, 0, 0, 0 });//13 vertical
            newBias.Add(new double[7] { 5, 0, 0, 5, 0, 0, 0 });//14 horizontal
            newBias.Add(new double[7] { 0, 0, 0, 0, 4.75, 4.25, 0 });//15 buttons

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
            irc.OnChannelAction += ircChanActi; //defined in harbbot_split
            irc.OnChannelMessage += ircChanMess; //defined in harbbot_split

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
                bot_name = "rngppbot";
                channels = "#rngplayspokemon";
                
                globalCooldown = 20; 
                antispam = true;
                oauth = "oauth:67h2n5dny6xf2ho6j7oj3xugu7uurd";
                logLevel = 2;
                progressLogPATH = @"C:\Users\Zack\Dropbox\Public\rnglog.txt";
                maxBiasDiff = 0.05;

                short temp2 = 0; if (antispam) { temp2 = 1; }
                SQLiteConnection.CreateFile("db.sqlite");
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7), points INT DEFAULT 0, alltime INT DEFAULT 0, isnew INTEGER DEFAULT 1);", dbConn).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE settings (name VARCHAR(25) NOT NULL, channel VARCHAR(26) NOT NULL, antispam TINYINT(1) DEFAULT 1, silence TINYINT(1) DEFAULT 0, oauth VARCHAR(200), cooldown INT DEFAULT 20,loglevel TINYINT(1) DEFAULT 2,logPATH VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE biassettings (timebetweenvote INT NOT NULL, timetovote INT NOT NULL,def VARCHAR(200) NOT NULL, maxdiff REAL NOT NULL,moneypervote INT DEFAULT 100);",dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE transactions (name VARCHAR(25) NOT NULL, amount INT NOT NULL,item VARCHAR(1024) NOT NULL,prevMoney INT NOT NULL,date VARCHAR(7) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE ascostlist (type VARCHAR(25), costs INT DEFAULT 0, message VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aswhitelist (name VARCHAR(50),regex VARCHAR(50));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE luacoms (keyword VARCHAR(60) NOT NULL, command VARCHAR(60) NOT NULL, defult VARCHAR(60), response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                
                new SQLiteCommand("INSERT INTO settings (name,channel,antispam,silence,oauth,cooldown,loglevel,logPATH) VALUES ('" + bot_name + "','" + channels + "','" + temp2 + "',0,'" + oauth + "','" + globalCooldown + "','"+logLevel+"','"+progressLogPATH+"');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen,isnew) VALUES ('" + channels.Substring(1) + "','4','" + getNowSQL() + "',0);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen,isnew) VALUES ('"+bot_name+"','-1','"+getNowSQL()+"',0);",dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO biassettings (timebetweenvote,timetovote,def,maxdiff) VALUES ('1800','300','1.00:1.00:1.00:1.00:0.96:0.92:0.82','0.05');", dbConn).ExecuteNonQuery();

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
                    progressLogPATH = sqldr.GetString(7);
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
            if (!File.Exists("buttons.sqlite"))
            {
                SQLiteConnection.CreateFile("buttons.sqlite");
                butDbConn = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
                butDbConn.Open();
                new SQLiteCommand("CREATE TABLE buttons (id INT, left INT, down INT, up INT, right INT, a INT, b INT, start INT);", butDbConn).ExecuteNonQuery();
            }
            else
            {
                butDbConn = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
                butDbConn.Open();
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

            rdr =  new SQLiteCommand("SELECT * FROM biassettings;",dbConn).ExecuteReader();
            while(rdr.Read())
            {
                timeBetweenVotes = rdr.GetInt32(0);
                timeToVote = rdr.GetInt32(1);
                List<double> tempDoubleArray = new List<double>();
                string[] tempStringArray = rdr.GetString(2).Split(':');
                foreach(string s in tempStringArray)
                {
                    tempDoubleArray.Add(double.Parse(s));
                }
                biasControl.setDefaultBias(tempDoubleArray.ToArray());
                maxBiasDiff = rdr.GetDouble(3);
                moneyPerVote = rdr.GetInt32(4);
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
            hardList.Add(new hardCom("!silence",3,1));
            hardList.Add(new hardCom("!rank", 0, 0,60));
            if (antispam)
            {
                hardList.Add(new hardCom("!permit", 2, 1));
                hardList.Add(new hardCom("!whitelist", 0, 0));
            }
            hardList.Add(new hardCom("!rngppcommands", 0, 0, 120));

            //RNGPP catered commands, commented out means no way of implementing that yet or no idea.
            hardList.Add(new hardCom("!setbias",4,7));
            hardList.Add(new hardCom("!setdefaultbias",4,7));
            hardList.Add(new hardCom("!setbiasmaxdiff", 4, 1));
            hardList.Add(new hardCom("!resetbias", 4, 0));
            hardList.Add(new hardCom("!bias",0,1));
            hardList.Add(new hardCom("!balance", 0, 0,60));
            hardList.Add(new hardCom("!addlog", 0, 1,5));
            hardList.Add(new hardCom("!setpoints",4,2));
            hardList.Add(new hardCom("!voting", 3, 1));
            //hardList.Add(new hardCom("!maintenance", 3, 1));
            hardList.Add(new hardCom("!background",0,1));
            //hardList.Add(new hardCom("!song",0,1));
            //hardList.Add(new hardCom("!seriousmode",3,1);
            hardList.Add(new hardCom("!save", 3, 1));
            hardList.Add(new hardCom("!funmode", 3, 0));//   >:)
            hardList.Add(new hardCom("!givemoney", 0, 0));
            hardList.Add(new hardCom("!giveball", 0, 0));
            
            /*
            //sayingsbot overrides, we might add these eventually            
            hardList.Add(new hardCom("!whois",0,1,20));
            hardList.Add(new hardCom("!editme",1,1));
            hardList.Add(new hardCom("!edituser",3,2));
            hardList.Add(new hardCom("!classic",0,1,20));
            hardList.Add(new hardCom("!addclassic",2,2));
            hardList.Add(new hardCom("!delclassic",2,2));
            */

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
            

            checkBackgrounds();

            try
            {
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch { }
        }

        void saveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            writeFile(commandsPATH, "<DOCTYPE html><head><title>RNGPPBot commands</title><h1>RNGPPBot commands</h1></head>If this page looks sloppy, it is because it is. I've paid no attention to any standards whatsoever.<table border='1px' cellspacing='0px'><tr><td><b>keyword</b></td><td><b>level required</b>(0 = user, 1 = regular, 2 = trusted, 3 = mod, 4 = broadcaster, 5 = secret)</td><td><b>output<b></td></tr>");
            foreach (command c in comlist)
            {
                appendFile(commandsPATH, "<tr><td>" + c.getKey() + "</td><td>" + c.getAuth() + "</td><td>" + c.getResponse() + "</td></tr>");
            }
            appendFile(commandsPATH, "</table>\nBOOTIFUL!");

            if (sender != null)
            {
                string s = "INSERT INTO buttons (left,down,up,right,a,b,start) VALUES (";
                foreach (int a in biasControl.stats)
                {
                    s += a + ",";
                }
                s = s.Substring(0, s.Length - 1);
                s += ");";
                new SQLiteCommand(s, butDbConn).ExecuteNonQuery();
                biasControl.stats = new int[] { 0, 0, 0, 0, 0, 0, 0 };

                irc.RfcPrivmsg(channels, ".mods");
            }
        }

        void connection()
        {
            irc.RfcJoin(channels);
            irc.Listen();

        }

        public void say(string message)
        {   
            sendMess(channels, message);
            checkCommand(channels, channels.Substring(1), filter(message));//I guess?
        }

        void checkBackgrounds()
        {
            bool fail = false; int a = -1;
            while (!fail)
            {
                a++;
                if (!File.Exists(backgroundPATH + "background_" + a + ".png") && !File.Exists(backgroundPATH + "background_" + a + ".gif") && !File.Exists(backgroundPATH + "background_" + a + ".jpg"))
                {
                    fail = true;
                }
            }
            backgrounds = a;
        }

        void voteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            checkBackgrounds();
            if (voteStatus != -1)
            {
                if (sender == voteTimer)
                {
                    voteStatus = 1;
                    voteTimer2.Start();
                    sendMess(channels, "Voting for bias is now possible! Type !bias <direction> [amount of votes] to vote! (For example \"!bias 3\" to vote once for down-right, \"!bias up 20\" would put 20 votes for up at the cost of some of your pokédollars)");
                }
                if (sender == voteTimer2)
                {
                    string str = "Voting is over.";
                    double[] tobebias = biasControl.getDefaultBias();
                    double[] values = new double[] { 0, 0, 0, 0, 0, 0, 0 };
                    string serverput = ""; int highest = 0, id = 0;
                    if (votingList.Count > 0)
                    {
                        int a = 0;
                        foreach (intIntStr b in votingList)
                        {
                            a += b.Int1;
                            for (int i = 0; i < b.Int1; i++)
                            {
                                for (int j = 0; j < 7; j++)
                                {
                                    values[j] += newBias[b.Int2][j];
                                }
                            }
                        }
                        for (int i = 0; i < 7; i++)
                        {
                            if (values[i] > highest)
                            {
                                id = i;
                            }
                            serverput += values[i] + " ";
                            values[i] = (values[i] * maxBiasDiff) / (a * 10);
                            tobebias[i] += values[i];
                            
                        }
                        str += " Processed " + a + " votes from " + votingList.Count + " users. ";
                        biasControl.setBias(tobebias);
                        luaServer.send_to_all("SETBIAS",tobebias[0]+" "+tobebias[1]+" "+tobebias[2]+" "+tobebias[3]+" "+tobebias[4]+" "+tobebias[5]+" "+tobebias[6]);
                    }
                    else
                    {
                        biasControl.doDecay();
                    }
                    str += " Next vote starts in " + (Math.Floor((((double)timeBetweenVotes)) / 6) / 10) + " minutes. ("+serverput+")";
                    votingList.Clear();
                    lastVoteTime = getNow();
                    voteStatus = 0;
                    voteTimer.Start();
                    sendMess(channels, str);
                }
            }
        }

        void reconTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!irc.IsConnected)
            {
                logger.WriteLine("HOLY AWEPRLFPVREA NOT CONNECTED.. RECONNECTING NOW!~");
            }
        
        }

        public void doDisconnect()
        {

            logger.Write("IRC Disconnecting, vote timers paused ");
            //one.Abort();

            voteTimer.Stop();  // stop the vote timers while we're down
            voteTimer2.Stop();

            reconTimer.Stop();



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

            if (irc.IsConnected)
            {
                logger.WriteLine("...  already connected.");   
                return;
            }

           // one = new Thread(connection); // does the old reference vanish?
           // one.Name = "RNGPPBOT IRC CONNECTION";
           // one.IsBackground = true;

            try                 { irc.Connect("irc.twitch.tv", 6667); }
            catch (Exception ex){ logger.WriteLine("IRC CONNECT FAILED: " + ex.Message); }

            if (!irc.IsConnected)
            {
                logger.WriteLine("... IRC seems to have failed to connect :( ;~; D: ");   
            }
            else
            {
                logger.WriteLine("... Connected! Vote timers resuming...");   
                voteTimer.Start();
                voteTimer2.Start();
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
            irc.Reconnect(true,true);
        }


        /* "reconnect()"
        public void reconnect()
        {
            try
            {
                one.Abort();
                voteTimer.Stop();
                voteTimer2.Stop();
                irc.Disconnect();
            }
            catch { };
            try
            {
                irc.Connect("irc.twitch.tv", 6667);
                voteTimer2.Start();
                voteTimer.Start();
            }
            catch (ConnectionException e)
            {
                if (logLevel != 0)
                {
                    logger.Write("IRC: Connection error: " + e.Message + ". Retrying in 5 seconds.");
                }
                Thread.Sleep(5000);
                reconnect();
            };

        }*/

        public void Close()
        {
            try
            {
                one.Abort();
                irc.RfcQuit();
            }
            catch { }
        }

        //eventbinders

 

        public void ircConnecting(object sender, EventArgs e)
        {
            logger.WriteLine("ircConnecting()");
            try
            {
                one.Abort();
            }
            catch{} // ignore this if it fails, because i'm lazy --bob

            one = new Thread(connection);
            one.Name = "RNGPPBOT IRC CONNECTION";
            one.IsBackground = true;
              
            logger.WriteLine("Thread \"one\" recreated...");

        }

        public void ircConnected(object sender, EventArgs e)
        {
            logger.WriteLine("ircConnected()");
            logger.WriteLine("IRC: Joining Twitch chat");
            irc.Login(bot_name, "HARBBOT", 0, bot_name, oauth);
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


        public void ircConError(object sender, EventArgs e)
        {

        }

        public void ircError(object sender, IrcEventArgs e)
        {
            logger.WriteLine("IRC: error in connect: " + e.Data.RawMessage);
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

        public void ircQuery(object sender, IrcEventArgs e)
        {
            string str = e.Data.Message;
            if(str.StartsWith("The moderators"))
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
                File.AppendAllText(path, stuff);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }

}