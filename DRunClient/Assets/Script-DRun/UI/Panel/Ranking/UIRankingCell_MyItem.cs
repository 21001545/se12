using DRun.Client.NetData;
using DRun.Client.ViewModel;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace DRun.Client
{
	public class UIRankingCell_MyItem : UIRankingCell_Item
	{
		public GameObject base_ranked;
		public GameObject base_not_ranked;
		
		private int _myDataIndex;

		public class PositionType
		{
			public const int top = 0;
			public const int bottom = 1;
			public const int on_list = 2;
		}

		private int _positionType;
		private UIRankingCell_MyItemTarget _targetCell;

		private RankingCacheData rankData => UIRanking.getInstance().getRankingData();
		private EnhancedScroller scroller => UIRanking.getInstance().scroller;
		private RectTransform rt => (RectTransform)transform;
		private RectTransform rtParent => (RectTransform)transform.parent;

		public void setup(ClientRankingData data,int mydataindex)
		{
			base.setup(data);
			_myDataIndex = mydataindex;
			base_ranked.SetActive(_data.rank != -1);
			base_not_ranked.SetActive(_data.rank == -1);
		}

		void LateUpdate()
		{
			checkPositionType();
			updatePosition();
		}

		private void checkPositionType()
		{
			Vector2Int rankRange = rankData.getRankRange();

			_positionType = PositionType.bottom;
			_targetCell = null;

			// 내가 순위에 없을 경우
			if( _data.rank == -1)
			{
				_positionType = PositionType.bottom;
				return;
			}

			if ( _myDataIndex == -1)
			{
				// 사용자 검색 모드에서는 제일 아래로 (검색 결과에 내가 없을때)
				if( UIRanking.getInstance().isSearchMode())
				{
					_positionType = PositionType.bottom;
				}
				else
				{
					if (_data.rank < rankRange.x)
					{
						_positionType = PositionType.top;
					}
					else if (_data.rank > rankRange.y)
					{
						_positionType = PositionType.bottom;
					}
					else
					{
						_positionType = PositionType.bottom;
					}
				}

				//#if UNITY_EDITOR
				//				Debug.LogWarning($"myDataIndex is -1, but not in rankRange: _data.rank[{_data.rank}] rankRange[{rankRange}]");
				//#endif
			}
			else
			{
				_targetCell = (UIRankingCell_MyItemTarget)scroller.GetCellViewAtDataIndex(_myDataIndex);
				
				if( _targetCell != null)
				{
					_positionType = PositionType.on_list;
				}
				else
				{
					if( _myDataIndex <= scroller.StartDataIndex)
					{
						_positionType = PositionType.top;
					}
					else if( _myDataIndex >= scroller.EndDataIndex)
					{
						_positionType = PositionType.bottom;
					}
				}
			}
		}

		private Vector3[] _worldCorners = new Vector3[4];

		private void updatePosition()
		{
			if( _positionType == PositionType.top)
			{
				rt.anchoredPosition = new Vector2(0, 0);
			}
			else if( _positionType == PositionType.bottom)
			{
				Rect rectParent = rtParent.rect;
				rt.anchoredPosition = new Vector2(0, -rectParent.height);
			}
			else
			{
				rtParent.GetWorldCorners(_worldCorners);
				Vector2 yRange = new Vector2(Mathf.Min(_worldCorners[0].y, _worldCorners[2].y), Mathf.Max(_worldCorners[0].y, _worldCorners[2].y));

				Vector3 targetPosition = _targetCell.transform.position;
				targetPosition.y = Mathf.Clamp(targetPosition.y, yRange.x, yRange.y);

				rt.position = targetPosition;
			}
		}
	}
}
