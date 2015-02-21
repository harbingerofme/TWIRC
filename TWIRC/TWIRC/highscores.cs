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
        public List<Label> moneyList = new List<Label>();
        public Label leaderboards = new Label();
        public Label leaderboardsType = new Label();
        public int type = 0;
        public List<intStr> data = new List<intStr>();
        System.Timers.Timer timer;
        double res;
        string[] typeText = new string[] { "Most $ earned (all time):", "Most Chat Lines:", "Most $:" };
        int defaultWidth, defaultHeight;

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
            

            res = (double)Height / (double)Width;

            BackColor = Color.Black;

            resize(Width, Height);
            leaderboards.Text = "Leaderboard";
            leaderboards.BackColor = Color.Black;
            leaderboards.ForeColor = Color.White;
            leaderboards.TextAlign = ContentAlignment.TopCenter;

            leaderboardsType.Text = typeText[type];
            leaderboardsType.BackColor = Color.Black;
            leaderboardsType.ForeColor = Color.White;
            leaderboardsType.TextAlign = ContentAlignment.TopCenter;


            Controls.Add(leaderboards);
            Controls.Add(leaderboardsType);
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



        public void resize(int w, int h)
        {
            leaderboards.Width = w-8;
            leaderboards.Height = h / 10;
            leaderboards.Location = new Point(0, h / 30);

            leaderboardsType.Width = w - 8;
            leaderboardsType.Height = h / 12;
            leaderboardsType.Location = new Point(0, h / 30 + h / 10);
            if(res >= (double) h/(double) w)//Heigth is the constraint
            {
                leaderboards.Font = new Font("Verdana", (float)(20 * ((double)h / (double)defaultHeight)));
                leaderboardsType.Font = new Font("VerdanaPOKEDOLLAR", (float)(12 * ((double)h / (double)defaultHeight)));
            }
            else//width is constraint.
            {
                leaderboards.Font = new Font("Verdana",(float)( 20 * ((double)w/(double)defaultWidth)));
                leaderboardsType.Font = new Font("VerdanaPOKEDOLLAR", (float)(12 * ((double)w / (double)defaultWidth)));
            }
        }

    }

}
