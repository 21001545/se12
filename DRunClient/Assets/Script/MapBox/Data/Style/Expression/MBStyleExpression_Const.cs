using Festa.Client.Module;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Festa.Client.MapBox.Expression
{
	public class Const_Number : MBStyleExpression
	{
		private object _value;

		public static Const_Number create(double v)
		{
			Const_Number n = new Const_Number();
			n._value = v;
			return n;
		}

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return _value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			return;
		}

		public void setObject(object obj)
		{
			if( obj is Int32)
			{
				_value = (double)(Int32)obj;
			}
			else if( obj is int)
			{
				_value = (double)(int)obj;
			}
			else if( obj is long)
			{
				_value = (double)(long)obj;
			}
			else
			{
				_value = obj;
			}
		}
	}

	public class Const_String : MBStyleExpression
	{
		private object _value;

		public static Const_String create(string v)
		{
			Const_String n = new Const_String();
			n._value = v;
			return n;
		}

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return _value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			return;
		}

		public void setObject(object obj)
		{
			_value = obj;
		}
	}

	public class Const_Boolean : MBStyleExpression
	{
		private object _value;

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return _value;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			return;
		}

		public void setObject(object obj)
		{
			_value = obj;
		}
	}

	public class Const_Color : MBStyleExpression
	{
		private Color _color;

		public static Const_Color create(Color c)
		{
			Const_Color cc = new Const_Color();
			cc._color = c;
			return cc;
		}

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			return _color;
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			return;
		}

		public void setObjectWeb(object obj)
		{
			string hexString = obj as string;

			//replace # occurences
			if (hexString.IndexOf('#') != -1)
				hexString = hexString.Replace("#", "");

			int r, g, b = 0;

			r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
			g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
			b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

			_color = new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
		}

		public void setObjectRGB(object obj)
		{
			string str = (string)obj;
			string[] tokens = str.Substring(4, str.Length - 5).Split(new char[] { ' ', ',', '%' }, StringSplitOptions.RemoveEmptyEntries);

			float r = (float)Int32.Parse(tokens[0]) / 255.0f;
			float g = (float)Int32.Parse(tokens[1]) / 255.0f;
			float b = (float)Int32.Parse(tokens[2]) / 255.0f;

			_color = new Color(r, g, b, 1.0f);
		}
		
		public void setObjectHSL(object obj)
		{
			string str = (string)obj;
			string[] tokens = str.Substring(4, str.Length - 5).Split( new char[] { ' ', ',', '%' }, StringSplitOptions.RemoveEmptyEntries);

			float h = (float)Int32.Parse(tokens[0]) / 360.0f;
			float s = (float)Int32.Parse(tokens[1]) / 100.0f;
			float l = (float)Int32.Parse(tokens[2]) / 100.0f;

			_color = ColorUtil.fromHSL(h, s, l);
		}

	}
}
