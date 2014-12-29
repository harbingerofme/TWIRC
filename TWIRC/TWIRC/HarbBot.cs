using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Meebey.SmartIrc4net;
using System.IO;
using System.Text.RegularExpressions;

namespace TWIRC
{
    public class HarbBot
    {
        public static IrcClient irc = new IrcClient();
        public bool running = true;

        public string bot_name;
        public string[] channels;
        public string oauth;
        public List<command> comlist = new List<command>();
        public List<ali> aliList = new List<ali>();
        public List<hardCom> hardList = new List<hardCom>();
        public bool hasSend;
        public int time;
        public int globalCooldown;
        public bool antispam = false;

        //debug these
        public bool debug_mode = true;//Remove me
        public List<string> opList;//might be deleted
        public List<string> adminList;
        public List<string> trustList;
        public List<string> regList;
        public List<string> banList;
        //Really


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
            /*DEBUG*/
            if (debug_mode)
            {
                try{
                Console.WriteLine("Debug mode enabled, this takes names from text files instead of a database!\nWe are not checking if the files exist.");
                adminList = FileLines("DEBUG_admin.txt").ToList();
                opList = FileLines("DEBUG_op.txt").ToList();
                trustList = FileLines("DEBUG_trust.txt").ToList();
                regList = FileLines("DEBUG_reg.txt").ToList();
                banList = FileLines("DEBUG_ban.txt").ToList();

                }
                catch
                {
                    Console.WriteLine("Failed to open files (do they exist?), maybe it's incorrect data, idk, I don't check.");
                    Environment.Exit(404);
                }
            }
            /*DEBUG*/

            if (File.Exists("Settings.txt"))
            {
                try
                {
                    string[] tempString = FileLines("Settings.txt");
                    string previousLine = null; ;
                    foreach (string tempString1 in tempString)
                    {
                        if (tempString1.Length > 0)
                        {
                            if (previousLine != null)
                            {
                                if (previousLine[0] == '@' && tempString1[0] != '@')
                                {
                                    switch (previousLine[1])
                                    {
                                        case '0': if (Regex.Match(tempString1.ToLower(), @"^[a-z0-9\\/_\.]+$").Success) { bot_name = tempString1.ToLower(); } else { throw new Exception("invalid name"); }; break;//Name
                                        case '1': if (Regex.Match(tempString1.ToLower(), @"^oauth:[a-z0-9]+$").Success) { oauth = tempString1.ToLower(); } else { throw new Exception("invalid oauth"); }; break;//oauth
                                        case '2': string[] tempString2 = tempString1.Split(','); foreach (string tempString3 in tempString2) { if (!Regex.Match(tempString3, @"^#[a-z0-9\\/_\.]+$").Success) { throw new Exception("invalid channel(s)"); } } channels = tempString2; ; break;//channels (this one is a bit trickier, it loops through all specified channels, and if one of them fails, it will throw the exception, since the program breaks when an exception is thrown, we can safely make tempString2 into channels.
                                        case '3': if (Regex.Match(tempString1, @"^[0-9]+$").Success) { globalCooldown = int.Parse(tempString1); } else { throw new Exception("invalid cooldown"); }; break;//cooldown
                                    }
                                }
                            }
                            previousLine = tempString1;
                            
                        }
                        
                    }
                    Console.WriteLine("Settings loaded, connecting as " + bot_name + ".");
                }
                catch(Exception e)
                {
                    Console.WriteLine("Critical error: failed to load the configuration: "+e.Message+"\n Press enter to close the application");
                    Console.ReadLine();
                    Environment.Exit(0);//F00barbob, pay attention here if attempting to merge, this will close everything down, might not be intended behavour for the entire program
                }
            }
            else//settings missing, so make a new file
            {
                Console.WriteLine("Settings file non-existant, making a new one.\n Application will close, be sure to enter configuration.");
                writeFile("Settings.txt", //some weird alligning here, because we are reading the string literally
@"Welcome to the settings file of the bot.
You'll want to enter valid info after each line starting with @#
@0 Here you enter the botname, please avoid using capitals (although it shouldn't matter)
samplename
@1 Enter your oauth token below, if you don't know what that is, you probably shouldn't be messing with this file, as I assume some technical know how.
oauth:thisisasampletoken123
@2 Enter the channels you want here preceded by a # and seperated by a comma (nothing else). I'm pretty sure multiple channels doesn't work, but if I ever fix it, it should work
#harbbot,#example
@3 Enter the cooldown of commands below (time it takes before the same command can be called again, (doesn't apply to mods/above))
20");
                Console.ReadLine();
                Environment.Exit(0);//F00barbob, pay attention here if attempting to merge, this will close everything down, might not be intended behavour for the entire program
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
            hardList.Add(new hardCom("!ac", 3, 2));//addcom (reduced now, so it doesn't conflict with nightbot)
            hardList.Add(new hardCom("!dc", 3, 1));//delcom
            hardList.Add(new hardCom("!ec", 3, 2));//editcom
            hardList.Add(new hardCom("!aa", 3, 2));//addalias
            hardList.Add(new hardCom("!da", 3, 1));//delete alias
            hardList.Add(new hardCom("!set", 2, 2));//elevate another user
            hardList.Add(new hardCom("!editcount", 3, 2));
            hardList.Add(new hardCom("!banuser", 3, 1));
            hardList.Add(new hardCom("!strip", 4, 1));


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
                Thread.Sleep(60000);//every SECOND
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
                        case "!ac":
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
                            break;
                        case "!ec":
                            fail = true;
                            for (int a = 0; a < comlist.Count() && fail; a++)
                            {
                                if (comlist[a].doesMatch(str[1])) {
                                    tempVar1 = 0;
                                    if (Regex.Match(str[2], @"@level(\d)@").Success) { tempVar1 = int.Parse(Regex.Match(str[2], @"@level(\d)@").Groups[1].Captures[0].Value); tempVar2 = str[3]; if (tempVar1 >= 5) { tempVar1 = 5; } }
                                    else { tempVar2 = str[2] + " " + str[3]; }
                                    tempVar3 = tempVar2.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    comlist[a].setResponse(tempVar3);
                                    comlist[a].setAuth(tempVar1);
                                    sendMess(channel, user+ "-> command \""+str[1]+"\" has been edited.");
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
                                if (comlist[a].doesMatch(str[1])) { comlist.RemoveAt(a); fail = false; sendMess(channel, user + "-> command \"" + str[1] + "\" has been deleted."); safe(); break; }

                            }
                            if (fail)
                            {
                                sendMess(channel, "I'm sorry, " + user + ". I can't find a command named that way. (maybe it's an alias?)");
                            }
                                break;
                        case "!aa": //add alias
                            fail = false;
                            foreach(command c in comlist){if(c.doesMatch(str[1])){fail=true;break;}}
                            foreach(hardCom c in hardList){if(c.doesMatch(str[1])||fail){fail=true;break;}}
                            foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; }}
                            if(fail){sendMess(channel,"I'm sorry, "+user+". A command or alias with the same name exists already.");}
                            else
                            {
                                fail = true;
                                foreach (ali c in aliList)
                                {
                                    if (c.getTo() == str[2]) { c.addFrom(str[1]); fail = false; break; }
                                }
                                if (fail) { aliList.Add(new ali(str[1],str[2])); }
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
                                    sendMess(channel, user + " -> Alias \""+str[1]+"\" removed.");
                                    if (c.getFroms().Count() == 0) { aliList.Remove(c); }
                                    fail = false;
                                    safe();
                                    break;
                                }
                            }
                            if (fail) { sendMess(channel,"I'm sorry, "+user+". I couldn't find any aliases that match it. (maybe it's a command?)");}
                            break;
                        case "!set"://!set <name> <level>
                            if(Regex.Match(str[2],"^[0-"+pullAuth(user,channel)+"]$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
                            {
                                setAuth(str[1].ToLower(), int.Parse(str[2]));
                                sendMess(channel, user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ". Note that this doesn't replace any other auth levels");
                                safe();
                            }
                            else
                            {
                                sendMess(channel, "I'm sorry, " + user + ". You either lack the authorisation to give such levels, or that level is not a vald number.");
                            }
                            break;
                        case "!editcount":
                            fail = true;
                            if(!Regex.Match(str[2],@"^\d+$").Success){
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
                            break;
                        case "!banuser":
                            banList.Add(str[1]);
                            sendMess(channel, user + "-> \""+ str[1] + "\" has been banned from using bot commands (note that this only works for people without a rank)");
                            safe();
                            break;
                        case "!strip":
                            opList.Remove(str[1]);
                            banList.Remove(str[1]);
                            trustList.Remove(str[1]);
                            regList.Remove(str[1]);
                            sendMess(channel,user + "-> \""+ str[1]+ "\" has been stripped from all ranks (except admin or broadcaster) and has been unbanned.");
                            safe();
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

        public int pullAuth(string name, string channel)
        {
            //fancy code to puul from the database
            //here
            //and here
            if (debug_mode)
            {
                if (adminList.Contains(name))
                {
                    return 5;//botAdmin
                }
                if ("#"+name == channel)
                {
                    return 4;//Broadcaster
                }
                if (opList.Contains(name))
                {
                    return 3;//mod
                }
                if (trustList.Contains(name))
                {
                    return 2; // (manual)
                }
                if (regList.Contains(name))
                {
                    return 1;//regular //should be auto
                }
                if (banList.Contains(name))
                {
                    return -1;//banned
                }
                return 0;
            }
            return 0;
        }
        public void setAuth(string user, int level)
        {
            //fancy code to edit database
            //here
            //and here
            if (debug_mode)
            {
                switch(level){
                    case 1:
                        regList.Add(user);
                        break;
                    case 2:
                        trustList.Add(user);
                        break;
                    case 3:
                        opList.Add(user);
                        break;
                    case 5:
                        adminList.Add(user);
                        break;
                    case -1:
                        banList.Add(user);
                        break;
                }
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
            if (debug_mode)
            {
                temp ="";
                foreach(string c in opList){temp+=c+ "\n";}
                writeFile("DEBUG_op",temp);
                temp = "";
                foreach (string c in trustList) { temp += c + "\n"; }
                writeFile("DEBUG_trust", temp);
                temp = "";
                foreach (string c in adminList) { temp += c + "\n"; }
                writeFile("DEBUG_admin", temp);
                temp = "";
                foreach (string c in regList) { temp += c + "\n"; }
                writeFile("DEBUG_reg", temp);
                temp = "";
                foreach (string c in banList) { temp += c + "\n"; }
                writeFile("DEBUG_ban", temp);
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