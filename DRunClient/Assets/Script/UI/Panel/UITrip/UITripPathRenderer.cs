using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UITripPathRenderer : Graphic
	{
		private PolylineMeshUI _mesh;

		protected UITripPathRenderer()
		{
			useLegacyMeshGeneration = false;
		}

		public void setup(PolylineMeshUI mesh)
		{
			_mesh = mesh;
			SetAllDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if( _mesh != null)
			{
				vh.AddUIVertexStream(_mesh.getUIVertexList(), _mesh.getUIIndexList());
			}
		}
	}
}
