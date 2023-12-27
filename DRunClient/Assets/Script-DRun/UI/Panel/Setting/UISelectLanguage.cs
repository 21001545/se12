using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace DRun.Client
{
	public class UISelectLanguage : UISingletonPanel<UISelectLanguage>
	{
		public UIColorToggleButton btnKorean;
		public UIColorToggleButton btnEnglish;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			base.open(param, transitionType, closeType);
		}

		public void onClick_Close()
		{
			close();
		}

		public void onClick_Korean()
		{
			close();

			changeLanguage(LanguageType.ko);
		}

		public void onClick_English()
		{
			close();

			changeLanguage(LanguageType.en);
		}

		private void changeLanguage(int newLanguageType)
		{
			StringCollection.setCurrentLangType(newLanguageType);

			List<UIRefString> ui_string_list = new List<UIRefString>();
			ClientMain.instance.root.GetComponentsInChildren<UIRefString>(false, ui_string_list);

			foreach(UIRefString ui_string in ui_string_list)
			{
				ui_string.applyText();
			}

			// 저장
			PlayerPrefs.SetInt("languageType", newLanguageType);

			// 동적으로 설정하는 UI들 다시 세팅할 수 있도록 신호를 준다
			List<SingletonBehaviour> singletonList = new List<SingletonBehaviour>();
			ClientMain.instance.root.GetComponentsInChildren<SingletonBehaviour>(false, singletonList);
			foreach(SingletonBehaviour s in singletonList)
			{
				if( s.gameObject.activeSelf == false)
				{
					continue;
				}

				if( s is UIAbstractPanel)
				{
					((UIAbstractPanel)s).onLanguageChanged(newLanguageType);
				}
			}

			//UISetting.getInstance().
		}
	}
}
