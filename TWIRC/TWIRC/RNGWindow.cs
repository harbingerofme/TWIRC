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

        private void btn_RestartIRC_Click(object sender, EventArgs e)
        {
            HB.reconnect();
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
            HarbBot hb = Program.HarbBot;
            hb.sendMess(hb.channels, "/me is going down for matinence, be back soon!");
            Program.RNGLogger.WriteLine("Silence has been set to: On");
            ts_MatinenceLevel.Text = "On";
            hb.silence = true; 
            new SQLiteCommand("UPDATE settings SET silence=1;", hb.dbConn).ExecuteNonQuery();
        }

        private void ts_Matinence_off_Click(object sender, EventArgs e)
        {
            HarbBot hb = Program.HarbBot;
            hb.silence = false;
            new SQLiteCommand("UPDATE settings SET silence=0;", hb.dbConn).ExecuteNonQuery();
            Program.RNGLogger.WriteLine("Silence has been set to: Off");
            ts_MatinenceLevel.Text = "Off";
            hb.sendMess(hb.channels, "/me is back! Enjoy the cake!");
        }
    }
}
