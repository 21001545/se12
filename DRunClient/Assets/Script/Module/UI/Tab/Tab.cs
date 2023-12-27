using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct TabContent
{
    [SerializeField]
    public TabCell _cell;

    [SerializeField]
    public GameObject _content;
}


/// <summary>
/// UI 여기저기에서 Tab Content를 작성할 게 많아 보이니.. 간단하게 만들어두자.
/// 상속 받을 경우 꼭.. base.Start() 호출 필요(이거 require 못거나..?)
/// </summary>
public class Tab : MonoBehaviour
{
    [Header("탭 버튼과, 그 탭이 활성화 되어 있을 때 보여질 GameObject들..")]
    /// <summary>
    /// 음, TabCell Component를 가진 객체와 해당 Tab이 활성화 되었을 때 보여줄.. Content?
    /// </summary>
    [SerializeField]
    protected List<TabContent> _contents = new List<TabContent>();

    [SerializeField]
    private int _activeTabIndex = 0;

    /// <summary>
    /// 탭이 변경 되었을 경우 호출 되는 콜백
    /// </summary>
    public UnityEvent<TabCell> OnTabChanged;

    protected void Start()
    {
        for( var i = 0; i < _contents.Count;++i)
        {
            int capture = i;
            _contents[i]._cell.setClickCallback((TabCell cell)=>
            {
                // 음.. list라서 찾기가 애매하군.
                // 퍼포먼스의 문제가 생길 때 쯤, dictionary로 변경하던.. 일단은 람다로 돌린다.
                onTabClicked(cell, capture);
            });

        }

        onTabClicked(_contents[_activeTabIndex]._cell, _activeTabIndex);
    }

    /// <summary>
    /// 탭이 클릭 되었을 경우 호출 되는 콜백 함수, 
    /// 기본적인 기능은 구현해 두겠지만, 디테일한 연출은 상속받아 처리 할 수 있도록..?
    /// </summary>
    protected virtual void onTabClicked(TabCell cell, int index)
    {
        _activeTabIndex = index;

        OnTabChanged?.Invoke(cell);

        Debug.Log($"[Tab] onTabClicked {cell.name}-{index}");

        if (index >= _contents.Count )
        {
            Debug.Log($"Tab overflow {_contents.Count} - {index}");
            return;
        }

        // Tab을 선택 상태로 변경 해주어야 하는데..
        for (var i = 0; i < _contents.Count; ++i)
        {
            _contents[i]._content.SetActive(i == index);
        }
    }
}
