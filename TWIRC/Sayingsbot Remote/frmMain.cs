using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Sayingsbot_Remote
{
    public partial class frmMain : Form
    {
        Logger log = null;
        NetComm.Client client;

        public frmMain(Logger getALogger)
        {
            log = getALogger;
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Text = "Sayingsbot Remote " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            log.setLogControl(txtMain);
            log.addLog("Success, i guess");
            client = new NetComm.Client();
            connectClient();
            log.addLog("Started client");
            client.Connected += new NetComm.Client.ConnectedEventHandler(client_Connected);
            client.Disconnected += new NetComm.Client.DisconnectedEventHandler(client_Disconnected);
            client.DataReceived += new NetComm.Client.DataReceivedEventHandler(client_DataReceived);

        }

        void frmMain_SizeChanged(object sender, System.EventArgs e)
        {
            txtMain.Width = this.Width - 16;
            txtMain.Height = this.Height - 78;
        }

        void frmMain_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            Application.Exit();
        }

        void connectClient()
        {
            client.Connect("192.168.1.27", 8523, "Sayingsbot Remote");
        }

        private void client_DataReceived(byte[] Data, string ID)
        {
            log.addLog(ASCIIEncoding.ASCII.GetString(Data));
        }

        private void client_Disconnected()
        {
            log.addLog("Disconnected!");
        }

        private void client_Connected()
        {
            log.addLog("Connected!");
        }

        private void txtBoxSend_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter) {
                string send = txtBoxSend.Text;
                txtBoxSend.Text = "";
                client.SendData(ASCIIEncoding.ASCII.GetBytes("CHAT:"+send));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            connectClient();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            connectClient();
        }

    }
}
