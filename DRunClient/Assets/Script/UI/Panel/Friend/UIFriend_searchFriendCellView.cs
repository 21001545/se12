using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using Festa.Client;

public class UIFriend_searchFriendCellView : EnhancedScrollerCellView
{
    public void onClickButton()
    {
        UIAddFriend.getInstance().open();
        // 당연히,, 친구 탭에서 열엇겟지..??
        ClientMain.instance.getPanelNavigationStack().push(UIFriend.getInstance(), UIAddFriend.getInstance());
    }
}
