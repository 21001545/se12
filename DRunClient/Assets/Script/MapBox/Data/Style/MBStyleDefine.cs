using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public static class MBStyleDefine
	{
		public class ExpressionType
		{
			public const int type_array = 0;
			public const int type_boolean = 1;
			public const int type_collator = 2;
			public const int type_format = 3;
			public const int type_image = 4;
			public const int type_literal = 5;
			public const int type_number = 6;
			public const int type_number_format = 7;
			public const int type_object = 8;
			public const int type_string = 9;
			public const int type_to_boolean = 10;
			public const int type_to_color = 11;
			public const int type_to_number = 12;
			public const int type_to_string = 13;
			public const int type_typeof = 14;

			public const int feature_accumulated = 15;
			public const int feature_feature_state = 16;
			public const int feature_geometry_type = 17;
			public const int feature_id = 18;
			public const int feature_line_progress = 19;
			public const int feature_properties = 20;

			public const int lookup_at = 21;
			public const int lookup_get = 22;
			public const int lookup_has = 23;
			public const int lookup_in = 24;
			public const int lookup_index_of = 25;
			public const int lookup_length = 26;
			public const int lookup_slice = 27;

			public const int decision_not = 28;
			public const int decision_not_equal = 29;
			public const int decision_less = 30;
			public const int decision_less_equal = 31;
			public const int decision_equal = 32;
			public const int decision_greater = 33;
			public const int decision_greater_equal = 34;
			public const int decision_all = 35;
			public const int decision_any = 36;
			public const int decision_case = 37;
			public const int decision_coalesce = 38;
			public const int decision_match = 39;
			public const int decision_within = 40;

			public const int curve_interpolate = 41;
			public const int curve_interpolate_hcl = 42;
			public const int curve_interpolate_lab = 43;
			public const int curve_step = 44;

			public const int binding_let = 45;
			public const int binding_var = 46;

			public const int string_concat = 47;
			public const int string_downcase = 48;
			public const int string_is_supported_script = 49;
			public const int string_resolved_locale = 50;
			public const int string_upcase = 51;

			public const int color_rgb = 52;
			public const int color_rgba = 53;
			public const int color_to_rgba = 54;

			public const int math_minus = 55;
			public const int math_multiply = 56;
			public const int math_divide = 57;
			public const int math_mod = 58;
			public const int math_pow = 59;
			public const int math_plus = 60;
			public const int math_abs = 61;
			public const int math_acos = 62;
			public const int math_asin = 63;
			public const int math_atan = 64;
			public const int math_ceil = 65;
			public const int math_cos = 66;
			public const int math_distance = 67;
			public const int math_e = 68;
			public const int math_floor = 69;
			public const int math_ln = 70;
			public const int math_ln2 = 71;
			public const int math_log10 = 72;
			public const int math_log2 = 73;
			public const int math_max = 74;
			public const int math_min = 75;
			public const int math_pi = 76;
			public const int math_round = 77;
			public const int math_sin = 78;
			public const int math_sqrt = 79;
			public const int math_tan = 80;

			public const int zoom = 81;
			public const int heatmap_density = 82;
		}

		public static string[] ExpressionType_Names = new string[]
		{
			"array",//type_array = 0;
			"boolean",//type_boolean = 1;
			"collator",//type_collator = 2;
			"format",//type_format = 3;
			"image",//type_image = 4;
			"literal",//type_literal = 5;
			"number",//type_number = 6;
			"number-format",//type_number_format = 7;
			"object",//type_object = 8;
			"string",//type_string = 9;
			"to-boolean",//type_to_boolean = 10;
			"to-color",//type_to_color = 11;
			"to-number",//type_to_number = 12;
			"to-string",//type_to_string = 13;
			"to-typeof",//type_typeof = 14;
			"accumulated",//feature_accumulated = 15;
			"feature-state",//feature_feature_state = 16;
			"geometry-type",//feature_geometry_type = 17;
			"id",//feature_id = 18;
			"line-progress",//feature_line_progress = 19;
			"properties",//feature_properties = 20;
			"at",//lookup_at = 21;
			"get",//lookup_get = 22;
			"has",//lookup_has = 23;
			"in",//lookup_in = 24;
			"index-of",//lookup_index_of = 25;
			"length",//lookup_length = 26;
			"slice",//lookup_slice = 27;
			"!",//decision_not = 28;
			"!=",//decision_not_equal = 29;
			"<",//decision_less = 30;
			"<=",//decision_less_equal = 31;
			"==",//decision_equal = 32;
			">",//decision_greater = 33;
			">=",//decision_greater_equal = 34;
			"all",//decision_all = 35;
			"any",//decision_any = 36;
			"case",//decision_case = 37;
			"coalesce",//decision_coalesce = 38;
			"match",//decision_match = 39;
			"within",//decision_within = 40;
			"interpolate",//curve_interpolate = 41;
			"interpolate-hcl",//curve_interpolate_hcl = 42;
			"interpolate-lab",//curve_interpolate_lab = 43;
			"step",//curve_step = 44;
			"let",//binding_let = 45;
			"var",//binding_var = 46;
			"concat",//string_concat = 47;
			"downcase",//string_downcase = 48;
			"is-supported-script",//string_is_supported_script = 49;
			"resolved-locale",//string_resolved_locale = 50;
			"upcase",//string_upcase = 51;
			"rgb",//color_rgb = 52;
			"rgba",//color_rgba = 53;
			"to-rgba",//color_to_rgba = 54;
			"-",//math_minus = 55;
			"*",//math_multiply = 56;
			"/",//math_divide = 57;
			"%",//math_mod = 58;
			"^",//math_pow = 59;
			"+",//math_plus = 60;
			"abs",//math_abs = 61;
			"acos",//math_acos = 62;
			"asin",//math_asin = 63;
			"atan",//math_atan = 64;
			"ceil",//math_ceil = 65;
			"cos",//math_cos = 66;
			"distance",//math_distance = 67;
			"e",//math_e = 68;
			"floor",//math_floor = 69;
			"ln",//math_ln = 70;
			"ln2",//math_ln2 = 71;
			"log10",//math_log10 = 72;
			"log2",//math_log2 = 73;
			"max",//math_max = 74;
			"min",//math_min = 75;
			"pi",//math_pi = 76;
			"round",//math_round = 77;
			"sin",//math_sin = 78;
			"sqrt",//math_sqrt = 79;
			"tan",//math_tan = 80;
			"zoom",//zoom = 81;
			"heatmap-density"//heatmap_density = 82;
		};

		private static Dictionary<string, int> _ExpressionTypeMap;

		public static void initStatic()
		{
			createExpressionTypeMap();
		}

		public static int getExpressionType(string name)
		{

			int type;
			if( _ExpressionTypeMap.TryGetValue(name,out type) == false)
			{
				return -1;
			}

			return type;
		}

		private static void createExpressionTypeMap()
		{
			_ExpressionTypeMap = new Dictionary<string, int>();
			for(int i = 0; i < ExpressionType_Names.Length; ++i)
			{
				_ExpressionTypeMap.Add(ExpressionType_Names[i], i);
			}
		}

		public static class InterpolationType
		{
			public const int linear = 0;
			public const int exponential = 1;
			public const int cube_bezier = 2;
		}

		public static class LayerType
		{
			public static int fill = EncryptUtil.makeHashCode("fill");
			public static int line = EncryptUtil.makeHashCode("line");
			public static int symbol = EncryptUtil.makeHashCode("symbol");
			public static int circle = EncryptUtil.makeHashCode("circle");
			public static int heatmap = EncryptUtil.makeHashCode("heatmap");
			public static int fill_extrusion = EncryptUtil.makeHashCode("fill-extrusion");
			public static int raster = EncryptUtil.makeHashCode("raster");
			public static int hillshade = EncryptUtil.makeHashCode("hillshade");
			public static int background = EncryptUtil.makeHashCode("background");
			public static int sky = EncryptUtil.makeHashCode("sky");
		}

		public static class SymbolPlacementType
		{
			public static int point = 0;
			public static int line = 1;
			public static int line_center = 2;
		}

		public static class RotationAlignment
		{
			public static int map = EncryptUtil.makeHashCode("map");
			public static int auto = EncryptUtil.makeHashCode("auto");
			public static int viewport = EncryptUtil.makeHashCode("viewport");
		}

		public static class LineJoinType
		{
			public static int miter = 1;
			public static int bevel = 2;
			public static int round = 3;

			public static int fake_round = 10;
			public static int flip_bevel = 11;
		}

		public static class LineCapType
		{
			public static int butt = 1;
			public static int round = 2;
			public static int square = 3;
		}

		public static class TextAnchorType
		{
			public static int center = 1;
			public static int left = 2;
			public static int right = 3;
			public static int top = 4;
			public static int bottom = 5;
			public static int top_left = 6;
			public static int top_right = 7;
			public static int bottom_left = 8;
			public static int bottom_right = 9;
		}

		public static class TextJustifyType
		{
			public static int auto = 1;
			public static int left = 2;
			public static int center = 3;
			public static int right = 4;
		}

		public static class TextPitchAlignmentType
		{
			public static int map = 1;
			public static int viewport = 2;
			public static int auto = 2;
		}

		public static class TextTransformType
		{
			public static int none = 1;
			public static int uppercase = 2;
			public static int lowercase = 3;
		}

		public static class TextTranlateAnchorType
		{
			public static int map = 1;
			public static int viewport = 2;
		}

		public static class TextWritingMode
		{
			public static int horizontal = 1;
			public static int vertical = 2;
		}

		public static class TextNamePropertyKey
		{
			public static int name = EncryptUtil.makeHashCode("name");
			public static int name_en = EncryptUtil.makeHashCode("name_en");
			public static int name_es = EncryptUtil.makeHashCode("name_es");
			public static int name_fr = EncryptUtil.makeHashCode("name_fr");
			public static int name_de = EncryptUtil.makeHashCode("name_de");
			public static int name_it = EncryptUtil.makeHashCode("name_it");
			public static int name_pt = EncryptUtil.makeHashCode("name_pt");
			public static int name_ru = EncryptUtil.makeHashCode("name_ru");
			public static int name_zh_hans = EncryptUtil.makeHashCode("name_zh-Hans"); // 간체
			public static int name_zh_hant = EncryptUtil.makeHashCode("name_zh-Hant"); // 번체
			public static int name_ja = EncryptUtil.makeHashCode("name_ja");
			public static int name_ko = EncryptUtil.makeHashCode("name_ko");
			public static int name_vi = EncryptUtil.makeHashCode("name_vi");
			public static int name_local = EncryptUtil.makeHashCode("name_local");
//name_ar Arabic
//name_en English
//name_es Spanish
//name_fr French
//name_de German
//name_it Italian
//name_pt Portuguese
//name_ru Russian
//name_zh-Hans Simplified Chinese
//name_zh-Hant Traditional Chinese(if available, but may contain some Simplified Chinese)
//name_ja Japanese
//name_ko Korean
//name_vi Vietnamese
			public static int fromLanguageType(int type)
			{
				if (type == LanguageType.en)
				{
					return name_en;
				}
				else if (type == LanguageType.ko)
				{
					return name_ko;
				}
				else if (type == LanguageType.zhCN)
				{
					return name_zh_hans;
				}
				else if (type == LanguageType.zhTW)
				{
					return name_zh_hant;
				}
				else if( type == LanguageType.de)
				{
					return name_de;
				}
				else if (type == LanguageType.fr)
				{
					return name_fr;
				}
				else if (type == LanguageType.ptPT)
				{
					return name_pt;
				}
				else if (type == LanguageType.esES)
				{
					return name_es;
				}
				else if( type == LanguageType.ru)
				{
					return name_ru;
				}
				else if( type == LanguageType.jp)
				{
					return name_ja;
				}
				else if( type == LanguageType.ko)
				{
					return name_ko;
				}

				return name_en;
			}
		}

		public static MBStyleExpressionContext mainThreadContext = new MBStyleExpressionContext();
	}
}
