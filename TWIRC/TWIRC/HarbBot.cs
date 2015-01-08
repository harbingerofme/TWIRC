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

namespace TWIRC
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
        SQLiteConnection dbConn;


        public Thread two;

        public HarbBot()
        {
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
            Console.WriteLine("Booting up, shouldn't take long!");
            string temp;
            if(!File.Exists("db.sqlite"))
            {
                Console.WriteLine("First time setup detected, please enter the following data:\nThe name of the bot (default harbbot):");
                temp = Console.ReadLine();
                while (!Regex.Match(temp, @"^[\w_]*$").Success) { temp = Console.ReadLine();}
                if (temp != "") { bot_name = temp; } else { bot_name = "harbbot"; }

                Console.WriteLine("Your channel (default harbbot):");
                channels = "" ;
                while (channels == "")
                {
                    temp = Console.ReadLine();
                    if (Regex.Match(temp, @"^[\w_]+$").Success) { channels = "#" + temp; }
                    if (Regex.Match(temp, @"^#[\w_]+$").Success) { channels = temp; }
                    if (temp == "") { channels = "#harbbot";}
                }

                Console.WriteLine("The cooldown on commands (default 20):");
                temp = Console.ReadLine();
                if (temp == "") { globalCooldown = 20; }
                else
                {
                    while (!Regex.Match(temp, @"^\d+$").Success) { temp = Console.ReadLine(); }
                    globalCooldown = int.Parse(temp);
                }

                Console.WriteLine("Whether or not the antispam module is enabled (default yes):");
                temp = Console.ReadLine();
                while (!Regex.Match(temp, @"^()|(yes)|(no)|(1)|(0)$", RegexOptions.IgnoreCase).Success) { temp = Console.ReadLine(); }
                switch (temp)
                {
                    case "no": antispam = false; break;
                    case "0": antispam = false; break;
                    default: antispam = true; break;
                }

                Console.WriteLine("And now lastly, I need you to open up oauth.txt and paste the bot's oauth there. Type \"done\" once you've done so.");
                oauth = ""; writeFile("oauth.txt", ""); string[] temp3;
                while (oauth == "")
                {
                    temp = Console.ReadLine();
                    if(temp == "done")
                    {
                        temp3 = FileLines("oauth.txt");
                        if (temp3.Count()>0) {
                            if (Regex.Match(temp3[0], @"^oauth:\w+$",RegexOptions.IgnoreCase).Success) { oauth = temp3[0].ToLower(); }
                            if (Regex.Match(temp3[0], @"^\w+$", RegexOptions.IgnoreCase).Success) { oauth = "oauth:"+temp3[0].ToLower(); }
                        }
                    }
                    if(oauth==""){
                        Console.WriteLine("Oauth invalid, please try again.");
                    }
                }
                File.Delete("oauth.txt");
                Console.WriteLine("That was it, you are done configuring the bot from the command line, if you want to change any of these settings at a later point in time, open the included Settings.exe");

                short temp2 = 0;if(antispam){temp2 =1;}
                SQLiteConnection.CreateFile("db.sqlite");
                dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                dbConn.Open();
                new SQLiteCommand("CREATE TABLE users (name VARCHAR(25) NOT NULL, rank INT DEFAULT 0, lastseen VARCHAR(7));", dbConn).ExecuteNonQuery();//lastseen is done in yyyyddd format. day as in day of year
                new SQLiteCommand("CREATE TABLE commands (keyword VARCHAR(60) NOT NULL, authlevel INT DEFAULT 0, count INT DEFAULT 0, response VARCHAR(1000));", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE aliases (keyword VARCHAR(60) NOT NULL, toword VARCHAR(1000) NOT NULL);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE settings (name VARCHAR(25) NOT NULL, channel VARCHAR(26) NOT NULL, antispam TINYINT(1) NOT NULL, oauth VARCHAR(200), cooldown INT);", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO settings (name,channel,antispam,oauth,cooldown) VALUES ('"+bot_name+"','"+channels+"','"+temp2+"','"+oauth+"','"+globalCooldown+"');",dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO users (name,rank,lastseen) VALUES ('"+channels.Substring(1)+"','4','"+getNowSQL()+"');", dbConn).ExecuteNonQuery();
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
                oauth = sqldr.GetString(3);
                globalCooldown = sqldr.GetInt32(4);
            }



            if (File.Exists("Commands.twirc"))
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
                    Console.WriteLine("Loaded up "+comlist.Count()+ " commands.");
                    File.Copy("Commands.twirc", "backupCommands.twirc",true);
                }
                catch
                {
                    Console.WriteLine("'Commands.twirc' contains an error and I was unable to parse it. Please check the file.");
                }
            }
            else
            {
                Console.WriteLine("Command File non-existant, making a new one. (no commands loaded, except hardcoded ones)");
                writeFile("Commands.twirc","");
            }


            if(File.Exists("Aliases.twirc"))
            {
                try
                {
                    string[] tempString = FileLines("Aliases.twirc");
                    foreach (string tempString1 in tempString)
                    {
                        aliList.Add(new ali(tempString1));
                    }
                    Console.WriteLine("Loaded up " + aliList.Count() + " aliases.");
                    File.Copy("Aliases.twirc", "backupAliases.twirc", true);
                }
                catch
                {
                    Console.WriteLine("'Aliases.twirc' contains an error and I was unable to parse it. Please check the file.");
                }
            }
            else
            {
                Console.WriteLine("Aliases File non-existant, making a new one. (none loaded)");
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


            two = new Thread(run_2);//manages saving of commandlists, etc.
            two.Start();
            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (ConnectionException e) { System.Diagnostics.Debug.WriteLine("Thread 1 Connection error: " + e.Message); }
        }

        public void reconnect()
        { 
            try
            {
                irc.Disconnect();
            }
            catch { };
            try{
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch (ConnectionException e) {
                Console.Write("Connection error: " + e.Message + ". Retrying in 5 seconds.");
                Thread.Sleep(5000);
                reconnect(); 
            };

        }

        public void run_2()
        {       
            while (true)
            {
                Thread.Sleep(60000);
                safe();
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
            string[] str,tempVar3;
            bool done = false;
            bool fail; int tempVar1 = 0; string tempVar2 = "";
            foreach (hardCom h in hardList)//hardcoded command
            {
                if (h.hardMatch(message))
                {
                    done = true;
                    str = h.returnPars(message);
                    switch (h.returnKeyword())
                    {
                        case "!adco":
                            if (pullAuth(user, channel)>2){
                            fail = false;
                            
                            foreach(command c in comlist){if(c.doesMatch(str[1])){fail=true;break;}}
                            foreach(hardCom c in hardList){if(c.doesMatch(str[1])||fail){fail=true;break;}}
                            foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; }}
                            if(fail){sendMess(channel,"I'm sorry, "+user+". A command or alias with the same name exists already.");}
                            else
                            {
                                tempVar1 = 0;
                                if (Regex.Match(str[2], @"@level(\d)@").Success) { tempVar1 = int.Parse(Regex.Match(str[2], @"@level(\d)@").Groups[1].Captures[0].Value); tempVar2 = str[3]; if (tempVar1 >= 5) { tempVar1 = 5; } }
                                else { tempVar2 = str[2]+" "+ str[3]; }
                                tempVar3 = tempVar2.Split(new string[] {"\\n"},StringSplitOptions.RemoveEmptyEntries);
                                comlist.Add(new command(str[1], tempVar3, tempVar1));
                                sendMess(channel, user + " -> command \"" + str[1] + "\" added. Please try it out to make sure it's correct.");
                                safe();
                            }
                            }
                            break;
                        case "!ec":
                            if (pullAuth(user, channel) > 2)
                            {
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
                                        sendMess(channel, user + "-> command \"" + str[1] + "\" has been edited.");
                                        safe();
                                        fail = false;
                                    }
                                }
                                if (fail)
                                {
                                    sendMess(channel, "I'm sorry, " + user + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                            }
                            break;
                        case "!dc"://delete command
                            if (pullAuth(user, channel) > 2)
                            {
                                fail = true;
                                for (int a = 0; a < comlist.Count() && fail; a++)
                                {
                                    if (comlist[a].doesMatch(str[1])) { comlist.RemoveAt(a); fail = false; sendMess(channel, user + "-> command \"" + str[1] + "\" has been deleted."); safe(); break; }

                                }
                                if (fail)
                                {
                                    sendMess(channel, "I'm sorry, " + user + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                            }
                                break;
                        case "!aa": //add alias
                                if (pullAuth(user, channel) > 2)
                                {
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
                                        sendMess(channel, user + " -> alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                                        safe();
                                    }
                                }
                            break;
                        case "!da":
                            if (pullAuth(user, channel) > 2)
                            {
                                fail = true;
                                foreach (ali c in aliList)
                                {
                                    if (c.delFrom(str[1]))
                                    {
                                        sendMess(channel, user + " -> Alias \"" + str[1] + "\" removed.");
                                        if (c.getFroms().Count() == 0) { aliList.Remove(c); }
                                        fail = false;
                                        safe();
                                        break;
                                    }
                                }
                                if (fail) { sendMess(channel, "I'm sorry, " + user + ". I couldn't find any aliases that match it. (maybe it's a command?)"); }
                            }
                            break;
                        case "!set"://!set <name> <level>
                            if(Regex.Match(str[2],"^([0-"+pullAuth(user,channel)+"])|(-1)$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
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
                            if (pullAuth(user, channel) > 2)
                            {
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
                                        sendMess(channel, user + "-> the count of \"" + str[1] + "\" has been updated to " + str[2] + ".");
                                        safe();
                                    }
                                }
                            }
                            break;
                        case "!banuser":
                            if (pullAuth(user, channel) > 1)
                            {
                                if (pullAuth(user, channel) > pullAuth(str[1], channel))
                                {
                                    setAuth(str[1], -1);
                                    sendMess(channel, user + "-> \"" + str[1] + "\" has been banned from using bot commands.");
                                }
                            }
                            break;
                        case "!unbanuser":
                            if (pullAuth(user, channel) > 1)
                            {
                                if (pullAuth(str[1],channel) == -1)
                                {
                                    setAuth(str[1], 0);
                                    sendMess(channel, user + "-> \"" + str[1] + "\" has been unbanned.");
                                }
                            }
                            break;
                    }
                    break;
                }
            }

        if(!done){
            tempVar1 =0;
            foreach (command c in comlist)//flexible commands
            {
                if (c.doesMatch(message))
                {
                    System.Diagnostics.Debug.Write("A command has been matched!");
                    if(c.canTrigger())
                    {
                        System.Diagnostics.Debug.Write(": it can trigger");
                        if (c.getAuth() <= pullAuth(user,channel))
                        {
                            System.Diagnostics.Debug.Write(": auth level correct");


                            str = c.getResponse(message, user);
                            c.addCount(1);
                            if (str.Count() != 0) { if (str[0] != "") { c.updateTime(); } }
                            foreach (string b in str)
                            {
                                sendMess(channel, b);
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.Write(".\n");
                tempVar1++;
            }
        }
        }

        public void sendMess(string channel, string message)
        {
            Console.WriteLine("->" + channel + ": " + message);
            hasSend = true;
            time = getNow();
            //irc.SendMessage(SendType.Message, channel, message);
        }

        public int getNow(){
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
            return str;
        }

        public int pullAuth(string name, string channel)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT rank FROM users WHERE name='"+name+"';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {
                
                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('"+name+"','"+ getNowSQL() +"');", dbConn).ExecuteNonQuery();
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

        //eventbinders
        public void ircConnected(object sender, EventArgs e)
        {
            IrcClient a = (IrcClient)sender;
            a.Login(bot_name, "HARBBOT", 0, bot_name, oauth);
            a.RfcJoin(channels);
            Console.WriteLine("Joined Twitch chat");
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
            Console.WriteLine("<-" + channel + ": <" + nick + "> " + message);
            if (antispam) { checkSpam(channel, nick, message); };
            message = filter(message);
            this.checkCommand(channel,nick,message);
        }
        public void ircChanActi(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            message = message.Remove(0, 8);
            message = message.Remove(message.Length - 1);
            Console.WriteLine("<-" + channel + ": " + nick + " " + message);
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