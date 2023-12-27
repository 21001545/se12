using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.Record;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;

using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DRun.Client
{
	public class UIRecordCell_Statistics : EnhancedScrollerCellView
	{
		public float height;
		public UIAnimationToggleButton[] tabButton;

		[Header("============== Average =========== ")]
		public TMP_Text avg_period_time;
		public TMP_Text avg_distance;
		public TMP_Text avg_log_count;
		public TMP_Text avg_velocity;
		public TMP_Text avg_time;

		[Header("============== Graph ============= ")]
		public TMP_Text[] y_axis_value;
		public RectTransform grid_v_line_root;
		public BareReusableMonoBehaviour grid_v_line_source;
		public RectTransform x_axis_root;
		public UIRecordGraph_XAxisItem x_axis_value_source;
		public RectTransform value_root;
		public UIRecordGraph_ValueItem value_source;
		public Color colorGraphOn;
		public Color colorGraphGray;

		[Header("============== Graph Tip ========== ")]
		public RectTransform tip_root;
		public UILine tip_line;
		public TMP_Text tip_value;
		public TMP_Text tip_time;

		public class TabType
		{
			public const int week = 0;
			public const int month = 1;
			public const int year = 2;
			public const int total = 3;
		}

		private int _currentTab = -1;
		private List<BareReusableMonoBehaviour> _vLineList;
		private List<UIRecordGraph_XAxisItem> _xAxisValueList;
		private List<UIRecordGraph_ValueItem> _valueList;

		private bool _init = false;
		private Dictionary<int, GraphData> _graphDataMap;

		//private bool _graphPointerDown = false;
		private UIRecordGraph_ValueItem _lastPickItem;
		private Camera _camera;

		private static int[] timeTypeFromTab = new int[]
		{
			ClientRunningLogCumulation.TimeType.week,
			ClientRunningLogCumulation.TimeType.month,
			ClientRunningLogCumulation.TimeType.year,
			ClientRunningLogCumulation.TimeType.total,
		};

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private void init()
		{
			if(_init)
			{
				return;
			}

			grid_v_line_source.gameObject.SetActive(false);
			x_axis_value_source.gameObject.SetActive(false);
			value_source.gameObject.SetActive(false);

			_vLineList = new List<BareReusableMonoBehaviour>();
			_xAxisValueList = new List<UIRecordGraph_XAxisItem>();
			_valueList = new List<UIRecordGraph_ValueItem>();
			_init = true;

			_graphDataMap = new Dictionary<int, GraphData>();
			_camera = Camera.main;
		}

		public void onClickTab(int tab)
		{
			if (_currentTab == tab)
			{
				return;
			}
			setTab(tab);
		}

		public void setTab(int tab)
		{
			_currentTab = tab;
			for(int i = 0; i < tabButton.Length; ++i)
			{
				tabButton[i].setStatus(i == tab);
			}

			setupGraphData(timeTypeFromTab[tab]);
			setupAverageData(timeTypeFromTab[tab]);
		}

		public void setup(Dictionary<int,GraphData> graphDataMap)
		{
			init();

			_graphDataMap = graphDataMap;
			setTab(TabType.week);
		}

		#region average
		private void setupAverageData(int timeType)
		{
			ClientRunningLogCumulation log = ViewModel.Record.getCurrentLogCumulation( UIRecord.getInstance().getCurrentPageRunningType(), timeType);

			avg_distance.text = StringUtil.toDistanceString(log.getAvgDistance());
			avg_log_count.text = log.log_count.ToString("N0");
			avg_velocity.text = log.getAvgVelocity().ToString("N1");
			avg_time.text = StringUtil.toRunningTimeString(log.getAvgTime());

			DateTime beginTime = log.begin_time.ToLocalTime();
			DateTime endTime = log.end_time.ToLocalTime();

			if( timeType == ClientRunningLogCumulation.TimeType.week)
			{
				avg_period_time.text = $"{beginTime.Year}/{beginTime.Month}/{beginTime.Day} - {endTime.Day}";
			}
			else if( timeType == ClientRunningLogCumulation.TimeType.month)
			{
				avg_period_time.text = $"{beginTime.Year}/{beginTime.Month}";
			}
			else if( timeType == ClientRunningLogCumulation.TimeType.year)
			{
				avg_period_time.text = $"{beginTime.Year}";
			}
			else if( timeType == ClientRunningLogCumulation.TimeType.total)
			{
				avg_period_time.text = "";
			}

		//public TMP_Text avg_period_time;
		}

		#endregion

		#region graph

		private void clearGraph()
		{
			GameObjectCache.getInstance().delete(_vLineList);
			GameObjectCache.getInstance().delete(_xAxisValueList);
			GameObjectCache.getInstance().delete(_valueList);

			//_graphPointerDown = false;
			_lastPickItem = null;
			tip_root.gameObject.SetActive(false);
		}

		private void setupGraphData(int timeType)
		{
			GraphData graphData = null;
			_graphDataMap.TryGetValue(timeType, out graphData);

			clearGraph();

			if( graphData == null)
			{
				return;
			}

			Rect rectVLine = grid_v_line_root.rect;
			Rect rectXAxis = x_axis_root.rect;
			Rect rectValue = value_root.rect;

			int v_line_count = graphData.xAxisList.Count + 1;
			for(int i = 0; i < v_line_count; ++i)
			{
				BareReusableMonoBehaviour vline = GameObjectCache.getInstance().make<BareReusableMonoBehaviour>(grid_v_line_source, grid_v_line_root, GameObjectCacheType.ui);
				vline.rt.anchoredPosition = new Vector2(i * rectVLine.width / (float)(v_line_count - 1), 0);

				_vLineList.Add(vline);
			}

			for (int i = 0; i < graphData.yAxisList.Count; ++i)
			{
				if(i < y_axis_value.Length)
				{
					y_axis_value[i].text = graphData.yAxisList[i];
				}
			}

			for(int i = 0; i < graphData.xAxisList.Count ; ++i)
			{
				UIRecordGraph_XAxisItem xAxis = GameObjectCache.getInstance().make<UIRecordGraph_XAxisItem>(x_axis_value_source, x_axis_root, GameObjectCacheType.ui);
				xAxis.label.text = graphData.xAxisList[i];
				xAxis.label.fontSize = graphData.xAxisLabel_FontSize;
				xAxis.label.margin = new Vector4( graphData.xAxisLabel_Margin, 0, 0, 0);
				xAxis.label.alignment = graphData.xAxisLabel_Alignment;
				xAxis.rt.anchoredPosition = new Vector2(i * rectXAxis.width / graphData.xAxisList.Count, 0);

				Vector2 size = xAxis.rt.sizeDelta;
				size.x = rectXAxis.width / graphData.xAxisList.Count;
				xAxis.rt.sizeDelta = size;

				_xAxisValueList.Add(xAxis);
			}

			float valueDelta = rectValue.width / graphData.valueList.Count;
			float zeroValueHeight = 0;

			for(int i = 0; i < graphData.valueList.Count; ++i)
			{
				GraphValue graphValue = graphData.valueList[i];
				UIRecordGraph_ValueItem value = GameObjectCache.getInstance().make<UIRecordGraph_ValueItem>(value_source, value_root, GameObjectCacheType.ui);
				value.graphValue = graphData.valueList[i];

				double valueRatio = (graphData.valueList[i].value - graphData.valueMin) / (graphData.valueMax - graphData.valueMin);
				value.rt.sizeDelta = new Vector2(graphData.valueWidth, (float)((rectValue.height - zeroValueHeight )* valueRatio) + zeroValueHeight);
				value.rt.anchoredPosition = new Vector2( ((float)i + 0.5f) * valueDelta, 0);

				value.image.color = graphValue.empty ? colorGraphGray : colorGraphOn;

				_valueList.Add(value);
			}
		}

		#endregion

		#region ToolTip
		public void onGraphPointer_Down(BaseEventData e)
		{
			//_graphPointerDown = true;

			PointerEventData pe = (PointerEventData)e;
			UIRecordGraph_ValueItem pickItem = pickValueItem(pe.position);
			if (pickItem != null && pickItem != _lastPickItem)
			{
				_lastPickItem = pickItem;
				showTooltip(pickItem);
			}

			// 흠
		}

		public void onGraphPointer_Drag(BaseEventData e)
		{
			PointerEventData pe = (PointerEventData)e;
			UIRecordGraph_ValueItem pickItem = pickValueItem(pe.position);
			if( pickItem != null && pickItem != _lastPickItem)
			{
				_lastPickItem = pickItem;
				showTooltip(pickItem);
			}

			UIRecord.getInstance().scroller.ScrollRect.OnDrag(pe);
		}

		public void onGraphPointer_Up(BaseEventData e)
		{
			//_graphPointerDown = false;
			_lastPickItem = null;
		}

		public void onGraphPointer_BeginDrag(BaseEventData e)
		{
			UIRecord.getInstance().scroller.ScrollRect.OnBeginDrag((PointerEventData)e);
		}

		public void onGraphPointer_InitializePotentialDrag(BaseEventData e)
		{
			UIRecord.getInstance().scroller.ScrollRect.OnInitializePotentialDrag((PointerEventData)e);
		}

		public void onGraphPointer_EndDrag(BaseEventData e)
		{
			UIRecord.getInstance().scroller.ScrollRect.OnEndDrag((PointerEventData)e);
		}

		public void onGraphPointer_Scroll(BaseEventData e)
		{
			UIRecord.getInstance().scroller.ScrollRect.OnScroll((PointerEventData)e);
		}

		private UIRecordGraph_ValueItem pickValueItem(Vector2 scPos)
		{
			Vector2 localPosition;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(value_root, scPos, _camera, out localPosition);

			// 0부터 시작하도록 좌표 보정
			localPosition.x += value_root.rect.width / 2.0f;

			int index = (int)(localPosition.x * _valueList.Count / value_root.rect.width);
			if( index < 0 || index >= _valueList.Count)
			{
				return null;
			}

			return _valueList[index];
		}

		public void showTooltip(UIRecordGraph_ValueItem item)
		{
			DateTime localTime = item.graphValue.logData.begin_time.ToLocalTime();

			tip_root.gameObject.SetActive(true);
			tip_value.text = StringUtil.toStatDistanceString(item.graphValue.value) + "km";

			if( _currentTab == TabType.week ||
				_currentTab == TabType.month ||
				_currentTab == TabType.total)
			{
				tip_time.text = $"{localTime.Year}.{localTime.Month}.{localTime.Day}";
			}
			else
			{
				tip_time.text = $"{localTime.Year}.{localTime.Month}";
			}

			// tip 위치 맞추기
			Rect rectTipRoot = tip_root.rect;
			Rect rectTipParent = ((RectTransform)tip_root.parent).rect;
			
			Vector3 tipPosition = tip_root.transform.parent.InverseTransformPoint(item.rt.position);
			tipPosition.y = tip_root.transform.localPosition.y;
			tipPosition.z = tip_root.transform.localPosition.z;
			
			// 좌/우 boundary처리
			if( tipPosition.x - rectTipRoot.width / 2.0f < 0)
			{
				tipPosition.x = rectTipRoot.width / 2.0f;
			}

			if( tipPosition.x + rectTipRoot.width / 2.0f > rectTipParent.width)
			{
				tipPosition.x = rectTipParent.width - rectTipRoot.width / 2.0f;
			}
			tip_root.transform.localPosition = tipPosition;

			// line 위치 맞추기
			Vector3 linePosition = tip_line.transform.parent.InverseTransformPoint(item.rt.position);
			linePosition.y = tip_line.transform.localPosition.y;
			linePosition.z = tip_line.transform.localPosition.z;
			tip_line.transform.localPosition = linePosition;
			
			// line 사이즈 맞추기
			Vector2 localValueTopPosition = tip_line.transform.parent.InverseTransformPoint(item.rt.TransformPoint(new Vector3( 0, item.rt.rect.height, 0)));

			float size = Mathf.Abs(tip_line.transform.localPosition.y - localValueTopPosition.y);

			tip_line.setEnd( new Vector2(0, -size));
		}

		#endregion
	}
}
