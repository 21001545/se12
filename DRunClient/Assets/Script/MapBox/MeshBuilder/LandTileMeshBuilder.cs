using Festa.Client.Module;
using Festa.Client.Module.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class LandTileMeshBuilder : BaseStepProcessor
	{
		private LandTile _tile;
		private MultiThreadWorker _multiThreadWorker;
		private LandPolygonMeshBuilder _meshBuilder;
		//private int _index;

		public static LandTileMeshBuilder create(LandTile landTile)
		{
			LandTileMeshBuilder builder = new LandTileMeshBuilder();
			builder.init(landTile);
			return builder;
		}

		private void init(LandTile landTile)
		{
			base.init();

			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();
			_meshBuilder = MBMeshBuilder.create<LandPolygonMeshBuilder>();
			_tile = landTile;
			//_index = 0;
		}

		protected override void buildSteps()
		{
			_stepList = new List<StepProcessor>();
			_stepList.Add(buildMesh);
		}

		private void buildMesh(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<Module.Void>(promise => { 
				
				try
				{
					foreach(LandTileFeature feature in _tile.featureList)
					{
						buildMeshFeature(feature);
					}

					promise.complete();
				}
				catch(Exception e)
				{
					promise.fail(e);
				}
			}, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		private void buildMeshFeature(LandTileFeature feature)
		{
			Color color = Color.green;
			//if( _index % 5 == 0)
			//{
			//	color = Color.red;
			//}
			//else if( _index % 5 == 1)
			//{
			//	color = Color.gray;
			//}
			//else if( _index % 5 == 2)
			//{
			//	color = Color.black;
			//}
			//else if( _index % 5 == 3)
			//{
			//	color = Color.green;
			//}
			//else
			//{
			//	color = Color.blue;
			//}

			//++_index;

			_meshBuilder.build(feature,color);
			feature._mesh = MBMesh.create(_meshBuilder);
		}
	}
}
