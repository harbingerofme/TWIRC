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

        public static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
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

            try//connection test
            {
                
                irc.Connect("irc.twitch.tv", 6667);
            }
            catch (ConnectionException e)
            {
                System.Console.WriteLine("Connection error: " + e.Message);
            }
            try
            {
                irc.RfcJoin("#harbbot");
                irc.Listen();
                irc.Disconnect();
            }
            catch
            {

            }
            
        }
    }
}
