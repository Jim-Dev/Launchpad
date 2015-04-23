using System;
using System.Collections.Generic;
using System.IO;

namespace Launchpad
{
	internal sealed class ManifestHandler
	{	
		private List<ManifestEntry> manifest = new List<ManifestEntry> ();

		/// <summary>
		/// Gets the manifest. Call sparsely, as it loads the entire manifest from disk each time 
		/// this property is accessed.
		/// </summary>
		/// <value>The manifest.</value>
		public List<ManifestEntry> Manifest
		{
			get
			{
				LoadManifest ();
				return manifest;
			}
		}

		private List<ManifestEntry> oldManifest = new List<ManifestEntry>();

		/// <summary>
		/// Gets the old manifest. Call sparsely, as it loads the entire manifest from disk each time
		/// this property is accessed.
		/// </summary>
		/// <value>The old manifest.</value>
		public List<ManifestEntry> OldManifest
		{
			get
			{
				LoadOldManifest ();
				return oldManifest;
			}
		}

		private object ManifestLock = new object();
		private object OldManifestLock = new object();

		public ManifestHandler ()
		{

		}

		private void LoadManifest()
		{
			try
			{
				lock (ManifestLock)
				{
					if (File.Exists(ConfigHandler.GetManifestPath()))
					{
						string[] rawManifest = File.ReadAllLines (ConfigHandler.GetManifestPath ());
						foreach (string rawEntry in rawManifest)
						{
							ManifestEntry newEntry = new ManifestEntry ();
							if (ManifestEntry.TryParse (rawEntry, out newEntry))
							{
								manifest.Add (newEntry);
							}
						}
					}
				}
			}
			catch (IOException ioex)
			{
				Console.WriteLine ("IOException in LoadManifest(): " + ioex.Message);
			}
		}

		private void LoadOldManifest()
		{
			try
			{
				lock (OldManifestLock)
				{
					if (File.Exists(ConfigHandler.GetOldManifestPath()))
					{
						string[] rawOldManifest = File.ReadAllLines (ConfigHandler.GetOldManifestPath ());
						foreach (string rawEntry in rawOldManifest)
						{
							ManifestEntry newEntry = new ManifestEntry ();
							if (ManifestEntry.TryParse (rawEntry, out newEntry))
							{
								oldManifest.Add (newEntry);
							}
						}
					}
				}
			}
			catch (IOException ioex)
			{
				Console.WriteLine ("IOException in LoadOldManifest(): " + ioex.Message);
			}
		}
	}

	internal sealed class ManifestEntry
	{
		public string RelativePath
		{
			get;
			set;
		}

		public string Hash
		{
			get;
			set;
		}

		public long Size
		{
			get;
			set;
		}

		public ManifestEntry()
		{
			RelativePath = String.Empty;
			Hash = String.Empty;
			Size = 0;
		}

		public static bool TryParse(string rawInput, out ManifestEntry entry)
		{
			//clear out the entry for the new data
			entry = new ManifestEntry ();

			if (!String.IsNullOrEmpty(rawInput))
			{
				//remove any and all bad characters from the input string
				string cleanInput = Utilities.Clean (rawInput);

				//split the string into its three components - file, hash and size
				string[] elements = cleanInput.Split (':');

				//if we have three elements (which we should always have), set them in the provided entry
				if (elements.Length == 3)
				{
					//clean the manifest path, converting \ to / on unix and / to \ on Windows.
					entry.RelativePath = elements [0].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
					entry.Hash = elements [1];

					long parsedSize = 0;
					if(long.TryParse(elements[2], out parsedSize))
					{
						entry.Size = parsedSize;
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public override string ToString() 
		{
			return RelativePath + ":" + Hash + ":" + Size.ToString ();
		}
	}
}
