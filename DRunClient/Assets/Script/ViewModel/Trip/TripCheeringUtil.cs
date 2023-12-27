using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class TripCheeringUtil
	{
		public static void saveLatestCheeringID(string base_url,int id)
		{
			string key = $"TripLatestCheeringID_{EncryptUtil.makeHashCodePositive(base_url)}";

			PlayerPrefs.SetInt(key, id);
		}

		public static int loadLatestCheeringID(string base_url)
		{
			string key = $"TripLatestCheeringID_{EncryptUtil.makeHashCodePositive(base_url)}";

			return PlayerPrefs.GetInt(key, 0);
		}
	}
}
