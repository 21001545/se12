using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBLayer
	{
		public string name;
		public List<int> keyList;
		public List<object> valueList;
		public List<MBFeature> featureList;

		[SerializeOption(SerializeOption.NONE)]
		public MBTile _tile;

		//[SerializeOption(SerializeOption.NONE)]
		//public MBMesh _mergedMesh;

		//[SerializeOption(SerializeOption.NONE)]
		//public List<MBFeature> _outlinePolygonList;

		[SerializeOption(SerializeOption.NONE)]
		public List<int> hashedValueList;

		public void buildHasedValueList()
		{
			hashedValueList = new List<int>();
			foreach(object obj in valueList)
			{
				if( obj is string)
				{
					hashedValueList.Add(EncryptUtil.makeHashCode(obj as string));
				}
				else
				{
					hashedValueList.Add(0);
				}
			}
		}
	}
}
