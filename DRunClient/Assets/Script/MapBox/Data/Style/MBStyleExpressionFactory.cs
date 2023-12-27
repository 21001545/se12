using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.MapBox.Expression;

namespace Festa.Client.MapBox
{
	public class MBStyleExpressionFactory
	{
		private delegate MBStyleExpression createFN();
		private Dictionary<int, createFN> _map;

		public static MBStyleExpressionFactory create()
		{
			MBStyleExpressionFactory f = new MBStyleExpressionFactory();
			f.init();
			return f;
		}

		private void init()
		{
			_map = new Dictionary<int, createFN>();

			register<Color_RGB>(MBStyleDefine.ExpressionType.color_rgb);
			register<Color_RGBA>(MBStyleDefine.ExpressionType.color_rgba);
			register<Color_ToRGBA>(MBStyleDefine.ExpressionType.color_to_rgba);
			register<Curve_Interpolation>(MBStyleDefine.ExpressionType.curve_interpolate);
			register<Curve_InterpolationHCL>(MBStyleDefine.ExpressionType.curve_interpolate_hcl);
			register<Curve_InterpolationLAB>(MBStyleDefine.ExpressionType.curve_interpolate_lab);
			register<Curve_Step>(MBStyleDefine.ExpressionType.curve_step);
			register<Decision_Not>(MBStyleDefine.ExpressionType.decision_not);
			register<Decision_NotEqual>(MBStyleDefine.ExpressionType.decision_not_equal);
			register<Decision_Less>(MBStyleDefine.ExpressionType.decision_less);
			register<Decision_LessEqual>(MBStyleDefine.ExpressionType.decision_less_equal);
			register<Decision_Equal>(MBStyleDefine.ExpressionType.decision_equal);
			register<Decision_Greater>(MBStyleDefine.ExpressionType.decision_greater);
			register<Decision_Greater_Equal>(MBStyleDefine.ExpressionType.decision_greater_equal);
			register<Decision_All>(MBStyleDefine.ExpressionType.decision_all);
			register<Decision_Any>(MBStyleDefine.ExpressionType.decision_any);
			register<Decision_Case>(MBStyleDefine.ExpressionType.decision_case);
			register<Decision_Coalesce>(MBStyleDefine.ExpressionType.decision_coalesce);
			register<Decision_Match>(MBStyleDefine.ExpressionType.decision_match);
			register<Decision_Within>(MBStyleDefine.ExpressionType.decision_within);
			register<Etc_Zoom>(MBStyleDefine.ExpressionType.zoom);
			register<Etc_HeatmapDensity>(MBStyleDefine.ExpressionType.heatmap_density);
			register<Feature_Accumulated>(MBStyleDefine.ExpressionType.feature_accumulated);
			register<Feature_FeatureState>(MBStyleDefine.ExpressionType.feature_feature_state);
			register<Feature_GeomtryType>(MBStyleDefine.ExpressionType.feature_geometry_type);
			register<Feature_ID>(MBStyleDefine.ExpressionType.feature_id);
			register<Feature_LineProgress>(MBStyleDefine.ExpressionType.feature_line_progress);
			register<Feature_Properties>(MBStyleDefine.ExpressionType.feature_properties);
			register<Lookup_At>(MBStyleDefine.ExpressionType.lookup_at);
			register<Lookup_Get>(MBStyleDefine.ExpressionType.lookup_get);
			register<Lookup_Has>(MBStyleDefine.ExpressionType.lookup_has);
			register<Lookup_In>(MBStyleDefine.ExpressionType.lookup_in);
			register<Lookup_IndexOf>(MBStyleDefine.ExpressionType.lookup_index_of);
			register<Lookup_Length>(MBStyleDefine.ExpressionType.lookup_length);
			register<Lookup_Slice>(MBStyleDefine.ExpressionType.lookup_slice);
			register<Math_Minus>(MBStyleDefine.ExpressionType.math_minus);
			register<Math_Multiply>(MBStyleDefine.ExpressionType.math_multiply);
			register<Math_Divide>(MBStyleDefine.ExpressionType.math_divide);
			register<Math_Mod>(MBStyleDefine.ExpressionType.math_mod);
			register<Math_Pow>(MBStyleDefine.ExpressionType.math_pow);
			register<Math_Plus>(MBStyleDefine.ExpressionType.math_plus);
			register<Math_Abs>(MBStyleDefine.ExpressionType.math_abs);
			register<Math_ACos>(MBStyleDefine.ExpressionType.math_acos);
			register<Math_ASin>(MBStyleDefine.ExpressionType.math_asin);
			register<Math_Atan>(MBStyleDefine.ExpressionType.math_atan);
			register<Math_Ceil>(MBStyleDefine.ExpressionType.math_ceil);
			register<Math_Cos>(MBStyleDefine.ExpressionType.math_cos);
			register<Math_Distance>(MBStyleDefine.ExpressionType.math_distance);
			register<Math_E>(MBStyleDefine.ExpressionType.math_e);
			register<Math_Floor>(MBStyleDefine.ExpressionType.math_floor);
			register<Math_Ln>(MBStyleDefine.ExpressionType.math_ln);
			register<Math_Ln2>(MBStyleDefine.ExpressionType.math_ln2);
			register<Math_Log10>(MBStyleDefine.ExpressionType.math_log10);
			register<Math_Log2>(MBStyleDefine.ExpressionType.math_log2);
			register<Math_Max>(MBStyleDefine.ExpressionType.math_max);
			register<Math_Min>(MBStyleDefine.ExpressionType.math_min);
			register<Math_Pi>(MBStyleDefine.ExpressionType.math_pi);
			register<Math_Round>(MBStyleDefine.ExpressionType.math_round);
			register<Math_Sin>(MBStyleDefine.ExpressionType.math_sin);
			register<Math_Sqrt>(MBStyleDefine.ExpressionType.math_sqrt);
			register<Math_Tan>(MBStyleDefine.ExpressionType.math_tan);
			register<Type_Array>(MBStyleDefine.ExpressionType.type_array);
			register<Type_Boolean>(MBStyleDefine.ExpressionType.type_boolean);
			register<Type_Collator>(MBStyleDefine.ExpressionType.type_collator);
			register<Type_Format>(MBStyleDefine.ExpressionType.type_format);
			register<Type_Image>(MBStyleDefine.ExpressionType.type_image);
			register<Type_Literal>(MBStyleDefine.ExpressionType.type_literal);
			register<Type_Number>(MBStyleDefine.ExpressionType.type_number);
			register<Type_NumberFormat>(MBStyleDefine.ExpressionType.type_number_format);
			register<Type_Object>(MBStyleDefine.ExpressionType.type_object);
			register<Type_String>(MBStyleDefine.ExpressionType.type_string);
			register<Type_ToBoolean>(MBStyleDefine.ExpressionType.type_to_boolean);
			register<Type_ToColor>(MBStyleDefine.ExpressionType.type_to_color);
			register<Type_ToNumber>(MBStyleDefine.ExpressionType.type_to_number);
			register<Type_ToString>(MBStyleDefine.ExpressionType.type_to_string);
			register<Type_Typeof>(MBStyleDefine.ExpressionType.type_typeof);
			register<String_Concat>(MBStyleDefine.ExpressionType.string_concat);
			register<String_Downcase>(MBStyleDefine.ExpressionType.string_downcase);
			register<String_IsSupportedScript>(MBStyleDefine.ExpressionType.string_is_supported_script);
			register<String_ResolvedLocate>(MBStyleDefine.ExpressionType.string_resolved_locale);
			register<String_Upcase>(MBStyleDefine.ExpressionType.string_upcase);
		}

		public void register<T>(int type) where T : MBStyleExpression, new()
		{
			_map.Add(type, () => { return new T(); });
		}

		public MBStyleExpression create(int type)
		{
			if( _map.ContainsKey(type) == false)
			{
				throw new Exception(string.Format("expression was not registered type : {0}", type));
			}

			return _map[type]();
		}
	}
}
