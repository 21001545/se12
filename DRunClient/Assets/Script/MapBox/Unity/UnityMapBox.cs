using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Festa.Client.MapBox
{
	public class UnityMapBox : MonoBehaviour
	{
		public RectTransform pivot;
		public RectTransform rotate_root;
		public RectTransform scroll_root;
		public RectTransform tile_root;
		public RectTransform building_root;
		public RectTransform label_root;
		public RectTransform global_actor_root;

		//public UMBOutlineBuilding outline_building_source;
		//public UMBLabel label_source;
		public UMBSymbolRenderer symbol_source;
		//public UMBSymbolTextCircleCollider symbol_circle_collider_source;
		public UMBLayerMeshRender layer_mesh_source;
		public PolylineRenderer polyline_renderer_source;
		public TextMeshPro text_measure;
		public UMBTile tile_source;

		public UMBStyleData styleData;
		//public MeshRenderer backgroundRenderer;
		public TMPro.TMP_Text text_debug;

		//private int _maskPropertyKey = -1;
		private UMBViewModel _viewModel;
		private UMBStyle _style;
		private AbstractInputModule _inputModule;
		private UMBInputFSM _inputFSM;
		private UMBControl _control;
		private UMBLabelManager _labelManager;
		private UMBTripPhotoManager _tripPhotoManager;
		private MBStyleCache _styleCache;

		private Camera _targetCamera;
		private RectTransform _rt;
		private bool _disbleInput;
		private bool _isOfflineMode;
		private bool _isInit = false;

		private MBLongLatCoordinate _currentLocation;

		//private MBLongLatCoordinate _testBoundMin = new MBLongLatCoordinate(127.051956, 37.505823);
		//private MBLongLatCoordinate _testBoundMax = new MBLongLatCoordinate(127.055630, 37.510047);

		private MBLongLatCoordinate _testBoundMin = new MBLongLatCoordinate(127.041557312012, 37.384578704834);
		private MBLongLatCoordinate _testBoundMax = new MBLongLatCoordinate(127.073120117188, 37.4438896179199);

		public MBLongLatCoordinate getTestBoundMin()
		{
			return _testBoundMin;
		}

		public MBLongLatCoordinate getTestBoundMax()
		{
			return _testBoundMax;
		}

		public UMBViewModel getViewModel()
		{
			return _viewModel;
		}

		public AbstractInputModule getInputModule()
		{
			return _inputModule;
		}

		public UMBStyle getUMBStyle()
		{
			return _style;
		}

		public RectTransform getControlArea()
		{
			return transform as RectTransform;
		}

		public Camera getTargetCamera()
		{
			return _targetCamera;
		}

		public UMBControl getControl()
		{
			return _control;
		}

		public UMBLabelManager getLabelManager()
		{
			return _labelManager;
		}

		public UMBTripPhotoManager getTripPhotoManager()
		{
			return _tripPhotoManager;
		}

		public MBLongLatCoordinate getCurrentLocation()
		{
			return _currentLocation;
		}

		public UMBInputFSM getInputFSM()
		{
			return _inputFSM;
		}

		public bool isDisableInput()
		{
			return _disbleInput;
		}

		public bool isOfflineMode()
		{
			return _isOfflineMode;
		}

		public void init(bool isOfflineMode, Camera targetCamera,MBStyleCache styleCache,string styleID)
		{
			_isInit = true;
			_rt = transform as RectTransform;
			_targetCamera = targetCamera;
			pivot.localScale = Vector3.one * MapBoxDefine.scale_pivot;

			_isOfflineMode = isOfflineMode;
			_disbleInput = isOfflineMode;
			_styleCache = styleCache;

			initItemSources();
			createViewModel();
			createInputModule();
			createStyle();
			createControl(styleID);
			createFSM();
			createLabelManager();
			createTripPhotoManager();
		}

		private void initItemSources()
		{
			//outline_building_source.gameObject.SetActive(false);
			//label_source.gameObject.SetActive(false);
			symbol_source.gameObject.SetActive(false);
			layer_mesh_source.gameObject.SetActive(false);
			//symbol_circle_collider_source.gameObject.SetActive(false);
			tile_source.gameObject.SetActive(false);
			polyline_renderer_source.gameObject.SetActive(false);
		}

		public void resetRectTransform()
		{
			_rt.anchorMin = Vector2.zero;
			_rt.anchorMax = Vector2.one;
			_rt.anchoredPosition = Vector2.zero;
		}

		private void createViewModel()
		{
			_viewModel = UMBViewModel.create();
		}

		private void createInputModule()
		{
			if (_disbleInput)
			{
				return;
			}

#if UNITY_EDITOR
			_inputModule = InputModule_PC.create();
#else
			_inputModule = InputModule_Mobile.create();
#endif
		}

		private void createFSM()
		{
			if (_disbleInput)
			{
				return;
			}

			_inputFSM = UMBInputFSM.create(this);
		}

		private void createLabelManager()
		{
			_labelManager = UMBLabelManager.create(this);
		}

		private void createTripPhotoManager()
		{
			_tripPhotoManager = UMBTripPhotoManager.create(this);
		}

		private void createStyle()
		{
			_style = UMBStyle.create(styleData);
			//backgroundRenderer.sharedMaterial = _style.getBackgroundMaterial();
		}

		private void createControl(string styleID)
		{
			_control = UMBControl.create(this,styleID);
		}

		public void update()
		{
			if( _isInit == false)
			{
				return;
			}

			updateScreenMask();
			if (_disbleInput == false)
			{
				_inputFSM.run();
			}

			_control.update();
			_labelManager.update();
		}
		
		public void updateFixed()
		{
			//_labelManager.updateFixed();
		}

		public void updateCurrentLocation(MBLongLatCoordinate worldPos)
		{
			_currentLocation = worldPos;
		}

		public void startMap()
		{
			int zoom = 16;
			_control.moveTo(_currentLocation, zoom);
		}

		public UMBActor spawnActor(string name, MBLongLatCoordinate position)
		{
			UMBActor source = styleData.actorSourceContainer.getSource(name).actor_source;

			return spawnActor(source, position);
		}

		public UMBActor spawnActor(UMBActor source, MBLongLatCoordinate position)
		{
			UMBActor new_one = source.make<UMBActor>(global_actor_root, GameObjectCacheType.actor);
			new_one.init(this, position);

			_control.registerActor(new_one);
			return new_one;
		}

		public void removeActor(UMBActor actor)
		{
			GameObjectCache.getInstance().delete(actor);
			_control.removeActor(actor);
		}

		public void removeActors<T>(List<T> actorList) where T : UMBActor
		{
			foreach(T actor in actorList)
			{
				removeActor(actor);
			}
		}

		public void removeAllActors()
		{
			_tripPhotoManager.clear(true);

			GameObjectCache.getInstance().delete(_control.getActorList());
			_control.removeAllActors();
		}

		public void removeAllTiles()
		{
			_control.removeAllTiles();
		}

		public void setCurrentLocationMode(int mode)
		{
			_viewModel.CurrentLocationMode = mode;
		}

		public void setProjectionMode(int mode)
		{
			_viewModel.ProjectionMode = mode;
			//if (mode == UMBDefine.ProjectionMode.two_d)
			//{
			//	_control.setExtrudeScale(0.0f);
			//	_control.rotateX(0.0f);
			//}
			//else if (mode == UMBDefine.ProjectionMode.three_d)
			//{
			//	_control.setExtrudeScale(1.0f);
			//	_control.rotateX(70.0f);
			//}
		}

		public void setStyle(string style_id)
		{
			_control.changeStyle(style_id);
		}

		void updateScreenMask()
		{
			if (transform.hasChanged == false)
			{
				return;
			}

			transform.hasChanged = false;

			// 일단 mask 없애보자
			Vector4 v = new Vector4(0, 0, 1, 1);
			_style.setScreenMask(v);

			//if (_disbleInput)
			//{
			//	Vector4 v = new Vector4(0, 0, 1, 1);
			//	_style.setScreenMask(v);
			//}
			//else
			//{
			//	Vector3[] corners = new Vector3[4];

			//	RectTransform rt = transform as RectTransform;
			//	rt.GetWorldCorners(corners);

			//	for (int i = 0; i < 4; ++i)
			//	{
			//		corners[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, corners[i]);

			//		//Debug.Log(corners[i]);
			//	}

			//	Vector4 v;
			//	v.x = corners[0].x / Screen.width;
			//	v.y = corners[0].y / Screen.height;
			//	v.z = corners[2].x / Screen.width;
			//	v.w = corners[2].y / Screen.height;

			//	_style.setScreenMask(v);
			//}
		}

		void OnDrawGizmos()
		{
			if( _labelManager != null)
				_labelManager.onDrawGizmos();

			//drawTextBound();			
		}

		void drawTextBound()
		{
			if( _control == null)
			{
				return;
			}
			int zoom = _control.getCurrentTilePos().zoom;
			MBTileCoordinateDouble minTilePos = MBTileCoordinateDouble.fromLonLat(_testBoundMin, zoom);
			MBTileCoordinateDouble maxTilePos = MBTileCoordinateDouble.fromLonLat(_testBoundMax, zoom);

			Vector3 minWorldPos = _control.tilePosToWorldPosition(minTilePos);
			Vector3 maxWorldPos = _control.tilePosToWorldPosition(maxTilePos);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(new Vector3(minWorldPos.x, minWorldPos.y, minWorldPos.z), new Vector3(maxWorldPos.x, minWorldPos.y, minWorldPos.z));
			Gizmos.DrawLine(new Vector3(maxWorldPos.x, minWorldPos.y, minWorldPos.z), new Vector3(maxWorldPos.x, maxWorldPos.y, minWorldPos.z));
			Gizmos.DrawLine(new Vector3(maxWorldPos.x, maxWorldPos.y, minWorldPos.z), new Vector3(minWorldPos.x, maxWorldPos.y, minWorldPos.z));
			Gizmos.DrawLine(new Vector3(minWorldPos.x, maxWorldPos.y, minWorldPos.z), new Vector3(minWorldPos.x, minWorldPos.y, minWorldPos.z));
		}

		public MBStyle getMBStyle(string style_id)
		{
			return _styleCache.get(style_id);
		}

		public Vector2 calcScreenExtent()
		{
			RectTransform rt = transform as RectTransform;

			Vector3[] worldCorner = new Vector3[4];
			rt.GetWorldCorners(worldCorner);

			Vector2 begin = _targetCamera.WorldToScreenPoint(worldCorner[0]);
			Vector2 end = _targetCamera.WorldToScreenPoint(worldCorner[2]);

			Vector2 extent = end - begin;
			extent.x = Mathf.Abs(extent.x);
			extent.y = Mathf.Abs(extent.y);
			return extent;
		}
	}
}