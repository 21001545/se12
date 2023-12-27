using DRun.Client.Logic.Account;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIInvitationCode : UISingletonPanel<UIInvitationCode>, IEnhancedScrollerDelegate
	{
		public UIColorToggleButton[] tabButtons;
		public UITabSlide tabSlide;
		public GameObject[] pages;
		public GameObject loading;

		[Header("--------- MyCode ------------")]
		public EnhancedScroller scroller;
		public UIInvitationCode_MyCode_Control cellControl;
		public UIInvitationCode_MyCode_Acceptor cellAcceptor;
		public UIInvitationCode_MyCode_Empty cellEmpty;

		[Header("--------- Input ------------")]
		public TMP_Text txtInputTitle;
		public TMP_Text txtInputDesc;
		public GameObject rootAccpeted;
		public GameObject rootNotAccepted;
		public UIInputField inputCode;
		public TMP_Text inputError;
		public UILoadingButton btnSubmitCode;

		public class TabType
		{
			public const int mycode = 0;
			public const int input = 1;
		}

		private int _currentTab;

		//-----------------------------------
		public class CellItem
		{
			public int type;
			public ClientInvitationAccept acceptor;
			public UIInvitationCode_MyCode_CellBase cellSource;

			public static CellItem create(int type, UIInvitationCode_MyCode_CellBase cell)
			{
				CellItem cellItem = new CellItem();
				cellItem.type = type;
				cellItem.acceptor = null;
				cellItem.cellSource = cell;
				return cellItem;
			}

			public static CellItem create(ClientInvitationAccept acceptor, UIInvitationCode_MyCode_CellBase cell)
			{
				CellItem cellItem = new CellItem();
				cellItem.type = CellType.acceptor;
				cellItem.acceptor = acceptor;
				cellItem.cellSource = cell;
				return cellItem;
			}
		}

		public class CellType
		{
			public const int control = 1;
			public const int acceptor = 2;
			public const int empty = 3;
		}

		private List<CellItem> _itemList;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_currentTab = -1;
			scroller.Delegate = this;
			_itemList = new List<CellItem>();
			loading.SetActive(false);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			resetBindings();
			base.open(param, transitionType, closeType);

			setupTitleDesc();

			inputCode.text = "";
			inputCode.hideClearButton();
			inputError.gameObject.SetActive(false);
		}

		private void setupTitleDesc()
		{
			double receive_reward = GlobalRefDataContainer.getConfigDouble(RefConfig.Key.DRun.invitation_receive_reward, 2);

			txtInputTitle.text = StringCollection.getFormat("invitation.input.title", 0, receive_reward);
			txtInputDesc.text = StringCollection.getFormat("invitation.input.desc", 0, receive_reward);
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			EventViewModel eventVM = ViewModel.Event;

			_bindingManager.makeBinding(eventVM, nameof(eventVM.InvitationCode), updateInvitationCode);
		}

		private void updateInvitationCode(object obj)
		{
			ClientInvitationCode code = ViewModel.Event.InvitationCode;

			rootAccpeted.SetActive(code.accept_account_id != 0);
			rootNotAccepted.SetActive(code.accept_account_id == 0);

			btnSubmitCode.gameObject.SetActive(code.accept_account_id == 0);
		}

		public override void onTransitionEvent(int type)
		{
			if (type == TransitionEventType.start_open)
			{
				setTab(TabType.mycode, true);
				loadAcceptorList();
			}
		}

		public void setTab(int tab, bool updateNow)
		{
			if (_currentTab == tab)
			{
				return;
			}

			_currentTab = tab;
			for (int i = 0; i < pages.Length; ++i)
			{
				pages[i].SetActive(i == tab);
				tabButtons[i].setStatus(i == tab);
			}

			tabSlide.setTab(tab, updateNow);
		}

		public void onClick_Back()
		{
			UIMainTab.getInstance().open();
			RectTransform rt = transform as RectTransform;
			rt.moveInDirection(delta: (from: 0, to: Screen.width * 0.5f));
		}

		public void onClick_MyCode()
		{
			setTab(TabType.mycode, false);
		}

		public void onClick_Input()
		{
			setTab(TabType.input, false);
		}

		public void onClick_SubmitCode()
		{
			string code = inputCode.text;
			if( code.Length < 6)
			{
				inputError.text = StringCollection.get("invitation.input.wrong_code", 0);
				inputError.gameObject.SetActive(true);
				return;
			}

			code = code.ToUpper();

			btnSubmitCode.beginLoading();

			AcceptInvitationCodeProcessor step = AcceptInvitationCodeProcessor.create(code);
			step.run(result => {
				btnSubmitCode.endLoading();

				if( result.failed())
				{
					if( step.getErrorCode() == ResultCode.error_device_already)
					{
						inputError.gameObject.SetActive(true);
						inputError.text = StringCollection.get("invitation.input.device_duplicate", 0);
					}
					else
					{
						inputError.gameObject.SetActive(true);
						inputError.text = StringCollection.get("invitation.input.wrong_code", 0);
					}
				}
				else
				{
					// 보상을 받았겠군
					ClientDRNTransaction transaction = step.getTransaction();
					UIClaimInvitationReward.getInstance().open(transaction.delta);
				}
			});
		}

		private void loadAcceptorList()
		{
			_itemList.Clear();
			_itemList.Add(CellItem.create(CellType.control, cellControl));

			if( scroller.Container != null)
			{
				scroller.ReloadData();
			}

			loading.SetActive(true);

			QueryInvitationAcceptorListProcessor step = QueryInvitationAcceptorListProcessor.create(0, 20);
			step.run(result => {
				loading.SetActive(false);

				if ( result.succeeded())
				{
					List<ClientInvitationAccept> list = step.getList();
					if( list.Count == 0)
					{
						_itemList.Add(CellItem.create(CellType.empty, cellEmpty));
					}
					else
					{
						foreach(ClientInvitationAccept item in list)
						{
							_itemList.Add(CellItem.create(item, cellAcceptor));
						}
					}

					if (scroller.Container != null)
					{
						scroller.ReloadData();
					}
				}
			});
		}

		#region scroller
		EnhancedScrollerCellView IEnhancedScrollerDelegate.GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellItem item = _itemList[dataIndex];
			UIInvitationCode_MyCode_CellBase cell = (UIInvitationCode_MyCode_CellBase)scroller.GetCellView(item.cellSource);
			cell.setup(item);
			return cell;
		}

		float IEnhancedScrollerDelegate.GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return _itemList[dataIndex].cellSource.height;
		}

		int IEnhancedScrollerDelegate.GetNumberOfCells(EnhancedScroller scroller)
		{
			return _itemList.Count;
		}
		#endregion
	}
}
