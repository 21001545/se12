using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UIRanking_SelectMarathonType : MonoBehaviour
	{
		public TMP_Text[] options;

		private int _currentType;
		private UnityAction<int> _callback;

		public void open(int currentType,UnityAction<int> callback)
		{
			gameObject.SetActive(true);

			_currentType = currentType;
			_callback = callback;
			for(int i = 0; i < options.Length; ++i)
			{
				TMP_Text text = options[i];
				bool selected = (i + 1) == _currentType;

				if (selected)
				{
					text.color = UIStyleDefine.ColorStyle.gray200;
				}
				else
				{
					text.color = UIStyleDefine.ColorStyle.gray500;
				}
			}
		}

		public void close()
		{
			gameObject.SetActive(false);
		}

		public void onClick_Type(int type)
		{
			close();
			_callback(type);
		}

		public void onClick_Back()
		{
			close();
			_callback(_currentType);
		}
	}
}
