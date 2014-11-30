using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Meebey.SmartIrc4net;

namespace TWIRC
{ //name will be changed later, care for it.
    class Our_own_implementation
    {
        public static IrcClient irc = new IrcClient();
        public static IrcClient irc2 = new IrcClient();//backup connection (to check if our messages arive)

        public string bot_name = "HarbBot";//gonna hardcode this in for now, but will be loaded from a file soonish
        public string[] channels = { "#rngplayspokemon","#harbbot","harbingerofme"}
        public string oauth = "oauth:474uj7qa2pqjcjevugz8sph6ncq4rd0";//might be invalid


        public static void Main(string[] args)
        {

            //loadInformation();

            Thread.CurrentThread.Name = "Main";
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
            irc2.onRawMessage += irc2Raw;

            try//connection test
            {
                
                irc.Connect("irc.twitch.tv", 6667);
                irc2.Connect("irc.twitch.tv",6667);
            }
            catch (ConnectionException e)
            {
                System.Console.WriteLine("Connection error: " + e.Message);
            }
            
        }

       public void ircConnected(object sender, EventArgs e)
       {
           IrcClient a = (IrcClient) sender;
           a.Login(bot_name,"HARBBOT",0,bot_name,oauth);
           a.RfcJoin(channels);
           a.Listen();
       }
    }
}
