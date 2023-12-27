using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using System.Globalization;
using System.Text;
using TMPro;

namespace DRun.Client
{
	public class UIGPSDebugger_Log : EnhancedScrollerCellView
	{
		public TMP_Text text;

		public void setup(ClientLocationLog log)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(log.event_time.ToString("yyyy-MM-dd HH:mm:ss.f",DateTimeFormatInfo.InvariantInfo));
			sb.Append("\n");
			sb.Append($"lon[{log.longitude}] lat[{log.latitude}] alt[{log.altitude}] acc[{log.accuracy}]");
			sb.Append("\n");
			sb.Append($"speed[{log.speed}] speed_acc[{log.speed_accuracy}]");
			text.text = sb.ToString();
		}
	}
}
