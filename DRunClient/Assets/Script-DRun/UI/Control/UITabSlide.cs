using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DRun.Client
{
	public class UITabSlide : MonoBehaviour
	{
		[Serializable]
		public class TabPosition
		{
			public float position;
		}

		public RectTransform tabObject;
		public float smoothTime;
		public TabPosition[] tabPositions;

		private FloatSmoothDamper _positionDamper;
		
		public void Awake()
		{
		}

		public void Update()
		{
			if( _positionDamper.update())
			{
				applyPosition();
			}
		}

		private void init()
		{
			if(_positionDamper == null)
			{
				_positionDamper = FloatSmoothDamper.create(tabObject.anchoredPosition.x, smoothTime, 1.0f);
			}
		}

		public void setTab(int pos,bool updateNow)
		{
			init();

			if ( updateNow)
			{
				_positionDamper.reset(tabPositions[pos].position);
				applyPosition();
			}
			else
			{
				_positionDamper.setTarget(tabPositions[pos].position);
			}
		}

		private void applyPosition()
		{
			Vector2 pos = tabObject.anchoredPosition;
			pos.x = _positionDamper.getCurrent();
			tabObject.anchoredPosition = pos;
		}
	}
}
