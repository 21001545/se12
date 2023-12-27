using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBActor : ReusableMonoBehaviour
	{
		protected RectTransform _rt;
		protected UnityMapBox _mapBox;
		protected UMBControl _control;

		protected MBLongLatCoordinate _position;
		protected int _lastProjectionMode;
		protected bool _canPick;

		public RectTransform rt => _rt;

		public virtual bool CanPick => _canPick;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_rt = transform as RectTransform;
			_lastProjectionMode = -1;
			_canPick = true;
		}

		public override void onReused()
		{
			_lastProjectionMode = -1;
		}

		public virtual void update()
		{

		}

		public virtual void delete()
		{
			_control.removeActor(this);
			GameObjectCache.getInstance().delete(this);
		}

		public virtual void init(UnityMapBox mapBox,MBLongLatCoordinate position)
		{
			_mapBox = mapBox;
			_control = _mapBox.getControl();
			_position = position;

			updateTransformPosition();
			checkNUpdateProjectionMode();
		}

		public virtual void changePosition(MBLongLatCoordinate position)
		{
			_position = position;
			updateTransformPosition();
		}

		public Vector3 calcLocalPosition()
		{
			MBTileCoordinate centerTilePos = _control.getCurrentTilePos();
			MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromLonLat(_position, centerTilePos.zoom);

			if (centerTilePos.isLeftEdge() && tilePos.isRightEdge())
			{
				int max = MapBoxUtil.maxTile(centerTilePos.zoom);
				tilePos.tile_pos.x -= max;
			}
			else if (centerTilePos.isRightEdge() && tilePos.isLeftEdge())
			{
				int max = MapBoxUtil.maxTile(centerTilePos.zoom);
				tilePos.tile_pos.x += max;
			}

			double offset_tile_x = tilePos.tile_x - centerTilePos.tile_x;
			double offset_tile_y = tilePos.tile_y - centerTilePos.tile_y;

			// local position
			Vector3 pos = Vector3.one;
			pos.x = (float)(offset_tile_x * 4096);
			pos.y = -(float)(offset_tile_y * 4096);
			pos.z = 0;

			// world position
			Vector3 world_pos = _mapBox.tile_root.TransformPoint(pos);

			// local to 3d root
			Vector3 local_pos = _rt.parent.InverseTransformPoint(world_pos);
			return local_pos;
		}

		public virtual void updateTransformPosition()
		{
			_rt.localPosition = calcLocalPosition();
			_rt.localRotation = _mapBox.rotate_root.localRotation;
		}

		protected virtual void checkNUpdateProjectionMode()
		{
			if( _lastProjectionMode != _mapBox.getViewModel().ProjectionMode)
			{
				_lastProjectionMode = _mapBox.getViewModel().ProjectionMode;

				updateProjectionMode();
			}
		}

		protected virtual void updateProjectionMode()
		{

		}
	}
}
