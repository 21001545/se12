using DG.Tweening;
using Festa.Client.Logic;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIMain : UISingletonPanel<UIMain>
	{
		public UIPhotoThumbnail image_photo;

		public UITypingAnimation txt_name;
		public UITypingAnimation txt_message;
		//public TMP_Text txt_name;
		//public TMP_Text txt_message;

		public UINumberCountingAnimation txt_todayTalkCount;
		public UIMain_StepGauge todayStepGauge;

		public UINumberCountingAnimation txt_FestaCoin;
		public TMP_Text txt_Star;

//		public RectTransform stepRewardStartPosition;
//		public RectTransform stepRewardClaimPosition;

//		public RectTransform rtStepRewardArea;
		//public UIMain_StepReward stepRewardSource;

//		public RectTransform rtWalkLevelRoot;
//		public UIWalkLevelItem walkLevelItemSource;

		public GameObject goTripBoost;
		//		public TMP_Text txt_boostRemainCount;
		public TMP_Text txt_BoostCoolTime;

		public Animator anim_circleHole;
		public TMP_Text txt_weather;

		//[SerializeField]
  //      private TMP_Text txt_address;

        [SerializeField]
        private ScrollRect scrollViewRoot;

        [SerializeField]
        private RectTransform go_scrollView;

        [SerializeField]
        public RectTransform img_coin;

        // 나무 테스트용
        public TMP_Text txt_tree_debug_info;

		//private Dictionary<ClientStepReward, UIMain_StepReward> _rewardItemMap;
		//private Dictionary<int, UIWalkLevelItem> _walkLevelItems;
		private IntervalTimer _dailyBoostTimer;
		//private IntervalTimer _walkLevelTimer;
		private IntervalTimer _treePresentaionEventTimer;
		//private bool _isRunningTreePresentation;
		private List<TreePresentationEvent> _treePresentationEventList;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringColletion => GlobalRefDataContainer.getStringCollection();

		//private WalkLevelExpireProcessor _walkLevelExpireProcessor;
		private int _id_time = Animator.StringToHash("time");

		private UIScrollBehaviour _scroller;

		//private float _scroll_height_max;
		//private float _scroll_height;
		//private Vector2 _last_pos;
		//private float _velocity;
		//private float _velocity_on_dragging;
		private bool _reserveScrollDown = false;
		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			//_rewardItemMap = new Dictionary<ClientStepReward, UIMain_StepReward>();
			//_walkLevelItems = new Dictionary<int, UIWalkLevelItem>();
			//stepRewardSource.gameObject.SetActive(false);
			//walkLevelItemSource.gameObject.SetActive(false);
			_dailyBoostTimer = IntervalTimer.create(1.0f, true, false);
			_treePresentaionEventTimer = IntervalTimer.create(0.1f, true, true);
			_treePresentationEventList = new List<TreePresentationEvent>();
			//_isRunningTreePresentation = false;
			//_walkLevelTimer = IntervalTimer.create(1.0f, false, false);
			//_walkLevelExpireProcessor = WalkLevelExpireProcessor.create();
			
			txt_FestaCoin.init();
			txt_name.init();
			txt_message.init();
			txt_todayTalkCount.init();
			todayStepGauge.init();


            //anim_circleHole.speed = 0;

            Vector2 ratioConvert = new Vector2( 5.0f / 50.0f, 45.0f / 50.0f);
			//_scroller = UIScrollBehaviour.create(ratioConvert, onScroll);
			//_scroll_height_max = Screen.height / 2;
			//_scroll_height = _scroll_height_max;
			//_velocity = 0;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);

			
			initTodaySaying();

			//Debug.Log(StringColletion.get("today.steps.desc", 0));
			//Debug.Log(StringColletion.get("week.steps_desc", 0)); 
		}

		private void initTodaySaying()
		{
			List<RefString> list = StringColletion.getList("RefWelcome");

			RefString ref_string = RandomUtil.select(list);

			if( ref_string != null)
			{
				txt_message.prepare( ref_string.value);
			}

			float length = txt_name.start(0);
			txt_message.start(length);
		}

		public void onBeginDrag(BaseEventData e)
		{
			//_scroller.onBeginDrag(e);
			//PointerEventData pe = e as PointerEventData;
			//_last_pos = pe.position;
			//_velocity = 0;
			//_velocity_on_dragging = 0;
		}

		public void onDrag(BaseEventData e)
		{
			//_scroller.onDrag(e);
			//PointerEventData pe = e as PointerEventData;
			////Debug.Log($"{pe.scrollDelta},{pe.position},{pe.delta}");

			//float delta = -pe.delta.y;

			//_last_pos = pe.position;

			//float newVelocity = delta / Time.unscaledDeltaTime;
			//_velocity_on_dragging = Mathf.Lerp(_velocity_on_dragging, newVelocity, Time.unscaledDeltaTime * 10);

			//scroll(delta);
		}

		private void onScroll(float ratio)
		{
			anim_circleHole.SetFloat(_id_time, ratio);
			//_scroll_height += delta;
			//_scroll_height = Mathf.Clamp(_scroll_height, 0, _scroll_height_max);

			//float hole_anim_ratio = 1.0f - _scroll_height / _scroll_height_max;
			//hole_anim_ratio = Mathf.Clamp(hole_anim_ratio, 0, 1);

			//anim_circleHole.SetFloat(_id_time, hole_anim_ratio);
		}

		public void onEndDrag(BaseEventData e)
		{
			//_scroller.onEndDrag(e);
			//PointerEventData pe = e as PointerEventData;

			//_velocity = _velocity_on_dragging;
		}

		public override void update()
		{
			base.update();

			//foreach (KeyValuePair<ClientStepReward,UIMain_StepReward> item in _rewardItemMap)
			//{
			//	item.Value.update();
			//}

			if( gameObject.activeSelf && _dailyBoostTimer.update())
			{
				updateDailyBoost(null);
			}
			
			//if(gameObject.activeSelf && _walkLevelTimer.update())
			//{
			//	processWalkLevelExpire();
			//}

			txt_name.update();
			txt_message.update();
			txt_todayTalkCount.update();
			todayStepGauge.update();
			txt_FestaCoin.update();

			//_scroller.update();

			updateTreePresentationEvent();

			//if (_velocity != 0)
			//{
			//	_velocity *= Mathf.Pow(0.05f, Time.unscaledDeltaTime);

			//	if (Mathf.Abs(_velocity) < 1)
			//	{
			//		_velocity = 0;
			//	}

			//	scroll(_velocity * Time.unscaledDeltaTime);
			//}
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			ProfileViewModel profile_vm = ClientMain.instance.getViewModel().Profile;
			HealthViewModel health_vm = ClientMain.instance.getViewModel().Health;
			WalletViewModel wallet_vm = ClientMain.instance.getViewModel().Wallet;
			TripViewModel trip_vm = ClientMain.instance.getViewModel().Trip;
			//StepRewardViewModel step_vm = ClientMain.instance.getViewModel().StepReward;
			StatureViewModel stature_vm = ClientMain.instance.getViewModel().Stature;
			WeatherViewModel weather_vm = ClientMain.instance.getViewModel().Weather;
			TreeViewModel tree_vm = ClientMain.instance.getViewModel().Tree;
            LocationViewModel location_vm = ClientMain.instance.getViewModel().Location;

            _bindingManager.makeBinding(health_vm, nameof(health_vm.TodayStepCount), updateTodayWalkCount);
            _bindingManager.makeBinding(wallet_vm, nameof(wallet_vm.FestaCoin), (value) =>
            {
                txt_FestaCoin.setValue(wallet_vm.FestaCoin, false);
            });
            _bindingManager.makeBinding(stature_vm, nameof(stature_vm.Star),
                                            txt_Star, nameof(txt_Star.text), UIUtil.convertCurrencyString);

            //_bindingManager.makeBinding(step_vm.StepRewardList, updateStepRewardList);
            _bindingManager.makeBinding(trip_vm, nameof(trip_vm.Data), updateTripConfig);
			//_bindingManager.makeBinding(step_vm, nameof(step_vm.DailyBoost), updateDailyBoost);
			//_bindingManager.makeBinding(step_vm.WalkLevelDic, updateWalkLevelDic);
			//_bindingManager.makeBinding(step_vm, nameof(step_vm.CurrentWalkLevel), updateCurrentWalkLevel);
			_bindingManager.makeBinding(profile_vm, nameof(profile_vm.Profile), updateProfile);
			_bindingManager.makeBinding(weather_vm, nameof(weather_vm.Data), updateWeather);
            //_bindingManager.makeBinding(tree_vm, nameof(tree_vm.PresentationEvents), onTreePresentationEvents);

            //_bindingManager.makeBinding(location_vm, nameof(location_vm.CurrentAddress), txt_address, nameof(txt_address.text), null);
            //initStepRewardList();
            //initWalkLevelList();
        }

		public void onStatistics()
		{
			//UIBackNavigation.getInstance().setup(this, UIStatistics.getInstance());
			//UIBackNavigation.getInstance().open();
			UIStatistics.getInstance().open();

            UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIStatistics.getInstance());
            stack.addPrev(UIMainTab.getInstance());
        }

        //private void removeAllStepRewardList()
        //{
        //	foreach(KeyValuePair<ClientStepReward,UIMain_StepReward> item in _rewardItemMap)
        //	{
        //		GameObjectCache.getInstance().delete(item.Value);
        //	}
        //	_rewardItemMap.Clear();
        //}

        //private void initStepRewardList()
        //{
        //	removeAllStepRewardList();

        //	ObservableList<ClientStepReward> list = ClientMain.instance.getViewModel().StepReward.StepRewardList;
        //	for(int i = 0; i < list.size(); ++i)
        //	{
        //		createStepReward(list.get(i));
        //	}
        //}

        private void updateProfile(object obj)
		{
			ProfileViewModel vm = ViewModel.Profile;

			txt_name.prepare(vm.Profile.name);

			//txt_name.text = vm.Profile.name;
			//txt_message.text = vm.Profile.message;

			image_photo.setImageFromCDN(vm.Profile.getPicktureURL(GlobalConfig.fileserver_url));
		}

		private void updateWeather(object obj)
		{
			WeatherViewModel vm = ViewModel.Weather;
			if( vm.Data != null)
			{
				txt_weather.text = vm.Data.getDisplayName( ViewModel.Profile.Setting_TemperatureUnit);
			}
		}

		private void updateTodayWalkCount(object obj)
		{
			HealthViewModel vm = ViewModel.Health;

			// 값이 작아 질 경우에는 animation처리를 하지 않음
			bool isInit = txt_todayTalkCount.getTargetValue() > vm.TodayStepCount;
			txt_todayTalkCount.setValue(vm.TodayStepCount,isInit);
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				txt_name.stop();
				txt_message.stop();

                // animator가 리셋된다
                //onScroll(_scroller.getNormalizedScrollPos());

                World.Instance?.gameObject.SetActive(true);
                Camera.main.clearFlags = CameraClearFlags.Skybox;

				// 2022.05.27 이강희
				anim_circleHole.SetFloat(_id_time, 0);
			}
			else if( type == TransitionEventType.end_open)
			{
				float length = txt_name.start(0);
				txt_message.start(length);

				txt_todayTalkCount.start();
				txt_FestaCoin.start();

            }
			else if( type == TransitionEventType.start_close)
			{
				txt_todayTalkCount.stop();
				txt_FestaCoin.stop();
			}
			else if ( type == TransitionEventType.end_close)
            {
				// uimain이 다 닫히면, 끄자.
                World.Instance?.gameObject.SetActive(false);
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
            }
		}

		//private void updateStepRewardList(int event_type, object obj)
		//{
		//	if(event_type == CollectionEventType.add)
		//	{
		//		ClientStepReward reward = (ClientStepReward)obj;
		//		createStepReward(reward);
		//	}
		//	else if( event_type == CollectionEventType.remove)
		//	{
		//	}
		//	else if( event_type == CollectionEventType.clear)
		//	{
		//		removeAllStepRewardList();
		//	}
		//	else if( event_type == CollectionEventType.sort)
		//	{
		//		initStepRewardList();
		//	}
		//}

		//private void updateWalkLevelDic(int event_type,object obj)
		//{
		//	if( event_type == CollectionEventType.add || event_type == CollectionEventType.update)
		//	{
		//		KeyValuePair<int, ClientWalkLevel> item = (KeyValuePair<int, ClientWalkLevel>)obj;

		//		_walkLevelItems[item.Value.level].updateData(item.Value);
		//	}
		//}

		//private void updateCurrentWalkLevel(object obj)
		//{
		//	foreach(KeyValuePair<int,UIWalkLevelItem> item in _walkLevelItems)
		//	{
		//		item.Value.updateUI();
		//	}
		//}

		private void updateTripConfig(object obj)
		{
			ClientTripConfig trip_config = (ClientTripConfig)obj;
			//goTripBoost.SetActive(trip_config.status == ClientTripConfig.StatusType.none);
		}

		//private UIMain_StepReward createStepReward(ClientStepReward step_reward)
		//{
		//	Vector2 start_pos = getStepRewardStartPos();
		//	Vector2 target_pos = generateRandomStepRewardPosition();

		//	UIMain_StepReward new_one = stepRewardSource.make<UIMain_StepReward>(rtStepRewardArea, GameObjectCacheType.ui);
		//	new_one.setup(step_reward, start_pos, target_pos);

		//	_rewardItemMap.Add(step_reward, new_one);
		//	return new_one;
		//}

		//private Vector2 getStepRewardStartPos()
		//{
		//	return rtStepRewardArea.transform.InverseTransformPoint(stepRewardStartPosition.position);
		//}

		//public Vector2 getStepRewardClaimPos()
		//{
		//	return rtStepRewardArea.transform.InverseTransformPoint(stepRewardClaimPosition.position);
		//}

		//private Vector2 generateRandomStepRewardPosition()
		//{
		//	Vector3[] corners = new Vector3[4];
		//	rtStepRewardArea.GetLocalCorners(corners);

		//	//Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;

		//	//float x = dir.x * (corners[2].x - corners[0].x) / 2.0f;
		//	//float y = dir.y * (corners[2].y - corners[0].y) / 2.0f;

		//	float x = UnityEngine.Random.Range(corners[0].x, corners[2].x);
		//	float y = UnityEngine.Random.Range(corners[0].y, corners[2].y);

		//	return new Vector2(x, y);
		//}

		//public void onClickStepReward(UIMain_StepReward reward)
		//{
		//	int slot_id = reward.getData().slot_id;

		//	ClientViewModel vm = ClientMain.instance.getViewModel();

		//	MapPacket req = ClientMain.instance.getNetwork().createReq(CSMessageID.HealthData.ClaimStepRewardReq);
		//	req.put("slot_id", slot_id);

		//	//Debug.Log("req claim");

		//	ClientMain.instance.getNetwork().call(req, ack => {
		//		//Debug.Log("ack claim");

		//		if( ack.getResult() == ResultCode.ok)
		//		{
		//			// 성공하면 없애 준다
		//			vm.StepReward.StepRewardList.remove(reward.getData());
		//			vm.updateFromPacket(ack);

		//			reward.onClaimed(getStepRewardClaimPos());
		//		}
		//	});
		//}

		//public void onStepRewardClaimComplete(UIMain_StepReward reward)
		//{
		//	_rewardItemMap.Remove(reward.getData());
		//	GameObjectCache.getInstance().delete(reward);
		//}

		//private void initWalkLevelList()
		//{
		//	foreach(KeyValuePair<int,UIWalkLevelItem> item in _walkLevelItems)
		//	{
		//		item.Value.delete();
		//	}

		//	StepRewardViewModel step_vm = ClientMain.instance.getViewModel().StepReward;

		//	Dictionary<int, RefData.RefData> ref_dic = GlobalRefDataContainer.getInstance().getMap<RefWalkLevel>();
		//	foreach (KeyValuePair<int,RefData.RefData> item in ref_dic)
		//	{
		//		RefWalkLevel ref_walk_level = item.Value as RefWalkLevel;

		//		UIWalkLevelItem new_one = walkLevelItemSource.make<UIWalkLevelItem>(rtWalkLevelRoot, GameObjectCacheType.ui);
		//		new_one.setup(ref_walk_level, step_vm.WalkLevelDic.get( ref_walk_level.level));

		//		_walkLevelItems.Add(ref_walk_level.level, new_one);
		//	}

		//}

		public void onClickDailyBoost()
		{
			if ( scrollViewRoot.enabled == false )
            {
				return;
            }

			//UIBackNavigation.getInstance().setup(this, UIDailyBoost.getInstance());
			//UIBackNavigation.getInstance().open();

			UIDailyBoost.getInstance().open();
			ClientMain.instance.getPanelNavigationStack().push(this, UIDailyBoost.getInstance());
		}

		private void updateDailyBoost(object obj)
		{
			RefConfig config_cool_time = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.cool_time);
			RefConfig config_daily_count = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.daily_count);

			ClientDailyBoost boost_config = ClientMain.instance.getViewModel().DailyBoost.DailyBoost;

			if( boost_config.status == ClientDailyBoost.StatusType.on)
			{
				txt_BoostCoolTime.gameObject.SetActive(false);
			}
			else if( boost_config.status == ClientDailyBoost.StatusType.off)
			{
				TimeSpan remain_time;
				if( UIDailyBoost.canTurnOnBoost(config_cool_time, config_daily_count, boost_config, out remain_time))
				{
					txt_BoostCoolTime.gameObject.SetActive(false);
				}
				else
				{
					txt_BoostCoolTime.gameObject.SetActive(true);
					txt_BoostCoolTime.text = UIFormatter.timePeroid(remain_time);
				}
			}
		}

		//public void processWalkLevelExpire()
		//{
		//	_walkLevelExpireProcessor.run(() => {
		//		_walkLevelTimer.setNext();
		//	});
		//}

		public void onClickProfileIcon()
		{
			//UIBackNavigation.getInstance().setup(this, UIProfile.getInstance());
			//UIBackNavigation.getInstance().open();

			UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.accountID = ClientMain.instance.getNetwork().getAccountID();
			
			UIProfile.getInstance().open(param);

			ClientMain.instance.getPanelNavigationStack().push(this, UIProfile.getInstance());
		}

		//public void test()
		//{
		//	UMBOfflineRenderer.getInstance().test();
		//}

		//public void onTreePresentationEvents(object obj)
		//{
		//	// 연출은 나중에 보여준다
		//	if( gameObject.activeSelf == false)
		//	{
		//		return;
		//	}

		//	List<TreePresentationEvent> event_list = new List<TreePresentationEvent>();
		//	ViewModel.Tree.popPresentationEvents(event_list);

		//	Debug.Log($"pop tree presentations : [{event_list.Count}]");

		//	if( event_list.Count > 0)
		//	{
		//		// 연출을 보여줌
		//		StartCoroutine(runTreePresentation(event_list));
		//	}
		//}

		private void updateTreePresentationEvent()
		{
			return;
			//if (gameObject.activeSelf == false)
			//{
			//	return;
			//}

			//if (_treePresentaionEventTimer.update() == false)
			//	return;

			//if (_isRunningTreePresentation)
			//	return;

			//_treePresentationEventList.Clear();
			//ViewModel.Tree.popPresentationEvents(_treePresentationEventList);

			//StartCoroutine(runTreePresentation(_treePresentationEventList));
		}

		IEnumerator runTreePresentation(List<TreePresentationEvent> event_list)
		{
			//_isRunningTreePresentation = true;
			foreach (TreePresentationEvent e in event_list)
			{
				if( e.getType() == TreePresentationEvent.EventType.init_tree)
				{
					updateTreeInfo(e);
				}
				else if( e.getType() == TreePresentationEvent.EventType.change_tree)
				{
					updateTreeInfo(e);
				}
				else if( e.getType() == TreePresentationEvent.EventType.grow_up)
				{
					updateTreeInfo(e);
				}
				else if( e.getType() == TreePresentationEvent.EventType.harvest)
				{
					updateTreeInfo(e);
				}
				else if( e.getType() == TreePresentationEvent.EventType.wither)
				{
					updateTreeInfo(e);
				}

				Debug.Log($"runTreePresentation:type[{e.getType()}]");

				yield return new WaitForEndOfFrame();
			}

			//_isRunningTreePresentation = false;
		}

		private void updateTreeInfo(TreePresentationEvent e)
		{
			string tree_name = StringColletion.get("Tree.Name", e.getTreeID());

			int harvestable_coin = ViewModel.Tree.calcHarvestableCoinAmount(e.getStepCount(), e.getRefTree());

			string info = $"tree_id[{e.getTreeID()}] tree_name[{tree_name}] step_count[{e.getStepCount()}] isWithered[{e.isWithered()}]\ngrow_ratio[{e.getGrowRatio()}] product_coin[{harvestable_coin}]\nlast event[{e.getType()}]";

			txt_tree_debug_info.text = info;
		}

		public void onClickTreeHarvest()
		{
			// 수확할게 없다
			if( ViewModel.Tree.calcHarvestableCoinAmount_Current() <= 0)
			{
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Tree.ClaimTreeRewardReq);
			Network.call(req, ack => { 
				
				if( ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
				}
			
			});
		}

        public void onClickShop()
        {
            UIShop.getInstance().open();

			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIShop.getInstance());
            stack.addPrev(UIMainTab.getInstance());
        }

		public void onClickMore()
        {
			anim_circleHole.SetTrigger("more");

            var rect = scrollViewRoot.GetComponent<RectTransform>();
			DOTween.To(() => rect.anchoredPosition, x => rect.anchoredPosition = x, new Vector2(rect.anchoredPosition.x, 713.0f), 0.3f);
            // scrollView가 올라와 있을 때, 스와이프로 내리기 위하여..
            scrollViewRoot.onValueChanged.AddListener(OnScrollViewValueChanged);
        }

		public void onClickLess()
        {
            anim_circleHole.SetTrigger("less");
			scrollViewRoot.onValueChanged.RemoveAllListeners();
			_scrollViewStartDragPosition = Vector2.zero;

            var rect = scrollViewRoot.GetComponent<RectTransform>();
            DOTween.To(() => rect.anchoredPosition, x => rect.anchoredPosition = x, new Vector2(rect.anchoredPosition.x, 226.0f), 0.3f);
            //go_scrollView.localPosition = new Vector3(go_scrollView.localPosition.x, 0.0f, 0.0f);

        }
        public void OnScrollViewValueChanged(Vector2 pos)
        {
            //float th = 0.4f;
            if (pos.y > 1.0f + 0.4f && _reserveScrollDown == false)
            {
                _reserveScrollDown = true;
            }
        }

		private Vector2 _scrollViewStartDragPosition;
        public void OnScrollViewBeginDrag(BaseEventData e)
        {
			// scrollViewRoot의 enabled는 애니메이션이 끝난뒤에 false로 변한다.
			if ( scrollViewRoot.enabled == false )
            {
                PointerEventData eventData = (PointerEventData)e;
                _scrollViewStartDragPosition = eventData.position;
            }
		}

        public void OnScrollViewEndDrag(BaseEventData e)
        {
            if (_reserveScrollDown)
            {
                _reserveScrollDown = false;
				Debug.Log(go_scrollView.localPosition);
                onClickLess();
            }
			else if (_scrollViewStartDragPosition != Vector2.zero )
            {
                PointerEventData eventData = (PointerEventData)e;
				var diff = eventData.position - _scrollViewStartDragPosition;
				if ( diff.y > 20.0f )
                {
					onClickMore();
                }
            }
        }

        public RectTransform getCoinTransform()
        {
			return img_coin;
        }

		// 코인 이미지 애님 연출!
		private Tweener _coinTweener = null;
		public void popCoinEffect()
        {
			if (_coinTweener == null )
            {
                _coinTweener = img_coin.DOPunchScale(new Vector3(2, 2), 1.0f, 10, 0).SetAutoKill(false).SetRecyclable(true);
            }
			
			if ( _coinTweener.IsPlaying() == false )
            {
				_coinTweener.Restart();
			}
		}
	}
}
