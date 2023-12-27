using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMakeMomentTagSelect : UIPanel
{
    // 개수만큼 추가를 해야하는데?
    private UnityAction<int> _callback = null;

    [SerializeField]
    private RectTransform list = null;
    [SerializeField]
    private GameObject itemPrefab = null;

    private List<UIMakeMomentTagSelectItem> _itemList;

    private int _selectID = 0;
    public static UIMakeMomentTagSelect spawn(int selectID, UnityAction<int> callback)
    {
        UIMakeMomentTagSelect popup = UIManager.getInstance().spawnInstantPanel<UIMakeMomentTagSelect>();

        popup._callback = callback;
        popup._selectID = selectID;

        // 이거 매번 만들어..?
        // RefData를 못가져오네.. 이건 목요일에 요청 드려보자.
        // 일단 하드하게 33개임.
        //GlobalRefDataContainer.getInstance().get<RefMomentLifelog>(_currentWalkLevel.level);

        popup.prepareList();
        popup.updateList();

        return popup;
    }

    private void prepareList()
	{
        if(_itemList != null)
		{
            return;
		}

        List<RefMomentLifeLog> refLogList = new List<RefMomentLifeLog>();
        foreach(KeyValuePair<int,RefData> item in GlobalRefDataContainer.getInstance().getMap<RefMomentLifeLog>())
		{
            refLogList.Add((RefMomentLifeLog)item.Value);
		}

        refLogList.Sort((a, b) => { 
            if( a.order < b.order)
			{
                return -1;
			}
            else if( a.order > b.order)
			{
                return 1;
			}

            return 0;
        });


        //
        _itemList = new List<UIMakeMomentTagSelectItem>();
        foreach (RefMomentLifeLog log in refLogList)
        {
            var child = Instantiate(itemPrefab, list);

            UIMakeMomentTagSelectItem item = child.GetComponent<UIMakeMomentTagSelectItem>();
            item.Initialize(log);

            child.GetComponent<Button>().onClick.AddListener(() =>
            {
                onClickTag(log.id);
            });

            _itemList.Add(item);
        }
    }

    private void updateList()
	{
        foreach(UIMakeMomentTagSelectItem item in _itemList)
		{
            item.SetSelect(item.getLogID() == _selectID);
		}
    }

    public void onClickTag(int id)
    {
        _selectID = id;

        updateList();
    }

    public void onClickConfirm()
    {
        close();
        _callback?.Invoke(_selectID);
    }
}
