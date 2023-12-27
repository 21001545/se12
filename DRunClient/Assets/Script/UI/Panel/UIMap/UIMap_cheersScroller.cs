using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.RefData;
using Festa.Client.NetData;

public class UIMap_cheersScroller : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField]
    private EnhancedScroller _scroller;
    [SerializeField]
    private UIMap_cheerFriendCellView _cheerFriendCellView;
    [SerializeField]
    private UIMap_titleCellView _titleCellView;
    [SerializeField]
    private UIMap_breakLineCellView _breakLineCellView;

    private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

    #region cell class

    class CellType
    {
        public static readonly int titleCell = 0;
        public static readonly int friendsCell = 1;
        public static readonly int lineCell = 2;
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

    class TitleCell : CellBase
    {
        public string title;
        public TitleCell(string title) : base(40f, CellType.titleCell)
        {
            this.title = title;
        }
    }

    class FriendsCell : CellBase
    {
        public ClientTripCheerable data;

        public FriendsCell(ClientTripCheerable cheerable) : base(64f, CellType.friendsCell)
        {
            data = cheerable;
        }
    }

    class LineCell : CellBase
    {
        public LineCell() : base(0.75f, CellType.lineCell)
        {
        }
    }

    private List<CellBase> _data = new List<CellBase>();
    private LineCell _lineData = new LineCell();

    #endregion

    private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

    public void setupData()
    {
        _scroller.Delegate = this;

        _data.Clear();

        List<ClientTripCheerable> cheerableByDistance = ViewModel.Trip.CheerableListByDistance;
        List<ClientTripCheerable> cheerableByFollow = ViewModel.Trip.CheerableListByFollow;

        _data.Add(new TitleCell(StringCollection.get("map.nearMe", 0)));
        
        foreach(ClientTripCheerable cheerable in cheerableByDistance)
        {
            _data.Add(new FriendsCell(cheerable));
        }
        _data.Add(_lineData);
        
        _data.Add(new TitleCell(StringCollection.get("map.myFriends", 0)));

        foreach(ClientTripCheerable cheerable in cheerableByFollow)
        {
            _data.Add(new FriendsCell(cheerable));
        }

        _scroller.ReloadData();
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var itemData = _data[dataIndex];

        if (itemData.type == CellType.lineCell)
        {
            UIMap_breakLineCellView cell = scroller.GetCellView(_breakLineCellView) as UIMap_breakLineCellView;
            return cell;
        }
        else if (itemData.type == CellType.titleCell)
        {
            TitleCell item = (TitleCell)itemData;
            UIMap_titleCellView cell = scroller.GetCellView(_titleCellView) as UIMap_titleCellView;
            cell.setup(item.title);
            return cell;
        }
        else if(itemData.type == CellType.friendsCell)
        {
            FriendsCell item = (FriendsCell)itemData;
            UIMap_cheerFriendCellView cell = scroller.GetCellView(_cheerFriendCellView) as UIMap_cheerFriendCellView;
            cell.setup(this,item.data);
            return cell;
        }

        return null;
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
