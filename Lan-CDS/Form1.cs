﻿using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Net;

using MonoTorrent.Client;
using MonoTorrent.Common;

using Cleverscape.UTorrentClient.WebClient;

using Library;

using Lan_CDS.Plugins;

namespace Lan_CDS
{
	public partial class Form1 : Form
	{
		#region Private Variables
		private string basePath;
		private string settingsFile;
		private string pluginPath;

		private Dictionary<string, string> config;

		private UTorrentWebClient webClient;
		private bool online = false;
		private TorrentCollection torrentCollection;
		private BackgroundWorker bgUpdate;
		private BackgroundWorker bgConnect;
		
		Transfer tEngine;

		List<string> localTorrents;
		List<string> remoteTorrents;

		List<IPlugin> plugins = new List<IPlugin>();
		#endregion

		#region Form Events

		public Form1()
		{
			InitializeComponent();
		}
		
		private void Form1_Load(object sender, EventArgs e)
		{
			Serial s = new Serial();
			basePath = Environment.CurrentDirectory;
			pluginPath = Path.Combine(basePath, "Plugins");
			settingsFile = Path.Combine(basePath, "settings.data");
			config = File.Exists(settingsFile) ? (Dictionary<string, string>)s.load(settingsFile) : new Dictionary<string, string>();
			
			// init torrent lists
			localTorrents = new List<string>();
			remoteTorrents = new List<string>();

			loadPlugins();

			// create background connection worker
			bgConnect = new BackgroundWorker();
			bgConnect.DoWork += new DoWorkEventHandler(connect);
			bgConnect.RunWorkerCompleted += new RunWorkerCompletedEventHandler(connectComplete);
			bgConnect.WorkerSupportsCancellation = true;

			// create background update worker
			bgUpdate = new BackgroundWorker();
            bgUpdate.DoWork += new DoWorkEventHandler(update);
            bgUpdate.RunWorkerCompleted += new RunWorkerCompletedEventHandler(updateComplete);
            bgUpdate.ProgressChanged += new ProgressChangedEventHandler(updateProgress);
			
			// create torrent engine and load local files
			try
			{
				tEngine = new Transfer(basePath);

				tEngine.engine.TorrentRegistered += delegate(object o, MonoTorrent.Client.TorrentEventArgs ex)
				{
					localTorrents.Add(ex.TorrentManager.InfoHash.ToHex());
					// Add known uTorrent client (seeder/peer) as we don't use the DHT network.
					if(config.ContainsKey("server"))
						ex.TorrentManager.AddPeers(new Peer("", new Uri("tcp://" + config["server"])));
				};

				tEngine.loadFastResume();
				tEngine.engine.StartAll();
				

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Data);
				throw new Exception();
			}

			// begin connection
			tryConnect(null);
			
			// prepare regular updater for ui.
			System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
			
			timer.Interval = 1000;
			//timer.AutoReset = false;
			timer.Tick += delegate(object o, EventArgs ex)
			{
				timer.Stop();
				// do list update
				draw();
				// start timer again
				timer.Start();
			};

			// start regular ui updater
			timer.Start();

		}

		private void itemDoubleClick(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection s = (sender as ListView).SelectedItems;
			foreach (ListViewItem i in s)
			{
				string hash = (string)i.Tag;
				if (remoteTorrents.Contains(hash) && torrentCollection.Contains(hash))
				{
					Cleverscape.UTorrentClient.WebClient.Torrent torrent = getTorrentByHash(hash);

					// add torrent hash to manager. 
					// Manager automatically added to the ClientEngine (tEngine) TorrentManager List
					TorrentManager manager = tEngine.AddHexHash(torrent.Hash);

					// Automatically start torrent data transfer
					manager.Start();

					// Debug state change errors.
					manager.TorrentStateChanged += delegate(object o, TorrentStateChangedEventArgs ex)
					{
						if (ex.NewState == TorrentState.Error)
						{
							Console.WriteLine("Torrent Error: {0} : {1}",ex.TorrentManager.Error.Reason,ex.TorrentManager.Error.Exception);
						}
					};
				}
				else if (localTorrents.Contains(hash) && tEngine.getTorrentByHash(hash).Complete)
				{
					Process p = new Process();
					p.StartInfo.FileName = Path.Combine( tEngine.getTorrentByHash(hash).SavePath, "Launcher.exe");
					//p.StartInfo.Arguments = "-x";
					p.Start();
				}
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			try
			{
				Serial s = new Serial();
				s.save(config,settingsFile);
				tEngine.stopDHT();
				foreach (TorrentManager manager in tEngine.managerCollection)
					if (manager.State == TorrentState.Metadata)
					{
						manager.Dispose();
					}
				
				tEngine.saveFastResume(tEngine.managerCollection);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.StackTrace + "\n" + ex.Message);
			}
		}
		
		#endregion

		#region Draw ListView

		private ListViewItem defaultItem(string name, string status, string hash, string groupStr)
		{
			string[] ar = { name, status };

			if (listView1.Groups[groupStr.ToLower()] == null)
			{
				listView1.Groups.Add(groupStr.ToLower(), groupStr);
			}
			ListViewItem item = new ListViewItem(ar, listView1.Groups[groupStr.ToLower()]);
			item.SubItems[0].Name = "name";
			item.SubItems[1].Name = "progress";
			item.Tag = hash;

			return item;
		}

		private void draw()
		{
			this.Text = "LAN-CDS: D:" + Utilities.FormatFileSize(tEngine.engine.TotalDownloadSpeed) + " U:" + Utilities.FormatFileSize(tEngine.engine.TotalUploadSpeed);
			
			//this.toolStripStatusLabel2.Text = tEngine.engine.DhtEngine.;

			List<string> covered = new List<string>();

			foreach (ListViewItem item in listView1.Items)
			{
				string hash = (string)item.Tag;
				if (localTorrents.Contains(hash))// local always has priority
				{
					if (remoteTorrents.Contains(hash))
						remoteTorrents.Remove(hash);
					// updates on progress, status, availability
					TorrentManager manager = tEngine.getTorrentByHash(hash);
					string newText = manager.State.ToString() + ": " + Math.Round( manager.Progress,2).ToString() + "%";
					if(newText != item.SubItems["progress"].Text)
						item.SubItems["progress"].Text = newText;
					if (item.Group == listView1.Groups["remote"])
						item.Group = listView1.Groups["local"];
					item.ToolTipText = manager.Peers.Seeds.ToString() + " friends have it. " + manager.Peers.Leechs.ToString() + " are getting it.";

				}
				else if (remoteTorrents.Contains(hash))
				{
					// these won't have much to update as they are remote torrents, not loaded locally at all
					// might have to check for new torrents available on the server
				}
				else
				{
					// where the hell did it come from?!
					item.Remove();
				}
				covered.Add(hash);
			}

			foreach (string hash in localTorrents)
			{
				if (!covered.Contains(hash))
				{
					if (remoteTorrents.Contains(hash))
						remoteTorrents.Remove(hash);
					TorrentManager manager = tEngine.getTorrentByHash(hash);
					if (manager.HasMetadata)
					{
						MonoTorrent.Common.Torrent torrent = manager.Torrent;
						listView1.Items.Add(defaultItem(torrent.Name, manager.State.ToString(), hash, "Local"));
					}
					else
					{
						listView1.Items.Add(defaultItem(manager.SavePath, manager.State.ToString(), hash, "Local"));
					}
				}
			}

			foreach (string hash in remoteTorrents)
			{
				if (!covered.Contains(hash))
				{
					Cleverscape.UTorrentClient.WebClient.Torrent torrent = getTorrentByHash(hash);

					listView1.Items.Add(defaultItem(torrent.Name, "", hash, "Remote"));
				}
			}
		}
		#endregion

		#region Find Torrent by Hash

		private MonoTorrent.Common.Torrent localTorrentByHash(string hash)
		{
			foreach (TorrentManager manager in tEngine.managerCollection)
			{
				if (manager.Torrent.InfoHash.ToHex() == hash)
				{
					return manager.Torrent;
				}
			}
			return null;
		}

		private Cleverscape.UTorrentClient.WebClient.Torrent getTorrentByHash(string hash)
		{
			foreach (Cleverscape.UTorrentClient.WebClient.Torrent torrent in torrentCollection)
			{
				if (torrent.Hash == hash)
					return torrent;
			}
			return null;
		}
		#endregion

		#region WebClient Functions

		private void connect(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (!config.ContainsKey("server") || e.Argument.ToString() == "force")
				{
					InputBox requestServer = new InputBox();
					requestServer.ShowDialog();
					if (requestServer.server == null)
						throw new Exception("No server specified");
					IPAddress address;
					string host = requestServer.server.Split(':')[0];

					if (!IPAddress.TryParse(host, out address))
					{
						string hostIP = Dns.GetHostAddresses(host)[0].ToString();
						config["server"] = hostIP + ":" + requestServer.server.ToString().Split(':')[1];
					}
					else
					{
						config["server"] = requestServer.server;
					}
					config["username"] = requestServer.user;
					config["password"] = requestServer.pass;
				}
				
				webClient = new UTorrentWebClient(config["server"], config["username"], config["password"]);
				if (webClient == null)
					throw new Exception("Not Connected");
				online = true;
			}
			catch(Exception ex)
			{
				Console.WriteLine( ex.StackTrace);
				Console.WriteLine( ex.Message);
				e.Result = "Connection Error";
				online = false;
			}
		}

		private void connectComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			// error handling
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.Message);
			}
			else if (e.Cancelled) // cancellation handling
			{
				MessageBox.Show("cancelled");
			}
			else // completed
			{
				//connection complete
				//get torrents from server and update form list
				if (online)
				{
					toolStripStatusLabel1.Text = "Connected: Requesting List";
					bgUpdate.RunWorkerAsync();
				}
				else
				{
					toolStripStatusLabel1.Text = "Offline";
				}
			}
		}

		private void update(object sender, DoWorkEventArgs e)
		{
			torrentCollection = webClient.Torrents;
		}

		private void updateComplete(object sender, RunWorkerCompletedEventArgs e)
		{
			// error handling
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.Message);
			}
			else if (e.Cancelled) // cancellation handling
			{
				// Next, handle the case where the user canceled 
				// the operation.
				// Note that due to a race condition in 
				// the DoWork event handler, the Cancelled
				// flag may not have been set, even though
				// CancelAsync was called.
				MessageBox.Show("cancelled");
			}
			else
			{
				if (online)
				{
					toolStripStatusLabel1.Text = "Updating...";
					try
					{
						foreach (Cleverscape.UTorrentClient.WebClient.Torrent torrent in torrentCollection)
						{
							remoteTorrents.Add(torrent.Hash);
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.StackTrace + "\n" + ex.Message);
					}
					finally
					{
						goOnline();
					}
				}
				else
				{
					goOnline(false);
				}
			}
		}

		private void updateProgress(object sender, ProgressChangedEventArgs e)
		{

		}

		private void connectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!online)
			{
				
			}
		}

		private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			goOnline(false);
			webClient.Dispose();
			webClient = null;
			remoteTorrents.Clear();
		}

		private void goOnline()
		{
			goOnline(true);
		}

		private void goOnline(bool online)
		{
			if (online)
			{
				this.online = true;
				connectToolStripMenuItem.Enabled = false;
				disconnectToolStripMenuItem.Enabled = true;
				toolStripStatusLabel1.Text = "Online";
			}
			else
			{
				this.online = false;
				connectToolStripMenuItem.Enabled = true;
				disconnectToolStripMenuItem.Enabled = false;
				toolStripStatusLabel1.Text = "Offline";
			}
		}

		private bool tryConnect(string arg)
		{
			toolStripStatusLabel1.Text = "Connecting...";
			if (bgConnect.IsBusy)
			{
				bgConnect.CancelAsync();
			}
			else
			{
				bgConnect.RunWorkerAsync("force");
			}
			return false;
		}

		#endregion

		#region Plugin System
		private void loadPlugins()
		{
			if (!Directory.Exists(pluginPath))
				return;

			Assembly asm;

			string[] files = Directory.GetFiles(pluginPath,"*.dll");
			foreach (string filePath in files)
			{
				try
				{
					asm = Assembly.LoadFile(filePath);
					if (asm != null)
					{
						foreach (Type type in asm.GetTypes())
						{
							if (type.IsAbstract == false)
							{
								IPlugin b = type.InvokeMember(null,
														   BindingFlags.CreateInstance,
														   null, null, null) as IPlugin;
								plugins.Add(b);
								//pluginsToolStripMenuItem.DropDownItems.Add(b.ToolItem);
							}
						}
					}
				}
				catch (Exception e)
				{
					MessageBox.Show(e.Message);
				}
			}
		}
		#endregion

	}
}
