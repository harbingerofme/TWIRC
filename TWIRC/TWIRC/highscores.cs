using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Timers;

namespace RNGBot
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
        string[] typeText = new string[] { "Most $ earned (all time):", "Most Chat Lines:", "Most PokéDollars:" };
        int defaultWidth, defaultHeight;
        Font labelFont;
        SQLiteConnection dbConn;

        public highscores()
        {
            Height = 280;//slightly larger since c# also counts the edges
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
            nameList[0].ForeColor = Color.Red;
            nameList[1].ForeColor = Color.Orange;
            nameList[2].ForeColor = Color.Yellow;
            nameList[3].ForeColor = Color.Green;
            nameList[4].ForeColor = Color.Green;
            dataList[0].ForeColor = Color.Red;
            dataList[1].ForeColor = Color.Orange;
            dataList[2].ForeColor = Color.Yellow;
            dataList[3].ForeColor = Color.Green;
            dataList[4].ForeColor = Color.Green;

            assignData();
            
            resize(Width,Height);

            Controls.Add(leaderboards);
            Controls.Add(leaderboardsType);
            Controls.AddRange(nameList.ToArray());
            Controls.AddRange(dataList.ToArray());
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
                if (type != 2)
                {
                    type++;
                }
                else
                {
                    type = 0;
                }
                if (type == 0)
                {
                    dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT name,alltime FROM users ORDER BY alltime,name LIMIT 7;", dbConn).ExecuteReader();
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
                    sqldr = new SQLiteCommand("SELECT name,lines FROM users WHERE lines>999 ORDER BY lines,name LIMIT 7;", dbConn).ExecuteReader();
                    while (sqldr.Read())
                    {
                        data.Add(new intStr(sqldr.GetString(0), sqldr.GetInt32(1)));
                    }
                    if (data.Count < 7)
                    {
                        type = 2;
                    }
                    dbConn.Close();
                }
                if (type == 2)
                {
                    dbConn = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
                    dbConn.Open();
                    sqldr = new SQLiteCommand("SELECT name,points FROM users ORDER BY points,name LIMIT 7;", dbConn).ExecuteReader();
                    while (sqldr.Read())
                    {
                        data.Add(new intStr(sqldr.GetString(0), sqldr.GetInt32(1)));
                    }
                    dbConn.Close();
                }
                for (int a = 0; a < 7; a++)
                {
                    nameList[a].Text = data[a].Str;
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

            int x1 = (int)((double)(w - 8) / 10);
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
                    nameList[a].Size = new Size((w - 8) / 2, h / 12);
                    dataList[a].Size = new Size(((w - 8) / 10)*3, h / 12);
                    nameList[a].Font = labelFont;
                    dataList[a].Font = labelFont;
                }
        }

    }

}
