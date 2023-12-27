using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public abstract class MBStyleExpression
	{
		private int _type;

		public abstract void parse(JsonArray json, MBStyleExpressionParser parser);
		public abstract object evaluate(MBStyleExpressionContext ctx);
	
		public virtual double evaluateDouble(MBStyleExpressionContext ctx)
		{
			return castDouble(evaluate(ctx));
		}

		public virtual Vector2 evaluateVector2(MBStyleExpressionContext ctx)
		{
			object value = evaluate(ctx);
			if( value is JsonArray)
			{
				JsonArray array = (JsonArray)value;
				return new Vector2(array.getFloat(0), array.getFloat(1));
			}
			else if( value is IList<object>)
			{
				IList<object> list = (IList<object>)value;
				return new Vector2((float)castDouble(list[0]), (float)castDouble(list[1]));
			}
			else
			{
				return Vector2.zero;
			}
		}

		public static string castString(object value)
		{
			if( value == null)
			{
				return null;
			}
			else if( value is string)
			{
				return (string)value;
			}
			else
			{
				return value.ToString();
			}
		}

		public static double castDouble(object value)
		{
			try
			{
				if( value == null)
				{
					return 0;
				}

				if (value is Int32)
				{
					return (double)(Int32)value;
				}
				else if (value is int)
				{
					return (double)(int)value;
				}
				else if (value is long)
				{
					return (double)(long)value;
				}
				else
				{
					return (double)value;
				}
			}
			catch (Exception e)
			{
				throw new Exception(string.Format("can't cast to double from type[{0}] value[{1}]", value.GetType().Name, value), e);
			}
		}

		public static bool isEqual(object a,object b)
		{
			if ( a is string)
			{
				return a.Equals(b);
			}

			try
			{
				double AA = castDouble(a);
				double BB = castDouble(b);

				return AA == BB;
			}
			catch(Exception e)
			{
				Debug.Log(e);
				return false;
			}

				
			//Type typeA = a.GetType();
			//Type typeB = b.GetType();

			//return a == b;


			//if( a.GetType().IsValueType)
			//{
			//	return a.Equals(b);
			//}
			//else
			//{
			//	return a == b;
			//}


			//if( a is double && b is double)
			//{
			//	double A = (double)a;
			//	double B = (double)b;

			//	return A == B;
			//}
			//else
			//{
			//	return a.Equals(b);
			//}
		}
	}
}
