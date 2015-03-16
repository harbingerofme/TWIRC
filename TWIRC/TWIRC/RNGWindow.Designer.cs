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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RNGWindow));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btn_KillClients = new System.Windows.Forms.Button();
            this.btn_RestartIRC = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.ts_isconnected = new System.Windows.Forms.ToolStripStatusLabel();
            this.ts_counter0 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ts_botColour = new System.Windows.Forms.ToolStripStatusLabel();
            this.ts_Matinence = new System.Windows.Forms.ToolStripSplitButton();
            this.ts_Matinence_on = new System.Windows.Forms.ToolStripMenuItem();
            this.ts_Matinence_off = new System.Windows.Forms.ToolStripMenuItem();
            this.ts_MatinenceLevel = new System.Windows.Forms.ToolStripStatusLabel();
            this.text_log = new System.Windows.Forms.TextBox();
            this.txt_IRCManual = new System.Windows.Forms.TextBox();
            this.txt_Halp = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_KillClients
            // 
            this.btn_KillClients.Location = new System.Drawing.Point(0, 0);
            this.btn_KillClients.Name = "btn_KillClients";
            this.btn_KillClients.Size = new System.Drawing.Size(75, 23);
            this.btn_KillClients.TabIndex = 36;
            // 
            // btn_RestartIRC
            // 
            this.btn_RestartIRC.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_RestartIRC.Location = new System.Drawing.Point(257, 363);
            this.btn_RestartIRC.Name = "btn_RestartIRC";
            this.btn_RestartIRC.Size = new System.Drawing.Size(76, 22);
            this.btn_RestartIRC.TabIndex = 2;
            this.btn_RestartIRC.Text = "Restart IRC";
            this.btn_RestartIRC.UseVisualStyleBackColor = true;
            this.btn_RestartIRC.Click += new System.EventHandler(this.btn_RestartIRC_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ts_isconnected,
            this.ts_counter0,
            this.ts_botColour,
            this.ts_Matinence,
            this.ts_MatinenceLevel});
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
            this.ts_counter0.Name = "ts_counter0";
            this.ts_counter0.Size = new System.Drawing.Size(0, 17);
            // 
            // ts_botColour
            // 
            this.ts_botColour.Name = "ts_botColour";
            this.ts_botColour.Size = new System.Drawing.Size(75, 17);
            this.ts_botColour.Text = "ts_botColour";
            // 
            // ts_Matinence
            // 
            this.ts_Matinence.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.ts_Matinence.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ts_Matinence_on,
            this.ts_Matinence_off});
            this.ts_Matinence.Image = ((System.Drawing.Image)(resources.GetObject("ts_Matinence.Image")));
            this.ts_Matinence.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.ts_Matinence.Name = "ts_Matinence";
            this.ts_Matinence.Size = new System.Drawing.Size(82, 20);
            this.ts_Matinence.Text = "Matinence:";
            // 
            // ts_Matinence_on
            // 
            this.ts_Matinence_on.Name = "ts_Matinence_on";
            this.ts_Matinence_on.Size = new System.Drawing.Size(91, 22);
            this.ts_Matinence_on.Text = "On";
            this.ts_Matinence_on.Click += new System.EventHandler(this.ts_Matinence_on_Click);
            // 
            // ts_Matinence_off
            // 
            this.ts_Matinence_off.Name = "ts_Matinence_off";
            this.ts_Matinence_off.Size = new System.Drawing.Size(91, 22);
            this.ts_Matinence_off.Text = "Off";
            this.ts_Matinence_off.Click += new System.EventHandler(this.ts_Matinence_off_Click);
            // 
            // ts_MatinenceLevel
            // 
            this.ts_MatinenceLevel.Name = "ts_MatinenceLevel";
            this.ts_MatinenceLevel.Size = new System.Drawing.Size(23, 17);
            this.ts_MatinenceLevel.Text = "On";
            // 
            // text_log
            // 
            this.text_log.BackColor = System.Drawing.Color.Gainsboro;
            this.text_log.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_log.ForeColor = System.Drawing.Color.Black;
            this.text_log.Location = new System.Drawing.Point(0, 0);
            this.text_log.Multiline = true;
            this.text_log.Name = "text_log";
            this.text_log.ReadOnly = true;
            this.text_log.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.text_log.Size = new System.Drawing.Size(415, 331);
            this.text_log.TabIndex = 16;
            this.text_log.WordWrap = false;
            this.text_log.TextChanged += new System.EventHandler(this.text_log_TextChanged);
            // 
            // txt_IRCManual
            // 
            this.txt_IRCManual.Location = new System.Drawing.Point(0, 337);
            this.txt_IRCManual.Name = "txt_IRCManual";
            this.txt_IRCManual.Size = new System.Drawing.Size(415, 20);
            this.txt_IRCManual.TabIndex = 30;
            this.txt_IRCManual.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_IRCManual_KeyDown);
            // 
            // txt_Halp
            // 
            this.txt_Halp.Location = new System.Drawing.Point(339, 363);
            this.txt_Halp.Name = "txt_Halp";
            this.txt_Halp.Size = new System.Drawing.Size(76, 22);
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
            this.Controls.Add(this.txt_IRCManual);
            this.Controls.Add(this.text_log);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btn_RestartIRC);
            this.Controls.Add(this.btn_KillClients);
            this.MinimumSize = new System.Drawing.Size(431, 448);
            this.Name = "RNGWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SayingsBot";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RNGWindow_FormClosed);
            this.Load += new System.EventHandler(this.RNGWindow_Load);
            this.LocationChanged += new System.EventHandler(this.RNGWindow_LocationChanged);
            this.SizeChanged += new System.EventHandler(this.RNGWindow_SizeChanged);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btn_KillClients;
        private System.Windows.Forms.Button btn_RestartIRC;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel ts_isconnected;
        private System.Windows.Forms.TextBox text_log;
        private System.Windows.Forms.ToolStripStatusLabel ts_counter0;
        private System.Windows.Forms.TextBox txt_IRCManual;
        private System.Windows.Forms.Button txt_Halp;
        public System.Windows.Forms.ToolStripStatusLabel ts_botColour;
        private System.Windows.Forms.ToolStripSplitButton ts_Matinence;
        private System.Windows.Forms.ToolStripMenuItem ts_Matinence_on;
        private System.Windows.Forms.ToolStripMenuItem ts_Matinence_off;
        public System.Windows.Forms.ToolStripStatusLabel ts_MatinenceLevel;
    }
}

