using Assets.Script_DRun.UI.Panel.Marathon;
using DRun.Client.BodyProfileSelectData;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace DRun.Client
{
    [RequireComponent(typeof(SlideUpDownTransition))]
    public class UISelectMarathonGoal : UISingletonPanel<UISelectMarathonGoal>
    {
        public Animator anim_selectType_arrow;
		
        public TMP_Text text_desc;
        public TMP_Text text_type_name;
        //public TMP_Text text_goal_value;
        //public TMP_Text text_goal_unit;
        public TMP_Text text_latest;

        [Header("Goal")]
        public TMP_Text text_goal_distance;
        public TMP_Text text_goal_time_hour;
        public TMP_Text text_goal_time_minute;
        public GameObject[] distanceGoalGroup;
        public GameObject[] timeGoalGroup;

        [Header("Freepace")]
        public GameObject freepace_root;
        public UIColorToggleButton[] freepace_tabButtons;
        public UITabSlide freepace_tabSlide;
        public GameObject[] freepace_sub_pages;
        public UISnapPicker distance_high;
        public UISnapPicker distance_low;
        public UISnapPicker time_high;
        public UISnapPicker time_low;

        private int _marathonType;
        private int _freepaceType;
        private List<UISnapPickerData> _dataDistanceHigh;
        private List<UISnapPickerData> _dataDistanceLow;
        private List<UISnapPickerData> _dataTimeHigh;
        private List<UISnapPickerData> _dataTimeLow;

        private bool _isDistanceScrollerActive;
        private bool _isTimeScrollerActive;

        private RectTransform _selfRectTransform;
		
        private static readonly int Open = Animator.StringToHash("open");
        private static readonly int Close = Animator.StringToHash("close");
        private RectTransform _freepaceRootRectTransform;


        public class MarathonType
        {
            public static int _5k = ClientRunningLogCumulation.MarathonType._5k;
            public static int _10k = ClientRunningLogCumulation.MarathonType._10k;
            public static int _20k = ClientRunningLogCumulation.MarathonType._20k;
            public static int _40k = ClientRunningLogCumulation.MarathonType._40k;
            public static int _free = ClientRunningLogCumulation.MarathonType._free_distance;
        }

        public class FreepaceType
        {
            public static int distance = 1;
            public static int time = 2;
        }

        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            distance_high.init();
            distance_low.init();
            time_high.init();
            time_low.init();

            buildPickerData();

            _isDistanceScrollerActive = false;
            _isTimeScrollerActive = false;

            _marathonType = -1;
            _freepaceType = -1;
            
            _selfRectTransform = GetComponent<RectTransform>();
            _freepaceRootRectTransform = freepace_root.GetComponent<RectTransform>();
            freepace_root.gameObject.SetActive(false);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);
            
            _selfRectTransform.anchoredPosition = Vector2.zero;
        }

        public override void onTransitionEvent(int type)
        {
            if (type == TransitionEventType.start_open)
            {
                setupFromLatest();
            }
            //else if( type == TransitionEventType.end_open)
            //{
            //}
        }

        private void setupFromLatest()
        {
            JsonObject latest = readLatest();
            int latestType = latest.getInteger("type");
            int goal = latest.getInteger("goal");

            if( latestType == ClientRunningLogCumulation.MarathonType._free_distance)
            {
                setType(MarathonType._free);
                setFreepaceType(FreepaceType.distance, true);
                setGoalToSnapPicker(FreepaceType.distance, goal, false);
                updateFreepaceGoalValue();

                text_latest.text = StringCollection.getFormat("marathon.select_goal.latest", 0, StringCollection.get("marathon.select_goal.option", MarathonType._free - 1));
            }
            else if( latestType == ClientRunningLogCumulation.MarathonType._free_time)
            {
                setType(MarathonType._free);
                setFreepaceType(FreepaceType.time, true);
                setGoalToSnapPicker(FreepaceType.time, goal, false);
                updateFreepaceGoalValue();

                text_latest.text = StringCollection.getFormat("marathon.select_goal.latest", 0, StringCollection.get("marathon.select_goal.option", MarathonType._free - 1));
            }
            else
            {
                setType(latestType);
                text_latest.text = StringCollection.getFormat("marathon.select_goal.latest", 0, StringCollection.get("marathon.select_goal.option", latestType - 1));
            }
        }

        private void setType(int type)
        {
            if( _marathonType == type)
            {
                return;
            }

            _marathonType = type;

            if( _marathonType == MarathonType._free)
            {
                freepace_root.gameObject.SetActive(true); 

                _freepaceRootRectTransform.moveInDirectionWithCallback(
	                delta: (from: 0, to: _freepaceRootRectTransform.sizeDelta.y),
	                whichDirection: WhichDirection.Vertical,
                    onBefore: () =>
	            	    {
							_freepaceRootRectTransform.anchorMax = new(0.5f, 0);
                	        _freepaceRootRectTransform.anchorMin = new(0.5f, 0);
	
                	        // 시작전에 이동 화면 아래로 이동시켜놓기. -> 화면 안으로 올라오는 트랜지션이기 때문에.
                	        _freepaceRootRectTransform.anchoredPosition = new(0, -_freepaceRootRectTransform.sizeDelta.y);
	            	    }
				);

                //if(_freepaceType == -1)
                //{
                //	setFreepaceType(FreepaceType.distance, false);
                //}

                text_desc.text = StringCollection.get("marathon.select_goal.freerace_warning", 0);
                text_desc.color = UIStyleDefine.ColorStyle.error;

                //updateFreepaceGoalValue();
            }
            else
            {
				_freepaceRootRectTransform.moveInDirectionWithCallback(
					delta: (from: _freepaceRootRectTransform.sizeDelta.y, to: 0),
					whichDirection: WhichDirection.Vertical,
					onComplete: () =>
					{
                	    freepace_root.gameObject.SetActive(false);
                	    _freepaceRootRectTransform.anchorMax = new(0.5f, 1);
                	    _freepaceRootRectTransform.anchorMin = new(0.5f, 1);
					}
				);

                //UIPanelTransitionHelper.moveInDirectionWithCallback(
                //    delta: (
                //        from: _freepaceRootRectTransform.sizeDelta.y,
                //        to: 0
                //    ),
                //    targetRectTransform: _freepaceRootRectTransform,
                //    whichDirection: WhichDirection.Vertical,
                //    onComplete: () =>
                //    {
                //        freepace_root.gameObject.SetActive(false);

                //        _freepaceRootRectTransform.anchorMax = new(0.5f, 1);
                //        _freepaceRootRectTransform.anchorMin = new(0.5f, 1);
                //    }
                //);

                int goal_meter = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.marathon_goals[_marathonType - 1], 5000);
                double goal_km = (double)goal_meter / 1000.0;
                text_goal_distance.text = goal_km.ToString();

                text_desc.text = StringCollection.get("marathon.select_goal.desc", 0);
                text_desc.color = UIStyleDefine.ColorStyle.gray400;
            }

            text_type_name.text = StringCollection.get("marathon.select_goal.option", type - 1);
            updateGoalValueUnit();
        }

        private void setFreepaceType(int type,bool updateNow)
        {
            if( _freepaceType == type)
            {
                return;
            }

            _freepaceType = type;
            for(int i = 0; i < freepace_sub_pages.Length; ++i)
            {
                freepace_sub_pages[i].gameObject.SetActive((i + 1) == type);
                freepace_tabButtons[i].setStatus((i + 1) == type);
            }

            freepace_tabSlide.setTab(type - 1, updateNow);

            //if ( type == FreepaceType.distance)
            //{
            //	distance_high.DataList = _dataDistanceHigh;
            //	distance_low.DataList = _dataDistanceLow;

            //	int configDefaultDistance = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.goal_distance_default, 5);

            //	distance_high.jumpByValue(configDefaultDistance, false);
            //	distance_low.jumpByValue(0, false);
            //}
            //else if( type == FreepaceType.time)
            //{
            //	time_high.DataList = _dataTimeHigh;
            //	time_low.DataList = _dataTimeLow;

            //	int configDefaultTime = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.goal_time_default, 30);

            //	int defaultHigh = configDefaultTime / 60;
            //	int defaultLow = configDefaultTime % 60;

            //	time_high.jumpByValue(defaultHigh, false);
            //	time_low.jumpByValue(defaultLow, false);
            //}

            updateGoalValueUnit();
        }

        private List<UISnapPickerData> makeInteger(int begin, int end, HorizontalAlignmentOptions align)
        {
            List<UISnapPickerData> list = new List<UISnapPickerData>();
            list.Add(BodyProfilePickerData.createEmpty());
            list.Add(BodyProfilePickerData.createEmpty());

            for (int i = begin; i <= end; ++i)
            {
                list.Add(BodyProfilePickerData.createNumber(i, align));
            }

            list.Add(BodyProfilePickerData.createEmpty());
            list.Add(BodyProfilePickerData.createEmpty());

            return list;
        }

        private void buildPickerData()
        {
            _dataDistanceHigh = makeInteger(1, 42, HorizontalAlignmentOptions.Right);
            _dataDistanceLow = makeInteger(0, 99, HorizontalAlignmentOptions.Left);
            _dataTimeHigh = makeInteger(0, 6, HorizontalAlignmentOptions.Right);
            _dataTimeLow = makeInteger(0, 59, HorizontalAlignmentOptions.Left);
        }

        public void onClick_SelectType()
        {
            anim_selectType_arrow.SetTrigger(Open);
            UISelectMarathonType.getInstance().open(_marathonType, type => {
                anim_selectType_arrow.SetTrigger(Close);
				
                setType(type);
                if( type == MarathonType._free)
                {
                    setFreepaceType(FreepaceType.distance, true);
                    setFreepaceDefaultGoal(FreepaceType.distance, true);
                    updateFreepaceGoalValue();
                }
            });
        }

        public void onClick_Close()
        {
	        _selfRectTransform.moveInDirectionWithCallback(
		        delta: (from: 0, to: -Screen.height * 0.5f),
		        whichDirection: WhichDirection.Vertical,
                onProgress:  progress =>
                {
					if (progress >= 0.02f) 
						UIMainTab.getInstance().open();
                }
			);
        }

        public void onClick_Start()
        {
            int type = _marathonType;
            int goal = 0;

            if (_marathonType == MarathonType._free)
            {
                if( _freepaceType == FreepaceType.distance)
                {
                    type = ClientRunningLogCumulation.MarathonType._free_distance;
                    goal = getGoalFromSnapPicker(_freepaceType);
                }
                else
                {
                    type = ClientRunningLogCumulation.MarathonType._free_time;
                    goal = getGoalFromSnapPicker(_freepaceType);
                }

                if( goal == 0)
                {
                    return;
                }
            }
            else
            {
                goal = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.marathon_goals[_marathonType - 1], 5000);
            }

            saveLatest(type, goal);

            ClientMain.instance.getRunningRecorder().startRecordingMarathonMode(type, goal);
            UIRunningCountdown.getInstance().open();
        }

        public void onClick_FreepaceDistance()
        {
            setFreepaceType(FreepaceType.distance, false);
            setFreepaceDefaultGoal(FreepaceType.distance, false);
            updateFreepaceGoalValue();
        }

        public void onClick_FreepaceTime()
        {
            setFreepaceType(FreepaceType.time, false);
            setFreepaceDefaultGoal(FreepaceType.time, false);
            updateFreepaceGoalValue();
        }

        public void onSnapChanged(int snapIndex)
        {
            if( _marathonType != MarathonType._free)
            {
                return;
            }

            if( _freepaceType == FreepaceType.time)
            {
                int high = time_high.getCurrentValue();
                int low = time_low.getCurrentValue();

                if( high == 0 && low == 0)
                {
                    time_high.jumpByValue(1,false);
                }
            }

            updateFreepaceGoalValue();
        }

        private void updateFreepaceGoalValue()
        {
            if (_freepaceType == FreepaceType.distance)
            {
                int high = distance_high.getCurrentValue();
                int low = distance_low.getCurrentValue();

                if (high != -1 && low != -1)
                {
                    text_goal_distance.text = $"{high.ToString("D2")}.{low.ToString("D2")}";
                }

            }
            else if (_freepaceType == FreepaceType.time)
            {
                int high = time_high.getCurrentValue();
                int low = time_low.getCurrentValue();

                if (high != -1 && low != -1)
                {
                    text_goal_time_hour.text = high.ToString("D2");
                    text_goal_time_minute.text = low.ToString("D2");
                }
            }
        }

        private void updateGoalValueUnit()
        {
            if( _marathonType == MarathonType._free && _freepaceType == FreepaceType.time)
            {
                setActiveList(distanceGoalGroup, false);
                setActiveList(timeGoalGroup, true);
            }
            else
            {
                setActiveList(distanceGoalGroup, true);
                setActiveList(timeGoalGroup, false);
            }
        }

        private void setActiveList(GameObject[] list,bool active)
        {
            foreach (GameObject go in list)
            {
                go.SetActive(active);
            }
        }

        private void saveLatest(int type,int goal)
        {
            JsonObject json = new JsonObject();
            json.put("type", type);
            json.put("goal", goal);

            PlayerPrefs.SetString("marathon.latest", json.encode());
            PlayerPrefs.Save();
        }

        private JsonObject makeDefaultLatest()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.put("type", MarathonType._5k);
            jsonObject.put("goal", 0);
            return jsonObject;
        }

        private JsonObject readLatest()
        {
            if( PlayerPrefs.HasKey("marathon.latest") == false)
            {
                return makeDefaultLatest();
            }

            try
            {
                JsonObject jsonObject = new JsonObject(PlayerPrefs.GetString("marathon.latest"));
                return jsonObject;
            }
            catch(System.Exception e)
            {
                Debug.Log(e);
                return makeDefaultLatest();
            }
        }

        private void setFreepaceDefaultGoal(int freepaceType,bool updateNow)
        {
            if( freepaceType == FreepaceType.distance)
            {
                setGoalToSnapPicker(freepaceType, GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.goal_distance_default, 5) * 1000, updateNow);
            }
            else
            {
                setGoalToSnapPicker(freepaceType, GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.goal_time_default, 30), updateNow);
            }
        }

        private void setGoalToSnapPicker(int freepaceType,int goal,bool updateNow)
        {
            if (freepaceType == FreepaceType.distance)
            {
                setDistanceScrollerData();

                int high = goal / 1000;
                int low = (goal % 1000) / 10;

                distance_high.jumpByValue(high, updateNow);
                distance_low.jumpByValue(low, updateNow);
            }
            else
            {
                setTimeScrollerData();

                int high = goal / 60;
                int low = goal % 60;

                time_high.jumpByValue(high, true);
                time_low.jumpByValue(low, true);
            }
        }

        private void setDistanceScrollerData()
        {
            if(_isDistanceScrollerActive)
            {
                return;
            }

            if( distance_high.scroller.Container == null)
            {
                return;
            }

            distance_high.DataList = _dataDistanceHigh;
            distance_low.DataList = _dataDistanceLow;
            _isDistanceScrollerActive = true;
        }

        private void setTimeScrollerData()
        {
            if(_isTimeScrollerActive)
            {
                return;
            }

            if( time_high.scroller.Container == null)
            {
                return;
            }

            time_high.DataList = _dataTimeHigh;
            time_low.DataList = _dataTimeLow;
            _isTimeScrollerActive = true;
        }

        private int getGoalFromSnapPicker(int freepaceType)
        {
            if( freepaceType == FreepaceType.distance)
            {
                int high = distance_high.getCurrentValue();
                int low = distance_low.getCurrentValue();

                return high * 1000 + low * 10;
            }
            else
            {
                int high = time_high.getCurrentValue();
                int low = time_low.getCurrentValue();

                return high * 60 + low;
            }
        }
    }
}