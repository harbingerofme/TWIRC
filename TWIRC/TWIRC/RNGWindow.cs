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
        HarbBot HB = null;
        Thread irc;
        Random randy = new Random();
        bool ishold = false;
        String lasthold = "";
        int holdtime = 0;
        string ircchannel;

        Action<string,string> sayfunc; 
        
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


        public RNGWindow(Logger newlogger, LuaServer newluaserver, Dictionary<string,LuaServer.EmuClientHandler> newrngemulators, ButtonMasher rngmasher)
        {
            
            RNGLogger = newlogger;
            RNGLuaServer = newluaserver;
            RNGEmulators = newrngemulators;
            RNGesus = rngmasher;
#if !OFFLINE
            irc = new Thread(createIrc);
            irc.Name = "RNGPPBOT irc main thread";
            irc.IsBackground = true;
            irc.Start();
#endif

            InitializeComponent();
        }

        private void createIrc()
        {
            HB = new HarbBot(RNGLogger,RNGesus,this);    
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
            timer_RNG.Enabled = !timer_RNG.Enabled;
            timer_RNG_bias.Enabled = !timer_RNG_bias.Enabled;
            timer_decay.Enabled = !timer_decay.Enabled;
            int newinterval;
            if (int.TryParse(textBox1.Text, out newinterval))
            {
                timer_RNG.Interval = newinterval;
            }
            else
            {
                RNGLogger.WriteLine("Bad interval: " + textBox1.Text + ", leaving unchanged at " + timer_RNG.Interval.ToString());
                textBox1.Text = timer_RNG.Interval.ToString();
            }


            ts_rngesus.Text = "RNG=" + timer_RNG.Enabled.ToString();
            RNGLogger.addLog("RNGTimer", 0, "enabled=" + timer_RNG.Enabled.ToString() + ", interval=" + timer_RNG.Interval.ToString());


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
                    timer_RNG.Interval = newinterval;
                }
                else
                {
                    RNGLogger.WriteLine("Bad interval: " + textBox1.Text + ", leaving unchanged at " + timer_RNG.Interval.ToString());
                    textBox1.Text = timer_RNG.Interval.ToString();
                }


                ts_rngesus.Text = "RNG=" + timer_RNG.Enabled.ToString();
                RNGLogger.addLog("RNGTimer", 0, "enabled=" + timer_RNG.Enabled.ToString() + ", interval=" + timer_RNG.Interval.ToString());

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            RNGesus.doDecay();
        }

        private void timer_decay_Tick(object sender, EventArgs e)
        {
            RNGesus.doDecay();
            RNGLogger.WriteLine("Doing decay");
            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {

                //RNGLogger.addLog("RNG-manually", 0, "rngagege");
                try
                {
                    rngclient.sendCommand("DODECAY:0"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }


            }
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

        private void button7_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias1);
  
            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:DOWN-LEFT"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

            RNGesus.setBias(bias2);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:DOWN"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias3);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:DOWN-RIGHT"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias4);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:LEFT"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias5);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:NEUTRAL"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias6);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:RIGHT"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias7);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:UP-LEFT"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias8);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:UP"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            RNGesus.setBias(bias9);

            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand("SETBIAS:UP-RIGHT"); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't rng:" + ex.Message);
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                do_manual_command();
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                do_manual_command();
            }
        }


        private void do_manual_command()
        { 
            if (textBox4.Text == "") textBox4.Text = "COMMAND";
            if (textBox2.Text == "") textBox2.Text = "0";
            string command = textBox4.Text + ":" + textBox2.Text;
            RNGLogger.WriteLine("manually sent" + command);
            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                        
                //RNGLogger.addLog("RNG-manually", 0, "rngagege");
                try
                {

                    rngclient.sendCommand(command);
                }
                catch (Exception ex)
                {
                    RNGLogger.addLog("Network", 0, "Regret, didn't send!:" + ex.Message);
                }

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }


        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                RNGLogger.WriteLine("trying to beep..." + ircchannel + ".." + textBox3.Text);
                if (ircchannel != null)
                {
                    sayfunc(ircchannel, textBox3.Text);
                }
            }
        }

        public void set_sayfunc(Action<string,string> newsayfunc)
        {
          RNGLogger.WriteLine("set sayfunc");
          sayfunc = newsayfunc;
        }
        public void set_ircchannel(string newircchannel)
        {
            RNGLogger.WriteLine("set irc channel for sayfunc: " + newircchannel);
            ircchannel = newircchannel;
        }
        
    }
}
