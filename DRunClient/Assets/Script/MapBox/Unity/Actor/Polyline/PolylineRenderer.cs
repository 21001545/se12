using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class PolylineRenderer : ReusableMonoBehaviour
	{
		private Mesh _mesh;
		private MeshFilter _mf;
		private MeshRenderer _mr;

		private Material _privateMaterial;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_mf = GetComponent<MeshFilter>();
			_mr = GetComponent<MeshRenderer>();

			_mesh = new Mesh();
			_mesh.name = "polyline";
			_mesh.MarkDynamic();
			_privateMaterial = null;
		}

		public override void onReused()
		{
			if( _privateMaterial != null)
			{
				UnityEngine.Object.Destroy(_privateMaterial);
				_privateMaterial = null;
			}
		}

		public void setup(PolylineMesh meshSource,Material materialSource,float width,Color color,int sortingOrder)
		{
			_privateMaterial = new Material(materialSource);

			_privateMaterial.SetFloat(UMBStyle._id_width, width);
			_privateMaterial.SetColor(UMBStyle._id_color, color);

			_mf.sharedMesh = _mesh;
			_mr.sharedMaterial = _privateMaterial;
			_mr.sortingOrder = sortingOrder;

			_mesh.Clear();
			_mesh.SetVertices(meshSource.getVertexList());
			_mesh.SetNormals(meshSource.getNormalList());
			_mesh.SetColors(meshSource.getColorList());
			_mesh.SetTriangles(meshSource.getIndexList(), 0);
			_mesh.SetUVs(0, meshSource.getUVList());
			_mesh.subMeshCount = 1;
		}
	}
}
