using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

namespace RNGBot
{
    class highscores : Form
    {
        public List<Label> nameList = new List<Label>();
        public List<Label> moneyList = new List<Label>();
        public highscores()
        {
            Height = 300;//slightly larger since c# also counts the edges
            Width = 330;//see above

            BackColor = Color.Black;


        }
    }
}
