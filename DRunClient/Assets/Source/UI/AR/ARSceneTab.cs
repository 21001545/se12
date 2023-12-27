using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARSceneTab : Tab
{
    [SerializeField]
    private RectTransform _content;

    protected override void onTabClicked(TabCell cell, int index)
    {
        base.onTabClicked(cell, index);

        // 정중앙으로 Tab을 옮기자.
        if (_content != null )
            DOTween.To(() => _content.localPosition, x => _content.localPosition = x, new Vector3(-index * 100, _content.localPosition.y, _content.localPosition.z), 0.5f);
    }
}
