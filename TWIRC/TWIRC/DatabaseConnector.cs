using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace TWIRC
{
    class DatabaseConnector
    {
        public SQLiteConnection main,chat,buttons;
        private Logger log;
        private string name;

        public DatabaseConnector(Logger Log)
        {
            main = new SQLiteConnection("Data Source=db.sqlite;Version=3;");
            main.Open();
            chat = new SQLiteConnection("Data Source=chat.sqlite;Version=3;");
            chat.Open();
            buttons = new SQLiteConnection("Data Source=buttons.sqlite;Version=3;");
            buttons.Open();

            log = Log;
            name = "DBCONN";
        }

        public SQLiteCommand Command(SQLiteConnection conn, string text)
        {
            return new SQLiteCommand(text, conn);
        }

        public bool Execute(SQLiteConnection conn, string text)
        {
            try
            {
                Command(conn, text).ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                log.addLog(name,0,"ERROR: "+e.Message);
                return false;
            }
        }

        public bool Execute(string text)
        {
            return Execute(main, text);
        }

        public SQLiteDataReader Reader(SQLiteConnection conn, string text)
        {
            try
            {
                return Command(conn, text).ExecuteReader();
            }
            catch
            {
                return null;
            }
        }

        public List<object[]> Read(SQLiteConnection conn, string text)
        {
            SQLiteDataReader sqldr = Reader(conn, text);
            List<object[]> list = new List<object[]>();
            if(sqldr == null)
            {
                list.Add(new object[] {-1});
                return list;
            }
            
            while(sqldr.Read())
            {
                object[] obj = new object[sqldr.FieldCount];
                for(int i = 0; i< sqldr.FieldCount; i++)
                {
                    obj[i] = sqldr.GetValue(i);
                }
                list.Add(obj);
            }
            return list;
        }
    }
}
