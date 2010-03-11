using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoTorrent;
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.BEncoding;
using System.IO;
using System.Runtime.Serialization;
using System.Net;
using MonoTorrent.Tracker;
using MonoTorrent.Tracker.Listeners;
using MonoTorrent.Dht;
using MonoTorrent.Dht.Listeners;
using MonoTorrent.Client.Connections;

namespace Lan_CDS
{
	class Transfer
	{
		#region Public Vars

		public ClientEngine engine;
		public List<TorrentManager> managerCollection;

		#endregion

		#region Private Vars

		private int enginePort = 55555;
		DhtListener dhtListener;
		private TorrentSettings defaultSettings;
		string basePath;
		string fastResumePath;
		string dhtNodes;
		string torrentsPath;
		string downloadsPath;

		#endregion

		public Transfer(string path)
		{
			this.basePath = Path.GetFullPath(path);
			this.torrentsPath = Path.Combine(basePath, "Torrents");
			this.fastResumePath = Path.Combine(basePath, "fastresume.data");
			this.dhtNodes = Path.Combine(basePath, "dhtNodes.data");
			this.downloadsPath = Path.Combine(basePath, "Downloads");

			EngineSettings settings = new EngineSettings(downloadsPath, enginePort);
			settings.AllowedEncryption = MonoTorrent.Client.Encryption.EncryptionTypes.All;
			settings.PreferEncryption = false;

			this.engine = new ClientEngine(settings);
			engine.ConnectionManager.PeerMessageTransferred += delegate(object o, PeerMessageEventArgs e)
			{
				Console.WriteLine( e.Message);
			};
			
			this.managerCollection = new List<TorrentManager>();
			this.defaultSettings = new TorrentSettings(50, 100, 100000, 100000, false);
			defaultSettings.UseDht = false;
			defaultSettings.EnablePeerExchange = true;
			this.startDHT();


		}

		#region Add Torrents
		public void AddTorrent(string path)
		{
			Torrent torrent = Torrent.Load(path);
			TorrentManager manager = new TorrentManager(torrent, downloadsPath, defaultSettings);
			engine.Register(manager);
			managerCollection.Add(manager);
		}

		public TorrentManager AddHexHash(string hash)
		{
			List<MonoTorrentCollection<string>> announces = new List<MonoTorrentCollection<string>>();
			announces.Add(new MonoTorrentCollection<string>() { });
			TorrentManager manager = new TorrentManager(InfoHash.FromHex(hash), downloadsPath, defaultSettings, torrentsPath, announces);
			engine.Register(manager);
			managerCollection.Add(manager);
			return manager;
		}
		#endregion

		#region Save/Load Torrents
		public void saveFastResume(List<TorrentManager> managers)
		{
			BEncodedList list = new BEncodedList();
			foreach (TorrentManager manager in managers)
			{
				FastResume data = manager.SaveFastResume();
				BEncodedDictionary fastResume = data.Encode();
				list.Add(fastResume);
			}
			File.WriteAllBytes(fastResumePath, list.Encode());
		}

		public void loadFastResume(List<TorrentManager> managers)
		{
			if (File.Exists(fastResumePath))
			{
				BEncodedList list = (BEncodedList)BEncodedValue.Decode(File.ReadAllBytes(fastResumePath));
				foreach (BEncodedDictionary fastResume in list)
				{
					FastResume data = new FastResume(fastResume);
					foreach (TorrentManager manager in managers)
					{
						if (manager.InfoHash == data.Infohash)
						{
							manager.LoadFastResume(data);
						}
					}
				}
			}
		}

		public void loadFastResume()
		{
			if (Directory.Exists(torrentsPath))
			{
				foreach (string s in Directory.GetFiles(torrentsPath, "*.torrent"))
				{
					AddTorrent(s);
				}
				this.loadFastResume(this.managerCollection);
			}
			else
			{
				Directory.CreateDirectory(torrentsPath);
				Directory.CreateDirectory(downloadsPath);
			}
		}
		#endregion

		public TorrentManager getTorrentByHash(string hash)
		{
			foreach (TorrentManager manager in this.managerCollection)
			{
				if (manager.InfoHash.ToHex() == hash)
					return manager;
				
			}
			return null;
		}

		#region DHT Stuff
		public void startDHT()
		{
			dhtListener = new DhtListener (new IPEndPoint (IPAddress.Any, enginePort)); 
            DhtEngine dht = new DhtEngine (dhtListener); 
            engine.RegisterDht(dht); // engine == ClientEngine) 
            dhtListener.Start();

			

			byte[] nodes = null;
			if (File.Exists(dhtNodes))
				nodes = File.ReadAllBytes(dhtNodes);
			engine.DhtEngine.Start(nodes);
		}

		public void stopDHT()
		{
			engine.DhtEngine.Stop();
			File.WriteAllBytes(dhtNodes, engine.DhtEngine.SaveNodes());
		}

		public string dhtStatus()
		{
			return "DL:"+this.dhtListener.Status + " DE:" +  this.engine.DhtEngine.State.ToString();
		}
		#endregion

	}
}
