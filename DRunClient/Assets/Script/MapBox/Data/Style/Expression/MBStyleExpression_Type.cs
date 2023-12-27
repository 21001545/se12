using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox.Expression
{
	public class Type_Array : MBStyleExpression
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

	public class Type_Boolean : MBStyleExpression
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

	public class Type_Collator : MBStyleExpression
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

	public class Type_Format : MBStyleExpression
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

	public class Type_Image : MBStyleExpression
	{
		public MBStyleExpression _value;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			object v = _value.evaluate(ctx);
			return v;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_value = parser.parse(json.getValue(1));
		}
	}

	public class Type_Literal : MBStyleExpression
	{
		private object _value;

		public static Type_Literal create(object obj)
		{
			Type_Literal type_Literal = new Type_Literal();
			type_Literal._value = obj;
			return type_Literal;
		}

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return _value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_value = json.getValue(1);
		}
	}

	public class Type_Number : MBStyleExpression
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

	public class Type_NumberFormat : MBStyleExpression
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

	public class Type_Object : MBStyleExpression
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

	public class Type_String : MBStyleExpression
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

	public class Type_ToBoolean : MBStyleExpression
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

	public class Type_ToColor : MBStyleExpression
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

	public class Type_ToNumber : MBStyleExpression
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

	public class Type_ToString : MBStyleExpression
	{
		private MBStyleExpression _value;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			//throw new NotImplementedException();
			return _value.evaluate(ctx).ToString();
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_value = parser.parse(json.getValue(1));
			//throw new NotImplementedException();
		}
	}

	public class Type_Typeof : MBStyleExpression
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
