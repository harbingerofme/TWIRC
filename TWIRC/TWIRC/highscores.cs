using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Timers;

namespace TWIRC
{
    class highscores : Form
    {
        public List<Label> nameList = new List<Label>();
        public List<Label> dataList = new List<Label>();
        public Label leaderboards = new Label();
        public Label leaderboardsType = new Label();
        public int type = 2;
        public List<intStr> data = new List<intStr>();
        System.Timers.Timer timer;
        double res;
        string[] typeText = new string[] { "Most $ earned (all time):", "Most Chat Lines:", "Most PokéDollars:","Most backgrounds purchased:", "Buttons pressed in last 24 hours:" };
        int defaultWidth, defaultHeight;
        Font labelFont;
        SQLiteConnection dbConn;

        public highscores()
        {
            Height = 240;//slightly larger since c# also counts the edges
            Width = 330;//see above
            defaultWidth = 330; defaultHeight = 280;
            ResizeBegin +=highscores_ResizeBegin;
            ResizeEnd +=highscores_ResizeEnd;
            Name = "Leaderboards";
            Text = "Leaderboards";

            timer = new System.Timers.Timer(300000);
            timer.Elapsed += timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();

            res = (double)Height / (double)Width;

            BackColor = Color.Black;

            leaderboards.Text = "Leaderboard";
            leaderboards.BackColor = Color.Black;
            leaderboards.ForeColor = Color.White;
            leaderboards.TextAlign = ContentAlignment.TopCenter;

            leaderboardsType.Text = typeText[type];
            leaderboardsType.BackColor = Color.Black;
            leaderboardsType.ForeColor = Color.White;
            leaderboardsType.TextAlign = ContentAlignment.TopCenter;

            Label l1,l2;
            for (int i = 0; i<7;i++ )
            {
                l1 = new Label();
                l1.TextAlign = ContentAlignment.TopCenter;
                l1.BackColor = Color.Black;
                l1.ForeColor = Color.White;
                l2  = new Label();
                l2.TextAlign = ContentAlignment.TopLeft;
                l2.BackColor = Color.Black;
                l2.ForeColor = Color.White;
                nameList.Add(l1);
                dataList.Add(l2);
            }
            //FF9900 -> FFFF00 -> 61FF00 -> 00FF21 -> 00FFBB‏
            nameList[0].ForeColor = Color.FromArgb(255,153,0);
            nameList[1].ForeColor = Color.FromArgb(255, 255, 0);
            nameList[2].ForeColor = Color.FromArgb(97, 255, 0);
            nameList[3].ForeColor = Color.FromArgb(0, 255, 33);
            nameList[4].ForeColor = Color.FromArgb(0,255,187);
            dataList[0].ForeColor = Color.FromArgb(255, 153, 0);
            dataList[1].ForeColor = Color.FromArgb(255, 255, 0);
            dataList[2].ForeColor = Color.FromArgb(97, 255, 0);
            dataList[3].ForeColor = Color.FromArgb(0, 255, 33);
            dataList[4].ForeColor = Color.FromArgb(0, 255, 187);

            assignData();
            
            resize(Width,Height);

            Controls.Add(leaderboards);
            Controls.Add(leaderboardsType);
            Controls.AddRange(nameList.ToArray());
            Controls.AddRange(dataList.ToArray());

            StartPosition = FormStartPosition.Manual;
            Location = Properties.Settings.Default.leaderboard_pos;
            Size = Properties.Settings.Default.leaderboard_size;
            resize(Size.Width, Size.Height);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            assignData();
        }

        private void highscores_ResizeBegin(object sender, EventArgs e)
        {
            leaderboardsType.Text = "Resizing in progress, stuff will look weird.";
        }

        private void highscores_ResizeEnd(object sender, EventArgs e)
        {
            leaderboardsType.Text = typeText[type];
            resize(Width, Height);
            Properties.Settings.Default.leaderboard_pos = this.Location;
            Properties.Settings.Default.leaderboard_size = this.Size;
            Properties.Settings.Default.Save();
        }

        public void assignData()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(assignData));
            }
            else
            {
                SQLiteDataReader sqldr; data = new List<intStr>();
                type++;
                if (type == typeText.Count())
                {
                    type = 0;
                }
                if (type == 3)
                {
                    dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT name, COUNT(*) FROM (SELECT name FROM transactions WHERE item = 'background') GROUP BY name ORDER BY COUNT(*) DESC LIMIT 7;", dbConn).ExecuteReader();
                    while (sqldr.Read())
                    {
                        data.Add(new intStr(sqldr.GetString(0), sqldr.GetInt32(1)));
                    }
                    if (data.Count < 7) { type = 4; data = new List<intStr>(); }
                    dbConn.Close();

                }
                if (type == 4)
                {
                    dbConn = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT * FROM buttons ORDER BY id DESC LIMIT 48;", dbConn).ExecuteReader();
                    int a = 0; List<intStr> temp = new intStr[7].ToList();
                    for (int b = 0; b < 7; b++)
                    {
                        temp[b] = new intStr();
                        temp[b].Int = 0;
                    }
                    temp[0].Str = "left"; temp[1].Str = "down"; temp[2].Str = "up"; temp[3].Str = "right"; temp[4].Str = "A"; temp[5].Str = "B"; temp[6].Str = "start";
                        while (sqldr.Read())
                        {
                            a++;
                            for(int b = 0;b<7;b++)
                            {
                                temp[b].Int += sqldr.GetInt32(b + 1);
                            }
                        }
                    if (a < 48)
                    {
                        type = 0;
                        data = new List<intStr>();
                    }
                    else
                    {
                        int highest; int id = -1;
                        while(temp.Count>0)
                        {
                            highest = -1; 
                            for (int b = 0; b < temp.Count; b++)
                            {
                                if (temp[b].Int > highest)
                                {
                                    id = b;
                                    highest = temp[b].Int;
                                }
                            }
                            data.Add(temp[id]);
                            temp.RemoveAt(id);
                        }
                    }
                }
                if (type == 0)
                {
                    dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT name,alltime FROM users ORDER BY alltime DESC,name LIMIT 7;", dbConn).ExecuteReader();
                    while (sqldr.Read())
                    {
                        data.Add(new intStr(sqldr.GetString(0), sqldr.GetInt32(1)));
                    }
                    dbConn.Close();
                }

                if (type == 1)
                {
                    dbConn = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT name,lines FROM users WHERE lines>749 AND not name like '%bot ORDER BY lines DESC,name LIMIT 7;", dbConn).ExecuteReader();
                    while (sqldr.Read())
                    {
                        data.Add(new intStr(sqldr.GetString(0), sqldr.GetInt32(1)));
                    }
                    if (data.Count < 7)
                    {
                        type = 2;
                        data = new List<intStr>();
                    }
                    dbConn.Close();
                }
                if (type == 2)
                {
                    dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT name,points FROM users ORDER BY points DESC,name LIMIT 7;", dbConn).ExecuteReader();
                    while (sqldr.Read())
                    {
                        data.Add(new intStr(sqldr.GetString(0), sqldr.GetInt32(1)));
                    }
                    dbConn.Close();
                }
                while (data.Count < 7)
                {
                    data.Add(new intStr("undefined", -1));
                }
                string name;
                for (int a = 0; a < 7; a++)
                {
                    name = data[a].Str;
                    if (name.Length > 1)
                    {
                        name = name.Substring(0, 1).ToUpper()+ name.Substring(1);
                    }
                    else
                    {
                        name = name.ToUpper();
                    }
                    nameList[a].Text = name;
                    if (name.Length > 15)
                    {
                        name = name.Substring(0, 12) + "...";
                    }
                    dataList[a].Text = "";
                    if (type == 0 || type == 2) { dataList[a].Text += "$"; }
                    dataList[a].Text += data[a].Int;
                }
                leaderboardsType.Text = typeText[type];
            }
        }

        public void resize(int w, int h)
        {
            leaderboards.Width = w-8;
            leaderboards.Height = h / 10;
            leaderboards.Location = new Point(0, h / 30);

            leaderboardsType.Width = w - 8;
            leaderboardsType.Height = h / 12;
            leaderboardsType.Location = new Point(0, h / 30 + h / 10);

            int x1 = (int)((double)(w - 8) / 20);
            int x2 = (int)(double)((w - 8) / 10) * 6;
            int y = (int)((h / 30 + h / 10+ h/12));

                if (res >= (double)h / (double)w)//Heigth is the constraint
                {
                    leaderboards.Font = new Font("Verdana", (float)(20 * ((double)h / (double)defaultHeight)));
                    leaderboardsType.Font = new Font("VerdanaPOKEDOLLAR", (float)(12 * ((double)h / (double)defaultHeight)));
                    labelFont = new Font("VerdanaPOKEDOLLAR", (float)(13 * ((double)h / (double)defaultHeight)));
                }
                else//width is constraint.
                {
                    leaderboards.Font = new Font("Verdana", (float)(20 * ((double)w / (double)defaultWidth)));
                    leaderboardsType.Font = new Font("VerdanaPOKEDOLLAR", (float)(12 * ((double)w / (double)defaultWidth)));
                    labelFont = new Font("VerdanaPOKEDOLLAR", (float)(13 * ((double)w / (double)defaultWidth)));
                }
                for (int a = 0; a < 7; a++)
                {
                    nameList[a].Location = new Point(x1, y + (a * (h / 13)));
                    dataList[a].Location = new Point(x2, y + (a * (h / 13)));
                    nameList[a].Size = new Size((w - 8) / 2+(w-8)/20, h / 12);
                    dataList[a].Size = new Size(((w - 8) / 10)*3, h / 12);
                    nameList[a].Font = labelFont;
                    dataList[a].Font = labelFont;
                }
        }

    }

}
