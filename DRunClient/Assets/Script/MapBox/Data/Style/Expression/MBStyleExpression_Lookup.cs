using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox.Expression
{
	public class Lookup_At : MBStyleExpression
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

	public class Lookup_Get : MBStyleExpression
	{
		private MBStyleExpression _property_key;

		private static int _key_name = EncryptUtil.makeHashCode("name");
		private static int _key_name_script = EncryptUtil.makeHashCode("name_script");
/*
		Arabic
		Armenian
		Bengali
		Bopomofo
		Canadian_Aboriginal
		Common
		Cyrillic
		Devanagari
		Ethiopic
		Georgian
		Glagolitic
		Greek
		Gujarati
		Gurmukhi
		Han
		Hangul
		Hebrew
		Hiragana
		Kannada
		Katakana
		Khmer
		Lao
		Latin
		Malayalam
		Mongolian
		Myanmar
		Nko
		Sinhala
		Syriac
		Tamil
		Telugu
		Thaana
		Thai
		Tibetan
		Tifinagh
		Unknown
*/
		public override object evaluate(MBStyleExpressionContext ctx)
		{
			if( ctx._feature == null)
			{
				return null;
			}
			string property_name = (string)_property_key.evaluate(ctx);
			int property_key = EncryptUtil.makeHashCode(property_name);
			if( property_key == _key_name)
			{
				string result = null;

				if(ctx._desireNameKey != 0 && ctx._feature.has(ctx._desireNameKey))
				{
					result = (string)ctx._feature.get(ctx._desireNameKey);
				}

				if( result == null && ctx._feature.has( _key_name_script))
				{
					string name_script = (string)ctx._feature.get(_key_name_script);
					if( name_script == "Hangul" && ctx._desireNameKey == MBStyleDefine.TextNamePropertyKey.name_ko)
					{
						result = (string)ctx._feature.get(_key_name);
					}
				}

				if( result == null)
				{
					result = (string)ctx._feature.get(MBStyleDefine.TextNamePropertyKey.name_en, null);
				}

				if( result == null)
				{
					result = (string)ctx._feature.get(MBStyleDefine.TextNamePropertyKey.name_local, null);
				}

				return result;
			}
			else
			{
				return ctx._feature.get(property_key, null);
			}
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_property_key = parser.parse(json.getValue(1));	
			//_value = parser.parse(json.getValue(2));
		}
	}

	public class Lookup_Has : MBStyleExpression
	{
		private MBStyleExpression _property_key;
		

		public override object evaluate(MBStyleExpressionContext ctx)
		{
			string property_name = (string)_property_key.evaluate(ctx);
			return ctx._feature.has(EncryptUtil.makeHashCode(property_name));
		}

		public override void parse(JsonArray json, MBStyleExpressionParser parser)
		{
			_property_key = parser.parse(json.getString(1));
		}
	}

	public class Lookup_In : MBStyleExpression
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

	public class Lookup_IndexOf : MBStyleExpression
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

	public class Lookup_Length : MBStyleExpression
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

	public class Lookup_Slice : MBStyleExpression
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
