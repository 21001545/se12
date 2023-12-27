using System;
using System.Collections;
using System.Linq;
using DRun.Client.Running;

using Festa.Client;
using Festa.Client.MapBox;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class ScreenShotCapturer : MonoBehaviour
	{
		[field: Header("스크린샷 찍기용 복사된 MapBox")]
		[field: SerializeField]
		public Camera RenderCamera { get; private set; }

		[field: SerializeField]
		public RectTransform CopiedMapBoxRoot { get; private set; }

		[field: SerializeField]
		public UnityMapBox CopiedMapBox { get; private set; }

		[field: SerializeField]
		public RectTransform CopiedMapShare { get; private set; }
		
		[field: SerializeField]
		public RectTransform CopiedMapShareBottomExtent { get; private set; }

		private static readonly MBLongLatCoordinate InitLongLat = new(126.05065589703139, 37.50850136014864);

		public UMBActor_StartPoint CopiedActorStartPoint { get; private set; }

		public UMBActor_StartPoint CopiedActorEndPoint { get; private set; }

		public UMBActor_RunningPath CopiedActorRunningPath { get; private set; }

		[field: Space(20)]
		[field: Header("공유 UI")]
		[field: SerializeField]
		public TMP_Text Share_text_distance { get; private set; }
		
		[field: SerializeField]
		public TMP_Text Share_text_date { get; private set;  }

		[field: SerializeField]
		public TMP_Text Share_text_time { get; private set; }

		[field: SerializeField]
		public TMP_Text Share_text_step { get; private set; }

		[field: SerializeField]
		public TMP_Text Share_text_calorie { get; private set; }

		[field: SerializeField]
		public TMP_Text Share_text_velocity { get; private set; }

		[Header("스크린샷 촬영 결과")]
		[Space(20)]
		[SerializeField]
		private Texture2D _previewScreenShot;

		const int DefaultMapUpdatingFrames = 100;

		private void Awake()
		{
			RenderCamera.targetTexture.width = Screen.width;
			RenderCamera.targetTexture.height = Screen.width;
		}

		public void initCopiedMap()
		{
			CopiedMapBox.init(false, RenderCamera, ClientMain.instance.getMBStyleCache(), MBAccess.defaultStyle);

			CopiedActorStartPoint = (UMBActor_StartPoint)CopiedMapBox.spawnActor("drun.result.startPoint", InitLongLat);
			CopiedActorEndPoint = (UMBActor_StartPoint)CopiedMapBox.spawnActor("drun.result.endPoint", InitLongLat);
			CopiedActorRunningPath = (UMBActor_RunningPath)CopiedMapBox.spawnActor("drun.path", InitLongLat);
			CopiedActorRunningPath.thickness = 5.0f;
		}

		private IEnumerator updateCopiedMapbox(int updatingFrames)
		{
			int cnt = 0;
			while (cnt <= updatingFrames)
			{
				CopiedMapBox.update();
				++cnt;

				yield return new WaitForEndOfFrame();
			}
		}

		public IEnumerator capture(Action before, Action<Texture2D> after)
		{
			before();
			yield return updateCopiedMapbox(DefaultMapUpdatingFrames);

			// 맵박스 corner 좌표 얻기 (캡쳐 Rect)
			var corners = new Vector3[4];
			CopiedMapShare.GetWorldCorners(corners);
			corners = corners.Select(RenderCamera!.WorldToScreenPoint).ToArray();

			var prev = RenderTexture.active;
			RenderTexture.active = RenderCamera.targetTexture;

			RenderCamera.Render();

			Vector2 border = corners[3];
			Texture2D screenShot = new((int)border.x, (int)border.x);

// #if UNITY_IOS
// 			screenShot.ReadPixels(new Rect(0, Screen.height - border.x, border.x, Screen.height), 0, 0);
// #else
			screenShot.ReadPixels(new Rect(0, 0, border.x, Screen.height - border.y), 0, 0);
// #endif
			screenShot.Apply(false);
			RenderTexture.active = prev;

			screenShot = scaleTexture(screenShot, 1080, 1080);

#if UNITY_EDITOR
			_previewScreenShot = screenShot;
#endif
			after(screenShot);
		}

		private static Texture2D scaleTexture(Texture2D source, int targetWidth, int targetHeight)
		{
			Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
			Color[] rpixels = result.GetPixels(0);
			float incX = (1.0f / (float)targetWidth);
			float incY = (1.0f / (float)targetHeight);
			for (int px = 0; px < rpixels.Length; px++)
			{
				rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
			}
			result.SetPixels(rpixels, 0);
			result.Apply();
			return result;
		}
	}
}


