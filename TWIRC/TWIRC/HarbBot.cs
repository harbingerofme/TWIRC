using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
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
        public bool hasSend;
        public int time;
        public int globalCooldown;
        public bool antispam;
        public bool silence;
        SQLiteConnection dbConn;
        public Logger logger;


        public Thread two;

        public HarbBot(Logger logLogger)
        {
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
            logger.WriteLine("Booting up, shouldn't take long!");
            string temp;
            if (!File.Exists("db.sqlite"))
            {
                logger.WriteLine("First time setup detected, making database");
                bot_name = "harbbot";
                channels = "#rngplayspokemon";
                globalCooldown = 20; 
                antispam = true;
                oauth = "oauth:l3jjnxjgfvkjuqa7q9yabgcezm5qpsr";

                short temp2 = 0; if (antispam) { temp2 = 1; }
                SQLiteConnection.CreateFile("db.sqlite");
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7), money INT DEFAULT 0);", dbConn).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE settings (name VARCHAR(25) NOT NULL, channel VARCHAR(26) NOT NULL, antispam TINYINT(1) NOT NULL, silence TINYINT(1) NOT NULL, oauth VARCHAR(200), cooldown INT);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE transactions (name VARCHAR(25) NOT NULL, amount INT NOT NULL,item VARCHAR(1024) NOT NULL,prevMoney INT NOT NULL,date VARCHAR(7) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO settings (name,channel,antispam,silence,oauth,cooldown) VALUES ('" + bot_name + "','" + channels + "','" + temp2 + "',0,'" + oauth + "','" + globalCooldown + "');", dbConn).ExecuteNonQuery();
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
            }

            SQLiteDataReader rdr = new SQLiteCommand("SELECT * FROM commands;", dbConn).ExecuteReader();
            while (rdr.Read())
            {
                try
                {
                    string[] tempString = FileLines("Commands.twirc");
                    foreach (string tempString1 in tempString)
                    {
                        if (tempString1.Length > 0)
                        {
                            if (tempString1[0] == '1')
                            {
                                comlist.Add(new command(tempString1));
                                comlist[comlist.Count - 1].setCooldown(globalCooldown);
                            }
                        }
                    }
                    logger.WriteLine("Loaded up " + comlist.Count() + " commands.");
                    File.Copy("Commands.twirc", "backupCommands.twirc", true);
                }
                catch
                {
                    logger.WriteLine("'Commands.twirc' contains an error and I was unable to parse it. Please check the file.");
                }
                string[] a = rdr.GetString(3).Split(new string[] {@"\n"},StringSplitOptions.RemoveEmptyEntries);
                command k = new command(rdr.GetString(0), a, rdr.GetInt32(1));
                k.setCount(rdr.GetInt32(2));
                k.setCooldown(globalCooldown);
                comlist.Add(k);
            }
            logger.WriteLine("Loaded " + comlist.Count() + " commands!");

            rdr = new SQLiteCommand("SELECT * FROM aliases;", dbConn).ExecuteReader();
            while (rdr.Read())
            {
                logger.WriteLine("Command File non-existant, making a new one. (no commands loaded, except hardcoded ones)");
                writeFile("Commands.twirc", "");
                string[] a = rdr.GetString(0).Split(' ');
                ali k = new ali(a, rdr.GetString(1));
                aliList.Add(k);
            }
            logger.WriteLine("Loaded " + aliList.Count() + " aliases!");

            if (File.Exists("Aliases.twirc"))
            {
                try
                {
                    string[] tempString = FileLines("Aliases.twirc");
                    foreach (string tempString1 in tempString)
                    {
                        aliList.Add(new ali(tempString1));
                    }
                    logger.WriteLine("Loaded up " + aliList.Count() + " aliases.");
                    File.Copy("Aliases.twirc", "backupAliases.twirc", true);
                }
                catch
                {
                    logger.WriteLine("'Aliases.twirc' contains an error and I was unable to parse it. Please check the file.");
                }
            }
            else
            {
                logger.WriteLine("Aliases File non-existant, making a new one. (none loaded)");
                writeFile("Aliases.twirc", "");
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
            hardlist.Add(new hardCom("!rank", 0, 0));

            //RNGPP catered commands, commented out means no way of implementing that yet.
            hardList.Add(new hardCom("!setbias",2,1));
            hardList.Add(new hardCom("!bias",0,1));
            hardList.Add(new hardCom("!balance", 0, 0));
            hardList.Add(new hardCom("!addlog", 0, 1));
            hardList.Add(new hardCom("!setpoints",4,2));
            //hardList.Add(new hardCom("!maintenance", 3, 1));
            //hardList.Add(new hardCom("!background",0,1));
            //hardList.Add(new hardCom("!song",0,1));
            hardList.Add(new hardCom("!save", 3, 0));
            
            //sayingsbot overrides, we might add these eventually            
            hardList.Add(new hardCom("!whois",0,1));
            hardList.Add(new hardCom("!editme",1,1));
            hardList.Add(new hardCom("!edituser",3,2));
            hardList.Add(new hardCom("!classic",0,1));
            hardList.Add(new hardCom("!addclassic",2,2));
            hardList.Add(new hardCom("!delclassic",2,2));

            
            two = new Thread(run_2);//manages saving of commandlists, etc.
            two.Name = "RNGPPBOT background irc thread.";
            two.IsBackground = true;
            two.Start();

#if !OFFLINE
            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (ConnectionException e) { System.Diagnostics.Debug.WriteLine("Thread 1 Connection error: " + e.Message); }
#endif


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
                logger.Write("Connection error: " + e.Message + ". Retrying in 5 seconds.");
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
            bool done = false;
            bool fail; int tempVar1 = 0; string tempVar2 = "";
            foreach (hardCom h in hardList)//hardcoded command
            {
                if (h.hardMatch(message,pullAuth(user)))
                {
                    done = true;
                    str = h.returnPars(message);
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
                            if (Regex.Match(str[2], "^([0-" + pullAuth(user) + "])|(-1)$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
                            {
                                setAuth(str[1].ToLower(), int.Parse(str[2]));
                                sendMess(channel, user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ".tha");
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
                                if (pullAuth(user) > pullAuth(str[1]))//should prevent mods from banning other mods, etc.
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
                            //needs a special antispam thing, before I implement this
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
                                if (tempVar1-1 <= getPoints(user)&&tempVar1!=0)
                                {
                                    addPoints(user, tempVar1 - 2,"vote");
                                    //code for bias voting here
                                    //biascontrol.addvote(user,tempVar2,tempVar1);//<user>,<dir>,<amount>
                                }
                                if (tempVar1 == 0)
                                { 
                                    //refund points (remove them if necessary)(, maybe a fancy look into the database? (or a biascontrol.getvote(user))
                                    //remove vote (biascontrol.removevote(user)
                                }
                            }     
                        
                            break;
                        case "":
                            break;

                    }
                    break;
                }
            }

            if (!done)
            {
                foreach (command c in comlist)//flexible commands
                {
                    if (c.doesMatch(message) && c.canTrigger() && c.getAuth() <= pullAuth(user))
                    {
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
            logger.WriteLine("->" + channel + ": " + message);
            hasSend = true;
            time = getNow();
            irc.SendMessage(SendType.Message, channel, message);
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
            logger.WriteLine("Joined Twitch chat");
            a.Listen();
        }

        public void ircJoined(object sender, EventArgs e)
        {

        }

        public void ircConError(object sender, EventArgs e)
        {

        }

        public void ircError(object sender, EventArgs e)
        {
            reconnect();
        }
        public static void ircRaw(object sender, IrcEventArgs e)
        {

        }
        public void ircChanMess(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            logger.WriteLine("<-" + channel + ": <" + nick + "> " + message);
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
            logger.WriteLine("<-" + channel + ": " + nick + " " + message);
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