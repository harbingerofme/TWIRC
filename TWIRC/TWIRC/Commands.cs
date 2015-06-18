using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SayingsBot
{
    public class Commands
    {
        HarbBot hb = Program.HarbBot;
        SQLiteConnection dbConn;

        public Commands(SQLiteConnection dbConnn)
        {
            this.dbConn = dbConnn;
        }

        public void checkCommand(string channel, string user, string message, List<hardCom> hardList, List<command> comlist, List<ali> aliList, int logLevel, Logger logger)
        {
            string[] str, tempVar3;
            bool done = false; int auth = hb.pullAuth(user);
            bool fail; int tempVar1 = 0; string tempVar2 = "";
            string User = user.Substring(0, 1).ToUpper() + user.Substring(1);
            user = user.ToLower();
            message = message.ToLower();
                foreach (hardCom h in hardList)//hardcoded command
                {
                    if (h.hardMatch(user, message, auth))
                    {
                        done = true;
                        str = h.returnPars(message);
                        if (logLevel == 1) { logger.WriteLine("IRC:<- <" + user + "> " + message); }

                        if (h.returnKeyword() != "!sbgetuseraliases")
                        {
                            for (int it = 0; it < str.Length; it++)
                            {
                                if (getUserFromAlias(str[it]) != null)
                                {
                                    string tmp = getUserFromAlias(str[it]);
                                    str[it] = tmp;
                                }
                            }
                        }
                        switch (h.returnKeyword())
                        {
                            case "!sbaddcom":
                                fail = false;

                                foreach (command c in comlist) { if (c.doesMatch(str[1])) { fail = true; break; } }
                                foreach (hardCom c in hardList) { if (c.doesMatch(str[1]) || fail) { fail = true; break; } }
                                foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; } }
                                if (fail) { sendMess("I'm sorry, " + User + ". A command or alias with the same name exists already."); }
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
                                    sendMess(User + " -> command \"" + str[1] + "\" added. Please try it out to make sure it's correct.");
                                    hb.appendFile(hb.progressLogPATH, "Command \"" + str[1] + "\" added.");
                                }
                                break;
                            case "!sbeditcom":
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
                                        SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET response = @par1 authlevel=@par3 WHERE keyword=@par2;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", tempVar2); cmd.Parameters.AddWithValue("@par2", str[1]); cmd.Parameters.AddWithValue("@par3", tempVar1);
                                        cmd.ExecuteNonQuery();
                                        sendMess(User + "-> command \"" + str[1] + "\" has been edited.");
                                        hb.appendFile(hb.progressLogPATH, "Command \"" + str[1] + "\" has been edited.");
                                        fail = false;
                                    }
                                }
                                if (fail)
                                {
                                    sendMess("I'm sorry, " + User + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                                break;
                            case "!sbdelcom"://delete command
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
                                        sendMess(User + "-> command \"" + str[1] + "\" has been deleted.");
                                        hb.appendFile(hb.progressLogPATH, "Command \"" + str[1] + "\" has been deleted.");
                                        break;
                                    }

                                }
                                if (fail)
                                {
                                    sendMess("I'm sorry, " + User + ". I can't find a command named that way. (maybe it's an alias?)");
                                }
                                break;
                            case "!sbaddalias": //add alias
                                fail = false;
                                foreach (command c in comlist) { if (c.doesMatch(str[1])) { fail = true; break; } }
                                foreach (hardCom c in hardList) { if (c.doesMatch(str[1]) || fail) { fail = true; break; } }
                                foreach (ali c in aliList) { if (c.filter(str[1]) != str[1] || fail) { fail = true; break; } }
                                if (fail) { sendMess("I'm sorry, " + user + ". A command or alias with the same name exists already."); }
                                else
                                {
                                    fail = true;
                                    foreach (ali c in aliList)
                                    {
                                        if (c.getTo() == str[2]) { c.addFrom(str[1]); fail = false; break; }
                                    }
                                    if (fail) { aliList.Add(new ali(str[1], str[2])); }
                                    SQLiteCommand cmd = new SQLiteCommand("INSERT INTO aliases (keyword,toword) VALUES (@par1,@par2);", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", str[1]); cmd.Parameters.AddWithValue("@par2", str[2]);
                                    cmd.ExecuteNonQuery();
                                    sendMess(User + " -> alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                                    hb.appendFile(hb.progressLogPATH, "Alias \"" + str[1] + "\" pointing to \"" + str[2] + "\" has been added.");
                                }
                                break;
                            case "!sbdelalias":
                                fail = true;
                                foreach (ali c in aliList)
                                {
                                    if (c.delFrom(str[1]))
                                    {
                                        SQLiteCommand cmd = new SQLiteCommand("DELETE FROM aliases WHERE keyword=@par1;", dbConn);
                                        cmd.Parameters.AddWithValue("@par1", str[1]); cmd.ExecuteNonQuery();
                                        sendMess( user + " -> Alias \"" + str[1] + "\" removed.");
                                        hb.appendFile(hb.progressLogPATH, "Alias \"" + str[1] + "\" removed.");
                                        if (c.getFroms().Count() == 0) { aliList.Remove(c); }
                                        fail = false;
                                        break;
                                    }
                                }
                                if (fail) { sendMess("I'm sorry, " + User + ". I couldn't find any aliases that match it. (maybe it's a command?)"); }
                                break;
                            case "!sbset"://!set <name> <level>
                                if (!Regex.Match(str[1].ToLower(), @"^[a-z0-9_]+$").Success) { sendMess("I'm sorry, " + User + ". That's not a valid name."); }
                                else
                                {
                                    if (Regex.Match(str[2], "^([0-" + auth + "])|(-1)$").Success)//look at that, checking if it's a number, and checking if the user is allowed to do so in one moment.
                                    {
                                        hb.setAuth(str[1].ToLower(), int.Parse(str[2]));
                                        sendMess(user + " -> \"" + str[1] + "\" was given auth level " + str[2] + ".");
                                        hb.appendFile(hb.progressLogPATH, str[1] + " was given auth level " + str[2] + ".");
                                    }
                                    else
                                    {
                                        sendMess("I'm sorry, " + User + ". You either lack the authorisation to give such levels to that person, or that level is not a valid number.");
                                    }
                                }
                                break;
                            case "!sbeditcount":
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
                                        sendMess(user + "-> the count of \"" + str[1] + "\" has been updated to " + str[2] + ".");
                                    }
                                }
                                break;
                            case "!banuser":
                                if (auth > hb.pullAuth(str[1]))//should prevent mods from banning other mods, etc.
                                {
                                    hb.setAuth(str[1], -1);
                                    sendMess(User + "-> \"" + str[1] + "\" has been banned from using bot commands.");
                                    hb.appendFile(hb.progressLogPATH, str[1] + " has been banned from using bot commands.");
                                }

                                break;
                            case "!unbanuser":
                                if (hb.pullAuth(str[1]) == -1)
                                {
                                    hb.setAuth(str[1], 0);
                                    sendMess(User + "-> \"" + str[1] + "\" has been unbanned.");
                                    hb.appendFile(hb.progressLogPATH, str[1] + "has been unbanned.");
                                }

                                break;
                            case "!sbsilence":
                                if (Regex.Match(str[1], "(on)|(off)|1|0|(true)|(false)|(yes)|(no)", RegexOptions.IgnoreCase).Success) { sendMess("Silence has been set to: " + str[1]); }
                                if (Regex.Match(str[1], "(on)|1|(true)|(yes)", RegexOptions.IgnoreCase).Success) { hb.silence = true; new SQLiteCommand("UPDATE settings SET silence=1;", dbConn).ExecuteNonQuery(); }
                                if (Regex.Match(str[1], "(off)|0|(false)|(no)", RegexOptions.IgnoreCase).Success) { hb.silence = false; new SQLiteCommand("UPDATE settings SET silence=0;", dbConn).ExecuteNonQuery(); }

                                break;

                            case "!sbrank":
                                string text = "";
                                switch (auth)
                                {
                                    case 0: text = "an user"; break;
                                    case 1: text = "a regular"; break;
                                    case 2: text = "trusted"; break;
                                    case 3: text = "a moderator"; break;
                                    case 4: text = "the broadcaster"; break;
                                    case 5: text = "an administrator of " + hb.bot_name; break;
                                    default: text = "special"; break;
                                }
                                sendMess(User + ", you are " + text + ".");
                                break;
                            case "!commands":
                                sendMess(User + " --> SayingsBot Commands at http://moddedbydude.net76.net/wiki/index.php/SayingsBot#Commands");
                                break;
                            case "!givecake":
                                if (str[1].Contains("poolala"))
                                {
                                    sendMess("No " + User + "! Poolala123 is alergic to cake!");
                                }
                                else
                                {
                                    sendMess(User + " gives " + str[1] + " some cake!");
                                }
                                
                                break;
                            case "!givepie":
                                sendMess(User + " gives " + str[1] + " some pie!");
                                break;
                            case "!givea":
                                sendMess(User + " gives " + str[1] + " a " + str[2] + "!");
                                break;
                            case "!givesome":
                                sendMess(User + " gives " + str[1] + " some " + str[2] + "!");
                                break;
                            case "!classicwhoisuser":
                                sendMess(getClassicWhoIs(str[1]));
                                break;
                            case "!whoisuser":
                                SQLiteCommand userCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '0';", dbConn);
                                userCommand.Parameters.AddWithValue("@par1", str[1].ToLower());
                                SQLiteDataReader userReader = userCommand.ExecuteReader();
                                if (userReader.Read()) { sendMess(userReader.GetString(0)); } else { sendMess(str[1] + " does not have a !whoisuser."); }
                                break;
                            case "!editme":
                                string newText = str[1];
                                setWhoIsUser(user, newText);
                                SQLiteCommand usersCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '0';", dbConn);
                                usersCommand.Parameters.AddWithValue("@par1", str[1].ToLower());
                                SQLiteDataReader usersReader = usersCommand.ExecuteReader();
                                if (usersReader.Read())
                                {
                                    sendMess(User + " your !whoisuser now reads as: " + usersReader.GetString(0));
                                    hb.appendFile(hb.progressLogPATH, User + " your !whoisuser now reads as: " + usersReader.GetString(0));
                                }
                                break;
                            case "!edituser":
                                string newUser = str[1];
                                string newTextEU = str[2];
                                setWhoIsUser(newUser, newTextEU);
                                SQLiteCommand userssCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '0';", dbConn);
                                userssCommand.Parameters.AddWithValue("@par1", newUser.ToLower());
                                SQLiteDataReader userssReader = userssCommand.ExecuteReader();
                                if (userssReader.Read())
                                {
                                    sendMess(newUser + "'s !whoisuser now reads as: " + userssReader.GetString(0));
                                    hb.appendFile(hb.progressLogPATH, User + " your !whoisuser now reads as: " + userssReader.GetString(0));
                                }
                                break;
                            case "!classic":
                                //TODO: Need to SQL this
                                SQLiteCommand classicCmd = new SQLiteCommand("SELECT data FROM userdata WHERE user = @par1 AND dataType = '4';", dbConn);
                                classicCmd.Parameters.AddWithValue("@par1", str[1]);
                                SQLiteDataReader classicReader = classicCmd.ExecuteReader();
                                if (classicReader.Read())
                                {
                                    sendMess(classicReader.GetString(0));
                                }
                                else
                                {
                                    sendMess("Classic " + str[1] + " does not exist!");
                                }
                                break;
                            case "!addclassic":
                                string classicAdd = str[1];
                                string classicMessage = str[2];
                                SQLiteCommand classicAddCommand = new SQLiteCommand("INSERT INTO userdata (user, datatype, data) VALUES (@par1, '4', @par2);", dbConn);
                                classicAddCommand.Parameters.AddWithValue("@par1", classicAdd);
                                classicAddCommand.Parameters.AddWithValue("@par2", classicMessage);
                                classicAddCommand.ExecuteNonQuery();
                                sendMess("Classic command " + classicAdd + " appears as " + classicMessage);
                                hb.appendFile(hb.progressLogPATH, "Classic command " + classicAdd + " added.");
                                break;
                            case "!kill":
                                sendMess(User + " politley murders " + str[1] + ".");
                                break;
                            case "!calluser":
                                sendMess("CALLING " + str[1].ToUpper() + "! WOULD " + str[1].ToUpper() + " PLEASE REPORT TO THE CHAT!");
                                break;
                            case "!count":
                                SQLiteDataReader getCountCmd = new SQLiteCommand("SELECT DATA FROM misc WHERE ID = 'CountGame';", dbConn).ExecuteReader(); //needs to be put into int count
                                int count;
                                if (getCountCmd.Read())
                                {
                                    count = int.Parse(getCountCmd.GetString(0));
                                    count++;
                                    new SQLiteCommand("UPDATE misc SET DATA = '" + count + "' WHERE ID='CountGame';", dbConn).ExecuteNonQuery();
                                }
                                else//countgame doesn't exist?
                                {
                                    count = 1;
                                    new SQLiteCommand("INSERT INTO misc (ID,DATA) VALUES ('CountGame','1');", dbConn).ExecuteNonQuery();
                                }
                                sendMess(hb.cstr(count));
                                break;
                            case "!newcount":
                                new SQLiteCommand("UPDATE misc SET DATA = '0' WHERE ID='CountGame';", dbConn).ExecuteNonQuery();
                                sendMess("Counting Game Reset");
                                break;
                            case "!points":
                                sendMess(user + " you have " + getPoints(user) + " points. (" + getAllTime(user) + ")");
                                break;
                            case "!seepoints":
                                sendMess(str[1] + " has " + getPoints(str[1]) + " points. (" + getAllTime(str[1]) + ")");
                                break;
                            case "!setpoints":
                                setPoints(str[1], hb.cint(str[2]));
                                sendMess(str[1] + "'s points set to " + str[2] + " points.");
                                break;
                            case "!addpoints":
                                addPoints(str[1], hb.cint(str[2]), "Manual Add");
                                addAllTime(str[1], hb.cint(str[2]));
                                sendMess(str[1] + " gained " + str[2] + " points.");
                                break;
                            case "!nc":
                                sendMess(str[1] + "! Please change the color of your name, neon colors hurt some peoples eyes! (If you don't know how type \".color\")");
                                break;
                            case "!sbversion":
                                sendMess("/me is currently HB" + Application.ProductVersion + " bassed off of SB2.8.2. Changelog at http://moddedbydude.net76.net/wiki/index.php/SayingsBot#ChangeLog");
                                break;
                            case "!sbleaderboard":
                                SQLiteDataReader sqldr;
                                string messString = "Leaderboard: ";
                                sqldr = new SQLiteCommand("SELECT name,alltime FROM users ORDER BY alltime DESC,name LIMIT 5;", dbConn).ExecuteReader();
                                while (sqldr.Read())
                                {
                                    messString += sqldr.GetString(0) + " with " + sqldr.GetInt32(1) + " points. ";
                                }
                                sendMess(messString);
                                break;
                            case "!sqlquery":
                                break;
                            case "sayingsbot":
                                sendMess("/me reporting, " + user + "!");
                                break;
                            case "!sbadduseralias":
                                setUserAlias(str[1], str[2]);
                                sendMess("Gave user " + str[1] + " the alias " + str[2] + ".");
                                break;
                            case "!sbgetuseralias":
                                if (getUserAlias(str[1]) != null)
                                {
                                    sendMess("User " + str[1] + " has the aliases " + getUserAlias(str[1]) + ".");
                                }
                                else
                                {
                                    sendMess("User " + str[1] + " has no aliases.");
                                }
                                break;
                            case "!swearjar":
                                SQLiteDataReader tmp;
                                tmp = new SQLiteCommand("SELECT data FROM userdata WHERE user='swearJar' AND datatype='6';", dbConn).ExecuteReader();
                                if (tmp.Read())
                                {
                                    int tmpI = hb.cint(tmp.GetString(0));
                                    sendMess("There is currently " +tmpI + " points in the swear jar.");
                                }
                                break;
                            case "!quotes":
                                Random rnd = new Random();
                                string function = str[1];
                                string quser = str[2];
                                string fParam = str[3];
                                Boolean success = false;
                                SQLiteDataReader quotesReader;
                                SQLiteCommand quotesCommand;
                                quser = quser.ToLower();

                                if (function == "info")
                                {
                                    sendMess("Infomation about quotes avalible on the !sbcommands page.");
                                    break;
                                }
                                else if (function == "say")
                                {
                                    if (quser == "random")
                                    {
                                        //randomquote
                                        quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = 'overallRandom' AND dataType = '5' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                        if (quotesReader.Read())
                                        {
                                            int ubound = quotesReader.GetInt32(0);
                                            int rand = rnd.Next(1, ubound + 1);
                                            quotesReader = new SQLiteCommand("SELECT data FROM userdata WHERE user = 'overallRandom' AND dataType = '5' AND dataID ='" + rand + "';", dbConn).ExecuteReader();
                                            if (quotesReader.Read())
                                            {
                                                string theuser = quotesReader.GetString(0);
                                                quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = '" + theuser + "' AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                                if (quotesReader.Read())
                                                {
                                                    rand = rnd.Next(1, quotesReader.GetInt32(0) + 1);
                                                    quotesReader = new SQLiteCommand("SELECT data FROM userdata WHERE user='" + theuser + "' AND datatype = '1' AND dataID = '" + rand + "';", dbConn).ExecuteReader();
                                                    if (quotesReader.Read()) { sendMess(theuser + "(" + rand + "): " + quotesReader.GetString(0)); } else { sendMess("Quote " + rand + " does not exist for " + quser); }
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        if (fParam == "r")
                                        {
                                            //random user quote
                                            quotesCommand = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = @par1 AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn);
                                            quotesCommand.Parameters.AddWithValue("@par1", quser);
                                            quotesReader = quotesCommand.ExecuteReader();

                                            if (quotesReader.Read())
                                            {
                                                int rand = rnd.Next(1, quotesReader.GetInt32(0) + 1);
                                                quotesCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '1' AND dataID = '" + rand + "';", dbConn);
                                                quotesCommand.Parameters.AddWithValue("@par1", quser);
                                                quotesReader = quotesCommand.ExecuteReader();
                                                if (quotesReader.Read()) { sendMess("(" + rand + "): " + quotesReader.GetString(0)); } else { sendMess("Quote " + rand + " does not exist for " + quser); }
                                                break;
                                            }
                                            break;
                                        }
                                        else
                                        {
                                            //specific user quote
                                            try { int.Parse(fParam); success = true; }
                                            catch { }
                                            if (success)
                                            {
                                                quotesCommand = new SQLiteCommand("SELECT data FROM userdata WHERE user=@par1 AND datatype = '1' AND dataID = @par2;", dbConn);
                                                quotesCommand.Parameters.AddWithValue("@par1", quser);
                                                quotesCommand.Parameters.AddWithValue("@par2", fParam);
                                                quotesReader = quotesCommand.ExecuteReader();
                                                if (quotesReader.Read()) { sendMess(quotesReader.GetString(0)); } else { sendMess("Quote " + fParam + " does not exist for " + quser); }
                                                break;
                                            }
                                            else
                                            {
                                                sendMess("Quote " + fParam + " does not exist for " + quser + ".");
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (function == "add")
                                {
                                    int length;
                                    int newLength;
                                    quotesCommand = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = @par1 AND dataType = '1' ORDER BY dataID DESC LIMIT 1;", dbConn);
                                    quotesCommand.Parameters.AddWithValue("@par1", quser);
                                    quotesReader = quotesCommand.ExecuteReader();
                                    if (quotesReader.Read())
                                    {
                                        length = quotesReader.GetInt32(0);
                                        newLength = length + 1;
                                    }
                                    else
                                    {
                                        //No quotes yet exsist
                                        newLength = 1;
                                    }
                                    quotesCommand = new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES (@par1, '1', '" + newLength + "',  @par2);", dbConn);
                                    quotesCommand.Parameters.AddWithValue("@par1", quser);
                                    quotesCommand.Parameters.AddWithValue("@par2", fParam);
                                    quotesCommand.ExecuteNonQuery();
                                    sendMess("Quote " +hb.cstr(newLength) + " for " + quser + " has been added as: " + fParam);
                                    hb.appendFile(hb.progressLogPATH, "Quote " + hb.cstr(newLength) + " for " + quser + " has been added as: " + fParam);

                                    //If it's the user's first quote
                                    //Originally addusertorandom
                                    if (newLength == 1)
                                    {
                                        quotesReader = new SQLiteCommand("SELECT dataID FROM userdata WHERE user = 'overallRandom' AND datatype = '5' ORDER BY dataID DESC LIMIT 1;", dbConn).ExecuteReader();
                                        if (quotesReader.Read())
                                        {
                                            length = quotesReader.GetInt32(0);
                                            newLength = length + 1;
                                            quotesCommand = new SQLiteCommand("INSERT INTO userdata (user, datatype, dataID, data) VALUES ('overallRandom', '5', '" + newLength + "', @par1);", dbConn);
                                            quotesCommand.Parameters.AddWithValue("@par1", quser);
                                            quotesCommand.ExecuteNonQuery();
                                            sendMess("Added " + quser + " to overall random list. They are user " + hb.cstr(newLength) + ".");
                                            hb.appendFile(hb.progressLogPATH, "Added " + quser + " to overall random list. They are user " + hb.cstr(newLength) + ".");
                                        }
                                        break;
                                    }
                                    else { break; }
                                }
                                else if (function == "edit")
                                {
                                    sendMess("Editing quotes unimplimented. Bug dude to change it.");
                                    break;
                                }
                                else
                                {
                                    sendMess("Incorrect use of !quotes.");
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
                                    str = c.getResponse(message, user);
                                    c.addCount(1);
                                    SQLiteCommand cmd = new SQLiteCommand("UPDATE commands SET count = '" + c.getCount() + "' WHERE keyword = @par1;", dbConn);
                                    cmd.Parameters.AddWithValue("@par1", c.getKey());
                                    cmd.ExecuteNonQuery();
                                    if (str.Count() != 0) { if (str[0] != "") { c.updateTime(); } }
                                    foreach (string b in str)
                                    {
                                        sendMess(b);
                                    }
                                }
                            }
                        }
                    }
            }
        }

        void sendMess(string message)
        {
            hb.sendMess(hb.channels, message);
        }

        #region Classic Whois
        /// <summary>
        /// Checks through the classic !whoisuser's. (April/May 2014)
        /// </summary>
        /// <param name="user">The user to check for.</param>
        /// <returns>Classic !whoisuser message.</returns>
        string getClassicWhoIs(string user)
        {
            string[] lines = hb.FileLines(hb.sysPath() + "\\people.txt");
            int line = getClassicWhoIsLine(user, lines);
            if (line == -1)
            {
                return "That user did not classically have a !whoisuser";
            }
            else
            {
                return lines[line].Substring(user.Length + 1, lines[line].Length - (user.Length + 1));
            }
        }

        /// <summary>
        /// The line in the txt file that contains the user
        /// </summary>
        /// <param name="user">The user to check for.</param>
        /// <param name="lines">String[] of the text document.</param>
        /// <returns>The line, or -1 is there is none for the user.</returns>
        int getClassicWhoIsLine(string user, string[] lines)
        {
            for (int I = 0; I < lines.Length; I++)
            {
                if (lines[I].StartsWith(user))
                {
                    return I;
                }
            }
            return -1;
        }
        #endregion
        #region User Aliases
        /// <summary>
        /// Sets a user's alias.
        /// </summary>
        /// <param name="user">The user to set the alias for.</param>
        /// <param name="alias">The alias to set.</param>
        void setUserAlias(string user, string alias)
        {
            string tmp;
            SQLiteCommand getUserAliasesCommand = new SQLiteCommand("SELECT alias FROM userAliases WHERE user=@par1;", dbConn);
            getUserAliasesCommand.Parameters.AddWithValue("@par1", user);
            SQLiteDataReader readAliases = getUserAliasesCommand.ExecuteReader();
            if (readAliases.Read())
            {
                tmp = readAliases.GetString(0);
            }
            else
            {
                tmp = null;
            }

            SQLiteCommand setAliasCommand;
            if (tmp != null)
            {
                setAliasCommand = new SQLiteCommand("UPDATE userAliases SET alias=@par2 WHERE user=@par1; ", dbConn);
            }
            else
            {
                setAliasCommand = new SQLiteCommand("INSERT INTO userAliases (user, alias) VALUES (@par1, @par2); ", dbConn);
            }
            setAliasCommand.Parameters.AddWithValue("@par1", user);
            setAliasCommand.Parameters.AddWithValue("@par2", alias);
            setAliasCommand.ExecuteNonQuery();
        }
        /// <summary>
        /// Get a user from an alias
        /// </summary>
        /// <param name="alias">The alias to check for.</param>
        /// <returns>The user, or null if it's not an alias.</returns>
        string getUserFromAlias(string alias)
        {
            SQLiteCommand getUserFromAliasCommand = new SQLiteCommand("SELECT user FROM userAliases WHERE alias=@par1;", dbConn);
            getUserFromAliasCommand.Parameters.AddWithValue("@par1", alias);
            SQLiteDataReader readAliases = getUserFromAliasCommand.ExecuteReader();
            if (readAliases.Read())
            {
                return readAliases.GetString(0);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Get's the user's alias.
        /// </summary>
        /// <param name="user">User to check for.</param>
        /// <returns>The alias, or null if there is none.</returns>
        string getUserAlias(string user)
        {
            SQLiteCommand getUserAliasesCommand = new SQLiteCommand("SELECT alias FROM userAliases WHERE user=@par1;", dbConn);
            getUserAliasesCommand.Parameters.AddWithValue("@par1", user);
            SQLiteDataReader readAliases = getUserAliasesCommand.ExecuteReader();
            if (readAliases.Read())
            {
                return readAliases.GetString(0);
            }
            else
            {
                return null;
            }

        }
        #endregion
        #region User Points
        /// <summary>
        /// Gets the user's current points.
        /// </summary>
        /// <param name="name">The user to get the points for.</param>
        /// <returns>The user's current points.</returns>
        public int getPoints(string name)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + hb.getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + hb.getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }

        /// <summary>
        /// Manually sets the user's points to a specific amount.
        /// </summary>
        /// <param name="user">The user to set points for.</param>
        /// <param name="amount">The amount to set.</param>
        public void setPoints(string user, int amount)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET points='" + amount + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + user + "','" + amount + "','FORCED CHANGE TO AMOUNT','" + sqldr.GetInt32(0) + "','" + hb.getNowSQL() + "');", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + user + "','" + hb.getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Adds to the user's current points.
        /// </summary>
        /// <param name="name">The user to add points to.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Amount added.</returns>
        public int addPoints(string name, int amount)
        {
            return addPoints(name, amount, null);
        }

        /// <summary>
        /// Adds to the user's current points.
        /// </summary>
        /// <param name="name">The user to add points to.</param>
        /// <param name="amount">The amount to add.</param>
        /// <param name="why">The reason for adding.</param>
        /// <returns>Amount added.</returns>
        public int addPoints(string name, int amount, string why)
        {
            int things;
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                new SQLiteCommand("UPDATE users SET points='" + (things + amount) + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                if (why != null)
                {
                    new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + name + "','" + amount + "','" + why + "','" + things + "','" + hb.getNowSQL() + "');", dbConn).ExecuteNonQuery();
                }
                return things;
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + hb.getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        /// <summary>
        /// Adds points to the user's all time amount.
        /// </summary>
        /// <param name="name">The user to preform the addition on.</param>
        /// <param name="amount">The ammount to add.</param>
        /// <returns>The amount added.</returns>
        public int addAllTime(string name, int amount)
        {
            int things;
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                new SQLiteCommand("UPDATE users SET alltime=alltime+" + amount + " WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return things;
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + hb.getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        /// <summary>
        /// Gets the user's all-time points.
        /// </summary>
        /// <param name="name">The user to check for.</param>
        /// <returns>The user's all-time points in an int.</returns>
        public int getAllTime(string name)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                return sqldr.GetInt32(0);
            }
            else return 0;
        }
        #endregion
        #region !whoisuser
        /// <summary>
        /// Set the !whoisuser message of a user.
        /// </summary>
        /// <param name="user">The user to set the message for.</param>
        /// <param name="message">The message to set to the user.</param>
        public void setWhoIsUser(string user, string message)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM userdata WHERE user='" + user + "' AND datatype='0';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE userdata SET data='" + message + "' WHERE user='" + user + "' AND datatype='0';", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO userdata (user,dataType,data) VALUES ('" + user + "','0','" + message + "');", dbConn).ExecuteNonQuery();
            }
        }
        #endregion
    }
}
