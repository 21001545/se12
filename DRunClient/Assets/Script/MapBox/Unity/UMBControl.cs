using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBControl
	{
		private UnityMapBox _mapBox;
		//private UMBTile[] _tiles;
		//private MBTileCoordinate[] _tileGrid;
		private MBTileCoordinate		_currentTilePos;
		private MBTileCoordinate		_updatingTargetTilePos;
		private MBTileCoordinateDouble _targetTilePos;

		private List<UMBTile> _tileList;
		private List<UMBTile> _delTileList;
		private Camera _targetCamera;
		private UMBCameraBound _cameraBound;

		private RectTransform _scroll_root;
		private RectTransform _rotate_root;
		private RectTransform _scale_root;
		private RectTransform _tile_root;
		private RectTransform _building_root;
		//private UMBOutlineBuilding _outlineBuilding;
		private bool _tileUpdating;

		private FloatSmoothDamper _zoomDamper;
		private DoubleVector2SmoothDamper _tileXYDamper;
		private AngleSmoothDamper _XAngleDamper;
		private AngleSmoothDamper _ZAngleDamper;
		private float _ZAngleVelocity;
		private float _ZAngleDecelerationRate = 0.001f;
		private DoubleVector2 _scrollRangeY;
		private int _scrollRangeZoom;

		// 2D/3D 모드 전환 효과용
		private FloatSmoothDamper _ExtrudeScaleDamper;

		private Vector2 _scrollVelocity;

		private List<MBTileCoordinate> _addTileList;
		private List<UMBActor> _actorList;
		private List<UMBTile> _oldZoomTileList;

		//private List<MBLayerRenderData> _outlineLayerList;

		private int _transformRevision;

		//hillShade Opacity
		//private float _lastHillShadeOpacity;
		private MBStyleExpression _exHillShadeOpacity;
		private MBStyleExpressionContext _ctxHillShadeOpacity;
		private UMBTileStencilIDManager _stencilIDManager;

		private UIBindingManager _uiBindingManager;
		private UMBViewModel ViewModel => _mapBox.getViewModel();

		private static Vector2Int[] _tile_offsets = new Vector2Int[]
		{
			new Vector2Int(-1,-1),
			new Vector2Int( 0,-1),
			new Vector2Int( 1,-1),
			new Vector2Int(-1, 0),
			new Vector2Int( 0, 0),
			new Vector2Int( 1, 0),
			new Vector2Int(-1, 1),
			new Vector2Int( 0, 1),
			new Vector2Int( 1, 1)
		};

		private string _styleID;

		public UnityMapBox getMapBox()
		{
			return _mapBox;
		}

		public MBTileCoordinate getCurrentTilePos()
		{
			return _currentTilePos;
		}

		public MBTileCoordinateDouble getTargetTilePos()
		{
			return _targetTilePos;
		}

		public FloatSmoothDamper getZoomDamper()
		{
			return _zoomDamper;
		}

		public AngleSmoothDamper getZAngleDamper()
		{
			return _ZAngleDamper;
		}

		public RectTransform getRotateRoot()
		{
			return _rotate_root;
		}

		public RectTransform getTileRoot()
		{
			return _tile_root;
		}

		public RectTransform getScaleRoot()
		{
			return _scale_root;
		}

		public bool isTileUpdating()
		{
			return _tileUpdating;
		}

		public int getTransformRevision()
		{
			return _transformRevision;
		}

		public List<UMBActor> getActorList()
		{
			return _actorList;
		}

		public Camera getTargetCamera()
		{
			return _targetCamera;
		}

		public UMBCameraBound getCameraBound()
		{
			return _cameraBound;
		}

		public string getStyleID()
		{
			return _styleID;
		}

		public UMBTileStencilIDManager getStencilIDManager()
		{
			return _stencilIDManager;
		}

		public static UMBControl create(UnityMapBox mapBox,string styleID)
		{
			UMBControl c = new UMBControl();
			c.init(mapBox,styleID);
			return c;
		}

		private void init(UnityMapBox mapBox,string styleID)
		{
			_mapBox = mapBox;
			_targetCamera = mapBox.getTargetCamera();
			//_tiles = new UMBTile[9];
			//_tileGrid = new MBTileCoordinate[9];
			_tileList = new List<UMBTile>();
			_delTileList = new List<UMBTile>();
			_scroll_root = mapBox.scroll_root;
			_rotate_root = mapBox.rotate_root;
			_scale_root = mapBox.pivot;
			_tile_root = mapBox.tile_root;
			_building_root = mapBox.building_root;
			//_tileUpdating = false;
			_zoomDamper = FloatSmoothDamper.create(0, 0.10f, 0.001f);
			_tileXYDamper = DoubleVector2SmoothDamper.create(DoubleVector2.zero, 0.1, 1.0 / 4096.0);
			_XAngleDamper = AngleSmoothDamper.create(0, 0.1f, 0.5f);
			_ZAngleDamper = AngleSmoothDamper.create(0, 0.1f, 0.5f);
			_ExtrudeScaleDamper = FloatSmoothDamper.create(0.0f, 0.1f, 0.01f);
			_actorList = new List<UMBActor>();
			_scrollRangeY = new DoubleVector2(0, double.MaxValue);

			_scale_root.localScale = Vector3.one * MapBoxDefine.scale_pivot;
			_addTileList = new List<MBTileCoordinate>();
			_oldZoomTileList = new List<UMBTile>();
			_tileUpdating = true;
			_transformRevision = 1;
			_cameraBound = UMBCameraBound.create(_targetCamera);

			//_outlineLayerList = new List<MBLayerRenderData>();

			//createOutlineBuilding();
			//_lastHillShadeOpacity = 0;
			_ctxHillShadeOpacity = new MBStyleExpressionContext();
			_stencilIDManager = UMBTileStencilIDManager.create();

			//_styleID = "ckvta2ylt1mq614s8vrp37ktq";
			_styleID = styleID;
			_uiBindingManager = UIBindingManager.create();
			makeBinding();
		}

		private void makeBinding()
		{
			UMBViewModel vm = _mapBox.getViewModel();

			_uiBindingManager.makeBinding(vm, nameof(vm.ProjectionMode), onUpdateProjectionMode);
			_uiBindingManager.makeBinding(vm, nameof(vm.ShowMapDeco), onUpdateShowMapDeco);
		}

		private void onUpdateProjectionMode(object obj)
		{
			applyProjectionMode();
		}

		private void onUpdateShowMapDeco(object obj)
		{
			applyProjectionMode();
		}

		private void applyProjectionMode()
		{
			if (ViewModel.ProjectionMode == UMBDefine.ProjectionMode.two_d)
			{
				rotateX(0.0f);
				setExtrudeScale(0.0f);
			}
			else if (ViewModel.ProjectionMode == UMBDefine.ProjectionMode.three_d)
			{
				rotateX(70.0f);
				if (ViewModel.ShowMapDeco)
				{
					setExtrudeScale(0.02f);// ㅋㅋㅋ
				}
				else
				{
					setExtrudeScale(1.0f);
				}
			}
		}

		public void initCurrentTilePos(MBTileCoordinateDouble pos)
		{
			_targetTilePos = pos;
			_currentTilePos = pos.toInteger();

			_zoomDamper.reset(pos.zoom);

			updateScaleFromZoom();
		}

		public void moveTo(MBLongLatCoordinate position, int zoom)
		{
			int tile_zoom = MapBoxDefine.clampControlZoom(zoom);

			MBTileCoordinateDouble tileCoordinate = MBTileCoordinateDouble.fromLonLat(position, tile_zoom);

			moveTo(tileCoordinate);
		}

		public void moveTo(MBTileCoordinateDouble tileCoordinate)
		{
			moveTo(tileCoordinate, tileCoordinate.zoom);
		}

		public void moveTo(MBTileCoordinateDouble tileCoordinate,float fZoom)
		{
			int controlZoom = MapBoxDefine.clampControlZoom(tileCoordinate.zoom);
			int tileZoom = MapBoxDefine.clampTileZoom(tileCoordinate.zoom);
			
			fZoom = MapBoxDefine.clampControlZoomFloat(fZoom);

			//
			if(!float.IsNaN(fZoom) && controlZoom != (int)fZoom)
			{
				throw new System.Exception($"invalid fZoom: controlZoom[{controlZoom}] fZoom[{fZoom}]");
			}

			if( tileCoordinate.zoom != tileZoom)
			{
				double tileScale = calcTileScale(tileZoom, tileCoordinate.zoom);
				tileCoordinate.tile_x *= tileScale;
				tileCoordinate.tile_y *= tileScale;
				tileCoordinate.zoom = tileZoom;
			}

			_targetTilePos = tileCoordinate;
			_currentTilePos = _targetTilePos.toInteger();

			//removeAllTiles();

			_cameraBound.updatePlanes();

			_addTileList.Clear();
			// 사이즈를 좀 키워보자
			for (int x = -_load_tile_offset; x <= _load_tile_offset; ++x)
			{
				for (int y = -_load_tile_offset; y <= _load_tile_offset; ++y)
				{
					MBTileCoordinate tilePos = MBTileCoordinate.fromTile(_currentTilePos);
					tilePos.tile_x += x;
					tilePos.tile_y += y;

					// TODO : 완전 축소하게 되면 Wrapping될 수 있도록 해보자
					

					if (tilePos.isValid() == false)
					{
						continue;
					}

					if (findTile(tilePos) != null)
					{
						continue;
					}
					//if (tile_offset.x != 0 || tile_offset.y != 0)
					//{
					//	continue;
					//}

					_addTileList.Add(tilePos);
					_tileUpdating = true;
				}
			}

			_zoomDamper.reset(fZoom);
			_mapBox.getViewModel().Zoom = fZoom;

			updateScaleFromZoom();

			clampTilePosY(ref _targetTilePos);
			_tileXYDamper.reset(_targetTilePos.tile_pos);

			_updatingTargetTilePos = _currentTilePos;
			finalizeLoadingTiles();
		}

		public void scrollTo(MBLongLatCoordinate worldPos)
		{
			MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromLonLat(worldPos, _targetTilePos.zoom);

			_targetTilePos.validateByWrap();

			clampTilePosY(ref _targetTilePos);

			_tileXYDamper.setTarget(tilePos.tile_pos);
			_scrollVelocity = Vector2.zero;
		}

		public void scroll(Vector2 deltaScrollRoot)
		{
			double tileScale = calcTileScale(_targetTilePos.zoom, _currentTilePos.zoom);

			Vector2Int offset = Vector2Int.one;
			offset.x = (int)(deltaScrollRoot.x / _scale_root.localScale.x);
			offset.y = (int)(deltaScrollRoot.y / _scale_root.localScale.y);

			_targetTilePos.tile_x -= (double)offset.x * tileScale / 4096;
			_targetTilePos.tile_y += (double)offset.y * tileScale / 4096;

			if( _targetTilePos.isValid() == false)
			{
				//Debug.Log($"wrap tile pos: [{Time.frameCount}][{_targetTilePos}]");
				_targetTilePos.validateByWrap();
			}

			clampTilePosY(ref _targetTilePos);

			_tileXYDamper.reset(_targetTilePos.tile_pos);

			updateScaleFromZoom();

			//_mapBox.scroll_root.anchoredPosition += deltaScrollRoot;
			//++_transformRevision;
			//updateActorPositions();
		}

		public void setScrollVelocity(Vector2 velocity)
		{
			_scrollVelocity = velocity;
		}

		private bool updateInertiaScroll()
		{
			if( _scrollVelocity == Vector2.zero)
			{
				return false;
			}

			float deltaTime = Time.unscaledDeltaTime;
			_scrollVelocity.x *= Mathf.Pow(0.05f, deltaTime);
			_scrollVelocity.y *= Mathf.Pow(0.05f, deltaTime);

			if( Mathf.Abs(_scrollVelocity.x) < 1)
			{
				_scrollVelocity.x = 0;
			}
			if( Mathf.Abs(_scrollVelocity.y) < 1)
			{
				_scrollVelocity.y = 0;
			}

			scroll(_scrollVelocity * deltaTime);
			return true;
		}

		public void rotateZ(float z_angle,bool update_now)
		{
			if( update_now)
			{
				_ZAngleDamper.reset(z_angle);
				_rotate_root.localRotation = Quaternion.Euler(_XAngleDamper.getCurrent(), 0, z_angle);
				_mapBox.getViewModel().ZAngle = z_angle;

				++_transformRevision;
				updateActorPositions();
			}
			else
			{
				_ZAngleDamper.setTarget(z_angle);
			}
		}

		public void setRotateZVelocity(float z_angle_velocity)
		{
			_ZAngleVelocity = z_angle_velocity;
		}

		public void rotateX(float z_angle)
		{
			_XAngleDamper.setTarget(z_angle);
		}

		public void setExtrudeScale(float scale)
		{
			_ExtrudeScaleDamper.setTarget(scale);
		}

		//public void rotate(float x_angle, float z_angle)
		//{
		//	_rotate_root.localRotation = Quaternion.Euler(x_angle, 0, z_angle);

		//	++_transformRevision;
		//	updateActorPositions();
		//}

		public void zoom(float target_zoom,bool updateNow)
		{
			target_zoom = Mathf.Clamp(target_zoom, MapBoxDefine.control_zoom_range.x, MapBoxDefine.control_zoom_range.y);

			if( updateNow)
			{
				_zoomDamper.reset(target_zoom);
				_mapBox.getViewModel().Zoom = target_zoom;

				updateScaleFromZoom();
				//applyHillShadeOpacity();
				_mapBox.getTripPhotoManager().onUpdateZoom();
			}
			else
			{
				_zoomDamper.setTarget(target_zoom);
			}
		}

		//private void removeAllTiles()
		//{
		//	foreach(UMBTile tile in _tileList)
		//	{
		//		tile.delete();
		//	}
		//	_tileList.Clear();
		//}

		private UMBTile createTile(MBTileCoordinate tile_pos)
		{
			UMBTile tile = GameObjectCache.getInstance().make<UMBTile>(_mapBox.tile_source, _tile_root, GameObjectCacheType.actor);
			tile.setup(_mapBox, tile_pos);

			//GameObject go_tile = new GameObject(string.Format("{0}/{1}/{2}", tile_pos.zoom, tile_pos.tile_x, tile_pos.tile_y));
			//go_tile.transform.SetParent(_tile_root, false);

			//UMBTile tile = go_tile.AddComponent<UMBTile>();
			//tile.setup(_mapBox, tile_pos);
			return tile;
		}

		public Vector2 getCenterScreenPos()
		{
			return RectTransformUtility.WorldToScreenPoint(_targetCamera, _mapBox.transform.position);
		}

		public void update()
		{
			updateOldZoomTiles();
			updateActors();
			updateInertiaScroll();
			updateTiles();

			bool updated = false;
			bool zoomUpdated = false;
			if ( _tileXYDamper.update())
			{
				updated = true;
				_targetTilePos.tile_pos = _tileXYDamper.getCurrent();
			}

			if( _zoomDamper.update())
			{
				updated = true;
				zoomUpdated = true;
				_mapBox.getViewModel().Zoom = _zoomDamper.getCurrent();
			}

			if( _ZAngleVelocity != 0)
			{
				float deltaTime = Time.unscaledDeltaTime;
				_ZAngleVelocity *= Mathf.Pow(_ZAngleDecelerationRate, deltaTime);
				if(Mathf.Abs(_ZAngleVelocity) < 1)
				{
					_ZAngleVelocity = 0;
				}

				float zAngle = _ZAngleDamper.getCurrent() + _ZAngleVelocity * deltaTime;
				_ZAngleDamper.reset(zAngle);
				_mapBox.getViewModel().ZAngle = zAngle;

				updated = true;
			}

			if (_XAngleDamper.update())
			{
				updated = true;
			}

			if( _ZAngleDamper.update())
			{
				updated = true;
				_mapBox.getViewModel().ZAngle = _ZAngleDamper.getCurrent();
			}

			if( _ExtrudeScaleDamper.update())
			{
				updated = true;
			}

			if (updated)
			{
				_rotate_root.localRotation = Quaternion.Euler(_XAngleDamper.getCurrent(), 0, _ZAngleDamper.getCurrent());
				//setupBackgroundColor();
				updateScaleFromZoom();
				//applyHillShadeOpacity();
			}

			if(zoomUpdated)
			{
				_mapBox.getTripPhotoManager().onUpdateZoom();
			}

#if UNITY_EDITOR
			updateDebugText();
#endif

			checkNRevealTile();
		}

		private void updateDebugText()
		{

			string debug_text = $"zoom[{_zoomDamper.getCurrent().ToString("F1")}/{_currentTilePos.zoom}] tx[{_targetTilePos.tile_x.ToString("F2")}] ty[{_targetTilePos.tile_y.ToString("F3")}]";
			debug_text += $"old:{_oldZoomTileList.Count}";
			debug_text += $"\nlabel:[{_mapBox.getLabelManager().getTotalPlacementCount()}][{_mapBox.getLabelManager().getRendererCount()}]";
			debug_text += $"\nloading:[{MBTileLoader.getLoadingCount()}]";

			//MBTileCoordinateDouble minTilePos = MBTileCoordinateDouble.fromLonLat(_mapBox.getTestBoundMin(), _currentTilePos.zoom);
			//MBTileCoordinateDouble maxTilePos = MBTileCoordinateDouble.fromLonLat(_mapBox.getTestBoundMax(), _currentTilePos.zoom);

			//Vector3 minWorldPos = tilePosToWorldPosition(minTilePos);
			//Vector3 maxWorldPos = tilePosToWorldPosition(maxTilePos);

			//Vector3 minSPos = _targetCamera.WorldToScreenPoint(minWorldPos);
			//Vector3 maxSPos = _targetCamera.WorldToScreenPoint(maxWorldPos);

			//Vector3 scExtent = maxSPos - minSPos;
			//scExtent.x = Mathf.Abs(scExtent.x);
			//scExtent.y = Mathf.Abs(scExtent.y);

			//float diff_zoom = calcZoomDeltaFromScale( scExtent.x, Screen.width);

			//debug_text += $"\nd[{maxSPos - minSPos}] x [{diff_zoom}]";

			//Vector3 centerScreenPosition = _targetCamera.WorldToScreenPoint(tilePosToWorldPosition(_targetTilePos));
			//Vector3 centerScreenPosition = _targetCamera.WorldToScreenPoint(_mapBox.transform.position);

			//Vector3 worldBottom = _targetCamera.ScreenToWorldPoint(new Vector3(0, _targetCamera.pixelHeight, centerScreenPosition.z));
			//Vector3 worldTop = _targetCamera.ScreenToWorldPoint(new Vector3(0, 0, centerScreenPosition.z));

			////double pivot_scale = calcPivotScale(_targetTilePos.zoom, _currentTilePos.zoom) * MapBoxDefine.scale_pivot;
			//float worldScale = _scale_root.parent.lossyScale.x * _scale_root.localScale.x;

			//float screenTileLength = (worldBottom - worldTop).magnitude;
			//screenTileLength /= worldScale;
			//screenTileLength /= 4096.0f;

			//int tileMax = MapBoxUtil.maxTile(_targetTilePos.zoom);

			//debug_text += $"\nth[{screenTileLength}] [{_scrollRangeY}]";

			_mapBox.text_debug.text = debug_text;

		}

		private void updateScaleFromZoom()
		{
			//			_targetTilePos.zoom = (int)_zoomDamper.getCurrent();
			float zoom_current = _zoomDamper.getCurrent();

			double tile_scale;
			double pivot_scale = calcPivotScale(zoom_current, _currentTilePos.zoom);

			_scale_root.localScale = Vector3.one * MapBoxDefine.scale_pivot * (float)pivot_scale;

			updateExtrudeScale((float)pivot_scale);

			if( _targetTilePos.zoom != (int)zoom_current)
			{
				int int_current = System.Math.Clamp((int)zoom_current, MapBoxDefine.tile_zoom_range.x, MapBoxDefine.tile_zoom_range.y);
				if( _targetTilePos.zoom != int_current)
				{
					tile_scale = calcTileScale(int_current, _targetTilePos.zoom);

					_targetTilePos.zoom = int_current;
					_targetTilePos.tile_x *= tile_scale;
					_targetTilePos.tile_y *= tile_scale;

					_tileXYDamper.rescale(tile_scale);
				}
			}
			
			tile_scale = calcTileScale(_currentTilePos.zoom, _targetTilePos.zoom);
			Vector2 offsetFromPivot = new Vector2();
			offsetFromPivot.x = (float)((_targetTilePos.tile_x * tile_scale - _currentTilePos.tile_x) * (double)MapBoxDefine.tile_extent);
			offsetFromPivot.y = -(float)((_targetTilePos.tile_y * tile_scale - _currentTilePos.tile_y) * (double)MapBoxDefine.tile_extent);
			_mapBox.scroll_root.anchoredPosition = -offsetFromPivot * _scale_root.localScale.x;

			++_transformRevision;
			updateActorPositions();
			updateScrollRangeY();
		}

		private void updateExtrudeScale(float pivot_scale)
		{
			float scale = Mathf.Pow(2, _currentTilePos.zoom - 16);
			float extrude_ratio = _ExtrudeScaleDamper.getCurrent() * scale;

			_mapBox.getViewModel().ExtrudeRatio = extrude_ratio;
			_mapBox.getUMBStyle().setExtrudeRatio(extrude_ratio);
		}

		public double calcTileScale(int new_zoom, int old_zoom)
		{
			if( new_zoom < old_zoom)
			{
				return 1.0 / (double)(1 << (old_zoom - new_zoom));
			}
			else if( new_zoom > old_zoom)
			{
				return (double)(1 << (new_zoom - old_zoom));
			}

			return 1.0;
		}

		public double calcPivotScale(float new_zoom,int old_zoom)
		{
			double tile_scale = calcTileScale((int)new_zoom, old_zoom);
			return tile_scale * (1.0 + new_zoom % 1.0);
		}

		public static float calcNewZoomFromScale(float current_zoom,float ratio)
		{
			float scale = MapBoxUtil.zoomToScale(current_zoom);
			scale *= ratio;

			return MapBoxUtil.scaleToZoom(scale);
		}
		
		//// 이거 쓰면 않됨 
		//public float calcZoomDeltaFromScale(float from,float to)
		//{
		//	float diff_zoom = 0;

		//	// 축소가 필요하다
		//	if (from > to)
		//	{
		//		float scale = from / to;

		//		int diff_int_zoom = Mathf.FloorToInt(Mathf.Log(scale, 2));
		//		float base_scale = Mathf.Pow(2, diff_int_zoom);

		//		diff_zoom = -(diff_int_zoom + (scale - base_scale) / base_scale);
		//	}
		//	// 확대가 필요하다
		//	else
		//	{
		//		float scale = to / from;

		//		int diff_int_zoom = Mathf.FloorToInt(Mathf.Log(scale, 2));
		//		float base_scale = Mathf.Pow(2, diff_int_zoom);

		//		diff_zoom = diff_int_zoom + (scale - base_scale) / base_scale;
		//	}

		//	return diff_zoom;
		//}

		//private void loadTile(MBTileCoordinate tilePos)
		//{
		//	if( _loadingList.Contains(tilePos))
		//	{
		//		Debug.LogWarning($"already tile loading :{tilePos}");
		//		return;
		//	}
		//	_loadingList.Add(tilePos);

		//	Debug.Log($"load tile : {tilePos.zoom}/{tilePos.tile_x}/{tilePos.tile_y}");

		//	MBTileCoordinate loadTilePos = tilePos;

		//	MultiThreadWorker worker = ClientMain.instance.getMultiThreadWorker();
		//	worker.execute<Module.Void>(promise => {

		//		promise.complete();

		//	}, result => {

		//		loadTileComplete(loadTilePos);

		//	});
		//}

		public UMBTile findTile(MBTileCoordinate tilePos)
		{
			foreach(UMBTile tile in _tileList)
			{
				if( tile.getTilePos().isEqual( tilePos))
				{
					return tile;
				}
			}

			return null;
		}

		private int _load_tile_offset = 3;

		private void checkNRevealTile()
		{
			if(_tileUpdating)
			{
				return;
			}

			if( _currentTilePos.isEqual( _targetTilePos.toInteger()) == false)
			{

				//Debug.Log(string.Format("need update tile: zoom[{0}->{1}] x[{2}->{3}] y[{4}->{5}]",
				//	_currentTilePos.zoom, _targetTilePos.zoom,
				//	(int)_currentTilePos.tile_x, (int)_targetTilePos.tile_x, (int)_currentTilePos.tile_y, (int)_targetTilePos.tile_y));

				MBTileCoordinate newCenterTilePosInteger = _targetTilePos.toInteger();

				//// zoom이 바뀌면 모든 tile을 버려야 한다
				bool zoom_changed = _currentTilePos.zoom != _targetTilePos.zoom;

				_addTileList.Clear();

				// 사이즈를 좀 키워보자
				for(int x = -_load_tile_offset; x <= _load_tile_offset; ++x)
				{
					for(int y = -_load_tile_offset; y <= _load_tile_offset; ++y)
					{
						MBTileCoordinate tilePos = MBTileCoordinate.fromTile(newCenterTilePosInteger);
						tilePos.tile_x += x;
						tilePos.tile_y += y;
					
						// y는 만들지 않음
						if( MapBoxUtil.isValidTile( tilePos.tile_y, tilePos.zoom) == false)
						{
							continue;
						}

						if (zoom_changed == false)
						{
							if (findTile(tilePos) == null)
							{
								_addTileList.Add(tilePos);
								_tileUpdating = true;
							}
						}
						else
						{
							_addTileList.Add(tilePos);
							_tileUpdating = true;
						}
					}
				}

				// 세팅
				//_currentTilePos = newCenterTilePosInteger;
				_updatingTargetTilePos = newCenterTilePosInteger;

				finalizeLoadingTiles();
				updateTiles();
			}
		}

		private void finalizeLoadingTiles()
		{
			_currentTilePos = _updatingTargetTilePos;
			_delTileList.Clear();
			foreach(UMBTile tile in _tileList)
			{
				MBTileCoordinate tilePos = tile.getTilePos();
				if (tilePos.zoom != _currentTilePos.zoom)
				{
					_delTileList.Add(tile);
					continue;
				}

				Vector2Int offset = tilePos.offsetFrom(_currentTilePos);
				if (offset.x < -_load_tile_offset || offset.x > _load_tile_offset || offset.y < -_load_tile_offset || offset.y > _load_tile_offset)
				{
					_delTileList.Add(tile);
					continue;
				}

				tile.setupPosition(_currentTilePos);

				//if (tile.getTileData() != null)
				//{
				//	_OutlineBuilding.addFeatures(tile.getTileData());
				//}
			}

			// wrapping 모드에 따라서 즉시 재활용이 가능 할 수도 있다
			foreach(MBTileCoordinate newTilePos in _addTileList)
			{
				MBTileCoordinate validTilePos = newTilePos.getValidByWrap();

				UMBTile umb_tile = null;
				foreach(UMBTile tile in _delTileList)
				{
					MBTileCoordinate delValidTilePos = tile.getTilePos().getValidByWrap();
					
					if( MBTileCoordinate.Equals(delValidTilePos, validTilePos))
					{
						//Debug.Log($"reuse tile: [{Time.frameCount}][{newTilePos}][{validTilePos}]");
						umb_tile = tile;
						break;
					}
				}

				if( umb_tile != null)
				{
					_delTileList.Remove(umb_tile);
					umb_tile.resetTilePos(newTilePos);
					umb_tile.setupPosition(_currentTilePos);
				}
				else
				{
					umb_tile = createTile(newTilePos);
					_tileList.Add(umb_tile);
					umb_tile.setupPosition(_currentTilePos);

					//Debug.Log($"create tile: {newTilePos}");
				}

			}

			//
			foreach (UMBTile tile in _delTileList)
			{
				_tileList.Remove(tile);

				if (tile.getTilePos().zoom != _currentTilePos.zoom)
				{
					// 좀 있다가 지우자
					tile.onBecameOldZoom();
					_oldZoomTileList.Add(tile);
				}
				else
				{
					tile.delete();
				}
			}

			_delTileList.Clear();

			foreach (UMBTile tile in _oldZoomTileList)
			{
				tile.setupPosition(_currentTilePos);
			}

			_addTileList.Clear();
			_tileUpdating = false;

			//setupBackgroundColor();
			updateScaleFromZoom();
			//setupOutlineBuilding();

			//Resources.UnloadUnusedAssets();
		}

		//private void setupBackgroundColor()
		//{
		//	_mapBox.getViewModel().BackgroundColor = _mapBox.getMBStyle(_styleID).getBackgroundColor(_zoomDamper.getCurrent());

		//	//_mapBox.getUMBStyle().getBackgroundMaterial().SetColor(backgroundColor);

		//	//MaterialPropertyBlock mb = new MaterialPropertyBlock();
		//	//_mapBox.backgroundRenderer.GetPropertyBlock(mb);
		//	//mb.SetColor("_Color", backgroundColor);
		//	//_mapBox.backgroundRenderer.SetPropertyBlock(mb);
		//}

		public void registerActor(UMBActor actor)
		{
			_actorList.Add(actor);
		}

		public void removeActor(UMBActor actor)
		{
			_actorList.Remove(actor);
		}

		public void removeAllActors()
		{
			_actorList.Clear();
		}

		public void updateActorPositions()
		{
			foreach(UMBActor actor in _actorList)
			{
				actor.updateTransformPosition();
			}
		}

		public void updateActors()
		{
			foreach(UMBActor actor in _actorList)
			{
				actor.update();
			}
		}

		public void updateTiles()
		{
			foreach(UMBTile tile in _tileList)
			{
				tile.update();
			}
		}

		public void updateOldZoomTiles()
		{
			_delTileList.Clear();
			foreach(UMBTile tile in _oldZoomTileList)
			{
				// 않보이는건 즉시 삭제
				if( tile.isVisible() == false)
				{
					_delTileList.Add(tile);
				}
				else if(checkOldZoomTileCovered(tile))
				{
					_delTileList.Add(tile);
				}
				else if(checkOldZoomTooDifferent(tile))	// zoom 차이가 많이 나는거는 covering으로 해결이 않된다 지워주자
				{
					_delTileList.Add(tile);
				}
			}

			if( _delTileList.Count > 0)
			{
				foreach(UMBTile tile in _delTileList)
				{
					_oldZoomTileList.Remove(tile);
					tile.delete();
				}

				_delTileList.Clear();
			}
		}

		// 개수가 많아지면 느릴 수 있다.
		// 화면 밖에 있는건 skip할 수 있도록 하자
		public UMBActor pickActorFromScreenPosition(Vector2 pos)
		{
			foreach(UMBActor actor in _actorList)
			{
				if( actor.CanPick == false)
				{
					continue;
				}

				if( RectTransformUtility.RectangleContainsScreenPoint(actor.rt, pos, _targetCamera))
				{
					Debug.Log("pick actor", actor.gameObject);

					return actor;
				}
			}

			return null;
		}

		public bool checkOldZoomTileCovered(UMBTile oldTile)
		{
			MBTileCoordinate tilePos = oldTile.getTilePos();
			
			double scale = calcTileScale(_currentTilePos.zoom, tilePos.zoom);

			bool covered = true;

			if ( scale > 1.0f)
			{
				// 커버가 되었다고 판단하는 기준

				// 해당 위치 타일이 목록에 없다
				// 해당 위치 타일이 Invisible이다

				for(int i = 0; i < (int)scale; ++i)
				{
					for(int j = 0; j < (int)scale; ++j)
					{
						int tile_x = (int)(tilePos.tile_x * scale) + i;
						int tile_y = (int)(tilePos.tile_y * scale) + j;

						UMBTile currentTile = findTile(new MBTileCoordinate(_currentTilePos.zoom, tile_x, tile_y));
						if( currentTile != null)
						{
							if( currentTile.isVisible() && currentTile.getTileData() == null)
							{
								covered = false;
								break;
							}
						}
					}
				}
				
			}
			// zoom이 축소되었다 (old가 더 작다)
			else
			{
				//int tile_x = Mathf.CeilToInt((float)(tilePos.tile_x * scale));
				//int tile_y = Mathf.CeilToInt((float)(tilePos.tile_y * scale));

				int tile_x = (int)(tilePos.tile_x * scale);
				int tile_y = (int)(tilePos.tile_y * scale);

				UMBTile currentTile = findTile(new MBTileCoordinate(_currentTilePos.zoom, tile_x, tile_y));
				if( currentTile != null && currentTile.getTileData() == null)
				{
					covered = false;
				}
			}

			return covered;
		}

		public bool checkOldZoomTooDifferent(UMBTile oldTile)
		{
			MBTileCoordinate tilePos = oldTile.getTilePos();
			int diff_zoom = _currentTilePos.zoom - tilePos.zoom;
			return diff_zoom < -1 || diff_zoom > 1;
		}

		public Vector3 tilePosToWorldPosition(MBTileCoordinateDouble tilePos)
		{
			if (_currentTilePos.zoom != tilePos.zoom)
			{
				double scale = calcTileScale(_currentTilePos.zoom, tilePos.zoom);
				tilePos.tile_x *= scale;
				tilePos.tile_y *= scale;
			}

			double offset_tile_x = tilePos.tile_x - _currentTilePos.tile_x;
			double offset_tile_y = tilePos.tile_y - _currentTilePos.tile_y;

			Vector3 pos = Vector3.one;
			pos.x = (float)(offset_tile_x * 4096);
			pos.y = -(float)(offset_tile_y * 4096);
			pos.z = 0;

			return _tile_root.TransformPoint(pos);
		}

		public MBTileCoordinateDouble screenToTilePos(Vector2 screen,int target_zoom)
		{
			Vector3 mapBoxScreenPoint = _targetCamera.WorldToScreenPoint(_mapBox.transform.position);
			Vector3 worldPos = _targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, mapBoxScreenPoint.z));
			return worldToTilePos(worldPos, target_zoom);
		}

		public MBTileCoordinateDouble worldToTilePos(Vector3 world,int target_zoom)
		{
			Vector3 local = _tile_root.InverseTransformPoint(world);

			double offset_tile_x = local.x / 4096;
			double offset_tile_y = -local.y / 4096;

			MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromInteger(_currentTilePos);
			tilePos.tile_x += offset_tile_x;
			tilePos.tile_y += offset_tile_y;

			if( tilePos.zoom != target_zoom)
			{
				double scale = calcTileScale(target_zoom, tilePos.zoom);
				tilePos.zoom = target_zoom;
				tilePos.tile_x *= scale;
				tilePos.tile_y *= scale;
			}

			return tilePos;
		}

		//private void applyHillShadeOpacity()
		//{
		//	if (_exHillShadeOpacity == null && _mapBox.getMBStyle(_styleID) != null)
		//	{
		//		_exHillShadeOpacity = _mapBox.getMBStyle(_styleID).getHillShadeOpacity();
		//	}

		//	if(_exHillShadeOpacity != null)
		//	{
		//		_ctxHillShadeOpacity._zoom = _zoomDamper.getCurrent();
		//		_mapBox.getViewModel().HillshadeOpacity = (float)_exHillShadeOpacity.evaluateDouble(_ctxHillShadeOpacity);
		//	}
		//}

		public bool checkAllVisibleTileLoaded()
		{
			int visibleNotLoaded = 0;

			foreach(UMBTile tile in _tileList)
			{
				if( tile.isVisible() && tile.getTileData() == null)
				{
					visibleNotLoaded++;
				}
			}

			return visibleNotLoaded == 0;
		}

		public void changeStyle(string style_id)
		{
			_styleID = style_id;
			foreach(UMBTile tile in _tileList)
			{
				tile.onChangeStyle();
			}

			//setupBackgroundColor();
		}

		private void updateScrollRangeY()
		{
			Vector3 centerScreenPosition = _targetCamera.WorldToScreenPoint(_mapBox.transform.position);

			Vector3 worldBottom = _targetCamera.ScreenToWorldPoint(new Vector3(0, _targetCamera.pixelHeight, centerScreenPosition.z));
			Vector3 worldTop = _targetCamera.ScreenToWorldPoint(new Vector3(0, 0, centerScreenPosition.z));

			float pivot_scale = _scale_root.localScale.x; // int zoom diff * (1.0f + zoom_diff % 1)
			//_scale_root.localScale = Vector3.one * MapBoxDefine.scale_pivot * (float)pivot_scale;
			
			float worldScale = _scale_root.parent.lossyScale.x * pivot_scale;

			float worldTileLength = (worldBottom - worldTop).magnitude;
			float screenTileLength = worldTileLength;
			screenTileLength /= worldScale;
			screenTileLength /= 4096.0f;

			int tileMax = MapBoxUtil.maxTile(_currentTilePos.zoom);

			_scrollRangeY.x = screenTileLength / 2.0f;
			_scrollRangeY.y = tileMax - screenTileLength / 2.0f;
			_scrollRangeZoom = _currentTilePos.zoom;

			if (_scrollRangeY.y < _scrollRangeY.x)
			{
				Debug.LogError($"_scrollRange[{_scrollRangeY}");
				_scrollRangeY.x = 0;
				_scrollRangeY.y = double.MaxValue;
			}

			//Debug.Log($"[{Time.frameCount}] scroll range y [{_scrollRangeY}] zoom[{_zoomDamper.getCurrent()},{_targetTilePos.zoom},{_currentTilePos.zoom}] scale[{_scale_root.localScale.x},{pivot_scale}] length[{worldTileLength},{screenTileLength}]");
		}

		private void clampTilePosY(ref MBTileCoordinateDouble tilePos)
		{
			double scale = calcTileScale(_currentTilePos.zoom, tilePos.zoom);

			double value = tilePos.tile_pos.y * scale;
			//bool forged = false;

			if(value < _scrollRangeY.x)
			{
				value = _scrollRangeY.x;
				//forged = true;
			}

			if(value > _scrollRangeY.y)
			{
				value = _scrollRangeY.y;
				//forged = true;
			}

			//if( forged)
			//{
			//	Debug.Log($"[{Time.frameCount}] tile_y clamed: [{tilePos.tile_pos.y}->{value}][{tilePos.zoom}] target_zoom[{ _zoomDamper.getCurrent()},{_targetTilePos.zoom}] current_zoom[{_currentTilePos.zoom}]");
			//}

			tilePos.tile_pos.y = value / scale;
		}

		public Vector2 calcScreenExtent(MBLongLatCoordinate min,MBLongLatCoordinate max)
		{
			MBTileCoordinateDouble tileMin = MBTileCoordinateDouble.fromLonLat(min, _currentTilePos.zoom);
			MBTileCoordinateDouble tileMax = MBTileCoordinateDouble.fromLonLat(max, _currentTilePos.zoom);

			Vector3 minWorldPos = tilePosToWorldPosition(tileMin);
			Vector3 maxWorldPos = tilePosToWorldPosition(tileMax);

			Vector3 minSPos = _targetCamera.WorldToScreenPoint(minWorldPos);
			Vector3 maxSPos = _targetCamera.WorldToScreenPoint(maxWorldPos);

			Vector2 scExtent = maxSPos - minSPos;
			scExtent.x = Mathf.Abs(scExtent.x);
			scExtent.y = Mathf.Abs(scExtent.y);
			return scExtent;
		}

		public float calcFitZoom(Vector2 extent, Vector2 mapboxExtent, Vector2 zoomRange, float screenCoverage)
		{
			return calcFitZoom(_zoomDamper.getCurrent(), extent, mapboxExtent, zoomRange, screenCoverage);

		}
		public static float calcFitZoom(float currentZoom,Vector2 extent, Vector2 mapboxExtent,Vector2 zoomRange, float screenCoverage)
		{
			float x_ratio = mapboxExtent.x * screenCoverage / extent.x;
			float y_ratio = mapboxExtent.y * screenCoverage / extent.y;

			// x축으로 맞추었을때
			float ratio;
			if( x_ratio < y_ratio)
			{
				ratio = x_ratio;
			}
			else
			{
				ratio = y_ratio;
			}

			float targetZoom = calcNewZoomFromScale(currentZoom, ratio);
			targetZoom = Mathf.Clamp(targetZoom, zoomRange.x, zoomRange.y);

			//Debug.Log($"fit zoom : extent[{extent}] mapbox[{mapboxExtent}] cur[{_zoomDamper.getCurrent()}] target[{targetZoom}] x_ratio[{x_ratio}] y_ratio[{y_ratio}]");

			return targetZoom;
		}

		public void removeAllTiles()
		{
			// 위험하지 않을까?
			foreach(UMBTile tile in _tileList)
			{
				tile.delete();
			}
			
			foreach(UMBTile tile in _oldZoomTileList)
			{
				tile.delete();
			}

			_tileList.Clear();
			_oldZoomTileList.Clear();
		}
	}
}
