using DRun.Client.NetData;
using System.Collections.Generic;
using TMPro;

namespace DRun.Client.Record
{
	public class GraphValue
	{
		public double value;
		public bool current;
		public bool empty;

		public ClientRunningLogCumulation logData;

		public static GraphValue create(double value, bool current,bool empty,ClientRunningLogCumulation logData)
		{
			GraphValue data = new GraphValue();
			data.value = value;
			data.current = current;
			data.empty = empty;
			data.logData = logData;
			return data;
		}
	}

	public class GraphData
	{
		public List<string> yAxisList = new List<string>();
		public List<string> xAxisList = new List<string>();

		public float xAxisLabel_FontSize;
		public float xAxisLabel_Margin;
		public TextAlignmentOptions xAxisLabel_Alignment;
		public float valueWidth;

		public double valueMin;
		public double valueMax;

		public List<GraphValue> valueList = new List<GraphValue>();
	}
}
