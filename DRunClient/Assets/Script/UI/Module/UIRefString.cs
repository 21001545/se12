using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Festa.Client.RefData;
using Festa.Client.Module;

namespace Festa.Client
{ 
	[RequireComponent(typeof(TMP_Text))]
	public class UIRefString : MonoBehaviour
	{
		public string type;
		public int id;

		private int _type_hashed = -1;
		private TMP_Text _tmp_text;
		private RefStringCollection sc => GlobalRefDataContainer.getStringCollection();

		private char[] charsToTrim = { ' ', '\n', '\r' };
		
		public void applyText()
		{
			if( sc == null || string.IsNullOrEmpty(type))
			{
				return;
			}

			if( _tmp_text == null)
			{
				_tmp_text = GetComponent<TMP_Text>();
			}

			if( _type_hashed == -1)
			{
				_type_hashed = EncryptUtil.makeHashCode(type.Trim(charsToTrim));
			}

			
			_tmp_text.text = sc.get(type.Trim(charsToTrim), id);
		}

		// 좀더 고민해보자
		void OnEnable()
		{
			applyText();
		}

		public void changeText(string type,int id)
		{
			this.type = type;
			this.id = id;
			applyText();
		}
	}
}
