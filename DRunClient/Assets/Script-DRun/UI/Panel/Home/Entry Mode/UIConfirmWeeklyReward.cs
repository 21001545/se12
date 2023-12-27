using DRun.Client.Module;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System;
using DG.Tweening;
using DRun.Client.RefData;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
    public class UIConfirmWeeklyReward : UISingletonPanel<UIConfirmWeeklyReward>
    {
        // public Image gaugeCoin;
        public TMP_Text text_day;
        public TMP_Text text_hour;
        public TMP_Text text_minute;
        public TMP_Text text_second;
        public TMP_Text text_amount;

        [SerializeField]
        private TMP_Text text_level_bonus_percentage;

        private IntervalTimer _timer;

        private RectTransform _selfRectTransform;

        ClientNetwork Network => ClientMain.instance.getNetwork();
        ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            _selfRectTransform = GetComponent<RectTransform>();
            if (TryGetComponent<SlideUpDownTransition>(out var c))
                    c.useOnlyOpen = true;

            _timer = IntervalTimer.create(1.0f, true, true);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        {
            resetBindings();
            base.open(param, transitionType, closeType);
        }

        private void resetBindings()
        {
            if (_bindingManager.getBindingList().Count > 0)
            {
                return;
            }

            BasicModeViewModel basicVM = ViewModel.BasicMode;

            _bindingManager.makeBinding(basicVM, nameof(basicVM.WeeklyReward), updateReward);
            _bindingManager.makeBinding(basicVM, nameof(basicVM.LevelData), updateLevel);
        }

        public override void update()
        {
            base.update();

            if (gameObject.activeSelf && _timer.update())
            {
                updateReward(null);
            }
        }

        public void onClick_Back()
        {
            //close();
            // NOTE 2023/01/12 - 이윤상
            // Open (SlideUpDownTransition) -> SmoothDamp 로 Panel 닫을 때 속도가 너무 빨라서,
            // + 강제로 Close 로직 막기
            // + 닫기 Transition 직접 DOTween 으로 함..
            // https://easings.net/ - 참조
            _selfRectTransform.moveInDirectionWithCallback(
                delta: (from: 0, to: -Screen.height * 0.5f),
                whichDirection: WhichDirection.Vertical,
                transitionDurationInSeconds: 0.4f,
                onComplete: () => base.close(),
                easeFn: Ease.InQuint
            );
        }

        public void onClick_Confirm()
        {
            UIClaimWeeklyReward.getInstance().open();
        }

        private void updateReward(object obj)
        {
            BasicModeViewModel basicVM = ViewModel.BasicMode;
            TimeSpan remainTime = basicVM.getWeekRewardRemainTime();
            int amount = basicVM.getWeekRewardAmount();

            // float ratio = (float)remainTime.TotalSeconds / (float)(TimeUtil.msWeek / TimeUtil.msSecond);
            // ratio = Mathf.Clamp( 1.0f - ratio, 0, 1);

            // gaugeCoin.fillAmount = ratio;

            text_day.text = StringCollection.getFormat("basic.weeklyrewards.time.day", 0, remainTime.Days);
            text_hour.text = remainTime.Hours.ToString("D2");
            text_minute.text = remainTime.Minutes.ToString("D2");
            text_second.text = remainTime.Seconds.ToString("D2");

            text_amount.text =
                StringCollection.getFormat("basic.weeklyrewards.finish.desc", 0, StringUtil.toDRNStringAll(amount));
        }

        private void updateLevel(object _)
        {
            var basicVM = ViewModel.BasicMode;
            var refLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(
                basicVM.LevelData.level
            );

            string level = basicVM.LevelData.level.ToString();
            string bonusPercentage = refLevel.bonus_percent.ToString();

            text_level_bonus_percentage.text =
                StringCollection.getFormat("basic.weeklyrewards.level_bonus", 0, level, bonusPercentage);
        }
    }
}