using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Festa.Client.MapBox
{
	public class MBMesh
	{
		protected List<Vector3> _vertexList;
		protected List<Vector3> _normalList;
		protected List<Color32> _colorList;
		protected List<Vector2> _uvList;
		protected List<ushort> _indexList;

		public List<Vector3> getVertexList()
		{
			return _vertexList;
		}

		public List<Vector3> getNormalList()
		{
			return _normalList;
		}

		public List<Color32> getColorList()
		{
			return _colorList;
		}

		public List<ushort> getIndexList()
		{
			return _indexList;
		}

		public List<Vector2> getUVList()
		{
			return _uvList;
		}

		public virtual int getVertexCount()
		{
			return _vertexList.Count;
		}

		public bool isEmpty()
		{
			return _vertexList.Count == 0;
		}

		public static MBMesh create()
		{
			MBMesh m = new MBMesh();
			m.init();
			return m;
		}

		public static MBMesh create(MBMeshBuilder builder)
		{
			MBMesh mesh = new MBMesh();
			mesh.init();
			mesh.append(builder);
			return mesh;
		}

		public static MBMesh createTileBase()
		{
			MBMesh mesh = new MBMesh();
			mesh.init();
			mesh._vertexList.Add(new Vector3(0, 0, 0));
			mesh._vertexList.Add(new Vector3(0, -1, 0));
			mesh._vertexList.Add(new Vector3(1, -1, 0));
			mesh._vertexList.Add(new Vector3(1, 0, 0));
			mesh._normalList.Add(Vector3.forward);
			mesh._normalList.Add(Vector3.forward);
			mesh._normalList.Add(Vector3.forward);
			mesh._normalList.Add(Vector3.forward);
			mesh._colorList.Add(Color.white);
			mesh._colorList.Add(Color.white);
			mesh._colorList.Add(Color.white);
			mesh._colorList.Add(Color.white);
			mesh._indexList.Add(2);
			mesh._indexList.Add(1);
			mesh._indexList.Add(0);
			mesh._indexList.Add(3);
			mesh._indexList.Add(2);
			mesh._indexList.Add(0);

			return mesh;
		}

		protected virtual void init()
		{
			_vertexList = new List<Vector3>();
			_normalList = new List<Vector3>();
			_colorList = new List<Color32>();
			_indexList = new List<ushort>();
			_uvList = new List<Vector2>();
		}

		public virtual void reset()
		{
			_vertexList.Clear();
			_normalList.Clear();
			_colorList.Clear();
			_indexList.Clear();
			_uvList.Clear();
		}

		public void append(MBMeshBuilder builder)
		{
			append(builder.getVertxList(), builder.getNormalList(), builder.getColorList(), builder.getIndexList(),builder.getUVList());
		}

		public void append(MBMesh mesh)
		{
			append(mesh.getVertexList(), mesh.getNormalList(), mesh.getColorList(), mesh.getIndexList(),mesh.getUVList());
		}

		public void append(List<Vector3> vertexList,List<Vector3> normalList, List<Color32> colorList,List<ushort> indexList,List<Vector2> uvList)
		{
			if( vertexList.Count == 0)
			{
				return;
			}

			ushort begin_index = (ushort)_vertexList.Count;
			_vertexList.AddRange(vertexList);
			_normalList.AddRange(normalList);
			_colorList.AddRange(colorList);
			_uvList.AddRange(uvList);

			for(int i = 0; i < indexList.Count; ++i)
			{
				_indexList.Add( (ushort)(begin_index + indexList[i]));
			}
		}

		//////
		private static MBMesh _tileBaseMesh;

		public static MBMesh getTileBaseMesh()
		{
			if( _tileBaseMesh == null)
			{
				_tileBaseMesh = MBMesh.createTileBase();
			}

			return _tileBaseMesh;
		}

	}
}
