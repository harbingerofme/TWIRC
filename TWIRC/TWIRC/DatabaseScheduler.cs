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

        public DatabaseScheduler(DatabaseConnector _databaseConnection)
        {
            db = _databaseConnection;
            taskList = new List<task>();

            child = new Thread(workingThreadFunction);
            child.IsBackground = true;
            child.Start();
        }

        public int Count
        {
            get { return taskList.Count; }
        }


        public int Add(string s)
        {
            return Add(db.main, s);
        }

        public int Add(SQLiteConnection _database, string s)
        {
            taskList.Add(new task(_database, s));
            return taskList.Count;
        }

        void workingThreadFunction()
        {
            while (true)
            {
                while (taskList.Count > 0)
                {
                    db.Execute(taskList[0].C, taskList[0].S);
                    taskList.RemoveAt(0);
                    Thread.Sleep(1000); //Sleep for a second to reduce database load;
                }
                for (int x = 0; x < 10;x++ )
                    Thread.Sleep(1000);//list is empty, wait 10 seconds. (the reason we do intervals of 1 second is because it doesn't stall the program for 10 seconds when closing then.)
            }
        }

        private class task
        {
            public SQLiteConnection C;
            public string S;

            public task(SQLiteConnection _connection,string _command)
            {
                C = _connection;
                S = _command;
            }
        }
    }

    
}
