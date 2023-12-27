using Festa.Client.MapBox;
using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class MapTileRevealData
	{
		private HashSet<int> _revealData;
		private int _revision;

		public HashSet<int> getRevealData()
		{
			return _revealData;
		}

		public int getRevision()
		{
			return _revision;
		}

		public static MapTileRevealData create()
		{
			MapTileRevealData revealData = new MapTileRevealData();
			revealData.init(null);
			return revealData;
		}

		public static MapTileRevealData fromBlobData(BlobData data)
		{
			MapTileRevealData revealData = new MapTileRevealData();
			revealData.init(data);
			return revealData;
		}

		private void init(BlobData data)
		{
			_revealData = new HashSet<int>();
			_revision = 0;

			if ( data != null)
			{
				byte[] byte_array = data.getData();
				for(int i = 0; i < byte_array.Length / 2; ++i)
				{
					int value = (byte_array[i * 2 + 0] << 8) + (byte_array[i * 2 + 1]);
					_revealData.Add(value);
				}
			}
		}

		public BlobData toBlobData()
		{
			byte[] byte_array = new byte[_revealData.Count * 2];

			int index = 0;
			foreach(int value in _revealData)
			{
				byte_array[index * 2 + 0] = (byte)(value >> 8);
				byte_array[index * 2 + 1] = (byte)(value & 0xFF);
				++index;
			}

			return BlobData.create(byte_array);
		}

		public bool check(int key)
		{
			return _revealData.Contains(key);
		}

		public bool set(int key)
		{
			if (_revealData.Contains(key) == true)
			{
				return false;
			}

			_revealData.Add(key);
			_revision++;
			return true;
		}
	}
}
