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

        bool ishold = false;
        String lasthold = "";
        int holdtime = 0,messagePoint = -1;

        List<string> sendMessages = new List<string>();
        HarbBot HB = null;


        public RNGWindow(Logger newlogger, HarbBot bot)
        {

            RNGLogger = newlogger;

#if !OFFLINE
            HB = bot;
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

            RNGLogger.setLogControl(null);
            RNGLogger.setStatusControl(null);
            RNGLogger.shuttingdown = true;

            try
            {
                HB.running = false;
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
            
        }



        private void timer_RNG_bias_Tick(object sender, EventArgs e)
        {
            
        }

        private void btn_DownLeft_Click(object sender, EventArgs e)
        {

        }

        private void btn_Down_Click(object sender, EventArgs e)
        {

        }

        private void btn_DownRight_Click(object sender, EventArgs e)
        {

        }

        private void btn_Left_Click(object sender, EventArgs e)
        {

        }

        private void btn_Neutral_Click(object sender, EventArgs e)
        {

        }

        private void btn_Right_Click(object sender, EventArgs e)
        {

        }

        private void btn_UpLeft_Click(object sender, EventArgs e)
        {

        }

        private void btn_Up_Click(object sender, EventArgs e)
        {

        }

        private void btn_UpRight_Click(object sender, EventArgs e)
        {

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
        }

        private void txt_Halp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Most buttons don't do anything anymore, yay!");
        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void btn_dumpBiases_Click(object sender, EventArgs e)
        {
        }

        private void timer_save_Tick(object sender, EventArgs e)
        {
        }

        private void btn_Leaderboard_Click(object sender, EventArgs e)
        {
        }

        private void btn_voteTimer_Click(object sender, EventArgs e)
        {
        }

        private void RNGWindow_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.mainwindow_pos = this.Location;
            Properties.Settings.Default.Save();
        }


        
    }
}
