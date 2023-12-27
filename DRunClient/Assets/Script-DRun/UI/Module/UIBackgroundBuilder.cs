using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UIBackgroundBuilder : SingletonBehaviourT<UIBackgroundBuilder>
	{
		public Camera targetCamera;
		public GameObject imageRoot;
		public GameObject blurRoot;

		[Serializable]
		public class Job
		{
			public GameObject targetRoot;
			public RenderTexture renderTexture;
		}

		public Job[] buildList;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			// 세로모드만 쓸거니까
			targetCamera.rect = new Rect(0, 0, (float)Screen.width / (float)Screen.height, 1.0f);
			targetCamera.gameObject.SetActive(false);

			imageRoot.SetActive(false);
		}

		public void build(UnityAction complete)
		{
			StartCoroutine(_build(complete));
		}

		private IEnumerator _build(UnityAction complete)
		{
			imageRoot.SetActive(true);
			blurRoot.SetActive(true);

			foreach (Job job in buildList)
			{
				job.targetRoot.SetActive(false);
			}

			foreach(Job job in buildList)
			{
				targetCamera.targetTexture = job.renderTexture;

				job.targetRoot.SetActive(true);

				yield return new WaitForEndOfFrame();
				yield return new WaitForEndOfFrame();

				targetCamera.Render();

				job.targetRoot.SetActive(false);
			}

			imageRoot.SetActive(false);
			complete();
			yield return null;
		}
	}
}
