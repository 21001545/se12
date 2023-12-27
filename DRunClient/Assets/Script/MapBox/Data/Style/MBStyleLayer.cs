using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBStyleLayer
	{
		MBStyle _style;
		private JsonObject _data;
		private int _maxZoom;
		private int _minZoom;
		private string _id;
		private int _idHashCode;
		private int _type;
		private string _source_layer;
		private int _source_layerCode;
		private MBStyleExpression _filter;
		private int _layer_order;
		private bool _visibility;

		private MBStyleExpression _line_width;
		private MBStyleExpression _line_gap_width;
		private MBStyleExpression _line_join;
		private MBStyleExpression _line_miter_limit;
		private MBStyleExpression _line_cap;
		private MBStyleExpression _line_round_limit;
		private MBStyleExpression _line_offset;
		private MBStyleExpression _line_opacity;
		private MBStyleExpression _line_color;

		private MBStyleExpression _iconImage;
		private MBStyleExpression _iconSize;
		private MBStyleExpression _iconRotationAlignment;

		private MBStyleExpression _text_allow_overlap;
		private MBStyleExpression _text_anchor;
		private MBStyleExpression _text_field;
//		private MBStyleExpression _text_font;
		private MBStyleExpression _text_halo_blur;
		private MBStyleExpression _text_halo_color;
		private MBStyleExpression _text_halo_width;
		private MBStyleExpression _text_ignore_placement;
		private MBStyleExpression _text_justify;
		private MBStyleExpression _text_keep_upright;
		private MBStyleExpression _text_letter_spacing;
		private MBStyleExpression _text_line_height;
		private MBStyleExpression _text_max_angle;
		private MBStyleExpression _text_max_width;
		private MBStyleExpression _text_offset;
		private MBStyleExpression _text_opacity;
		private MBStyleExpression _text_optional;
		private MBStyleExpression _text_padding;
		private MBStyleExpression _text_pitch_alignment;
		private MBStyleExpression _text_radial_offset;
		private MBStyleExpression _text_rotate;
		private MBStyleExpression _text_rotation_alignment;
		private MBStyleExpression _text_size;
		private MBStyleExpression _text_transform;
		private MBStyleExpression _text_translate;
		private MBStyleExpression _text_translate_anchor;
		private MBStyleExpression _text_variable_anchor;
		private MBStyleExpression _text_writing_mode;

		private MBStyleExpression _text_color;

		private MBStyleExpression _fill_color;
		private MBStyleExpression _fill_opacity;

		private MBStyleExpression _fill_extrusion_color;
		private MBStyleExpression _fill_extrusion_opacity;

		private MBStyleExpression _backgroundColor;

		private int _text_font;
		private MBStyleExpression _symbol_placement;
		private MBStyleExpression _symbol_spacing;

		private MBStyleExpressionParser _ex_parser;

		public MBStyle getStyle()
		{
			return _style;
		}

		public string getID()
		{
			return _id;
		}

		public int getIDHashCode()
		{
			return _idHashCode;
		}

		public string getSourceLayer()
		{
			return _source_layer;
		}

		public int getSourceLayerHashCode()
		{
			return _source_layerCode;
		}

		public int getType()
		{
			return _type;
		}

		public int getMinZoom()
		{
			return _minZoom;
		}

		public int getMaxZoom()
		{
			return _maxZoom;
		}

		public MBStyleExpression getBackgroundColor()
		{
			return _backgroundColor;
		}

		//public MBStyleExpression getTextColor()
		//{
		//	return _textColor;
		//}

		//public MBStyleExpression getTextSize()
		//{
		//	return _textSize;
		//}

		//public MBStyleExpression getTextMaxWidth()
		//{
		//	return _textMaxWidth;
		//}

		//public MBStyleExpression getTextField()
		//{
		//	return _textField;
		//}

		//public MBStyleExpression getTextRotationAlignment()
		//{
		//	return _textRotationAlignment;
		//}


		public MBStyleExpression getIconImage()
		{
			return _iconImage;
		}

		public MBStyleExpression getIconRotationAlignment()
		{
			return _iconRotationAlignment;
		}


		public int getTextFont()
		{
			return _text_font;
		}

		public MBStyleExpression getIconSize()
		{
			return _iconSize;
		}

		public int getLayerOrder()
		{
			return _layer_order;
		}

		public bool getVisibility()
		{
			return _visibility;
		}

		public static MBStyleLayer create(MBStyle style,int layer_order,JsonObject json, MBStyleExpressionParser ex_parser)
		{
			MBStyleLayer layer = new MBStyleLayer();
			layer.init(style,layer_order,json, ex_parser);
			return layer;
		}

		private void parseExpression(ref MBStyleExpression ex, string key,JsonObject jsonData)
		{
			if( jsonData.contains(key))
			{
				ex = _ex_parser.parse(jsonData.getValue(key));
			}
			else
			{
				ex = null;
			}
		}


		private void init(MBStyle style,int layer_order,JsonObject json,MBStyleExpressionParser ex_parser)
		{
			_style = style;
			_data = json;
			_id = _data.getString("id");
			_idHashCode = EncryptUtil.makeHashCodePositive(_id);
			_minZoom = 0;
			_maxZoom = 24;
			_filter = null;
			_type = EncryptUtil.makeHashCode(json.getString("type"));
			_source_layer = json.getString("source-layer");
			_source_layerCode = EncryptUtil.makeHashCode(_source_layer);
			_layer_order = layer_order;
			_ex_parser = ex_parser;

			parseExpression(ref _filter, "filter", _data);

			if( _data.contains("minzoom"))
			{
				_minZoom = _data.getInteger("minzoom");
			}
			
			if( _data.contains("maxzoom"))
			{
				_maxZoom = _data.getInteger("maxzoom");
			}

			if ( _data.contains("paint"))
			{
				JsonObject json_paint = _data.getJsonObject("paint");

				if( _type == MBStyleDefine.LayerType.fill)
				{
					parseExpression(ref _fill_color, "fill-color", json_paint);
					parseExpression(ref _fill_opacity, "fill-opacity", json_paint);
				}
				else if( _type == MBStyleDefine.LayerType.fill_extrusion)
				{
					parseExpression(ref _fill_extrusion_color, "fill-extrusion-color", json_paint);
					parseExpression(ref _fill_extrusion_opacity, "fill-extrusion-opacity", json_paint);
				}
				else if( _type == MBStyleDefine.LayerType.line)
				{
					parseExpression(ref _line_color, "line-color", json_paint);
					parseExpression(ref _line_opacity, "line-opacity", json_paint);
					parseExpression(ref _line_width, "line-width", json_paint);
					parseExpression(ref _line_gap_width, "line-gap-width", json_paint);
					parseExpression(ref _line_join, "line-json", json_paint);
					parseExpression(ref _line_miter_limit, "line-miter-limit", json_paint);
					parseExpression(ref _line_cap, "line-cap", json_paint);
					parseExpression(ref _line_round_limit, "line-round-limit", json_paint);
					parseExpression(ref _line_offset, "line-offset", json_paint);
				}
				else if( _type == MBStyleDefine.LayerType.background)
				{
					if( json_paint.contains("background-color"))
					{
						_backgroundColor = ex_parser.parse(json_paint.getValue("background-color"));
					}
					else
					{
						_backgroundColor = Expression.Const_Color.create(Color.red);
					}
				}
				else if( _type == MBStyleDefine.LayerType.symbol)
				{
					parseExpression(ref _text_color, "text-color", json_paint);
					parseExpression(ref _text_halo_blur, "text-halo-blur", json_paint);
					parseExpression(ref _text_halo_color, "text-halo-color", json_paint);
					parseExpression(ref _text_halo_width, "text-halo-width", json_paint);
					parseExpression(ref _text_opacity, "text-opacity", json_paint);
					parseExpression(ref _text_translate, "text-translate", json_paint);
					parseExpression(ref _text_translate_anchor, "text-translate-anchor", json_paint);


					//if( json_paint.contains("text-halo-color"))
					//{
					//	_haloColor = ex_parser.parse(json_paint.getValue("text-halo-color"));
					//}
					//else
					//{
					//	_haloColor = Expression.Const_Color.create(Color.white); 
					//}

				}
			}

			if( _data.contains("layout"))
			{
				JsonObject json_layer = _data.getJsonObject("layout");
				if( json_layer.contains("icon-image"))
				{
					_iconImage = ex_parser.parse(json_layer.getValue("icon-image"));
				}

				if (json_layer.contains("icon-size"))
				{
					_iconSize = ex_parser.parse(json_layer.getValue("icon-size"));
				}
				else
				{
					_iconSize = Expression.Const_Number.create(0.7);
				}

				parseExpression(ref _symbol_placement, "symbol-placement", json_layer);
				parseExpression(ref _symbol_spacing, "symbol-spacing", json_layer);

				if (json_layer.contains("icon-rotation-alignment"))
				{
					_iconRotationAlignment = ex_parser.parse(json_layer.getValue("icon-rotation-alignment"));
				}
				else
				{
					_iconRotationAlignment = Expression.Const_String.create("auto");
				}

				if (json_layer.contains("visibility"))
				{
					_visibility = json_layer.getString("visibility") == "visible";
				}
				else
				{
					_visibility = true;
				}

				if (json_layer.contains("text-font"))
				{
					JsonArray array = json_layer.getJsonArray("text-font");
					_text_font = EncryptUtil.makeHashCode(array.getString(0));
				}
				else
				{
					_text_font = 0;
				}

				parseExpression(ref _text_allow_overlap			,"text-allow-overlap", json_layer);
				parseExpression(ref _text_anchor				,"text-anchor", json_layer);
				parseExpression(ref _text_field					,"text-field", json_layer);
				parseExpression(ref _text_ignore_placement		,"text-ignore-placement", json_layer);
				parseExpression(ref _text_justify				,"text-justify", json_layer);
				parseExpression(ref _text_keep_upright			,"text-keep-upright", json_layer);
				parseExpression(ref _text_letter_spacing		,"text-letter-spacing", json_layer);
				parseExpression(ref _text_line_height			,"text-line-height", json_layer);
				parseExpression(ref _text_max_angle				,"text-max-angle", json_layer);
				parseExpression(ref _text_max_width				,"text-max-width", json_layer);
				parseExpression(ref _text_offset				,"text-offset", json_layer);
				parseExpression(ref _text_optional				,"text-optional", json_layer);
				parseExpression(ref _text_padding				,"text-padding", json_layer);
				parseExpression(ref _text_pitch_alignment		,"text-pitch-alignment", json_layer);
				parseExpression(ref _text_radial_offset			,"text-radial-offset", json_layer);
				parseExpression(ref _text_rotate				,"text-rotate", json_layer);
				parseExpression(ref _text_rotation_alignment	,"text-rotation-alignment", json_layer);
				parseExpression(ref _text_size					,"text-size", json_layer);
				parseExpression(ref _text_transform				,"text-transform", json_layer);
				parseExpression(ref _text_variable_anchor		,"text-variable-anchor", json_layer);
				parseExpression(ref _text_writing_mode			,"text-writing-mode", json_layer);
			}
		}

		//
		public double evaluateLineMiterLimit(MBStyleExpressionContext ctx)
		{
			if( _line_miter_limit != null)
			{
				return _line_miter_limit.evaluateDouble(ctx);
			}
			else
			{
				return 2;
			}
		}

		public int evaluateLineJointType(MBStyleExpressionContext ctx)
		{
			if( _line_join != null)
			{
				string type = (string)_line_join.evaluate(ctx);
				if (type == "bevel")
				{
					return MBStyleDefine.LineJoinType.bevel;
				}
				else if (type == "round")
				{
					return MBStyleDefine.LineJoinType.round;
				}
			}

			return MBStyleDefine.LineJoinType.miter;
		}

		public int evaluateLineCap(MBStyleExpressionContext ctx)
		{
			if( _line_cap != null)
			{
				string type = (string)_line_cap.evaluate(ctx);
				if( type == "round")
				{
					return MBStyleDefine.LineCapType.round;
				}
				else if( type == "square")
				{
					return MBStyleDefine.LineCapType.square;
				}
			}

			return MBStyleDefine.LineCapType.butt;
		}

		public double evaluateLineGapWidth(MBStyleExpressionContext ctx)
		{
			if( _line_gap_width != null)
			{
				return _line_gap_width.evaluateDouble(ctx);
			}
			else
			{
				return 0;
			}
		}

		public double getLineRoundLimit(MBStyleExpressionContext ctx)
		{
			if( _line_round_limit != null)
			{
				return _line_round_limit.evaluateDouble(ctx);
			}
			else
			{
				return 1.05;
			}
		}

		public Color evaluateLineColor(MBStyleExpressionContext ctx)
		{
			if( _line_color != null)
			{
				return (Color)_line_color.evaluate(ctx);
			}
			else
			{
				return Color.red;
			}
		}

		public double evaluateLineOffset(MBStyleExpressionContext ctx)
		{
			if( _line_offset != null)
			{
				return _line_offset.evaluateDouble(ctx);
			}
			else
			{
				return 0;
			}
		}

		public double evaluateLineWidth(MBStyleExpressionContext ctx)
		{
			if( _line_width != null)
			{
				return _line_width.evaluateDouble(ctx);
			}
			else
			{
				return 1;
			}
		}

		public double evaluateLineOpacity(MBStyleExpressionContext ctx)
		{
			if( _line_opacity != null)
			{
				return _line_opacity.evaluateDouble(ctx);
			}
			else
			{
				return 1;
			}
		}

		// true 보임, false 않보임
		public bool evaluateFilter(MBStyleExpressionContext ctx)
		{
			if( _filter != null)
			{
				return (bool)_filter.evaluate(ctx);
			}
			else
			{
				return true;
			}
		}

		public Color evaluateFillColor(MBStyleExpressionContext ctx)
		{
			if( _fill_color != null)
			{
				return (Color)_fill_color.evaluate(ctx);
			}
			else
			{
				return Color.red;
			}
		}

		public double evaluateFillOpacity(MBStyleExpressionContext ctx)
		{
			if( _fill_opacity != null)
			{
				return _fill_opacity.evaluateDouble(ctx);
			}
			else
			{
				return 1;
			}
		}

		public Color evaluateFillExtrusionColor(MBStyleExpressionContext ctx)
		{
			if( _fill_extrusion_color != null)
			{
				return (Color)_fill_extrusion_color.evaluate(ctx);
			}
			else
			{
				return Color.red;
			}
		}

		public double evaluateFillExtrusionOpacity(MBStyleExpressionContext ctx)
		{
			if( _fill_extrusion_opacity != null)
			{
				return _fill_extrusion_opacity.evaluateDouble(ctx);
			}
			else
			{
				return 1.0;
			}
		}

		public Color evaluateBackgroundColor(MBStyleExpressionContext ctx)
		{
			if( _backgroundColor != null)
			{
				return (Color)_backgroundColor.evaluate(ctx);
			}
			else
			{
				return Color.red;
			}
		}

		public bool evaluateTextAllowOverlap(MBStyleExpressionContext ctx)
		{
			if( _text_allow_overlap != null)
			{
				return (bool)_text_allow_overlap.evaluate(ctx);
			}

			return false;
		}

		public int evaluateTextAnchor(MBStyleExpressionContext ctx)
		{
			if( _text_anchor != null)
			{
				string name = (string)_text_anchor.evaluate(ctx);
				if (name == "left")
				{
					return MBStyleDefine.TextAnchorType.left;
				}
				else if (name == "right")
				{
					return MBStyleDefine.TextAnchorType.right;
				}
				else if (name == "top")
				{
					return MBStyleDefine.TextAnchorType.top;
				}
				else if( name == "bottom")
				{
					return MBStyleDefine.TextAnchorType.bottom;
				}
				else if( name == "top-left")
				{
					return MBStyleDefine.TextAnchorType.top_left;
				}
				else if( name == "top-right")
				{
					return MBStyleDefine.TextAnchorType.top_right;
				}
				else if( name == "bottom-left")
				{
					return MBStyleDefine.TextAnchorType.bottom_left;
				}
				else if( name == "bottom-right")
				{
					return MBStyleDefine.TextAnchorType.bottom_right;
				}
			}

			return MBStyleDefine.TextAnchorType.center;
		}
		
		public Color evaluateTextColor(MBStyleExpressionContext ctx)
		{
			if( _text_color != null)
			{
				return (Color)_text_color.evaluate(ctx);
			}

			return Color.black;
		}

		public string evaluateTextField(MBStyleExpressionContext ctx)
		{
			if( _text_field != null)
			{
				string value = (string)_text_field.evaluate(ctx);
				if( string.IsNullOrEmpty(value))
				{
					return null;
				}
				else
				{
					return value;
				}
			}

			return null;
		}

		public double evaluateTextHaloBlur(MBStyleExpressionContext ctx)
		{
			if (_text_halo_blur != null)
			{
				return _text_halo_blur.evaluateDouble(ctx);
			}

			return 0;
		}

		public Color evaluateTextHaloColor(MBStyleExpressionContext ctx)
		{
			if( _text_halo_color != null)
			{
				return (Color)_text_halo_color.evaluate(ctx);
			}

			return new Color(0, 0, 0, 0);
		}

		public double evaluateTextHaloWidth(MBStyleExpressionContext ctx)
		{
			if( _text_halo_width != null)
			{
				return _text_halo_width.evaluateDouble(ctx);
			}

			return 0;
		}

		public bool evaluateTextIgnorePlacement(MBStyleExpressionContext ctx)
		{
			if( _text_ignore_placement != null)
			{
				return (bool)_text_ignore_placement.evaluate(ctx);
			}

			return false;
		}
		
		public int evaluateTextJustify(MBStyleExpressionContext ctx)
		{
			if( _text_justify != null)
			{
				string name = (string)_text_justify.evaluate(ctx);
				if (name == "auto")
				{
					return MBStyleDefine.TextJustifyType.auto;
				}
				else if( name == "left")
				{
					return MBStyleDefine.TextJustifyType.left;
				}
				else if( name == "right")
				{
					return MBStyleDefine.TextJustifyType.right;
				}
			}

			return MBStyleDefine.TextJustifyType.center;
		}
		
		public bool evaluateTextKeepUpright(MBStyleExpressionContext ctx)
		{
			if( _text_keep_upright != null)
			{
				return (bool)_text_keep_upright.evaluate(ctx);
			}

			return true;
		}

		public double evaluateTextLetterSpacing(MBStyleExpressionContext ctx)
		{
			if( _text_letter_spacing != null)
			{
				return _text_letter_spacing.evaluateDouble(ctx);
			}

			return 0;
		}

		public double evaluateTextLineHeight(MBStyleExpressionContext ctx)
		{
			if( _text_line_height != null)
			{
				return _text_line_height.evaluateDouble(ctx);
			}

			return 1.2;
		}

		public double evaluateTextMaxAngle(MBStyleExpressionContext ctx)
		{
			if( _text_max_angle != null)
			{
				return _text_max_angle.evaluateDouble(ctx);
			}

			return 45;
		}

		public double evaluateTextMaxWidth(MBStyleExpressionContext ctx)
		{
			if( _text_max_width != null)
			{
				return _text_max_width.evaluateDouble(ctx);
			}

			return 10;
		}
		
		public Vector2 evaluateTextOffset(MBStyleExpressionContext ctx)
		{
			if( _text_offset != null)
			{
				return _text_offset.evaluateVector2(ctx);
			}

			return Vector2.zero;
		}
		
		public double evaluateTextOpacity(MBStyleExpressionContext ctx)
		{
			if( _text_opacity != null)
			{
				return _text_opacity.evaluateDouble(ctx);
			}

			return 1;
		}

		public bool evaluateTextOptional(MBStyleExpressionContext ctx)
		{
			if(_text_optional != null)
			{
				return (bool)_text_optional.evaluate(ctx);
			}

			return false;
		}

		public double evaluateTextPadding(MBStyleExpressionContext ctx)
		{
			if( _text_padding != null)
			{
				return _text_padding.evaluateDouble(ctx);
			}

			return 2;
		}

		public int evaluateTextPitchAlignment(MBStyleExpressionContext ctx)
		{
			if( _text_pitch_alignment != null)
			{
				string name = (string)_text_pitch_alignment.evaluate(ctx);
				if( name == "map")
				{
					return MBStyleDefine.TextPitchAlignmentType.map;
				}
				else if( name == "viewport")
				{
					return MBStyleDefine.TextPitchAlignmentType.viewport;
				}
			}

			return MBStyleDefine.TextPitchAlignmentType.auto;
		}

		public double evaluateTextRadialOffset(MBStyleExpressionContext ctx)
		{
			if( _text_radial_offset != null)
			{
				return _text_radial_offset.evaluateDouble(ctx);
			}

			return 0;
		}

		public double evaluateTextRotate(MBStyleExpressionContext ctx)
		{
			if( _text_rotate != null)
			{
				return _text_rotate.evaluateDouble(ctx);
			}

			return 0;
		}

		public int evaluateTextRotateAlignment(MBStyleExpressionContext ctx)
		{
			if (_text_rotation_alignment != null)
			{
				string name = (string)_text_rotation_alignment.evaluate(ctx);
				if (name == "map")
				{
					return MBStyleDefine.TextPitchAlignmentType.map;
				}
				else if (name == "viewport")
				{
					return MBStyleDefine.TextPitchAlignmentType.viewport;
				}
			}

			return MBStyleDefine.TextPitchAlignmentType.auto;
		}

		public double evaluateTextSize(MBStyleExpressionContext ctx)
		{
			if( _text_size != null)
			{
				return _text_size.evaluateDouble(ctx);
			}
			return 16;
		}

		public int evaluateTextTransform(MBStyleExpressionContext ctx)
		{
			if( _text_transform != null)
			{
				string name = (string)_text_transform.evaluate(ctx);

				if( name == "uppercase")
				{
					return MBStyleDefine.TextTransformType.uppercase;
				}
				else if( name == "lowercase")
				{
					return MBStyleDefine.TextTransformType.lowercase;
				}
			}

			return MBStyleDefine.TextTransformType.none;
		}

		public double evaluateTextTranslate(MBStyleExpressionContext ctx)
		{
			if( _text_translate != null)
			{
				return _text_translate.evaluateDouble(ctx);
			}

			return 0;
		}

		public int evaluateTextTranslateAnchor(MBStyleExpressionContext ctx)
		{
			if( _text_translate_anchor != null)
			{
				string name = (string)_text_translate_anchor.evaluate(ctx);

				if( name == "viewport")
				{
					return MBStyleDefine.TextTranlateAnchorType.viewport;
				}
			}

			return MBStyleDefine.TextTranlateAnchorType.map;
		}

		public int evaluateTextVariableAnchor(MBStyleExpressionContext ctx)
		{
			if (_text_variable_anchor != null)
			{
				string name = (string)_text_variable_anchor.evaluate(ctx);
				if (name == "left")
				{
					return MBStyleDefine.TextAnchorType.left;
				}
				else if (name == "right")
				{
					return MBStyleDefine.TextAnchorType.right;
				}
				else if (name == "top")
				{
					return MBStyleDefine.TextAnchorType.top;
				}
				else if (name == "bottom")
				{
					return MBStyleDefine.TextAnchorType.bottom;
				}
				else if (name == "top-left")
				{
					return MBStyleDefine.TextAnchorType.top_left;
				}
				else if (name == "top-right")
				{
					return MBStyleDefine.TextAnchorType.top_right;
				}
				else if (name == "bottom-left")
				{
					return MBStyleDefine.TextAnchorType.bottom_left;
				}
				else if (name == "bottom-right")
				{
					return MBStyleDefine.TextAnchorType.bottom_right;
				}
			}

			return MBStyleDefine.TextAnchorType.center;
		}

		public int evaluateTextWritingMode(MBStyleExpressionContext ctx)
		{
			if( _text_writing_mode != null)
			{
				string name = (string)_text_writing_mode.evaluate(ctx);
				if( name == "vertical")
				{
					return MBStyleDefine.TextWritingMode.vertical;
				}
			}

			return MBStyleDefine.TextWritingMode.horizontal;
		}

		public int evaluateSymbolPlacement(MBStyleExpressionContext ctx)
		{
			if( _symbol_placement != null)
			{
				string name = (string)_symbol_placement.evaluate(ctx);
				if (name == "line")
				{
					return MBStyleDefine.SymbolPlacementType.line;
				}
				else if( name == "line-center")
				{
					return MBStyleDefine.SymbolPlacementType.line_center;
				}
			}

			return MBStyleDefine.SymbolPlacementType.point;
		}

		public double evaluateSymbolSpacing(MBStyleExpressionContext ctx)
		{
			if( _symbol_spacing != null)
			{
				return _symbol_spacing.evaluateDouble(ctx);
			}
			return 250;
		}

	}
}
