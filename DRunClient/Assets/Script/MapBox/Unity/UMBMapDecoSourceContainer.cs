using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[CreateAssetMenu(fileName = "MapBoxMapDecoSource", menuName = "Festa/MapBox/MapDecoSource", order = 3)]
	public class UMBMapDecoSourceContainer : ScriptableObject
	{
		[Serializable]
		public class SourceData
		{
			public string id;
			public UMBDeco deco_source;
		}

		public List<SourceData> source_list;

		private Dictionary<int, SourceData> _dataDic;
		private List<SourceData> _noneTreeList;

		public SourceData getSource(int id)
		{
			if( _dataDic == null )
			{
				makeDic();
			}

			SourceData data;
			if( _dataDic.TryGetValue(id, out data) )
			{
				return data;
			}

			return null;
		}

		public SourceData getSource(string id)
		{
			return getSource(EncryptUtil.makeHashCode(id));
		}

		public List<SourceData> getNoneTreeList()
		{
			return _noneTreeList;
		}

		private void makeDic()
		{
			_dataDic = new Dictionary<int, SourceData>();
			_noneTreeList = new List<SourceData>();
			foreach(SourceData data in source_list)
			{
				data.deco_source.initSource();

				_dataDic.Add(EncryptUtil.makeHashCode(data.id), data);
				if( data.id.StartsWith("tree_") == false)
				{
					_noneTreeList.Add(data);
				}
			}
		}
	}
}
