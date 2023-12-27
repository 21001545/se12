using DRun.Client.BodyProfileSelectData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UISelectHeight : UISingletonPanel<UISelectHeight>
	{
		public UISnapPicker pickerHigh;
		public UISnapPicker pickerLow;
		public UISnapPicker pickerUnit;

		private int _currentUnit;
		private List<UISnapPickerData> _pickerData_Unit;
		private List<UISnapPickerData> _pickerData_Centi;
		private List<UISnapPickerData> _pickerData_FeetLow;
		private List<UISnapPickerData> _pickerData_FeetHigh;

		private int _initial_cm_height;
		private UnityAction<double> _selectCallback;

		public GameObject backdrop;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			pickerHigh.init();
			pickerLow.init();
			pickerUnit.init();

			_pickerData_Unit = makeUnit();
			_pickerData_Centi = makeCentiLow();
			_pickerData_FeetLow = makeFeetLow();
			_pickerData_FeetHigh = makeFeetHigh();

			pickerHigh.DataList = _pickerData_FeetHigh;
			pickerUnit.DataList = _pickerData_Unit;

			pickerUnit.onSnapped.AddListener(onUnitChanged);
		}

		public void open(double cm_height,UnityAction<double> selectCallback)
		{
			_initial_cm_height = (int)cm_height;
			_selectCallback = selectCallback;

			base.open();
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				setup(_initial_cm_height, UnitDefine.DistanceType.cm);
			}
		}

		private void setup(int cm_height,int unit_type)
		{
			_currentUnit = unit_type;
			setupScrollData(unit_type);
			jumpTo(cm_height, unit_type,true);
		}

		private void setupScrollData(int unit_type)
		{
			if( unit_type == UnitDefine.DistanceType.cm)
			{
				pickerHigh.gameObject.SetActive(false);
				pickerLow.DataList = _pickerData_Centi;
			}
			else if (unit_type == UnitDefine.DistanceType.ft)
			{
				pickerHigh.gameObject.SetActive(true);
				pickerHigh.scroller.ReloadData();
				pickerLow.DataList = _pickerData_FeetLow;
			}
		}

		private List<UISnapPickerData> makeFeetHigh()
		{
			List<UISnapPickerData> list = new List<UISnapPickerData>();

			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			for (int i = 1; i <= 7; ++i)
			{
				list.Add(BodyProfilePickerData.createNumber(i, "'", TMPro.HorizontalAlignmentOptions.Center));
			}

			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			return list;
		}

		private List<UISnapPickerData> makeFeetLow()
		{
			List<UISnapPickerData> list = new List<UISnapPickerData>();

			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			for (int i = 0; i < 12; ++i)
			{
				list.Add(BodyProfilePickerData.createNumber(i, "\"", TMPro.HorizontalAlignmentOptions.Center));
			}

			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			return list;
		}

		private List<UISnapPickerData> makeCentiLow()
		{
			List<UISnapPickerData> list = new List<UISnapPickerData>();

			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			for (int i = 30; i <= 241; ++i)
			{
				list.Add(BodyProfilePickerData.createNumber(i, TMPro.HorizontalAlignmentOptions.Center));
			}

			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			return list;
		}

		private List<UISnapPickerData> makeUnit()
		{
			List<UISnapPickerData> list = new List<UISnapPickerData>();
			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createUnit(UnitDefine.DistanceType.cm, "cm", TMPro.HorizontalAlignmentOptions.Left));
			list.Add(BodyProfilePickerData.createUnit(UnitDefine.DistanceType.ft, "ft", TMPro.HorizontalAlignmentOptions.Left));
			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());
			//list.Add(BodyProfilePickerData.createEmpty());
			return list;
		}

		private void jumpTo(int cm_height, int unit_type,bool isImmediate)
		{
			if( unit_type == UnitDefine.DistanceType.cm)
			{
				pickerLow.jumpByValue(cm_height, isImmediate);
			}
			else
			{
				int high, low;
				cm_to_ft(cm_height, out high, out low);

				pickerHigh.jumpByValue(high, isImmediate);
				pickerLow.jumpByValue(low, isImmediate);
			}

			pickerUnit.jumpByValue(unit_type, isImmediate);
		}

		public static void cm_to_ft(int cm_height,out int high,out int low)
		{
			float feet = (float)cm_height / 30.48f;

			high = Mathf.FloorToInt(feet);
			low = Mathf.FloorToInt(Mathf.Round((feet - Mathf.Floor(feet)) * 12f));
		}

		public static int ft_to_cm(int high,int low)
		{
			return Mathf.RoundToInt((high + low / 12.0f) * 30.48f);
		}

		private int getSelectedHeight()
		{
			if( _currentUnit == UnitDefine.DistanceType.cm)
			{
				return pickerLow.getCurrentValue();
			}
			else
			{
				int high = pickerHigh.getCurrentValue();
				int low = pickerLow.getCurrentValue();

				return ft_to_cm(high, low);
			}
		}

		public void toggleBackdrop(bool isShow = true)
		{
			backdrop.SetActive(isShow);
		}

		public void onUnitChanged(int dataIndex)
		{
			int value = pickerUnit.DataList[dataIndex + pickerUnit.selectOffset].getValue();
			if( value == -1)
			{
				return;
			}

			if( _currentUnit != value)
			{
				int cm_height = getSelectedHeight();
				cm_height = System.Math.Clamp(cm_height, 30, 241);

				_currentUnit = value;
				setupScrollData(_currentUnit);
				jumpTo(cm_height, _currentUnit, false);
			}
		}

		public void onClick_Background()
		{
			close();
		}

		public void onClick_Confirm()
		{
			close();
			_selectCallback((double)getSelectedHeight());
		}
	}
}
