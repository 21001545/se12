using System.Collections.Generic;
using Festa.Client.Module;
using UnityEngine.Events;
using UnityEngine;
using Unity.Profiling;

namespace Festa.Client.MapBox
{
	public class MBTileMeshBuilder
	{
		private MBTile _tile;
		private MBStyle _style;
		private MBPolygonMeshBuilder _polygonBuilder;
		private MBLineStringMeshBuilder _lineBuilder;
		private MBLineMeshBuilder _lineMeshBuilder;
		//private Dictionary<MBLayer, MBMesh> _layerMesh;

		public static MBTileMeshBuilder create(MBTile tile,MBStyle style)
		{
			MBTileMeshBuilder builder = new MBTileMeshBuilder();
			builder.init(tile,style);
			return builder;
		}

		private void init(MBTile tile,MBStyle style)
		{
			_tile = tile;
			_style = style;
			_polygonBuilder = MBMeshBuilder.create<MBPolygonMeshBuilder>();
			_lineBuilder = MBMeshBuilder.create<MBLineStringMeshBuilder>();
			_lineMeshBuilder = MBMeshBuilder.create<MBLineMeshBuilder>();
			//	_layerMesh = new Dictionary<MBLayer, MBMesh>();
		}


		static ProfilerMarker profileMarker = new ProfilerMarker("MapBoxTileMeshBuilder.run");

		public void run(UnityAction<bool> callback)
		{
			ClientMain.instance.getMultiThreadWorker().execute<List<MBLayerRenderData>>(promise => {

				profileMarker.Begin();
				try
				{
					MBStyleExpressionContext ctx = new MBStyleExpressionContext();
					MBStyleExpressionContext ctx_hillshade = new MBStyleExpressionContext();

					List<MBLayerRenderData> layerRenderDataList = buildStyledRenderData(ctx);

					ctx._zoom = _tile._tilePos.zoom;
					ctx_hillshade._zoom = _tile._tilePos.zoom;

					foreach (MBLayerRenderData layer in layerRenderDataList)
					{
						buildLayerMesh(layer, ctx, ctx_hillshade);
					}

					//ms_time = System.Environment.TickCount - total_build_start_time;
					//if( ms_time > 0)
					//{
					//	Debug.Log($"[{System.Environment.TickCount}] buildMesh[{_tile._tilePos}] mesh total - {ms_time}ms");
					//}

					//complete_time = System.Environment.TickCount;
					promise.complete( layerRenderDataList);
				}
				catch(System.Exception e)
				{
					promise.fail(e);
				}

				profileMarker.End();

			}, result=>{

				//int total_ms_time = System.Environment.TickCount - total_start_time;
				//int complete_ms_time = System.Environment.TickCount - complete_time;
				//Debug.Log($"[{System.Environment.TickCount}] buildMesh[{_tile._tilePos}] total {total_ms_time}ms, complete {complete_ms_time}ms");


				if ( result.failed())
				{
					Debug.LogException(result.cause());
					callback(false);
				}
				else
				{
					_tile._dicStyleRenderData.Add(_style, result.result());

					callback(true);
				}

			});
		}

		//private float line_thickness_ratio = 1.2f;

		private void buildLayerMesh(MBLayerRenderData layer,MBStyleExpressionContext ctx,MBStyleExpressionContext ctxHillShade)
		{
			MBMesh merged_mesh = MBMesh.create();
			int render_layer_type = layer.getLayerStyle().getType();
			MBStyleLayer layerStyle = layer.getLayerStyle();

			//MBStyleExpression ex_line_width = layer.getLayerStyle().getLineWidth();
			//MBStyleExpression ex_line_gap_width = layer.getLayerStyle().getLineGapWidth();
			//MBStyleExpression ex_opacity = layer.getLayerStyle().getOpacity();

			//bool isHillShade = layer.getLayerStyle().getID() == "hillshade";
			//bool isHillShade = layer.getLayerStyle().getIDHashCode() == MBLayerRenderData.id_hillShade;

			int start_time = System.Environment.TickCount;

			foreach (MBFeature feature in layer.getFeatureList())
			{
				ctxHillShade._feature = ctx._feature = feature;

				if (feature.type == MBFeatureType.polygon && render_layer_type == MBStyleDefine.LayerType.fill)
				{
					Color color = layerStyle.evaluateFillColor(ctx);
					//color.a = (float)layerStyle.evaluateFillOpacity(ctx);

					_polygonBuilder.build(feature, color, layer, ctx);

					if (merged_mesh != null)
					{
						merged_mesh.append(_polygonBuilder);
					}
				}
				else if (feature.type == MBFeatureType.polygon && render_layer_type == MBStyleDefine.LayerType.fill_extrusion)
				{
					Color color = layerStyle.evaluateFillExtrusionColor(ctx);
					color.a = (float)layerStyle.evaluateFillExtrusionOpacity(ctx);

					_polygonBuilder.setExtrusion(true);
					_polygonBuilder.build(feature, color, layer, ctx);
					_polygonBuilder.setExtrusion(false);

					if (merged_mesh != null)
					{
						merged_mesh.append(_polygonBuilder);
					}
				}
				else if( render_layer_type == MBStyleDefine.LayerType.line && (feature.type == MBFeatureType.polygon || feature.type == MBFeatureType.linestring))
				{
					_lineMeshBuilder.build(feature, Color.white, layer, ctx);
					if( merged_mesh != null)
					{
						merged_mesh.append(_lineMeshBuilder);
					}
				}
				//else if (feature.type == MBFeatureType.linestring && render_layer_type == MBStyleDefine.LayerType.line)
				//{
				//	MBStyleExpression lineColor = layer.getLayerStyle().getLineColor();
				//	Color color = (Color)lineColor.evaluate(ctx);

				//	double line_width = ex_line_width.evaluateDouble(ctx);
				//	double line_gap_width = ex_line_gap_width.evaluateDouble(ctx);

				//	double thickness = 0;

				//	if (line_gap_width > 0)
				//	{
				//		thickness = line_gap_width + 2 * line_width;
				//	}
				//	else
				//	{
				//		thickness = line_width;
				//	}

				//	thickness *= line_thickness_ratio;

				//	_lineBuilder.build(feature, (float)thickness, color);

				//	if (merged_mesh != null)
				//	{
				//		merged_mesh.append(_lineBuilder);
				//	}


				//	try
				//	{
				//		// 테스트
				//		_lineMeshBuilder.build(feature, color, layer, ctx);
				//	}
				//	catch (System.Exception e)
				//	{
				//		Debug.LogException(e);
				//	}

				//}
				//else if (feature.type == MBFeatureType.polygon && render_layer_type == MBStyleDefine.LayerType.line)
				//{
				//	MBStyleExpression lineColor = layer.getLayerStyle().getLineColor();
				//	Color color = (Color)lineColor.evaluate(ctx);

				//	double line_width = ex_line_width.evaluateDouble(ctx);
				//	double line_gap_width = ex_line_gap_width.evaluateDouble(ctx);

				//	double thickness = 0;

				//	if (line_gap_width > 0)
				//	{
				//		thickness = line_gap_width + 2 * line_width;
				//	}
				//	else
				//	{
				//		thickness = line_width;
				//	}

				//	thickness *= line_thickness_ratio;

				//	_lineBuilder.build(feature, (float)thickness, color);

				//	if (merged_mesh != null)
				//	{
				//		merged_mesh.append(_lineBuilder);
				//	}
				//}
			}


			int ms_time = System.Environment.TickCount - start_time;

			if (ms_time > 50)
			{
				Debug.Log($"buildMesh[{_tile._tilePos}] layer[{layer.getLayerStyle().getID()}][{layer.getFeatureList().Count}] - {ms_time}ms");
			}

			if (merged_mesh.isEmpty() == false)
			{
				layer.setMergedMesh(merged_mesh);
			}
		}

		private List<MBLayerRenderData> buildStyledRenderData(MBStyleExpressionContext ctx)
		{
			int zoom = _tile._tilePos.zoom;
			List<MBLayerRenderData> layerRenderList = new List<MBLayerRenderData>();
			foreach(MBStyleLayer layer_style in _style.getLayers())
			{
				if( layer_style.getVisibility() == false)
				{
					continue;
				}

				if( zoom >= layer_style.getMaxZoom())
				{
					//Debug.Log(string.Format("[{0}] filtered by maxZoom : {1} >= {2}", layer_style.getID(), zoom, layer_style.getMaxZoom()));
					continue;
				}
				if( zoom < layer_style.getMinZoom())
				{
					//Debug.Log(string.Format("[{0}] filtered by minZoom : {1} >= {2}", layer_style.getID(), zoom, layer_style.getMinZoom()));
					continue;
				}

				if( layer_style.getType() != MBStyleDefine.LayerType.fill &&
					layer_style.getType() != MBStyleDefine.LayerType.fill_extrusion &&
					layer_style.getType() != MBStyleDefine.LayerType.line &&
					layer_style.getType() != MBStyleDefine.LayerType.symbol)
				{
					continue;
				}

				//// 일단 막고 빌드
				//if( layer_style.getID() == "hillshade")
				//{
				//	continue;
				//}
				
				MBLayer sourceLayer = _tile.getLayer(layer_style.getSourceLayerHashCode());
				if( sourceLayer == null)
				{
					//Debug.Log(string.Format("[{0}] can't find source layer : {1}", layer_style.getID(), layer_style.getSourceLayer()));
					continue;
				}

				MBLayerRenderData render_data = MBLayerRenderData.create(_tile, layer_style, sourceLayer, ctx);
				if( render_data.isEmpty())
				{
					continue;
				}

				//Debug.Log(string.Format("[{0}] features[{1}]", layer_style.getID(), render_data.getFeatureList().Count));

				layerRenderList.Add(render_data);
			}

			return layerRenderList;
		}
	}
}
