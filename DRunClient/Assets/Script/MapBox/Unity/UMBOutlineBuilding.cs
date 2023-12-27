//using Festa.Client.Module;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering;

//namespace Festa.Client.MapBox
//{
//	[RequireComponent(typeof(MeshFilter))]
//	[RequireComponent(typeof(MeshRenderer))]
//	public class UMBOutlineBuilding : ReusableMonoBehaviour
//	{
//		private Mesh _mesh;
//		private MBMesh _mbMesh;
//		private MeshFilter		_mf;
//		private MeshRenderer	_mr;

//		private UMBControl _control;
//		private RectTransform _rt;
//		private MBTileCoordinate _centerTilePos;
//		//private bool _is_active = false;
//		private MultiThreadWorker _threadWorker;
//		private List<MBLayerRenderData> _layerList;
//		private List<UMBFeatureGroup> _groupList;

//		private MBTileCoordinate _lastTilePos;

//		public MBTileCoordinate getTilePos()
//		{
//			return _centerTilePos;
//		}

//		public override void onCreated(ReusableMonoBehaviour source)
//		{
//			_rt = transform as RectTransform;

//			_mbMesh = MBMesh.create();

//			_mesh = new Mesh();
//			_mesh.name = "outline_buildings";
//			_mesh.MarkDynamic();
//			_mesh.Clear();

//			_mf = GetComponent<MeshFilter>();
//			_mr = GetComponent<MeshRenderer>();
			
//			_mr.receiveShadows = false;
//			_mr.shadowCastingMode = ShadowCastingMode.Off;

//			_mf.sharedMesh = _mesh;

//			//_is_active = true;

//			_layerList = new List<MBLayerRenderData>();
//			_groupList = null;
			
//		}

//		public override void onReused()
//		{
//			//_is_active = true;
//			_mf.sharedMesh = null;
//		}

//		public void delete()
//		{
//			//_is_active = false;
//			GameObjectCache.getInstance().delete(this);
//		}

//		public void init(UMBControl control)
//		{
//			_control = control;
//			_threadWorker = ClientMain.instance.getMultiThreadWorker();
//		}

//		public void setup(MBTileCoordinate center,List<MBLayerRenderData> layer_list)
//		{
//			_centerTilePos = center;

//			_layerList.Clear();
//			_layerList.AddRange(layer_list);

//			if( layer_list.Count > 0)
//			{
//				MBStyleLayer style_layer = layer_list[0].getLayerStyle();

//				_mr.sharedMaterial = _control.getMapBox().getUMBStyle().getMaterial(style_layer);
//				gameObject.layer = _control.getMapBox().getUMBStyle().getLayer(style_layer);

//				buildMesh();
//			}
//			else
//			{
//				_mr.enabled = false;
//			}
//		}

//		private void buildMesh()
//		{
//			//_mr.enabled = false;

//			//if (_lastTilePos.zoom != 0 && _lastTilePos.zoom != _centerTilePos.zoom)
//			//{
//				float scale = (float)_control.calcTileScale(_centerTilePos.zoom, _lastTilePos.zoom);

//				float offset_x = (_lastTilePos.tile_x * scale - _centerTilePos.tile_x);
//				float offset_y = (_lastTilePos.tile_y * scale - _centerTilePos.tile_y);

//				//Debug.Log($"{_centerTilePos} <- {_lastTilePos} : offset[{offset_x},{offset_y}]");

//				_rt.localScale = Vector3.one * scale;
//				_rt.anchoredPosition = new Vector2(offset_x, -offset_y) * MapBoxDefine.tile_extent;
////				return;
//			//}

//			_threadWorker.execute<Void>(promise => {

//				List<UMBFeatureGroup> group_list = new List<UMBFeatureGroup>();
//				MBStyleExpressionContext ctx = new MBStyleExpressionContext();
//				ctx._zoom = _centerTilePos.zoom;

//				MBPolygonMeshBuilder meshBuilder = MBMeshBuilder.create<MBPolygonMeshBuilder>();
//				_mbMesh.reset();

//				foreach (MBLayerRenderData layer in _layerList)
//				{
//					if( layer.getLayerStyle().getType() != MBStyleDefine.LayerType.fill_extrusion)
//					{
//						continue;
//					}

//					foreach(MBFeature feature in layer.getOutlinePolygonList() )
//					{
//						if( feature.type != MBFeatureType.polygon)
//						{
//							continue;
//						}

//						ctx._feature = feature;

//						MBStyleExpression ex_color = layer.getLayerStyle().getFillExtrusionColor();
//						MBStyleExpression ex_opacity = layer.getLayerStyle().getOpacity();

//						Color color = (Color)ex_color.evaluate(ctx);
//						//Color color = Color.red;
//						color.a = (float)ex_opacity.evaluateDouble(ctx);

//						bool forged = false;

//						foreach (UMBFeatureGroup group in group_list)
//						{
//							if( group.isOverlap( feature, color))
//							{
//								group.addFeature(feature);
//								forged = true;
//								break;
//							}
//						}

//						if( forged == false)
//						{
//							group_list.Add(UMBFeatureGroup.create(feature, _centerTilePos, color));

//							MBTileCoordinate offsetTilePos = feature._tile._tilePos;
//							int offset_x = (offsetTilePos.tile_x - _centerTilePos.tile_x) * 4096;
//							int offset_y = (offsetTilePos.tile_y - _centerTilePos.tile_y) * 4096;

//							meshBuilder.setExtrusion(true);
//							meshBuilder.build(feature, color, offset_x, offset_y);
//							meshBuilder.setExtrusion(false);

//							_mbMesh.append(meshBuilder);
//						}
//					}
//				}

//				_groupList = group_list;
//				//System.Threading.Thread.Sleep(1000);

//				promise.complete();

//			}, result => { 
			
//				if( result.succeeded())
//				{
//					applyMesh();
//				}
			
//			});
//		}

//		void OnDrawGizmos()
//		{
//			if( _groupList == null)
//			{
//				return;
//			}

//			Gizmos.color = Color.black;

//			foreach(UMBFeatureGroup group in _groupList)
//			{
//				MBBound bound = group.getBound();
//				Vector3 min = new Vector3(bound.min.x, -bound.min.y, 0);
//				Vector3 max = new Vector3(bound.max.x, -bound.max.y, 0);

//				min = transform.TransformPoint(min);
//				max = transform.TransformPoint(max);

//				Gizmos.DrawWireCube((min + max) / 2, (max - min));

//			}
//		}

//		void applyMesh()
//		{
//			_mesh.Clear();

//			_mesh.SetVertices(_mbMesh.getVertexList());
//			_mesh.SetNormals(_mbMesh.getNormalList());
//			_mesh.SetColors(_mbMesh.getColorList());
//			_mesh.SetTriangles(_mbMesh.getIndexList(), 0);

//			_mesh.subMeshCount = 1;

//			_mr.enabled = _mbMesh.isEmpty() == false;

//			_lastTilePos = _centerTilePos;
//			_rt.localScale = Vector3.one;
//			_rt.localPosition = Vector3.zero;
//		}

//		//private void initRenderer(MBStyleLayer style_layer,Material layer_mat)
//		//{

//		//	_mf.sharedMesh = null;

//		//	//_mr.sortingLayerID = style_layer.getSortingLayerID();
//		//	//_mr.sortingOrder = style_layer.sortingOrder;
//		//	_mr.sharedMaterial = layer_mat;

//		//}

//		//public void updateMesh()
//		//{
//		//	ClientMain.instance.getMultiThreadWorker().execute<Void>(promise => {

//		//		try
//		//		{
//		//			MBPolygonMeshBuilder meshBuilder = MBMeshBuilder.create<MBPolygonMeshBuilder>();
//		//			_mbMesh.reset();

//		//			foreach (KeyValuePair<MBTileCoordinate, List<UMBFeatureGroup>> item in _map)
//		//			{
//		//				foreach (UMBFeatureGroup group in item.Value)
//		//				{
//		//					group.buildMesh(meshBuilder, _centerTilePos);

//		//					_mbMesh.append(group.getMesh());
//		//				}
//		//			}

//		//			promise.complete();
//		//		}
//		//		catch(System.Exception e)
//		//		{
//		//			promise.fail(e);
//		//		}


//		//	}, result => {

//		//		if( _oldChain != null)
//		//		{
//		//			_oldChain.delete();
//		//			_oldChain = null;
//		//		}

//		//		if ( result.failed())
//		//		{
//		//			Debug.LogException(result.cause());
//		//			return;
//		//		}

//		//		if( _is_active == false)
//		//		{
//		//			return;
//		//		}

//		//		_mesh.Clear();
//		//		//_mesh.vertices = _mbMesh.getVertexList().ToArray();
//		//		//_mesh.normals = _mbMesh.getNormalList().ToArray();
//		//		//_mesh.triangles = _mbMesh.getIndexList().ToArray();
//		//		//_mesh.subMeshCount = 1;

//		//		//_mf.sharedMesh = _mesh;

//		//	});
//		//}
//	}
//}
