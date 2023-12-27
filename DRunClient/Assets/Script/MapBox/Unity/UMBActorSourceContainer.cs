using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[CreateAssetMenu(fileName = "MapBoxActorSource", menuName = "Festa/MapBox/ActorSource", order = 2)]
	public class UMBActorSourceContainer : ScriptableObject
	{
		[Serializable]
		public class SourceData
		{
			public string id;
			public UMBActor actor_source;
		}

		public List<SourceData> source_list;

		private Dictionary<int, SourceData> _dataDic;

		public SourceData getSource(int id)
		{
			if( _dataDic == null)
			{
				makeDic();
			}

			SourceData data;
			if( _dataDic.TryGetValue(id, out data))
			{
				return data;
			}

			return null;
		}

		public SourceData getSource(string id)
		{
			return getSource(EncryptUtil.makeHashCode(id));
		}

		private void makeDic()
		{
			_dataDic = new Dictionary<int, SourceData>();
			foreach(SourceData data in source_list)
			{
				_dataDic.Add(EncryptUtil.makeHashCode(data.id), data);
			}
		}
	}
}
