using EnhancedScrollerDemos.CellEvents;
using TMPro;
using UnityEngine.UIElements;

namespace DRun.Client.BodyProfileSelectData
{
	public class BodyProfilePickerData : UISnapPickerData
	{
		public int value;
		public string text;
		public HorizontalAlignmentOptions alignment;

		public override int getValue()
		{
			return value;
		}

		public override string getText()
		{
			return text;
		}

		public override HorizontalAlignmentOptions getTextHorizontalAlign()
		{
			return alignment;
		}

		public static BodyProfilePickerData createUnit(int unit_type,string text,HorizontalAlignmentOptions align)
		{
			BodyProfilePickerData data = new BodyProfilePickerData();
			data.value = unit_type;
			data.text = text;
			data.alignment = align;
			return data;
		}

		public static BodyProfilePickerData createNumber(int value, HorizontalAlignmentOptions align)
		{
			BodyProfilePickerData data = new BodyProfilePickerData();
			data.value = value;
			data.text = value.ToString();
			data.alignment = align;
			return data;
		}
	
		public static BodyProfilePickerData createNumber(int value,string postfix, HorizontalAlignmentOptions align)
		{
			BodyProfilePickerData data = new BodyProfilePickerData();
			data.value = value;
			data.text = $"{value}{postfix}";
			data.alignment = align;
			return data;
		}
	
		public static BodyProfilePickerData createEmpty()
		{
			BodyProfilePickerData data = new BodyProfilePickerData();
			data.value = -1;
			data.text = "";
			data.alignment = HorizontalAlignmentOptions.Center;
			return data;
		}
	}

}
