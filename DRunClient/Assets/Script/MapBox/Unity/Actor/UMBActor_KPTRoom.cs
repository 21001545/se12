using Festa.Client.MapBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script.MapBox.Unity.Actor
{
	public class UMBActor_KPTRoom : UMBActor
	{
		public string url;
		public Transform pivot;

		public override void updateTransformPosition()
		{
			_rt.localPosition = calcLocalPosition();
		}


	}
}
