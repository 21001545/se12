using Festa.Client.Module;
using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Festa.Client.MapBox
{
	public class UMBTile : ReusableMonoBehaviour
	{
		public GameObject pivot;
		public Transform layer_mesh_root;
		public Transform land_mesh_root;

		private MBTileCoordinate _tilePos;

		private MBTile _tileData;
		private List<MBLayerRenderData> _layerRenderList;
		private LandTile _landTileData;
		private UnityMapBox _mapBox;
		private UMBControl _control;
		private List<UMBLayerMeshRender> _layerMeshList;
		//private MBLayerRenderData _buildingLayerRenderData;
		private bool _loadingTile;
		private int _loaderCallID;
		private UIBindingManager _uiBindingManager;
		private int _mapRevealRevision;

		private UMBLayerMeshRender _landTileMeshRender;
		private MBStyle _mbStyle;

		private int _stencilID;

		//
		private List<LandTileFeature> _revealedLandTileFeatureList;
		private List<LandTileFeature> _addLandTileFeatureList;
		private List<UMBDeco> _mapDecoList;
		private List<UMBDeco> _updatingDecoList;
		private List<UMBDeco> _deleteDecoList;
		private bool _buildLandTileRenderer;

		private UMBMapDecoSourceContainer _decoSourecContainer => _mapBox.styleData.mapDecoSourceContainer;
		private UMBViewModel ViewModel => _mapBox.getViewModel();

		private static MBStyleExpressionContext _exContext = new MBStyleExpressionContext();

		//
		private static int _last_load_call_id = 0;
		private static int _id_Stencil = Shader.PropertyToID("_Stencil");

		public UnityMapBox getMapBox()
		{
			return _mapBox;
		}

		public bool isLoadingTile()
		{
			return true;
		}

		public MBStyle getMBStyle()
		{
			return _mbStyle;
		}

		public int getStencilID()
		{
			return _stencilID;
		}

		public List<LandTileFeature> getRevealedLandTileFeatureList()
		{
			return _revealedLandTileFeatureList;
		}

		//public MBLayerRenderData getBuildingLayerRenderData()
		//{
		//	return _buildingLayerRenderData;
		//}

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_layerMeshList = new List<UMBLayerMeshRender>();
			_mapDecoList = new List<UMBDeco>();
			_updatingDecoList = new List<UMBDeco>();
			_deleteDecoList = new List<UMBDeco>();
			_loadingTile = false;
			_uiBindingManager = UIBindingManager.create();

			_revealedLandTileFeatureList = new List<LandTileFeature>();
			_addLandTileFeatureList = new List<LandTileFeature>();
			_stencilID = 0;
			_buildLandTileRenderer = false;
		}

		public override void onReused()
		{
			_loadingTile = false;
			_buildLandTileRenderer = false;
			deleteLayerMeshs();
		}

		public override void onDelete()
		{
		}

		public void setup(UnityMapBox mapBox,MBTileCoordinate tilePos)
		{
			_mapBox = mapBox;
			_control = _mapBox.getControl();
			_tilePos = tilePos;
			_tileData = null;
			_landTileData = null;
			_loadingTile = false;
			pivot.gameObject.SetActive(false);
			_mapRevealRevision = 0;
			_stencilID = _control.getStencilIDManager().allocID();

			updateGameObjectName();

			UMBViewModel vm = _mapBox.getViewModel();

			// 2022.01.24
			if ( tilePos.zoom == MapBoxDefine.landTileGridZoom && mapBox.isOfflineMode() == false)
			{
				UMBMapRevealViewModel vmReveal = vm.MapReveal;
				
				// 2022.08.08 MainTab UIMap이 아닌 곳에서는 null일 수 있다

				if( vmReveal != null)
				{
					_uiBindingManager.makeBinding(vmReveal, nameof(vmReveal.RevealData), onUpdateMapReveal);
					_uiBindingManager.makeBinding(vmReveal, nameof(vmReveal.RevealData_Adjacency), onUpdateMapReveal_Adjacency);
					_uiBindingManager.makeBinding(vm, nameof(vm.ShowMapDeco), onUpdateShowMapDeco);
				}
			}
			
			//_uiBindingManager.makeBinding(vm, nameof(vm.ExtrudeRatio), onUpdateExtrudeRatio);
			_uiBindingManager.makeBinding(vm, nameof(vm.Zoom), onUpdateZoom);

			_uiBindingManager.updateAllBindings();
		}

		// 데이터는 동일한데 Tile위치만 다른 경우 (Wrapping모드)
		public void resetTilePos(MBTileCoordinate tilePos)
		{
			_tilePos = tilePos;
			updateGameObjectName();
		}

		private void updateGameObjectName()
		{
			gameObject.name = $"{_tilePos.zoom}/{_tilePos.tile_x}/{_tilePos.tile_y}" + (_tileData != null ? "- !!" : "");
		}

		public void setupPosition(MBTileCoordinate centerTilePos)
		{
			if( _tilePos.zoom == centerTilePos.zoom)
			{
				Vector2Int offset = _tilePos.offsetFrom(centerTilePos);
				transform.localPosition = new Vector3(offset.x * MapBoxDefine.tile_extent, -offset.y * MapBoxDefine.tile_extent, 0);
				transform.localScale = Vector3.one;
			}
			else
			{
				double scale = _control.calcTileScale(centerTilePos.zoom,_tilePos.zoom);

				float offset_x = (float)(_tilePos.tile_x * scale) - centerTilePos.tile_x;
				float offset_y = (float)(_tilePos.tile_y * scale) - centerTilePos.tile_y;

				transform.localPosition = new Vector3(offset_x * MapBoxDefine.tile_extent, -offset_y * MapBoxDefine.tile_extent);
				transform.localScale = new Vector3((float)scale, (float)scale, 1.0f);

				//int tile_x = Mathf.CeilToInt((float)(_tilePos.tile_x * scale));
				//int tile_y = Mathf.CeilToInt((float)(_tilePos.tile_y * scale));

				//Debug.Log($"{_tilePos.zoom}/{_tilePos.tile_x}/{_tilePos.tile_y}->{centerTilePos.zoom}/{tile_x}/{tile_y}");

				//Vector2Int offset = new Vector2Int();
				//offset.x = tile_x - centerTilePos.tile_x;
				//offset.y = tile_y - centerTilePos.tile_y;

				//transform.localPosition = new Vector3(offset.x * MapBoxDefine.tile_extent, -offset.y * MapBoxDefine.tile_extent, 0);
			}

		}

		public void update()
		{
			if( _tileData == null)
			{
				if( _loadingTile == false)
				{
					checkNLoadTile();
				}
			}
			else
			{
				processVisible();
				updateLandTileRenderer();
				updateDecoList();
			}
		}

		public bool isVisible()
		{
			Bounds bounds = calcWorldBounds();
			return _control.getCameraBound().isVisible(bounds);
		}

		private void checkNLoadTile()
		{
			if( isVisible() == false)
			{
				return;
			}

			_loaderCallID = ++_last_load_call_id;

			_loadingTile = true;
			_mbStyle = _mapBox.getMBStyle(_mapBox.getControl().getStyleID());
			//Debug.Log($"loadTile: {_tilePos.zoom}/{_tilePos.tile_x}/{_tilePos.tile_y}");

			int start_time = System.Environment.TickCount;
			//Debug.Log($"[{_tilePos.ToString()}] start");

			MBTileLoader loader = MBTileLoader.create(GlobalConfig.fileserver_url, _tilePos, _mbStyle);
			loader.run(_loaderCallID, (call_id, result) => {

				int delta_time = System.Environment.TickCount - start_time;
				if (delta_time > 300)
				{
					Debug.Log($"[{_tilePos.ToString()}] end {delta_time}ms");
				}

				if ( result == false || call_id != _loaderCallID || isInCache())
				{
					_loadingTile = false;
					return;
				}

				MBTile tile = loader.getTile();

				// 2022.07.15 타일 로딩이 느릴경우 뭔가 꼬이는것 같은데..
				if (MBTileCoordinate.Equals( tile._tilePos, _tilePos.getValidByWrap()) == false)
				{
					Debug.Log($"discard tiledata: pos[{_tilePos}] data[{tile._tilePos}]");
					_loadingTile = false;
					return;
				}

				// 2022.11.01 이강희
				// D-Run은 LandTile이 필요 없다

				//if ( _tilePos.zoom < 16)
				//{
					_loadingTile = false;
					setTileData(tile, null);
				//}
				//else
				//{
				//	LandTileLoader landtile_loader = LandTileLoader.create(GlobalConfig.fileserver_url, _tilePos);
				//	landtile_loader.run(_loaderCallID, (land_call_id, land_result) => {
				//		if (land_result == false || land_call_id != _loaderCallID || isInCache())
				//		{
				//			return;
				//		}

				//		LandTile landtile = landtile_loader.getTile();

				//		if (MBTileCoordinate.Equals( landtile._tilePos, _tilePos.getValidByWrap()) == false)
				//		{
				//			Debug.Log($"discard land tiledata: pos[{_tilePos}] data[{tile._tilePos}]");
				//			_loadingTile = false;
				//			return;
				//		}

				//		_loadingTile = false;
				//		setTileData(tile, landtile);
				//	});
				//}
			});
		}

		public void onChangeStyle()
		{
			// 일단 tiledata를 날려보자
			if( _loadingTile)
			{
				_loaderCallID = 0;
			}

			if( _tileData != null)
			{
				//deleteLayerMeshs();
				unregisterLabel();
				_tileData = null;
				_landTileData = null;
				_mapRevealRevision = 0;
//				_revealedLandTileFeatureList.Clear();
			}
		}

		private void processVisible()
		{
			bool result = isVisible();
			if( result && pivot.activeSelf == false)
			{
				//if( _tileData != null)
				//{
				//	Debug.Log($"show: [{Time.frameCount}][{_tilePos}][{_tilePos.getValidByWrap()}]");
				//}
				pivot.SetActive(result);
			}
			else if (result == false && pivot.activeSelf)
			{
				//if( _tileData != null)
				//{
				//	Debug.Log($"hide: [{Time.frameCount}][{_tilePos}][{_tilePos.getValidByWrap()}]");
				//}
				pivot.SetActive(result);
			}
		}

		private void setTileData(MBTile tile,LandTile landTile)
		{
			deleteLayerMeshs();
			unregisterLabel();

			_tileData = tile;
			_landTileData = landTile;
			buildLayerRenderers();

			_revealedLandTileFeatureList.Clear();

			if ( _mapBox.isOfflineMode() == false)
			{
				onUpdateMapReveal(null);
				onUpdateMapReveal_Adjacency(null);
			}

			updateMaterialParams();
			updateGameObjectName();
		}

		public MBTile getTileData()
		{
			return _tileData;
		}

		public LandTile getLandTileData()
		{
			return _landTileData;
		}

		public MBTileCoordinate getTilePos()
		{
			return _tilePos;
		}

		private void buildLayerRenderers()
		{
			//_buildingLayerRenderData = null;
			_layerRenderList = null;
			if ( _tileData._dicStyleRenderData.TryGetValue( _mbStyle, out _layerRenderList) == false)
			{
				return;
			}

			createLayerMeshRender(layer_mesh_root)
					.setupBackground(this, _mbStyle.getBackgroundColor());

			//foreach (MBLayer layer in _tileData.layers)
			foreach (MBLayerRenderData layer in _layerRenderList)
			{
				MBStyleLayer layer_style = layer.getLayerStyle();

				if ( layer_style.getType() == MBStyleDefine.LayerType.symbol)
				{
					_mapBox.getLabelManager().register(this, layer);
				}
				else if (layer.hasValidMergedMesh())
				{
					createLayerMeshRender(layer_mesh_root)
						.setup(this, layer);
				}
			}
		}

		protected UMBLayerMeshRender createLayerMeshRender(Transform parent)
		{
			UMBLayerMeshRender lm =_mapBox.layer_mesh_source.make<UMBLayerMeshRender>(parent, GameObjectCacheType.actor);
			_layerMeshList.Add(lm);
			return lm;
		}

		private UMBLayerMeshRender buildMergedMeshObject(MBMesh merged_mesh,Transform parent,int sorting_order,Material matSource,string name,int layer)
		{
			//Material layer_mat = prepareLayerMaterial(matSource);

			UMBLayerMeshRender lm = _mapBox.layer_mesh_source.make<UMBLayerMeshRender>(parent, GameObjectCacheType.actor);
			lm.setupLegacy(merged_mesh, sorting_order, matSource, name, layer);

			_layerMeshList.Add(lm);

			return lm;
		}

		private void deleteLayerMeshs()
		{
			GameObjectCache.getInstance().delete(_layerMeshList);
			GameObjectCache.getInstance().delete(_mapDecoList);
			_updatingDecoList.Clear();
			_landTileMeshRender = null;
		}

		private void unregisterLabel()
		{
			if (_tileData != null)
			{
				foreach (MBLayerRenderData layer in _layerRenderList)
				{
					MBStyleLayer layer_style = layer.getLayerStyle();

					if (layer_style.getType() == MBStyleDefine.LayerType.symbol)
					{
						_mapBox.getLabelManager().unregister(this, layer);
					}
				}
			}
		}

		public void delete()
		{
			deleteLayerMeshs();
			unregisterLabel();

			_uiBindingManager.clearAllBindings();

			_tileData = null;
			_landTileData = null;
			if (_stencilID > 0)
			{
				_control.getStencilIDManager().freeID(_stencilID);
			}

			GameObjectCache.getInstance().delete(this);
		}

		private void onUpdateShowMapDeco(object obj)
		{
			_buildLandTileRenderer = true;

			bool showMapDeco = ViewModel.ShowMapDeco;
			if( showMapDeco)
			{
				foreach(LandTileFeature feature in _revealedLandTileFeatureList)
				{
					createMapDeco(feature);
				}
			}
			else
			{
				GameObjectCache.getInstance().delete(_mapDecoList);
				_updatingDecoList.Clear();
			}

			//bool showMapDeco = ViewModel.ShowMapDeco;
			//if( showMapDeco)
			//{
			//	// onUpdateMapReveal에서 다시 만듬
			//	_mapRevealRevision = 0;
			//	onUpdateMapReveal(null);
			//}
			//else
			//{
			//	// 기존 deco를 다 지워야 한다
			//	_revealedLandTileFeatureList.Clear();
			//	GameObjectCache.getInstance().delete(_mapDecoList);
			//	if( _landTileMeshRender != null)
			//	{
			//		_layerMeshList.Remove(_landTileMeshRender);
			//		GameObjectCache.getInstance().delete(_landTileMeshRender);
			//		_landTileMeshRender = null;
			//	}
			//	_updatingDecoList.Clear();
			//}
		}

		/* 
			추가해야될 LandTile Feature 계산하는 방법

			1. Grid상에 유저가 실제로 밟았을 경우 feature추가
			2. 인접 타일의 경계면 feature와 맞 닿아 있는 feature추가
		*/

		private void onUpdateMapReveal(object obj)
		{
			UMBMapRevealViewModel vmMapReveal = _mapBox.getViewModel().MapReveal;
			if (vmMapReveal == null)
			{
				return;
			}

			MapTileRevealData data = vmMapReveal.get(_tilePos);
			if (data == null)
			{
				return;
			}

			if (data.getRevision() == _mapRevealRevision)
			{
				return;
			}

			// 데이터가 없음
			if ( _landTileData == null)
			{
				return;
			}

			_mapRevealRevision = data.getRevision();

			// 2022.6.3 MapDeco On/Off
			if ( ViewModel.ShowMapDeco == false)
			{
				return;
			}

			_addLandTileFeatureList.Clear();
			foreach (int key in data.getRevealData())
			{
				List<LandTileFeature> featureList;
				if (_landTileData.featureGrid.TryGetValue(key, out featureList))
				{
					foreach (LandTileFeature feature in featureList)
					{
						if (feature._outlinePaths == null)
						{
							continue;
						}

						if (_revealedLandTileFeatureList.Contains(feature) == false)
						{
							_addLandTileFeatureList.Add(feature);
						}
					}
				}
			}

			// 변화 없음
			if (_addLandTileFeatureList.Count == 0)
			{
				return;
			}

			_revealedLandTileFeatureList.AddRange(_addLandTileFeatureList);
			
			foreach(LandTileFeature feature in _addLandTileFeatureList)
			{
				createMapDeco(feature);
			}
			
			_addLandTileFeatureList.Clear();

			_buildLandTileRenderer = true;
		}

		private void onUpdateMapReveal_Adjacency(object obj)
		{
			if( _landTileData == null)
			{
				return;
			}

			_addLandTileFeatureList.Clear();

			revealFromAdjacencyTile(LandTileEdge.EdgeType.left, _tilePos.offset(-1, 0));
			revealFromAdjacencyTile(LandTileEdge.EdgeType.right, _tilePos.offset(1, 0));
			revealFromAdjacencyTile(LandTileEdge.EdgeType.top, _tilePos.offset(0, -1));
			revealFromAdjacencyTile(LandTileEdge.EdgeType.bottom, _tilePos.offset(0, 1));

			if ( _addLandTileFeatureList.Count == 0)
			{
				return;
			}

			_revealedLandTileFeatureList.AddRange(_addLandTileFeatureList);
			foreach(LandTileFeature feature in _addLandTileFeatureList)
			{
				createMapDeco(feature);
			}
			_buildLandTileRenderer = true;
		}

		private void revealFromAdjacencyTile(int edge_type,MBTileCoordinate tilePos)
		{
			UMBTile targetTile = _control.findTile(tilePos);
			if( targetTile == null || targetTile.getLandTileData() == null)
			{
				return;
			}

			LandTile targetLandTile = targetTile.getLandTileData();
			MultiDictionary<LandTileFeature, LandTileFeature> edgeFacingFeatureMap = _landTileData.getEdgeFacingFeature(edge_type, targetLandTile);
			foreach (LandTileFeature feature in targetTile.getRevealedLandTileFeatureList())
			{
				List<LandTileFeature> feature_list = edgeFacingFeatureMap.get(feature);
				if( feature_list == null)
				{
					continue;
				}

				foreach(LandTileFeature this_feature in feature_list)
				{
					if(_revealedLandTileFeatureList.Contains(this_feature) == false)
					{
						_addLandTileFeatureList.Add(this_feature);
					}
				}
			}
			
		
		}

		private void updateLandTileRenderer()
		{
			if(_buildLandTileRenderer == false)
			{
				return;
			}

			_buildLandTileRenderer = false;

			if( _revealedLandTileFeatureList.Count > 0 && ViewModel.ShowMapDeco)
			{
				if( _landTileMeshRender == null)
				{
					_landTileMeshRender = createLayerMeshRender(land_mesh_root);
				}

				MBMesh mesh = MBMesh.create();
				foreach(LandTileFeature feature in _revealedLandTileFeatureList)
				{
					mesh.append(feature._mesh);
				}
				_landTileMeshRender.setupLandTile(this, mesh);

				_exContext._zoom = _mapBox.getViewModel().Zoom;
				_landTileMeshRender.updateMaterialParams(_exContext);
			}
			else
			{
				if (_landTileMeshRender != null)
				{
					_layerMeshList.Remove(_landTileMeshRender);
					GameObjectCache.getInstance().delete(_landTileMeshRender);
					_landTileMeshRender = null;
				}
			}
		}

		public Bounds calcWorldBounds()
		{
			Vector3 vMin = Vector3.zero;
			Vector3 vMax = Vector3.zero;

			for (int i = 0; i < 4; ++i)
			{
				Vector3 world_pos = transform.TransformPoint(UMBDefine.tileBoundVerts[i]);

				if (i == 0)
				{
					vMin = vMax = world_pos;
				}
				else
				{
					vMin.x = Mathf.Min(world_pos.x, vMin.x);
					vMin.y = Mathf.Min(world_pos.y, vMin.y);
					vMin.z = Mathf.Min(world_pos.z, vMin.z);

					vMax.x = Mathf.Max(world_pos.x, vMax.x);
					vMax.y = Mathf.Max(world_pos.y, vMax.y);
					vMax.z = Mathf.Max(world_pos.z, vMax.z);
				}
			}

			Vector3 vCenter = (vMax + vMin) / 2.0f;
			Vector3 vSize = (vMax - vMin);

			return new Bounds(vCenter, vSize);
		}

		private void createMapDeco(LandTileFeature feature)
		{
			if (feature._outlinePaths == null)
			{
				return;
			}

			RandomGenerator random = RandomGenerator.create(feature._id);
			
			List<UMBDeco> decoList = new List<UMBDeco>();

			createMapDeco_OutlineTree(feature, random, decoList);
			createMapDeco_Building(feature, random, decoList);
			_mapDecoList.AddRange(decoList);
			_updatingDecoList.AddRange(decoList);
		}

		private void createMapDeco_OutlineTree(LandTileFeature feature,RandomGenerator random,List<UMBDeco> decoList)
		{
			UMBDeco deco_source;

			if( random.nextBoolean(0.5))
			{
				deco_source = _decoSourecContainer.getSource("tree_01").deco_source;
			}
			else
			{
				deco_source = _decoSourecContainer.getSource("tree_02").deco_source;
			}

			float diameter = deco_source.getMBRadius() * 2.1f;

			MBAnchors anchors = MBAnchors.create(feature._outlinePaths[0]);
			MBAnchorCursor cursor = anchors.getCursor(0);
			while (cursor.getOffset() < anchors.getLength())
			{
				Vector2 pos = cursor.getPosition();
				Vector2 dir = cursor.getDirection();

				bool skip = false;

				// vertical
				if( dir.x == 0 && (dir.y == 1 || dir.y == -1))
				{
					if( pos.x <= 80 || pos.x >= (MapBoxDefine.tile_extent - 80))
					{
						skip = true;
					}
				}
				// horizontal

				if( skip == false && dir.y == 0 && (dir.x == 1 || dir.x == -1))
				{
					if (pos.y <= 80 || pos.y >= (MapBoxDefine.tile_extent - 80))
					{
						skip = true;
					}
				}

				if (skip)
				{
					cursor.moveForward(diameter);
					continue;
				}

				//pos += cursor.getDirectionPerpendicular() * 80;

				UMBDeco deco = createMapDeco(deco_source, pos, (float)random.nextDouble() * 360.0f);
				decoList.Add(deco);

				cursor.moveForward(diameter);
			}
		}

		private void createMapDeco_Building(LandTileFeature feature,RandomGenerator random,List<UMBDeco> decoList)
		{
			List<UMBMapDecoSourceContainer.SourceData> noneTreeListSource = _decoSourecContainer.getNoneTreeList();

			List<UMBMapDecoSourceContainer.SourceData> noneTreeList = noneTreeListSource.OrderBy(a => random.nextInt()).ToList();

			int building_count = 3;
			int try_find_position_count = 30;

			for(int i = 0; i < building_count; ++ i)
			{
				UMBDeco deco_source = random.select(noneTreeList).deco_source;

				float radius = deco_source.getMBRadius();
				Vector2 found_pos = Vector2.zero;
				bool found = false;

				for(int j = 0; j < try_find_position_count; ++j)
				{
					Vector2 pos = genRandomPosition(feature, random);
					if( feature.contains(pos) == false)
					{
						continue;
					}

					if( checkDecoOverlap( decoList, pos, radius) == false)
					{
						found = true;
						found_pos = pos;
						break;
					}
				}

				if( found == false)
				{
					continue;
				}

				UMBDeco deco = createMapDeco(deco_source, found_pos,(float)random.nextDouble() * 360.0f);
				decoList.Add(deco);
			}
		}

		private UMBDeco createMapDeco(UMBDeco deco_source,Vector2 pos,float angle)
		{
			UMBDeco deco = GameObjectCache.getInstance().make(deco_source, land_mesh_root, GameObjectCacheType.actor);
			deco.setup(pos, angle);

			return deco;
		}

		private Vector2 genRandomPosition(LandTileFeature feature,RandomGenerator random)
		{
			int x = random.nextInt(feature._bound.min.x, feature._bound.max.x);
			int y = random.nextInt(feature._bound.min.y, feature._bound.max.y);

			return new Vector2(x, y);
		}

		private bool checkDecoOverlap(List<UMBDeco> decoList,Vector2 pos,float radius)
		{
			foreach(UMBDeco deco in decoList)
			{
				if( deco.isOverlap(pos, radius))
				{
					return true;
				}
			}

			return false;
		}

		private void updateDecoList()
		{
			foreach(UMBDeco deco in _updatingDecoList)
			{
				if( deco.update())
				{
					_deleteDecoList.Add(deco);
				}
			}
			
			if( _deleteDecoList.Count > 0)
			{
				foreach(UMBDeco deco in _deleteDecoList)
				{
					_updatingDecoList.Remove(deco);
				}

				_deleteDecoList.Clear();
			}
		}

		public void onBecameOldZoom()
		{
			unregisterLabel();
		}

		//private void onUpdateExtrudeRatio(object o)
		//{
		//	_buildingMaterial.SetFloat(UMBStyle._id_extrudeRatio, ViewModel.ExtrudeRatio);
		//	_buildingZOnlyMaterial.SetFloat(UMBStyle._id_extrudeRatio, ViewModel.ExtrudeRatio);
		//}

		private void onUpdateZoom(object o)
		{
			updateMaterialParams();
		}

		private void updateMaterialParams()
		{
			_exContext._zoom = _mapBox.getViewModel().Zoom;
			foreach (UMBLayerMeshRender render in _layerMeshList)
			{
				render.updateMaterialParams(_exContext);
			}
		}

#if UNITY_EDITOR

		//private static Vector3[] tileBound = new Vector3[4] { 
		//	new Vector3( 0, 0, 0),
		//	new Vector3( 0,  -4096, 0),
		//	new Vector3(  4096,  -4096, 0),
		//	new Vector3(  4096, 0, 0)
		//};

		//private static float tileBoundMarginMin = 0.01f;
		//private static float tileBoundMarginMax = 1.0f - tileBoundMarginMin;

		//private static Vector3[] tileOutlineBound = new Vector3[4] {
		//	new Vector3( 4096 * tileBoundMarginMin, -4096 * tileBoundMarginMin, 0),
		//	new Vector3( 4096 * tileBoundMarginMin, -4096 * tileBoundMarginMax, 0),
		//	new Vector3( 4096 * tileBoundMarginMax, -4096 * tileBoundMarginMax, 0),
		//	new Vector3( 4096 * tileBoundMarginMax, -4096 * tileBoundMarginMin, 0)
		//};

		private static Vector3[] tileBoundVerts = new Vector3[4];

		void OnDrawGizmos()
		{
			if( _mapBox == null)
			{
				return;
			}

			Bounds bounds = calcWorldBounds();

			bool isVisible = _mapBox.getControl().getCameraBound().isVisible(bounds);
			if (isVisible)
			{
				Gizmos.color = Color.green;
			}
			else
			{
				Gizmos.color = Color.red;
			}

			for (int i = 0; i < 4; ++i)
			{
				tileBoundVerts[i] = transform.TransformPoint(UMBDefine.tileBoundVerts[i]);
			}

			Gizmos.DrawLine(tileBoundVerts[0], tileBoundVerts[1]);
			Gizmos.DrawLine(tileBoundVerts[1], tileBoundVerts[2]);
			Gizmos.DrawLine(tileBoundVerts[2], tileBoundVerts[3]);
			Gizmos.DrawLine(tileBoundVerts[3], tileBoundVerts[0]);

			//if(isVisible)
			//{
			//	Gizmos.color = new Color( 0, 0, 1, 0.25f);
			//	for(int i = 1; i < MapBoxDefine.landTileGridCount; ++i)
			//	{
			//		Vector3 begin = transform.TransformPoint(new Vector2(i * MapBoxDefine.landTileGridSize, 0));
			//		Vector3 end = transform.TransformPoint(new Vector2(i * MapBoxDefine.landTileGridSize, -MapBoxDefine.tile_extent));

			//		Gizmos.DrawLine(begin, end);
			//	}

			//	for (int i = 1; i < MapBoxDefine.landTileGridCount; ++i)
			//	{
			//		Vector3 begin = transform.TransformPoint(new Vector2(0, -i * MapBoxDefine.landTileGridSize));
			//		Vector3 end = transform.TransformPoint(new Vector2(MapBoxDefine.tile_extent, -i * MapBoxDefine.landTileGridSize));

			//		Gizmos.DrawLine(begin, end);
			//	}
			//}

			//Gizmos.color = Color.red;

			//foreach(LandTileFeature feature in _revealedLandTileFeatureList)
			//{
			//	foreach(List<ClipperLib.IntPoint> path in feature._outlinePaths)
			//	{
			//		for(int i = 0; i < path.Count; ++i)
			//		{
			//			Vector3 begin = transform.TransformPoint(new Vector2(path[i].X, -path[i].Y));
			//			Vector3 end;// = Vector3.zero;
						
			//			if( i == path.Count - 1)
			//			{
			//				end = transform.TransformPoint(new Vector2(path[0].X, -path[0].Y));
			//			}
			//			else
			//			{
			//				end = transform.TransformPoint(new Vector2(path[i + 1].X, -path[i + 1].Y));
			//			}

			//			Gizmos.DrawLine(begin, end);
			//		}
			//	}
			//}
			
		}
#endif
	}
}
