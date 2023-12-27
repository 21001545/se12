using UnityEngine;

namespace Festa.Client.Module
{
	public static class GizmoExtension
	{
		public static void drawCircleLine(Vector3 pos, float range, Color c, int segment = 16)
		{
			Gizmos.color = c;
			for (int i = 0; i <= segment; ++i)
			{
				Vector3 v1 = new Vector3();
				Vector3 v2 = new Vector3();

				v1.x = range * Mathf.Sin((float)i * Mathf.PI * 2.0f / (float)segment);
				v1.y = range * Mathf.Cos((float)i * Mathf.PI * 2.0f / (float)segment);

				v2.x = range * Mathf.Sin((float)(i + 1) * Mathf.PI * 2.0f / (float)segment);
				v2.y = range * Mathf.Cos((float)(i + 1) * Mathf.PI * 2.0f / (float)segment);

				Gizmos.DrawLine(pos + v1, pos + v2);
			}
		}

		public static void drawCircleLine(Transform tr,Vector3 pos,float range,Color c,int segment = 16)
		{
			drawCircleLine(tr.TransformPoint(pos), tr.lossyScale.x * range, c, segment);
		}

		public static void drawBoxLine(Transform tr,Vector3 pos,Vector2 extent,Color c)
		{
			Vector3 left_bottom = tr.TransformPoint(new Vector3(pos.x - extent.x, pos.y - extent.y, pos.z));
			Vector3 left_top = tr.TransformPoint(new Vector3(pos.x - extent.x, pos.y + extent.y, pos.z));
			Vector3 right_bottom = tr.TransformPoint(new Vector3(pos.x + extent.x, pos.y - extent.y, pos.z));
			Vector3 right_top = tr.TransformPoint(new Vector3(pos.x + extent.x, pos.y + extent.y, pos.z));

			Gizmos.color = c;
			Gizmos.DrawLine(left_bottom, right_bottom);
			Gizmos.DrawLine(right_bottom, right_top);
			Gizmos.DrawLine(right_top, left_top);
			Gizmos.DrawLine(left_bottom, left_top);
		}
	}
}
