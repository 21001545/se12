using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.NetData;

namespace Festa.Client
{
    public class UITripEndResult_scrollDelegate : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField]
        private EnhancedScroller _scroller;
        [SerializeField]
        private UITripEndResult_coin _coinCell;
        [SerializeField]
        private UITripEndResult_title _titleCell;
        [SerializeField]
        private UITripEndResult_cheerCell _cheerCell;

        #region cell class

        class CellType
        {
            public static readonly int coinCell = 0;
            public static readonly int titleCell = 1;
            public static readonly int cheerCell = 2;
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

        class CoinCell : CellBase
        {
            public int normalCoin;
            public int bonusCoin;

            public CoinCell() : base(180f, CellType.coinCell)
            {
            }

            public void setData(int normalCoin, int bonusCoin)
            {
                this.normalCoin = normalCoin;
                this.bonusCoin = bonusCoin;
            }
        }

        class TitleCell : CellBase
        {
            public TitleCell() : base(62f, CellType.titleCell)
            {
            }
        }

        class CheerCell : CellBase
        {
            public ClientTripCheering data;  // 일단 친구유아이처럼 만들어보자,,
            public CheerCell(ClientTripCheering data) : base(74f, CellType.cheerCell)
            {
                this.data = data;
            }
        }

        private List<CellBase> _data = new List<CellBase>();
        private CoinCell _coin = new CoinCell();
        private TitleCell _title = new TitleCell();

        #endregion

        public void setData(ClientTripLog log)
        {
            _data.Clear();
            _data.Add(_coin);
            _data.Add(_title);

            // 응원데이터 추가
            foreach(ClientTripCheering cheering in log._cheeringList)
            {
                _data.Add(new CheerCell(cheering));
            }
        }

        public void setCoins(int normal, int bonus)
        {
            _coin.setData(normal, bonus);
        }

        public void setup(ClientTripLog log)
        {
            _scroller.Delegate = this;
            setData(log);
            _scroller.ReloadData();
        }


        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var data = _data[dataIndex];

            if (dataIndex == 0)
            {
                var cellData = (CoinCell) data;
                var cell = scroller.GetCellView(_coinCell) as UITripEndResult_coin;
                cell.setup(cellData.normalCoin, cellData.bonusCoin);
                return cell;
            }
            else if(dataIndex == 1)
            {
                return scroller.GetCellView(_titleCell) as UITripEndResult_title;
            }
            else
            {
                dataIndex -= 2;
                var cellData = (CheerCell) data;
                var cell = scroller.GetCellView(_cheerCell) as UITripEndResult_cheerCell;
                // 이렇게,, 데이터 넣기
                cell.setup(cellData.data);

                return cell;
            }
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _data[dataIndex].height;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _data.Count;
        }
    }

}