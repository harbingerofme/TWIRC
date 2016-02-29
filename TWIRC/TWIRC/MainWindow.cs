using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Timers;
using System.Linq;

namespace TWIRC
{
    class MainWindow : Form
    {
        int selectedMenu = 0; int oldMenu = 1;
        int height = 400;
        int width = 660;
        Bitmap but_ac, but_inac;
        Label[] tabs_label; String[] tabs_text;
        Panel[] tabs;

        HarbBot irc;
        DatabaseConnector dbConn;
        LuaServer luaServer;
        DatabaseScheduler dbSched;
        ButtonMasher biasControl;
        Dictionary<string, LuaServer.EmuClientHandler> RNGEmulators;

        //tab0
        bool running;
        CheckBox sendOutMainMessage;
        //tab1
        bool multipleWindows;
        public CHILDFORM[] childWindows;
        System.Timers.Timer timeSaver;
        NumericUpDown windowTimerSaveInterval;
        //tab2
        //tab3

        //tab4
        Chat Chatter;
        TextBox chatBox;
        List<string> chatMessages;
        int chatLine;
        //tab5
        formLogger logger;//type,severity,time,message
        int logLevelLastValue;//Value is used, ignore warning!

        #region controls


        #endregion

        public MainWindow(HarbBot _irc, Logger _log, DatabaseConnector _dbconn, LuaServer _luaServer, DatabaseScheduler _dbsched, ButtonMasher _biasControl, Dictionary<string, LuaServer.EmuClientHandler> _RNGEmulators)
        {
            #region parameters
            irc = _irc;
            dbConn = _dbconn;
            luaServer = _luaServer;
            dbSched = _dbsched;
            biasControl = _biasControl;
            RNGEmulators = _RNGEmulators;
            #endregion

            #region Startup
            this.Text = "RNGPPBot - Starting Up";
            Width = width;
            Height = height;
            DoubleBuffered = true;
            tabs_label = new Label[6]; tabs_text = new string[6] { "MAIN", "WINDOWS", "SETTINGS", "DATABASE", "CHAT", "LOGS" };
            SuspendLayout();
            tabs = new Panel[6];
            for (int i = 0; i < 6; i++)
            {
                tabs[i] = new Panel();
                tabs[i].BackColor = Color.Transparent;
                tabs[i].SuspendLayout();
                tabs[i].Location = new Point(120, 0);
                tabs[i].Size = new Size(width - 140, ClientSize.Height);
                Controls.Add(tabs[i]);
                tabs[i].ResumeLayout(false);
                tabs[i].PerformLayout();
                tabs[i].Enabled = false;
                tabs[i].Visible = false;
            }

            logger = new formLogger(_log, 4, 4, tabs[5].Width - 8, 320);
            #endregion

            #region tab0: MAIN
            Button mainButton = new Button();
            mainButton.Text = "START!";
            mainButton.Font = new Font("Arial", 30);
            mainButton.Size = new Size(200,100);
            mainButton.Location = new Point(tabs[0].Width / 2 - mainButton.Width/2, 20);
            mainButton.Click += mainButton_Click;
            tabs[0].Controls.Add(mainButton);

            Label mainLabel = new Label();
            mainLabel.Location = new Point(0, 300);
            mainLabel.Text = "Planning to put all sorts of information here, but put off for now, will add soon!";
            mainLabel.Size = new Size(tabs[0].Width, 50);
            mainLabel.TextAlign = ContentAlignment.MiddleCenter;
            tabs[0].Controls.Add(mainLabel);

            sendOutMainMessage = new CheckBox();
            sendOutMainMessage.Location = new Point(tabs[0].Width / 2 - 180, 125);
            sendOutMainMessage.Text = "Send out maintenance message?";
            sendOutMainMessage.Width = 360;
            sendOutMainMessage.Height = 40;
            sendOutMainMessage.TextAlign = ContentAlignment.TopCenter;
            sendOutMainMessage.CheckAlign = ContentAlignment.TopCenter;
            tabs[0].Controls.Add(sendOutMainMessage);
            #endregion

            #region tab1: WINDOWS
            CheckBox multipleWindows = new CheckBox();
            tabs[1].Controls.Add(multipleWindows);
            multipleWindows.Location = new Point(10, 5);
            multipleWindows.Text = "Allow duplicates?";
            multipleWindows.Size = new Size(120, 20);
            multipleWindows.CheckedChanged += multipleWindows_CheckedChanged;

            string[] windowNames = new string[] { "Timer", "VoteTimer", "Leaderboards", "GoalWindow" };
            childWindows = new CHILDFORM[4];
            for(int i = 0; i< windowNames.Length; i++)
            {
                string s = windowNames[i];
                int[] windowSettings = childWindow_settings(s); //initialise settings

                Button windowButton = new Button();
                tabs[1].Controls.Add(windowButton);
                windowButton.Text = s;
                windowButton.Name = "" + i;
                windowButton.Click += windowButton_Click;
                windowButton.Location = new Point(10, 50 + 60 * i);
                windowButton.Size = new Size(90, 40);
                childWindows[i] = null;

                CheckBox windowStartWith = new CheckBox();
                windowStartWith.Text = "Start with TWIRC?";
                windowStartWith.Checked = childWindow_startWith(s);
                windowStartWith.Location = new Point(110, 50 + 60 * i);
                windowStartWith.Size = new Size(70, 40);
                windowStartWith.CheckAlign = ContentAlignment.MiddleLeft;
                windowStartWith.TextAlign = ContentAlignment.MiddleCenter;
                windowStartWith.Name = s;
                tabs[1].Controls.Add(windowStartWith);
                windowStartWith.CheckedChanged +=windowStartWith_CheckedChanged;

                ComboBox windowSaveOn = new ComboBox();
                windowSaveOn.Text = "Save on:";
                windowSaveOn.Items.AddRange(new string[] { "Close", "Change" });
                windowSaveOn.Location = new Point(185, 50 + 60 * i);
                windowSaveOn.Size = new Size(60, 30);
                windowSaveOn.Name = s;
                windowSaveOn.SelectedIndex = windowSettings[4];
                tabs[1].Controls.Add(windowSaveOn);
                windowSaveOn.SelectedValueChanged += windowSaveOn_CheckedChanged;

                if(windowStartWith.Checked)
                {
                    startChild(s);
                }                
            }//end for
            List<object[]> windowTemp = dbConn.Read(dbConn.main, "SELECT value FROM childWindows WHERE name = 'Timer' and varname='autoSaveInterval';");
            if(windowTemp.Count == 0||windowTemp[0][0].Equals(-1))
            {
                dbConn.Execute("INSERT INTO childWindows (name,varname,value) VALUES ('Timer','autoSaveInterval','60');");
                if (windowTemp.Count == 0)
                    windowTemp.Add(new object[1]);
                windowTemp[0][0] = "60";
            }
            Label windowTimerSaveText = new Label();
            windowTimerSaveText.Text = "Autosave time (seconds):";
            windowTimerSaveText.Location = new Point(250,50);
            windowTimerSaveText.Size = new Size(70, 30);
            windowTimerSaveText.TextAlign = ContentAlignment.TopCenter;
            tabs[1].Controls.Add(windowTimerSaveText);

            windowTimerSaveInterval = new NumericUpDown();
            windowTimerSaveInterval.Value = int.Parse(windowTemp[0][0] as string);
            windowTimerSaveInterval.TextAlign = HorizontalAlignment.Right;
            windowTimerSaveInterval.Location = new Point(320,50);
            windowTimerSaveInterval.Size = new Size(50, 30);
            windowTimerSaveInterval.Minimum = 1;
            windowTimerSaveInterval.Maximum = int.MaxValue;
            windowTimerSaveInterval.Increment = 10;
            tabs[1].Controls.Add(windowTimerSaveInterval);

            Button windowTimerSaveButton = new Button();
            windowTimerSaveButton.Text = "Apply Autosave";
            windowTimerSaveButton.Size = new Size(40, 30);
            windowTimerSaveButton.Location = new Point(375, 50);
            windowTimerSaveButton.Click += autosaveButton_clicked;

            timeSaver = new System.Timers.Timer(1000 * int.Parse(windowTemp[0][0] as string));
            timeSaver.AutoReset = true;
            timeSaver.Elapsed += timeSaver_Elapsed;
            timeSaver.Start();
            #endregion

            #region tab2: SETTINGS
            tabs[2].AutoScroll = true;
            List<Control[]> settings = new List<Control[]>(); ;
            settings.Add(createSetting("name", "Name of the bot", false, true));
            settings.Add(createSetting("channel", "Channel we are connecting to", false, true));
            settings.Add(createSetting("oauth", "Our oauth key", false, true));
            settings.Add(createSetting("antispam", "Autoban people whose first message contains a link?", true, false));
            settings.Add(createSetting("antistreambot", "Autoban people whose first message contains 'streambot'?", true, false));
            settings.Add(createSetting("silence", "Must I be quiet?", true, false));
            settings.Add(createSetting("logpath", "Path for !addlog"));
            settings.Add(createSetting("backgroundspath", "Path to the backgrounds folder (may take up to 30 min to update)"));
            settings.Add(createSetting("commandsurl", "The url to display when people use !rngppcommands"));
            settings.Add(createSetting("commandspath", "The local path to the commandsfile"));
            settings.Add(createSetting("votingenabled", "Enable voting at startup?", false, false));
            settings.Add(createSetting("timebetweenvote","Time between votes in seconds.",false,true));
            settings.Add(createSetting("timetovote","Time to vote in seconds.",false,true));
            settings.Add(createSetting("moneypervote", "Money awarded/deducted for each vote."));
            settings.Add(createSetting("moneyconversionrate", "Rate for how much money the protagonist is awarded for !givemoney"));
            settings.Add(createSetting("expallfunction", "Function to calculate cost for expAll. Use 'X' to denote money spend."));
            settings.Add(createSetting("cooldown", "Cooldown before a softCommand can be repeated", true, true));
            settings.Add(createSetting("welcomemessagecd", "Time in seconds after how long a new welcome message is shown."));
            settings.Add(createSetting("defaultbias","Default bias, please use !setdefaultbias instead!",true,true));
            settings.Add(createSetting("biasmaxdiff", "Maximum addition if full bias in one direction"));
            settings.Add(createSetting("biaspointspread","Rate how much these votes matter (10 = normal, 0 = not at all)"));
            

            for(int i = 0; i<settings.Count; i++)
            {
                if (settings[i] != null)
                {
                    settings[i][0].Location = new Point(0, i * 25);
                    settings[i][1].Location = new Point(335, i * 25);
                    settings[i][2].Location = new Point(440, i * 25);
                }
            }

            #endregion
            
            #region tab3: DATABASE
            Label dbLabel = new Label();
            dbLabel.Text = "Coming soon!";
            dbLabel.Location = new Point(0, 0);
            dbLabel.Size = new Size(400, 400);
            tabs[3].Controls.Add(dbLabel);
            dbLabel.Font = new System.Drawing.Font("arial", 40);
            dbLabel.TextAlign = ContentAlignment.MiddleCenter;

            #endregion
            
            #region tab4: CHAT
            Chatter = new Chat(4, 4, tabs[4].Width - 8, 316);
            tabs[4].Controls.Add(Chatter);
            irc.chatter = Chatter;
            chatMessages = new List<string>();
            chatLine = -1;

            Button chatButton = new Button();
            tabs[4].Controls.Add(chatButton);
            chatButton.Size = new Size(69, 20);
            chatButton.Location = new Point(4, 322);
            chatButton.Text = "SEND";
            chatButton.Click += chatButton_Click;

            chatBox = new TextBox();
            tabs[4].Controls.Add(chatBox);
            chatBox.Size = new Size(Chatter.Width - 69 - 4, 23);
            chatBox.Location = new Point(77, 323);
            chatBox.KeyDown += chatBox_KeyDown;

            CheckBox hideSelfChat = new CheckBox();
            tabs[4].Controls.Add(hideSelfChat);
            hideSelfChat.Text = "Hide own messages?";
            hideSelfChat.Location = new Point(4, 345);
            hideSelfChat.Size = new Size(125, 18);
            hideSelfChat.Font = new Font("Arial", 7);
            hideSelfChat.CheckedChanged += hideSelfChat_CheckedChanged;

            CheckBox hideAutoChat = new CheckBox();
            tabs[4].Controls.Add(hideAutoChat);
            hideAutoChat.Text = "Hide automated messages?";
            hideAutoChat.Location = new Point(129, 345);
            hideAutoChat.Size = new Size(160, 18);
            hideAutoChat.Font = new Font("Arial", 7);
            hideAutoChat.CheckedChanged += hideAutoChat_CheckedChanged;

            CheckBox hideSpamChat = new CheckBox();
            tabs[4].Controls.Add(hideSpamChat);
            hideSpamChat.Text = "Hide spam?";
            hideSpamChat.Location = new Point(289, 345);
            hideSpamChat.Size = new Size(85, 15);
            hideSpamChat.Font = new Font("Arial", 7);
            hideSpamChat.CheckedChanged += hideSpamChat_CheckedChanged;

            CheckBox hideBotsChat = new CheckBox();
            tabs[4].Controls.Add(hideBotsChat);
            hideBotsChat.Text = "Hide bots?";
            hideBotsChat.Location = new Point(374, 345);
            hideBotsChat.Size = new Size(120, 15);
            hideBotsChat.Font = new Font("Arial", 7);
            hideBotsChat.CheckedChanged += hideBotsChat_CheckedChanged;
            #endregion

            #region tab5: LOGS
            logLevelLastValue = 2;
            tabs[5].Controls.Add(logger);
            irc.logger = _log;

            TrackBar logSlider = new TrackBar();
            
            logSlider.Location = new Point(4, 323);
            logSlider.Size = new Size(tabs[5].Width, 15);
            logSlider.Scroll += logSlider_Scroll;
            logSlider.Maximum = 3;
            logSlider.Minimum = 0;
            logSlider.Value = logger.parent.logLevel;
            logSlider.TickFrequency = 1;
            logSlider.LargeChange = 1;
            logSlider.SmallChange = 1;
            logSlider.TickStyle = TickStyle.BottomRight;

            Label[] logLabels = new Label[4];
            for (int i = 0; i < 4;i++ )
            {
                logLabels[i] = new Label();
                tabs[5].Controls.Add(logLabels[i]);
                logLabels[i].Location = new Point(8 + ((logSlider.Width-22)/3) * i, 343);
                logLabels[i].Text = "" + i;
                logLabels[i].Size = new Size(15, 14);
            }
            tabs[5].Controls.Add(logSlider);
            #endregion

            #region tabcode
            for (int i = 0; i < 6; i++)
            {
                Label tabs_catcher = new Label();
                tabs_catcher.Location = new Point(0, 3 + i * (40 + 22));
                tabs_catcher.Width = 122;
                tabs_catcher.Height = 45;
                tabs_catcher.BackColor = Color.Transparent;
                //tabs_catcher.BackColor = Color.FromArgb(30, 0, 255, 0);
                tabs_catcher.Text = tabs_text[i];
                tabs_catcher.Name = "" + i;
                tabs_catcher.TextAlign = ContentAlignment.MiddleCenter;
                tabs_catcher.Click += tabs_catcher_Click;
                Controls.Add(tabs_catcher);
            }
            try
            {
                but_ac = new Bitmap("Resources/button_active.png");
            }
            catch ( Exception e)
            {
                logger.parent.addLog("Main", 0, "Failed loading 'Resources/button_active.png'. (" + e.Message + ")");
                but_ac  = new Bitmap(120,41);
                Graphics g = Graphics.FromImage(but_ac);
                g.FillRectangle(new SolidBrush(SystemColors.Control), 0, 0, but_ac.Width, but_ac.Height);
            }
            try{
                but_inac = new Bitmap("Resources/button_inactive.png");
            }
            catch( Exception e)
            {
                logger.parent.addLog("Main",0,"Failed loading 'Resources/button_inactive.png'. ("+e.Message+")");
                but_inac = new Bitmap(120, 41);
                Graphics g = Graphics.FromImage(but_inac);
                g.FillRectangle(new SolidBrush(SystemColors.ControlDark), 0, 0, but_inac.Width, but_inac.Height);
            }
            this.Paint += draw_tabs;
            #endregion
            ResumeLayout();
            this.Paint += menu_switcher;
            this.Closing += mainClosing;

            logger.parent.addLog("MAIN", 0, "Main Window is done loading!");
            Text = "RNGPPBot";
        }

        void mainButton_Click(object sender, EventArgs e)
        {
            running = !running;
            Button but = sender as Button;
            if(running)
            {
                if (childWindows[0] != null)
                {
                    timerWindow timer = childWindows[0] as timerWindow;
                    timer.switchRunMain(2);
                }
                if (irc.voteStatus != -2 && (but.Text == "START!" && irc.voteStatus == -1) == false) //votestatus not -2, AND not the first time click with voting disabled.
                {
                    if (sendOutMainMessage.Checked)
                        irc.say("Maintenance is over, go vote! (yadda yadda !bias up etc. you know the drill)", 3);
                    irc.toggleVoting(1);
                }
                else
                {
                    irc.voteStatus = -1;
                    if (sendOutMainMessage.Checked)
                        irc.say("Maintenance is over. voting is DISABLED.",3);
                }
                biasControl.timer_RNG.Enabled = true;
                but.Text = "Maintenance";
                but.Font = new Font("arial", 20);
            }
            else
            {
                if (childWindows[0] != null)
                {
                    timerWindow timer = childWindows[0] as timerWindow;
                    timer.switchRunMain(1);
                }
                if (irc.voteStatus != -1) {
                    irc.toggleVoting(2);
                    if(sendOutMainMessage.Checked)
                        irc.say("Maintenance! Go picnic! (voting is stopped for the duration)", 3);
                }
                else
                {
                    irc.voteStatus = -2;
                    if (sendOutMainMessage.Checked)
                        irc.say("Maintenance! Go picnic!", 3);
                }
                irc.voteTimer.Stop();  // stop the vote timers while we're down
                irc.voteTimer2.Stop();
                biasControl.timer_RNG.Enabled = false;
                but.Text = "Resume!";
                but.Font = new Font("arial", 30);
            }
        }

        void autosaveButton_clicked(object sender, EventArgs e)
        {
            timeSaver.Stop();
            timeSaver.Dispose();
            timeSaver = new System.Timers.Timer((int)(1000 * windowTimerSaveInterval.Value));
        }

        void mainClosing(object sender, EventArgs e)
        {
            Text = "RNGPPBot - Closing (this may take a few seconds)";

            timeSaver.Stop();
            timeSaver.Dispose();

            foreach (LuaServer.EmuClientHandler dyingclient in RNGEmulators.Values.ToList())
            {
                dyingclient.stopClient();
                dyingclient.deadClient(luaServer.RNGEmulators);

            }
            logger.parent.setLogControl(null);
            logger.parent.setStatusControl(null);
            logger.parent.shuttingdown = true;

            luaServer.shutdown();
            luaServer.serverSocket.Stop();

             //irc.doDisconnect();
             irc.Close();
        }

        Control[] createSetting(string name, string showName = "NULL",bool reload = true,bool restartTimers = false)
        {
            if(showName == "NULL")
                showName = name;
            Control[] returnal = new Control[3];
            returnal[0] = new Label();
            returnal[0].Text = showName;
            returnal[0].Width = 330;

            List<object[]> data = dbConn.Read(dbConn.main, "SELECT type, value FROM newsettings WHERE variable like '"+name+"' ;");
            if (!data[0][0].Equals(-1))
            {
                string type = data[0][0] as string;
                switch (type)
                {
                    case "int":
                        NumericUpDown nud = new NumericUpDown();
                        nud.Value = int.Parse(data[0][1] as string);
                        returnal[1] = nud;
                        break;
                    case "bit":
                        ComboBox cb = new ComboBox();
                        cb.Items.AddRange(new object[] { 0, 1 });
                        cb.SelectedIndex = int.Parse(data[0][1] as string);
                        returnal[1] = cb;
                        break;
                    default:
                        TextBox tb = new TextBox();
                        tb.Text = data[0][1] as string;
                        returnal[1] = tb;
                        break;

                }
                returnal[1].Width = 100;
                returnal[1].Name = name;

                returnal[2] = new callBackButton();
                returnal[2].Text = "Save";
                returnal[2].Width = 60;
                returnal[2].Name = name;
                callBackButton cbb = returnal[2] as callBackButton;
                cbb.callBack = returnal[1];
                returnal[2].Click += saveButton_Click;
                if (reload)
                    returnal[2].Click += saveButton_Reload;
                if (restartTimers)
                    returnal[2].Click += saveButton_Restart;
                tabs[2].Controls.AddRange(returnal);
            }
            else
            {
                logger.parent.addLog("MainWindow", 0, "SEVERE: setting " + name + " not found! Can't create control!");
                returnal = null;
            }
            
            return returnal;
        }

        public void saveButton_Restart(object sender, EventArgs e)
        {
            Button but = sender as Button;
            if(but.Name != "Confirm")
            {
                settingConfirm sC = new settingConfirm(this);
                sC.Show();
            }
        }

        private void saveButton_Reload(object sender, EventArgs e)
        {
            irc.loadSettings();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            callBackButton cbb = sender as callBackButton;
            object value = null;
            if(cbb.callBack.GetType() == new NumericUpDown().GetType())
            {
                NumericUpDown dj = cbb.callBack as NumericUpDown;
                value = dj.Value;
            }
            if(cbb.callBack.GetType() == new ComboBox().GetType())
            {
                ComboBox dj = cbb.callBack as ComboBox;
                value = dj.SelectedIndex;
            }
            if(cbb.callBack.GetType() == new TextBox().GetType())
            {
                TextBox dj = cbb.callBack as TextBox;
                value = dj.Text;
            }

            dbConn.safeExecute(dbConn.main, "UPDATE newsettings SET value = @par0 WHERE variable = '" + cbb.Name + "';",new object[] {value});
        }

        void timeSaver_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(childWindows[0] != null)
            {
                timerWindow tw = childWindows[0] as timerWindow;
                if(tw.running && !tw.maintenance)
                {
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.hours + "' where varname = 'hours' AND name = 'Timer';");
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.minutes + "' where varname = 'minutes' AND name = 'Timer';");
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.seconds + "' WHERE varname = 'seconds' AND name = 'Timer';");
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.countdown.ToString().ToLower() + "' WHERE varname = 'countdown' AND name = 'Timer';");
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.countUpafter.ToString().ToLower() + "' WHERE varname = 'countUpAfter' AND name = 'Timer';");
                }
            }
        }

        private void windowSaveOn_CheckedChanged(object sender, EventArgs e)
        {
            
            ComboBox q = sender as ComboBox;
            CHILDFORM ch = childWindows[getChildID(q.Name)];
            if (ch != null)
            {
                switch (q.SelectedIndex)
                {
                    case 0: ch.saveOnClose = true;
                        ch.autoSave = false;
                        break;
                    case 1: ch.saveOnClose = false;
                        ch.autoSave = true;
                        break;
                }
            }
            dbConn.Execute(dbConn.main, "UPDATE childwindows SET value = '" + q.SelectedIndex + "' WHERE name = '"+q.Name+"' AND  varname = 'saveMethod';");
        }

        private string getChildName(int id)
        {
            string name="";
            switch(id)
            {
                case 0: name = "Timer"; break;
                case 1: name = "VoteTimer"; break;
                case 2: name = "Leaderboards"; break;
                case 3: name = "GoalWindow"; break;
            }
            return name;
        }
        private int getChildID(string name)
        {
            int id = -1;
            switch (name)
            {
                case "Timer": id = 0; break;
                case "VoteTimer": id = 1; break;
                case "Leaderboards": id = 2; break;
                case "GoalWindow": id = 3; break;
            }
            return id;
        }
        private string getChildPhoneNumber(string name)
        {
            return "985-655-2500";
        }

        private CHILDFORM startChild(string name)
        {
            CHILDFORM returnal = null;
            int id = getChildID(name);
            switch (name)
            {
                case "Timer":
                    List<object[]> li = dbConn.Read(dbConn.main,"SELECT varname,value FROM childWindows WHERE name = 'Timer' AND (varname = 'hours' OR varname = 'minutes' OR varname = 'seconds' OR varname = 'countdown' OR varname = 'countUpAfter');");
                    int hours = 0,minutes = 0, seconds =0 , blackHeight=55;
                    bool countdown = false,countUp = false, locked = false;
                    if(li.Count == 0)
                    {
                        dbSched.Add(dbConn.main, "INSERT INTO childWindows (name, varname, value) VALUES ('Timer', 'blackHeight', 55), ('Timer', 'locked', 'false'), ('Timer', 'hours', 0), ('Timer', 'minutes', 0), ('Timer', 'seconds', 0), ('Timer', 'countdown', 'false'), ('Timer', 'countUpAfter', 'false');");
                    }
                    else
                    {
                        foreach(object[] entry in li)
                        {
                            string s = (string)entry[0];
                            switch(s)
                            {
                                case "hours": hours = int.Parse((string)entry[1]); break;
                                case "minutes": minutes = int.Parse((string)entry[1]); break;
                                case "seconds": seconds = int.Parse((string)entry[1]); break;
                                case "countdown": countdown = bool.Parse((string)entry[1]); break;
                                case "countUpAfter": countUp = bool.Parse((string)entry[1]); break;
                                case "blackHeight": blackHeight = int.Parse((string)entry[1]);break;
                                case "locked": locked = bool.Parse((string)entry[1]); break;
                            }
                        }
                    }
                    returnal = new timerWindow(hours, minutes, seconds, countdown,countUp,locked,blackHeight);
                    break;
                case "VoteTimer":
                    returnal = new votetimer(irc);
                    break;
                case "Leaderboards":
                    returnal = new highscores(irc);
                    break;
                case "GoalWindow":
                    returnal = new goalWindow(irc);
                    break;
            }
            int[] stuff = childWindow_settings(name);
            returnal.Name = name;
            returnal.FormClosing += childWindow_FormClosing;
            returnal.ResizeEnd += returnal_ResizeEnd;
            returnal.Show();
            returnal.Location = new Point(stuff[0], stuff[1]);
            if (stuff[2] != -1)
                returnal.Size = new Size(stuff[2], stuff[3]);
            if(stuff[4] == -1 || stuff[4] == 0)
                returnal.saveOnClose = true;
            if (stuff[4] == 1)
                returnal.autoSave = true;
            childWindows[id] = returnal;
            return returnal;
        }

        void returnal_ResizeEnd(object sender, EventArgs e)
        {
            CHILDFORM q  = sender as CHILDFORM;
            if(q.autoSave)
            {
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Height + "' WHERE name = '" + q.Name + "' AND varname = 'height';");
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Width + "' WHERE name = '" + q.Name + "' AND varname = 'width';");
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Location.X + "' WHERE name = '" + q.Name + "' AND varname = 'x';");
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Location.Y + "' WHERE name = '" + q.Name + "' AND varname = 'y';");

                if(q.Name == "Timer")
                {
                    timerWindow tw = q as timerWindow;
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.locked.ToString().ToLower() + "' WHERE varname = 'locked' AND name = 'Timer';");
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.black_height + "' WHERE varname = 'blackHeight' AND name = 'Timer';");
                }
            }
        }

        private void windowStartWith_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox b = sender as CheckBox;
            string value ="";
            if(b.Checked)
                value = "'true'";
            else
                value = "'false'";
            dbConn.Execute(dbConn.main,"UPDATE childWindows SET value = "+value+" WHERE name = '"+b.Name+"' AND varname = 'startWith';");
        }

        void multipleWindows_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox q = sender as CheckBox;
            multipleWindows = q.Checked;
        }

        void windowButton_Click(object sender, EventArgs e)
        {
            Button q = sender as Button;
            int id = int.Parse(q.Name); Form thing = null;
            if(multipleWindows || childWindows[id] == null)
                thing = startChild(getChildName(id));
            if (thing == null)
                childWindows[id].Focus();
        }

        bool childWindow_startWith(string name)
        {
            bool returnal = false;
            List<object[]> li = dbConn.Read(dbConn.main, "SELECT value FROM childWindows WHERE name = '" + name + "' AND varname = 'startWith';");
            if(li.Count == 1)
            {
                if (li[0][0].Equals(-1))//table does not exist
                    dbConn.Execute(dbConn.main, "CREATE TABLE childWindows (name VARCHAR(64) ,varname VARCHAR(64) ,value VARCHAR(256)  );");
                else
                    if (li[0][0].Equals("true"))
                        return true;
            }
            else
            {
                dbConn.Execute(dbConn.main, "INSERT INTO childWindows (name, varname, value) VALUES ('" + name + "', 'startWith', 'false');");
            }
            return returnal;
        }

        int[] childWindow_settings(string name)
        {
            int[] returnal = new int[5];
            bool[] exists = new bool[5];
            List<object[]> li = dbConn.Read(dbConn.main, "SELECT varname, value FROM childWindows WHERE name = '" + name+"';");
            if(li.Count == 1 && li[0][0].Equals(-1))//error!
            {
                dbConn.Execute(dbConn.main, "CREATE TABLE childWindows (name VARCHAR(64) ,varname VARCHAR(64) ,value VARCHAR(256)  );");
                li = new List<object[]>();
            }
            foreach(object[] entry in li)
            {
                if((string) entry[0] == "x")
                {
                    returnal[0] = int.Parse((string)entry[1]);
                    exists[0] = true;
                }
                if((string) entry[0] == "y")
                {
                    returnal[1] = int.Parse((string)entry[1]);
                    exists[1] = true;
                }
                if((string) entry[0] == "width")
                {
                    exists[2] = true;
                    if (((string)entry[1]) != "-1")
                        returnal[2] = int.Parse((string)entry[1]);
                    else
                        returnal[2] = -1;
                }
                if(entry[0] as string == "height")
                {
                    exists[3] = true;
                    if (((string)entry[1]) != "-1")
                     returnal[3] = int.Parse((string)entry[1]);
                    else
                        returnal[3] = -1;
                }
                if(entry[0] as string == "saveMethod")
                {
                    exists[4] = true;
                    if (((string)entry[1]) != "-1")
                        returnal[4] = int.Parse((string)entry[1]);
                    else
                        returnal[4] = -1;
                }
            }
            if (!(exists[0]&&exists[1]&&exists[2]&&exists[3]&&exists[4]))//UHOH shit doesn't exist?
            {
                if (!exists[0])
                {
                    dbConn.Execute(dbConn.main, "INSERT INTO childWindows (name,varname,value) VALUES ('" + name + "','x','0');");
                    returnal[0] = 0;
                }
                if (!exists[1])
                {
                    dbConn.Execute(dbConn.main, "INSERT INTO childWindows (name,varname,value) VALUES ('" + name + "','y','0');");
                    returnal[1] = 0;
                }
                if (!exists[2])
                {
                    dbConn.Execute(dbConn.main, "INSERT INTO childWindows (name,varname,value) VALUES ('" + name + "','width','-1');");
                    returnal[2] = -1;
                }
                if (!exists[3])
                {
                    dbConn.Execute(dbConn.main, "INSERT INTO childWindows (name,varname,value) VALUES ('" + name + "','height','-1');");
                    returnal[3] = -1;
                }
                if(!exists[4])
                {
                    dbConn.Execute("INSERT INTO childWindows (name, varname, value) VALUES ('" + name + "', 'saveMethod',1);");
                    returnal[4] = 1;
                }
            }
            return returnal;
        }


        void childWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            CHILDFORM q = sender as CHILDFORM;
            if (childWindows[getChildID(q.Name)] == q) 
               childWindows[getChildID(q.Name)] = null;
            if (q.saveOnClose)
            {
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Height + "' WHERE name = '" + q.Name + "' AND varname = 'height';");
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Width + "' WHERE name = '" + q.Name + "' AND varname = 'width';");
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Location.X + "' WHERE name = '" + q.Name + "' AND varname = 'x';");
                dbSched.Add(dbConn.main, "UPDATE childWindows SET value = '" + q.Location.Y + "' WHERE name = '" + q.Name + "' AND varname = 'y';");

                if(q.Name == "Timer")
                {
                    timerWindow tw = q as timerWindow;
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.locked.ToString().ToLower() + "' WHERE varname = 'locked' AND name = 'Timer';");
                    dbSched.Add("UPDATE childWindows SET value = '" + tw.black_height + "' WHERE varname = 'blackHeight' AND name = 'Timer';");
                }
            }
        }



        void logSlider_Scroll(object sender, EventArgs e)
        {
            TrackBar a = sender as TrackBar;
            logger.parent.logLevel = a.Value;
            logger.parent.Rewrite();
            logger.Focus();
        }

        void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                chatButton_Click(sender, e);
            }
            if(e.KeyCode == Keys.Up)
            {
                if(chatLine>0)
                {
                    chatLine--;
                    chatBox.Text = chatMessages[chatLine];
                }
                else
                {
                    chatLine = chatMessages.Count;
                    chatBox.Text = "";
                }
            }
            if (e.KeyCode == Keys.Down)
            {
                if (chatLine < chatMessages.Count-1)
                {
                    chatLine++;
                    chatBox.Text = chatMessages[chatLine];
                }
                else if (chatLine < chatMessages.Count)
                {
                    chatLine = chatMessages.Count;
                    chatBox.Text = "";
                }
                else //finally
                {
                    chatLine = 0;
                    chatBox.Text = chatMessages[chatLine];
                }
            }
        }

        void chatButton_Click(object sender, EventArgs e)
        {
            irc.say(chatBox.Text,1);
            chatMessages.Add(chatBox.Text);
            if(chatMessages.Count>20)
                chatMessages.RemoveAt(0);
            chatBox.Text ="";
            chatLine = chatMessages.Count;
        }

        private void hideBotsChat_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox obj = sender as CheckBox;
            Chatter.hideBots = obj.Checked;
            Chatter.reWrite();
        }

        void hideSpamChat_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox obj = sender as CheckBox;
            Chatter.hideSpam = obj.Checked;
            Chatter.reWrite();
        }

        void hideAutoChat_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox obj = sender as CheckBox;
            Chatter.hideAuto = obj.Checked;
            Chatter.reWrite();
        }

        void hideSelfChat_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox obj = sender as CheckBox;
            Chatter.hideSelf = obj.Checked;
            Chatter.reWrite();
        }

        private void menu_switcher(object sender, PaintEventArgs e)
        {
            if (oldMenu != selectedMenu)
            {
                tabs[oldMenu].Visible = false;
                tabs[oldMenu].Enabled = false;
                oldMenu = selectedMenu;
                tabs[selectedMenu].Visible = true;
                tabs[selectedMenu].Enabled = true;
            }
            Graphics gr = e.Graphics;
            switch (selectedMenu)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
            }
        }

        void tabs_catcher_Click(object sender, EventArgs e)
        {
            Label clicked = sender as Label;
            selectedMenu = int.Parse(clicked.Name);
            this.Invalidate();
        }

        private void draw_tabs(object sender, PaintEventArgs e)
        {
            //117*40
            Graphics gr = e.Graphics;
            Brush black = new SolidBrush(Color.Black);
            gr.FillRectangle(new SolidBrush(Color.FromArgb(255, 240, 240, 240)), 0, 0, 117, height);
            gr.FillRectangle(black, 118, 0, 1, 6);
            for (int i = 0; i < 6; i++)
            {
                if (selectedMenu != i)
                {
                    gr.DrawImage(but_inac, 3, 5 + i * (40 + 22), 117, 40);
                    gr.FillRectangle(black, 118, 6 + i * (40 + 22), 1, 39);
                }
                else
                {
                    gr.DrawImage(but_ac, 3, 5 + i * (40 + 22), 117, 40);
                }
                gr.FillRectangle(black, 118, 45 + i * (40 + 22), 1, 23);

            }
            //gr.FillRectangle()
        }

        private class callBackButton : Button
        {
            public Control callBack;
        }

        private class settingConfirm : Form
        {
            public settingConfirm(MainWindow parent)
            {
                Size = new Size(400, 115);
                Text = "WARNING";

                Label l1 = new Label();
                l1.Text = "This setting won't be applied in runtime, and will need a restart to work.\n Blame Harb's horrible way of programming.\n(It's really his fault!)";
                l1.Size = new Size(this.ClientSize.Width, 50);
                l1.Location = new Point(0,0);
                l1.TextAlign = ContentAlignment.MiddleCenter;

                Button save = new Button();
                save.Text = "Confirm";
                save.Name = "Confirm";
                save.Location = new Point((this.ClientSize.Width/2)-75/2, 50);
                save.Click += parent.saveButton_Restart;
                save.Click += button_Click;

                Controls.AddRange(new Control[] { l1, save});
            }

            private void button_Click(object sender, EventArgs e)
            {
                this.Close();
            }
        }
    }


    public class CHILDFORM : Form
    {
        public bool saveOnClose;
        public bool autoSave;
    }


}
