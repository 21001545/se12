using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using System.Text;
using TMPro;

namespace DRun.Client
{
	public class UIStepDebugger_Log : EnhancedScrollerCellView
	{
		public TMP_Text text;

		public void setup(ClientHealthLogData log)
		{
			text.text = $"{log.record_time.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.f")} : {log.value}";
		}
	}
}
