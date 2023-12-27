//using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	[Serializable]
	[CreateAssetMenu(fileName = "UIPhoneNumberInputValidator.asset", menuName = "Festa/UI/PhoneNumberInputValidator", order = 100)]
	public class UIPhoneNumberInputValidator : TMP_InputValidator
	{
		//AsYouTypeFormatter typeFormatter;

		public override char Validate(ref string text, ref int pos, char ch)
		{
			return ch;
			//Debug.Log($"text:{text} pos:{pos} ch:{ch}");

			//if (ch < '0' || ch > '9') 
			//	return (char)0;

			//if(typeFormatter == null)
			//{
			//	var util = PhoneNumberUtil.GetInstance();
			//	typeFormatter = util.GetAsYouTypeFormatter("KR");
			//}

			//text = typeFormatter.InputDigit(ch);
			//pos = text.Length;
			//return ch;
		}
	}
}
