using DRun.Client.Module;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.RefData;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Festa.Client.CSMessageID;

namespace DRun.Client
{
	public class UIWalletSpending_Log : UIWalletScrollerCellView
	{
		public Image image_status;
		public TMP_Text text_status;
		public Image asset_icon;
		public TMP_Text delta;
		public TMP_Text time;

		public Color[] statusColor;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public void setup(ClientDRNTransaction data)
		{
			if( data.step == ClientDRNTransaction.Step.done)
			{
				image_status.color = statusColor[0];
				text_status.text = StringCollection.get("wallet.log.status.complete", 0);
			}
			else
			{
				image_status.color = statusColor[1];
				text_status.text = StringCollection.get("wallet.log.status.inprogress", 0);
			}

			if( data.delta > 0)
			{
				delta.text = "+" + StringUtil.toDRNStringDefault(data.delta);
			}
			else
			{
				delta.text = StringUtil.toDRNStringDefault(data.delta);
			}

			time.text = StringUtil.toRecordTime(data.update_time.ToLocalTime());
		}
	}
}
