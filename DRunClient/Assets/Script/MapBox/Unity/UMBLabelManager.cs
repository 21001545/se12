using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBLabelManager
	{
		private UnityMapBox _mapBox;
		private UMBControl _control;
		private RectTransform _labelRoot;
		private Camera _targetCamera;
		private RectTransform _rt;
		private AABBTree _tree;

		//private Dictionary<long, Dictionary<long,UMBLabelObject>> _dicID;
		//private Dictionary<int, List<UMBLabelObject>> _dicTile;
		//private Dictionary<Collider2D, UMBLabel> _colliderMap;
		//private List<UMBLabel> _hidingLabelList;
		//private List<UMBLabel> _hidingLabelDeleteList;
		//private List<UMBLabelObject> _activeList;
		//private List<UMBLabelObject> _addList;
		//private List<UMBLabelObject> _deleteList;

		public struct LayerFeatureKey : IEquatable<LayerFeatureKey>
		{
			public int layer_id;
			public long feature_id;

			public LayerFeatureKey(MBLayerRenderData layer,MBFeature feature)
			{	
				layer_id = layer.getLayerStyle().getIDHashCode();
				feature_id = feature.id;
			}

			public bool Equals(LayerFeatureKey key)
			{
				return layer_id == key.layer_id && feature_id == key.feature_id;
			}

			public override int GetHashCode()
			{
				return (int)feature_id;
			}
		}

		private Dictionary<MBLayerRenderData, List<UMBSymbolPlacement>> _placementDic;
		private Dictionary<int, List<UMBSymbolPoint>> _compareNameDic;

		private List<UMBSymbolRenderer> _rendererList;
		private List<UMBSymbolRenderer> _hidingRendererList;
		private List<UMBSymbolRenderer> _tempRendererList;

		private int _transformRevision;
		private IntervalTimer _visibleTimer;
		private IntervalTimer _collisionTimer;

		private bool _enable;
		private int _remainRendererCreationCount;

		private int _totalPlacementCount;

		public AABBTree getTree()
		{
			return _tree;
		}

		public int getRemainRendererCreationCount()
		{
			return _remainRendererCreationCount;
		}

		public int getTotalPlacementCount()
		{
			return _totalPlacementCount;
		}

		public int getRendererCount()
		{
			return _rendererList.Count + _hidingRendererList.Count;
		}

		public static UMBLabelManager create(UnityMapBox mapBox)
		{
			UMBLabelManager m = new UMBLabelManager();
			m.init(mapBox);
			return m;
		}

		private void init(UnityMapBox mapBox)
		{
			_mapBox = mapBox;
			_control = mapBox.getControl();
			_targetCamera = mapBox.getTargetCamera();
			_rt = _mapBox.transform as RectTransform;
			_labelRoot = _mapBox.label_root;

			_placementDic = new Dictionary<MBLayerRenderData, List<UMBSymbolPlacement>>();
			_rendererList = new List<UMBSymbolRenderer>();
			_hidingRendererList = new List<UMBSymbolRenderer>();
			_tempRendererList = new List<UMBSymbolRenderer>();
			_compareNameDic = new Dictionary<int, List<UMBSymbolPoint>>();

			_transformRevision = 1;
			_visibleTimer = IntervalTimer.create(0.25f, true, false);
			_collisionTimer = IntervalTimer.create(0.15f, true, false);
			_tree = new AABBTree();
			_enable = false;
			
		}

		public void setEnable(bool b)
		{
			_enable = b;

			if( _enable == false)
			{
				cleanUp();
			}
		}

		public void update()
		{
			if( _enable == false)
			{
				return;
			}

			if( _visibleTimer.update())
			{
				updatePlacements();
			}

			if( _transformRevision != _control.getTransformRevision())
			{
				updateRenderersPosition();
				_transformRevision = _control.getTransformRevision();
				//updateActivePositions();
				//updateHidingPositions();
			}

			updateRenderers();
			//updateActives();
			//updateHidingLabels();

			if (_collisionTimer.update())
			{
				checkRenderersCollision();
				//checkCollision();
			}
		}

		// 물리 엔진과 sync를 맞추자
		public void updateFixed()
		{

		}

		public void register(UMBTile owner,MBLayerRenderData layer)
		{
			//List<UMBLabelObject> list;
			
			//if( _dicTile.TryGetValue(owner.GetInstanceID(), out list) == false)
			//{
			//	list = new List<UMBLabelObject>();
			//	_dicTile.Add(owner.GetInstanceID(), list);
			//}

			List<UMBSymbolPlacement> placementList;
			if (_placementDic.TryGetValue(layer, out placementList) == false)
			{
				placementList = new List<UMBSymbolPlacement>();
				_placementDic.Add(layer, placementList);
			}

			_compareNameDic.Clear();

			foreach (MBFeature feature in layer.getFeatureList())
			{
				UMBSymbolPlacement placement = UMBSymbolPlacement.create(_control, owner, layer, feature, placementList.Count);
				if( placement.getPointList().Count == 0)
				{
					continue;
				}

				placementList.Add(placement);
			}

			//.LogWarning(string.Format("register[{0}] : dicTile[{1}]", owner.GetInstanceID(), _dicTile.Count));
			calceTotalPlacementCount();
		}

		public void unregister(UMBTile owner,MBLayerRenderData layer)
		{
			List<UMBSymbolPlacement> placementList;
			if( _placementDic.TryGetValue(layer, out placementList) == false)
			{
				return;
			}

			foreach (UMBSymbolPlacement placement in placementList)
			{
				placement.delete();
			}

			_placementDic.Remove(layer);
			calceTotalPlacementCount();
		}

		private void calceTotalPlacementCount()
		{
			_totalPlacementCount = 0;
			foreach(KeyValuePair<MBLayerRenderData,List<UMBSymbolPlacement>> item in _placementDic)
			{
				_totalPlacementCount += item.Value.Count;
			}
		}

		private void updateRenderersPosition()
		{
			foreach(UMBSymbolRenderer renderer in _rendererList)
			{
				renderer.updatePosition();
			}

			foreach(UMBSymbolRenderer renderer in _hidingRendererList)
			{
				renderer.updatePosition();
			}
		}

		private void checkRenderersCollision()
		{
			foreach(UMBSymbolRenderer renderer in _rendererList)
			{
				renderer.checkCollision();
			}
        }

		private void updateRenderers()
		{
			foreach(UMBSymbolRenderer renderer in _rendererList)
			{
				renderer.update();
			}

			
			foreach(UMBSymbolRenderer renderer in _hidingRendererList)
			{
				renderer.update();
				if( renderer.isHidingComplete())
				{
					renderer.delete();
					_tempRendererList.Add(renderer);
				}
			}

			if( _tempRendererList.Count > 0)
			{
				foreach(UMBSymbolRenderer renderer in _tempRendererList)
				{
					_hidingRendererList.Remove(renderer);
				}
				_tempRendererList.Clear();
			}
		}

		public void updatePlacements()
		{
			_remainRendererCreationCount = 20;

			foreach (KeyValuePair<MBLayerRenderData, List<UMBSymbolPlacement>> layerItem in _placementDic)
			{
				foreach(UMBSymbolPlacement placement in layerItem.Value)
				{
					placement.update();
				}
			}
		}

		void cleanUp()
		{
			_tree.resetTree();
			foreach(UMBSymbolRenderer renderer in _rendererList)
			{
				UMBSymbolPoint point = renderer.getSymbolPoint();
				
				point.setVisible(false);
				point.setRenderer(null);

				renderer.delete();
			}

			foreach(UMBSymbolRenderer renderer in _hidingRendererList)
			{
				renderer.delete();
			}

			_rendererList.Clear();
			_hidingRendererList.Clear();
		}

		public UMBSymbolRenderer createSymbolRenderer()
		{
			UMBSymbolRenderer renderer = _mapBox.symbol_source.make<UMBSymbolRenderer>(_labelRoot, GameObjectCacheType.actor);
			_rendererList.Add(renderer);
			_remainRendererCreationCount--;
			return renderer;
		}

		public void removeSymbolRenderer(UMBSymbolRenderer renderer)
		{
			_rendererList.Remove(renderer);
			_hidingRendererList.Add(renderer);

            renderer.startDelete();
		}

		private Vector3 tileToWorldPosition(MBTileCoordinateDouble tilePos)
		{
			MBTileCoordinate centerTilePos = _control.getCurrentTilePos();

			if (centerTilePos.zoom != tilePos.zoom)
			{
				double scale = _control.calcTileScale(centerTilePos.zoom, tilePos.zoom);
				tilePos.zoom = centerTilePos.zoom;
				tilePos.tile_x *= scale;
				tilePos.tile_y *= scale;
			}

			double offset_tile_x = tilePos.tile_x - centerTilePos.tile_x;
			double offset_tile_y = tilePos.tile_y - centerTilePos.tile_y;

			Vector3 pos = Vector3.one;
			pos.x = (float)(offset_tile_x * 4096);
			pos.y = -(float)(offset_tile_y * 4096);
			pos.z = 0;

			return _control.getTileRoot().TransformPoint(pos);
		}


		public void onDrawGizmos()
		{
			foreach(KeyValuePair<MBLayerRenderData,List<UMBSymbolPlacement>> layerItem in _placementDic)
			{
				foreach(UMBSymbolPlacement placement in layerItem.Value)
				{
					placement.onDrawGizmos();
				}
			}
		}

		public bool symbolPointIsTooClose(int name_key,UMBSymbolPoint point,float repeatDistance)
		{
			List<UMBSymbolPoint> pointList;
			if( _compareNameDic.TryGetValue( name_key, out pointList) == false)
			{
				pointList = new List<UMBSymbolPoint>();
				pointList.Add(point);
				_compareNameDic.Add(name_key, pointList);
				return false;
			}
			else
			{
				bool result = false;
				foreach(UMBSymbolPoint exist_point in pointList)
				{
					float distance = (float)(point.getTilePos().tile_pos - exist_point.getTilePos().tile_pos).magnitude * MapBoxDefine.tile_extent;
					if (distance < repeatDistance)
					{
						result = true;
						break;
					}
				}

				if( result == false)
				{
					pointList.Add(point);
				}
				return result;
			}
		}

	}
}
