using UnityEngine;

namespace Festa.Client.MapBox
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class UMBSVGImage : MonoBehaviour
	{
		private SpriteRenderer _sr;
//		private int _id = -1;
	//	private static MaterialPropertyBlock _pb;

		public void setup(Sprite sprite)
		{
			if( _sr == null)
			{
				_sr = GetComponent<SpriteRenderer>();
			}

			//if( _id == -1)
			//{
			//	_id = Shader.PropertyToID("_RendererColor");
			//}

			//if( _pb == null)
			//{
			//	_pb = new MaterialPropertyBlock();
			//}

			_sr.sortingOrder = 100;
			_sr.sprite = sprite;

			//_sr.GetPropertyBlock(_pb);
			//_pb.SetColor(_id, color);
			//_sr.SetPropertyBlock(_pb);
		}

	}
}
