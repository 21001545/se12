using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIFullscreenBlurredImage : RawImage
	{
		protected override void Awake()
		{
			base.Awake();

			uvRect = new Rect(0, 0, (float)(Screen.width - 4) / (float)Screen.height, 1);
		}
	}
}
