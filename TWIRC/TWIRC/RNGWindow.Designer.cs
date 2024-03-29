﻿/*
namespace TWIRC
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
            this.timer_RNG_bias = new System.Windows.Forms.Timer(this.components);
            this.txt_Parameter = new System.Windows.Forms.TextBox();
            this.txt_IRCManual = new System.Windows.Forms.TextBox();
            this.timer_interface_stats = new System.Windows.Forms.Timer(this.components);
            this.btn_Save = new System.Windows.Forms.Button();
            this.txt_Halp = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.btn_dumpBiases = new System.Windows.Forms.Button();
            this.timer_save = new System.Windows.Forms.Timer(this.components);
            this.btn_Leaderboard = new System.Windows.Forms.Button();
            this.btn_voteTimer = new System.Windows.Forms.Button();
            this.btn_rngTest = new System.Windows.Forms.Button();
            this.txt_numrolls = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_KillClients
            // 
            this.btn_KillClients.Location = new System.Drawing.Point(0, 271);
            this.btn_KillClients.Name = "btn_KillClients";
            this.btn_KillClients.Size = new System.Drawing.Size(77, 24);
            this.btn_KillClients.TabIndex = 0;
            this.btn_KillClients.Text = "Kill Clients";
            this.btn_KillClients.UseVisualStyleBackColor = true;
            this.btn_KillClients.Click += new System.EventHandler(this.btn_KillClients_Click);
            // 
            // btn_RestartIRC
            // 
            this.btn_RestartIRC.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_RestartIRC.Location = new System.Drawing.Point(246, 361);
            this.btn_RestartIRC.Name = "btn_RestartIRC";
            this.btn_RestartIRC.Size = new System.Drawing.Size(87, 22);
            this.btn_RestartIRC.TabIndex = 2;
            this.btn_RestartIRC.Text = "disconnect irc";
            this.btn_RestartIRC.UseVisualStyleBackColor = true;
            this.btn_RestartIRC.Click += new System.EventHandler(this.btn_RestartIRC_Click);
            // 
            // btn_Decay
            // 
            this.btn_Decay.Location = new System.Drawing.Point(196, 272);
            this.btn_Decay.Name = "btn_Decay";
            this.btn_Decay.Size = new System.Drawing.Size(49, 23);
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
            this.text_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.text_log.Size = new System.Drawing.Size(415, 228);
            this.text_log.TabIndex = 16;
            this.text_log.TextChanged += new System.EventHandler(this.text_log_TextChanged);
            // 
            // timer_RNG
            // 
            this.timer_RNG.Interval = 40;
            this.timer_RNG.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // txt_RNGInterval
            // 
            this.txt_RNGInterval.Location = new System.Drawing.Point(166, 273);
            this.txt_RNGInterval.Name = "txt_RNGInterval";
            this.txt_RNGInterval.Size = new System.Drawing.Size(24, 20);
            this.txt_RNGInterval.TabIndex = 17;
            this.txt_RNGInterval.Text = "40";
            this.txt_RNGInterval.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_RNGInterval_KeyDown);
            // 
            // timer_RNG_bias
            // 
            this.timer_RNG_bias.Interval = 60000;
            this.timer_RNG_bias.Tick += new System.EventHandler(this.timer_RNG_bias_Tick);
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
            this.txt_IRCManual.Size = new System.Drawing.Size(415, 20);
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
            this.btn_Save.Size = new System.Drawing.Size(77, 23);
            this.btn_Save.TabIndex = 31;
            this.btn_Save.Text = "Save";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // txt_Halp
            // 
            this.txt_Halp.Location = new System.Drawing.Point(339, 362);
            this.txt_Halp.Name = "txt_Halp";
            this.txt_Halp.Size = new System.Drawing.Size(76, 22);
            this.txt_Halp.TabIndex = 32;
            this.txt_Halp.Text = "Halp";
            this.txt_Halp.UseVisualStyleBackColor = true;
            this.txt_Halp.Click += new System.EventHandler(this.txt_Halp_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(0, 325);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(77, 37);
            this.button1.TabIndex = 33;
            this.button1.Text = "Manual Bias Entry";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(88, 275);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 17);
            this.checkBox1.TabIndex = 34;
            this.checkBox1.Text = "RNGesus";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // btn_dumpBiases
            // 
            this.btn_dumpBiases.Location = new System.Drawing.Point(0, 296);
            this.btn_dumpBiases.Name = "btn_dumpBiases";
            this.btn_dumpBiases.Size = new System.Drawing.Size(77, 23);
            this.btn_dumpBiases.TabIndex = 35;
            this.btn_dumpBiases.Text = "Dump Biases";
            this.btn_dumpBiases.UseVisualStyleBackColor = true;
            this.btn_dumpBiases.Click += new System.EventHandler(this.btn_dumpBiases_Click);
            // 
            // timer_save
            // 
            this.timer_save.Enabled = true;
            this.timer_save.Interval = 3600000;
            this.timer_save.Tick += new System.EventHandler(this.timer_save_Tick);
            // 
            // btn_Leaderboard
            // 
            this.btn_Leaderboard.Location = new System.Drawing.Point(339, 271);
            this.btn_Leaderboard.Name = "btn_Leaderboard";
            this.btn_Leaderboard.Size = new System.Drawing.Size(76, 35);
            this.btn_Leaderboard.TabIndex = 36;
            this.btn_Leaderboard.Text = "Show Leaderboard";
            this.btn_Leaderboard.UseVisualStyleBackColor = true;
            this.btn_Leaderboard.Click += new System.EventHandler(this.btn_Leaderboard_Click);
            // 
            // btn_voteTimer
            // 
            this.btn_voteTimer.Location = new System.Drawing.Point(339, 312);
            this.btn_voteTimer.Name = "btn_voteTimer";
            this.btn_voteTimer.Size = new System.Drawing.Size(76, 35);
            this.btn_voteTimer.TabIndex = 37;
            this.btn_voteTimer.Text = "Show Vote Timer";
            this.btn_voteTimer.UseVisualStyleBackColor = true;
            this.btn_voteTimer.Click += new System.EventHandler(this.btn_voteTimer_Click);
            // 
            // btn_rngTest
            // 
            this.btn_rngTest.Location = new System.Drawing.Point(319, 2);
            this.btn_rngTest.Name = "btn_rngTest";
            this.btn_rngTest.Size = new System.Drawing.Size(75, 23);
            this.btn_rngTest.TabIndex = 38;
            this.btn_rngTest.Text = "Test RNG";
            this.btn_rngTest.UseVisualStyleBackColor = true;
            this.btn_rngTest.Click += new System.EventHandler(this.button2_Click);
            // 
            // txt_numrolls
            // 
            this.txt_numrolls.Location = new System.Drawing.Point(217, 4);
            this.txt_numrolls.Name = "txt_numrolls";
            this.txt_numrolls.Size = new System.Drawing.Size(100, 20);
            this.txt_numrolls.TabIndex = 39;
            this.txt_numrolls.Text = "1000";
            this.txt_numrolls.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button2.Location = new System.Drawing.Point(246, 333);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 22);
            this.button2.TabIndex = 38;
            this.button2.Text = "connect irc";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button3.Location = new System.Drawing.Point(246, 304);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(87, 22);
            this.button3.TabIndex = 39;
            this.button3.Text = "reconnect irc";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // RNGWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 410);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btn_voteTimer);
            this.Controls.Add(this.btn_Leaderboard);
            this.Controls.Add(this.btn_dumpBiases);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txt_Halp);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.txt_IRCManual);
            this.Controls.Add(this.txt_Parameter);
            this.Controls.Add(this.txt_RNGInterval);
            this.Controls.Add(this.txt_Command);
            this.Controls.Add(this.text_log);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btn_Decay);
            this.Controls.Add(this.btn_RestartIRC);
            this.Controls.Add(this.btn_KillClients);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "RNGWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "RNG Bot";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RNGWindow_FormClosed);
            this.Load += new System.EventHandler(this.RNGWindow_Load);
            this.LocationChanged += new System.EventHandler(this.RNGWindow_LocationChanged);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_KillClients;
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
        private System.Windows.Forms.Timer timer_RNG_bias;
        private System.Windows.Forms.TextBox txt_Parameter;
        private System.Windows.Forms.TextBox txt_IRCManual;
        private System.Windows.Forms.Timer timer_interface_stats;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button txt_Halp;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button btn_dumpBiases;
        private System.Windows.Forms.Timer timer_save;
        private System.Windows.Forms.Button btn_Leaderboard;
        private System.Windows.Forms.Button btn_voteTimer;
        private System.Windows.Forms.Button btn_rngTest;
        private System.Windows.Forms.TextBox txt_numrolls;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}

*/