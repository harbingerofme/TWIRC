using System;
using System.Drawing;
using System.Windows.Forms;

namespace TWIRC
{
    class goalWindow : CHILDFORM
    {
        Label the_label;
        System.Timers.Timer timer;
        HarbBot _bot;

        public goalWindow(HarbBot bot = null)
        {
            _bot = bot;
            this.ClientSize = new System.Drawing.Size(240, 90);
            BackColor = Color.Black;
            DoubleBuffered = true;

            Name = "goalWindow";
            Text = "GoalWindow";

            the_label = new Label();
            the_label.Left = 0;
            the_label.Top = 0;
            the_label.TextAlign = ContentAlignment.MiddleCenter;
            the_label.Width = ClientSize.Width;
            the_label.Height = ClientSize.Height;
            the_label.Font = new Font("arial", 16);
            the_label.ForeColor = Color.White;
            Controls.Add(the_label);

            if(bot != null)
            {
                the_label.Text = bot.goal;
            }

            timer = new System.Timers.Timer(5*60*1000);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();

            ResizeEnd += goalWindow_ResizeEnd;
            KeyPress += goalWindow_KeyPress;
        }

        void goalWindow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 'r')
            {
                this.ClientSize = new System.Drawing.Size(240, 90);
            }
        }

        void goalWindow_ResizeEnd(object sender, EventArgs e)
        {
            the_label.Width = ClientSize.Width;
            the_label.Height = ClientSize.Height;
        }

        void timer_Elapsed(object sender = null, System.Timers.ElapsedEventArgs e = null)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(setGoal));
            }
            else
            {
                setGoal();
            }
        }
        void setGoal()
        {
            if (_bot != null)
            {
                the_label.Text = _bot.goal;
            }
        }
    }
}
