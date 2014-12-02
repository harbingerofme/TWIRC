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
        public string[] channels = { "#rngplayspokemon", "#harbbot", "harbingerofme" };
        public string oauth = "oauth:l3jjnxjgfvkjuqa7q9yabgcezm5qpsr";//might be invalid

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
            irc2.OnQueryMessage += irc2Query;
            irc2.OnRawMessage += ircRaw;
            
            
            Thread two = new Thread(run_2);
            try { irc.Connect("irc.twitch.tv", 6667); }
            catch (ConnectionException e) { System.Diagnostics.Debug.WriteLine("Thread 1 Connection error: " + e.Message); }
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


        public void ircConnected(object sender, EventArgs e)
        {
            IrcClient a = (IrcClient)sender;
            a.Login(bot_name, "HARBBOT", 0, bot_name, oauth);
            a.RfcJoin(channels);
            a.Listen();
            System.Diagnostics.Debug.WriteLine("Joined Twitch chat");
        }

        public void ircJoined(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Joined a channel!");
        }

        public void ircConError(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Connection error");
        }

        public void ircError(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("IRC error");
        }
        public static void ircRaw(object sender, IrcEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("IRC1: " + e.Data.RawMessage);
        }
        public void ircQuery(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("query message on irc1");
        }
        public void irc2Raw(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("raw Message received on irc2");
        }
        public void irc2Query(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("query message on irc1");
        }
    }
}
