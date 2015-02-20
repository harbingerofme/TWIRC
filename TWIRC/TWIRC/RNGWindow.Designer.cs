namespace RNGBot
{
    partial class RNGWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_KillClients = new System.Windows.Forms.Button();
            this.btn_RNGesus = new System.Windows.Forms.Button();
            this.btn_RestartIRC = new System.Windows.Forms.Button();
            this.btn_Decay = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ts_isconnected = new System.Windows.Forms.ToolStripStatusLabel();
            this.ts_counter0 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ts_rngesus = new System.Windows.Forms.ToolStripStatusLabel();
            this.txt_Command = new System.Windows.Forms.TextBox();
            this.text_log = new System.Windows.Forms.TextBox();
            this.timer_RNG = new System.Windows.Forms.Timer(this.components);
            this.txt_RNGInterval = new System.Windows.Forms.TextBox();
            this.btn_ClearLog = new System.Windows.Forms.Button();
            this.btn_DumpLog = new System.Windows.Forms.Button();
            this.timer_decay = new System.Windows.Forms.Timer(this.components);
            this.timer_RNG_bias = new System.Windows.Forms.Timer(this.components);
            this.btn_DownLeft = new System.Windows.Forms.Button();
            this.btn_Down = new System.Windows.Forms.Button();
            this.btn_DownRight = new System.Windows.Forms.Button();
            this.btn_Left = new System.Windows.Forms.Button();
            this.btn_Neutral = new System.Windows.Forms.Button();
            this.btn_Right = new System.Windows.Forms.Button();
            this.btn_UpLeft = new System.Windows.Forms.Button();
            this.btn_Up = new System.Windows.Forms.Button();
            this.btn_UpRight = new System.Windows.Forms.Button();
            this.txt_Parameter = new System.Windows.Forms.TextBox();
            this.txt_IRCManual = new System.Windows.Forms.TextBox();
            this.timer_interface_stats = new System.Windows.Forms.Timer(this.components);
            this.btn_Save = new System.Windows.Forms.Button();
            this.txt_Halp = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_KillClients
            // 
            this.btn_KillClients.Location = new System.Drawing.Point(0, 271);
            this.btn_KillClients.Name = "btn_KillClients";
            this.btn_KillClients.Size = new System.Drawing.Size(64, 24);
            this.btn_KillClients.TabIndex = 0;
            this.btn_KillClients.Text = "Kill Clients";
            this.btn_KillClients.UseVisualStyleBackColor = true;
            this.btn_KillClients.Click += new System.EventHandler(this.btn_KillClients_Click);
            // 
            // btn_RNGesus
            // 
            this.btn_RNGesus.Location = new System.Drawing.Point(70, 271);
            this.btn_RNGesus.Name = "btn_RNGesus";
            this.btn_RNGesus.Size = new System.Drawing.Size(64, 24);
            this.btn_RNGesus.TabIndex = 1;
            this.btn_RNGesus.Text = "RNGesus";
            this.btn_RNGesus.UseVisualStyleBackColor = true;
            this.btn_RNGesus.Click += new System.EventHandler(this.btn_RNGesus_Click);
            // 
            // btn_RestartIRC
            // 
            this.btn_RestartIRC.Location = new System.Drawing.Point(186, 271);
            this.btn_RestartIRC.Name = "btn_RestartIRC";
            this.btn_RestartIRC.Size = new System.Drawing.Size(81, 24);
            this.btn_RestartIRC.TabIndex = 2;
            this.btn_RestartIRC.Text = "Restart IRC";
            this.btn_RestartIRC.UseVisualStyleBackColor = true;
            this.btn_RestartIRC.Click += new System.EventHandler(this.btn_RestartIRC_Click);
            // 
            // btn_Decay
            // 
            this.btn_Decay.Location = new System.Drawing.Point(273, 271);
            this.btn_Decay.Name = "btn_Decay";
            this.btn_Decay.Size = new System.Drawing.Size(47, 24);
            this.btn_Decay.TabIndex = 3;
            this.btn_Decay.Text = "Decay";
            this.btn_Decay.UseVisualStyleBackColor = true;
            this.btn_Decay.Click += new System.EventHandler(this.btn_Decay_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ts_isconnected,
            this.ts_counter0,
            this.ts_rngesus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 388);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(415, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // ts_isconnected
            // 
            this.ts_isconnected.AutoSize = false;
            this.ts_isconnected.Name = "ts_isconnected";
            this.ts_isconnected.Size = new System.Drawing.Size(90, 17);
            this.ts_isconnected.Text = "ts_isconnected";
            // 
            // ts_counter0
            // 
            this.ts_counter0.AutoSize = false;
            this.ts_counter0.Name = "ts_counter0";
            this.ts_counter0.Size = new System.Drawing.Size(110, 17);
            this.ts_counter0.Text = "ts_counter0";
            this.ts_counter0.Click += new System.EventHandler(this.ts_counter0_Click);
            // 
            // ts_rngesus
            // 
            this.ts_rngesus.AutoSize = false;
            this.ts_rngesus.Name = "ts_rngesus";
            this.ts_rngesus.Size = new System.Drawing.Size(62, 17);
            this.ts_rngesus.Text = "ts_rngesus";
            // 
            // txt_Command
            // 
            this.txt_Command.Dock = System.Windows.Forms.DockStyle.Left;
            this.txt_Command.Location = new System.Drawing.Point(0, 228);
            this.txt_Command.Name = "txt_Command";
            this.txt_Command.Size = new System.Drawing.Size(134, 20);
            this.txt_Command.TabIndex = 15;
            this.txt_Command.Text = "COMMAND";
            this.txt_Command.TextChanged += new System.EventHandler(this.txt_Command_TextChanged);
            this.txt_Command.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_Command_KeyDown);
            // 
            // text_log
            // 
            this.text_log.BackColor = System.Drawing.Color.Gainsboro;
            this.text_log.Dock = System.Windows.Forms.DockStyle.Top;
            this.text_log.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_log.ForeColor = System.Drawing.Color.Black;
            this.text_log.Location = new System.Drawing.Point(0, 0);
            this.text_log.Multiline = true;
            this.text_log.Name = "text_log";
            this.text_log.ReadOnly = true;
            this.text_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.text_log.Size = new System.Drawing.Size(415, 228);
            this.text_log.TabIndex = 16;
            this.text_log.WordWrap = false;
            this.text_log.TextChanged += new System.EventHandler(this.text_log_TextChanged);
            // 
            // timer_RNG
            // 
            this.timer_RNG.Interval = 40;
            this.timer_RNG.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txt_RNGInterval
            // 
            this.txt_RNGInterval.Location = new System.Drawing.Point(140, 271);
            this.txt_RNGInterval.Name = "txt_RNGInterval";
            this.txt_RNGInterval.Size = new System.Drawing.Size(40, 20);
            this.txt_RNGInterval.TabIndex = 17;
            this.txt_RNGInterval.Text = "40";
            this.txt_RNGInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_RNGInterval_KeyDown);
            // 
            // btn_ClearLog
            // 
            this.btn_ClearLog.Location = new System.Drawing.Point(368, 300);
            this.btn_ClearLog.Name = "btn_ClearLog";
            this.btn_ClearLog.Size = new System.Drawing.Size(46, 24);
            this.btn_ClearLog.TabIndex = 18;
            this.btn_ClearLog.Text = "Clear";
            this.btn_ClearLog.UseVisualStyleBackColor = true;
            this.btn_ClearLog.Click += new System.EventHandler(this.btn_ClearLog_Click);
            // 
            // btn_DumpLog
            // 
            this.btn_DumpLog.Location = new System.Drawing.Point(326, 271);
            this.btn_DumpLog.Name = "btn_DumpLog";
            this.btn_DumpLog.Size = new System.Drawing.Size(88, 23);
            this.btn_DumpLog.TabIndex = 19;
            this.btn_DumpLog.Text = "DumpLog";
            this.btn_DumpLog.UseVisualStyleBackColor = true;
            this.btn_DumpLog.Click += new System.EventHandler(this.btn_DumpLog_Click);
            // 
            // timer_decay
            // 
            this.timer_decay.Interval = 60000;
            this.timer_decay.Tick += new System.EventHandler(this.timer_decay_Tick);
            // 
            // timer_RNG_bias
            // 
            this.timer_RNG_bias.Interval = 60000;
            this.timer_RNG_bias.Tick += new System.EventHandler(this.timer_RNG_bias_Tick);
            // 
            // btn_DownLeft
            // 
            this.btn_DownLeft.Location = new System.Drawing.Point(81, 361);
            this.btn_DownLeft.Name = "btn_DownLeft";
            this.btn_DownLeft.Size = new System.Drawing.Size(75, 23);
            this.btn_DownLeft.TabIndex = 20;
            this.btn_DownLeft.Text = "DN-LEFT";
            this.btn_DownLeft.UseVisualStyleBackColor = true;
            this.btn_DownLeft.Click += new System.EventHandler(this.btn_DownLeft_Click);
            // 
            // btn_Down
            // 
            this.btn_Down.Location = new System.Drawing.Point(162, 361);
            this.btn_Down.Name = "btn_Down";
            this.btn_Down.Size = new System.Drawing.Size(75, 23);
            this.btn_Down.TabIndex = 21;
            this.btn_Down.Text = "DOWN";
            this.btn_Down.UseVisualStyleBackColor = true;
            this.btn_Down.Click += new System.EventHandler(this.btn_Down_Click);
            // 
            // btn_DownRight
            // 
            this.btn_DownRight.Location = new System.Drawing.Point(243, 361);
            this.btn_DownRight.Name = "btn_DownRight";
            this.btn_DownRight.Size = new System.Drawing.Size(75, 23);
            this.btn_DownRight.TabIndex = 22;
            this.btn_DownRight.Text = "DN-RIGHT";
            this.btn_DownRight.UseVisualStyleBackColor = true;
            this.btn_DownRight.Click += new System.EventHandler(this.btn_DownRight_Click);
            // 
            // btn_Left
            // 
            this.btn_Left.Location = new System.Drawing.Point(81, 332);
            this.btn_Left.Name = "btn_Left";
            this.btn_Left.Size = new System.Drawing.Size(75, 23);
            this.btn_Left.TabIndex = 23;
            this.btn_Left.Text = "LEFT";
            this.btn_Left.UseVisualStyleBackColor = true;
            this.btn_Left.Click += new System.EventHandler(this.btn_Left_Click);
            // 
            // btn_Neutral
            // 
            this.btn_Neutral.Location = new System.Drawing.Point(162, 332);
            this.btn_Neutral.Name = "btn_Neutral";
            this.btn_Neutral.Size = new System.Drawing.Size(75, 23);
            this.btn_Neutral.TabIndex = 24;
            this.btn_Neutral.Text = "NEUTRAL";
            this.btn_Neutral.UseVisualStyleBackColor = true;
            this.btn_Neutral.Click += new System.EventHandler(this.btn_Neutral_Click);
            // 
            // btn_Right
            // 
            this.btn_Right.Location = new System.Drawing.Point(243, 332);
            this.btn_Right.Name = "btn_Right";
            this.btn_Right.Size = new System.Drawing.Size(75, 23);
            this.btn_Right.TabIndex = 25;
            this.btn_Right.Text = "RIGHT";
            this.btn_Right.UseVisualStyleBackColor = true;
            this.btn_Right.Click += new System.EventHandler(this.btn_Right_Click);
            // 
            // btn_UpLeft
            // 
            this.btn_UpLeft.Location = new System.Drawing.Point(81, 301);
            this.btn_UpLeft.Name = "btn_UpLeft";
            this.btn_UpLeft.Size = new System.Drawing.Size(75, 23);
            this.btn_UpLeft.TabIndex = 26;
            this.btn_UpLeft.Text = "UP-LEFT";
            this.btn_UpLeft.UseVisualStyleBackColor = true;
            this.btn_UpLeft.Click += new System.EventHandler(this.btn_UpLeft_Click);
            // 
            // btn_Up
            // 
            this.btn_Up.Location = new System.Drawing.Point(162, 303);
            this.btn_Up.Name = "btn_Up";
            this.btn_Up.Size = new System.Drawing.Size(75, 23);
            this.btn_Up.TabIndex = 27;
            this.btn_Up.Text = "UP";
            this.btn_Up.UseVisualStyleBackColor = true;
            this.btn_Up.Click += new System.EventHandler(this.btn_Up_Click);
            // 
            // btn_UpRight
            // 
            this.btn_UpRight.Location = new System.Drawing.Point(243, 303);
            this.btn_UpRight.Name = "btn_UpRight";
            this.btn_UpRight.Size = new System.Drawing.Size(75, 23);
            this.btn_UpRight.TabIndex = 28;
            this.btn_UpRight.Text = "UP-RIGHT";
            this.btn_UpRight.UseVisualStyleBackColor = true;
            this.btn_UpRight.Click += new System.EventHandler(this.btn_UpRight_Click);
            // 
            // txt_Parameter
            // 
            this.txt_Parameter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Parameter.Location = new System.Drawing.Point(134, 228);
            this.txt_Parameter.Name = "txt_Parameter";
            this.txt_Parameter.Size = new System.Drawing.Size(281, 20);
            this.txt_Parameter.TabIndex = 29;
            this.txt_Parameter.Text = "0";
            this.txt_Parameter.TextChanged += new System.EventHandler(this.txt_Parameter_TextChanged);
            this.txt_Parameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_Parameter_KeyDown);
            // 
            // txt_IRCManual
            // 
            this.txt_IRCManual.Location = new System.Drawing.Point(0, 249);
            this.txt_IRCManual.Name = "txt_IRCManual";
            this.txt_IRCManual.Size = new System.Drawing.Size(414, 20);
            this.txt_IRCManual.TabIndex = 30;
            this.txt_IRCManual.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_IRCManual_KeyDown);
            // 
            // timer_interface_stats
            // 
            this.timer_interface_stats.Enabled = true;
            this.timer_interface_stats.Tick += new System.EventHandler(this.timer_interface_stats_Tick);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(0, 361);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(64, 23);
            this.btn_Save.TabIndex = 31;
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // txt_Halp
            // 
            this.txt_Halp.Location = new System.Drawing.Point(352, 361);
            this.txt_Halp.Name = "txt_Halp";
            this.txt_Halp.Size = new System.Drawing.Size(62, 23);
            this.txt_Halp.TabIndex = 32;
            this.txt_Halp.Text = "Halp";
            this.txt_Halp.UseVisualStyleBackColor = true;
            this.txt_Halp.Click += new System.EventHandler(this.txt_Halp_Click);
            // 
            // RNGWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 410);
            this.Controls.Add(this.txt_Halp);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.txt_IRCManual);
            this.Controls.Add(this.txt_Parameter);
            this.Controls.Add(this.btn_UpRight);
            this.Controls.Add(this.btn_Up);
            this.Controls.Add(this.btn_UpLeft);
            this.Controls.Add(this.btn_Right);
            this.Controls.Add(this.btn_Neutral);
            this.Controls.Add(this.btn_Left);
            this.Controls.Add(this.btn_DownRight);
            this.Controls.Add(this.btn_Down);
            this.Controls.Add(this.btn_DownLeft);
            this.Controls.Add(this.btn_DumpLog);
            this.Controls.Add(this.btn_ClearLog);
            this.Controls.Add(this.txt_RNGInterval);
            this.Controls.Add(this.txt_Command);
            this.Controls.Add(this.text_log);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btn_Decay);
            this.Controls.Add(this.btn_RestartIRC);
            this.Controls.Add(this.btn_RNGesus);
            this.Controls.Add(this.btn_KillClients);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "RNGWindow";
            this.Text = "RNG Bot";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RNGWindow_FormClosed);
            this.Load += new System.EventHandler(this.RNGWindow_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_KillClients;
        private System.Windows.Forms.Button btn_RNGesus;
        private System.Windows.Forms.Button btn_RestartIRC;
        private System.Windows.Forms.Button btn_Decay;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel ts_isconnected;
        private System.Windows.Forms.TextBox txt_Command;
        private System.Windows.Forms.TextBox text_log;
        private System.Windows.Forms.ToolStripStatusLabel ts_counter0;
        private System.Windows.Forms.Timer timer_RNG;
        private System.Windows.Forms.TextBox txt_RNGInterval;
        private System.Windows.Forms.ToolStripStatusLabel ts_rngesus;
        private System.Windows.Forms.Button btn_ClearLog;
        private System.Windows.Forms.Button btn_DumpLog;
        private System.Windows.Forms.Timer timer_decay;
        private System.Windows.Forms.Timer timer_RNG_bias;
        private System.Windows.Forms.Button btn_DownLeft;
        private System.Windows.Forms.Button btn_Down;
        private System.Windows.Forms.Button btn_DownRight;
        private System.Windows.Forms.Button btn_Left;
        private System.Windows.Forms.Button btn_Neutral;
        private System.Windows.Forms.Button btn_Right;
        private System.Windows.Forms.Button btn_UpLeft;
        private System.Windows.Forms.Button btn_Up;
        private System.Windows.Forms.Button btn_UpRight;
        private System.Windows.Forms.TextBox txt_Parameter;
        private System.Windows.Forms.TextBox txt_IRCManual;
        private System.Windows.Forms.Timer timer_interface_stats;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button txt_Halp;
    }
}

