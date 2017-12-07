namespace SayingsBot
{
    partial class frmDiscord
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDiscord));
            this.txt_Discord = new System.Windows.Forms.TextBox();
            this.lbBanter = new System.Windows.Forms.ListBox();
            this.lbGuilds = new System.Windows.Forms.ListBox();
            this.lbChannels = new System.Windows.Forms.ListBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_Discord
            // 
            this.txt_Discord.Location = new System.Drawing.Point(0, 322);
            this.txt_Discord.Name = "txt_Discord";
            this.txt_Discord.Size = new System.Drawing.Size(415, 20);
            this.txt_Discord.TabIndex = 31;
            this.txt_Discord.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbMsg_KeyPress);
            // 
            // lbBanter
            // 
            this.lbBanter.FormattingEnabled = true;
            this.lbBanter.Location = new System.Drawing.Point(0, 0);
            this.lbBanter.Name = "lbBanter";
            this.lbBanter.Size = new System.Drawing.Size(415, 316);
            this.lbBanter.TabIndex = 32;
            // 
            // lbGuilds
            // 
            this.lbGuilds.FormattingEnabled = true;
            this.lbGuilds.Location = new System.Drawing.Point(-1, 348);
            this.lbGuilds.Name = "lbGuilds";
            this.lbGuilds.Size = new System.Drawing.Size(205, 56);
            this.lbGuilds.TabIndex = 33;
            this.lbGuilds.SelectedValueChanged += new System.EventHandler(this.lbGuilds_SelectedValueChanged);
            // 
            // lbChannels
            // 
            this.lbChannels.FormattingEnabled = true;
            this.lbChannels.Location = new System.Drawing.Point(210, 348);
            this.lbChannels.Name = "lbChannels";
            this.lbChannels.Size = new System.Drawing.Size(205, 56);
            this.lbChannels.TabIndex = 34;
            this.lbChannels.SelectedValueChanged += new System.EventHandler(this.lbChannels_SelectedValueChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(340, 410);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 35;
            this.btnConnect.Text = "Start";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // frmDiscord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 432);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lbChannels);
            this.Controls.Add(this.lbGuilds);
            this.Controls.Add(this.lbBanter);
            this.Controls.Add(this.txt_Discord);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(431, 471);
            this.Name = "frmDiscord";
            this.Text = "SayingsBot - Discord";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmDiscord_ForumClosed);
            this.Load += new System.EventHandler(this.frmDiscord_Load);
            this.SizeChanged += new System.EventHandler(this.frmDiscord_SizeChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txt_Discord;
        private System.Windows.Forms.ListBox lbBanter;
        private System.Windows.Forms.ListBox lbGuilds;
        private System.Windows.Forms.ListBox lbChannels;
        private System.Windows.Forms.Button btnConnect;
    }
}