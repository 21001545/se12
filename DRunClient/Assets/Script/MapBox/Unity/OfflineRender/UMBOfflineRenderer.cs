using Festa.Client.Module;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.NetData;
using UnityEngine.Events;

namespace Festa.Client.MapBox
{
	public class UMBOfflineRenderer : SingletonBehaviourT<UMBOfflineRenderer>
	{
		public Camera targetCamera;
		public UnityMapBox mapBox;

		private Queue<UMBAbstractOfflineRenderJob> _job_list;
		private UMBAbstractOfflineRenderJob _currentJob;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			mapBox.init(true, targetCamera, ClientMain.instance.getMBStyleCache(), MBAccess.defaultStyle);

			_job_list = new Queue<UMBAbstractOfflineRenderJob>();
			_currentJob = null;
		}

		public UMBActor_TripPath createPath()
		{
			UMBActor actor_source = mapBox.styleData.actorSourceContainer.getSource("path").actor_source;
			return (UMBActor_TripPath)mapBox.spawnActor(actor_source, new MBLongLatCoordinate(0, 0));
		}

		public void removeAllActors()
		{
			mapBox.removeAllActors();
		}

		public void removeAllTiles()
		{
			mapBox.removeAllTiles();
		}

		public void test()
		{
			targetCamera.Render();
		}

		public void update()
		{
			mapBox.update();

			processJob();
		}

		private void processJob()
		{
			if( _currentJob == null && _job_list.Count > 0)
			{
				_currentJob = _job_list.Dequeue();
				StartCoroutine(_currentJob.run());
			}
		}

		public void endCurrentJob()
		{
			_currentJob = null;
		}

		public void buildForTripPath(List<ClientTripPathData> path_data,Vector2Int size,UnityAction<Texture2D> callback)
		{
			UMBOfflineRenderJob job = UMBOfflineRenderJob.create(this, path_data, size, callback);
			enqueueJob(job);
		}

		public void enqueueJob(UMBAbstractOfflineRenderJob job)
		{
			_job_list.Enqueue(job);
		}
	}
}
