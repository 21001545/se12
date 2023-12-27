using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIJoinParty : UIPanel
	{
		public RawImage image;
		private UnityAction _clickOK;
		private UnityAction _clickOther;

		public void clickJoin()
		{
			close();

			if( _clickOK != null)
			{
				_clickOK();
			}
		}

		public void clickCancel()
		{
			close();

			if( _clickOther != null)
			{
				_clickOther();
			}
		}

		public static UIJoinParty spawn(Texture2D image, UnityAction clickOK = null,UnityAction clickOther = null)
		{
			UIJoinParty ui = UIManager.getInstance().spawnInstantPanel<UIJoinParty>();
			ui.image.texture = image;
			ui._clickOK = clickOK;
			ui._clickOther = clickOther;
			return ui;
		}
	}
}
