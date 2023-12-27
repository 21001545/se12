using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBStyle
	{
		private JsonObject _data;
		private DateTime _modifiedTime;
		private List<MBStyleLayer> _layers;

		private MBStyleExpression _backgroundColor;
		//private MBStyleExpression _hillShadeOpacity;
		private Dictionary<string, Sprite> _dicSprite;

		private MBStyleExpressionContext _ctxBackgroundColor = new MBStyleExpressionContext();

		public JsonObject getData()
		{
			return _data;
		}

		public DateTime getModifiedTime()
		{
			return _modifiedTime;
		}

		public List<MBStyleLayer> getLayers()
		{
			return _layers;
		}

		//public MBStyleExpression getHillShadeOpacity()
		//{
		//	return _hillShadeOpacity;
		//}

		public MBStyleExpression getBackgroundColor()
		{
			return _backgroundColor;
		}

		public Sprite getMakiSprite(string name)
		{
			Sprite sprite;
			if( _dicSprite.TryGetValue(name, out sprite))
			{
				return sprite;
			}

			return null;
		}

		public void setSpriteDic(Dictionary<string,Sprite> dicSprite)
		{
			_dicSprite = dicSprite;
		}

		public static MBStyle create(JsonObject json,MBStyleExpressionParser ex_parser)
		{
			MBStyle s = new MBStyle();
			s.init(json, ex_parser);
			return s;
		}

		private void init(JsonObject json,MBStyleExpressionParser ex_parser)
		{
			_data = json;

			DateTime modifiedDate;
			if( TimeUtil.tryParseISO8601( _data.getString("modified"), out modifiedDate))
			{
				_modifiedTime = modifiedDate;
			}
			else
			{
				_modifiedTime = DateTime.UtcNow;
			}

			_layers = new List<MBStyleLayer>();
			JsonArray layers = json.getJsonArray("layers");
			for(int i = 0; i < layers.size(); ++i)
			{
				MBStyleLayer layer = MBStyleLayer.create(this,i, layers.getJsonObject(i), ex_parser);
				_layers.Add(layer);
			
				if( layer.getType() == MBStyleDefine.LayerType.background)
				{
					_backgroundColor = layer.getBackgroundColor();
				}
			}

			//createHillShadeOpacity(ex_parser);
		}

		//public Color getBackgroundColor(double zoom)
		//{
		//	_ctxBackgroundColor._zoom = zoom;
		//	return (Color)_backgroundColor.evaluate(_ctxBackgroundColor);
		//}

		//private void createHillShadeOpacity(MBStyleExpressionParser parser)
		//{
		//	JsonArray array = new JsonArray("[\"interpolate\",[\"linear\"],[\"zoom\"],14,1,16,0]");
		//	_hillShadeOpacity = parser.parse(array);
		//}
	}
}
