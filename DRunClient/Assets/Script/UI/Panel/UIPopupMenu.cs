using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
    public class UIPopupMenu : UIPanel
    {
        [SerializeField]
        Button btn_secondBtn;
        [SerializeField]
        Button btn_thirdBtn;

        [SerializeField]
		private TMP_Text txt_firstBtn;
		[SerializeField]
		private TMP_Text txt_secondBtn;
		[SerializeField]
		private TMP_Text txt_thirdBtn;

		private UnityAction callbackFirst;
		private UnityAction callbackSecond;
		private UnityAction callbackThird;

		private void setSecondThirdOn(bool second, bool third)
        {
			btn_secondBtn.gameObject.SetActive(second);
			btn_thirdBtn.gameObject.SetActive(third);
        }

		public void onClickFirst()
        {
			close();
			callbackFirst();
        }

		public void onClickSecond()
        {
			close();
			callbackSecond();
        }	
		
		public void onClickThird()
        {
			close();
			callbackThird();
        }

		public static UIPopupMenu spawnOneMenu(string firstName, UnityAction firstClick)
		{
			UIPopupMenu popup = UIManager.getInstance().spawnInstantPanel<UIPopupMenu>();
			popup.setSecondThirdOn(false, false);
			popup.txt_firstBtn.text = firstName;
			popup.callbackFirst = firstClick;

			return popup;
		}

		public static UIPopupMenu spawnTwoMenu(string firstName, string secondName, UnityAction firstClick, UnityAction secondClick)
		{
			UIPopupMenu popup = UIManager.getInstance().spawnInstantPanel<UIPopupMenu>();
			popup.setSecondThirdOn(true, false);
			popup.txt_firstBtn.text = firstName;
			popup.txt_secondBtn.text = secondName;
			popup.callbackFirst = firstClick;
			popup.callbackSecond = secondClick;

			return popup;
		}

		public static UIPopupMenu spawnThreeMenu(string firstName, string secondName, string thirdName, UnityAction firstClick, UnityAction secondClick, UnityAction thirdClick)
		{
			UIPopupMenu popup = UIManager.getInstance().spawnInstantPanel<UIPopupMenu>();
			popup.setSecondThirdOn(true, true);
			popup.txt_firstBtn.text = firstName;
			popup.txt_secondBtn.text = secondName;
			popup.txt_thirdBtn.text = thirdName;
			popup.callbackFirst = firstClick;
			popup.callbackSecond = secondClick;
			popup.callbackThird = thirdClick;

			return popup;
		}
	}
}