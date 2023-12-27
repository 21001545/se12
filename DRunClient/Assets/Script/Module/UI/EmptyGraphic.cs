using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class EmptyGraphic : UnityEngine.UI.Graphic
{

	protected EmptyGraphic()
	{
		useLegacyMeshGeneration = false;
	}

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}


}
