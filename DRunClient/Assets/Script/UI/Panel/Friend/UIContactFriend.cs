using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
//using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
    public class UIContactFriend : UISingletonPanel<UIContactFriend>, IEnhancedScrollerDelegate
    {
        public EnhancedScroller scroller;
        public UIAddFriend_CellView cellViewItemSource;
        public UIContactFriend_CellView contactCellViewItemSource;
        public UIContactFriend_addFriendCellView addFriendCellView;
        public UIContactFriend_contactFriendCellView contactFriendCellView;
        private UnityAction<UIAddFriend_CellView> _followCallback;

        public GameObject loading;

        [SerializeField]
        private TMP_Text txt_inviteCount;

        private List<NativeContacts.NativeContactContext> _totalContactList; // 디바이스의 모든 연락처 리스트
        private List<NativeContacts.NativeContactContext> _inviteContactList; // 초대 메시지를 보낼 수 있는 연락처 리스트
        private List<ClientProfileCache> _friendProfileCacheList = new List<ClientProfileCache>(); // 이미 설치된 친구들 리스트

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private ClientNetwork Network => ClientMain.instance.getNetwork();

        #region cell class

        class CellType
        {
            public static readonly int friendTitleCell = 0;
            public static readonly int friendCell = 1;
            public static readonly int inviteTitleCell = 2;
            public static readonly int inviteCell = 3;
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
        class FriendTitleCell : CellBase
        {
            public FriendTitleCell() : base(40f, CellType.friendTitleCell)
            {
            }
        };

        class FriendCell : CellBase
        {
            public ClientProfileCache data;

            public FriendCell(ClientProfileCache data) : base(75f, CellType.friendCell)
            {
                this.data = data;
            }
        };

        class InviteTitleCell : CellBase
        {
            public InviteTitleCell() : base(75f, CellType.inviteTitleCell)
            {
            }
        }

        class InviteCell : CellBase
        {
            public NativeContacts.NativeContactContext context;
            public InviteCell(NativeContacts.NativeContactContext context) : base(72f, CellType.inviteCell)
            {
                this.context = context;
            }
        }

        private List<CellBase> _friendCellList = new List<CellBase>();
        private List<CellBase> _inviteCellList = new List<CellBase>();
        private FriendTitleCell _friendTitle = new FriendTitleCell();
        private InviteTitleCell _inviteTitle = new InviteTitleCell();

        private void reloadFriendList()
        {
            _friendCellList.Clear();
            _friendCellList.Add(_friendTitle);

            for(int i = 0; i < _friendProfileCacheList.Count; ++i)
            {
                _friendCellList.Add(new FriendCell(_friendProfileCacheList[i]));
            }
        }

        private void reloadInviteList()
        {
            _inviteCellList.Clear();
            _inviteCellList.Add(_inviteTitle);

            for (int i = 0; i < _inviteContactList.Count; ++i)
            {
                _inviteCellList.Add(new InviteCell(_inviteContactList[i]));
            }
        }

        #endregion

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            //friendScroller.Delegate = new FriendScrollerDelegate(_friendProfileCacheList, cellViewItemSource, onClickFollow);
            scroller.Delegate = this;
            loading.SetActive(false);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);

            // 연락처 리스트를 가져오자.
            // 이거 어차피 동기 함수니깐..
            _totalContactList = NativeContacts.NativeContacts.GetContacts();
            _inviteContactList = new List<NativeContacts.NativeContactContext>();

            // 서버에 연락처를 기준으로 앱 설치 여부를 질의 해보자.
            searchByPhoneNumber(_totalContactList);
        }

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

/*        private void loadData()
        {
            ClientFollowSuggestionConfig config = ViewModel.Social.SuggestionConfig;
            if (DateTime.UtcNow < config.next_update_time)
            {
                reloadFriendList();
                scroller.ReloadData();
                return;
            }

            loading.SetActive(true);

            GetFollowSuggestionProcessor processor = GetFollowSuggestionProcessor.create();
            processor.run(() =>
            {
                reloadFriendList();
                scroller.ReloadData();
                loading.SetActive(false);
            });
        }*/

        private void onClickFollow(UIAddFriend_CellView cellView)
        {
            ClientFollowSuggestion suggestion = cellView.getData();

            UIBlockingInput.getInstance().open();

            MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
            req.put("id", suggestion.suggestion_id);

            Network.call(req, ack =>
            {

                UIBlockingInput.getInstance().close();

                if (ack.getResult() == ResultCode.ok)
                {
                    ViewModel.updateFromPacket(ack);

                    ClientFollow newFollow = (ClientFollow)ack.get(MapPacketKey.ClientAck.follow);

                    ViewModel.Social.SuggestionList.Remove(suggestion);
                    reloadFriendList();
                    scroller.ReloadData();
                }
            });
        }

        public void onSendSMS(UIContactFriend_CellView view)
        {
            string name = ClientMain.instance.getViewModel()?.Profile?.Profile?.name;
            if (NativeContacts.NativeContacts.OpenSMS(view.getPhoneNumber(), GlobalRefDataContainer.getStringCollection().getFormat("friend.invite.meesage", 0, name)) == false)
            {
                //팝업 띄우자~
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            List<CellBase> allCells = new List<CellBase>();
            allCells.AddRange(_friendCellList);
            allCells.AddRange(_inviteCellList);

            var cellItem = allCells[dataIndex];

            if(cellItem.type == CellType.friendTitleCell)
            {
                return scroller.GetCellView(addFriendCellView) as UIContactFriend_addFriendCellView;
            }
            else if(cellItem.type == CellType.friendCell)
            {
                FriendCell cell = (FriendCell)cellItem;
                UIAddFriend_CellView item = scroller.GetCellView(cellViewItemSource) as UIAddFriend_CellView;

                _followCallback = onClickFollow;
                item.setup(cell.data, _followCallback);
                return item;
            }
            else if(cellItem.type == CellType.inviteTitleCell)
            {
                UIContactFriend_contactFriendCellView item = scroller.GetCellView(contactFriendCellView) as UIContactFriend_contactFriendCellView;
                item.setup(_inviteContactList.Count);
                return item;
            }
            else if(cellItem.type == CellType.inviteCell)
            {
                InviteCell cell = (InviteCell)cellItem;
                UIContactFriend_CellView item = scroller.GetCellView(contactCellViewItemSource) as UIContactFriend_CellView;
                item.setup(cell.context.phoneNumber, cell.context.name, onSendSMS);

                return item;
            }

            return null;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            List<CellBase> allCells = new List<CellBase>();
            allCells.AddRange(_friendCellList);
            allCells.AddRange(_inviteCellList);
            return allCells[dataIndex].height;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _friendCellList.Count + _inviteCellList.Count;
            //return _inviteContactList != null ? _inviteContactList.Count : 0;
        }

        private void searchByPhoneNumber(List<NativeContacts.NativeContactContext> contactContexts)
        {
            searchByPhoneNumberIter(contactContexts.GetEnumerator());
        }

        private void searchByPhoneNumberIter(List<NativeContacts.NativeContactContext>.Enumerator e)
        {
            if (e.MoveNext() == false)
            {
                reloadFriendList();
                reloadInviteList();
                scroller.ReloadData();
                return;
            }

            string phoneNumber = e.Current.phoneNumber;
            //var util = PhoneNumberUtil.GetInstance();
            //phoneNumber = util.Format(util.Parse(phoneNumber, ClientMain.instance.getLocale().getCountryCode()), PhoneNumberFormat.E164);

            SearchAccountProcessor processor = SearchAccountProcessor.create(SearchAccountProcessor.QueryType.phone_number, phoneNumber);
            processor.run(result =>
            {
                if (result.succeeded())
                {
                    List<ClientProfileCache> accountList = processor.getResultList();
                    if ( accountList.Count == 0)
                    {
                        // 검색 결과가 없으면, 초대 리스트에 추가..
                        _inviteContactList.Add(e.Current);

                        // 음 중간에 한번은 리프레시 해줘야할 것 같은데,
                        if (_inviteContactList.Count % 10 == 0 )
                        {
                            reloadInviteList();
                            scroller.ReloadData();
                        }
                    }
                    else
                    {
                        // 번호 검색인데.. 한개만 있지 않을까?
                        _friendProfileCacheList.AddRange(accountList);
                    }
                }

                searchByPhoneNumberIter(e);
            });
        }
    }
}