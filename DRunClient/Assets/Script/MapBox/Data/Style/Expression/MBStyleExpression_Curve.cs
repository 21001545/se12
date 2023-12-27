using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox.Expression
{
	public class Curve_Interpolation : MBStyleExpression
	{
		private int _type;
		private double _exponetial_base;
		private MBStyleExpression _input;
		private List<double> _keyList;
		private List<MBStyleExpression> _valueList;
		private MBUnitBezier _ub;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double input_value = _input.evaluateDouble(ctx);

			for(int i = 0; i < _keyList.Count; ++i)
			{
				double key = (double)_keyList[i];
				if( input_value <= key)
				{
					if( i == 0)
					{
						return _valueList[i].evaluate(ctx);
					}
					else
					{
						double prev_key = (double)_keyList[i - 1];
						object prev_value = _valueList[i - 1].evaluate(ctx);
						object value = _valueList[i].evaluate(ctx);

						double t;

						if (_type == MBStyleDefine.InterpolationType.cube_bezier)
						{
							t = MapBoxUtil.exponentialInterpolation(input_value, 1.0, prev_key, key);
							_ub.solve(t, 1e-6);
						}
						else
						{
							t = MapBoxUtil.exponentialInterpolation(input_value, _exponetial_base, prev_key, key);
						}

						if ( value is Color)
						{
							Color prev_color = (Color)prev_value;
							Color next_color = (Color)value;

							return Color.Lerp(prev_color, next_color, (float)t);
						}
						else
						{
							double prev_double = castDouble(prev_value);
							double next_double = castDouble(value);

							return MapBoxUtil.lerp(prev_double, next_double, t);
						}
					}
				}
			}

			return _valueList[_valueList.Count - 1].evaluate(ctx);
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			JsonArray type_data = json.getJsonArray(1);
			if (type_data.getString(0) == "linear")
			{
				_type = MBStyleDefine.InterpolationType.linear;
				_exponetial_base = 1.0;
			}
			else if (type_data.getString(0) == "exponential")
			{
				_type = MBStyleDefine.InterpolationType.exponential;
				_exponetial_base = type_data.getDouble(1);
			}
			else if (type_data.getString(0) == "cubic-bezier")
			{
				_type = MBStyleDefine.InterpolationType.cube_bezier;
				_ub = new MBUnitBezier( type_data.getDouble(1),
										type_data.getDouble(2),
										type_data.getDouble(3),
										type_data.getDouble(4));
			}

			_input = parser.parse(json.getValue(2));

			_keyList = new List<double>();
			_valueList = new List<MBStyleExpression>();

			int key_count = (json.size() - 3) / 2;
			for(int i = 0; i < key_count; ++i)
			{
				double key = json.getDouble(3 + i * 2 + 0);
				object value = json.getValue(3 + i * 2 + 1);

				_keyList.Add(key);
				_valueList.Add(parser.parse(value));
			}
		}
	}

	public class Curve_InterpolationHCL : MBStyleExpression
	{
		public override object evaluate(MBStyleExpressionContext ctx)
		{
			throw new NotImplementedException();
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			throw new NotImplementedException();
		}
	}

	public class Curve_InterpolationLAB : MBStyleExpression
	{
		public override object evaluate(MBStyleExpressionContext ctx)
		{
			throw new NotImplementedException();
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			throw new NotImplementedException();
		}
	}

	public class Curve_Step : MBStyleExpression
	{
		private MBStyleExpression _input;
		private MBStyleExpression _step0_output;
		private List<double> _keyList;
		private List<MBStyleExpression> _valueList;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double input_value = castDouble(_input.evaluate(ctx));
			MBStyleExpression result_ex = _step0_output;

			for (int i = 0; i < _keyList.Count; ++i)
			{
				if( input_value >= _keyList[ i])
				{
					result_ex = _valueList[i];
				}
			}

			return result_ex.evaluate(ctx);
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_input = parser.parse(json.getValue(1));
			_step0_output = parser.parse(json.getValue(2));

			_keyList = new List<double>();
			_valueList = new List<MBStyleExpression>();

			int key_count = (json.size() - 3) / 2;
			for(int i = 0; i < key_count; ++i)
			{
				double key = json.getDouble(3 + i * 2 + 0);
				object value = json.getValue(3 + i * 2 + 1);

				_keyList.Add(key);
				_valueList.Add(parser.parse(value));
			}
		}
	}
}
