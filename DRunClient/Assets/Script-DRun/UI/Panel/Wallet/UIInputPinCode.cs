using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIInputPinCode : MonoBehaviour
	{
		public CanvasGroup canvasGroup;
		public Image[] pinIconList;
		public GameObject btnBack;

		private List<int> _codeList = new List<int>();
		public List<int> CodeList => _codeList;

		public bool Interactable
		{
			get
			{
				return canvasGroup.interactable;
			}
			set
			{
				canvasGroup.interactable = value;
			}
		}

		[Serializable]
		public class ChangeCodeListEvent : UnityEvent { };

		public ChangeCodeListEvent onChanged = new ChangeCodeListEvent();

		public void reset()
		{
			_codeList.Clear();
			updatePinIconList();
		}

		public void onClick_Number(int number)
		{
			if( _codeList.Count >= 6)
			{
				return;
			}

			_codeList.Add(number);
			updatePinIconList();

			onChanged.Invoke();
		}

		public void onClick_Back()
		{
			if( _codeList.Count < 1)
			{
				return;
			}

			_codeList.RemoveAt(_codeList.Count - 1);
			updatePinIconList();

			onChanged.Invoke();
		}

		private void updatePinIconList()
		{
			for(int i = 0; i < pinIconList.Length; ++i)
			{
				Color color = UIStyleDefine.ColorStyle.gray300;
				if ( i < _codeList.Count)
				{
					color.a = 1.0f;
				}
				else
				{
					color.a = 0.5f;
				}

				pinIconList[i].color = color;
			}

			btnBack.SetActive(_codeList.Count > 0);
		}

	}
}
