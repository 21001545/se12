using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBActor_Party : UMBActor
	{
		public Texture2D party_image;
		public string scene_name;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);
		}

		public override void onReused()
		{
			base.onReused();
		}

	}
}
