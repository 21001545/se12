using DRun.Client.BodyProfileSelectData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UISelectWeight : UISingletonPanel<UISelectWeight>
	{
		public UISnapPicker pickerInteger;
		public UISnapPicker pickerDecimal;
		public UISnapPicker pickerUnit;

		private int _currentUnit;
		private List<UISnapPickerData> _integerData_KG;
		private List<UISnapPickerData> _integerData_LB;
		private List<UISnapPickerData> _integerData_ST;
		private List<UISnapPickerData> _decimalData;
		private List<UISnapPickerData> _unitData;

		private double _initial_kg_weight;
		private UnityAction<double> _selectCallback;

		public GameObject backdrop;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			pickerInteger.init();
			pickerDecimal.init();
			pickerUnit.init();

			_integerData_KG = makeInteger(0, 356);
			_integerData_LB = makeInteger(0, 784);
			_integerData_ST = makeInteger(0, 60);
			_decimalData = makeInteger(0, 9);
			_unitData = makeUnit();

			pickerDecimal.DataList = _decimalData;
			pickerUnit.DataList = _unitData;

			pickerUnit.onSnapped.AddListener(onUnitChanged);
		}

		public void open(double kg_weight,UnityAction<double> selectCallback)
		{
			_initial_kg_weight = kg_weight;
			_selectCallback = selectCallback;

			base.open();
		}


		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				setup(_initial_kg_weight, UnitDefine.WeightType.kg);
			}
		}

		private void setup(double kg_weight,int unit_type)
		{
			_currentUnit = unit_type;
			setupScrollData(unit_type);
			jumpTo(kg_weight, unit_type, true);
		}

		private void setupScrollData(int unit_type)
		{
			if (unit_type == UnitDefine.WeightType.kg)
			{
				pickerInteger.DataList = _integerData_KG;
			}
			else if (unit_type == UnitDefine.WeightType.lb)
			{
				pickerInteger.DataList = _integerData_LB;
			}
			else if (unit_type == UnitDefine.WeightType.st)
			{
				pickerInteger.DataList = _integerData_ST;
			}
		}

		private void jumpTo(double kg_weight,int unit_type,bool isImmeidiate)
		{
			double value = kg_weight;
			if( unit_type == UnitDefine.WeightType.lb)
			{
				value = kg_to_lb( kg_weight);
			}
			else if( unit_type == UnitDefine.WeightType.st)
			{
				value = kg_to_st(kg_weight);
			}
			
			int high = (int)value;
			int low = (int)((value * 10) % 10);

			pickerInteger.jumpByValue(high, isImmeidiate);
			pickerDecimal.jumpByValue(low, isImmeidiate);
			pickerUnit.jumpByValue(unit_type, isImmeidiate);
		}

		private double kg_to_lb(double value)
		{
			return value * 2.205;
		}

		private double kg_to_st(double value)
		{
			return value / 6.35;
		}

		private double lb_to_kg(double value)
		{
			return value / 2.205;
		}

		private double st_to_kg(double value)
		{
			return value * 6.35;
		}

		private double getSelectedWeight()
		{
			int high = pickerInteger.getCurrentValue();
			int low = pickerDecimal.getCurrentValue();

			double value = high + low * 0.1;

			if( _currentUnit == UnitDefine.WeightType.lb)
			{
				return lb_to_kg(value);
			}
			else if( _currentUnit == UnitDefine.WeightType.st)
			{
				return st_to_kg(value);
			}

			return value;
		}

		public void toggleBackdrop(bool isShow = true)
		{
			backdrop.SetActive(isShow);
		}

		public void onUnitChanged(int dataIndex)
		{
			int value = pickerUnit.getCurrentValue();
			if( value == -1)
			{
				return;
			}

			if(_currentUnit == value)
			{
				return;
			}

			double kg_weight = getSelectedWeight();
			_currentUnit = value;

			setupScrollData(_currentUnit);
			jumpTo(kg_weight, _currentUnit, false);
		}

		private List<UISnapPickerData> makeInteger(int begin,int end)
		{
			List<UISnapPickerData> list = new List<UISnapPickerData>();
			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());

			for (int i = begin; i <= end; ++i)
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
			list.Add(BodyProfilePickerData.createUnit(UnitDefine.WeightType.kg, "kg", TMPro.HorizontalAlignmentOptions.Left));
			list.Add(BodyProfilePickerData.createUnit(UnitDefine.WeightType.lb, "lb", TMPro.HorizontalAlignmentOptions.Left));
			list.Add(BodyProfilePickerData.createUnit(UnitDefine.WeightType.st, "st", TMPro.HorizontalAlignmentOptions.Left));
			list.Add(BodyProfilePickerData.createEmpty());
			list.Add(BodyProfilePickerData.createEmpty());
			return list;
		}

		public void onClick_Background()
		{
			close();
		}

		public void onClick_Confirm()
		{
			close();
			_selectCallback(getSelectedWeight());
		}
	}
}
