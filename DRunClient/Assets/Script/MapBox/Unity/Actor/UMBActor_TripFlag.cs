using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client.MapBox
{
	public class UMBActor_TripFlag : UMBActor
	{
		public Image image;
		public Sprite[] sprites;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);

			_canPick = false;
		}

		public void setup(int sprite_index)
		{
			image.sprite = sprites[sprite_index];
		}
	}
}
