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
        public bool hasSend;
        public int time;
        public int globalCooldown;
        public bool antispam = false;

        //debug these
        public bool debug_mode = true;//Remove me
        public string[] opList;//might be deleted
        public string[] adminList;
        public string[] trustList;
        public string[] regList;
        public string[] banList;
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
                Console.WriteLine("Debug mode enabled, this takes names from text files instead of a database!\nWe are not checking if the files ex");
                adminList = FileLines("DEBUG_admin.txt");
                opList = FileLines("DEBUG_op.txt");
                trustList = FileLines("DEBUG_trust.txt");
                regList = FileLines("DEBUG_reg.txt");
                banList = FileLines("DEBUG_ban.txt");

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
                    string[] tempString = FileLines("Commands.twirc");
                    foreach (string tempString1 in tempString)
                    {
                        aliList.Add(new ali(tempString1));
                    }
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


            /*
            comlist.Add(new command("!harbbot", "Heyo, @user@!"));
            comlist.Add(new command("!morepars","This is a mandatory parameter: #par1#, while this is not: @par2@"));
            comlist.Add(new command("!countExample", "This command has been called @count@ times!"));
            comlist.Add(new command("!parexample", "You said \"@par1@\", followed by \"@par2@\", and then ended it all with \"@par3-@\"."));
            comlist.Add(new command("!rnd", "6 Random numbers between other things: @rand1-10@, @rand20-40@, @ran90-130@, @rand23-29@, @rand900-1200@, @rand0-2014@"));
            comlist[2].setCount(230);
            //*debug*/

            two = new Thread(run_2);//manages saving of commandlists, etc.
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
            string temp;
            while (true)
            {
                Thread.Sleep(60000);//every min  
                temp = "";
                foreach (com acom in comlist)
                {
                    temp += acom.ToString() + "\n";
                }
                writeFile("Commands.twirc", temp);//we can be sure it works, but I don't feel comfortable overwrtiting the backup.
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
            int a = 0;
            string[] str;



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
                            c.updateTime();
                            foreach (string b in str)
                            {
                                sendMess(channel, b);
                                Console.WriteLine("->" + channel + ": " + b);
                            }
                        }
                    }
                }
                System.Diagnostics.Debug.Write(".\n");
                a++;
            }
        }

        public void sendMess(string channel, string message)
        {
            hasSend = true;
            time = getNow();
            irc.SendMessage(SendType.Message, channel, message);
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