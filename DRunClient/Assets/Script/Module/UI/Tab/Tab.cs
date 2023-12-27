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
/// UI �������⿡�� Tab Content�� �ۼ��� �� ���� ���̴�.. �����ϰ� ��������.
/// ��� ���� ��� ��.. base.Start() ȣ�� �ʿ�(�̰� require ���ų�..?)
/// </summary>
public class Tab : MonoBehaviour
{
    [Header("�� ��ư��, �� ���� Ȱ��ȭ �Ǿ� ���� �� ������ GameObject��..")]
    /// <summary>
    /// ��, TabCell Component�� ���� ��ü�� �ش� Tab�� Ȱ��ȭ �Ǿ��� �� ������.. Content?
    /// </summary>
    [SerializeField]
    protected List<TabContent> _contents = new List<TabContent>();

    [SerializeField]
    private int _activeTabIndex = 0;

    /// <summary>
    /// ���� ���� �Ǿ��� ��� ȣ�� �Ǵ� �ݹ�
    /// </summary>
    public UnityEvent<TabCell> OnTabChanged;

    protected void Start()
    {
        for( var i = 0; i < _contents.Count;++i)
        {
            int capture = i;
            _contents[i]._cell.setClickCallback((TabCell cell)=>
            {
                // ��.. list�� ã�Ⱑ �ָ��ϱ�.
                // �����ս��� ������ ���� �� ��, dictionary�� �����ϴ�.. �ϴ��� ���ٷ� ������.
                onTabClicked(cell, capture);
            });

        }

        onTabClicked(_contents[_activeTabIndex]._cell, _activeTabIndex);
    }

    /// <summary>
    /// ���� Ŭ�� �Ǿ��� ��� ȣ�� �Ǵ� �ݹ� �Լ�, 
    /// �⺻���� ����� ������ �ΰ�����, �������� ������ ��ӹ޾� ó�� �� �� �ֵ���..?
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

        // Tab�� ���� ���·� ���� ���־�� �ϴµ�..
        for (var i = 0; i < _contents.Count; ++i)
        {
            _contents[i]._content.SetActive(i == index);
        }
    }
}
