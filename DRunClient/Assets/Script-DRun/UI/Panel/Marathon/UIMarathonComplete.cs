using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Serialization;

namespace DRun.Client
{
    public class UIMarathonComplete : UISingletonPanel<UIMarathonComplete>
    {
        public TMP_Text text_goal_ratio;
        public TMP_Text text_goal_unit;
		public TMP_Text text_current;
        public TMP_Text text_goal;
        public UICircleLine gauge_goal;

        [SerializeField]
        private SVGImage _svg_goal;
        
        [SerializeField]
        private float _bouncingDuration = 1.25f;

        private ClientRunningLog _log;
        private RefStringCollection StringCollection => GlobalRefDataContainer.getInstance().getStringCollection();
        private Tween _bouncingLightTw;

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            // bouncing 라이트 Tween 만들기.
            var origCol = _svg_goal.color;
            _svg_goal.color = new Color(
                origCol.r,
                origCol.g,
                origCol.b,
                0
            );

            // start alpha of svg with 0.
            _bouncingLightTw = DOTween.ToAlpha(
                getter: () => _svg_goal.color,
                setter: newCol =>
                {
                    _svg_goal.color = new Color(
                        origCol.r,
                        origCol.g,
                        origCol.b,
                        newCol.a
                    );
                },
                endValue: 1,
                duration: _bouncingDuration
            )
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false)
                .Pause();
        }

        public void open(ClientRunningLog log)
        {
            base.open();

            _log = log;
            setupUI();
        }

        private void setupUI()
        {
            _bouncingLightTw.Restart();

            float goalRatio = _log.getGoalRatio();

            gauge_goal.setFillAmount(goalRatio);
            text_goal_ratio.text = $"{(int)(goalRatio * 100)}%";

            if (_log.running_sub_type == ClientRunningLogCumulation.MarathonType._free_time)
            {
				int goal_minute = _log.goal;
				int result_minute = _log.running_time / 60;


				int curHour = result_minute % 60;
				string minutes = StringCollection.getFormat("marathon.result.goal.time.minutes", 0, curHour);
				text_current.text = minutes;

				// 목표 시간 00시간 (00분 은 생략)
				string timeStr = StringCollection.getFormat(
					"marathon.result.goal.time", 0,
					goal_minute / 60,
					goal_minute % 60);
				text_goal.text = timeStr;
			}
			else
            {
                double result = _log.distance;
                double goal_km = _log.goal / 1000.0;

                text_current.text = $"{result.ToString("N2")}km";
                text_goal.text = StringCollection.getFormat(
	                "marathon.result.goal.option",
	                0, goal_km.ToString("N2")
	                );
            }
        }

		//     private string makeTimeString(int minutes)
		//     {
		//      int h = minutes / 60;
		//      int m = minutes % 60;
		//string formatted = StringCollection
		//	.getFormat("marathon.result.goal.time", 0, h, m);
		//return formatted;
		//     }

		public void onClick_checkRecord()
        {
            _bouncingLightTw.Pause();

            UIRunningResult.getInstance().open(_log);
        }
    }
}
