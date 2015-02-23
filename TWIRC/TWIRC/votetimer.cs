using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace RNGBot
{
    class votetimer : Form
    {
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
            if (bot != null)
            {
                me = this;
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                HB = bot;
                Height = h;
                Width = w;
                BackColor = Color.Black;
                DoubleBuffered = true;
                Name = "votetimer";
                Text = "VoteTimer";

                title = new Label();
                title.Location = new Point(11, 6);
                title.Size = new Size(195, 20);
                title.TextAlign = ContentAlignment.TopCenter;
                title.Font = new Font("Verdana", 12);
                title.BackColor = Color.Black;
                title.ForeColor = Color.White;
                title.Text = "title";
                title.MouseClick += votetimer_MouseClick;
                

                time = new Label();
                time.Location = new Point(11, 27);
                time.Size = new Size(195, 20);
                time.TextAlign = ContentAlignment.TopCenter;
                time.Font = new Font("Verdana", 10);
                time.BackColor = Color.Black;
                time.ForeColor = Color.White;
                time.Text = "time";
                time.MouseClick += votetimer_MouseClick;
                Controls.Add(time); Controls.Add(title);

                lvt = HB.lastVoteTime;
                ttv = HB.timeToVote;
                tbv = HB.timeBetweenVotes;
                voting = HB.voteStatus;

                one = new Thread(tmr_Elapsed);
                one.IsBackground = true;
                one.Name = "VoteTimer Window";
                one.Priority = ThreadPriority.Lowest;
                one.Start();

                MouseClick += votetimer_MouseClick;
                Paint += votetimer_Paint;
                FormClosing += closing;
            }
        }

        void tmr_Elapsed()
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

        void votetimer_MouseClick(object sender, MouseEventArgs e)
        {
            if (clicked)
            {
                me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                clicked = false;
            }
            else
            {
                me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                clicked = true;
            }
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
                time.Text = tim;
                title.Text = tit;
            }
        }
    }
}
