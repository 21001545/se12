using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMessengerPhotoCollect : UISingletonPanel<UIMessengerPhotoCollect>, IEnhancedScrollerDelegate
{
    public class CellType
    {
        public static readonly int date = 1;
        public static readonly int picture = 2;
    }

    public class CellDataBase
    {
        public CellDataBase(int type, float height)
        {
            this.height = height;
            this.type = type;
        }

        public int type;
        public float height;
    }

    public class DateCell : CellDataBase
    {
        public System.DateTime date;
        public DateCell(System.DateTime date) : base(CellType.date, 34.0f)
        {
            this.date = date;
        }
    }

    public class PictureCell : CellDataBase
    {
        public List<string> data;
        public PictureCell(List<string> data) : base(CellType.picture, 166.0f)
        {
            this.data = data;
        }
    }

    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private GameObject go_empty;

    [SerializeField]
    private UIMessengerPhotoCollectPictureRowCell _pictureRowCellPrefab;

    [SerializeField]
    private UIMessengerPhotoCollectDateCell _dateCellPrefab;

    private List<CellDataBase> _data = new List<CellDataBase>();

    private ChatRoomViewModel _roomVM = null;

    private class PictureData
    {
        public ClientChatRoomLog log;
        public string url;
        public int photoIndex = 0;
    };

    public void setup(ChatRoomViewModel roomVM)
    {
        _roomVM = roomVM;
    }

    public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
    {
        base.open(param, transitionType, closeType);

        _scroller.Delegate = this;

        _data.Clear();
        if (_roomVM != null )
        {
            System.DateTime time = new System.DateTime();
            List<PictureData> pictureData = new List<PictureData>();
            foreach (var log in _roomVM.LogList)
            {
                int logType = log.payload.getInteger("type");
                if (logType != 2)
                {
                    continue;
                }

                if (time != log.create_time)
                {
                    _data.Add(new DateCell(log.create_time));
                }

                JsonArray files = log.payload.getJsonArray("files");
                for (int k = 0; k < files.size(); ++k)
                {
                    pictureData.Add(new PictureData() { log = log, url = ClientChatRoomLog.getFileURL(files.getString(k)), photoIndex = k });

                    if (pictureData.Count == 3)
                    {
                        // 일단 대충..
                        _data.Add(new PictureCell(new List<string>() { pictureData[0].url, pictureData[1].url, pictureData[2].url }));
                        pictureData.Clear();
                    }
                }
            }

            if (pictureData.Count > 0)
            {
                var list = new List<string>();
                foreach (var data in pictureData)
                    list.Add(data.url);
                _data.Add(new PictureCell(list));
            }
        }


        go_empty.SetActive(_data.Count == 0);
    }

    public void onClickCloseNavigation()
    {
        ClientMain.instance.getPanelNavigationStack().pop();
    }

    public override void onTransitionEvent(int type)
    {
        base.onTransitionEvent(type);

        if (type == TransitionEventType.start_open)
        {
            _scroller.ReloadData();
        }
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        CellDataBase cellData = _data[dataIndex];
        if ( cellData.type == CellType.date)
        {
            UIMessengerPhotoCollectDateCell rowItem = scroller.GetCellView(_dateCellPrefab) as UIMessengerPhotoCollectDateCell;
            rowItem.setup(((DateCell)cellData).date);
            return rowItem;
        }
        else if (cellData.type == CellType.picture)
        {
            UIMessengerPhotoCollectPictureRowCell rowItem = scroller.GetCellView(_pictureRowCellPrefab) as UIMessengerPhotoCollectPictureRowCell;
            rowItem.setup(((PictureCell)cellData).data);
            return rowItem;
        }
        return null;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        if ( _data[dataIndex].type == CellType.picture )
        {
            float ratio = 166.0f / 123.0f;
            float width = (scroller.Container.rect.width - 6.0f) / 3.0f;
            return width * ratio;
        }
        return _data[dataIndex].height;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _data.Count;
    }
}
