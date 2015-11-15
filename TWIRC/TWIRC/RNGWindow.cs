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
using System.Data.SQLite;


namespace SayingsBot
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
           if (HB.silence)
           {
               this.ts_MatinenceLevel.Text = "On";
           }
           else
           {
               this.ts_MatinenceLevel.Text = "Off";
           }
        }

         private void RNGWindow_FormClosed(object sender, FormClosedEventArgs e)
        {

            RNGLogger.setLogControl(null);
            RNGLogger.setStatusControl(null);
            RNGLogger.shuttingdown = true;

            try
            {
                HB.running = false;
                HB.Server.CloseConnection();
                HB.Close();
            }
            catch { }
             
            Application.Exit(); //The process is still running after closing the window(on WinXP), so maybe this will help?
        }

         private void RNGWindow_SizeChanged(object sender, System.EventArgs e)
         {
             //widths
             txt_Halp.Width = 76;
             btn_RestartIRC.Width = 76;
             txt_IRCManual.Width = this.Width;
             text_log.Width = this.Width;
             txt_Halp.Height = 22;
             btn_RestartIRC.Height = 22;
             txt_IRCManual.Height = 20;
             txt_Halp.Left = ((this.Width - txt_Halp.Width)-16);
             txt_Halp.Top = ((this.Height - txt_Halp.Height)-63);
             btn_RestartIRC.Left = ((txt_Halp.Left - btn_RestartIRC.Width)-6);
             btn_RestartIRC.Top = txt_Halp.Top;
             txt_IRCManual.Top = txt_Halp.Top - 26;
             text_log.Height = this.Height - (this.Height - txt_IRCManual.Top);
             btnUpdateNotify.Top = btn_RestartIRC.Top;
             btnUpdateNotify.Left = ((btn_RestartIRC.Left - btnUpdateNotify.Width) - 6);


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

        public void btn_RestartIRC_Click(object sender, EventArgs e)
        {
            HB.doReconnect();
        }

        private void btn_ClearLog_Click(object sender, EventArgs e)
        {
            text_log.Clear();
        }

        private void btn_DumpLog_Click(object sender, EventArgs e)
        {
            RNGLogger.dumpLogger();
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

        private void txt_Halp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Most buttons don't do anything anymore, yay!");
        }

        private void RNGWindow_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.mainwindow_pos = this.Location;
            Properties.Settings.Default.Save();
        }

        public void setColourText(string txt)
        {
            this.ts_botColour.Text = txt;
        }

        private void ts_Matinence_on_Click(object sender, EventArgs e)
        {
            HB.sendMess(HB.channels, "/me is going down for maintenance, be back soon!");
            Program.RNGLogger.WriteLine("Silence has been set to: On");
            ts_MatinenceLevel.Text = "On";
            HB.silence = true; 
            new SQLiteCommand("UPDATE settings SET silence=1;", HB.dbConn).ExecuteNonQuery();
        }

        private void ts_Matinence_off_Click(object sender, EventArgs e)
        {
            HB.silence = false;
            new SQLiteCommand("UPDATE settings SET silence=0;", HB.dbConn).ExecuteNonQuery();
            Program.RNGLogger.WriteLine("Silence has been set to: Off");
            ts_MatinenceLevel.Text = "Off";
            HB.sendMess(HB.channels, "/me is back! Enjoy the cake!");
        }

        private void btnUpdateNotify_Click(object sender, EventArgs e)
        {
            HB.say("I've been updated! I think I'm now version " + Application.ProductVersion + ". Ask dude22072 for the changes or check the changelog!");
        }
    }
}
