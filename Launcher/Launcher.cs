using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using Library;

namespace Launcher
{
    public partial class Launcher : Form
    {
		string settingsFile = "launcher.data";
		string fileName;
		string fileArgs;

        public Launcher()
        {
            InitializeComponent();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
			if (File.Exists(settingsFile))
			{
				Process pr = new Process();
				pr.StartInfo = (ProcessStartInfo)Serial.load(settingsFile);
				pr.Start();
				this.Close();
			}
			foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.exe", SearchOption.AllDirectories))
			{
				if (Path.GetFileName(file) != "Launcher.exe")
				{
					ListViewItem item = new ListViewItem(Path.GetFileName(file));
					item.Tag = file;
					FileInfo info = new FileInfo(file);
					item.SubItems.Add(info.Extension);
					listView1.Items.Add(item);
				}
			}
        }

		private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Process pr = new Process();
			pr.StartInfo = new ProcessStartInfo((sender as ListView).SelectedItems[0].Tag.ToString());
			pr.Start();
			this.Close();
			if (checkBox1.Checked == false)
				Serial.Save(new ProcessStartInfo(fileName, fileArgs), settingsFile);
		}

		private void Launcher_FormClosing(object sender, FormClosingEventArgs e)
		{
			
		}
    }
}
