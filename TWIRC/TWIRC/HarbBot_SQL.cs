using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;

namespace TWIRC
{
    public partial class HarbBot //contains various sql methods
    {
        public void loadBiases()
        {
            List<Bias> biases = new List<Bias>();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM biases;",dbConn).ExecuteReader();
            while(sqldr.Read())
            {
                foreach(string s in sqldr.GetString(0).Trim().Split(' '))
                {
                    if(s.Length>=1)
                    biases.Add(new Bias(s, sqldr.GetString(1)));
                }
            }
            biasList = biases;
        }

        public bool addBias(string keyword, string numbers)
        {
            SQLiteDataReader sql  = new SQLiteCommand("SELECT * FROM biases WHERE keyword LIKE '% "+keyword +" %';",dbConn).ExecuteReader();
            if(sql.Read()){
                return false;
            }
            else
            {
                sql = new SQLiteCommand("SELECT keyword FROM biases WHERE numbers  = '"+ numbers +"';",dbConn).ExecuteReader();
                if (sql.Read())
                {
                    string str = sql.GetString(0) +" "+ keyword + " ";
                    new SQLiteCommand("UPDATE biases SET keyword = '" + str + "' WHERE numbers  = '" + numbers + "';",dbConn).ExecuteNonQuery();
                }
                else
                {
                    new SQLiteCommand("INSERT INTO biases (keyword,numbers) VALUES (' " + keyword + " ', '" + numbers + "');",dbConn).ExecuteNonQuery();
                }
                return true;
               
            }
        }

        public bool delBias(string keyword)
        {
            SQLiteDataReader sql = new SQLiteCommand("SELECT keyword,numbers FROM biases WHERE keywords LIKE '% "+keyword+" %';",dbConn).ExecuteReader();
            if (sql.Read())
            {
                string numbers = sql.GetString(1);
                string oldAli = sql.GetString(0).Trim();
                string[] split = oldAli.Split(' ');
                string newAli = " ";
                foreach (string str in split)
                {
                    if(str != keyword)
                    {
                        newAli += str + " ";
                    }
                }
                if(newAli.Length > 1)
                {
                    new SQLiteCommand("UPDATE biases SET keyword = '"+newAli+"' WHERE numbers = '"+numbers+"';", dbConn).ExecuteNonQuery();
                }
                else
                {
                    new SQLiteCommand("DELETE FROM biases WHERE numbers = '" + numbers + "';", dbConn).ExecuteNonQuery();
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        public void notNew(string user)
        {
            new SQLiteCommand("UPDATE users SET isnew = 0 WHERE name = '" + user + "';", dbConn).ExecuteNonQuery();
        }

        public int getNow()
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = DateTime.Now.ToUniversalTime() - origin;
            return (int)Math.Floor(diff.TotalSeconds);
        }

        public string getNowSQL()
        {
            string str = DateTime.Now.Year.ToString();
            if (DateTime.Now.DayOfYear < 100) { str += "0"; }
            if (DateTime.Now.DayOfYear < 10) { str += "0"; }
            str += DateTime.Now.DayOfYear.ToString();
            //(int)DateTime.Now.TimeOfDay.TotalSeconds;
            return str;
        }
        public string getNowExtended()
        {
            string str = DateTime.Now.Year.ToString();
            if (DateTime.Now.DayOfYear < 100) { str += "0"; }
            if (DateTime.Now.DayOfYear < 10) { str += "0"; }
            str += DateTime.Now.DayOfYear.ToString();
            if (DateTime.Now.Hour < 10) { str += "0"; }
            str += DateTime.Now.Hour;
            if (DateTime.Now.Minute < 10) { str += "0"; }
            str += DateTime.Now.Minute;
            if (DateTime.Now.Second < 10) { str += "0"; }
            str += DateTime.Now.Second;
            return str;
        }

        public int pullAuth(string name)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT rank FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        public void setAuth(string user, int level)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET rank='" + level + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,rank) VALUES ('" + user + "','" + getNowSQL() + "','" + level + "');", dbConn).ExecuteNonQuery();
            }
        }
        public int getPoints(string name)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }
        public void setPoints(string user, int amount)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET points='" + amount + "' WHERE name='" + user + "';", dbConn).ExecuteNonQuery();
                new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + user + "','" + amount + "','FORCED CHANGE TO AMOUNT','" + sqldr.GetInt32(0) + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + user + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
            }
        }
        public int addPoints(string name, int amount, string why)
        {
            if (amount != 0)
            {
                int things;
                SQLiteDataReader sqldr = new SQLiteCommand("SELECT points FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
                if (sqldr.Read())
                {
                    things = sqldr.GetInt32(0);
                    new SQLiteCommand("UPDATE users SET points='" + (things + amount) + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                    new SQLiteCommand("INSERT INTO transactions (name,amount,item,prevmoney,date) VALUES ('" + name + "','" + amount + "','" + why + "','" + things + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                    return things;
                }
                else
                {
                    new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
                    return 0;
                }
            }
            return 0;
        }

        public bool isNew(string user)
        {
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT isnew FROM users WHERE name='" + user + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                int a = sqldr.GetInt32(0);
                if (sqldr.GetInt32(0) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public int addAllTime(string name, int amount)
        {
            int things, rank;
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime, rank FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                things = sqldr.GetInt32(0);
                rank = sqldr.GetInt32(1);
                if (rank == 0 && things + amount > 2500) { setAuth(name, 1); }
                new SQLiteCommand("UPDATE users SET alltime=alltime+" + amount + " WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return things;
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name,lastseen,points) VALUES ('" + name + "','" + getNowSQL() + "','" + amount + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }

        public int getAllTime(string name)
        {
            name = name.ToLower();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT alltime FROM users WHERE name='" + name + "';", dbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lastseen='" + getNowSQL() + "' WHERE name='" + name + "';", dbConn).ExecuteNonQuery();
                return sqldr.GetInt32(0);
            }
            else
            {

                new SQLiteCommand("INSERT INTO users (name,lastseen) VALUES ('" + name + "','" + getNowSQL() + "');", dbConn).ExecuteNonQuery();
                return 0;
            }
        }

        public void storeMessage(string user, string message)
        {
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO messages (name,message,time) VALUES ('" + user + "',@par1," + getNowExtended() + ");", chatDbConn);
            cmd.Parameters.AddWithValue("@par1", message); cmd.ExecuteNonQuery();
            SQLiteDataReader sqldr = new SQLiteCommand("SELECT * FROM users WHERE name= '" + user + "';", chatDbConn).ExecuteReader();
            if (sqldr.Read())
            {
                new SQLiteCommand("UPDATE users SET lines = lines+1 WHERE name = '" + user + "';", chatDbConn).ExecuteNonQuery();
            }
            else
            {
                new SQLiteCommand("INSERT INTO users (name) VALUES ('" + user + "');", chatDbConn).ExecuteNonQuery();
            }
        }
    }
}
