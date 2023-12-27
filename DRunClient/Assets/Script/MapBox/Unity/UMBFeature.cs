using System.Collections.Generic;
using Festa.Client.Module;
using UnityEngine;

namespace Festa.Client.MapBox
{
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public abstract class UMBFeature : ReusableMonoBehaviour
	{
		protected UnityMapBox		_mapBox;
		protected long _feature_id;
		protected MBTileCoordinate	_tilePos;

		protected MeshFilter		_mf;
		protected MeshRenderer		_mr;

		protected List<MBFeature> _feature_list;

		public long getFeatureID()
		{
			return _feature_id;
		}

		public virtual void init(UnityMapBox owner,long feature_id,MBTileCoordinate tilePos,Material mat,int sorting_layer_id,int sorting_order)
		{
			_mapBox = owner;
			_tilePos = tilePos;
			_feature_id = feature_id;

			_mr.sharedMaterial = mat;
			_mr.sortingLayerID = sorting_layer_id;
			_mr.sortingOrder = sorting_order;
		}

		public virtual void setupPosition(MBTileCoordinate centerTilePos)
		{
			Vector2Int position = new Vector2Int();
			position.x = (_tilePos.tile_x - centerTilePos.tile_x) * 4096;
			position.y = (_tilePos.tile_y - centerTilePos.tile_y) * 4096;

			transform.localPosition = new Vector3(position.x, -position.y);
		}

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_mf = GetComponent<MeshFilter>();
			_mr = GetComponent<MeshRenderer>();

			_feature_list = new List<MBFeature>();
		}

		public override void onReused()
		{
			_feature_list.Clear();
		}

		public virtual void addFeature(MBFeature feature)
		{
			_feature_list.Add(feature);

			startUpdateMesh();
		}

		public virtual void removeFeature(MBFeature feature)
		{
			_feature_list.Remove(feature);

			startUpdateMesh();
		}

		//
		protected static List<MBFeature> _delete_list = new List<MBFeature>();
	
		public virtual void removeFeatureByTile(MBTile tile)
		{
			_delete_list.Clear();
			foreach(MBFeature feature in _feature_list)
			{
				if( feature._tile == tile)
				{
					_delete_list.Add(feature);
				}
			}

			foreach(MBFeature feature in _delete_list)
			{
				_feature_list.Remove(feature);
			}

			startUpdateMesh();
		}

		public abstract void updateMesh();
		public abstract void startUpdateMesh();
	}
}
