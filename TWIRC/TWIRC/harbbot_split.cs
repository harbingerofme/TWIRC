﻿using System;
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
    public partial class HarbBot  //deals with text handling
    {
        /// INDEX:
        /// sendMess(string1,string2) --            Sends string2 to specified channel string1, DOES NOT CHECK IF CONNECTED
        /// ircChanActi(object,IrcEventArgs) --     Eventhandler for channel actions (like "/me is amazing"), prints to console, then stores the message
        /// ircChanMess(object,IrcEventArgs) --     Eventhandler for channel messages, 
        ///   |                                     stores the message, prints it to console, 
        ///   |                                     checks if it's spam (if bot = mod and antispam enabled),    (checkspam)
        ///   |                                     otherwise, 
        ///   |                                     if the user is not banned from bot commands, 
        ///   |                                     checks for bot commands,                                   (checkCommands)
        ///   |                                     if no matching commands founds, and the user does not have a rank and the user is new,
        ///   |                                     display a welcome messssage.                                (newMessage)
        ///   |__                                   IF any of these actions fail, it will print to console that it almost crashed. 
        /// checkSpam(string1,string2,string3) --   Cleans up list of spammers and permits,   
        ///   |                                     checks if it's a single letter, 3 same letters in a row, 4 not alphanumerical characters in a row, if so, deduct aspoints
        ///   |                                     checks if it's a single word of more than 40 characters, if so, deduct points
        ///   |                                     check if it's an url, and check if it has a valid TLD, if so, remove permit OR deduct points(normally enough to instantly silence a user), excepts links to replays from current channel
        ///   |                                     If the points from the user are less than 0 at this point, INSTANTLY send a timeout, and then normally sends a message with why they were timed out.
        ///   |__                                   (if it's zack, and he does that god awful wix1,wix2,wix3,wix4 emote, clear the chat)
        /// filter(string) --                       replaces the beginning of a message with it's alias if it has one. (If somehow a train of aliases is generated, they will be replacing each other in timestamp order)
        /// checkCommand(string1,string2,string3) -- Check if it's a hardcoded command
        ///   |                                      in order: !ac !ec !dc !addalias !delalias !set !editcount !banuser !unbanuser !silence !rank !permit !whitelist [!addlua] [!dellua] !setbias !setdefaultbias !setbiasmaxdiff !addbias !delbias !bias !balance !setpoints !addlog !voting !save !rngppcommands !givemoney !giveball !background
        ///   |                                      if no matches were found
        ///   |                                      check if it's a softcommand, if so:
        ///   |__                                    update it's count and lasttime (for cooldown purposes), and send response
        /// newMessage(string) --                    Sends 1 of 3 welcome messages (chosen at random) to the chat.




        public void sendMess(string channel, string message)
        {
            if (logLevel > 0)
            {
                logger.WriteLine("IRC: ->" + channel + ": " + message);
            }
            if (!silence)
            {
                irc.SendMessage(SendType.Message, channel, message);
                storeMessage(bot_name, message);
            }
        }

        public void ircChanActi(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            message = message.Remove(0, 8);
            message = message.Remove(message.Length - 1);
            if (logLevel == 2) { logger.WriteLine("<-" + channel + ": " + nick + " " + message); }
            storeMessage(nick, "/me " + message);
        }


        public void ircChanMess(object sender, IrcEventArgs e)
        {
            bool a = false;
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            int level = pullAuth(nick);
            storeMessage(nick, message);
            try
            {
                if (logLevel == 2) { logger.WriteLine("IRC: <-" + channel + ": <" + nick + "> " + message); }
                message = message.TrimEnd();
                if (antispam && isMod) { a = checkSpam(channel, nick, message); };
                if (!a && level >= 0)
                {
                    message = filter(message);
                    a = checkCommand(channel, nick, message);
                }
                if (a == false && level == 0 && isNew(nick))
                {
                    if (!message.StartsWith("wtf"))
                    {
                        newMessage(nick);
                    }
                    notNew(nick);
                }
            }
            catch (Exception exc)
            {
                logger.WriteLine("IRC: Crisis adverted: " + exc.Message + " :: Message: <" + nick + "> " + message);
            }
        }




        public bool checkSpam(string channel, string user, string message)
        {
            List<asUser> temp = new List<asUser>(); List<intStr> temp2 = new List<intStr>();
            foreach (asUser person in asUsers)
            {
                if (person.lastUpdate < getNow() - asCooldown) { temp.Add(person); }
                if (person.points < 1) { person.points = 2; }//resets the person's limit if they misused it, but keeps it within quick timeout range.
            }
            foreach (asUser person in temp)
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
                int type = -1;
                if (a == -1)
                {
                    a = asUsers.Count;
                    asUsers.Add(new asUser(user, pullAuth(user)));
                }
                if (message != "")
                {
                    message = message.ToLower();
                    if (Regex.Match(message, @"^.$").Success || Regex.Match(message, @"([a-zA-Z])\1\1").Success || Regex.Match(message, @"([0-9])\1\1\1").Success || Regex.Match(message, @"([^[0-9a-zA-Z]]){4}").Success) { asUsers[a].update(asCosts[2].Int); type = 2; }//either a single letter, 3 same letters in a row, 4 not alphanumerical characters in a row,
                    if (message.Length > 40 && Regex.Match(message, @"^[^[a-zA-Z]]*$").Success) { asUsers[a].update(asCosts[3].Int); type = 3; }

                    MatchCollection mc = Regex.Matches(message, @"[^ ]+\.([a-z]{2,})[\/\?\#]?".ToLower());
                    MatchCollection me = Regex.Matches(message, @"([^ ]+\.[a-z]{2,})[\/\?\#]?".ToLower());
                    int b = mc.Count; int d = 0; ;
                    if (b > 0)
                    {
                        foreach (Match c in mc)
                        {
                            if (asTLDs.Contains(c.Groups[1].Value.ToUpper())) { d++; }
                        }
                        foreach (Match c in me)
                        {
                            if (Regex.Match(message, @"twitch\.tv\/" + channel.Substring(1) + @"\/c\/").Success) { d--; continue; }
                            foreach (string e in asWhitelist)
                            {
                                if (Regex.Match(c.Value, e).Success) { d--; continue; }

                            }
                        }
                        foreach (intStr f in permits)
                        {
                            if (f.Str == user) { b = 0; permits.Remove(f); break; }
                        }
                        if (d > 0) { asUsers[a].update(asCosts[0].Int); type = 0; }
                    }
                }
                if (type != -1 && asUsers[a].points < 1)
                {
                    irc.RfcPrivmsg(channel, ".timeout " + user + " 1");//overrides the send delay (hopefully)
                    int c = new Random().Next(0, 4);
                    sendMess(channel, user + " -> " + asResponses[type][c] + " (" + asCosts[type].Str + ")");
                    return true;
                }
            }
            if (user == "zackattack9909" && Regex.Match(message, "wix[1-4]").Success) { irc.RfcPrivmsg(channel, ".clear"); sendMess(channel, "Zack, please don't."); }
            return false;
        }

        public string filter(string message)
        {
            string result = message;
            foreach (ali alias in aliList)
            {
                result = alias.filter(result);//shouldn't matter much
            }
            return result;
        }


        public bool checkCommand(string channel, string user, string message)
        {
            string[] str, tempVar3;
            bool done = false; int auth = pullAuth(user);
            bool fail; int tempVar1 = 0; string tempVar2 = "";
            string User = user.Substring(0, 1).ToUpper() + user.Substring(1);
            foreach (hardCom h in hardList)//hardcoded command
            {
                if (h.hardMatch(user, message, auth))
                {
                    done = true;
                    str = h.returnPars(message);
                    if (logLevel == 1) { logger.WriteLine("IRC:<- <" + user + "> " + message); }
                    switch (h.returnKeyword())
                    {
                        case "!ac":
                            fail = false;

                            foreach (command c in comlist) { if (c.doesMatch(str[1])) { fail = true; break; } }
                            foreach (hardCom c in hardList) { if (c.doesMatch(str[1]) || fail) { fail = true; break; } }
                            foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; } }
                            if (fail) { sendMess(channel, "I'm sorry, " + User + ". A command or alias with the same name exists already."); }
                            else
                            {
                                tempVar1 = 0;
                                if (Regex.Match(str[2], @"@level(\d)@").Success) { tempVar1 = int.Parse(Regex.Match(str[2], @"@level(\d)@").Groups[1].Captures[0].Value); tempVar2 = str[3]; if (tempVar1 >= 5) { tempVar1 = 5; } }
                                else { tempVar2 = str[2] + " " + str[3]; }
                                tempVar3 = tempVar2.Split(new string[] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
                                comlist.Add(new command(str[1], tempVar3, tempVar1));
                                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO commands (keyword,authlevel,count,response) VALUES (@par1,'" + tempVar1 + "','" + 0 + "',@par2);", dbConn);
                                cmd.Parameters.AddWithValue("@par1", str[1].ToLower());
                                cmd.Parameters.AddWithValue("@par2", tempVar2);
                                cmd.ExecuteNonQuery();
                                sendMess(channel, User + " -> command \"" + str[1] + "\" added. Please try it out to make sure it's correct.");
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
                                    SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET response = @par1, authlevel=@par3 WHERE keyword=@par2;", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", tempVar2); cmd.Parameters.AddWithValue("@par2", str[1]); cmd.Parameters.AddWithValue("@par3", tempVar1);
                                    cmd.ExecuteNonQuery();
                                    sendMess(channel, User + "-> command \"" + str[1] + "\" has been edited.");
                                    fail = false;
                                }
                            }
                            if (fail)
                            {
                                sendMess(channel, "I'm sorry, " + User + ". I can't find a command named that way. (maybe it's an alias?)");
                            }
                            break;
                        case "!dc"://delete command
                            fail = true;
                            for (int a = 0; a < comlist.Count() && fail; a++)
                            {
                                if (comlist[a].doesMatch(str[1]))
                                {
                                    comlist.RemoveAt(a);
                                    fail = false;
                                    SQLiteCommand cmd = new SQLiteCommand("DELETE FROM commands WHERE keyword=@par1;", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", str[1]);
                                    cmd.ExecuteNonQuery();
                                    sendMess(channel, User + "-> command \"" + str[1] + "\" has been deleted.");
                                    break;
                                }

                            }
                            if (fail)
                            {
                                sendMess(channel, "I'm sorry, " + User + ". I can't find a command named that way. (maybe it's an alias?)");
                            }
                            break;
                        case "!addalias": //add alias
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
                                    if (c.getTo() == str[2])
                                    {
                                        c.addFrom(str[1]);
                                        fail = false;
                                        string gatherer = "";
                                        foreach (string tempAli in c.getFroms())
                                        {
                                            gatherer += tempAli + " ";
                                        }
                                        gatherer = gatherer.TrimEnd();
                                        SQLiteCommand cmd = new SQLiteCommand("UPDATE aliases SET keyword = @par1 WHERE toword = @par2;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", gatherer); cmd.Parameters.AddWithValue("@par2", str[2]);
                                        cmd.ExecuteNonQuery();
                                        break;
                                    }
                                }
                                if (fail)
                                {
                                    aliList.Add(new ali(str[1], str[2]));
                                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO aliases (keyword,toword) VALUES (@par1,@par2);", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", str[1]); cmd.Parameters.AddWithValue("@par2", str[2]);
                                    cmd.ExecuteNonQuery();
                                }
                                sendMess(channel, User + " -> alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                            }
                            break;
                        case "!delalias":
                            fail = true;
                            foreach (ali c in aliList)
                            {
                                if (c.delFrom(str[1]))
                                {
                                    sendMess(channel, user + " -> Alias \"" + str[1] + "\" removed.");
                                    if (c.getFroms().Count() == 0)
                                    {
                                        aliList.Remove(c);
                                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM aliases WHERE keyword=@par1;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", str[1]); cmd.ExecuteNonQuery();
                                    }
                                    else
                                    {
                                        string gatherer = "";
                                        foreach (string tempAli in c.getFroms())
                                        {
                                            gatherer += tempAli + " ";
                                        }
                                        gatherer = gatherer.TrimEnd();
                                        SQLiteCommand cmd = new SQLiteCommand("UPDATE aliases SET keyword = @par1 WHERE toword = @par2;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", gatherer); cmd.Parameters.AddWithValue("@par2", c.getTo());
                                        cmd.ExecuteNonQuery();
                                        break;
                                    }
                                    fail = false;
                                    break;
                                }
                            }
                            if (fail) { sendMess(channel, "I'm sorry, " + User + ". I couldn't find any aliases that match it. (maybe it's a command?)"); }
                            break;
                        case "!set"://!set <name> <level>
                            if (!Regex.Match(str[1].ToLower(), @"^[a-z0-9_]+$").Success) { sendMess(channel, "I'm sorry, " + User + ". That's not a valid name."); }
                            else
                            {
                                if (Regex.Match(str[2], "^([0-" + auth + "])|(-1)$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
                                {
                                    setAuth(str[1].ToLower(), int.Parse(str[2]));
                                    sendMess(channel, user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ".");
                                }
                                else
                                {
                                    sendMess(channel, "I'm sorry, " + User + ". You either lack the authorisation to give such levels to that person, or that level is not a valid number.");
                                }
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
                                    SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET count='" + str[2] + "' WHERE keyword = @par1;", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", str[1]);
                                    cmd.ExecuteNonQuery();
                                    sendMess(channel, user + "-> the count of \"" + str[1] + "\" has been updated to " + str[2] + ".");
                                }
                            }
                            break;
                        case "!banuser":
                            if (auth > pullAuth(str[1]))//should prevent mods from banning other mods, etc.
                            {
                                setAuth(str[1], -1);
                                sendMess(channel, User + "-> \"" + str[1] + "\" has been banned from using bot commands.");
                            }

                            break;
                        case "!unbanuser":
                            if (pullAuth(str[1]) == -1)
                            {
                                setAuth(str[1], 0);
                                sendMess(channel, User + "-> \"" + str[1] + "\" has been unbanned.");
                            }

                            break;
                        case "!silence":
                            if (Regex.Match(str[1], "^((on)|(off)|1|0|(true)|(false)|(yes)|(no))$", RegexOptions.IgnoreCase).Success) { sendMess(channel, "Silence has been set to: " + str[1]); }
                            if (Regex.Match(str[1], "^((on)|1|(true)|(yes))$", RegexOptions.IgnoreCase).Success) { silence = true; setSetting("silence", "bit", "1"); }
                            if (Regex.Match(str[1], "^((off)|0|(false)|(no))$", RegexOptions.IgnoreCase).Success) { silence = false; setSetting("silence", "bit", "0"); }

                            break;

                        case "!rank":
                            string text = "";
                            switch (auth)
                            {
                                case 0: text = "a user"; break;
                                case 1: text = "a regular"; break;
                                case 2: text = "trusted"; break;
                                case 3: text = "a moderator"; break;
                                case 4: text = "the broadcaster"; break;
                                case 5: text = "an administrator of " + bot_name; break;
                                default: text = "special"; break;
                            }
                            sendMess(channel, User + ", you are " + text + ".");
                            break;

                        case "!permit":
                            if (antispam)
                            {
                                permits.Add(new intStr(str[1], getNow()));
                                sendMess(channel, str[1].Substring(0, 1).ToUpper() + str[1].Substring(1) + ", you have been granted permission to post a link by " + User + ". This permit expires in " + permitTime + " seconds.");
                            }
                            break;

                        case "!whitelist":
                            if (antispam)
                            {
                                if (auth == 3 && message.Split(' ').Count() >= 3)
                                {
                                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO aswhitelist (name,regex) VALUES (@par1,@par2) VALUES", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", message.Split(' ')[1]);
                                    cmd.Parameters.AddWithValue("@par2", message.Split(new string[] { " " }, 3, StringSplitOptions.None)[2]);
                                    cmd.ExecuteNonQuery();
                                    asWhitelist.Add(message.Split(new string[] { " " }, 3, StringSplitOptions.None)[2]);
                                    asWhitelist2.Add(message.Split(' ')[1]);
                                    sendMess(channel, User + "-> I've added it to the whitelist, I can't guarantee any results.");
                                }
                                else
                                {
                                    if (asWhitelist.Count == 0)
                                    {
                                        tempVar2 = "There are no whitelisted links.";
                                    }
                                    if (asWhitelist.Count == 1)
                                    {
                                        tempVar2 = "The only whitelisted website is " + asWhitelist2[0];
                                    }
                                    if (asWhitelist.Count > 1)
                                    {
                                        tempVar2 = "Whitelisted websites are: ";
                                        foreach (string tempStr1 in asWhitelist2)
                                        {

                                            tempVar2 += tempStr1 + ", ";
                                        }
                                        tempVar2 = tempVar2.Substring(0, tempVar2.Length - 2);
                                        tempVar2 += ".";
                                    }
                                    sendMess(channel, tempVar2);
                                }
                            }
                            break;
                        case "!calculate":
                            tempVar2 = str[1] + str[2];
                            Calculation calc = calculator.Parse(tempVar2);
                            if (calc.Valid)
                            {
                                sendMess(channel, "Answer: "+ calc.Answer+". Interpreted as: "+calc.Input+".");
                            }
                            else
                            {
                                sendMess(channel,"I'm sorry, either your calculation is wrong, or I am not programmed yet to be able to read it.");
                            }
                            break;
                        case "!addlua"://<keyword> <command> [default (if parameter is omitted)]

                            break;
                        case "!dellua"://<keyword>

                            break;
                        ///////////////////////////////////begin RNGPP catered stuff                    //////////////////////////////////
                        case "!setbias":
                            double[] tobebias = new double[7]; fail = false;
                            for (int a = 1; a < 8; a++)
                            {
                                str[a] = str[a].Replace(',', '.');//So people for who 0,1 == 0.1 also can do stuff (like me)
                                if (!Regex.Match(str[a], @"^([01][\.][0-9]{1,9})|(1)$").Success)
                                {
                                    fail = true;
                                    break;
                                }
                                else
                                {
                                    tobebias[a - 1] = double.Parse(str[a]);
                                }
                            }
                            if (!fail)
                            {
                                biasControl.setBias(tobebias);
                                luaServer.send_to_all("SETBIAS", "MANUAL");
                                sendMess(channel, User + "-> Bias set!");
                            }
                            else
                            {
                                sendMess(channel, User + "-> Atleast one of the values wasn't correct. Nothing has been changed.");
                            }
                            break;
                        case "!setdefaultbias":
                            double[] tobedefaultbias = new double[7]; fail = false;
                            for (int a = 1; a < 8; a++)
                            {
                                str[a] = str[a].Replace(',', '.');//So people for who 0,1 == 0.1 also can do stuff (like me)
                                if (!Regex.Match(str[a], @"^([01][\.][0-9]{1,9})|(1)$").Success)
                                {
                                    fail = true;
                                    break;
                                }
                                else
                                {
                                    tobedefaultbias[a - 1] = double.Parse(str[a]);
                                }
                            }
                            if (!fail)
                            {
                                biasControl.setDefaultBias(tobedefaultbias);
                                string sqlStr="";
                                for (int a = 0; a < 7; a++)
                                {
                                    sqlStr += tobedefaultbias[a].ToString();
                                    if (a != 6) { sqlStr += ":"; }
                                }
                                setSetting("defaultbias", "string", sqlStr);
                                sendMess(channel, User + "-> Default bias set! I really hope you know what you are doing.");
                            }
                            else
                            {
                                sendMess(channel, User + "-> Atleast one of the values wasn't correct. Nothing has been changed.");
                            }
                            break;
                        case "!setbiasmaxdiff":
                            str[1] = str[1].Replace(",", ".");//make it accessible for dutchies ( we use commas to define floating points here (and dots for thousands).)
                            if (Regex.Match(str[1], @"^([01]\.[0-9]{1,9})|(1)").Success)
                            {
                                maxBiasDiff = double.Parse(str[1]);
                                setSetting("biasmaxdiff", "double", "" + maxBiasDiff);
                                sendMess(channel, User + "-> Max bias difference updated, this will take effect after the next vote.");
                            }
                            else
                            {
                                sendMess(channel, User + "-> Value in incorrect format, no changes made.");
                            }
                            break;
                        case "!bias":
                            if (voteStatus == 1)
                            {
                                tempVar3 = (str[1] + " " + str[2]).Split(' ');//merge all 
                                Bias q = null;
                                tempVar1 = 1;
                                foreach (Bias b in biasList)
                                {
                                    if (str[1].ToLower() == b + "")
                                    {
                                        q = b;
                                        break;
                                    }
                                }
                                if (Regex.Match(str[2], @"^1?[0-9]{1,9}\b").Success)//is the second "word" a number?
                                {
                                    try//don't trust this one bit.
                                    {
                                        tempVar1 = int.Parse(str[2].Split(new string[] { " " }, 2, StringSplitOptions.None)[0]);//try to parse it as the number for bias votes
                                    }
                                    catch
                                    {
                                        fail = true;
                                        logger.WriteLine("IRC: parsing error in bias vote, send more robots!");//has never happened, ever. Will be removed soon to improve code quality.
                                    }
                                }
                                if (tempVar3.Length > 6)//if the bias votes contrains enough words for a biasnumbers vote
                                {
                                    fail = false;
                                    double[] dbl = new double[7];
                                    for (int a = 0; a < 7; a++)
                                    {
                                        if(Regex.Match(tempVar3[a],@"^(10|[0-9](\.[0-9]){0,1})$").Success)
                                        {
                                            dbl[a] = double.Parse(tempVar3[a]);//try to parse these numbers...
                                        }
                                        else
                                        {
                                            fail = true;
                                            break;
                                        }
                                    }
                                    if (!fail)
                                    {
                                        q = new Bias("custom", dbl);
                                        tempVar1 = 1;
                                        try
                                        {
                                            tempVar1 = int.Parse(tempVar3[7]);
                                            if(tempVar1<1)
                                            {
                                                tempVar1 = 1;
                                            }
                                        }
                                        catch { };
                                    }

                                }
                                if (q != null && (tempVar1 - 2) * moneyPerVote <= getPoints(user))//(7-2) * 50 <= 50... What the fuck.
                                {
                                    addVote(user, q, tempVar1);
                                }
                            }
                            else
                            {
                                if (getNow() - lastVoteTime > 30)
                                {
                                    sendMess(channel, "No vote currently in progress, try again in " + (((lastVoteTime + timeBetweenVotes) - getNow()) / 60 + 1) + " minutes.");
                                }
                                else
                                {
                                    sendMess(channel, "You just missed it, sorry!");
                                }
                            }
                            break;
                        case "!balance":
                            tempVar1 = getPoints(user); tempVar2 = "";
                            if (tempVar1 == 0)
                            {
                                tempVar2 = "zero";
                            }
                            else if (tempVar1 == 1 || tempVar1 == -1)
                            {
                                tempVar2 = tempVar1 + " PokéDollar";
                            }
                            else
                            {
                                tempVar2 = tempVar1 + " PokéDollars";
                            }
                            tempVar1 = getAllTime(user);
                            sendMess(channel, User + ", your balance is " + tempVar2 + ". (" + tempVar1 + ")");
                            break;
                        case "!setpoints":
                            if (Regex.Match(str[2], "^([1-9][0-9]{1,8}|[0-9])$").Success)
                            {
                                setPoints(str[1], int.Parse(str[2]));
                                sendMess(channel, "Points have been changed.");
                            }
                            break;
                        case "!check":
                            sendMess(channel, str[1].Substring(0, 1).ToUpper() + str[1].Substring(1).ToLower() + " has " + getPoints(str[1].ToLower()) + " pokédollars. (" + getAllTime(str[1]) + ")");
                            break;

                        case "!addlog":
                            appendFile(progressLogPATH, "\n" + getNowExtended() + " " + User + " " + str[1] + " " + str[2]);
                            sendMess(channel, "Affirmative, " + User + "!");
                            break;

                        case "!resetbias":
                            biasControl.setBias(biasControl.getDefaultBias());
                            sendMess(channel, User + "-> Bias reset.");
                            break;

                        case "!voting":
                            if (Regex.Match(str[1], @"^(1)|(on)|(true)|(yes)|(positive)$").Success)
                            {
                                if (voteStatus == -1)
                                {
                                    voteStatus = 1;
                                    sendMess(channel, "Voting for bias now possible again! Type !bias <direction> [amount of votes] to vote! (For example \"!bias 3\" to vote once for down-right, \"!bias up 20\" would put 20 votes for up at the cost of some of your pokédollars)");
                                    voteTimer2.Start();
                                }
                                else
                                {
                                    try { voteTimer.Stop(); voteTimer2.Stop(); }
                                    catch { }
                                    voteStatus = 1;
                                    voteTimer2.Start();
                                    sendMess(channel, "Voting started by " + User + ".");
                                }
                            }
                            else
                            {
                                if (Regex.Match(str[1], @"^(0)|(off)|(false)|(no)|(negative)$").Success)
                                {
                                    if (voteStatus != -1)
                                    {
                                        voteStatus = -1;
                                        sendMess(channel, "Voting disabled until bot or chat restart.");
                                        try { voteTimer.Stop(); voteTimer2.Stop(); }
                                        catch { }
                                    }
                                }
                            }
                            break;

                        case "!save":
                            tempVar2 = str[1];
                            luaServer.send_to_all("SAVE", tempVar2);
                            sendMess(channel, User + "-> Saved game with parameter '" + tempVar2 + "'.");
                            break;

                        case "!rngppcommands":
                            sendMess(channel, User + "-> commands are located at " + commandsURL + " .");
                            break;

                        case "!delbias":
                            if (delBias(str[1]))
                            {
                                sendMess(channel, "Bias deleted.");
                            }
                            else
                            {
                                sendMess(channel, "No bias by that name exists");
                            }
                            break;

                        case "!addbias":
                            fail = false;
                            for (int i = 2; i < 9; i++)
                            {
                                if (Regex.Match(str[i], @"^[0-9](\.[1-9])?$|^10$").Success)
                                { tempVar2 += str[i] + " "; }
                                else
                                {
                                    fail = true; break;
                                }
                            }
                            if (!fail)
                            {
                                tempVar2 = tempVar2.Trim();
                                Bias k = new Bias(str[1].ToLower(), tempVar2);
                                foreach (Bias b in biasList)
                                {
                                    if (b.Equals(k))
                                    {
                                        k.numbers = b.numbers;
                                        k.factor = b.factor;
                                        break;
                                    }
                                }
                                if (addBias(k + "", k.strNumbers()))
                                {
                                    biasList.Add(k);
                                    sendMess(channel, User + "-> Bias added!");
                                }
                                else
                                {
                                    sendMess(channel, "Bias with that keyword already exists.");
                                }
                            }
                            else
                            {
                                sendMess(channel, "One or more numbers were incorrect. (Use up to one floating point values between and including 0 and 10)");
                            }

                            break;

                        case "!givemoney":

                            bool givemoneysucces = false;
                            if (Regex.Match(str[1], @"^[1-9]([0-9]{1,8})?$").Success)
                            {
                                if (int.Parse(str[1]) * 2 <= getPoints(user))
                                {
                                    addPoints(user, int.Parse(str[1]) * -2, "Money to game");
                                    luaServer.send_to_all("ADDMONEY", str[1]);
                                    sendMess(channel, User + " converted " + int.Parse(str[1]) * 2 + " of their funds into " + str[1] + " PokéDollar for ?birja.");
                                    givemoneysucces = true;
                                }
                                else
                                {
                                    sendMess(channel, User + "-> You have insufficient funds for this. Please round up some more.");
                                }
                            }
                            else
                            {
                                sendMess(channel, "Not a (valid) number.");
                            }
                            if (!givemoneysucces)
                            {
                                h.removeFromCD(user);
                            }
                            break;
                        case "!giveball":
                            bool giveballsucces = false;
                            if (1500 <= getPoints(user))
                            {
                                addPoints(user, -1500, "ball to game");
                                luaServer.send_to_all("ADDBALLS", "1");
                                sendMess(channel, User + " gave ?birja a pokéball.");
                                giveballsucces = true;
                            }
                            else
                            {
                                sendMess(channel, User + "-> You have insufficient funds for this. Please round up some more.");
                            }
                            if (!giveballsucces)
                            {
                                h.removeFromCD(user);
                            }
                            break;
                        case "!background":
                            if (backgrounds_enabled)
                            {
                                if (backgrounds != 0)
                                {
                                    if (Regex.Match(str[1], @"^[1-9]([0-9]{1,9})?$").Success)
                                    {
                                        if (int.Parse(str[1]) <= backgrounds && int.Parse(str[1]) > 0)
                                        {
                                            if (getPoints(user) >= 500)
                                            {
                                                bool succeeded = false;
                                                try
                                                {
                                                    File.Copy(backgroundPATH + "background_" + str[1] + ".png", backgroundPATH + "background.gif", true);
                                                    succeeded = true;
                                                }
                                                catch { }
                                                try
                                                {
                                                    File.Copy(backgroundPATH + "background_" + str[1] + ".gif", backgroundPATH + "background.gif", true);
                                                    succeeded = true;
                                                }
                                                catch { }
                                                try
                                                {
                                                    File.Copy(backgroundPATH + "background_" + str[1] + ".jpg", backgroundPATH + "background.gif", true);
                                                    succeeded = true;
                                                }
                                                catch { }
                                                if (!succeeded)
                                                {

                                                    sendMess(channel, "Something went wrong, no PokéDollars deducted.");
                                                }
                                                else
                                                {
                                                    addPoints(user, -500, "background");
                                                    sendMess(channel, User + " changed the background of the stream for 500 PokéDollars!");
                                                }
                                            }
                                            else
                                            {
                                                sendMess(channel, User + "-> You have insufficient funds for this. Please round up some more.");
                                            }
                                        }
                                        else
                                        {
                                            sendMess(channel, User + "-> I do not have any backgrounds with that number, please try a different one.");
                                        }
                                    }
                                    else
                                    {
                                        sendMess(channel, "That's not a valid number, sweetie. Try using (positive) integers?");
                                    }
                                }
                                else
                                {
                                    sendMess(channel, "There are no backgrounds, WINSANE PLS!");
                                }
                            }
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
                        if (logLevel == 1) { logger.WriteLine("IRC:<- <" + user + ">" + message); }
                        done = true;
                        str = c.getResponse(message, user);
                        c.addCount(1);
                        SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET count = '" + c.getCount() + "' WHERE keyword = @par1;", dbConn);
                        cmd.Parameters.AddWithValue("@par1", c.getKey());
                        cmd.ExecuteNonQuery();
                        if (str.Count() != 0) { if (str[0] != "") { c.updateTime(); } }
                        foreach (string b in str)
                        {
                            sendMess(channel, b);
                        }
                    }
                }
            }
            return done;
        }


        public void newMessage(string user)
        {
            string output = ""; string usr = user.Substring(0, 1).ToUpper() + user.Substring(1);
            int now = getNow();
            if (now > lastWelcomeMessageTime + welcomeMessageCD)
            {
                lastWelcomeMessageTime = now;
                int rgn = new Random().Next(3);
                switch (rgn)
                {
                    case 0: output = "Hello " + usr + "! Welcome to RNGPlaysPokemon. If you are new, it might be worthwile to take a look at the FAQ, or type !what."; break;
                    case 1: output = "All welcome " + usr + " to the chat. (also, " + usr + ", try !what)"; break;
                    case 2: output = "Heyo, " + usr + ". This channel is a random number generator playing pokémon, very fancy team rocket science stuff. (try !what)."; break;
                }
                sendMess(channel, output);
            }

        }

        public void addVote(string user, Bias b, int amount)
        {
            var money = 0;
            var x = -1;
            for(int a = 0; a<votingList.Count; a++)
            {
                if (votingList[a].Str == user.ToLower())
                {
                    x = a;
                    break;
                }
            }
            if(x>-1)
            {
                money -= (votingList[x].Int - 1) * moneyPerVote;
                votingList.RemoveAt(x);
                votinglist.RemoveAt(x);
            }
            if(amount!=0){
                money += ((1 - amount) * moneyPerVote);
                votingList.Add(new intStr(user, amount));
                votinglist.Add(b);
                if(x == -1)
                {
                    money += moneyPerVote;
                    addAllTime(user, moneyPerVote);
                }
            }
            addPoints(user, money, "vote");

        }
    }
}