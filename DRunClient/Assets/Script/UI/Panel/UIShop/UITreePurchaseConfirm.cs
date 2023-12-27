using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UITreePurchaseConfirm : UIPanel
{
    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private Image img_icon;

    [SerializeField]
    private TMP_Text txt_desc;

    [SerializeField]
    private TMP_Text txt_cost;

    private UnityAction _callback;

    public void onClickPurchase()
    {
        _callback?.Invoke();
        close();
    }

    public static UITreePurchaseConfirm spawn(RefTree tree, RefShopItem shopItem, UnityAction clickOK = null)
    {
        UITreePurchaseConfirm popup = UIManager.getInstance().spawnInstantPanel<UITreePurchaseConfirm>();

        var sc = GlobalRefDataContainer.getStringCollection();
        var name = sc.get("Tree.Name", tree.id);
        popup.txt_name.text = name;
        popup.txt_cost.text = $"{shopItem.cost}";
        popup.img_icon.sprite = Resources.Load<Sprite>(tree.shop_thumbnail);
        popup.txt_desc.text = sc.getFormat("tree.purchase.popup.desc", 0, name);
        popup._callback = clickOK;

        return popup;
    }

}
