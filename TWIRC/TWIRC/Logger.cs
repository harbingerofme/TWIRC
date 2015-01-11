using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RNGBot
{
    public class Logger
    {
        delegate void SetTextCallback(string text);

        int LogClog = 0; // how backed up is the log? for dumping startup entities.
        TextBox LogTextBox = null;
        ToolStripStatusLabel LogStatusLabel = null;

        public bool shuttingdown = false; 
        

        private class LogEntry
        {
            public string name, text;
            public int level;
            
            public LogEntry(string n, int l, string t)
            {
                name = n;
                level = l;
                text = t;
            }
        }

        List<LogEntry> logtable = new List<LogEntry>();

        public Logger()
        {

        }

        public void setLogControl(TextBox thisbox)
        {
            LogTextBox = thisbox;

            if (LogClog > 0)
            {
                for (int i = 0; i < LogClog; i++)
                {
                    logAppendText(logtable[i].name + "::" + logtable[i].level.ToString() + "::" + logtable[i].text);
                }
            }

        }

        public void dumpLogger()
        {
            for (int i = 0; i < logtable.Count; i++)
            {
                logAppendText(logtable[i].name + "::" + logtable[i].level.ToString() + "::" + logtable[i].text);
            }

        }

        public void setStatusControl(ToolStripStatusLabel thisstrip)
        {

            LogStatusLabel = thisstrip;

        }
        public void addLog(string name, int level, string text)
        {

            if (shuttingdown) return; 


            if (LogTextBox != null)
            {
                logAppendLine(name + "::" + level.ToString() + "::" + text );
            }
            else
            { 
                LogClog ++;
                
            }
            text += "\r\n"; 
            logtable.Add(new LogEntry(name, level, text));
        }

        public void addText(string text)
        {
            if (shuttingdown) return;

            logtable.Add(new LogEntry("", 0, text));

            if (LogTextBox != null)
            {
                logAppendText(text);
            }
            else
            {
                LogClog++;
            }
        }

        public void logAppendLine(string text)
        {
            text += "\r\n";
            logAppendText(text);
            /*                // InvokeRequired required compares the thread ID of the 
                            // calling thread to the thread ID of the creating thread. 
                            // If these threads are different, it returns true. 
                            if (LogTextBox.InvokeRequired)
                            {
                                SetTextCallback d = new SetTextCallback(logAppendText);
                                LogTextBox.Parent.Invoke(d, new object[] { text });
                            }
                            else
                            {
                                LogTextBox.AppendText(text);
                            }
                        }*/
        }

        public void logAppendText(string text)
        {
            if (LogTextBox == null)
            {
                return;
            }

            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (LogTextBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(logAppendText);
                LogTextBox.Parent.Invoke(d, new object[] { text });
            }
            else
            {
                LogTextBox.AppendText(text);
            }
        }

        public void Write(string text)
        {
            addText(text);
        }


        public void WriteLine(string text)
        {
            addText(text + "\r\n");
        }






        public void setStatusText(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 

            if (LogStatusLabel.GetCurrentParent().InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(setStatusText);
                LogStatusLabel.GetCurrentParent().Invoke(d, new object[] { text });
            }
            else
            {
                LogStatusLabel.Text = text;
                //ts_counter0.
                //RNGWindow
            }
        }

    }
}
