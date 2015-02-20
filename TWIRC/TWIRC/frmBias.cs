using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RNGBot

{
    public partial class frmBias : Form
    {
        ButtonMasher RNGesus;
        Logger RNGLogger;
        public frmBias(Logger newRNGLogger, ButtonMasher newRNGesus)
        {
            RNGLogger = newRNGLogger;
            RNGesus = newRNGesus;

            InitializeComponent();
        }

        private void frmBias_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            double[] nextbias = new double[7];

            nextbias[0] = Convert.ToDouble(txtLeft.Text);
            nextbias[1] = Convert.ToDouble(txtDown.Text);
            nextbias[2] = Convert.ToDouble(txtUp.Text);
            nextbias[3] = Convert.ToDouble(txtRight.Text);
            nextbias[4] = Convert.ToDouble(txtA.Text);
            nextbias[5] = Convert.ToDouble(txtB.Text);
            nextbias[6] = Convert.ToDouble(txtStart.Text);

            RNGesus.setBias(nextbias);
        }


    }
}
