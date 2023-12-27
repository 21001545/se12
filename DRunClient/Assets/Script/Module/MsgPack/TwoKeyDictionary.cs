using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module.MsgPack
{
	public class TwoKeyDictionary<K1,K2,V1> : Dictionary<K1, Dictionary<K2, V1>>
	{
		public void put(K1 k1,K2 k2,V1 v)
		{
			Dictionary<K2, V1> sub_dic;
			if ( TryGetValue(k1, out sub_dic) == false)
			{
				sub_dic = new Dictionary<K2, V1>();

				Add(k1, sub_dic);
			}

			sub_dic.Add(k2, v);
		}

		public V1 get(K1 k1,K2 k2)
		{
			Dictionary<K2, V1> sub_dic;
			if( TryGetValue( k1, out sub_dic) == false)
			{
				return default(V1);
			}

			V1 v;
			if( sub_dic.TryGetValue( k2, out v) == false)
			{
				return default(V1);
			}

			return v;
		}
	}

}
