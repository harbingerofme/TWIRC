using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

namespace TWIRC
{
    class votetimer : CHILDFORM
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


        HarbBot HB = null;
        int w = 217;
        int h = 71;
        int lvt = 0;
        int ttv = 0;
        int tbv = 0;
        int voting = 0;
        Thread one;
        bool clicked = false; bool running = true;
        int cntr = 0;

        Label title;
        Label time;

        Form me;

        public votetimer(HarbBot bot)
        {
            Height = h;
            Width = w;
            me = this;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            BackColor = Color.Black;
            DoubleBuffered = true;
            Name = "votetimer";
            Text = "VoteTimer";
            if (bot != null)
            {
                HB = bot;

                title = new Label();
                title.Location = new Point(11, 6);
                title.Size = new Size(195, 20);
                title.TextAlign = ContentAlignment.TopCenter;
                title.Font = new Font("Verdana", 12);
                title.BackColor = Color.Black;
                title.ForeColor = Color.White;
                title.Text = "title";
                title.MouseDown += votetimer_mouseDown;
                

                time = new Label();
                time.Location = new Point(11, 27);
                time.Size = new Size(195, 20);
                time.TextAlign = ContentAlignment.TopCenter;
                time.Font = new Font("Verdana", 10);
                time.BackColor = Color.Black;
                time.ForeColor = Color.White;
                time.Text = "time";
                time.MouseDown += votetimer_mouseDown;
                Controls.Add(time); Controls.Add(title);

                lvt = HB.lastVoteTime;
                ttv = HB.timeToVote;
                tbv = HB.timeBetweenVotes;
                voting = HB.voteStatus;

                one = new Thread(background_thread);
                one.IsBackground = true;
                one.Name = "VoteTimer Window";
                one.Priority = ThreadPriority.Lowest;
                one.Start();

                Location = Properties.Settings.Default.votetimer_pos;
                MouseDown += votetimer_mouseDown;
                LocationChanged += votetimer_LocationChanged;
                Paint += votetimer_Paint;
                FormClosing += closing;
            }
        }

        void background_thread()
        {
            while (running)
            {
                if (voting != HB.voteStatus)
                {
                    voting = HB.voteStatus;
                    cntr = 0;
                }
                else
                {
                    cntr++;
                }
                Invalidate();
                Thread.Sleep(1000);
            }
        }

        private void votetimer_mouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void votetimer_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.votetimer_pos = this.Location;
            Properties.Settings.Default.Save();
        }

        void closing(object sender, EventArgs e)
        {
            running = false;
        }

        void votetimer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.DarkGray, 6, Height-16, 205, 10);
            string tit= "",tim="";int m= 0,s =0;
            if (voting == 1)
            {
                tit = "VOTE NOW!";
                s = (ttv - cntr);
                e.Graphics.FillRectangle(Brushes.White, 6, Height - 16, (int) (205 * ((double)cntr / (double)ttv)), 10);
            }
             if (voting == 0)
             {
                tit = "NEW VOTE IN:";
                s = (tbv - cntr);
                e.Graphics.FillRectangle(Brushes.White, 6, Height - 16, (int) (205 * ((double) cntr / (double) tbv)), 10);

             }
             m = (int)Math.Floor((double)(s) / 60);
             s = s % 60;
             tim += m;
             tim += ":";
             if (s < 10)
             {
                 tim += "0";
             }
             tim += s;
            if ( voting == -1)
            {
                time.Text = "";
                title.Text = "VOTING DISABLED";
            }
            else
            {
                if (m < 0) { tim = "error, fixed soon!"; }
                time.Text = tim;
                title.Text = tit;
            }
        }
    }
}
