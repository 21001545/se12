using Festa.Client;
using Festa.Client.RefData;
using PolyAndCode.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMomentTag : MonoBehaviour
{
    [SerializeField]
    private Image img_tag;

    [SerializeField]
    private TMP_Text txt_tag;

    public int Tag = 1;
    public int Index = 0;
    public void Initialize(int index, int tag)
    {
        Index = index;
        Tag = tag;
        var sc = GlobalRefDataContainer.getStringCollection();
        txt_tag.text = sc.get("RefMomentLifeLog.id", tag);
        RefMomentLifeLog log = (RefMomentLifeLog)GlobalRefDataContainer.getInstance().getMap<RefMomentLifeLog>()[tag];

        if (string.IsNullOrEmpty(log.icon) == false)
        {
            var sprite = Resources.Load<Sprite>($"UI/Tag/{log.icon}");
            if (sprite != null)
                img_tag.sprite = sprite;
        }
    }

    public void onClickTag()
    {
        UIMakeMomentTagSelect.spawn(Tag, (int tag) =>
        {
            ClientMain.instance.getViewModel().MakeMoment.TagList[Index] = tag;
            Initialize(Index, tag);
        });
    }

    public void onClickRemove()
    {
        var tagList = ClientMain.instance.getViewModel().MakeMoment.TagList;
        tagList.RemoveAt(Index);
        // 음.. 강제 갱신 어떻게 해볼까?
        ClientMain.instance.getViewModel().MakeMoment.TagList = new List<int>(tagList);
    }
}
