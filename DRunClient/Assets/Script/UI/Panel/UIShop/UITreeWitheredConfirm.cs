using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITreeWitheredConfirm : UIPanel
{
    [SerializeField]
    private TMP_Text txt_desc;

    public static UITreeWitheredConfirm spawn(RefTree tree)
    {
        UITreeWitheredConfirm popup = UIManager.getInstance().spawnInstantPanel<UITreeWitheredConfirm>();

        var sc = GlobalRefDataContainer.getStringCollection();
        popup.txt_desc.text = sc.getFormat("tree.withered.popup.desc", 0, sc.get("Tree.Name", tree.id));

        return popup;
    }

    public void onClickConfirm()
    {
        close();

        UIShop.getInstance().open();

        UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIShop.getInstance());
        stack.addPrev(UIMainTab.getInstance());
    }
}
