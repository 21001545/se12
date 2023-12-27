using UnityEngine;

namespace Festa.Client.MapBox
{
	public class PolylinePoint
	{
		//public int index;
		public Vector2Int position;
		public Color32 color;

		public PolylinePoint(Vector2Int position, Color32 color)
		{
			this.position = position;
			this.color = color;
		}
	}
}
