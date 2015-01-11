using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Windows.Forms;

namespace RNGBot
{
    public class LuaServer
    {
       // RNGWindow mainWindow;
        Logger RNGLogger;
        Thread lsThread;

        int luaPort;
        IPAddress luaAddr;

        public Dictionary<string, EmuClientHandler> RNGEmulators;

        public TcpListener serverSocket;

        public bool running = true;

        public LuaServer(Logger thislogger, Dictionary<string,EmuClientHandler> newemulatortable)
        {
            luaPort = 22222;
            luaAddr = IPAddress.Any;//.Parse("127.0.0.1");
            RNGLogger = thislogger;
            //mainWindow = thiswindow;
            RNGLogger.addLog("LuaServer", 0, "Creating LuaServer thread");
            RNGEmulators = newemulatortable;
            lsThread = new Thread(LuaServerListener);
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
           
            while (running == true)
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
        

        //Class to handle each client request separatly
        public class EmuClientHandler
        {
            public TcpClient clientSocket;
            public string emuROM;
            string clNo;
            Logger RNGLogger;
            int requestCount = 0;
            bool runs = true;

            public EmuClientHandler()
            {

            }

            public void startClient(TcpClient inClientSocket, string clineNo, Dictionary<string,EmuClientHandler> clientTable, Logger thisLogger)
            {
                RNGLogger = thisLogger;
                this.clientSocket = inClientSocket;
                this.clNo = clineNo;
                clientTable.Add(clineNo, this);

                RNGLogger.WriteLine("added a client");
                Thread ctThread = new Thread(clientLoop);
                ctThread.Start();
            }

            


            public void stopClient()
            {
                this.runs = false;

            }

            public void deadClient(Dictionary<string,EmuClientHandler> clientTable)
            {
                this.sendCommand(999);
                this.clientSocket.Close();

                clientTable.Remove(this.clNo);
            }


            private void clientLoop()
            {

                byte[] bytesFrom = new byte[65536];
                string dataFromClient = null;
                Byte[] sendBytes = null;
                string serverResponse = null;
                string rCount = null;
                requestCount = 0;



                while (runs)
                {
                    
                    try
                    {
                        requestCount = requestCount + 1;
                        NetworkStream networkStream = clientSocket.GetStream();

                        networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                        dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                        //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                        //RNGLogger.addLine(" >> " + "From client-" + dataFromClient);
                        //RNGLogger.setStatusText(dataFromClient);
                        rCount = Convert.ToString(requestCount);
                        //serverResponse = "STROBE:" + rCount + "\n";
                        //sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                        //networkStream.Write(sendBytes, 0, sendBytes.Length);
                        networkStream.Flush();
                        //Thread.Sleep(100);
                        //RNGLogger.addLine(" >> " + serverResponse); // overly verbose.
                    }
                    catch (Exception ex)
                    {
                        RNGLogger.WriteLine("Something went wrong with the socket; killing it::");
                        RNGLogger.WriteLine(ex.Message);
                        break;
                    }
                }

            }



  

            public void sendCommand(int command)
            {
                Byte[] sendBytes = null;
                string serverResponse = null;

                if (clientSocket.Connected == true)
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    serverResponse = "COMMAND:" + Convert.ToString(command) + "\n";
                    sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.Flush();
                }

            }
        }
    }
}
