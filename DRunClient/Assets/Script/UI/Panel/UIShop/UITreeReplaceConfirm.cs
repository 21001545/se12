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

public class UITreeReplaceConfirm : UIPanel
{
    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private TMP_Text txt_desc;

    private UnityAction _callback;

    public void onClickPurchase()
    {
        _callback?.Invoke();
        close();
    }

    public static UITreeReplaceConfirm spawn(UnityAction clickOK = null)
    { 
        UITreeReplaceConfirm popup = UIManager.getInstance().spawnInstantPanel<UITreeReplaceConfirm>();

        popup._callback = clickOK;

        return popup;
    }

}
