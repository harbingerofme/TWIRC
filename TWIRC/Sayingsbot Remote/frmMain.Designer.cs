namespace Sayingsbot_Remote
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.timerReconnect = new System.Windows.Forms.Timer(this.components);
            this.txtMain = new System.Windows.Forms.TextBox();
            this.txtBoxSend = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtMain
            // 
            this.txtMain.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtMain.Location = new System.Drawing.Point(0, 0);
            this.txtMain.Multiline = true;
            this.txtMain.Name = "txtMain";
            this.txtMain.ReadOnly = true;
            this.txtMain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMain.Size = new System.Drawing.Size(415, 370);
            this.txtMain.TabIndex = 0;
            // 
            // txtBoxSend
            // 
            this.txtBoxSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtBoxSend.Location = new System.Drawing.Point(0, 390);
            this.txtBoxSend.Name = "txtBoxSend";
            this.txtBoxSend.Size = new System.Drawing.Size(415, 20);
            this.txtBoxSend.TabIndex = 1;
            this.txtBoxSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtBoxSend_KeyDown);
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.button1.Location = new System.Drawing.Point(0, 367);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(415, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Reconnect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 410);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtBoxSend);
            this.Controls.Add(this.txtMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(431, 448);
            this.Name = "frmMain";
            this.Text = "Sayingsbot Remote";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.SizeChanged += this.frmMain_SizeChanged;
            this.FormClosed += this.frmMain_FormClosed;
            this.FormClosing += this.frmMain_FormClosing;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timerReconnect;
        private System.Windows.Forms.TextBox txtMain;
        private System.Windows.Forms.TextBox txtBoxSend;
        private System.Windows.Forms.Button button1;
    }
}

