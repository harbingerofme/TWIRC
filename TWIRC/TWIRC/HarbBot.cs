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

namespace RNGBot
{
    public class HarbBot
    {
        public static IrcClient irc = new IrcClient();
        public bool running = true;

        public string bot_name;
        public string channels;
        public string oauth;
        public List<command> comlist = new List<command>();
        public List<ali> aliList = new List<ali>();
        public List<hardCom> hardList = new List<hardCom>();
        public List<asUser> asUsers = new List<asUser>();
        public int globalCooldown;
        public int logLevel;
        public bool antispam; public List<intStr> permits = new List<intStr>(); public int asCooldown = 60,permitTime = 300;
        public bool silence;
        public List<intStr> votingList = new List<intStr>();
        public string progressLogPATH;
        SQLiteConnection dbConn;
        public Logger logger;
        public int timeBetweenVotes = 1800, lastVoteTime, voteStatus = 0,timeToVote = 300; public System.Timers.Timer voteTimer,voteTimer2;


        public Thread two;

        public HarbBot(Logger logLogger)
        {
            lastVoteTime = getNow();
            logger = logLogger;
            irc.Encoding = System.Text.Encoding.UTF8;//twitch's encoding
            irc.SendDelay = 1500;
            irc.ActiveChannelSyncing = true;

            //write these Methods
            irc.OnConnected += ircConnected;
            irc.OnJoin += ircJoined;
            irc.OnConnectionError += ircConError;
            irc.OnError += ircError;

            irc.OnQueryMessage += ircQuery;
            irc.OnRawMessage += ircRaw;
            irc.OnChannelAction += ircChanActi;
            irc.OnChannelMessage += ircChanMess;

            //LoadCommands
            if (logLevel != 0)
            {
                logger.WriteLine("IRC: Booting up, shouldn't take long!");
            }
            string temp;
            if (!File.Exists("db.sqlite"))
            {
                if (logLevel != 0)
                {
                    logger.WriteLine("First time setup detected, making database");
                }
                bot_name = "harbbot";
                channels = "#rngplayspokemon";
                globalCooldown = 20; 
                antispam = true;
                oauth = "oauth:l3jjnxjgfvkjuqa7q9yabgcezm5qpsr";
                logLevel = 1;
                progressLogPATH = @"C:\Users\Zack\Dropbox\Public\rnglog.txt";

                short temp2 = 0; if (antispam) { temp2 = 1; }
                SQLiteConnection.CreateFile("db.sqlite");
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7), money INT DEFAULT 0);", dbConn).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE settings (name VARCHAR(25) NOT NULL, channel VARCHAR(26) NOT NULL, antispam TINYINT(1) NOT NULL, silence TINYINT(1) NOT NULL, oauth VARCHAR(200), cooldown INT,loglevel TINYINT(1),logPATH VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE transactions (name VARCHAR(25) NOT NULL, amount INT NOT NULL,item VARCHAR(1024) NOT NULL,prevMoney INT NOT NULL,date VARCHAR(7) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO settings (name,channel,antispam,silence,oauth,cooldown,loglevel,logPATH) VALUES ('" + bot_name + "','" + channels + "','" + temp2 + "',0,'" + oauth + "','" + globalCooldown + "','"+logLevel+"','"+progressLogPATH+"');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen) VALUES ('" + channels.Substring(1) + "','4','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen) VALUES ('" + bot_name + "','-1','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
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
                logger.WriteLine("Loaded " + comlist.Count() + " commands!");
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
                logger.WriteLine("Loaded " + aliList.Count() + " aliases!");
            }


            //Here we add some hardcoded commands and stuff (while we do have to write out their responses hardocded too, it's a small price to pay for persitency)
            hardList.Add(new hardCom("!adco", 3, 2));//addcom (reduced now, so it doesn't conflict with nightbot)
            hardList.Add(new hardCom("!dc", 3, 1));//delcom
            hardList.Add(new hardCom("!ec", 3, 2));//editcom
            hardList.Add(new hardCom("!aa", 3, 2));//addalias
            hardList.Add(new hardCom("!da", 3, 1));//delete alias
            hardList.Add(new hardCom("!set", 2, 2));//elevate another user
            hardList.Add(new hardCom("!editcount", 3, 2));
            hardList.Add(new hardCom("!banuser", 3, 1));
            hardList.Add(new hardCom("!unbanuser", 4, 1));
            hardList.Add(new hardCom("!silence",3,1));
            hardList.Add(new hardCom("!rank", 0, 0,600));
            hardList.Add(new hardCom("!permit", 2, 1));

            //RNGPP catered commands, commented out means no way of implementing that yet or no idea.
            hardList.Add(new hardCom("!setbias",2,1));
            hardList.Add(new hardCom("!bias",0,1));
            hardList.Add(new hardCom("!balance", 0, 0,600));
            hardList.Add(new hardCom("!addlog", 0, 1));
            hardList.Add(new hardCom("!setpoints",4,2));
            //hardList.Add(new hardCom("!maintenance", 3, 1));
            //hardList.Add(new hardCom("!background",0,1));
            //hardList.Add(new hardCom("!song",0,1));
            //hardList.Add(new hardCom("!seriousmode",3,1);
            hardList.Add(new hardCom("!save", 3, 0));
            hardList.Add(new hardCom("!funmode", 3, 0));//   >:)
            
            //sayingsbot overrides, we might add these eventually            
            hardList.Add(new hardCom("!whois",0,1,20));
            hardList.Add(new hardCom("!editme",1,1));
            hardList.Add(new hardCom("!edituser",3,2));
            hardList.Add(new hardCom("!classic",0,1,20));
            hardList.Add(new hardCom("!addclassic",2,2));
            hardList.Add(new hardCom("!delclassic",2,2));

            
            two = new Thread(run_2);//manages saving of commandlists, etc.
            two.Name = "RNGPPBOT background irc thread.";
            two.IsBackground = true;
            two.Start();

            voteTimer = new System.Timers.Timer(timeBetweenVotes*1000);
            voteTimer.Elapsed += voteTimer_Elapsed;
            voteTimer.Start();

            voteTimer2 = new System.Timers.Timer(timeToVote*1000);
            voteTimer2.Elapsed += voteTimer_Elapsed;

            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (ConnectionException e) { System.Diagnostics.Debug.WriteLine("Thread 1 Connection error: " + e.Message); }


        }

        void voteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (sender == voteTimer)
            {
                voteStatus = 1;
                voteTimer2.Start();
                sendMess(channels, "Voting for bias is now possible! Type !bias <direction> [amount of votes] to vote! (For example \"!bias 3\" to vote once for down-right, \"!bias up 20\" would put 20 votes for up at the cost of some of your points)");
            }
            else
            {
                int a = 0;
                foreach(intStr b in votingList){
                    a += b.Int;
                }
                sendMess(channels, "Voting is over. Processed " + a + " votes from " + votingList.Count + " users. Next vote starts in "+(Math.Floor((((double)timeBetweenVotes))/6)/10) + " minutes.");
                votingList = new List<intStr>();
                lastVoteTime = getNow();
                voteStatus = 0;
                voteTimer.Start();
            }
        }

        public void reconnect()
        {
            try
            {
                irc.Disconnect();
            }
            catch { };
            try
            {
                irc.Connect("irc.twitch.tv", 6667);
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

        }

        public void run_2()
        {
            int count = 0;
            while (running)
            {
                count++;
                Thread.Sleep(1000);
                if (count > 3600)
                {
                    safe();
                    count = 0;
                }
            }
        }

        public void checkSpam(string channel, string user, string message)
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
                List<intStr> costs = new List<intStr>();
                int type = -1;

                costs.Add(new intStr("link", 5));//0
                costs.Add(new intStr("emote spam", 3));//1
                costs.Add(new intStr("letter spam", 1));//2
                costs.Add(new intStr("ASCII", 5));//3
                costs.Add(new intStr("tpp",2));//4

                List<List<string>> responses = new List<List<string>>(); responses.Add(new List<string>()); responses.Add(new List<string>()); responses.Add(new List<string>()); responses.Add(new List<string>()); responses.Add(new List<string>());
                responses[0].Add("Google those nudes!");responses[0].Add("We are not buying your shoes!");responses[0].Add("The stuff people would have to put up with...");
                responses[1].Add("Images say more than a thousand words, so stop writing essays!");responses[1].Add("How is a timeout for a twitch chat feature?");responses[1].Add("I dislike emotes, they are all text to me.");
                responses[2].Add("There's no need to type that way.");responses[2].Add("I do not take kindly upon that.");responses[2].Add("Stop behaving like a spoiled little RNG!");
                responses[3].Add("Whatever that was, it's gone now.");responses[3].Add("This is not the place to use that!");responses[3].Add("Woah, you typed all of that? Who am I kidding, get out!");
                responses[4].Add("This is not TwitchPlaysPokemon, this is a computer playing pokémon, quite the reverse actually.");responses[4].Add("Can you read? There's plenty of stuff that says THIS ISN'T TPP");responses[4].Add("You think you know better than the RNGesus? Download the save and play it without annoying us.");

                if (a == -1)
                {
                    a = asUsers.Count;
                    asUsers.Add(new asUser(user, pullAuth(user)));
                }
                if (message != "")
                {
                    message = message.ToLower();
                    if (Regex.Match(message, @"^.$").Success || Regex.Match(message,@"([a-zA-Z])\1\1").Success || Regex.Match(message,@"([0-9])\1\1\1").Success || Regex.Match(message,@"([^[0-9a-zA-Z]]){4}").Success) { asUsers[a].update(costs[2].Int); type=2; }//either a single letter, 3 same letters in a row, 4 not alphanumerical characters in a row,
                    if (message.Length > 40 && Regex.Match(message, @"^[^[a-zA-Z]]*$").Success) { asUsers[a].update(costs[3].Int); type=3; }

                    MatchCollection mc = Regex.Matches(message, @"([^ ]+\.[a-z]{2,})[\/\?\#]?".ToLower());
                    int b = mc.Count;
                    if (b > 0) { }
                    b -= Regex.Matches(message, @"imgur\.com").Count;
                    b -= Regex.Matches(message, @"xkcd\.com").Count;
                    b -= Regex.Matches(message, @"rngpp\.booru\.org").Count;
                    b -= Regex.Matches(message, @"bulbapedia\.bulbagarden\.net").Count;
                    foreach (intStr f in permits)
                    {
                        if (f.Str == user) { b = 0; permits.Remove(f); break; }
                    }
                    if (b > 0) { asUsers[a].update(costs[0].Int); type=0; }   
                }
                if(type!=-1 && asUsers[a].points<1){
                    irc.RfcPrivmsg(channel,".timeout "+user+" 1");//overrides the send delay (hopefully)
                    int c = new Random().Next(0,4);
                    sendMess(channel,user+" -> "+responses[type][c]+" ("+costs[type].Str+")");
                }
            }
            if (user == "zackattack9909" && Regex.Match(message,"wix[1-4]").Success) {irc.RfcPrivmsg(channel,".clear");sendMess(channel,"Zack, please don't.");}
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
            foreach (hardCom h in hardList)//hardcoded command
            {
                if (h.hardMatch(user,message,auth))
                {
                    done = true;
                    str = h.returnPars(message);
                    if (logLevel == 1) { logger.WriteLine("IRC:<- <"+user +"> " + message); }
                    switch (h.returnKeyword())
                    {
                        case "!adco":
                                fail = false;

                                foreach (command c in comlist) { if (c.doesMatch(str[1])) { fail = true; break; } }
                                foreach (hardCom c in hardList) { if (c.doesMatch(str[1]) || fail) { fail = true; break; } }
                                foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; } }
                                if (fail) { sendMess(channel, "I'm sorry, " + user + ". A command or alias with the same name exists already."); }
                                else
                                {
                                    tempVar1 = 0;
                                    if (Regex.Match(str[2], @"@level(\d)@").Success) { tempVar1 = int.Parse(Regex.Match(str[2], @"@level(\d)@").Groups[1].Captures[0].Value); tempVar2 = str[3]; if (tempVar1 >= 5) { tempVar1 = 5; } }
                                    else { tempVar2 = str[2] + " " + str[3]; }
                                    tempVar3 = tempVar2.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    comlist.Add(new command(str[1], tempVar3, tempVar1));
                                    new SQLiteCommand("INSERT INTO commands (keyword,authlevel,count,response) VALUES ('" + str[1] + "','" + tempVar1 + "','" + 0 + "','" + MySqlEscape(tempVar2) + "');",dbConn).ExecuteNonQuery();
                                    sendMess(channel, user + " -> command \"" + str[1] + "\" added. Please try it out to make sure it's correct.");
                                }
                            break;
                        case "!ec":
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
                                        new SQLiteCommand("UPDATE commands SET response = '" + MySqlEscape(tempVar2) + "' WHERE keyword='" + str[1] + "';",dbConn).ExecuteNonQuery();
                                        sendMess(channel, user + "-> command \"" + str[1] + "\" has been edited.");
                                        safe();
                                        fail = false;
                                    }
                                }
                                if (fail)
                                {
                                    sendMess(channel, "I'm sorry, " + user + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                            break;
                        case "!dc"://delete command
                                fail = true;
                                for (int a = 0; a < comlist.Count() && fail; a++)
                                {
                                    if (comlist[a].doesMatch(str[1])) { 
                                        comlist.RemoveAt(a);
                                        fail = false;
                                        new SQLiteCommand("DELETE FROM commands WHERE keyword='"+str[1]+"';",dbConn).ExecuteNonQuery();
                                        sendMess(channel, user + "-> command \"" + str[1] + "\" has been deleted.");
                                        safe();
                                        break; }

                                }
                                if (fail)
                                {
                                    sendMess(channel, "I'm sorry, " + user + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                            break;
                        case "!aa": //add alias
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
                                    new SQLiteCommand("INSERT INTO aliases (keyword,toword) VALUES ('"+str[1]+"','"+str[2]+"');", dbConn).ExecuteNonQuery();
                                    sendMess(channel, user + " -> alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                                    safe();
                                }
                            break;
                        case "!da":
                                fail = true;
                                foreach (ali c in aliList)
                                {
                                    if (c.delFrom(str[1]))
                                    {
                                        new SQLiteCommand("DELETE FROM aliases WHERE keyword='" + str[1] + "';", dbConn).ExecuteNonQuery();
                                        sendMess(channel, user + " -> Alias \"" + str[1] + "\" removed.");
                                        if (c.getFroms().Count() == 0) { aliList.Remove(c); }
                                        fail = false;
                                        safe();
                                        break;
                                    }
                                }
                                if (fail) { sendMess(channel, "I'm sorry, " + user + ". I couldn't find any aliases that match it. (maybe it's a command?)"); }
                            break;
                        case "!set"://!set <name> <level>
                            if (Regex.Match(str[2], "^([0-" + auth + "])|(-1)$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
                            {
                                setAuth(str[1].ToLower(), int.Parse(str[2]));
                                sendMess(channel, user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ".");
                                safe();
                            }
                            else
                            {
                                sendMess(channel, "I'm sorry, " + user + ". You either lack the authorisation to give such levels, or that level is not a valid number.");
                            }
                            break;
                        case "!editcount":
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
                                        new SQLiteCommand("UPDATE commands SET count='"+str[2]+"' WHERE keyword = '"+str[1]+"';",dbConn).ExecuteNonQuery();
                                        sendMess(channel, user + "-> the count of \"" + str[1] + "\" has been updated to " + str[2] + ".");
                                        safe();
                                    }
                                }
                            break;
                        case "!banuser":
                                if (auth > pullAuth(str[1]))//should prevent mods from banning other mods, etc.
                                {
                                    setAuth(str[1], -1);
                                    sendMess(channel, user + "-> \"" + str[1] + "\" has been banned from using bot commands.");
                                }

                            break;
                        case "!unbanuser":
                                if (pullAuth(str[1]) == -1)
                                {
                                    setAuth(str[1], 0);
                                    sendMess(channel, user + "-> \"" + str[1] + "\" has been unbanned.");
                                }

                            break;
                        case "!silence":
                            if (Regex.Match(str[1], "(on)|(off)|1|0|(true)|(false)|(yes)|(no)", RegexOptions.IgnoreCase).Success) { sendMess(channel, "Silence has been set to: " + str[1]); }
                            if (Regex.Match(str[1], "(on)|1|(true)|(yes)", RegexOptions.IgnoreCase).Success) { silence = true; new SQLiteCommand("UPDATE settings SET silence=1;", dbConn).ExecuteNonQuery(); }
                            if (Regex.Match(str[1], "(off)|0|(false)|(no)",RegexOptions.IgnoreCase).Success) { silence = false; new SQLiteCommand("UPDATE settings SET silence=0;", dbConn).ExecuteNonQuery(); }
                            
                            break;

                        case "!rank":
                            string text ="";
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
                            sendMess(channel, user + ", you are "+text+".");
                            break;

                        case "!permit":
                            if (antispam)
                            {
                                permits.Add(new intStr(str[1], getNow()));
                                sendMess(channel, str[1].Substring(0, 1).ToUpper() + str[1].Substring(1) + ", you have been granted permission to post a link by " + user+". This permit expires in "+permitTime+" seconds.");
                            }
                            break;
///////////////////////////////////begin RNGPP catered stuff                    //////////////////////////////////
                        case "!setbias":
                            tempVar2 = str[1];
                            tempVar2 = tempVar2.ToLower().Replace("up-left", "7");
                            tempVar2 = tempVar2.ToLower().Replace("up-right", "9");
                            tempVar2 = tempVar2.ToLower().Replace("up", "8");
                            tempVar2 = tempVar2.ToLower().Replace("neutral", "5");
                            tempVar2 = tempVar2.ToLower().Replace("down-left", "1");
                            tempVar2 = tempVar2.ToLower().Replace("down-right", "3");
                            tempVar2 = tempVar2.ToLower().Replace("left", "4");
                            tempVar2 = tempVar2.ToLower().Replace("right", "6");
                            tempVar2 =  tempVar2.ToLower().Replace("start","0");//f00?
                            if (Regex.Match(tempVar2, @"^[0-9ab]$").Success)
                            {
                                //code for tempVar2 setting here
                            }     
                        
                            break;
                        case "!bias":
                            if (voteStatus == 1)
                            {
                                tempVar2 = str[1];
                                tempVar2 = tempVar2.ToLower().Replace("up-left", "7");
                                tempVar2 = tempVar2.ToLower().Replace("up-right", "9");
                                tempVar2 = tempVar2.ToLower().Replace("up", "8");
                                tempVar2 = tempVar2.ToLower().Replace("neutral", "5");
                                tempVar2 = tempVar2.ToLower().Replace("down-left", "1");
                                tempVar2 = tempVar2.ToLower().Replace("down-right", "3");
                                tempVar2 = tempVar2.ToLower().Replace("left", "4");
                                tempVar2 = tempVar2.ToLower().Replace("right", "6");
                                tempVar2 = tempVar2.ToLower().Replace("start", "0");//f00?
                                if (Regex.Match(tempVar2, @"^[0-9ab]$").Success)
                                {
                                    tempVar1 = 1;
                                    if (Regex.Match(str[2], @"^[0-9]+\b").Success)
                                    {
                                        try//don't trust this one bit.
                                        {
                                            tempVar1 = int.Parse(str[2].Split(new string[] { " " }, 2, StringSplitOptions.None)[0]);
                                        }
                                        catch
                                        {
                                            logger.WriteLine("IRC: parsing error in bias vote, send more robots!");
                                        }
                                    }
                                    if (tempVar1 - 1 <= getPoints(user) && tempVar1 != 0)
                                    {
                                        fail = false;
                                        foreach (intStr IS in votingList)
                                        {
                                            if (IS.s() == user) { fail = true; break; }
                                        }
                                        if (!fail)
                                        {
                                            votingList.Add(new intStr(user, tempVar1));
                                            addPoints(user, tempVar1 - 2, "vote");
                                            //code for bias voting here
                                            //biascontrol.addvote(user,tempVar2,tempVar1);//<user>,<dir>,<amount>
                                        }
                                    }
                                    if (tempVar1 == 0)
                                    {
                                        fail = true; int a = 0;
                                        foreach (intStr IS in votingList)
                                        {
                                            if (IS.s() == user) { fail = false; break; }
                                            a++;
                                        }
                                        if (!fail)
                                        {
                                            addPoints(user, votingList[a].i() + 2, "refundvote");
                                            votingList.RemoveAt(a);
                                            //remove vote (biascontrol.removevote(user)
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sendMess(channel, "No vote currently in progress, try again in " + (((lastVoteTime + timeBetweenVotes)- getNow())/60 )+" minutes.");
                            }
                            break;
                        case "!balance":
                            tempVar1 = getPoints(user);tempVar2 = "";
                            if (tempVar1 == 0)
                            {
                                tempVar2 = "zero";
                            }
                            else if (tempVar1 == 1||tempVar1 == -1)
                            {
                                tempVar2 = tempVar1+" pokéDollar";
                            }
                            else
                            {
                                tempVar2 = tempVar1+ "PokéDollars";
                            }
                            sendMess(channel, user+", your balance is "+tempVar2+".");
                            break;

                        case "!addlog":
                            appendFile(progressLogPATH, getNowExtended()+" "+user+ " "+str[1] + str[2]);
                            sendMess(channel,"Affirmative, " + user+"!");
                            break;

                        case "!save":
                            //code to save here, if done, uncomment next line
                            //sendMess(channel, user + "-> Saved game to oldest slot.");
                            break;
                    }
                    break;


                }
            }

            if (!done)
            {
                foreach (command c in comlist)//flexible commands
                {
                    if (c.doesMatch(message) && c.canTrigger() && c.getAuth() <= auth)
                    {
                        if (logLevel == 1) { logger.WriteLine("IRC:<- <" + user +">" +message); }
                        str = c.getResponse(message, user);
                        c.addCount(1);
                        new SQLiteCommand("UPDATE commands SET count = '" + c.getCount() + "' WHERE keyword = '" + c.getKey() + "';");
                        if (str.Count() != 0) { if (str[0] != "") { c.updateTime(); } }
                        foreach (string b in str)
                        {
                            sendMess(channel, b);
                        }
                    }
                }
            }
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
        public void setPoints(string user, int amount)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET money='" + amount + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + user + "','" + amount + "','FORCED CHANGE TO AMOUNT','"+sqldr.GetString(0)+"','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,money) VALUES ('" + user + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
            }
        }
        public int addPoints(string name, int amount,string why)
        {
            int things;
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0)+amount;
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' money='"+ things+"' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + name + "','" + amount + "','"+why+"','" + sqldr.GetString(0) + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return things;
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen,money) VALUES ('" + name + "','" + getNowSQL() + "','"+amount+"');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }

        //eventbinders
        public void ircConnected(object sender, EventArgs e)
        {
            IrcClient a = (IrcClient)sender;
            a.Login(bot_name, "HARBBOT", 0, bot_name, oauth);
            a.RfcJoin(channels);
            logger.WriteLine("IRC: Joined Twitch chat");
            a.Listen();
        }

        public void ircJoined(object sender, EventArgs e)
        {
            logger.WriteLine("IRC: Succesfully joined " + channels);
        }

        public void ircConError(object sender, EventArgs e)
        {

        }

        public void ircError(object sender, EventArgs e)
        {
            reconnect();
        }
        public void ircRaw(object sender, IrcEventArgs e)
        {
            if (logLevel == 3)
            {
                logger.WriteLine("IRC RAW:<- " + e.Data.RawMessage);
            }
        }
        public void ircChanMess(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            if (logLevel == 2) { logger.WriteLine("IRC: <-" + channel + ": <" + nick + "> " + message); }
            message = message.TrimEnd();
            if (antispam) { checkSpam(channel, nick, message); };
            message = filter(message);
            this.checkCommand(channel, nick, message);
        }
        public void ircChanActi(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            message = message.Remove(0, 8);
            message = message.Remove(message.Length - 1);
            if (logLevel == 2) { logger.WriteLine("<-" + channel + ": " + nick + " " + message); }
        }
        public void ircQuery(object sender, EventArgs e)
        {

        }
        public void safe()//saves all data
        {
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

        }

        public static string MySqlEscape(string usString)
        {
            if (usString == null)
            {
                return null;
            }
            // it escapes \r, \n, \x00, \x1a, baskslash, single quotes, and double quotes
            return Regex.Replace(usString, "[\\r\\n\\x00\\x1a\\\'\"]", @"\$1");
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