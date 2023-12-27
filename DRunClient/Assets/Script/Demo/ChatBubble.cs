using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client.Demo
{
	public class ChatBubble : ReusableMonoBehaviour
	{
		public Animator animator;
		public TMP_Text txt_message;
		public RectTransform rt_container;
		public RectTransform rt_arrow;

		private RectTransform _rt;
		private Transform _target;
		private Camera _worldCamera;
		private Camera _targetCamera;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_rt = transform as RectTransform;
		}

		public override void onReused()
		{
			animator.Rebind();
			animator.ResetTrigger("hide");
		}

		public void setup(Camera world_camera,Transform target,Camera ui_camera,string text)
		{
			txt_message.text = text;
			_target = target;
			_worldCamera = world_camera;
			_targetCamera = ui_camera;

			//LayoutRebuilder.MarkLayoutForRebuild(_rt);

			updatePosition();
		}

		//void Update()
		//{
		//	if( _remainTime > 0)
		//	{
		//		_remainTime -= Time.deltaTime;

		//		if( _remainTime < 0 )
		//		{
		//			delete();
		//		}
		//	}
		//}

		//private void LateUpdate()
		//{
		//	updatePosition();
		//}

		public void delete()
		{
			animator.SetTrigger("hide");
			StartCoroutine(WaitDelete());
		}

		IEnumerator WaitDelete()
		{
			yield return new WaitForSeconds(0.5f);

			GameObjectCache.getInstance().delete(this);
		}

		void updatePosition()
		{
			_rt.anchoredPosition = GetAnchoredPositionFromWorldPosition(_target.position, _targetCamera);

			StartCoroutine(fitToScreen());
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;

			Vector2 container_min = rt_container.rect.min;
			Vector2 container_max = rt_container.rect.max;

			container_min = rt_container.TransformPoint(container_min);
			container_max = rt_container.TransformPoint(container_max);

			Gizmos.DrawWireCube((container_min + container_max) / 2, (container_max - container_min));
		}

		IEnumerator fitToScreen()
		{
			yield return new WaitForEndOfFrame();

			RectTransform rt_root = transform.parent as RectTransform;

			Vector2 container_min = rt_container.rect.min;
			Vector2 container_max = rt_container.rect.max;

			//Debug.Log($"{container_min},{container_max}");

			container_min = rt_root.InverseTransformPoint(rt_container.TransformPoint(container_min));
			container_max = rt_root.InverseTransformPoint(rt_container.TransformPoint(container_max));

			//Debug.Log($"{container_min},{container_max}");

			container_min -= new Vector2(20, 20);
			container_max += new Vector2(20, 20);

			Rect rect_container = Rect.MinMaxRect(container_min.x, container_min.y, container_max.x, container_max.y);

			Vector2 offset = Vector2.zero;

			//Debug.Log(string.Format($"rt_container:[{rect_container.min},{rect_container.max},{rect_container.center}] rt_root:[{rt_root.rect.min},{rt_root.rect.max},{rt_root.rect.center}]"));

			if( rect_container.x < rt_root.rect.x)
			{
				offset.x = rt_root.rect.x - rect_container.x;
			}
			if( rect_container.xMax > rt_root.rect.xMax)
			{
				offset.x -= rect_container.xMax - rt_root.rect.xMax;
			}

			_rt.anchoredPosition += offset;
			rt_arrow.anchoredPosition = new Vector2( -offset.x, rt_arrow.anchoredPosition.y);

//			Vector2 arrow_pos = rt_arrow.parent.InverseTransformPoint(pos);
//			rt_arrow.anchoredPosition = new Vector2(arrow_pos.x, rt_arrow.anchoredPosition.y);
		}

		public Vector2 GetAnchoredPositionFromWorldPosition(Vector3 _worldPostion, Camera _camera)
		{
			Vector2 viewPos = _camera.WorldToViewportPoint(_worldPostion);
			viewPos.x = viewPos.x * 2 - 1;
			viewPos.y = viewPos.y * 2 - 1;

			RectTransform rt_root = transform.parent as RectTransform;

			return new Vector2(viewPos.x * rt_root.rect.width / 2.0f, viewPos.y * rt_root.rect.height / 2.0f);
		}
	}
}
