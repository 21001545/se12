using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public abstract class UMBSymbolPlacement
	{
		protected int _type;
		protected UMBControl _control;
		protected UMBLabelManager _labelManager;
		protected MBLayerRenderData _layer;
		protected MBFeature _feature;
		protected MBTileCoordinate _ownerTilePos;

		protected int _layerOrder;
		protected int _subOrder;

		protected string _text;
		protected Color _textColor;
		protected float _textSize;
		protected float _textMaxWidth;
		protected int _textFont;
		protected UMBFontSource.FontSource _fontSource;
		protected int _textRotationAlignment;
		protected Vector2 _textOffset;

		protected Sprite _icon;
		protected float _iconSize;
		protected int _iconRotationAlignment;

		protected int _collision_order;

		protected List<UMBSymbolPoint> _symbolPoints;
		protected List<UMBSymbolRenderer> _rendererList;

		protected bool _isAnyPointVisible;
		protected MBStyle _mbStyle;

		public static class PlacementType
		{
			public static int point = EncryptUtil.makeHashCode("point");
			public static int line_center = EncryptUtil.makeHashCode("line-center");
			public static int line = EncryptUtil.makeHashCode("line");
		}

		#region getter

		public int getType()
		{
			return _type;
		}

		public MBFeature getFeature()
		{
			return _feature;
		}

		public long getFeatureID()
		{
			return _feature.id;
		}

		public int getStyleLayerID()
		{
			return _layer.getLayerStyle().getIDHashCode();
		}

		public MBLayerRenderData getLayer()
		{
			return _layer;
		}

		public int getLayerOrder()
		{
			return _layerOrder;
		}

		public string getText()
		{
			return _text;
		}

		public Color getTextColor()
		{
			return _textColor;
		}

		public float getTextSize()
		{
			return _textSize;
		}

		public float getTextMaxWidth()
		{
			return _textMaxWidth;
		}

		public UMBFontSource.FontSource getFontSource()
		{
			return _fontSource;
		}

		public int getTextRotationAlignment()
		{
			return _textRotationAlignment;
		}

		public Vector2 getTextOffset()
		{
			return _textOffset;
		}

		public Sprite getIcon()
		{
			return _icon;
		}

		public float getIconSize()
		{
			return _iconSize;
		}

		public int getIconRotationAlignment()
		{
			return _iconRotationAlignment;
		}

		public int getTextFont()
		{
			return _textFont;
		}

		public bool hasText()
		{
			return _text != null;
		}

		public bool hasIcon()
		{
			return _icon != null;
		}

		public UMBControl getControl()
		{
			return _control;
		}

		public UMBLabelManager getLabelManager()
		{
			return _labelManager;
		}

		public List<UMBSymbolPoint>	getPointList()
		{
			return _symbolPoints;
		}

		public List<UMBSymbolRenderer> getRendererList()
		{
			return _rendererList;
		}

		public int getCollisionOrder()
		{
			return _collision_order;
		}

		public bool isAnyPointVisible()
		{
			return _isAnyPointVisible;
		}

		#endregion

		public static UMBSymbolPlacement create(UMBControl control,UMBTile owner,MBLayerRenderData layer,MBFeature feature,int subOrder)
		{
			//if( layer.getLayerStyle().getID() == "waterway-label")
			//{
			//	int a = 0;
			//}

			UMBSymbolPlacement p;
			MBStyleExpressionContext ctx = MBStyleDefine.mainThreadContext;
			ctx._zoom = control.getCurrentTilePos().zoom;
			ctx._feature = feature;

			int type = layer.getLayerStyle().evaluateSymbolPlacement(ctx);
			if(type == MBStyleDefine.SymbolPlacementType.point)
			{
				p = new UMBSymbolPlacementPoint();
			}
			else if(type == MBStyleDefine.SymbolPlacementType.line)
			{
				p = new UMBSymbolPlacementLine();
			}
			else if(type == MBStyleDefine.SymbolPlacementType.line_center)
			{
				p = new UMBSymbolPlacementLineCenter();
			}
			else
			{
				throw new Exception("unknown placement type");
			}

			p.init(type,control, owner, layer, feature, subOrder);
			return p;
		}

		protected virtual void init(int type,UMBControl control,UMBTile owner,MBLayerRenderData layer,MBFeature feature,int subOrder)
		{
			_type = type;
			_ownerTilePos = owner.getTilePos();

			_control = control;
			_labelManager = control.getMapBox().getLabelManager();
			_layer = layer;
			_feature = feature;
			_subOrder = subOrder;
			_symbolPoints = new List<UMBSymbolPoint>();
			_collision_order = layer.getLayerStyle().getLayerOrder() * 1000 + _subOrder;
			_mbStyle = owner.getMBStyle();

			MBStyleExpressionContext ctx = new MBStyleExpressionContext();
			ctx._zoom = owner.getTilePos().zoom;
			ctx._feature = feature;
			ctx._desireNameKey = MBStyleDefine.TextNamePropertyKey.fromLanguageType(GlobalRefDataContainer.getStringCollection().getCurrentLangType());

			evaluateSymbolData(ctx);

			createPoints();
		}

		protected abstract void createPoints();

		protected virtual void evaluateSymbolData(MBStyleExpressionContext ctx)
		{
			MBStyleLayer styleLayer = _layer.getLayerStyle();
			MBStyleExpression ex_icon = styleLayer.getIconImage();
			MBStyleExpression ex_iconSize = styleLayer.getIconSize();
			MBStyleExpression ex_iconRotationAlignment = styleLayer.getIconRotationAlignment();

			_textColor = styleLayer.evaluateTextColor(ctx);
			_textFont = styleLayer.getTextFont();

			_icon = null;
			if( ex_icon != null)
			{
				string icon_name = (string)ex_icon.evaluate(ctx);
				if( string.IsNullOrEmpty(icon_name) == false)
				{
					_icon = _mbStyle.getMakiSprite(icon_name);
				}
			}

			_text = styleLayer.evaluateTextField(ctx);
			if( _text != null)
			{
				_fontSource = _control.getMapBox().getUMBStyle().getFontSource(_textFont);
			}

			_textSize = (float)styleLayer.evaluateTextSize(ctx);
			_textMaxWidth = (float)styleLayer.evaluateTextMaxWidth(ctx);
			_textRotationAlignment = styleLayer.evaluateTextRotateAlignment(ctx);
			_textOffset = styleLayer.evaluateTextOffset(ctx);
			
			//if( _text != null)
			//{
			//	Debug.Log($"{_text},{_textOffset}");
			//}

			_iconSize = (float)ex_iconSize.evaluateDouble(ctx);
			_iconRotationAlignment = EncryptUtil.makeHashCode((string)ex_iconRotationAlignment.evaluate(ctx));
		}

		public MBTileCoordinateDouble tilePosFromLocalPos(Vector2 pos)
		{
			MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromInteger(_ownerTilePos);
			tilePos.tile_x += (double)pos.x / (double)MapBoxDefine.tile_extent;
			tilePos.tile_y += (double)pos.y / (double)MapBoxDefine.tile_extent;
			return tilePos;
		}

		public Vector2Int localPosFromTilePos(MBTileCoordinateDouble tilePos)
		{
			if( tilePos.zoom != _ownerTilePos.zoom)
			{
				double scale = _control.calcTileScale(_ownerTilePos.zoom, tilePos.zoom);
				tilePos.tile_x *= scale;
				tilePos.tile_y *= scale;
			}

			int x = (int)((tilePos.tile_x - _ownerTilePos.tile_x) * MapBoxDefine.tile_extent);
			int y = (int)((tilePos.tile_y - _ownerTilePos.tile_y) * MapBoxDefine.tile_extent);

			return new Vector2Int(x, y);
		}

		public void update()
		{
			_isAnyPointVisible = false;
			foreach (UMBSymbolPoint point in _symbolPoints)
			{
				bool visible = pointIsVisible(point);

				if( visible)
				{
					if( point.getRenderer() == null && _labelManager.getRemainRendererCreationCount() > 0)
					{
						UMBSymbolRenderer renderer = _labelManager.createSymbolRenderer();
						renderer.setup(this, point);

						point.setRenderer(renderer);
					}

					_isAnyPointVisible = true;
				}
				else
				{
					if( point.getRenderer() != null)
					{
						UMBSymbolRenderer renderer = point.getRenderer();
						point.setRenderer(null);
						_labelManager.removeSymbolRenderer(renderer);
					}
				}
			}
		}

		protected bool pointIsVisible(UMBSymbolPoint point)
		{
			Vector3 viewPos = _control.getTargetCamera().WorldToViewportPoint(point.getWorldPosition());
			return viewPos.x >= 0 && viewPos.y >= 0 && viewPos.x <= 1 && viewPos.y <= 1;
		}

		public void delete()
		{
			foreach(UMBSymbolPoint point in _symbolPoints)
			{
				if( point.getRenderer() != null)
				{
					UMBSymbolRenderer renderer = point.getRenderer();
					point.setRenderer(null);
					_labelManager.removeSymbolRenderer(renderer);
				}
			}
		}

		public virtual void onDrawGizmos()
		{
			//drawSymbolPoints();
		}

		private void drawSymbolPoints()
		{
			foreach (UMBSymbolPoint point in _symbolPoints)
			{
				Vector3 pos = _control.tilePosToWorldPosition(point.getTilePos());
				Gizmos.color = Color.red;
				Gizmos.DrawCube(pos, Vector3.one / 50.0f);
			}
		}
	}
}
