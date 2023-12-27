using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIMainTab_Item : MonoBehaviour
	{
		public Image image_normal;
		public Image image_selected;

		public void setSelection(bool b)
		{
			if( image_selected == false)
			{
				image_normal.gameObject.SetActive(true);
			}
			else
			{
				image_normal.gameObject.SetActive(b == false);
				image_selected.gameObject.SetActive(b == true);
			}
		}
	}
}
