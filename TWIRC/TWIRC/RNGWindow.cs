using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace RNGBot
{
    public partial class RNGWindow : Form
    {
        delegate void SetTextCallback(string text);
        Logger RNGLogger = null;
        LuaServer RNGLuaServer = null;
        Dictionary<string, LuaServer.EmuClientHandler> RNGEmulators = null;
        ButtonMasher RNGesus = null;
        Thread irc;

        public RNGWindow(Logger newlogger, LuaServer newluaserver, Dictionary<string,LuaServer.EmuClientHandler> newrngemulators, ButtonMasher rngmasher)
        {
            RNGLogger = newlogger;
            RNGLuaServer = newluaserver;
            RNGEmulators = newrngemulators;
            RNGesus = rngmasher;
            irc = new Thread(createIrc);
            irc.Name = "irc";
            irc.Start();

            InitializeComponent();
        }

        private void createIrc()
        {
            HarbBot HB = new HarbBot(RNGLogger);
        }

        private void RNGWindow_Load(object sender, EventArgs e)
        {
           RNGLogger.addLog("RNGWindow_Load", 0, "Success, i guess");
           RNGLogger.setLogControl(text_log); // as this calls the log dumper, best not to add immediately afterwards, lest an unfortunate game of digital chicken occur.
           RNGLogger.setStatusControl(ts_counter0);
        }

        private void RNGWindow_clososos(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

            irc.Abort();
            
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

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (LuaServer.EmuClientHandler dyingclient in RNGEmulators.Values.ToList())
            {
                dyingclient.stopClient();
                dyingclient.deadClient(RNGLuaServer.RNGEmulators);
               
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
            int newinterval;
            if (int.TryParse(textBox1.Text, out newinterval))
            {
                timer1.Interval = newinterval;
            }
            else
            {
                RNGLogger.WriteLine("Bad interval: " + textBox1.Text + ", leaving unchanged at " + timer1.Interval.ToString());
                textBox1.Text = timer1.Interval.ToString();
            }


            ts_rngesus.Text = "RNG=" + timer1.Enabled.ToString();
            RNGLogger.addLog("RNGTimer", 0, "enabled=" + timer1.Enabled.ToString() + ", interval=" + timer1.Interval.ToString());


        }

        private void button3_Click(object sender, EventArgs e)
        {
            irc.Abort();
            irc = new Thread(createIrc);
            irc.Name = "irc";
            irc.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int nextAction = RNGesus.doRNG(7);


            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {

                //RNGLogger.addLog("RNG-manually", 0, "rngagege");
                try
                {
                    rngclient.sendCommand(nextAction);
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

        private void button5_Click(object sender, EventArgs e)
        {
            text_log.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RNGLogger.dumpLogger();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                int newinterval;
                if (int.TryParse(textBox1.Text, out newinterval))
                {
                    timer1.Interval = newinterval;
                }
                else
                {
                    RNGLogger.WriteLine("Bad interval: " + textBox1.Text + ", leaving unchanged at " + timer1.Interval.ToString());
                    textBox1.Text = timer1.Interval.ToString();
                }


                ts_rngesus.Text = "RNG=" + timer1.Enabled.ToString();
                RNGLogger.addLog("RNGTimer", 0, "enabled=" + timer1.Enabled.ToString() + ", interval=" + timer1.Interval.ToString());

            }
        }
    }
}
