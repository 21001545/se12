using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public abstract class MBMeshBuilder
	{
		protected MBFeature _feature;
		protected List<Vector3> _vertexList;
		protected List<Vector3> _normalList;
		protected List<Vector2> _uvList;
		protected List<Color32> _colorList;
		protected List<ushort> _indexList;

		public virtual List<Vector3> getVertxList()
		{
			return _vertexList;
		}

		public virtual List<ushort> getIndexList()
		{
			return _indexList;
		}

		public virtual List<Vector3> getNormalList()
		{
			return _normalList;
		}

		public virtual List<Vector2> getUVList()
		{
			return _uvList;
		}

		public virtual List<Color32> getColorList()
		{
			return _colorList;
		}

		public static T create<T>() where T : MBMeshBuilder, new()
		{
			T builder = new T();
			builder.init();
			return builder;
		}

		protected virtual void reset()
		{
			_vertexList.Clear();
			_normalList.Clear();
			_colorList.Clear();
			_indexList.Clear();
			_uvList.Clear();
		}

		protected virtual void init()
		{
			_vertexList = new List<Vector3>();
			_normalList = new List<Vector3>();
			_colorList = new List<Color32>();
			_uvList = new List<Vector2>();
			_indexList = new List<ushort>();
		}

		protected virtual void makeColors(Color color)
		{
			Color32 c = color;
			for(int i = 0; i < _vertexList.Count; ++i)
			{
				_colorList.Add(c);
			}
		}

		public abstract void build(MBFeature feature,Color color, MBLayerRenderData layer, MBStyleExpressionContext ctx);

	}
}
