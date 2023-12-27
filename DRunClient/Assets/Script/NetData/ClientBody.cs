using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientBody
	{
		public int gender;
		public double height;
		public double weight;
		public double stride;
		public int height_unit_type;
		public int weight_unit_type;
		public int stride_unit_type;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public static class Gender
		{
			public static int unknown = 0;
			public static int male = 1;
			public static int female = 2;
			public static int netural = 3;  // ????
		}

		public string getWeightDisplayString()
		{
			if( weight_unit_type == UnitDefine.WeightType.unknown)
			{
				return "";
			}

			return StringCollection.getFormat("setting.measure_display.weight", weight_unit_type, weight);
		}

		public string getHeightDisplayString()
		{
			if( height_unit_type == UnitDefine.DistanceType.unknown)
			{
				return "";
			}

			UnityEngine.Debug.Log($"saved height is {height}");

			if( height_unit_type == UnitDefine.DistanceType.cm)
			{
				return StringCollection.getFormat("setting.measure_display.height", height_unit_type, UnityEngine.Mathf.CeilToInt((float)height));
			}
			else if( height_unit_type == UnitDefine.DistanceType.ft)
			{
				return StringCollection.getFormat("setting.measure_display.height", height_unit_type, UnityEngine.Mathf.FloorToInt((float)height), UnityEngine.Mathf.CeilToInt((float)(height % 1.0) * 100));
			}

			return "N/A";
		}

		// 칼로리 계산을 위해 kg단위 몸무게가 필요
		public double getWeightWithKG()
		{
			if( weight_unit_type == UnitDefine.WeightType.kg)
			{
				return weight;
			}
			else if( weight_unit_type == UnitDefine.WeightType.lb)
			{
				return weight / 2.205f;
			}
			else if( weight_unit_type == UnitDefine.WeightType.st)
			{
				return weight / 14.0f;
			}

			return weight;
		}
	}
}
