Created by: Scott Mundorff
Project hosted at: http://github.com/descention/LAN-CDS/

== Lan-CDS ==

This application is designed as a Content Distribution System for Local 
Area Networks. It connects to the uTorrent web API of a single host and 
pulls the list of available torrents. These are displayed in a list and 
provide an easy to use interface for downloading selected applications.

=== Required libraries ===
*MonoTorrent
*MonoTorrent.Dht
*Cleverscape.UTorrentClient (included)

This project was last using build 150259 of the MonoTorrent SVN source.

=== To use this application ===

This application connects to uTorrent’s Web API. That requires that you 
have the web front-end enabled for a uTorrent client you wish to connect 
to. The username and password must also be set. This application does not 
currently support guest login. The uTorrent client you connect to will be 
know as the ‘server’.

The application checks for .torrent files in a folder named “Torrents” in 
the program’s current directory. These torrent files will be loaded into 
the application as local torrent files.

When the application launches, it checks for the “settings.data” file 
which contains connection information. If this file does not exist, you 
will be prompted for login information for the server. Once connected to 
a server, you will have a list of remote torrents available for download. 
Double-click an item in the “Remote” list to begin transfer of the torrent 
data to your local client.

NOTE: The transfer of metadata information is not automatic. This is a 
known bug. Current workaround until the issue is resolved: add the lan-cds 
client to the peer list in utorrent manually. Current port for the client 
is 55555.