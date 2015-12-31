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

namespace TWIRC
{
    public partial class HarbBot  //deals with text handling
    {
        public void sendMess(string channel, string message, int type = 3)
        {
            log(3,"->" + channel + ": " + message);
            if (!silence || type == 1)
            {
                irc.SendMessage(SendType.Message, channel, message);
                storeMessage(bot_name, message,type);
            }
        }

        void ircChanActi(object sender, IrcEventArgs e)
        {
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            int level = pullAuth(nick);
            bool b = false;
            message = message.Remove(0, 8);
            message = message.Remove(message.Length - 1);
            if (channel != "#" + bot_name)
            {
                message = message.TrimEnd();
                if (level == 0 && isNew(nick))
                {
                    if (antispam)
                        b = noBOTS(nick, message);
                    if (!b)
                    {
                        newMessage(nick);
                        notNew(nick);
                        storeMessage(nick, "/me " + message, 0);
                    }
                }
                else
                    storeMessage(nick, "/me " + message, 0);
            }
        }


        void ircChanMess(object sender, IrcEventArgs e)
        {
            bool a = false, b = false; int i = 0;
            string channel = e.Data.Channel;
            string nick = e.Data.Nick;
            string message = e.Data.Message;
            int level = pullAuth(nick);
            
#if !STRICT
            try
            {
#endif
            if (channel != "#" + bot_name)
            {
                message = message.TrimEnd();
                if (level == 0 && isNew(nick))
                {
                    if (antispam)
                        b = noBOTS(nick, message);
                    if (!b)
                    {
                        newMessage(nick);
                        notNew(nick);
                    }
                    else
                        i = 5;
                }
                if (!b && level >= 0)
                {
                    message = filter(message);
                    if (checkCommand(channel, nick, message))
                        i = 4;
                }
                if (!b)
                    storeMessage(nick, message, i);
            }
            else//This is #rngppbot
            {
                if (level == -2 && message.ToLower().StartsWith("i'm not a bot"))
                {
                    sendMess(channels[0], ".unban " + nick);
                    storeMessage("SYSTEM", "Unbanned: " + nick, 3);
                }
                if (message.ToLower().StartsWith("is mod?"))
                {
                    sendMess(channels[1], isMod.ToString(), 2);
                }
            }
#if !STRICT
            }
            catch (Exception exc)
            {
                log(0,"IRC: Crisis adverted: " + exc.Message + " :: Message: <" + nick + "> " + message);
            }
#endif
        }

        #region banMessages
        string[] banMessages = {
                                           "And another one down, and another one down, another one bites the dust! (TB)",
                                           "Piece of cake! (TB)",
                                           "There's only room for so many bots here. (TB)",
                                           "How about no? (TB)",
                                           "That makes TB.",
                                           "OMN NOM NOM! (TB)","And out goes the TBth candle.",
                                           "I am having none of that. (TB)",
                                           "My rule over this channel is supreme, you will not interfere. (TB)",
                                           "Error 403.2: Write access forbidden. (TB)",
                                           "Error 401.4: Authorization failed by filter.. (TB)",
                                           "Nonintellligent life detected... Assuming hostile intend... Purged. (TB)",
                                           "Trifling gnome! Your arrogance will be your undoing! (TB)",
                                           "CRUSH. KILL. DESTROY. (TB)",
                                           "Guess what belongs in the trash: it's you! (TB)",
                                           "-_- (TB)"
                                       };
        #endregion
        bool noBOTS(string nick, string message)
        {
            message = message.ToLower();
            if (Regex.Match(message, @".*([\w_\.-]+\.[\w]{2,}|bit_ly)[/\w]*\b.*").Success || (antistreambot && Regex.Match(message,"streambot").Success))
            {
                sendMess(channel, ".ban "+nick, 3);
                
                new SQLiteCommand("UPDATE users SET rank = -2 WHERE name = '" + nick + "' ;", dbConn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM messages WHERE name = '" + nick + "';", chatDbConn).ExecuteNonQuery();
                new SQLiteCommand("DELETE FROM users WHERE name = '" + nick + "';", chatDbConn).ExecuteNonQuery();
                new SQLiteCommand("UPDATE users SET lines = lines + 1 WHERE name = '#autoBans';",chatDbConn).ExecuteNonQuery();
                int totalbans = 1;
                SQLiteDataReader sqldr = new SQLiteCommand("SELECT lines FROM users WHERE name = '#autoBans';",chatDbConn).ExecuteReader();
                if(sqldr.Read())
                {
                    totalbans = sqldr.GetInt32(0);
                }
                else
                {
                    new SQLiteCommand("INSERT INTO users (name, lines) VALUES ('#autoBans',1);",chatDbConn).ExecuteNonQuery();
                }
                string returnMessage = banMessages[new Random().Next(banMessages.Length)].Replace("(TB)","("+totalbans+")");
                sendMess(channel, returnMessage+" (If you are not a bot, say \"I'm not a bot\" in my channel.)");
                return true;
            }
            else
                return false;
        }

        string filter(string message)
        {
            string result = message;
            foreach (ali alias in aliList)
            {
                result = alias.filter(result);
            }
            return result;
        }


        bool checkCommand(string channel, string user, string message)
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
                    switch (h.returnKeyword())
                    {
                        #region commands
                        #region addcom
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
                        #endregion
                        #region editcom
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
                        #endregion
                        #region delcom
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
                        #endregion
                        #region editcount
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
                        #endregion
                        #endregion
                        #region aliases
                        #region addalias
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
                        #endregion
                        #region delalias
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
                        #endregion
                        #endregion
                        #region userlevels
                        #region set
                        case "!set"://!set <name> <level>
                            if (!Regex.Match(str[1].ToLower(), @"^[a-z0-9_]+$").Success) { sendMess(channel, "I'm sorry, " + User + ". That's not a valid name."); }
                            else
                            {
                               if( int.TryParse(str[2],out tempVar1) && tempVar1 < auth && pullAuth(str[1]) < auth && tempVar1>-2 && tempVar1<6)
                               {
                                       setAuth(str[1], tempVar1);
                                       sendMess(channel, user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ".");
                                }
                                else
                                {
                                    sendMess(channel, "I'm sorry, " + User + ". You either lack the authorisation to give such levels to that person, or that level is not a valid number.");
                                }
                            }
                            break;
                        #endregion
                        #region banuser
                        case "!banuser":
                            if (auth > pullAuth(str[1]))//should prevent mods from banning other mods, etc.
                            {
                                setAuth(str[1], -1);
                                sendMess(channel, User + "-> \"" + str[1] + "\" has been banned from using bot commands.");
                            }

                            break;
                        #endregion
                        #region unbanuser
                        case "!unbanuser":
                            if (pullAuth(str[1]) == -1)
                            {
                                setAuth(str[1], 0);
                                sendMess(channel, User + "-> \"" + str[1] + "\" has been unbanned.");
                            }

                            break;
                        #endregion
                        #region rank
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
                        #endregion
                        #endregion
                        #region poll stuff
                        #region poll
                        case "!poll": switch (str[1].ToLower())
                            {
                                case "open":
                                    if (!poll_active)
                                    {
                                        if (str[2] != "")
                                        {
                                            tempVar3 = str[2].Split('|');
                                            poll_name = tempVar3[0];
                                            tempVar2 = User + " opened a poll for: '" + poll_name + "', with the options:";
                                            poll = new string[tempVar3.Length - 1];
                                            for (int i = 1; i < tempVar3.Length; i++)
                                            {
                                                poll[i - 1] = tempVar3[i];
                                                tempVar2 += " (" + i + ") '" + tempVar3[i] + "'.";
                                            }
                                            tempVar2 += " Use !vote X to cast your vote!";
                                            pollOpen();
                                            poll_active = true;
                                            sendMess(channel, tempVar2);
                                        }
                                        else
                                        {
                                            sendMess(channel, "The poll for '" + poll_name + "' has been re-opened! Type !vote X to vote!");
                                            poll_active = true;
                                        }
                                    }
                                    else
                                    {
                                        sendMess(channel, User + ", there's already a poll going for '" + poll_name + "', try closing that one first.");
                                    }

                                    break;
                                case "close": poll_active = false; sendMess(channel, "Poll has been closed."); sendMess(channel, "Results were: " + pollResults()); break;
                                case "results": if (poll_active) { sendMess(channel, "Current results are: " + pollResults()); } else { sendMess(channel, "Results were: " + pollResults()); } break;
                            }
                            break;
                        #endregion
                        #region vote
                        case "!vote":
                            if (poll_active)
                            {
                                if (int.TryParse(str[1], out tempVar1) && str[1] != "")
                                {
                                    if (tempVar1 <= poll.Length && tempVar1 > 0)
                                    {
                                        if (pollVote(user, tempVar1))
                                        {
                                            sendMess(channel, User + ", your vote has been cast for '" + poll[tempVar1 - 1] + "'.");
                                            h.cdlist.Add(new intStr(user, 5));
                                        }
                                        else
                                        {
                                            sendMess(channel, User + ", you've already cast your vote for this option.");
                                        }
                                    }
                                    else
                                    {
                                        sendMess(channel, "Not a valid option");
                                    }
                                }
                                else
                                {
                                    tempVar2 = "There's currently a poll running for: ' " + poll_name + "'. The options are:";
                                    for (int i = 0; i < poll.Length; i++)
                                    {
                                        tempVar2 += " (" + (i + 1) + ") '" + poll[i] + "'.";
                                    }
                                    tempVar2 += " Use !vote X to cast your vote!";
                                    sendMess(channel, tempVar2);
                                }
                            }
                            else
                            {
                                sendMess(channel, "No poll active.");
                            }
                            break;
                        #endregion
                        #endregion
                        
                        #region lua
                        case "!addlua"://<keyword> <command> [default (if parameter is omitted)]

                            break;
                        case "!dellua"://<keyword>

                            break;
                        #endregion

                        #region settings
                        #region setdefaultbias
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
                        #endregion
                        #region setbiasmaxdiff
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
#endregion
                        #region individual settings
                        case "!reloadsettings": loadSettings(); break;
                        case "!changesetting": if (setSetting(str[1], str[2], str[3])) { sendMess(channel, "Setting changed! Reloading settings.."); loadSettings(); } else { sendMess(channel, "Setting not found!"); }; break;
                        case "!addsetting": setSetting(str[1], str[2], str[3], true); sendMess(channel, "Setting added (following checks not preformed: validity, duplicate)."); break;
#endregion
                        #region voting
                        case "!voting":
                            if (Regex.Match(str[1], @"^(1)|(on)|(true)|(yes)|(positive)$").Success)
                            {
                                if (voteStatus == -1)
                                {
                                    voteStatus = 1;
                                    sendMess(channel, "Voting for bias now possible again! Type !bias <direction> [amount of votes] to vote! (For example \"!bias 3\" to vote once for down-right, \"!bias up 20\" would put 20 votes for up at the cost of some of your PokeDollars)");
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
#endregion
                        #region silence
                        case "!silence":
                            if (Regex.Match(str[1], "^((on)|(off)|1|0|(true)|(false)|(yes)|(no))$", RegexOptions.IgnoreCase).Success) { sendMess(channel, "Silence has been set to: " + str[1]); }
                            if (Regex.Match(str[1], "^((on)|1|(true)|(yes))$", RegexOptions.IgnoreCase).Success) { silence = true; setSetting("silence", "bit", "1"); }
                            if (Regex.Match(str[1], "^((off)|0|(false)|(no))$", RegexOptions.IgnoreCase).Success) { silence = false; setSetting("silence", "bit", "0"); }

                            break;
                        #endregion
                        #region goal related
                        #region goal
                        case "!goal": sendMess(channel, goal); break;
                        #endregion
                        #region setgoal
                        case "!setgoal": goal = str[1]; insertIntoSettings("goal", "string", goal); sendMess(channel, "Goal set: \""+goal+"\"."); break;
                        #endregion
                        #endregion
                        #endregion
                        #region bias and economy
                        #region bias
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
                                        log(1, "Parsing error in bias vote, send more robots!");//has never happened, ever. Will be removed soon to improve code quality. --or not, I like this message.
                                    }
                                }
                                if (tempVar3.Length > 2)//if the bias votes contrains enough words for a biasnumbers vote
                                {
                                    fail = false;
                                    double[] dbl = new double[7];
                                    int count = 0;
                                    try
                                    {
                                        for (int a = 0; a < 7; a++)
                                        {
                                            if (Regex.Match(tempVar3[a], @"^(10|[0-9](\.[0-9]){0,1})$").Success)
                                            {
                                                dbl[a] = double.Parse(tempVar3[a]);//try to parse these numbers...
                                                count++;
                                            }
                                            else
                                            {
                                                fail = true;
                                                break;
                                            }
                                        }
                                    }
                                    catch { fail = true; }
                                    if (!fail)
                                    {
                                        q = new Bias("custom", dbl);
                                        tempVar1 = 1;
                                        try
                                        {
                                            tempVar1 = int.Parse(tempVar3[7]);
                                            if (tempVar1 < 1)
                                            {
                                                tempVar1 = 1;
                                            }
                                        }
                                        catch { };
                                    }
                                    else
                                    {
                                        if(count == 4)
                                        {
                                            for(int i =4; i<7; i++)
                                            {
                                                dbl[i] = 0;
                                            }
                                            q = new Bias("custom", dbl);
                                            tempVar1 = 1;
                                        }
                                        else if( count>2)
                                        {
                                            q = null;
                                            if(count != 3)
                                            sendMess(channel, User + ", it seems you tried a custom bias, but failed.");
                                        }
                                    }

                                }
                                if (q != null && (tempVar1 - 2) * moneyPerVote <= getPoints(user))//(7-2) * 50 <= 50... What the fuck.
                                {
                                    addVote(user, q, tempVar1);
                                }
                            }
                            if(voteStatus==0)
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
                            if(voteStatus==-1)
                            {
                                sendMess(channel, "Voting is disabled.");
                            }
                            break;
                        #endregion
                        #region setbias
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
                        #endregion
                        #region resetbias
                        case "!resetbias":
                            biasControl.setBias(biasControl.getDefaultBias());
                            sendMess(channel, User + "-> Bias reset.");
                            break;
                        #endregion
                        #region balance
                        case "!balance":
                            tempVar1 = getPoints(user); tempVar2 = "";
                            if (tempVar1 == 0)
                            {
                                tempVar2 = "zero";
                            }
                            else if (tempVar1 == 1 || tempVar1 == -1)
                            {
                                tempVar2 = tempVar1 + " PokeDollar";
                            }
                            else
                            {
                                tempVar2 = tempVar1 + " PokeDollars";
                            }
                            tempVar1 = getAllTime(user);
                            sendMess(channel, User + ", your balance is " + tempVar2 + ". (" + tempVar1 + ")");
                            break;
                        #endregion
                        #region setpoints
                        case "!setpoints":
                            if (Regex.Match(str[2], "^([1-9][0-9]{1,8}|[0-9])$").Success)
                            {
                                setPoints(str[1], int.Parse(str[2]));
                                sendMess(channel, "Points have been changed.");
                            }
                            break;
                        #endregion
                        #region check
                        case "!check":
                            sendMess(channel, str[1].Substring(0, 1).ToUpper() + str[1].Substring(1).ToLower() + " has " + getPoints(str[1].ToLower()) + " PokeDollars. (" + getAllTime(str[1]) + ")");
                            break;
                        #endregion
                        #region bias adding and removing
                        #region delbias
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
                        #endregion
                        #region addbias
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
                        #endregion
                        #endregion
                        #region purchasable stuff
                        #region givemoney
                        case "!givemoney":

                            bool givemoneysucces = false;
                            if (Regex.Match(str[1], @"^[1-9]([0-9]{1,8})?$").Success)
                            {
                                if (int.Parse(str[1]) * 2 <= getPoints(user))
                                {
                                    addPoints(user, int.Parse(str[1]) * -2, "Money to game");
                                    luaServer.send_to_all("ADDMONEY", str[1]);
                                    sendMess(channel, User + " converted " + int.Parse(str[1]) * 2 + " of their funds into " + str[1] + " PokeDollar for IA.");
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
                        #endregion
                        #region giveball
                        case "!giveball":
                            bool giveballsucces = false;
                            if (1500 <= getPoints(user))
                            {
                                addPoints(user, -1500, "ball to game");
                                luaServer.send_to_all("ADDBALLS", "1");
                                sendMess(channel, User + " gave IA a pokéball.");//HARB $playername
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
                        #endregion
                        #region background
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

                                                    sendMess(channel, "Something went wrong, no PokeDollars deducted.");
                                                }
                                                else
                                                {
                                                    addPoints(user, -500, "background");
                                                    sendMess(channel, User + " changed the background of the stream for 500 PokeDollars!");
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
                        #endregion
                        #region expall
                        case "!expall":
                            int.TryParse(str[1], out tempVar1);
                            if (tempVar1 > 0)
                            {
                                tempVar2 = expAllFunc.Replace("X", tempVar1 + "");
                                int expAllMoney = (int)calculator.Parse(tempVar2).Answer;
                                if (expAllMoney <= getPoints(user))
                                {
                                    addPoints(user, -1 * expAllMoney, "EXP ALL (" + str[1] + ")");
                                    if (exp_allTimer.Enabled)
                                    {
                                        expTime += tempVar1;
                                        expTimeEnd += tempVar1;
                                        luaServer.send_to_all("EXPON", "" + (expTimeEnd - getNow()));
                                    }
                                    else
                                    {
                                        luaServer.send_to_all("EXPON", "" + tempVar1);
                                        expTimeEnd = getNow() + tempVar1;
                                        exp_allTimer.Dispose();
                                        exp_allTimer = new System.Timers.Timer(tempVar1 * 1000);
                                        exp_allTimer.AutoReset = false;
                                        exp_allTimer.Elapsed += exp_allTimer_Elapsed;
                                        exp_allTimer.Start();
                                    }
                                    sendMess(channel, "EXPALL turned on for :" + tempVar1 + " seconds. " + expAllMoney + " has been deducted from your account, " + User);
                                }
                                else
                                {
                                    sendMess(channel, "You don't have enough money, you'd need; " + expAllMoney);
                                }
                            };
                            break;
                        #endregion
                        #endregion
                        #endregion

                        #region calculate
                        case "!calculate":
                            tempVar2 = str[1] + str[2];
                            Calculation calc = calculator.Parse(tempVar2);
                            if (calc.Valid)
                            {
                                sendMess(channel, "Answer: " + calc.Answer + ".");
                            }
                            else
                            {
                                sendMess(channel, "I'm sorry, either your calculation is wrong, or I am not programmed yet to be able to read it.");
                            }
                            break;
                        #endregion
                        #region addlog
                        case "!addlog":
                            appendFile(progressLogPATH, "\n" + getNowExtended() + " " + User + " " + str[1] + " " + str[2]);
                            sendMess(channel, "Affirmative, " + User + "!");
                            break;
                        #endregion
                        #region save
                        case "!save":
                            tempVar2 = str[1];
                            luaServer.send_to_all("SAVE", tempVar2);
                            sendMess(channel, User + "-> Saved game with parameter '" + tempVar2 + "'.");
                            break;
                        #endregion
                        #region commands (rngppcommands)
                        case "!rngppcommands":
                            sendMess(channel, User + "-> commands are located at " + commandsURL + " .");
                            break;
                        #endregion
                        #region repel
                        case "!repel":
                            if (Regex.Match(str[1], "^((on)|1|(true)|(yes))$", RegexOptions.IgnoreCase).Success) { luaServer.send_to_all("REPELON", ""); sendMess(channel, "Repel ON"); }
                            if (Regex.Match(str[1], "^((off)|0|(false)|(no))$", RegexOptions.IgnoreCase).Success) { luaServer.send_to_all("REPELOFF", ""); sendMess(channel, "Repel OFF"); }
                                break;
                        #endregion
                    }
                    break;
                }
            }
            if (!done)
            {
                foreach (command c in comlist)//check for softcoms
                {
                    if (c.doesMatch(message) && c.canTrigger() && c.getAuth() <= auth)
                    {
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


        void newMessage(string user)
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
                sendMess(channel, output,2);
            }

        }

        public string pollResults()
        {
            int[] tally = new int[poll.Length];
            foreach(intStr s in poll_votes)
            {
                tally[s.i()-1]++;
            }
            string str = "";
            for(int i=0; i<poll.Length;i++)
            {
                str += " '" + poll[i] + "' received " + tally[i];
                if(tally[i]!=1)
                {
                    str += " votes.";
                }
                else
                {
                    str += " vote.";
                }
            }
            return str;
        }

        void addVote(string user, Bias b, int amount)
        {
            var money = 0;//amount to be deducted/added
            var x = -1;//index of person who voted
            for(int a = 0; a<votingList.Count; a++)
            {
                if (votingList[a].Str == user.ToLower())
                {
                    x = a;
                    break;
                }
            }
            if(x>-1)//this user already voted
            {
                money += (votingList[x].Int - 1) * moneyPerVote;//add (their amount of votes minus one (to compensate for the free vote)) * the money_per_vote
                votingList.RemoveAt(x);//remove the vote
                votinglist.RemoveAt(x);//remove it from the otherlist as well
            }
            if(amount!=0){
                money += ((1 - amount) * moneyPerVote);//money is (1 vote - amount of votes) * moneyPerVote; so 1 vote =0, 2 votes -50, etc.
                votingList.Add(new intStr(user, amount));//add these
                votinglist.Add(b);//do it
                if(x == -1)//if it was a new vote
                {
                    money += moneyPerVote;//give the user his money
                    addAllTime(user, moneyPerVote);//add some money
                }
            }
            addPoints(user, money, "vote");

        }
    }
}