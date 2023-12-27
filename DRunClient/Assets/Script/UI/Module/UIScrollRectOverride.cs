using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIScrollRectOverride : ScrollRect
	{
		private static FieldInfo _fieldDragging;
		private static FieldInfo _fieldPrevPosition;

		public bool isDragging()
		{
			if (_fieldDragging == null)
			{
				//Type type = typeof(ScrollRect);

				//FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				//foreach(FieldInfo field in fields)
				//{
				//	Debug.Log(field.Name);
				//}

				_fieldDragging = typeof(ScrollRect).GetField("m_Dragging", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			}
			//else
			//{
			//	Debug.Log(_fieldDragging.Name);
			//}
			return (bool)_fieldDragging.GetValue(this);
		}

		public Vector2 getPrevPosition()
		{
			if (_fieldPrevPosition == null)
			{
				_fieldPrevPosition = typeof(ScrollRect).GetField("m_PrevPosition", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			}
			//else
			//{
			//	Debug.Log(_fieldPrevPosition.Name);
			//}

			return (Vector2)_fieldPrevPosition.GetValue(this);
		}

		protected override void LateUpdate()
		{
			bool forged = false;
			Vector2 newVelocity = Vector2.zero;
			if (isDragging() && inertia)
			{
				newVelocity = (content.anchoredPosition - getPrevPosition()) / Time.unscaledDeltaTime;
				forged = true;
			}

			base.LateUpdate();

			if (forged)
			{
				velocity = newVelocity;
			}
		}
	}
}
