using Festa.Client.Module;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public class UMBActor_Avatar : UMBActor
	{
		public GameObject pivot_3d;
		public GameObject pivot_2d;
		public Texture2D party_image;
		public string scene_name;
		public Transform direction;

		private DoubleVector2SmoothDamper _posDamper;
		private AngleSmoothDamper _directionDamper;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);

			_posDamper = DoubleVector2SmoothDamper.create(DoubleVector2.zero, 0.1, 0.00000001);
			_directionDamper = AngleSmoothDamper.create(0, 0.1f, 0.5f);
		}

		public override void onReused()
		{
			base.onReused();
		}

		public override void init(UnityMapBox mapBox, MBLongLatCoordinate position)
		{
			base.init(mapBox, position);

			_posDamper.reset(position.pos);
		}

		public override void changePosition(MBLongLatCoordinate position)
		{
			_posDamper.setTarget(position.pos);
		}

		public void setDirection(float angle)
		{
			_directionDamper.setTarget(angle);
		}

		public override void update()
		{
			if( _posDamper.update())
			{
				_position.pos = _posDamper.getCurrent();
				updateTransformPosition();
			}

			if(_directionDamper.update())
			{
				direction.rotation = Quaternion.Euler(0, 0, _directionDamper.getCurrent());
			}

			checkNUpdateProjectionMode();
		}

		protected override void updateProjectionMode()
		{
			//pivot_2d.SetActive(_lastProjectionMode == UMBDefine.ProjectionMode.two_d);
			//pivot_3d.SetActive(_lastProjectionMode == UMBDefine.ProjectionMode.three_d);
			pivot_2d.SetActive(true);
		}

	}
}
