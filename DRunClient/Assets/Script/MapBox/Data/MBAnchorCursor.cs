using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBAnchorCursor
	{
		private MBAnchors _anchors;

		private MBAnchor _current;
		private float	_offset;

		private Vector2 _position;
		private float _angle;

		public float getOffset()
		{
			return _offset;
		}

		public Vector2 getPosition()
		{
			return _position;
		}

		public Vector2Int getPositionInt()
		{
			return new Vector2Int( (int)_position.x, (int)_position.y);
		}

		public float getAngle()
		{
			return _angle;
		}

		public Vector2 getDirection()
		{
			return _current.getDirection();
		}

		public Vector2 getDirectionPerpendicular()
		{
			return _current.getDirectionPerpendicular();
		}

		public Vector2 getPositionPerpendicularDistance(float p_length)
		{
			return _position + _current.getDirectionPerpendicular() * p_length;
		}

		public MBAnchor getCurrentAnchor()
		{
			return _current;
		}

		public MBAnchors getAnchors()
		{
			return _anchors;
		}

		public static MBAnchorCursor create(MBAnchors anchors,MBAnchor current,float offset,Vector2 position,float angle)
		{
			MBAnchorCursor cursor = new MBAnchorCursor();
			cursor._current = current;
			cursor._anchors = anchors;
			cursor._offset = offset;
			cursor._position = position;
			cursor._angle = angle;
			return cursor;
		}

		public MBAnchorCursor copy()
		{
			MBAnchorCursor cursor = new MBAnchorCursor();
			cursor._current = _current;
			cursor._anchors = _anchors;
			cursor._offset = _offset;
			cursor._position = _position;
			cursor._angle = _angle;
			return cursor;
		}

		// back는 나중에 구현하자 헷갈림
		public void moveForward(float delta)
		{
			_offset += delta;

			float local_remain_offset = _offset - _current.getPrevAccumLength();
			MBAnchor cur = _current;
			while(cur != null)
			{
				if( local_remain_offset < cur.getLength())
				{
					_position = cur.getPosition() + cur.getDirection() * local_remain_offset;
					_angle = cur.getAngle();

					_current = cur;
					return;
				}

				if( cur.getNext() != null)
				{
					local_remain_offset -= cur.getLength();
					cur = cur.getNext();
					continue;
				}

				// cur이 끝이다
				break;
			}

			_position = cur.getPosition() + cur.getDirection() * local_remain_offset;
			_angle = cur.getAngle();
			_current = cur;
		}

		public void moveBackward(float delta)
		{
			_offset -= delta;

			MBAnchor cur = _current;
			while(cur != null)
			{
				if(_offset >= cur.getPrevAccumLength())
				{
					_position = cur.getPosition() + cur.getDirection() * (_offset - cur.getPrevAccumLength());
					_angle = cur.getAngle();
					_current = cur;
					return;
				}

				if( cur.getPrev() != null)
				{
					cur = cur.getPrev();
					continue;
				}

				break;
			}

			_position = cur.getPosition() + cur.getDirection() * _offset;
			_angle = cur.getAngle();
			_current = cur;
		}

		// 우왕 버그많음
		//public void moveBackward(float delta)
		//{
		//	_offset -= delta;
		//	float local_remain_offset = _offset - _current.getPrevAccumLength();
		//	MBAnchor cur = _current;
		//	while(cur != null)
		//	{
		//		if( local_remain_offset > 0)
		//		{
		//			_position = cur.getPosition() + cur.getDirection() * local_remain_offset;
		//			_angle = cur.getAngle();

		//			_current = cur;
		//			return;
		//		}

		//		if( cur.getPrev() != null)
		//		{
		//			local_remain_offset += cur.getLength();
		//			cur = cur.getPrev();
		//			continue;
		//		}

		//		break;
		//	}

		//	_position = cur.getPosition() + cur.getDirection() * local_remain_offset;
		//	_angle = cur.getAngle();
		//	_current = cur;
		//}
	}
}
