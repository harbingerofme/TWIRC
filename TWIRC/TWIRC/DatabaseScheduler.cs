using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading;

namespace TWIRC
{
    class DatabaseScheduler
    {
        DatabaseConnector db;
        List<task> taskList;
        Thread child;
        bool _closing = false;

        public DatabaseScheduler(DatabaseConnector _databaseConnection)
        {
            db = _databaseConnection;
            taskList = new List<task>();

            child = new Thread(workingThreadFunction);
            child.IsBackground = false;
            child.Start();
        }

        public bool closing
        {
            get { return _closing; }
            set { _closing = value; }
        }

        public int Count
        {
            get { return taskList.Count; }
        }


        public int Add(string s, object[] v = null)
        {
            return Add(db.main, s, v);
        }

        public int Add(SQLiteConnection _database, string s, object[] v = null)
        {
            taskList.Add(new task(_database, s, v));
            return taskList.Count;
        }

        void workingThreadFunction()
        {
            while (true)
            {
                while (taskList.Count > 0)
                {
                    if (taskList[0].V == null)
                    {
                        db.Execute(taskList[0].C, taskList[0].S);
                    }
                    else
                    {
                        db.safeExecute(taskList[0].C, taskList[0].S, taskList[0].V);
                    }
                    taskList.RemoveAt(0);
                    if(!closing)
                        Thread.Sleep(1000); //Sleep for a second to reduce database load;
                }
                for (int x = 0; x < 10; x++)
                {
                    Thread.Sleep(100);//list is empty, wait 10 seconds. (the reason we do intervals of 1 second is because it doesn't stall the program for 10 seconds when closing then.)
                    if(closing && taskList.Count == 0)
                    {
                        Thread.CurrentThread.Abort();
                    }
                }
            }
        }

        private class task
        {
            public SQLiteConnection C;
            public string S;
            public object[] V;

            public task(SQLiteConnection connection,string command)
            {
                C = connection;
                S = command;
                V = null;
            }

            public task(SQLiteConnection connection,string command, object[] values)
            {
                C = connection;
                S = command;
                V = values;
            }
        }
    }

    
}
