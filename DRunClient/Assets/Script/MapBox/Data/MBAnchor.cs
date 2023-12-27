using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBAnchor
	{
		private Vector2Int _position;
		private Vector2 _direction;
		private Vector2 _directionPerpendicular;
		private float _angle;
		private float _length;
		private float _prev_accum_length;
		private MBAnchor _prev;
		private MBAnchor _next;

		public Vector2Int getPosition()
		{
			return _position;
		}

		public Vector2 getDirection()
		{
			return _direction;
		}

		public Vector2 getDirectionPerpendicular()
		{
			return _directionPerpendicular;
		}

		public float getAngle()
		{
			return _angle;
		}

		public float getLength()
		{
			return _length;
		}

		public float getPrevAccumLength()
		{
			return _prev_accum_length;
		}

		public MBAnchor getPrev()
		{
			return _prev;
		}

		public MBAnchor getNext()
		{
			return _next;
		}

		public void setLink(MBAnchor prev,MBAnchor next)
		{
			_prev = prev;
			_next = next;
		}

		public static MBAnchor create(Vector2Int pos,Vector2 direction,float angle,float length,float prev_accum_length)
		{
			MBAnchor anchor = new MBAnchor();
			anchor._position = pos;
			anchor._direction = direction;
			anchor._directionPerpendicular = Vector2.Perpendicular(direction);
			anchor._angle = angle;
			anchor._length = length;
			anchor._prev_accum_length = prev_accum_length;
		
			return anchor;
		}
	}
}
