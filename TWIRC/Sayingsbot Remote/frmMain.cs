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
        NotifyIcon notification = new NotifyIcon();

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
            notification.Text = this.Text;
            notification.Icon = this.Icon;
            notification.Visible = true;
            notification.Click += notification_Click;
            notification.MouseDoubleClick += notification_MouseDoubleClick;
            notification.BalloonTipClicked += notification_Click;

        }

        void notification_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        void notification_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Focus();
        }

        void frmMain_SizeChanged(object sender, System.EventArgs e)
        {
            txtMain.Width = this.Width - 16;
            txtMain.Height = this.Height - 78;

            if (FormWindowState.Minimized == this.WindowState)
            {
                notification.ShowBalloonTip(2500, "Sayingsbot Remote", "Minimized to tray.", ToolTipIcon.None);
                this.Hide();
            }
        }

        void frmMain_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            notification.Visible = false;
        }

        void frmMain_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            client.Disconnect();
            Application.Exit();
        }

        void connectClient()
        {
#if DEBUG
            client.Connect("localhost", 8524, "Sayingsbot Remote");
#else
            client.Connect("192.168.1.14", 8523, "Sayingsbot Remote");
#endif
        }

        private void client_DataReceived(byte[] Data, string ID)
        {
            string tmp = ASCIIEncoding.ASCII.GetString(Data);
            if (tmp.StartsWith("NOTIFY:"))
            {
                tmp = tmp.Remove(0, 7);
                notification.ShowBalloonTip(5000, "Sayingsbot Remote", tmp, ToolTipIcon.Info);
            }
            else { log.addLog(tmp); }
            
        }

        private void client_Disconnected()
        {
            log.addLog("Disconnected!");
        }

        private void client_Connected()
        {
            log.addLog("Connected!");
            notification.ShowBalloonTip(2500, "Sayingsbot Remote", "Connected!", ToolTipIcon.Info);
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

    }
}
