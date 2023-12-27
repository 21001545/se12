using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UIPhotoConfirm_Item : ReusableMonoBehaviour
	{
		public UIPhotoThumbnail thumbnail;

		public void setup(RectTransform rtViewport, NativeGallery.NativePhotoContext context)
		{
			thumbnail.setImageFromFile(context);

			RectTransform rt = transform as RectTransform;

			Vector2 sizeDelta = rt.sizeDelta;
			sizeDelta.x = rtViewport.rect.width;
			sizeDelta.y = rtViewport.rect.height;

			rt.sizeDelta = sizeDelta;
		}
	}
}
