using Festa.Client.Module;
using System;
using System.Collections.Generic;

namespace Festa.Client.MapBox.Expression
{
	public class Math_Minus : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			if( _b == null)
			{
				return -_a.evaluateDouble(ctx);
			}
			else
			{
				return _a.evaluateDouble(ctx) - _b.evaluateDouble(ctx);
			}
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getValue(1));

			if(json.size() == 3)
			{
				_b = parser.parse(json.getValue(2));
			}
			else
			{
				_b = null;
			}
		}
	}

	public class Math_Multiply : MBStyleExpression
	{
		private List<MBStyleExpression> _numberList;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double product = _numberList[0].evaluateDouble(ctx);
			for(int i = 1; i < _numberList.Count; ++i)
			{
				product *= _numberList[i].evaluateDouble(ctx);
			}

			return product;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_numberList = new List<MBStyleExpression>();
			for(int i = 1; i < json.size(); ++i)
			{
				_numberList.Add(parser.parse(json.getValue(i)));
			}
		}
	}

	public class Math_Divide : MBStyleExpression
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

	public class Math_Mod : MBStyleExpression
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

	public class Math_Pow : MBStyleExpression
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

	public class Math_Plus : MBStyleExpression
	{
		private MBStyleExpression _a;
		private MBStyleExpression _b;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			double a = (double)_a.evaluate(ctx);
			double b = (double)_b.evaluate(ctx);

			return a + b;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_a = parser.parse(json.getValue(1));
			_b = parser.parse(json.getValue(2));
		}
	}

	public class Math_Abs : MBStyleExpression
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

	public class Math_ACos : MBStyleExpression
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

	public class Math_ASin : MBStyleExpression
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

	public class Math_Atan : MBStyleExpression
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

	public class Math_Ceil : MBStyleExpression
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

	public class Math_Cos : MBStyleExpression
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

	public class Math_Distance : MBStyleExpression
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

	public class Math_E : MBStyleExpression
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

	public class Math_Floor : MBStyleExpression
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

	public class Math_Ln : MBStyleExpression
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

	public class Math_Ln2 : MBStyleExpression
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

	public class Math_Log10 : MBStyleExpression
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

	public class Math_Log2 : MBStyleExpression
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

	public class Math_Max : MBStyleExpression
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

	public class Math_Min : MBStyleExpression
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

	public class Math_Pi : MBStyleExpression
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

	public class Math_Round : MBStyleExpression
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

	public class Math_Sin : MBStyleExpression
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

	public class Math_Sqrt : MBStyleExpression
	{
		MBStyleExpression _value;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return System.Math.Sqrt(_value.evaluateDouble(ctx));
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_value = parser.parse(json.getValue(1));
		}
	}

	public class Math_Tan : MBStyleExpression
	{
		MBStyleExpression _value;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return Math.Tan(_value.evaluateDouble(ctx));
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_value = parser.parse(json.getValue(1));
		}
	}

}
