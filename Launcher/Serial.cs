using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Library
{
	public class Serial
	{
		public string defaultPath;

		public Serial()
		{
			this.defaultPath = Environment.CurrentDirectory + "\\object.save";
		}

		public Serial(string path)
		{
			this.defaultPath = path;
		}

		public void save(object saveObject, string filename)
		{
			try
			{
				Stream stream = File.Open(filename, FileMode.Create);
				BinaryFormatter bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, saveObject);
				stream.Close();
			}
			catch(Exception e)
			{
				throw new Exception(e.StackTrace + "\n==============\n" + e.Message);
			}
		}

		public static void Save(object saveObject, string filename)
		{
			try
			{
				Stream stream = File.Open(filename, FileMode.Create);
				BinaryFormatter bFormatter = new BinaryFormatter();
				bFormatter.Serialize(stream, saveObject);
				stream.Close();
			}
			catch (Exception e)
			{
				throw new Exception(e.StackTrace + "\n==============\n" + e.Message);
			}
		}

		public void save(object saveObject)
		{
			this.save(saveObject, defaultPath);
		}

		public static object load(string filename)
		{
			object objectToSerialize = null;
			try
			{
				if (File.Exists(filename))
				{
					Stream stream = File.Open(filename, FileMode.Open);
					if (stream.Length > 0)
					{
						BinaryFormatter bFormatter = new BinaryFormatter();
						objectToSerialize = (object)bFormatter.Deserialize(stream);
					}
					stream.Close();
				}
			}
			catch
			{
				return null;
			}
			return objectToSerialize;
		}
	}
}
