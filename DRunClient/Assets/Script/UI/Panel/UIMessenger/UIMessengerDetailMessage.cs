using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerDetailMessage : UISingletonPanel<UIMessengerDetailMessage>
{
    [SerializeField]
    private TMP_Text txt_text;

    public override void initSingleton(SingletonInitializer initializer)
    {
        base.initSingleton(initializer);
    }

    public void setup(string message)
	{
        txt_text.text = message;
    }
    public void onClickBackNavigation()
    {
        ClientMain.instance.getPanelNavigationStack().pop();
    }
}
