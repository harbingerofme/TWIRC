using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Meebey.SmartIrc4net;

namespace TWIRC
{
    public class HarbBot
    {
        public static IrcClient irc = new IrcClient();
        public static IrcClient irc2 = new IrcClient();//backup connection (to check if our messages arive)
        public bool running = true;

        public string bot_name = "harbbot";//gonna hardcode this in for now, but will be loaded from a file soonish
        public string[] channels = { "#rngplayspokemon","harbbot"};
        public string oauth = "oauth:l3jjnxjgfvkjuqa7q9yabgcezm5qpsr";//might be invalid
        public List<com> comlist = new List<com>();
        public bool hasSend;
        public int time;

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
            irc2 = irc;

            irc.OnQueryMessage += ircQuery;
            irc.OnRawMessage += ircRaw;
            irc.OnChannelAction += ircChanActi;
            irc.OnChannelMessage += ircChanMess;
            irc2.OnChannelMessage += irc2ChanMess;
            
            /*debug*/
            string[] temp = { "Harb is the one who wrote my code","He's pretty cool for that","line 3","bla","more bla","line 6","another line","I should really limit this","line 9"};
            comlist.Add(new command("!harbbot", "Heyo, @user@!"));
            comlist.Add(new command("!longtext", temp,5));
            comlist.Add(new command("!countExample", "This command has been called @count@ times!"));
            comlist.Add(new command("!parexample", "You said \"@par1@\", followed by \"@par2@\", and then ended it all with \"@par3-@\"."));
            comlist.Add(new command("!rnd", "HEre's an example of rng'ing. @rand200@ <- random number between 0 and 200. @rand1-2@ <- either 1 or 2"));
            comlist[2].setCount(230);
            /*debug*/

            //two = new Thread(run_2);
            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (ConnectionException e) { System.Diagnostics.Debug.WriteLine("Thread 1 Connection error: " + e.Message); }
        }

        public void reconnect()
        {
            try
            {
                irc.Disconnect();
                irc2.Disconnect();
            }
            catch { };
            two.Abort();
            two = new Thread(run_2);
            try{
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch (ConnectionException e) {
                Console.Write("Connection error: " + e.Message + ". Retrying in 5 seconds.");
                Thread.Sleep(5000);
                reconnect(); 
            };

        }

        public void run_1()
        {
            Thread.Sleep(100);
                
        }

        public void run_2()
        {
            Thread.Sleep(1);
                try { irc2.Connect("irc.twitch.tv", 6667); }
                catch (ConnectionException e) { System.Diagnostics.Debug.WriteLine("Thread 2 Connection error: " + e.Message); }
        }

        public void checkSpam(string channel, string user, string message)
        {

        }

        public void checkCommand(string channel, string user, string message)
        {
            int a = 0;
            string[] str;
            foreach (com c in comlist)
            {
                System.Diagnostics.Debug.Write("Checking command");
                if (c.doesMatch(message))
                {
                    System.Diagnostics.Debug.Write(": it matches");
                    if(c.canTrigger())
                    {
                        System.Diagnostics.Debug.Write(": it can trigger");
                        str = c.getResponse(message,user);
                        foreach (string b in str)
                        {
                             sendMess(channel, b);
                            Console.WriteLine("->" + channel + ": " + b);
                            c.updateTime();
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
        public void irc2Raw(object sender, EventArgs e)
        {
            
        }
        public void irc2ChanMess(object sender, IrcEventArgs e)
        {
            if (hasSend)
            {
                if (e.Data.Nick == bot_name)
                {
                    hasSend = false;
                }
                
            }
        }
    }
}