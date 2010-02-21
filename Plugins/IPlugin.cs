using System;

namespace Lan_CDS.Plugins
{
	public interface IPlugin
	{
		//IPluginHost Host { get; set; }

		string Name { get; }
		string Description { get; }

		System.Windows.Forms.ToolStripMenuItem ToolItem { get; }

		void Initialize();
		void Dispose();
	}
	
	public enum PluginType
	{
		Unknown
	};

	[AttributeUsage(AttributeTargets.Class)]
public sealed class PluginAttribute:Attribute
{
	private PluginType _Type;
 
	public PluginAttribute(PluginType T) { _Type = T; }
 
	public PluginType Type { get { return _Type; } }
}
}
