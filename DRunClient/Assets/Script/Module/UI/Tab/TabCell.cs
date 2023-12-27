using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabCell : MonoBehaviour
{
    // �� TabCell�� Ŭ�� �Ǿ����� �˸� �� �ִ� �ݹ�.
    private Action<TabCell> _onClick = null;

    protected virtual void Start()
    {
        var button = GetComponent<Button>();
        if ( button == null )
        {
            Debug.Log($"TabCell : ��ư�� ��ã��!");
        }

        button?.onClick.AddListener(onClick);

    }

    public void setClickCallback(Action<TabCell> callback)
    {
        _onClick = callback;
    }

    // TabCell�� button�� Ŭ�� �Ǿ��� ��
    protected virtual void onClick()
    {
        _onClick?.Invoke(this);
    }
}
