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
        public Chat chatter;
        public ButtonMasher biasControl;
        public LuaServer luaServer;

        //important stuff
        public string bot_name, oauth, channel;
        public string[] channels;

        //commands and aliases
        public List<command> comlist = new List<command>();
        public List<ali> aliList = new List<ali>();
        public List<hardCom> hardList = new List<hardCom>();
        public List<luaCom> luaList = new List<luaCom>();
        public List<Bias> biasList = new List<Bias>();
        public int globalCooldown;
        public int welcomeMessageCD = 60,lastWelcomeMessageTime = 0;

        //Calculator (used for !calculate and expressions for money stuff.
        Calculator calculator =  new Calculator();

        //antispam
        public bool antispam; public List<intStr> permits = new List<intStr>(); public int asCooldown = 60, permitTime = 300;
        public List<asUser> asUsers = new List<asUser>();
        public List<intStr> asCosts = new List<intStr>();
        public List<string> asTLDs = new List<string>(), asWhitelist = new List<string>(),asWhitelist2 = new List<string>();
        List<List<string>> asResponses = new List<List<string>>();
        
        //some settings
        public bool silence,isMod = false;
        public string progressLogPATH; public string backgroundPATH = @"C:\Users\Zack\Desktop\rngpp\backgrounds\"; int backgrounds;
        public string commandsURL = @"https://dl.dropboxusercontent.com/u/273135957/commands.html"; public string commandsPATH = @"C:\Users\Zack\Desktop\RNGPPDropbox\Dropbox\Public\commands.html";//@"C:\Users\Zack\Dropbox\Public\commands.html"

        //voting and bias related stuff.
        public List<intStr> votingList = new List<intStr>();
        public List<Bias> votinglist = new List<Bias>();

        public int timeBetweenVotes = 1800, lastVoteTime, voteStatus = 0,timeToVote = 300;
        public System.Timers.Timer voteTimer = null,voteTimer2 = null,saveTimer = null,reconTimer = null, exp_allTimer = null, pollTimer = null;
        public double[] newBias = new double[7]; double maxBiasDiff; int expTime = 0,expTimeEnd=0;
        public string poll_name = ""; public string[] poll = null; public bool poll_active; public List<intStr> poll_votes = new List<intStr>();

        int moneyPerVote = 50; double moneyconversionrate = 0.5; string expAllFunc = "2*X+50";

        public bool backgrounds_enabled = true;

        public Thread one;

        public HarbBot(Logger logLogger, ButtonMasher buttMuncher,LuaServer luaSurfer)
        {
            lastVoteTime = getNow();
            logger = logLogger;
            biasControl = buttMuncher;
            luaServer = luaSurfer;

            luaServer.send_to_all("EXPOFF", "");
            luaServer.send_to_all("REPELOFF", "");

            newBias = new double[7] { 10,10,10,10,9,8,6.5 };
            log(0, "Created.");
            setUpIRC(); log(1, "Done setting up parameters.");

            initialiseDatabase(); log(1, "Loaded settings.");
            initialiseChat(); log(1, "Chat database connection established.");
            initialiseButtons(); log(1, "button database connection established.");

            loadCommands(); log(1, "Loaded commands ("+comlist.Count.ToString()+").");
            loadAliases(); log(1, "Loaded aliases (" + aliList.Count.ToString() + ").");
            loadBiases(); log(1, "Loaded biases (" + biasList.Count.ToString() + ").");

            loadAntispam(); log(1, "Loaded antispam.");

            loadHardComs(); log(1, "Prepared " + hardList.Count + " hardcoded commands.");

            prepareTimers(); log(1, "Started timers.");

            checkBackgrounds(); log(1, "All done, connecting now!");

            try
            {
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch { }
        }

        void log(int level, string message)
        {
             logger.addLog("IRC", level, message);
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

                irc.RfcPrivmsg(channel, ".mods");
            }
        }

        void pollTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (poll_active)
            {
                string str = "There's currently a poll running for: ' " + poll_name+"'. The options are:";
                for (int i = 0; i < poll.Length; i++)
                {
                    str += " (" + (i+1) + ") '" + poll[i] + "'.";
                }
                str += " Use !vote X to cast your vote!";
                say(str);
            }
        }

        void connection()
        {
            channels = new string[] {channel, bot_name};
            irc.RfcJoin(channels);
            irc.Listen();

        }

        public void say(string message, int type = 2)
        {   
            sendMess(channel, message, type);
            checkCommand(channel, channel.Substring(1), filter(message));//I guess?
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
            if (backgrounds_enabled)
            {
                checkBackgrounds();
            }
            if (voteStatus > -1)
            {
                if (sender == voteTimer)
                {
                    voteStatus = 1;
                    voteTimer2.Start();
                    sendMess(channel, "Voting for bias is now possible! Type !bias <direction> [amount of votes] to vote! (For example \"!bias 3\" to vote once for down-right, \"!bias up 20\" would put 20 votes for up at the cost of some of your pokedollars)");
                }
                if (sender == voteTimer2)
                {
                    string str = "Voting is over.";
                    double[] tobebias = biasControl.getDefaultBias();
                    double[] values = new double[] { 0, 0, 0, 0, 0, 0, 0 };
                    string serverput = "";
                    if (votingList.Count > 0)
                    {
                        int a = 0;
                        for(int q = 0; q<votinglist.Count; q++)
                        {
                            var b = votingList[q].Int;
                            Bias B = votinglist[q];
                            a += b;
                            for (int j = 0; j < 7; j++)
                            {
                                values[j] += B[j]*B.factor*b;
                            }
                        }
                        for (int i = 0; i < 7; i++)
                        {
                            serverput += values[i] + " ";
                            values[i] = (values[i] * maxBiasDiff * newBias[i]/10) / (a * 10);
                            tobebias[i] += values[i];
                            
                        }
                        str += " Processed " + a + " vote";
                        if (a != 1) { str += "s"; }
                        str += "from " + votingList.Count;
                        
                        if (votingList.Count != 1) str += " users. ";
                        else str += "user. ";
                        biasControl.setBias(tobebias);
                        luaServer.send_to_all("SETBIAS",tobebias[0]+" "+tobebias[1]+" "+tobebias[2]+" "+tobebias[3]+" "+tobebias[4]+" "+tobebias[5]+" "+tobebias[6]);
                    }
                    else
                    {
                        biasControl.doDecay();
                    }
                    str += " Next vote starts in " + (Math.Floor((((double)timeBetweenVotes)) / 6) / 10) + " minutes. ("+serverput+")";
                    votingList.Clear();
                    votinglist.Clear();
                    lastVoteTime = getNow();
                    voteStatus = 0;
                    voteTimer.Start();
                    sendMess(channel, str);
                }
            }
        }

        void exp_allTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (expTime > 0)
            {
                exp_allTimer.Dispose();
                exp_allTimer = new System.Timers.Timer(expTime*1000);
                exp_allTimer.AutoReset = false;
                exp_allTimer.Elapsed += exp_allTimer_Elapsed;
                exp_allTimer.Start();
                expTime = 0;
            }
            else
            {
                exp_allTimer.Enabled = false;
                luaServer.send_to_all("EXPOFF", "");
                sendMess(channel, "EXP ALL timer has elapsed.");
            }
        }


        void reconTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!irc.IsConnected)
            {
                log(1,"NOT CONNECTED! RECONNECTING NOW!");
                doReconnect();
            }
        
        }

        public void doDisconnect()
        {

            log(2, "Disconnecting.");
            //one.Abort();

            if (voteStatus != -1) { voteStatus = -1; }
            else voteStatus = -2;
            voteTimer.Stop();  // stop the vote timers while we're down
            voteTimer2.Stop();

            reconTimer.Stop();



            if (!irc.IsConnected)
            {
                log(2,"Already disconnected.");
                return;
            }

            try
            {
                irc.Disconnect();
            }
            catch (Exception ex)
            {
                log(0,"DISCONNECT FAILED: " + ex.Message);
            }

            if (!irc.IsConnected)
            {
                log(1,"Disconnected.");
                return;
            }

        }

        public void doConnect()
        {


            log(2,"Connecting ");
            reconTimer.Start();

            if (irc.IsConnected)
            {
                log(2,"...  already connected.");   
                return;
            }

           // one = new Thread(connection); // does the old reference vanish?
           // one.Name = "RNGPPBOT IRC CONNECTION";
           // one.IsBackground = true;

            try                 { irc.Connect("irc.twitch.tv", 6667); }
            catch (Exception ex){ log(0,"CONNECT FAILED: " + ex.Message); }

            if (!irc.IsConnected)
            {
                log(1,"Not connected: retrying in 5 seconds.");
            }
            else
            {
                log(1,"Connected! If enabled: vote timers resuming.");

                if (voteStatus != -2)
                    voteStatus = 1;
                else
                    voteStatus = -1;
                voteTimer2.Start();

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
            //logger.WriteLine("ircConnecting()");
            try
            {
                one.Abort();
            }
            catch{} // ignore this if it fails, because i'm lazy --bob

            one = new Thread(connection);
            one.Name = "RNGPPBOT IRC CONNECTION";
            one.IsBackground = true;
              
            log(1,"Thread \"one\" recreated.");

        }

        public void ircConnected(object sender, EventArgs e)
        {
            //logger.WriteLine("ircConnected()");
            log(0,"Connected.");
            irc.Login(bot_name, "HARBBOT", 0, bot_name, oauth);
            one.Start();
        }

        public void ircDisconnecting(object sender, EventArgs e)
        {
            //logger.WriteLine("ircDisconnecting()");

        }
        
        public void ircDisconnected(object sender, EventArgs e)
        {
           // logger.WriteLine("ircDisconnected()");
            try
            {
                one.Abort();
            }
            catch { }
        }


        public void ircConError(object sender, EventArgs e)
        {
            log(0, "CONNECTION ERROR: " + e.ToString());
        }

        public void ircError(object sender, IrcEventArgs e)
        {
            log(0,"ERROR IN CONNECT: " + e.Data.RawMessage);
        }
        public void ircRaw(object sender, IrcEventArgs e)
        {
                log(3,"IRC RAW:<- " + e.Data.RawMessage);
        }
        public void ircNotice(object sender, IrcEventArgs e)
        {
            if (e.Data.Message == "Error logging in")
            {
                log(0,"SEVERE: Unsuccesful login, please check the username and oauth.");
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
                        irc.SendDelay = 60000 / 50;//if we are modded, we can send 50 messages a minute.
                    }
                    else
                    {
                        if (pullAuth(moderator) < 4 && pullAuth(moderator) != -1)//we kinda forgot to check for banned moderators, woops.
                        {
                            setAuth(moderator, 3);
                        }
                    }
                }
                if (!isMod) { irc.SendDelay = 60000 /20; }//We are allowed to send 20 messages a minute to channels we are not modded in.
                log(2, "Moderators updated.");
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