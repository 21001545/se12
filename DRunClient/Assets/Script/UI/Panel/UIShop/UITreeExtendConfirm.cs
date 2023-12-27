using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITreeExtendConfirm : UIPanel
{
    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private Image img_icon;

    [SerializeField]
    private TMP_Text txt_desc;

    [SerializeField]
    private TMP_Text txt_cost;

    [SerializeField]
    private RectTransform rt_time;

    [SerializeField]
    private Slider slider_time;

    [SerializeField]
    private TMP_Text txt_time;

    private UnityAction _callback;

    public void onClickPurchase()
    {
        _callback?.Invoke();
        close();
    }

    public static UITreeExtendConfirm spawn(RefTree tree, RefShopItem shopItem, ClientTree treeData, UnityAction clickOK = null)
    {
        UITreeExtendConfirm popup = UIManager.getInstance().spawnInstantPanel<UITreeExtendConfirm>();

        var sc = GlobalRefDataContainer.getStringCollection();
        var name = sc.get("Tree.Name", tree.id);
        popup.txt_name.text = name;
        popup.txt_cost.text = $"{shopItem.cost}";
        popup.img_icon.sprite = Resources.Load<Sprite>(tree.shop_thumbnail);
        popup.txt_desc.text = sc.getFormat("tree.purchase.popup.desc", 0, name);

        int configDurationUnitTime = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Tree.duration_unit_time, 1440);
        TimeSpan diff = treeData.expire_time - DateTime.UtcNow;

        if (diff.TotalSeconds > 0 )
        {
            if (configDurationUnitTime < 1440)
            {
                popup.txt_time.text = $"{(int)diff.TotalMinutes} minutes left";
            }
            else
            {
                popup.txt_time.text = $"{(int)diff.TotalDays} days left";
            }
            popup.slider_time.value = treeData.remain_time / tree.available_duration;
        }
        else
        {
            popup.rt_time.gameObject.SetActive(false);
        }

        popup._callback = clickOK;

        return popup;
    }

}
