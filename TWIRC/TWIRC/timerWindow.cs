using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace TWIRC
{
    public class timerWindow : CHILDFORM
    {
        //settings
        int window_height = 117;
        int window_width = 270;
        int black_height = 55;//width is always equal to screen width

        //some declarations
        Button toggle;
        Button mainten_but;
        Button but_changeTime;
        Label timer;
        TextBox hour;
        TextBox minute;
        TextBox second;
        CheckBox cntdwn;
        Label cntdwn_lbl;
        CheckBox cntup;
        Label cntup_lbl;

        public bool running = false;//starting paused
        public bool maintenance = false;
        bool die = false;
        bool showMain = false;
        public int tenthSeconds = 0;
        public int seconds = 0;
        public int minutes = 0;
        public int hours = 0;
        public bool countdown = false;
        public bool countUpafter = false;

        //startup
        public timerWindow(int _hours = 0, int _minutes = 0, int _seconds = 0, bool _countdown = false, bool _countUpAfterwards = false)
        {
            hours = _hours;
            seconds = _seconds;
            minutes = _minutes;
            countdown = _countdown;
            countUpafter = _countUpAfterwards;

            this.Text = "Timer";
            this.StartPosition = FormStartPosition.Manual;
            this.ClientSize = new Size(window_width, window_height);

            DoubleBuffered = true;//prevents flashing

            formHandler();//draws the forms and textboxes.
            Thread trd = new Thread(this.count);
            trd.Start();
            trd.IsBackground = true;

            this.KeyPreview = true;//we apparently need this
            this.Paint += drawTimer;//binds drawtimer to the paintevent
            this.FormClosed += terminate;//make sure the thread closes as well

        }//functions below this;

        void formHandler()
        {
            toggle = new Button();
            toggle.Text = "ᐅ";
            toggle.Size = new Size(40, 18);
            toggle.Location = new Point(20, black_height + 5);
            toggle.MouseClick += toggle_animation;
            Controls.Add(toggle);

            mainten_but = new Button();
            mainten_but.Text = "M";
            mainten_but.Size = new Size(40, 18);
            mainten_but.Location = new Point(120, black_height + 5);
            mainten_but.MouseClick += maintenanceMode;
            Controls.Add(mainten_but);

            timer = new Label();
            timer.Text = "0:00:0.0";
            timer.Size = new Size(window_width - 15, black_height);//gives us some spaaaaaace from the edge
            timer.TextAlign = ContentAlignment.MiddleRight;//assures us it's always alligned properly
            timer.Location = new Point(5, 0);//gives us some spaaaaaace from the edge
            timer.ForeColor = Color.White;
            timer.BackColor = Color.Transparent;
            Controls.Add(timer);

            hour = new TextBox();
            hour.Text = "0";
            hour.Font = new Font("Arial", 8);
            hour.Location = new Point(5, black_height + 30);
            hour.Size = new Size(36, 10);
            hour.TextAlign = HorizontalAlignment.Right;
            Controls.Add(hour);

            minute = new TextBox();
            minute.Text = "0";
            minute.Font = new Font("Arial", 8);
            minute.Location = new Point(42, black_height + 30);
            minute.Size = new Size(18, 10);
            minute.TextAlign = HorizontalAlignment.Right;
            Controls.Add(minute);

            second = new TextBox();
            second.Text = "0";
            second.Font = new Font("Arial", 8);
            second.Location = new Point(61, black_height + 30);
            second.Size = new Size(18, 10);
            second.TextAlign = HorizontalAlignment.Right;
            Controls.Add(second);

            but_changeTime = new Button();
            but_changeTime.Text = "GO";
            but_changeTime.Size = new Size(30, 18);
            but_changeTime.Location = new Point(80, black_height + 30);
            but_changeTime.MouseClick += changeTime;
            Controls.Add(but_changeTime);

            cntdwn_lbl = new Label();
            cntdwn_lbl.Text = "CD?";
            cntdwn_lbl.Size = new Size(30, 20);
            cntdwn_lbl.Location = new Point(111, black_height + 33);
            Controls.Add(cntdwn_lbl);

            cntup_lbl = new Label();
            cntup_lbl.Text = "UA?";
            cntup_lbl.Size = new Size(30, 20);
            cntup_lbl.Location = new Point(153, black_height + 33);
            cntup_lbl.Visible = false;
            Controls.Add(cntup_lbl);

            cntdwn = new CheckBox();
            cntdwn.Checked = false;
            cntdwn.Location = new Point(142, black_height + 30);
            cntdwn.Size = new Size(10, 25);
            cntdwn.Click += cntdwn_click;
            Controls.Add(cntdwn);

            cntup = new CheckBox();
            cntup.Checked = false;
            cntup.Visible = false;
            cntup.Location = new Point(184, black_height + 30);
            Controls.Add(cntup);
        }


        void drawTimer(object o, PaintEventArgs pea)
        {
            SolidBrush black = new SolidBrush(Color.Black);
            pea.Graphics.FillRectangle(black, 0, 0, window_width, black_height);
            if (!maintenance || !showMain)
            {
                timer.Font = new Font("pokemon fireleaf", 30);
                string ts = "" + tenthSeconds;
                string s = "";
                if (seconds < 10) { s += "0"; }//make sure it's always 2 characters long
                s += "" + seconds;
                string m = "";
                if (minutes < 10) { m += "0"; }//make sure it's always 2 characters long
                m += "" + minutes;
                string h = "" + hours;//we don't do this for hours, as we might see 0's for a long time and nothing is preceding hours.
                timer.Text = h + ":" + m + ":" + s + "." + ts;
            }
            else//maintenance
            {
                timer.Font = new Font("pokemon fireleaf", 28);
                timer.Text = "[MAINTENANCE]";
            }
        }

        void toggle_animation(object o, EventArgs e)
        {
            if (running)
            {
                toggle.Text = "ᐅ";
                running = false;
            }
            else
            {
                toggle.Text = "X";
                running = true;
            }
            if (maintenance)
            {
                mainten_but.Text = "M";
                maintenance = false;
            }
        }

        void cntdwn_click(object o, EventArgs e)
        {
            if (cntdwn.Checked)
            {
                cntup.Visible = true;
                cntup_lbl.Visible = true;
            }
            else
            {
                cntup.Visible = false;
                cntup_lbl.Visible = false;
            }
        }

        void changeTime(object o, MouseEventArgs mea)
        {
            int a, b, c, d; bool e, f;
            try //we will do nothing, if any non correct values are entered
            {
                a = int.Parse(hour.Text);
                b = int.Parse(minute.Text);
                c = int.Parse(second.Text);
                d = 0;
                if (b < 0 || b > 59) { b = 0; }
                if (c < 0 || c > 59) { c = 0; }
                e = cntdwn.Checked;
                f = cntup.Checked;
            }
            catch
            {
                a = hours;
                b = minutes;
                c = seconds;
                d = tenthSeconds;
                e = countdown;
                f = countUpafter;
            }
            countdown = e;
            countUpafter = f;
            hours = a;
            minutes = b;
            seconds = c;
            tenthSeconds = d;
            running = false;
            Invalidate();
        }

        void maintenanceMode(object o, EventArgs e)//can be called by a click and num_0 so we use all eventargs
        {
            if (maintenance)
            {
                maintenance = false;
                mainten_but.Text = "M";
                toggle.Text = "ᐅ";
            }
            else
            {
                maintenance = true;
                running = false;
                mainten_but.Text = "P";
                toggle.Text = "M";
            }
        }


        void terminate(object o, EventArgs e)
        {
            die = true;
            running = false;
            maintenance = false;
        }

        void count()
        {
            bool a = false;
            while (!die)//we do not want the thread to close itself
            {
                if (tenthSeconds + minutes + hours + seconds == 0) { countdown = false; }
                if (!countdown)
                {
                    while (running)
                    {
                        tenthSeconds++;
                        if (tenthSeconds > 9)
                        {
                            tenthSeconds = 0;
                            seconds++;
                            if (seconds > 59)//since this condition can only happen if a second is added, we will only check it then.
                            {
                                seconds = 0;
                                minutes++;
                                if (minutes > 59)
                                {
                                    minutes = 0;
                                    hours++;
                                }
                            }
                        }
                        Invalidate();
                        Thread.Sleep(100);//sleep is measured in 1/1000 of a sec. should be fine-ish, if we assume the time needed for above calculations is near zero
                    }
                }
                else
                {
                    while (running)
                    {
                        tenthSeconds--;
                        if (tenthSeconds < 0)
                        {
                            tenthSeconds = 9;
                            seconds--;
                            if (seconds < 0)
                            {
                                seconds = 59;
                                minutes--;
                                if (minutes < 0)
                                {
                                    minutes = 59;
                                    hours--;
                                }
                            }
                        }
                        Invalidate();
                        Thread.Sleep(100);
                        if (hours + minutes + seconds + tenthSeconds == 0)
                        {
                            running = false;
                            a = true;
                        }
                    }
                }
                if (a)
                {
                    a = false;
                    if (countUpafter) { countdown = false; running = true; }
                }

                while (maintenance)
                {
                    showMain = !showMain;
                    Invalidate();
                    Thread.Sleep(1000);
                }
                Thread.Sleep(100);
            }
        }
    }
}
