//using EnhancedUI.EnhancedScroller;
//using PhoneNumbers;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//#if UNITY_EDITOR

//public class LibPhoneNumberTest : MonoBehaviour
//{
//    public TMP_InputField inputField;
//	public TMP_Dropdown dropDownCountry;

//	public EnhancedScroller scroller;
//	public EnhancedScrollerCellView itemSource;

//	private AsYouTypeFormatter _formatter;

//	private void Start()
//	{
//		var util = PhoneNumberUtil.GetInstance();
//		_formatter = util.GetAsYouTypeFormatter("KR");

//		foreach(string regionCode in util.GetSupportedRegions())
//		{
//			int country_code = util.GetCountryCodeForRegion(regionCode);
//			Debug.Log($"{regionCode} : {country_code}");
//		}

//		scroller.ReloadData();
//	}

//	public void onValueChanged(string value)
//	{
//		_formatter.Clear();

//		value = inputField.text;
//		string result = "";

//		for(int i = 0; i < value.Length; ++i)
//		{
//			char ch = value[i];
//			if( char.IsNumber(ch) == false)
//			{
//				continue;
//			}
//			result = _formatter.InputDigit(ch);
//		}

//		inputField.text = result;
//		inputField.stringPosition = result.Length;
//	}
//}

//#endif