
using EnhancedUI.EnhancedScroller;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Festa.Client
{
	public class UIActivity : UISingletonPanel<UIActivity>, IEnhancedScrollerDelegate
	{
		public GameObject go_loading;
		public EnhancedScroller scroller;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		[SerializeField]
        private UIActivityItem itemCellPrefab;

        [SerializeField]
        private UIActivityDateCell dateCellPrefab;

        class CellType
        {
            public static readonly int dataCell = 0;
            public static readonly int dateCell = 1;
        }

		class CellBase
		{
			public float height;
			public int type;
			public CellBase(float height, int type)
			{
				this.height = height;
				this.type = type;
			}
		};
        class DataCell : CellBase
		{
			public ClientActivity activity;
			public DataCell(ClientActivity activity) : base(64.0f, CellType.dataCell)
            {
				this.activity = activity;
            }
		};

		class DateCell : CellBase
		{
			public string text;
			public DateCell(string text, float height) : base(height, CellType.dateCell)
            {
				this.text = text;
			}
		};

        private List<CellBase> _data = new List<CellBase>();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
			go_loading.gameObject.SetActive(false);
		}

		public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);

			loadActivity();
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			ActivityViewModel vm = ViewModel.Activity;
			_bindingManager.makeBinding(vm, nameof(vm.ActivityList), onUpdateActivityList);
		}

		private void onUpdateActivityList(object obj)
		{
			if (scroller.Container != null)
			{
				_data.Clear();

				var sc = GlobalRefDataContainer.getStringCollection();
				// 우선, 좋아요 보상 부터 모으자.
                for (var i = 0; i < ViewModel.Activity.ActivityList.Count; ++i)
                {
                    if (ViewModel.Activity.ActivityList[i].event_type == ClientActivity.Type.reward_moment_like)
                    {
						_data.Add(new DataCell(ViewModel.Activity.ActivityList[i]));
                    }
                }

				if (_data.Count > 0)
                {
					// 맨 처음에 "아직 안 받은 스타" 넣어주자.
					_data.Insert(0, new DateCell(sc.get("moment.activity.cellTitle", 0), 39.0f));
                }

                bool today = false;
                bool yesterday = false;
                bool otherDay = false;
                for (var i = 0; i < ViewModel.Activity.ActivityList.Count; ++i)
                {
					var activity = ViewModel.Activity.ActivityList[i];

					if (activity.event_type == ClientActivity.Type.reward_moment_like)
                    {
						continue;
                    }
					var date = DateTime.UtcNow - activity.event_time;

					if (date.TotalDays < 1 && today == false )
                    {
						// 오늘
                        _data.Add(new DateCell(sc.get("moment.activity.cellTitle", 1), 39.0f));
                        today = true;
                    }
					else if (date.TotalDays > 1 && date.TotalDays < 2 && yesterday == false)
                    {
						// 어제
                        _data.Add(new DateCell(sc.get("moment.activity.cellTitle", 2), 47.0f));
						yesterday = true;
                    }
                    else if (date.TotalDays > 2 && otherDay == false)
                    {
						// 이전 아림
                        _data.Add(new DateCell(sc.get("moment.activity.cellTitle", 3), 47.0f));
						otherDay = true;
                    }

                    _data.Add(new DataCell(activity));
                }

                scroller.ReloadData();
			}
		}

		private void loadActivity()
		{
			go_loading.SetActive(true);

			ViewModel.Activity.ClearActivityList();

			QueryActivityProcessor processor = QueryActivityProcessor.create(0, 0, 20);
			processor.run(result => {

				go_loading.SetActive(false);

				saveReadSlotID();
			});
		}

		public void claimReward(ClientActivity data, int dataIndex)
		{
			MapPacket req = Network.createReq(CSMessageID.Social.ClaimActivityRewardReq);
			req.put("slot_id", data.slot_id);

			Network.call(req, ack => { 
				
				if( ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);
					ViewModel.Activity.ActivityList.RemoveAt(dataIndex);
					data.claim_status = ClientActivity.ClaimStatus.claimed;
					onUpdateActivityList(null);
				}
			});
		}

		private void saveReadSlotID()
		{
			MapPacket req = Network.createReq(CSMessageID.Social.SaveActivityReadSlotIDReq);
			req.put("slot_id", ViewModel.Activity.ReadSlotID);

			Network.call(req, ack => { });
		}

		#region scroller_delegate
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller,int dataIndex,int cellIndex)
		{
			var cellData = _data[dataIndex];
			if (cellData.type == CellType.dataCell) 
            {
                UIActivityItem cellView = scroller.GetCellView(itemCellPrefab) as UIActivityItem;
                cellView.setup((cellData as DataCell).activity);
                return cellView;
            }
			else
            {
                UIActivityDateCell cellView = scroller.GetCellView(dateCellPrefab) as UIActivityDateCell;
                cellView.setup((cellData as DateCell).text, cellData.height > 40.0f);
                return cellView;
            }

			//return null;
		}

		public float GetCellViewSize(EnhancedScroller scroller,int dataIndex)
		{
			return _data[dataIndex].height;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _data.Count;
		}

		#endregion
	}
}
