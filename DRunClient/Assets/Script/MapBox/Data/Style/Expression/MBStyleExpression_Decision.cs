using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox.Expression
{
	public class Decision_Not : MBStyleExpression
	{
		private MBStyleExpression _value;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return (bool)_value.evaluate(ctx) == false;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_value = parser.parse(json.getValue(1));
		}
	}

	public class Decision_NotEqual : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			object a_value = (object)_a.evaluate(ctx);
			object b_value = (object)_b.evaluate(ctx);
			return isEqual(a_value,b_value) == false;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getValue(1));
			_b = parser.parse(json.getValue(2));
		}
	}

	public class Decision_Less : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double a_value = _a.evaluateDouble(ctx);
			double b_value = _b.evaluateDouble(ctx);
			return a_value < b_value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getValue(1));
			_b = parser.parse(json.getValue(2));
		}
	}

	public class Decision_LessEqual : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double a_value = _a.evaluateDouble(ctx);
			double b_value = _b.evaluateDouble(ctx);
			return a_value <= b_value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getValue(1));
			_b = parser.parse(json.getValue(2));
		}
	}

	public class Decision_Equal : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			object result_a = _a.evaluate(ctx);
			object result_b = _b.evaluate(ctx);
			return isEqual(result_a, result_b);
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getList()[1]);
			_b = parser.parse(json.getList()[2]);
		}
	}

	public class Decision_Greater : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double a_value = _a.evaluateDouble(ctx);
			double b_value = _b.evaluateDouble(ctx);
			return a_value > b_value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getList()[1]);
			_b = parser.parse(json.getList()[2]);
		}
	}

	public class Decision_Greater_Equal : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double a_value = _a.evaluateDouble(ctx);
			double b_value = _b.evaluateDouble(ctx);
			return a_value >= b_value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getList()[1]);
			_b = parser.parse(json.getList()[2]);
		}
	}

	public class Decision_All : MBStyleExpression
	{
		private List<MBStyleExpression> _checkList;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			for(int i = 0; i < _checkList.Count; ++i)
			{
				bool result = (bool)_checkList[i].evaluate(ctx);
				if( result == false)
				{
					return false;
				}
			}

			return true;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_checkList = new List<MBStyleExpression>();

			for(int i = 1; i < json.size(); ++i)
			{
				_checkList.Add(parser.parse(json.getValue(i)));
			}
		}
	}

	public class Decision_Any : MBStyleExpression
	{
		private List<MBStyleExpression> _conditionList;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			foreach(MBStyleExpression condition in _conditionList)
			{
				if((bool)condition.evaluate(ctx) == true)
				{
					return true;
				}
			}

			return false;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_conditionList = new List<MBStyleExpression>();
			for(int i = 1; i < json.size(); ++i)
			{
				_conditionList.Add(parser.parse(json.getValue(i)));
			}
		}
	}

	public class Decision_Case : MBStyleExpression
	{
		private List<MBStyleExpression> _conditionList;
		private List<MBStyleExpression> _outputList;
		private MBStyleExpression _fallback;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			for (int i = 0; i < _conditionList.Count; ++i)
			{
				MBStyleExpression condition = _conditionList[i];
				MBStyleExpression output = _outputList[i];

				if ((bool)condition.evaluate(ctx) == true)
				{
					return output.evaluate(ctx);
				}
			}

			return _fallback.evaluate(ctx);
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_conditionList = new List<MBStyleExpression>();
			_outputList = new List<MBStyleExpression>();
			_fallback = null;

			for(int i = 1; i < json.size();)
			{
				if( i + 1 <= json.size() - 1)
				{
					MBStyleExpression condition = parser.parse(json.getValue(i));
					MBStyleExpression output = parser.parse(json.getValue(i + 1));

					_conditionList.Add(condition);
					_outputList.Add(output);

					i += 2;
				}
				else
				{
					_fallback = parser.parse(json.getValue(i));
					i++;
				}
			}
		}
	}

	public class Decision_Coalesce : MBStyleExpression
	{
		private List<MBStyleExpression> _valueList;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			for(int i = 0; i < _valueList.Count; ++i)
			{
				MBStyleExpression ex = _valueList[i];
				object v = ex.evaluate(ctx);
				if( v != null)
				{
					return v;
				}
			}

			return null;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_valueList = new List<MBStyleExpression>();
			
			for(int i = 1; i < json.size(); ++i)
			{
				_valueList.Add(parser.parse(json.getValue(i)));
			}
		}
	}

	public class Decision_Match : MBStyleExpression
	{
		public class MatchPair
		{
			public List<MBStyleExpression> conditionList;
			public MBStyleExpression output;
		}


		private MBStyleExpression _input;
		private List<MatchPair> _matchList;
		private MBStyleExpression _fallback;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			object input_value = _input.evaluate(ctx);

			if( input_value != null)
			{
				foreach (MatchPair pair in _matchList)
				{
					bool matched = false;
					foreach (MBStyleExpression ex in pair.conditionList)
					{
						object condition = ex.evaluate(ctx);
						//if (condition != null && input_value.Equals(condition))
						if( condition != null && isEqual( input_value, condition))
						{
							matched = true;
							break;
						}
					}

					if (matched)
					{
						return pair.output.evaluate(ctx);
					}
				}
			}

			return _fallback.evaluate(ctx);
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_input = parser.parse(json.getValue(1));
			_matchList = new List<MatchPair>();

			for (int i = 2; i < json.size();)
			{
				object condition = json.getValue(i);

				if( i == json.size() - 1)
				{
					_fallback = parser.parse(condition);
					++i;
				}
				else if( i + 1 <= (json.size() - 1))
				{
					object output = json.getValue(i + 1);

					MatchPair pair = new MatchPair();
					pair.conditionList = new List<MBStyleExpression>();
					pair.output = parser.parse(output);

					if( condition is IList<object>)
					{
						foreach(object c_item in (IList<object>)condition)
						{
							pair.conditionList.Add(parser.parse(c_item));
						}
					}
					else if( condition is JsonArray)
					{
						JsonArray condition_array = (JsonArray)condition;
						for(int j = 0; j < condition_array.size(); ++j)
						{
							object c_item = condition_array.getValue(j);
							pair.conditionList.Add(parser.parse(c_item));
						}
					}
					else
					{
						pair.conditionList.Add(parser.parse(condition));
					}

					_matchList.Add(pair);
					i += 2;
				}
			}
		}
	}

	public class Decision_Within : MBStyleExpression
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
}
