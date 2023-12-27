using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using TMPro;
using Festa.Client;

public class UIContactFriend_contactFriendCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_invitation;

    public void setup(int count)
    {
        txt_invitation.text = GlobalRefDataContainer.getStringCollection().getFormat("friend.find.contact.invite.count", 0, count.ToString("N0"));
    }
}
