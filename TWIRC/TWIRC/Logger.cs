﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

#if DEBUG
using System.Diagnostics;
#endif

namespace TWIRC
{
    public class Logger
    {
        Mutex mutex_logging = new Mutex();
        delegate void SetTextCallback(string text);
        const int LogMax = 2500;

        public int logLevel;//everything below or equal to loglevel will be displayed :: 0 - ERRORS; 1 - Warnings and messages; 2 - Non important messages; 3 - DEBUG;

        int LogCount = 0; // true number of messages logged

        int LogClog = 0; // how backed up is the log? for dumping startup entities.
        RichTextBox LogTextBox = null;
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

        //OrderedDictionary<LogEntry> logtable = new OrderedDictionary<LogEntry>();
        OrderedDictionary logtable = new OrderedDictionary();
        

        public Logger()
        {
            logLevel = 2;
        }

        public void setLogControl(RichTextBox thisbox)
        {
            
            LogTextBox = thisbox;

            if (LogClog > 0 && !shuttingdown)
            {
                for (int i = 0; i < LogClog; i++)
                {
                    //logAppendText(logtable[i].name + "::" + logtable[i].level.ToString() + "::" + logtable[i]);
                    LogEntry logtableentry = (LogEntry)logtable[i];
                    logAppendText(logtableentry.name + "::" + logtableentry.level.ToString() + "::" + logtableentry.text);
                }
            }

        }

        public void dumpLogger()
        {
            if (!mutex_logging.WaitOne(2000)) //crap code to abort on some lockup of yet unknown cause.
            {
                System.Console.WriteLine("WHAT HAPPEN! dumpLogger Failed!");
                System.Console.WriteLine(Environment.StackTrace);
                //mutex_logging.ReleaseMutex();
                return;
            }
            for (int i = 0; i < logtable.Count; i++)
            {
                //logAppendText(logtable[i].name + "::" + logtable[i].level.ToString() + "::" + logtable[i].text);
                LogEntry logtableentry = (LogEntry)logtable[i];
                logAppendText(logtableentry.name + "::" + logtableentry.level.ToString() + "::" + logtableentry.text);
                
            }
            mutex_logging.ReleaseMutex();
        }


        public void setStatusControl(ToolStripStatusLabel thisstrip)
        {

            LogStatusLabel = thisstrip;

        }
        public void addLog(string name, int level, string text)
        {
            
            if (!mutex_logging.WaitOne(200)) //crap code to abort on some lockup of yet unknown cause.
            {
                System.Console.WriteLine("WHAT HAPPEN! AddLog Failed!");
                System.Console.WriteLine(Environment.StackTrace);
                //mutex_logging.ReleaseMutex();
                return;
            }

            LogCount++;
            if (shuttingdown) return;

            if (logtable.Count >= LogMax)
            {
                //logAppendLine("logtable has swollen! deleting first entry!" + logtable.Count + " real message count is " + LogCount + " LogClog count is " + LogClog);
                logtable.RemoveAt(0);

            }

            if (LogTextBox != null)
            {
                if (level <= logLevel)
                    logAppendLine(name + "::" + level + "::" + text );

            }
            else
            { 
                LogClog ++;
                
            }
            text += "\r\n"; 
            logtable.Add(LogCount, new LogEntry(name, level, text));
            mutex_logging.ReleaseMutex();
        }

        public void Rewrite()
        {
            LogTextBox.Text = "";
            for(int i = 0; i< logtable.Count ; i++)
            {
                LogEntry l = (LogEntry) logtable[i];
                if (l.level <= logLevel)
                    LogTextBox.Text += l.name + "::" + l.level + "::" + l.text;
            }
        }


        public void addText(string text)
        {
            if (!mutex_logging.WaitOne(200)) //crap code to abort on some lockup of yet unknown cause.
            {
                System.Console.WriteLine("WHAT HAPPEN! addText Failed!");
                System.Console.WriteLine(Environment.StackTrace);
                //mutex_logging.ReleaseMutex();
                return;
            }
#if DEBUG
            Debug.WriteLine("addText(" + text + "), LogCount before = " + LogCount);
#endif
            LogCount++;
            if (shuttingdown) return;

            if (LogTextBox != null)
            {
                logAppendText(text);
            }
            else
            {
                LogClog++;
            }
            
            logtable.Add(LogCount, new LogEntry("", 0, text));
            mutex_logging.ReleaseMutex();
        }

        public void logAppendLine(string text)
        {
            text += "\r\n";
            logAppendText(text);
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
    public class formLogger : RichTextBox
    {
        public Logger parent;

        public formLogger(Logger log, int x, int y, int w, int h)
            : base()
        {
            Location = new Point(x, y);
            Size = new Size(w, h);
            ReadOnly = true;
            DetectUrls = true;
            AcceptsTab = true;
            Font = new System.Drawing.Font("Lucida Consolas", 9);
            BackColor = Color.FromKnownColor(KnownColor.ControlLight);
            parent = log;
            log.setLogControl(this);
        }

    }
}
