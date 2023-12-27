using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIContactFriend_CellView : EnhancedScrollerCellView
	{
        public TMP_Text txt_shotName;
        public TMP_Text txt_name;
		public TMP_Text txt_message;
		public Button btn_follow;

		private string phoneNumber;
		private UnityAction<UIContactFriend_CellView> _clickEvent;

		public string getPhoneNumber() => phoneNumber;
		
		public void setup(string phoneNumber, string name, UnityAction<UIContactFriend_CellView> clickEvent)
		{
			this.phoneNumber = phoneNumber;
			if( name.Length > 0)
			{
				txt_shotName.text = name.Substring(0, 1).ToUpper();
			}
			else
			{
				txt_shotName.text = "";
			}

			txt_name.text = name;
			txt_message.text = "0 friends in Actively";
			_clickEvent = clickEvent;
        }

        public void onClickInvite()
		{
			_clickEvent(this);
		}

	}
}
