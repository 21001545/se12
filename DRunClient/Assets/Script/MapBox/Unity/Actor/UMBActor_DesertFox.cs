using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBActor_DesertFox : UMBActor
	{
		public Transform pivot;
		public Animator animator;
		public UMBActor_RootMotionHandler _rootMotionHandler;
		public Transform direction;

		public Texture2D party_image;
		public string scene_name;

		private DoubleVector2 _currentPosition;
		private DoubleVector2 _targetPosition;

		private DoubleVector2SmoothDamper _posDamper;
		private AngleSmoothDamper _angleDamper;
		private AngleSmoothDamper _radarAngleDamper;

		private static int _id_speed = Animator.StringToHash("speed");

		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);

			_rootMotionHandler.init();
			_posDamper = DoubleVector2SmoothDamper.create(DoubleVector2.zero, 0.1f, 0.0000001);
			_angleDamper = AngleSmoothDamper.create(0, 0.1f, 0.5f);
			_radarAngleDamper = AngleSmoothDamper.create(0, 0.1f, 0.5f);
		}

		public override void onReused()
		{
			base.onReused();
		}

		public override void init(UnityMapBox mapBox, MBLongLatCoordinate position)
		{
			base.init(mapBox, position);

			_posDamper.reset(position.pos);
			_angleDamper.reset(180);
			_radarAngleDamper.reset(180);

			//pivot.localRotation = _mapBox.rotate_root.localRotation * Quaternion.Euler(0, _angleDamper.getCurrent(), 0);
			//transform.localEulerAngles = new Vector3(0, 0, _angleDamper.getCurrent());

			//Debug.Log($"initPosition:[{position.lon},{position.lat}]");
		}

		public override void changePosition(MBLongLatCoordinate position)
		{
			DoubleVector2 diff = _posDamper.getTarget() - _posDamper.getCurrent();

			double max_speed;
			
			if( diff.magnitude <= 0.00010)
			{
				max_speed = 0.00010;
			}
			else if( diff.magnitude <= 0.00030)
			{
				max_speed = 0.00060;
			}
			else
			{
				max_speed = diff.magnitude / 0.2f;
			}
			
			double duration = diff.magnitude / max_speed;

			_posDamper.setTarget2(position.pos, duration, max_speed);

			//Debug.Log($"changePosition:[{position.lon},{position.lat}] diff[{diff.magnitude}]");
		}

		public void setDirection(float angle)
		{
			_radarAngleDamper.setTarget(angle);
			//_directionDamper.setTarget(angle);
		}

		public override void update()
		{
			if( _posDamper.update())
			{
				_position.pos = _posDamper.getCurrent();
				//Vector3 last_pos = _rt.localPosition;
				updateTransformPosition();
				//Vector3 cur_pos = _rt.localPosition;

				//Vector3 delta_pos = cur_pos - last_pos;
				DoubleVector2 velocity = _posDamper.getLastVelocity();

				//Debug.Log($"delta_pos[{delta_pos.magnitude}] velocity[{velocity.magnitude}]");

				if( velocity.magnitude > 0.00001)
				{
					float angle = Mathf.Atan2(-(float)velocity.y, (float)velocity.x);
					_angleDamper.setTarget(angle * Mathf.Rad2Deg + 90);

					if( velocity.magnitude > 0.00010)
					{
						animator.SetFloat(_id_speed, 6);
					}
					else
					{
						animator.SetFloat(_id_speed, 2);
					}
				}
				else
				{
					animator.SetFloat(_id_speed, 0);
				}
			}
			else
			{
				animator.SetFloat(_id_speed, 0);
			}


			if(_radarAngleDamper.update())
			{
				//	updateTransformPosition();
				direction.localRotation = _mapBox.rotate_root.localRotation * Quaternion.Euler(0, 0, _radarAngleDamper.getCurrent());
			}

			if(_angleDamper.update())
			{
				updateTransformPosition();
			}
		}

		public override void updateTransformPosition()
		{
			_rt.localPosition = calcLocalPosition();
			pivot.localRotation = Quaternion.Euler(0, _angleDamper.getCurrent() - _mapBox.rotate_root.localEulerAngles.z, 0);

		}
	}
}
