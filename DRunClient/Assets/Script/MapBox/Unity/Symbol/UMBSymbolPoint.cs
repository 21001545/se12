using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBSymbolPoint
	{
		private UMBSymbolPlacement _owner;
		private MBTileCoordinateDouble _tilePos;
		private MBAnchorCursor _anchorCursor;
		private Vector3 _worldPosition;
		private int _lastWorldPositionFrame;

		private bool _isVisible;
		private UMBSymbolRenderer _renderer;

		public MBTileCoordinateDouble getTilePos()
		{
			return _tilePos;
		}

		public MBAnchorCursor getAnchorCursor()
		{
			return _anchorCursor;
		}

		public bool isVisible()
		{
			return _isVisible;
		}

		public void setVisible(bool visible)
		{
			_isVisible = visible;
		}

		public UMBSymbolRenderer getRenderer()
		{
			return _renderer;
		}

		public void setRenderer(UMBSymbolRenderer renderer)
		{
			_renderer = renderer;
		}

		public static UMBSymbolPoint create(UMBSymbolPlacement owner,MBTileCoordinateDouble tilePos,MBAnchorCursor anchorCursor)
		{
			UMBSymbolPoint p = new UMBSymbolPoint();
			p.init(owner,tilePos, anchorCursor);
			return p;
		}

		private void init(UMBSymbolPlacement owner,MBTileCoordinateDouble tilePos,MBAnchorCursor anchorCursor)
		{
			_owner = owner;
			_tilePos = tilePos;
			_anchorCursor = anchorCursor;
			_isVisible = false;
			_lastWorldPositionFrame = -1;
		}

		public Vector3 getWorldPosition()
		{
			if( _lastWorldPositionFrame != Time.frameCount)
			{
				updateWorldPosition();
				_lastWorldPositionFrame = Time.frameCount;
			}

			return _worldPosition;
		}

		protected void updateWorldPosition()
		{
			MBTileCoordinate centerTilePos = _owner.getControl().getCurrentTilePos();
			
			// check TilePos update
			if( centerTilePos.zoom != _tilePos.zoom)
			{
				double scale = _owner.getControl().calcTileScale(centerTilePos.zoom, _tilePos.zoom);
				_tilePos.zoom = centerTilePos.zoom;
				_tilePos.tile_x *= scale;
				_tilePos.tile_y *= scale;
			}

			double offset_tile_x = _tilePos.tile_x - centerTilePos.tile_x;
			double offset_tile_y = _tilePos.tile_y - centerTilePos.tile_y;

			Vector3 pos = Vector3.one;
			pos.x = (float)(offset_tile_x * 4096);
			pos.y = -(float)(offset_tile_y * 4096);
			pos.z = 0;

			_worldPosition = _owner.getControl().getTileRoot().TransformPoint(pos);
		}
	}
}
