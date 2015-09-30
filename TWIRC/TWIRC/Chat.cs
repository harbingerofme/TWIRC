using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TWIRC
{
    public class Chat : RichTextBox
    {
        List<object[]> goodies;
        public bool hideSelf, hideAuto, hideSpam, hideBots;//autoinit'd as false.
        string invokeUser, invokeMessage;
        int invokeAuth, invokeId;

        public Chat(int x, int y, int w, int h)
            : base()
        {
            Location = new Point(x, y);
            Size = new Size(w, h);
            ReadOnly = true;
            DetectUrls = true;
            AcceptsTab = true;
            Font = new System.Drawing.Font("Lucida Consolas", 9);
            BackColor = Color.FromKnownColor(KnownColor.ControlLight);
            goodies = new List<object[]>();
        }

        public void Add(string user, int auth, string message, int isMeOrCommandOrSpam)//0 normal,1 = me, 2= auto, 3 = response, 4 = command, 5 = spam.
        {
            if (InvokeRequired)
            {
                invokeAuth = auth;
                invokeUser = user;
                invokeMessage = message;
                invokeId = isMeOrCommandOrSpam;
                Invoke(new MethodInvoker(AddInvoke));
            }
            else
            {
                object[] thing = new object[] { hhmmss, user, format(message), auth, isMeOrCommandOrSpam };
                if (isVisible(thing))
                {
                    Text += "\n<" + hhmmss + "><" + user + ">\n\t" + format(message);
                }
                goodies.Add(thing);
                if (goodies.Count > 1000) { Remove(1); }
                SelectionStart = Text.Length;
                ScrollToCaret();
            }
        }

        private void AddInvoke()
        {
            Add(invokeUser, invokeAuth, invokeMessage, invokeId);
        }

        public void reWrite()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(reWrite));
            }
            else
            {
                Text = "";
                foreach (object[] thing in goodies)
                {
                    if (isVisible(thing))
                    {
                        string time = (string)thing[0];
                        string user = (string)thing[1];
                        string message = (string)thing[2];
                        Text += "\n<" + time + "><" + user + ">\n\t" + message;
                    }
                }
            }
        }

        public void Remove(int amount)///zero and negative amounts are ignored. (what do you want me to do? Make up entries?)
        {
            if(InvokeRequired)
            {
                invokeAuth = amount;
                    Invoke(new MethodInvoker(InvokeRemove));
            }
            else
            {
                while (amount-- > 0)
                {
                    object[] thing = goodies[0];
                    string user = (string)thing[1];
                    string message = (string)thing[2];
                    Text.Remove(0, 10 + user.Length + 3 + message.Length);
                    goodies.RemoveAt(0);
                }
            }
        }

        private void InvokeRemove()
        {
            Remove(invokeAuth);
        }

        public bool isVisible(object[] line)
        {
            if ((hideAuto && (int)line[4] == 2) || (hideSelf && (int)line[4] > 0 && (int)line[4] < 4) || (hideSpam && (int)line[4] == 5) || (hideBots && (int)line[3] < 0))
                return false;
            return true;
        }

        int lineLength(string message)
        {
            return message.Split('\n').Length;
        }

        string format(string message)
        {
            string s = "";
            while (message.Length > 0)
            {
                message = message.Trim();
                if (message.Length < 64)
                {
                    s += message;
                    message = "";
                }
                else
                {
                    int a = message.Substring(0, 64).IndexOf(" ", 0);
                    if (a != -1)
                    {
                        a = 63;
                    }
                    s += message.Substring(0, a) + "\n\t";
                    message = message.Substring(a);
                }
            }

            return s;
        }

        string hhmmss
        {

            get
            {
                string s = "";
                if (DateTime.Now.Hour < 10)
                    s += "0";
                s += DateTime.Now.Hour + ":";
                if (DateTime.Now.Minute < 10)
                    s += "0";
                s += DateTime.Now.Minute + ":";
                if (DateTime.Now.Second < 10)
                    s += "0";
                s += DateTime.Now.Second + "";
                return s;
            }
        }
    }
}
