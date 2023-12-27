using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBStyleFilter
	{
		public static MBStyleFilter create(JsonArray json)
		{
			MBStyleFilter filter = new MBStyleFilter();
			filter.init(json);
			return filter;
		}

		private void init(JsonArray json)
		{
			string type = json.getString(0);

			//Debug.Log(string.Format("filter type:{0}", type));
		}
	}
}
