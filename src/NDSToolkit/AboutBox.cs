using System;
using System.Windows.Forms;

namespace NDSToolkit
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }
    }
}
