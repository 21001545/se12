using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TabCell : MonoBehaviour
{
    // 이 TabCell이 클릭 되었음을 알릴 수 있는 콜백.
    private Action<TabCell> _onClick = null;

    protected virtual void Start()
    {
        var button = GetComponent<Button>();
        if ( button == null )
        {
            Debug.Log($"TabCell : 버튼을 못찾음!");
        }

        button?.onClick.AddListener(onClick);

    }

    public void setClickCallback(Action<TabCell> callback)
    {
        _onClick = callback;
    }

    // TabCell의 button이 클릭 되었을 때
    protected virtual void onClick()
    {
        _onClick?.Invoke(this);
    }
}
