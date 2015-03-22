using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Reflection;


namespace TWIRC
{
    public partial class RNGWindow : Form
    {
        delegate void SetTextCallback(string text);
        Logger RNGLogger = null;
        LuaServer RNGLuaServer = null;
        Dictionary<string, LuaServer.EmuClientHandler> RNGEmulators = null;
        ButtonMasher RNGesus = null;
        Random randy = new Random();

        frmBias biasWindow;
        highscores highWindow;
        votetimer timerWindow;

        bool ishold = false;
        String lasthold = "";
        int holdtime = 0,messagePoint = -1;

        List<string> sendMessages = new List<string>();
        HarbBot HB = null;
        List<System.Timers.Timer> HBtimerList = new List<System.Timers.Timer>();


        //Action<string,string> sayfunc; 
        
        //ewww
                           // LT   DN     UP     RT
        double[] bias1 = { 1.20, 1.20, 1.00, 1.00, 0.96, 0.92, 0.82 };
        double[] bias2 = { 1.00, 1.28, 1.00, 1.00, 0.96, 0.92, 0.82 };
        double[] bias3 = { 1.00, 1.20, 1.00, 1.20, 0.96, 0.92, 0.82 };
        double[] bias4 = { 1.28, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
        double[] bias5 = { 1.00, 1.00, 1.00, 1.00, 0.96, 0.92, 0.82 };
        double[] bias6 = { 1.00, 1.00, 1.00, 1.28, 0.96, 0.92, 0.82 };
        double[] bias7 = { 1.20, 1.00, 1.20, 1.00, 0.96, 0.92, 0.82 };
        double[] bias8 = { 1.00, 1.00, 1.28, 1.00, 0.96, 0.92, 0.82 };
        double[] bias9 = { 1.00, 1.00, 1.20, 1.20, 0.96, 0.92, 0.82 };


        public RNGWindow(Logger newlogger, LuaServer newluaserver, Dictionary<string, LuaServer.EmuClientHandler> newrngemulators, ButtonMasher rngmasher, frmBias newbiaswindow, HarbBot bot)
        {

            RNGLogger = newlogger;
            RNGLuaServer = newluaserver;
            RNGEmulators = newrngemulators;
            RNGesus = rngmasher;

            biasWindow = newbiaswindow;
#if !OFFLINE
            highWindow = new highscores();
            highWindow.Show();
            timerWindow = new votetimer(bot);
            timerWindow.Show();
            
            HB = bot;
            HBtimerList.Add(HB.voteTimer);
            HBtimerList.Add(HB.voteTimer2);
#endif
            InitializeComponent();
        }

        private void RNGWindow_Load(object sender, EventArgs e)
        {
           RNGLogger.addLog("RNGWindow_Load", 0, "Success, i guess");
           RNGLogger.setLogControl(text_log); // as this calls the log dumper, best not to add immediately afterwards, lest an unfortunate game of digital chicken occur.
           RNGLogger.setStatusControl(ts_counter0);
           this.StartPosition = FormStartPosition.Manual;
           this.Location = Properties.Settings.Default.mainwindow_pos;
           ts_isconnected.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

         private void RNGWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

            foreach (LuaServer.EmuClientHandler dyingclient in RNGEmulators.Values.ToList())
            {
                dyingclient.stopClient();
                dyingclient.deadClient(RNGLuaServer.RNGEmulators);

            }
            RNGLogger.setLogControl(null);
            RNGLogger.setStatusControl(null);
            RNGLogger.shuttingdown = true;
            
            RNGLuaServer.shutdown();
            RNGLuaServer.serverSocket.Stop();

            try
            {
                HB.running = false;
                foreach (System.Timers.Timer timer in HBtimerList)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                HB.Close();
            }
            catch { }
            
        }

        private void text_log_TextChanged(object sender, EventArgs e)
        {
            int maxlines = 1000;
            if (this.text_log.Lines.Count() > maxlines)
            {
                var lines = this.text_log.Lines;
                var newLines = lines.Skip(maxlines);
                this.text_log.Lines = newLines.ToArray();
            }

        }

        private void btn_KillClients_Click(object sender, EventArgs e)
        {
            foreach (LuaServer.EmuClientHandler dyingclient in RNGEmulators.Values.ToList())
            {
                dyingclient.stopClient();
                dyingclient.deadClient(RNGLuaServer.RNGEmulators);
               
            }

        }

        private void btn_RNGesus_Click(object sender, EventArgs e)
        {

        }

        private void btn_RestartIRC_Click(object sender, EventArgs e)
        {
            HB.reconnect();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (RNGLuaServer.get_client_count() == 0) return; // we're not doing anything

            String command;
            int nextRNG = RNGesus.doRNG();

            if (!ishold && randy.Next(8) == 3 && nextRNG < 4) // do we hold?
            {
                //RNGLogger.WriteLine("Doing hold: " + nextRNG);
                ishold = true;
                lasthold = nextRNG.ToString();
                holdtime = 0; // randy.Next(8);
                command = "HOLD:";

            }
            else
            {
                command = "PRESS:";
            }

            if (ishold)
            {
                //RNGLogger.WriteLine("holding!" + holdtime + "     " + lasthold);
                holdtime-- ;
                command = "HOLD:" + lasthold;
            }
            else 
            {
                command += nextRNG;
            }

            if (holdtime < 1)
            {
                ishold = false;
            }


            
            
            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {

                //RNGLogger.addLog("RNG-manually", 0, "rngagege");
                try
                {
                    rngclient.sendCommand(command);
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }


            }
        }

        private void ts_votestatus_Click(object sender, EventArgs e)
        {

        }

        private void btn_ClearLog_Click(object sender, EventArgs e)
        {
            text_log.Clear();
        }

        private void btn_DumpLog_Click(object sender, EventArgs e)
        {
            RNGLogger.dumpLogger();
        }

        private void txt_RNGInterval_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                int newinterval;
                if (int.TryParse(txt_RNGInterval.Text, out newinterval))
                {
                    timer_RNG.Interval = newinterval;
                }
                else
                {
                    RNGLogger.WriteLine("Bad interval: " + txt_RNGInterval.Text + ", leaving unchanged at " + timer_RNG.Interval.ToString());
                    txt_RNGInterval.Text = timer_RNG.Interval.ToString();
                }


                ts_rngesus.Text = "RNG=" + timer_RNG.Enabled.ToString();
                RNGLogger.addLog("RNGTimer", 0, "enabled=" + timer_RNG.Enabled.ToString() + ", interval=" + timer_RNG.Interval.ToString());

            }
        }

        private void btn_Decay_Click(object sender, EventArgs e)
        {
            RNGesus.doDecay();
        }



        private void timer_RNG_bias_Tick(object sender, EventArgs e)
        {
            String newbiasname = "";
            double[] newbias = {0,0,0,0,0,0,0,0,0,0};
            switch (randy.Next(1, 10))
            {
                case 1:
                    newbiasname = "DOWN-LEFT";
                    newbias = bias1;
                    break;
                case 2:
                    newbiasname = "DOWN";
                    newbias = bias2;
                    break;
                case 3:
                    newbiasname = "DOWN-RIGHT";
                    newbias = bias3;
                    break;
                case 4:
                    newbiasname = "LEFT";
                    newbias = bias4;
                    break;
                case 5:
                    newbiasname = "NEUTRAL";
                    newbias = bias5;
                    break;
                case 6:
                    newbiasname = "RIGHT";
                    newbias = bias6;
                    break;
                case 7:
                    newbiasname = "UP-LEFT";
                    newbias = bias7;
                    break;
                case 8:
                    newbiasname = "UP";
                    newbias = bias8;
                    break;
                case 9:
                    newbiasname = "UP-RIGHT";
                    newbias = bias9;
                    break;
                default:
                    newbiasname = "OOPS";
                    break;
            }


            RNGesus.setBias(newbias);
            RNGLogger.WriteLine("SETBIAS" + newbiasname);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {

                //RNGLogger.addLog("RNG-manually", 0, "rngagege");
                try
                {
                    rngclient.sendCommand("SETBIAS:" + newbiasname); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }


            }
        }

        private void btn_DownLeft_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.downLeft);
            RNGLuaServer.send_to_all("SETBIAS", "DOWNLEFT");
            RNGLogger.WriteLine("Manually set bias to DOWNLEFT");
        }

        private void btn_Down_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.down);
            RNGLuaServer.send_to_all("SETBIAS", "DOWN");
            RNGLogger.WriteLine("Manually set bias to DOWN");
        }

        private void btn_DownRight_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.downRight);
            RNGLuaServer.send_to_all("SETBIAS", "DOWNRIGHT");
            RNGLogger.WriteLine("Manually set bias to DOWNRIGHT");
        }

        private void btn_Left_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.left);
            RNGLuaServer.send_to_all("SETBIAS", "LEFT");
            RNGLogger.WriteLine("Manually set bias to LEFT");
        }

        private void btn_Neutral_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.neutral);
            RNGLuaServer.send_to_all("SETBIAS", "NEUTRAL");
            RNGLogger.WriteLine("Manually set bias to NEUTRAL");
        }

        private void btn_Right_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.getBias("right"));
            RNGLuaServer.send_to_all("SETBIAS","RIGHT");
            RNGLogger.WriteLine("Manually set bias to RIGHT");
        }

        private void btn_UpLeft_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.upLeft);
            RNGLuaServer.send_to_all("SETBIAS", "UPLEFT");
            RNGLogger.WriteLine("Manually set bias to UPLEFT");
        }

        private void btn_Up_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.up);
            RNGLuaServer.send_to_all("SETBIAS","UP");
            RNGLogger.WriteLine("Manually set bias to UP");
        }

        private void btn_UpRight_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(Biases.upRight);
            RNGLuaServer.send_to_all("SETBIAS", "UPRIGHT");
            RNGLogger.WriteLine("Manually set bias to UPRIGHT");
        }

        private void txt_Command_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_Command_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                do_manual_command();
            }
        }

        private void txt_Parameter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                do_manual_command();
            }
        }


        private void do_manual_command()
        { 
            if (txt_Command.Text == "") txt_Command.Text = "COMMAND";
            if (txt_Parameter.Text == "") txt_Parameter.Text = "0";
            string command = txt_Command.Text + ":" + txt_Parameter.Text;
            RNGLogger.WriteLine("manually sent" + command);
            RNGLuaServer.send_to_all(txt_Command.Text, txt_Parameter.Text);
        }

        private void txt_Parameter_TextChanged(object sender, EventArgs e)
        {

        }


        private void txt_IRCManual_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                RNGLogger.WriteLine("Manual irc message:");
                HB.say(txt_IRCManual.Text);
                sendMessages.Add(txt_IRCManual.Text);
                if (sendMessages.Count > 100)
                {
                    sendMessages.RemoveAt(0);
                }
                txt_IRCManual.Text = "";//empty the textbox.
                messagePoint = sendMessages.Count;
            }
            if(e.KeyCode == Keys.Down)
            {
                if(messagePoint<sendMessages.Count)
                {
                    messagePoint++;
                    if(messagePoint==sendMessages.Count)
                    {
                        txt_IRCManual.Text = "";//wipe textbox
                    }
                    else
                    {
                        txt_IRCManual.Text = sendMessages[messagePoint];
                    }
                }
            }
            if(e.KeyCode == Keys.Up)
            {
                if (messagePoint > 0)
                {
                    messagePoint--;
                    txt_IRCManual.Text = sendMessages[messagePoint];
                }
            }
        }

        private void ts_counter0_Click(object sender, EventArgs e)
        {

        }

        private void timer_interface_stats_Tick(object sender, EventArgs e)
        {
            ts_counter0.Text = "Clients: " + RNGLuaServer.get_client_count();
        }

        private void txt_Halp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Sometimes, it is not so much knowing what to click as it is what not to click.");
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            RNGLuaServer.send_to_all("SAVE", "0");
            RNGLogger.WriteLine("Saved game!");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

            timer_RNG.Enabled = checkBox1.Checked;

            int newinterval;
            if (int.TryParse(txt_RNGInterval.Text, out newinterval))
            {
                timer_RNG.Interval = newinterval;
            }
            else
            {
                RNGLogger.WriteLine("Bad interval: " + txt_RNGInterval.Text + ", leaving unchanged at " + timer_RNG.Interval.ToString());
                txt_RNGInterval.Text = timer_RNG.Interval.ToString();
            }


            ts_rngesus.Text = "RNG=" + timer_RNG.Enabled.ToString();
            RNGLogger.addLog("RNGTimer", 0, "enabled=" + timer_RNG.Enabled.ToString() + ", interval=" + timer_RNG.Interval.ToString());


        }

        private void button1_Click(object sender, EventArgs e)
        {
            biasWindow.Show();
        }

        private void btn_dumpBiases_Click(object sender, EventArgs e)
        {
            RNGLogger.WriteLine("Default:" + Biases.printBias(RNGesus.getDefaultBias()));
            RNGLogger.WriteLine("Current:" + Biases.printBias(RNGesus.getCurrentBias()));
        }

        private void timer_save_Tick(object sender, EventArgs e)
        {
            RNGLogger.WriteLine("Autosaving...");
            RNGLuaServer.send_to_all("SAVE","0");
        }

        private void btn_Leaderboard_Click(object sender, EventArgs e)
        {
            Point oldwin = new Point();
            Size oldwins = new Size();
            oldwins = highWindow.Size;
            oldwin.X = highWindow.Location.X;
            oldwin.Y = highWindow.Location.Y;
            highWindow.Close();
            highWindow = new highscores();
            highWindow.StartPosition = FormStartPosition.Manual;
            highWindow.Location = oldwin;
            highWindow.Size = oldwins;
            highWindow.resize(oldwins.Width, oldwins.Height);
            highWindow.Show();
        }

        private void btn_voteTimer_Click(object sender, EventArgs e)
        {
            Point oldwin = new Point();
            oldwin.X = timerWindow.Location.X;
            oldwin.Y = timerWindow.Location.Y;
            timerWindow.Close();
            timerWindow = new votetimer(HB);
            timerWindow.StartPosition = FormStartPosition.Manual;
            timerWindow.Location = oldwin;
            timerWindow.Show();
        }

        private void RNGWindow_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.mainwindow_pos = this.Location;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RNGLogger.WriteLine(RNGesus.rngTest(Convert.ToInt32(txt_numrolls.Text)));
        }


        
    }
}
