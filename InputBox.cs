using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lan_CDS
{
    public partial class InputBox : Form
    {
        public string server;
		public string user;
		public string pass;

        public InputBox(string text)
        {
            InitializeComponent();
            label1.Text = text;
        }

        public InputBox(string title, string text)
        {
            InitializeComponent();
            this.Text = title;
            label1.Text = text;

        }

        private void okButton_Click(object sender, EventArgs e)
        {
            server = textBox1.Text;
			user = textBox2.Text;
			pass = textBox3.Text;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Input_Leave(object sender, EventArgs e)
        {
            
        }
    }
}
