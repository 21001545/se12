using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox.Expression
{
	public class String_Concat : MBStyleExpression
	{
		private List<MBStyleExpression> _valueList;
		private StringBuilder _sb = new StringBuilder();

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			_sb.Clear();

			for(int i = 0; i < _valueList.Count; ++i)
			{
				_sb.Append( castString(_valueList[i].evaluate(ctx)));
			}
			return _sb.ToString();
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_valueList = new List<MBStyleExpression>();

			for(int i = 1; i < json.size(); ++i)
			{
				MBStyleExpression value = parser.parse(json.getValue(i));
				_valueList.Add(value);
			}
		}
	}

	public class String_Downcase : MBStyleExpression
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

	public class String_IsSupportedScript : MBStyleExpression
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

	public class String_ResolvedLocate : MBStyleExpression
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

	public class String_Upcase : MBStyleExpression
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
