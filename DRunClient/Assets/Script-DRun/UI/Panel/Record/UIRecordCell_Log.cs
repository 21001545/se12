using DRun.Client.Logic.Record;
using DRun.Client.Module;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIRecordCell_Log : EnhancedScrollerCellView
	{
		public float height;

		public TMP_Text text_end_time;
		public TMP_Text text_distance;
		public TMP_Text text_running_time;
		public TMP_Text text_step_count;
		public TMP_Text text_calorie;
		public TMP_Text text_velocity;

		public RawImage image_map;
		public GameObject image_loading;
		public Texture2D emptyTexture;

		public GameObject[] mode_pages;

		[Header("======= ProMode =======")]
		public TMP_Text text_drn_total;

		[Header("======= Marathon ======")]
		public UICircleLine goal_gauge;
		public GameObject goal_complete;

		private TextureCacheItemUsage _textureUsage;
		private ClientRunningLog _log;

		private TextureCache textureCache => ClientMain.instance.getTextureCache();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public void setup(ClientRunningLog log)
		{
			_log = log;

			text_end_time.text = StringUtil.toRecordTime(log.end_time.ToLocalTime());
			text_distance.text = StringUtil.toDistanceString(log.distance);
			text_running_time.text = StringUtil.toRunningTimeString(System.TimeSpan.FromSeconds(log.running_time));
			text_step_count.text = StringCollection.getFormat("pro.record.log.step", 0, log.step_count.ToString("N0"));
			text_calorie.text = StringCollection.getFormat("pro.record.log.calorie", 0, log.calories.ToString("N0"));
			text_velocity.text = StringCollection.getFormat("pro.record.log.velocity", 0, log.velocity.ToString("N1"));

			mode_pages[0].SetActive( _log.isProMode());
			mode_pages[1].SetActive(_log.isProMode() == false);

			if ( _log.isProMode())
			{

				text_drn_total.text = StringUtil.toDRNStringGrouped(log.drn_total);
			}
			else
			{
				float ratio = _log.getGoalRatio();
				goal_gauge.setFillAmount(ratio);

				goal_complete.SetActive(ratio >= 1.0f);
			}


			loadMapImage();
		}

		public void onWillRecycle()
		{
			clear();
		}

		public void clear()
		{
			image_map.texture = emptyTexture;
			if( _textureUsage != null)
			{
				textureCache.deleteUsage(_textureUsage);
				_textureUsage = null;
			}
		}

		private void loadMapImage()
		{
			clear();

			image_loading.gameObject.SetActive(true);

			Vector2 size = image_map.rectTransform.rect.size;
			Vector2Int sizeInt = new Vector2Int((int)size.x, (int)size.y);

			RunningMapImageProcessor step = RunningMapImageProcessor.create( _log, sizeInt, textureCache);
			step.run(result => {
				image_loading.gameObject.SetActive(false);

				if ( result.succeeded())
				{
					TextureCacheItemUsage textureUsage = step.getTextureUsage();

					// 그럴 수 있다
					if( step.getLog() != _log)
					{
						textureCache.deleteUsage(textureUsage);
					}
					else
					{
						image_map.texture = textureUsage.texture;
						_textureUsage = textureUsage;
					}
				}
			});
		}

		public void onClickDetail()
		{
			UIRecord.getInstance().close();
			UIRunningResult.getInstance().open(_log);
		}

		//private Vector2Int calcImageSize()
		//{
		//	Vector3[] localCorner = new Vector3[4]; ;
		//	RectTransform rt = image_map.transform as RectTransform;
		//	rt.GetLocalCorners(localCorner);

		//	//Rect thisRect = rt.rect;
		//	//Rect parentRect = ((RectTransform)rt.parent).rect;

		//	Vector2Int result = new Vector2Int();
		//	result.x = (int)(localCorner[2].x - localCorner[0].x);
		//	result.y = (int)(localCorner[2].y - localCorner[0].y);

		//	// Debug.Log($"calcImageSize:{result}");

		//	return result;
		//}
	}
}
