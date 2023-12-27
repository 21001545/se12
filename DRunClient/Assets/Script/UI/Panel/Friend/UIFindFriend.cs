using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
    public class UIFindFriend : UISingletonPanel<UIFindFriend>, IEnhancedScrollerDelegate
    {
        public EnhancedScroller scroller;
        public GameObject loading;

        [SerializeField]
        private UIFriendRecommended _recommendedText;
        [SerializeField]
        private UIAddFriend_CellView cellViewItemSource;
        [SerializeField]
        private Button btn_searchButton;

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private ClientNetwork Network => ClientMain.instance.getNetwork();

        private List<ClientProfileCache> _searchedList;

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            scroller.Delegate = this;
            loading.SetActive(false);

            _searchedList = new List<ClientProfileCache>();
		}

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

        private void loadData()
        {
            ClientFollowSuggestionConfig config = ViewModel.Social.SuggestionConfig;
            if (DateTime.UtcNow < config.next_update_time)
            {
                scroller.ReloadData();
                return;
            }

            loading.SetActive(true);

            GetFollowSuggestionProcessor processor = GetFollowSuggestionProcessor.create();
            processor.run(() =>
            {
                scroller.ReloadData();
                loading.SetActive(false);
            });
        }

        private void onClickSuggestionFollow(UIAddFriend_CellView cellView)
        {
            ClientFollowSuggestion suggestion = cellView.getData();

            follow(suggestion.suggestion_id, (int result) =>
            {
                ViewModel.Social.SuggestionList.Remove(suggestion);
                scroller.ReloadData();
            });
        }
        private void onClickSearchFollow(UIAddFriend_CellView cellView)
        {
            ClientProfileCache profile = cellView.getProfileCache();

            follow(profile._accountID, (int result) =>
            {
                profile.Profile._isFollow = result == ResultCode.ok;
                cellView.updateFollow();
            });
        }

        private void follow(int id, Action<int> callback)
        {
            UIBlockingInput.getInstance().open();
            MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
            req.put("id", id);

            Network.call(req, ack =>
            {
                UIBlockingInput.getInstance().close();

                if (ack.getResult() == ResultCode.ok)
                {
                    ViewModel.updateFromPacket(ack);
                }

                callback?.Invoke(ack.getResult());
            });
        }


        public void searchByName(string name)
        {
            // 별도의 로딩은 띄워주지 말고, 그냥 버튼을 비활성화 시키자.
            btn_searchButton.interactable = false;
            SearchAccountProcessor processor = SearchAccountProcessor.create(SearchAccountProcessor.QueryType.name, name);
            processor.run(result =>
            {
                btn_searchButton.interactable = true;
                if (result.succeeded())
                {
                    _searchedList = processor.getResultList();
                    scroller.ReloadData();
 
					//// 음, 이거 통합 기능 만들어야 하는데..
					//List<ClientProfileCache> accountList = processor.getResultList();
     //               for (int i = _searchResultList.childCount; i < accountList.Count; ++i)
     //               {
     //                   Instantiate(cellViewItemSource, _searchResultList);
     //               }

     //               int processCount = 0;
     //               foreach (ClientProfileCache profile in accountList)
     //               {
     //                   var cellView = _searchResultList.GetChild(processCount).GetComponent<UIAddFriend_CellView>();
     //                   cellView.gameObject.SetActive(true);
     //                   cellView.setup(profile, onClickSearchFollow);
     //                   processCount++;
     //               }

     //               for (; processCount < _searchResultList.childCount; ++processCount)
     //               {
     //                   _searchResultList.GetChild(processCount).gameObject.SetActive(false);
     //               }

     //               LayoutRebuilder.ForceRebuildLayoutImmediate(_searchResultList);
                }
            });
        }

        #region scroller_delegate

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            if( dataIndex < _searchedList.Count)
            {
				UIAddFriend_CellView item = scroller.GetCellView(cellViewItemSource) as UIAddFriend_CellView;
                item.setup(_searchedList[dataIndex], onClickSearchFollow);
                return item;
			}
			else if( dataIndex == _searchedList.Count)
            {
				return scroller.GetCellView(_recommendedText) as UIFriendRecommended;
			}
            else
            {
                int suggestionIndex = dataIndex - _searchedList.Count - 1;
				UIAddFriend_CellView item = scroller.GetCellView(cellViewItemSource) as UIAddFriend_CellView;

				ClientFollowSuggestion suggestion = ViewModel.Social.SuggestionList[suggestionIndex];
				item.setup(suggestion, onClickSuggestionFollow);
                return item;
			}
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            if( dataIndex ==_searchedList.Count)
            {
				return 40;
			}
			else
            {
				return 72.0f;
            }
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ViewModel.Social.SuggestionList.Count + 1 + _searchedList.Count();
        }
        #endregion
    }
}