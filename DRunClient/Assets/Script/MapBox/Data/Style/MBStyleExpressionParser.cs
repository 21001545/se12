using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.MapBox.Expression;

namespace Festa.Client.MapBox
{
	public class MBStyleExpressionParser
	{
		private MBStyleExpressionFactory _factory;

		public static MBStyleExpressionParser create(MBStyleExpressionFactory factory)
		{
			MBStyleExpressionParser parser = new MBStyleExpressionParser();
			parser.init(factory);
			return parser;
		}

		private void init(MBStyleExpressionFactory factory)
		{
			_factory = factory;
		}

		public MBStyleExpression parse(object obj)
		{
			if (obj is IList<object> || obj is JsonArray)
			{
				JsonArray json = null;
				if (obj is IList<object>)
				{
					json = new JsonArray((IList<object>)obj);
				}
				else
				{
					json = (JsonArray)obj;
				}

				if( json.getValue(0) is string)
				{
					string type_name = json.getString(0);
					int type = MBStyleDefine.getExpressionType(type_name);

					if (type == -1)
					{
						throw new Exception(string.Format("unknown expression type : {0}", type_name));
					}

					MBStyleExpression expression = _factory.create(type);
					expression.parse(json, this);
					return expression;
				}
				else
				{
					// 그냥 literal 처리
					return Type_Literal.create(json);
				}
			}
			else if (obj is string)
			{
				string str = (string)obj;

				if( str.StartsWith("hsl("))
				{
					Const_Color ex = new Const_Color();
					ex.setObjectHSL(obj);
					return ex;
				}
				else if( str.StartsWith("rgb("))
				{
					Const_Color ex = new Const_Color();
					ex.setObjectRGB(obj);
					return ex;
				}
				else if( str.StartsWith("#"))
				{
					Const_Color ex = new Const_Color();
					ex.setObjectWeb(obj);
					return ex;
				}
				else
				{
					Const_String ex = new Const_String();
					ex.setObject(obj);
					return ex;
				}
			}
			else if (obj is int || obj is double || obj is float || obj is Int64)
			{
				Const_Number ex = new Const_Number();
				ex.setObject(obj);
				return ex;
			}
			else if( obj is bool)
			{
				Const_Boolean ex = new Const_Boolean();
				ex.setObject(obj);
				return ex;
			}
			else
			{
				throw new Exception(string.Format("unkonwon obj type : {0}", obj.GetType().Name));
			}

		}
	}
}
