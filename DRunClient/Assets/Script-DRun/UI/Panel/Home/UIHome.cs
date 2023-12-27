using DG.Tweening;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.RefData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Script.Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DRun.Client.Logic.ProMode;
using UnityEngine.UI.ProceduralImage;
using Random = UnityEngine.Random;

namespace DRun.Client
{
    public class UIHome : UISingletonPanel<UIHome>
    {
        private const float NoExpLeftToGainTransitionDurationInSeconds = 1.0f;

        #region fields

        /// <summary>
        /// 0 - on
        /// 1 - off
        /// </summary>
        [Space(20)]
        [Header("=========== Resources ==========")]
        [SerializeField]
        private Sprite[] _icon_heart_on_off;

        /// <summary>
        /// 0 - on
        /// 1 - off
        /// </summary>
        [SerializeField]
        private Sprite[] _icon_distance_on_off;

        /// <summary>
        /// 0 - on
        /// 1 - off
        /// </summary>
        [SerializeField]
        private Sprite[] _icon_stamina_on_off;

        public Vector2 hds_icon_dimension = new(24, 24);

        [SerializeField]
        private float _drnChangeNotificationClosingTime = 3.0f;

        [SerializeField]
        private UIEntryProfileNotification _entryProfileNotification;

		[Space(20)]
        [Header("========== DRN ============ ")]
        public TMP_Text text_drn;

        public TMP_Text text_drnChangeAmount;
        public GameObject _drnNotiPanel;

        [SerializeField]
        private Animator _anim_noti_panel;

        [Header("========== Mode =========== ")]
        public AbstractPanelTransition[] transitionsBasic;

        public AbstractPanelTransition[] transitionsPro;
        public AbstractPanelTransition[] transitionsMarathon;

        public TMP_Text text_mode;
        public Image mode_background;
        public Animator anim_mode_arrow;
        public Animator anim_mode_dropdown;


        [Header("========== Entry Mode =========== ")]
        [SerializeField]
        private UIEntryModeDailyReward[] _rewards;

        public TMP_Text text_completeRewardCount;
        public UINumberCountingAnimation txt_todaySteps;

        [SerializeField]
        private UICircleLineFillAnimator _dailyStepGaugeAnimator;

        [SerializeField]
        private UICircleLinePointMover _dailyStepGaugePointMover;

        [SerializeField]
        private ProceduralImage _dailyStepGaugePointShadow;

        public Button btnStepDebugger;

        [Header("Daily Step Background")]
        [Space(10)]
        [SerializeField]
        private UIAlphaBlender _bg_step_bg_alphaBlender;

        [SerializeField]
        private UISwitcher _bg_step_bg_switcher;

        [Space(10)]
        [Header("Profile")]
        public TMP_Text text_entry_currentExpMaxExp;

        /// <summary>
        /// 0 - entry 경험치 표시 text
        /// 1 - entry level-up 버튼
        /// </summary>
        public UISwitcher entry_profileExt_buttons_switcher;

        public TMP_Text text_entry_currentLevel;
        public TMP_Text text_entry_welcome;
        public TMP_Text text_entry_name;

        public UIPhotoThumbnail top_profile_image;
        public UIPhotoThumbnail top_profile_extended_image;

        [SerializeField]
        private GameObject _level_bonus_container;

        [Space(10)]
        [Header("Notification")]
        [SerializeField]
        private GameObject _profile_panel;

        [SerializeField]
        private GameObject _notification_panel;

        [Header("========== Pro Mode ============= ")]
        [Space(10)]
        [Header("Gauge")]
        public UICircleLineFillAnimator pro_exp_gauge_fill_animator;

        public UICircleLinePointMover pro_exp_gauge_point_mover;
        public UICircleLineFillAnimator pro_stamina_gauge_animator;
        public UICircleLineFillAnimator pro_heart_gauge_animator;
        public UICircleLineFillAnimator pro_distance_gauge_animator;

        [Space(10)]
        public TMP_Text text_pro_welcome;

        public TMP_Text text_pro_name;

        public TMP_Text pro_cur_level;
        public TMP_Text pro_next_level;

        public GameObject pro_pfp_not_selected;
        public UIPhotoThumbnail pro_pfp_image;

        [SerializeField]
        private Image _img_heart;

        [SerializeField]
        private Image _img_distance;

        [SerializeField]
        private Image _img_stamina;

        [SerializeField]
        private Image _img_logo;

        [SerializeField]
        private Button _btn_pro_levelUp;

        [SerializeField]
        private Button _btn_startRecordingPromode;

        [Space(10)]
        public TMP_Text pro_level_bonus;

        public TMP_Text pro_text_heart;
        public TMP_Text pro_text_distance;
        public TMP_Text pro_text_stamina;

        [Header("========== Marathon Mode ============= ")]
        public TMP_Text text_mt_welcome;

        public TMP_Text text_mt_name;
        public TMP_Text text_mt_distance;
        public TMP_Text text_mt_time;
        public TMP_Text text_mt_velocity;
        public TMP_Text text_mt_calories;


        [Space(20)]
        private List<AbstractPanelTransition> pageTransitionList;

        private List<AbstractPanelTransition[]> pageTransitionGroupList;

        #endregion fields

        #region data

        private ClientNetwork Network => ClientMain.instance.getNetwork();
        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        private IDisposable _drnChangeDeferUnsubscriber;

        private bool HasUserPFP => ViewModel.ProMode.EquipedNFTItem is not null;
        private ClientNFTItem NftItem => ViewModel.ProMode.EquipedNFTItem;

        private float LevelUpCost =>
            GlobalRefDataContainer.getInstance().get<RefNFTLevel>(NftItem.level + 1).levelup_drnt;

        private string LevelUpCostAsStr => LevelUpCost.ToString();
        private bool IsMaxLevel => GlobalRefDataContainer.getInstance().get<RefNFTLevel>(NftItem.level + 1) is null;

        private bool _isInitGauge = false;
        private string WelcomeText => StringCollection.get("pro.welcome.type", Random.Range(0, 2 + 1));

        private static readonly int Close = Animator.StringToHash("close");
        private static readonly int Open = Animator.StringToHash("open");

        #region tweener

        private TweenerCore<Color, Color, ColorOptions> _dailyStepGaugePointMoverFadeOutTweener;
        private TweenerCore<Color, Color, ColorOptions> _dailyStepGaugePointShadowFadeOutTweener;
        private IEnumerable<TweenerCore<Color, Color, ColorOptions>> _bgStepBackgroundAlphaBlenderTweens;

        private bool _alreadyGetDailyStepCountExpNotified;

		/// <summary>
		/// PlayModeViewModel.PlayMode 로 비교.
		/// </summary>
		private int CurrentMode => ViewModel.PlayMode.Mode;

        private const float WelcomeTextTransitionInSeconds = 1.5f;

        #endregion tweener

        #endregion data

        #region event functions

        private void OnDestroy()
        {
            _drnChangeDeferUnsubscriber?.Dispose();
        }

        #endregion event functions

        #region override

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            txt_todaySteps.init();
            txt_todaySteps.onNumberChanged.AddListener(onTodayStepCountNumberChanged);

            // drn 변화 noti panel 은 처음에 끄쟈
            _anim_noti_panel.gameObject.SetActive(false);
            _drnChangeDeferUnsubscriber = ViewModel
                .Wallet
                .SubscribeToDRNChangeDeferrer(
	                new DRNChangeDeferObserver(
		                _drnNotiPanel, 
		                _anim_noti_panel, 
		                text_drnChangeAmount, 
		                _drnChangeNotificationClosingTime
					)
				);

            initPageTransitions();

            initTweens();
        }

        public override void onLanguageChanged(int newLanguage)
        {
            // 음
            _bindingManager.updateAllBindings();
        }

        private void initGauge()
        {
            if (_isInitGauge)
                return;

            _isInitGauge = false;

            pro_exp_gauge_fill_animator.init();
            pro_exp_gauge_point_mover.init();

            pro_heart_gauge_animator.init();
            pro_distance_gauge_animator.init();
            pro_stamina_gauge_animator.init();

            _dailyStepGaugeAnimator.init();
            _dailyStepGaugePointMover.init();
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        {
            

            resetBindings();

            initGauge();

            this.startTypingAnimationForWelcomeMessage();

            // notification / profile UI 초기화.
            _profile_panel.SetActive(true);
            _notification_panel.SetActive(false);

            //btn_entry_levelup.SetActive(false);

            // normal bg 선택
            _bg_step_bg_switcher.@switch(0);

            base.open(param, transitionType, closeType);
        }

        public override void close(int transitionType = 0)
        {
            this.pauseTweeners();
 
            base.close(transitionType);
        }

        public override void update()
        {
            base.update();

            txt_todaySteps.update();
            updatePageTransitions();
        }

        #endregion override

        private void transitionExpGaugeAndBackground()
        {
            _dailyStepGaugePointMoverFadeOutTweener?.Play();
            _dailyStepGaugePointShadowFadeOutTweener?.Play();

            foreach (var t in _bgStepBackgroundAlphaBlenderTweens)
                t?.Play();
        }

        private void resetBindings()
        {
            if (_bindingManager.getBindingList().Count > 0)
            {
                return;
            }

            ProfileViewModel profileVM = ViewModel.Profile;
            WalletViewModel walletVM = ViewModel.Wallet;
            BasicModeViewModel basicVM = ViewModel.BasicMode;
            PlayModeViewModel modeVM = ViewModel.PlayMode;
            ProModeViewModel proVM = ViewModel.ProMode;
            RecordViewModel recordVM = ViewModel.Record;

            // mode
            _bindingManager.makeBinding(modeVM, nameof(modeVM.Mode), updatePlayMode);

            _bindingManager.makeBinding(walletVM, nameof(walletVM.DRN_Balance), updateDRN_Balance);

            // Profile
            _bindingManager.makeBinding(profileVM, nameof(profileVM.Profile), updateName);

            foreach (var reward in _rewards)
            {
                _bindingManager.makeBinding(basicVM, nameof(basicVM.DailyReward), reward.updateDailyReward);
                reward.onNotifyAllDailyRewardsRecived += () =>
                {
                    // 이미 
                    if (_alreadyGetDailyStepCountExpNotified)
                        return;

                    _entryProfileNotification.notify("+ 1 exp");
					_alreadyGetDailyStepCountExpNotified = true;
                };

                reward.onNotifyAllDailyRewardsRecived += () => ViewModel.Wallet.NotifyAccumulatedDRNChange();
            }

            // BasicMode
            _bindingManager.makeBinding(basicVM, nameof(basicVM.TodayStepCount), updateTodayStepCount);
            _bindingManager.makeBinding(basicVM, nameof(basicVM.LevelData), updateEntryLevel);

            _bindingManager.makeBinding(basicVM, nameof(basicVM.DailyReward), updateDailyReward);

            _bindingManager.makeBinding(basicVM, nameof(basicVM.ClaimedWeeklyRewardData), updateClaimedWeeklyRewardData);

            _bindingManager.makeBinding(proVM, nameof(proVM.EquipedNFTItem), updateProMode);

            // 마라톤 통계
            _bindingManager.makeBinding(recordVM, nameof(recordVM.CumulationLogMap), updateMarathonRecord);
        }

        #region modeTransition

        public override void onTransitionEvent(int type)
        {
            if (type == TransitionEventType.start_open)
            {
                txt_todaySteps.stop();
                showStepDebugger();

            }
            else if (type == TransitionEventType.end_open)
            {
                float speed =
                    (float)GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.ui_stepcount_duration,
                        100) / 1000.0f;
                txt_todaySteps.smoothTime = speed;
                txt_todaySteps.start();

                ViewModel.Wallet.NotifyAccumulatedDRNChange();
				processInvitationReward();
			}
			else if (type == TransitionEventType.start_close)
            {
                txt_todaySteps.stop();
            }
        }

        private void initPageTransitions()
        {
            pageTransitionList = new List<AbstractPanelTransition>();
            pageTransitionList.AddRange(transitionsBasic);
            pageTransitionList.AddRange(transitionsPro);
            pageTransitionList.AddRange(transitionsMarathon);

            pageTransitionGroupList = new List<AbstractPanelTransition[]>();
            pageTransitionGroupList.Add(transitionsBasic);
            pageTransitionGroupList.Add(transitionsPro);
            pageTransitionGroupList.Add(transitionsMarathon);

            foreach (AbstractPanelTransition transition in pageTransitionList)
            {
                transition.init(null);
            }
        }

        private void initTweens()
        {
            //  exp point tween (alpha -> 0)
            //  exp point shadow tween (alpha -> 0)
            _dailyStepGaugePointMoverFadeOutTweener =
                _dailyStepGaugePointMover.pointCircle
                    .DOFade(0, NoExpLeftToGainTransitionDurationInSeconds)
                    .SetDelay(1.5f)
                    .Pause()
                    .SetAutoKill(false);

            _dailyStepGaugePointShadowFadeOutTweener = _dailyStepGaugePointShadow
                .DOFade(0, NoExpLeftToGainTransitionDurationInSeconds)
                .SetDelay(1.5f)
                .Pause()
                .SetAutoKill(false);

            // fade in centre circle bg (alpha -> 1)
            const float dstAlpha = 0.5019f;
            const float origAlpha = 0;//0.1254f;

            _bgStepBackgroundAlphaBlenderTweens = _bg_step_bg_alphaBlender
                .blendToEnableOnlyOneTweener(
                    enableOnlyIndex: 1,
                    range: (from: origAlpha, to: dstAlpha),
                    blendDurationInSecondsOverride: NoExpLeftToGainTransitionDurationInSeconds + 1
                ).Select(t => t
                    .SetDelay(2.5f)
                    .Pause()
                    .SetAutoKill(false)
                );
        }

        private void updatePageTransitions()
        {
            foreach (AbstractPanelTransition transition in pageTransitionList)
            {
                transition.update();
            }
        }

        private void setMode(int mode, bool noTransition)
        {
            if (noTransition)
            {
                for (int i = 0; i < pageTransitionGroupList.Count; ++i)
                {
                    if ((i + 1) == mode)
                    {
                        openImmediately(pageTransitionGroupList[i]);
                    }
                    else
                    {
                        closeImmediately(pageTransitionGroupList[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < pageTransitionGroupList.Count; ++i)
                {
                    if ((i + 1) == mode)
                    {
                        startOpen(pageTransitionGroupList[i]);
                    }
                    else
                    {
                        startClose(pageTransitionGroupList[i]);
                    }
                }
            }
        }

        private void startOpen(AbstractPanelTransition[] list)
        {
            foreach (AbstractPanelTransition transition in list)
            {
                transition.startOpen();
            }
        }

        private void startClose(AbstractPanelTransition[] list)
        {
            foreach (AbstractPanelTransition transition in list)
            {
                transition.startClose();
            }
        }

        private void openImmediately(AbstractPanelTransition[] list)
        {
            foreach (AbstractPanelTransition transition in list)
            {
                transition.openImmediately();
            }
        }

        private void closeImmediately(AbstractPanelTransition[] list)
        {
            foreach (AbstractPanelTransition transition in list)
            {
                transition.closeImmediately(0);
            }
        }


        public void onClick_Mode()
        {
            anim_mode_arrow.SetTrigger(Open);
            anim_mode_dropdown.SetTrigger(Open);

            UISelectMode2.getInstance().open(new UIPanelOpenParam_UISelectMode2
            {
                onClosePanel = OnCloseSelectModePanel
            });
        }

        private void OnCloseSelectModePanel()
        {
            anim_mode_arrow.SetTrigger(Close);
            anim_mode_dropdown.SetTrigger(Close);
        }

        private void toggleUIForNonPFPHolder(bool isEnable)
        {
            // 달리기 버튼 비활성화.
            _btn_startRecordingPromode.interactable = isEnable;

            // 아이콘 변경
            // HDS Gauge 끄기.
            if (isEnable)
            {
                pro_heart_gauge_animator.gameObject.SetActive(true);
                pro_distance_gauge_animator.gameObject.SetActive(true);
                pro_stamina_gauge_animator.gameObject.SetActive(true);

                _img_heart.sprite = _icon_heart_on_off[0];
                _img_distance.sprite = _icon_distance_on_off[0];
                _img_stamina.sprite = _icon_stamina_on_off[0];
                _img_logo.gameObject.SetActive(false);
            }
            else
            {
                pro_heart_gauge_animator.gameObject.SetActive(false);
                pro_distance_gauge_animator.gameObject.SetActive(false);
                pro_stamina_gauge_animator.gameObject.SetActive(false);

                _img_heart.sprite = _icon_heart_on_off[1];
                _img_distance.sprite = _icon_distance_on_off[1];
                _img_stamina.sprite = _icon_stamina_on_off[1];
                _img_logo.gameObject.SetActive(true);
            }
        }

        private bool needToSpawnUnavailableToPFPDetail()
        {
            if (HasUserPFP)
                return false;

            string proModeUnavailableTitle = StringCollection.get("pro.home.popup.unavailable.title", 0);
            string proModeUnavailableDesc = StringCollection.get("pro.home.popup.unavailable.desc", 0);

            UIPopup.spawnOK(proModeUnavailableTitle, proModeUnavailableDesc);
            return true;
        }

        #endregion

        private void updateDRN_Balance(object obj)
        {
            text_drn.text = StringUtil.toDRNStringDefault(ViewModel.Wallet.DRN_Balance.balance);
        }

        private void updatePlayMode(object obj)
        {
            setMode(CurrentMode, _bindingManager.isUpdatingAllBindings());

            switch (CurrentMode)
            {
                case PlayModeViewModel.PlayMode.Basic:
                    {
                        text_mode.text = StringCollection.get("running.mode.basic", 0);
                        mode_background.color = new Color(0.7607843f, 0.3294118f, 1, 0.5f);
                    }
                    break;

                case PlayModeViewModel.PlayMode.Pro:
                    {
                        text_mode.text = StringCollection.get("running.mode.pro", 0);
                        mode_background.color = new Color(0.2666667f, 0.5843138f, 1, 0.5f);
                    }
                    break;

                case PlayModeViewModel.PlayMode.Marathon:
                    text_mode.text = StringCollection.get("running.mode.marathon", 0);
                    break;
            }

            this.startTypingAnimationForWelcomeMessage();
        }

        #region TopMenu

        public void onClick_Wallet()
        {
            UIWallet.getInstance().open();
        }

        #endregion

        #region entry mode

        private void updateName(object obj)
        {
            // 사용자 이름 설정.
            text_mt_name.text = text_entry_name.text = text_pro_name.text =
                ViewModel.Profile.Profile.name;

            text_pro_welcome.text = string.Empty;
            text_mt_welcome.text = string.Empty;
        }

        private void startTypingAnimationForWelcomeMessage()
        {
            switch (CurrentMode)
            {
                case PlayModeViewModel.PlayMode.Basic:
	                text_entry_welcome.text = string.Empty;
					text_entry_welcome.DOText(WelcomeText, WelcomeTextTransitionInSeconds);
                    break;

                case PlayModeViewModel.PlayMode.Pro:
					text_pro_welcome.text = string.Empty;
					text_pro_welcome.DOText(WelcomeText, WelcomeTextTransitionInSeconds);
					break;

                case PlayModeViewModel.PlayMode.Marathon:
					text_mt_welcome.text = string.Empty;
	                text_mt_welcome.DOText(WelcomeText, WelcomeTextTransitionInSeconds);
                    break;
            }
        }

        private void pauseTweeners()
        {
            if (_dailyStepGaugePointMoverFadeOutTweener != null && _dailyStepGaugePointMoverFadeOutTweener.IsPlaying())
                _dailyStepGaugePointMoverFadeOutTweener.Pause();

            if (_dailyStepGaugePointShadowFadeOutTweener != null && _dailyStepGaugePointShadowFadeOutTweener.IsPlaying())
                _dailyStepGaugePointShadowFadeOutTweener.Pause();
        }

        private void updateEntryLevel(object obj)
        {
	        ClientBasicMode levelData = ViewModel.BasicMode.LevelData;
            RefEntryLevel refLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(levelData.level);
            RefEntryLevel refNextLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(levelData.level + 1);

            var profile = (Texture2D)ClientMain.instance.basicModeLevelImage.levelList[levelData.level - 1];

            top_profile_image.setImage(profile, true);
            top_profile_extended_image.setImage(profile, true);

            // 만렙일 때
            var isMaxLevel = refNextLevel == null;
            if (isMaxLevel)
            {
                text_entry_currentLevel.text = "Lv.Max";
                text_entry_currentExpMaxExp.text = "--";

                entry_profileExt_buttons_switcher.@switch(0);

                return;
            }

            // 레벨업 가능 한지
            int gainExp = levelData.exp - refLevel.required_EXP;
            int levelDeltaExp = refNextLevel.required_EXP - refLevel.required_EXP;
            //float expRatio = Mathf.Clamp(gainExp / (float)levelDeltaExp, 0, 1.0f);

            text_entry_currentLevel.text = $"Lv.{refLevel.level}";
            text_entry_currentExpMaxExp.text = $"{gainExp}/{levelDeltaExp}";

            // 레벨업 가능한 상태일 때
            // 2023.02.14 이강희 엔트리모드는 경험치 체계가 다름
            //if (levelData.exp >= levelDeltaExp)
            if( levelData.exp >= refNextLevel.required_EXP)
            {
				entry_profileExt_buttons_switcher.@switch(1);

				// 프로필 Noti 에 레벨 업 가능 하다고 뜸
				string contents = StringCollection.get("basic.levelup.noti.available", 0);
				_entryProfileNotification.notify(contents, 5.0f);

				return;
            }

            entry_profileExt_buttons_switcher.@switch(0);
        }

        private void updateTodayStepCount(object obj)
        {
	        int dailyStepCount = ViewModel.BasicMode.TodayStepCount;

            // 100000 카운트 넘으면 자동으로 띄어주고 경험치 이미 획득한 걸로 표시.
            if (!_alreadyGetDailyStepCountExpNotified && dailyStepCount >= 100_000)
            {
	            _entryProfileNotification.notify("+ 1 exp");
	            _alreadyGetDailyStepCountExpNotified = true;
            }

			txt_todaySteps.setValue(dailyStepCount , false);
            onTodayStepCountNumberChanged(txt_todaySteps.getCurrentValue());
        }

        private void onTodayStepCountNumberChanged(double stepCount)
        {
	        float ratio = (float)(stepCount / (double)ViewModel.BasicMode.TodayStepCountMax);
            ratio = Mathf.Clamp(ratio, 0, 1);

            _dailyStepGaugeAnimator.transtion(ratio);
            _dailyStepGaugePointMover.moveWithTransition(ratio);

            // 오늘 모든 채굴 걸음 수를 채우면
            //if (ratio >= 1.0f)
            if (stepCount >= ViewModel.BasicMode.TodayStepCountMax)
            {
                _dailyStepGaugePointMoverFadeOutTweener?.Play();
                _dailyStepGaugePointShadowFadeOutTweener?.Play();

                _bg_step_bg_switcher.@switch(1);
                // daily step 중앙 blur 채우기
                foreach (var t in _bgStepBackgroundAlphaBlenderTweens)
                    t.Play();
            }

            foreach (var reward in _rewards)
            {
                reward.updateStepCounts((int)stepCount);
            }
        }

        private void updateDailyReward(object obj)
        {
	        text_completeRewardCount.text = $"{ViewModel.BasicMode.DailyRewardCompleteCount}/3";
        }

        private void updateClaimedWeeklyRewardData(object obj)
        {
            ClaimedWeeklyRewardData data = ViewModel.BasicMode.PopClaimedWeeklyRewardData();
            if (data == null)
            {
                return;
            }

            UIClaimWeeklyReward.getInstance().open(data);
        }

        public void onClick_Entry_LevelUp()
        {
            UIConfirmLevelUp.getInstance().open(new UIPanelOpenParam_ConfirmLevelUp
            {
                whereLevelUp = WhereLevelUp.EntryMode,
            });
        }

        public void onClick_Pro_LevelUp()
        {
            CheckNFTLevelUpLimitProcessor checkLimit =
                CheckNFTLevelUpLimitProcessor.create(ViewModel.ProMode.EquipedNFTItem.token_id);
            checkLimit.run(result =>
            {
                if (result.failed())
                {
                    return;
                }

                if (checkLimit.getCheckResult() == false)
                {
                    int daily_limit =
                        GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.daily_levelup_limit, 2);
                    string message = StringCollection.getFormat("pro.levelup.fail.daily_limit", 0, daily_limit);

                    UIToast.spawn(message)
                        .autoClose(3)
                        .setType(UIToastType.alert)
                        .withTransition<FadePanelTransition>();
                    return;
                }

                UIConfirmLevelUp.getInstance().open(new UIPanelOpenParam_ConfirmLevelUp
                {
                    tokenLevelup = this.NftItem.token_id,
                    whereLevelUp = WhereLevelUp.ProMode,
                    formattedPurchaseCost = StringCollection.getFormat("pro.purchase", 0, LevelUpCostAsStr),
                    levelUpCost = LevelUpCost
                });
            });
        }

        public void onClick_WeekyReward()
        {
            UIConfirmWeeklyReward.getInstance().open();
        }

        public void onClick_StepDebugger()
        {
            UIStepDebugger.getInstance().open();
        }

        private void showStepDebugger()
        {
            btnStepDebugger.gameObject.SetActive(GlobalConfig.isInhouseBranch());
		}

        private void processInvitationReward()
        {
            CheckIncompleteInvitationRewardProcessor step = CheckIncompleteInvitationRewardProcessor.create();
            step.run(result => { 
                if( result.succeeded() && step.getIncompleteList().Count > 0)
                {
                    UIConfirmInvitationReward.getInstance().open(step.getIncompleteList());
                }
            });
        }

		#endregion entry mode

		#region ProMode

		private void updateProMode(object obj)
        {
			float exp_ratio = ViewModel.ProMode.ExpRatio;

            pro_exp_gauge_fill_animator.transtion(exp_ratio);
            pro_exp_gauge_point_mover.moveWithTransition(exp_ratio);

            pro_stamina_gauge_animator.transtion(ViewModel.ProMode.StaminaRatio);
            pro_heart_gauge_animator.transtion(ViewModel.ProMode.HeartRatio);
            pro_distance_gauge_animator.transtion(ViewModel.ProMode.DistanceRatio);

            ClientProMode data = ViewModel.ProMode.Data;

            ClientNFTItem nftItem = ViewModel.ProMode.EquipedNFTItem;
            ClientNFTBonus nftBonus = ViewModel.ProMode.NFTBonus;

            //-==================== 스탯 수치

            string heart_value;
            string distance_value;
            string stamina_value;

            if (nftItem != null)
            {
                distance_value = StringUtil.toStatDistanceString(nftItem.distance / 1000.0);
                stamina_value = StringUtil.toStaminaString(nftItem.stamina);

                if (nftItem.max_heart == 0)
                {
                    pro_text_heart.text = "∞";
                }
                else
                {
                    heart_value = nftItem.heart.ToString();

                    if (nftBonus.heart > 0)
                    {
						pro_text_heart.text = StringCollection.getFormat("pro.home.stat.heart", 0, $"{heart_value} <color=#E33ABC>+{nftBonus.heart}</color>");
					}
					else
                    {
                        pro_text_heart.text = StringCollection.getFormat("pro.home.stat.heart", 0, heart_value);
                    }
                }
                
                _level_bonus_container.SetActive(true);
            }
            else
            {
                heart_value = "--";
                distance_value = "--";
                stamina_value = "--";
                pro_text_heart.text = StringCollection.getFormat("pro.home.stat.heart", 0, heart_value);
                
                _level_bonus_container.SetActive(false);
            }

            if( nftBonus.distance > 0)
            {
				string bonus_distance_value = StringUtil.toStatDistanceString(nftBonus.distance / 1000.0);
				pro_text_distance.text = StringCollection.getFormat("pro.home.stat.distance", 0, $"{distance_value} <color=#4F92E7>+{bonus_distance_value}</color>");
			}
            else
            {
				pro_text_distance.text = StringCollection.getFormat("pro.home.stat.distance", 0, distance_value);
			}

			pro_text_stamina.text = StringCollection.getFormat("pro.home.stat.stamina", 0, stamina_value);

            //====================== 레벨

            if (nftItem != null)
            {
                _btn_pro_levelUp.gameObject.SetActive(exp_ratio >= 1.0f && !IsMaxLevel);
            }
            else
            {
                _btn_pro_levelUp.gameObject.SetActive(false);
            }

            //====================== ?덈꺼

            if (nftItem != null)
            {
                //
                RefNFTLevel cur_level = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(nftItem.level);
                RefNFTLevel next_level = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(nftItem.level + 1);

                // 만렙이다
                if (next_level == null)
                {
                    pro_cur_level.text = $"Lv.{nftItem.level}";
                    pro_next_level.text = $"Lv.{nftItem.level}";
                }
                else
                {
                    pro_cur_level.text = $"Lv.{nftItem.level}";
                    pro_next_level.text = $"Lv.{nftItem.level + 1}";
                }

                double bonus_percent = cur_level.mining_bonus_percent / 100.0;
                pro_level_bonus.text = $"{(1.0 + bonus_percent).ToString("N2")}X";
            }
            else
            {
	            pro_cur_level.text = $"Lv. 1";
	            pro_next_level.text = $"Lv. 2";
	            pro_level_bonus.text = $"1.0X";

            }

            //======================= pfp 이미지
            if (nftItem != null)
            {
                pro_pfp_not_selected.gameObject.SetActive(false);
                pro_pfp_image.gameObject.SetActive(true);

                ClientMain.instance.getNFTMetadataCache().getMetadata(nftItem.token_id, cache =>
                {
                    if (cache == null)
                    {
                        pro_pfp_image.setEmpty();
                    }
                    else
                    {
                        pro_pfp_image.setImageFromCDN(cache.imageUrl);
                    }
                });
            }
            else // nft 없으면 기본 로고로 대체.
            {
                pro_pfp_not_selected.gameObject.SetActive(true);
                pro_pfp_image.gameObject.SetActive(false);
            }

			this.toggleUIForNonPFPHolder(HasUserPFP);
		}

        public void onClickStat_Heart()
        {
            string title = StringCollection.get("pro.home.popup.stat.heart.title", 0);
            string desc = StringCollection.get("pro.home.popup.stat.heart.desc", 0);

            var popUp = UIPopup.spawnOK(_icon_heart_on_off[0], title, desc);
            this.adjustSpriteInsideHdsUIPopup(popUp);
        }

        public void onClickStat_Distance()
        {
            string title = StringCollection.get("pro.home.popup.stat.distance.title", 0);
            string desc = StringCollection.get("pro.home.popup.stat.distance.desc", 0);

            var popUp = UIPopup.spawnOK(_icon_distance_on_off[0], title, desc);
            this.adjustSpriteInsideHdsUIPopup(popUp);
        }

        public void onClickStat_Stamina()
        {
            string title = StringCollection.get("pro.home.popup.stat.stamina.title", 0);
            string desc = StringCollection.get("pro.home.popup.stat.stamina.desc", 0);

            var popUp = UIPopup.spawnOK(_icon_stamina_on_off[0], title, desc);
            this.adjustSpriteInsideHdsUIPopup(popUp);
        }

        private void adjustSpriteInsideHdsUIPopup(UIPopup popUp)
        {
            if (popUp.icon_title.TryGetComponent<RectTransform>(out var rt))
            {
                rt.sizeDelta = hds_icon_dimension;
                rt.anchoredPosition = new Vector2(-9, rt.anchoredPosition.y);
            }
        }

        public void onClick_StartRunning()
        {
            if (checkStartRunningCondition() == false)
            {
                return;
            }

            ClientMain.instance.getRunningRecorder().startRecordingProMode();
            UIRunningCountdown.getInstance().open();
        }

        private bool checkStartRunningCondition()
        {
            string desc = null;

            

            ClientNFTItem nftItem = ViewModel.ProMode.EquipedNFTItem;
            if (nftItem == null)
            {
                desc = StringCollection.get("pro.record.start.fail.nft_not_equiped", 0);
            }
            else if(checkNFTStamina() == false)
            {
                desc = StringCollection.get("pro.record.start.fail", 1);
            }
            else if (nftItem.distance <= 0)
            {
                desc = StringCollection.get("pro.record.start.fail", 0);
            }
            else if (nftItem.max_heart > 0 && nftItem.heart <= 0) // 하트 무제한 반영
            {
                desc = StringCollection.get("pro.record.start.fail", 0);
            }

            if (desc != null)
            {
                UIPopup.spawnError(desc);
            }

            return desc == null;
        }

        private bool checkNFTStamina()
        {
            ClientNFTItem nftItem = ViewModel.ProMode.EquipedNFTItem;
            if( nftItem == null)
            {
                return false;
            }

            double staminaRatio = nftItem.getStaminaRatio();
            int efficiencyDRN = GlobalRefDataContainer.getRefDataHelper().getNFTStaminaEfficiencyDRN((int)(staminaRatio * 100));
            if( efficiencyDRN == 0)
            {
                return false;
            }

            return true;
        }

        public void onClick_OpenPFPDetail()
        {
            if (this.needToSpawnUnavailableToPFPDetail())
                return;

            UIPFPDetail.getInstance().open(UIPanelOpenParam_PFPDetail.create(ViewModel.ProMode.EquipedNFTItem, ViewModel.ProMode.NFTBonus, this));
        }

        public void onClick_Setting()
        {
            UISetting.getInstance().open();
        }

#endregion

#region MarathonMode

        public void onClick_StartMarathon()
        {
            UISelectMarathonGoal.getInstance().open();
        }

        public void updateMarathonRecord(object obj)
        {
            var record = ViewModel.Record.getCurrentLogCumulation(ClientRunningLogCumulation.RunningType.marathon, ClientRunningLogCumulation.TimeType.day);

            if (record.log_count == 0)
            {
                text_mt_distance.text = "--";
                text_mt_time.text = "--:--";
                text_mt_velocity.text = "--";
                text_mt_calories.text = "--";
            }
            else
            {
                TimeSpan total_time = TimeSpan.FromSeconds(record.total_time);

                text_mt_distance.text = StringUtil.toDistanceString(record.total_distance);
                text_mt_time.text = $"{total_time.Hours.ToString("D2")}:{total_time.Minutes.ToString("D2")}";
                text_mt_velocity.text = record.getAvgVelocity().ToString("N1");
                text_mt_calories.text = StringUtil.toCaloriesString(record.total_calorie);
            }
        }

#endregion
    }
}