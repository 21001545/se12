using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Specialized;
using TMPro;

namespace DRun.Client
{
	public class UIEvent_Item : EnhancedScrollerCellView
	{
		public float height;
		public TMP_Text txtTitle;
		public TMP_Text txtDesc;

		private RefStringCollection sc => GlobalRefDataContainer.getStringCollection();
		private UIEvent.EventData _eventData;

		public void setup(UIEvent.EventData e)
		{
			_eventData = e;

			txtTitle.text = sc.get("event.item.title", e.event_id);
			txtDesc.text = sc.get("event.item.desc", e.event_id);
		}

		public void onClickMore()
		{
			if( _eventData.event_id == 1)
			{
				if (GlobalConfig.isOpenInvitationCodeEvent())
				{
					UIInvitationCode.getInstance().open();
				}
				else
				{
					spawnComingSoon();
				}
			}
		}

		private void spawnComingSoon()
		{
			UIToast.spawn(
					sc.get("popup.coming_soon", 0),
					new(20, -606))
				.setType(UIToastType.error)
				.withTransition<FadePanelTransition>()
				.autoClose(3.0f);
		}
	}
}
