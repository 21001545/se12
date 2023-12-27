using Festa.Client;
using Festa.Client.RefData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMakeMomentTagSelectItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text txt_tag;

    [SerializeField]
    private Image img_icon;

    [SerializeField]
    private GameObject img_select;

    private RefMomentLifeLog _log;

    public int getLogID()
	{
        return _log.id;
	}

    public void Initialize(RefMomentLifeLog log)
    {
        _log = log;
        var sc = GlobalRefDataContainer.getStringCollection();
        txt_tag.text = sc.get("RefMomentLifeLog.id", log.id);

        if (string.IsNullOrEmpty(log.icon) == false)
        {
            var sprite = Resources.Load<Sprite>($"UI/Tag/{log.icon}");
            if (sprite != null)
                img_icon.sprite = sprite;
        }
    }

    public void SetSelect(bool select)
    {
        img_select.SetActive(select);
    }
}
