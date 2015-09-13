using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Windows.Forms;

namespace TWIRC
{
    public class LuaServer : IDisposable
    {
       // RNGWindow mainWindow;
        Logger RNGLogger;
        Thread lsThread;

        bool _isDisposing;

        int luaPort;
        IPAddress luaAddr;

        public Dictionary<string, EmuClientHandler> RNGEmulators;

        public TcpListener serverSocket;

        public bool running = true;

        public LuaServer(Logger thislogger, Dictionary<string,EmuClientHandler> newemulatortable)
        {
            luaPort = 22222;
            luaAddr = IPAddress.Any;
            RNGLogger = thislogger;
            RNGLogger.addLog("LuaServer", 0, "Creating LuaServer thread");
            RNGEmulators = newemulatortable;
            lsThread = new Thread(LuaServerListener);
            lsThread.Name = "lsThread";
            lsThread.IsBackground = true;
        }
        public void Run()
        {
            lsThread.Start();
        }
        
        public void LuaServerListener()
        {
            serverSocket = new TcpListener(luaAddr, luaPort);
            TcpClient clientSocket = default(TcpClient);
            int counter = 0;
            bool started = false;

            while (started == false)
            {
                try
                {
                    serverSocket.Start();
                    RNGLogger.WriteLine("LuaServer Started on port " + luaPort);
                    started = true;
                }
                catch (SocketException SockEx)
                {
                    RNGLogger.WriteLine("LuaServer failed to start! ");
                    if (SockEx.ErrorCode == 10048)
                    {
                    RNGLogger.WriteLine("Probably already running on port " + luaPort);
                    }
                    else
                    {
                        RNGLogger.WriteLine(SockEx.ToString()); // something has gone horriby wrong.
                    }
                    Thread.Sleep(5000);
                }
            }
           
            while (!_isDisposing)
            {
                counter += 1;
                try
                {
                    clientSocket = serverSocket.AcceptTcpClient();
                }
                catch (SocketException sockEx)
                {
                    if (sockEx.SocketErrorCode == SocketError.Interrupted)
                    {
                        return;
                    }
                    else
                    {
                        MessageBox.Show(sockEx.ToString());
                    }
                }


                
                RNGLogger.addLog("LuaServer", 0, "Client No:" + Convert.ToString(counter) + " starting!");
                
                EmuClientHandler client = new EmuClientHandler();
                client.startClient(clientSocket, Convert.ToString(counter), RNGEmulators, RNGLogger);
            }
        }

        public void shutdown()
        {
            running = false;
            foreach (LuaServer.EmuClientHandler dyingclient in RNGEmulators.Values)
            {
                dyingclient.stopClient();
            }
        }

        public int get_client_count()
        {
            return RNGEmulators.Count;
        }

        public void send_to_all(string command, string param)
        {
            foreach (LuaServer.EmuClientHandler rngclient in RNGEmulators.Values.ToList())
            {
                try
                {
                    rngclient.sendCommand(command + ":" + param); // update all clients that a decay has happened
                }
                catch (Exception ex)
                {
                    RNGLogger.WriteLine("sendCommand failed! " + ex.Message);
                }
            }
        }

        public void Dispose()
        {
            this._isDisposing = true;
            this.serverSocket.Stop();
        }


        //Class to handle each client request separatly
        public class EmuClientHandler
        {
            public TcpClient clientSocket;
            public string emuROM;
            string clNo;
            Logger RNGLogger;
            int requestCount = 0;
            bool runs = true;
            Dictionary<string, EmuClientHandler> clientTable;

            public EmuClientHandler()
            {

            }

            public void startClient(TcpClient inClientSocket, string clientNumber, Dictionary<string,EmuClientHandler> newClientTable, Logger thisLogger)
            {
                clientTable = newClientTable;
                RNGLogger = thisLogger;
                this.clientSocket = inClientSocket;
                this.clNo = clientNumber;
                clientTable.Add(clientNumber, this);

                RNGLogger.WriteLine("added a client");
                Thread ctThread = new Thread(clientLoop);
                ctThread.Name = "ctThread" + clientNumber;
                ctThread.Start();
            }

            


            public void stopClient()
            {
                this.runs = false;

            }

            public void deadClient(Dictionary<string,EmuClientHandler> clientTable)
            {
                this.sendCommand("EXPLODE");
                this.clientSocket.Close();

                clientTable.Remove(this.clNo);
            }


            private void clientLoop()
            {

                byte[] bytesFrom = new byte[65536];
                string dataFromClient = null;
                //Byte[] sendBytes = null;
                //string serverResponse = null;
                //string rCount = null;
                requestCount = 0;



                while (runs)
                {
                    
                    try
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();

                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);

                        networkStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        RNGLogger.WriteLine("Something went wrong with the socket; killing it::");
                        RNGLogger.WriteLine(ex.Message);
                        clientTable.Remove(clNo); 
                        break;
                    }
                }

            }



  

            public void sendCommand(String command)
            {
                Byte[] sendBytes = null;
                //string serverResponse = null;
                command += "\n";

                if (clientSocket.Connected == true)
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    sendBytes = Encoding.ASCII.GetBytes(command);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                }

            }
        }
    }
}
