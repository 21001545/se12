using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMomentCommentViewRepliesCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_count;

    UnityAction<ClientMomentComment> _clickCallback;
    ClientMomentComment _momentComment;

    public void setup(ClientMomentComment momentComment, UnityAction<ClientMomentComment> clickCallback)
    {
        _momentComment = momentComment;
        _clickCallback = clickCallback;
        var sc = GlobalRefDataContainer.getInstance().getStringCollection();

        if (_momentComment._loaded_sub_count == 0)
            txt_count.text = sc.getFormat("moment.comment.more", 0, _momentComment.sub_count.ToString("N0"));
        else
            txt_count.text = sc.getFormat("moment.comment.more.previous", 0, (_momentComment.sub_count - _momentComment._loaded_sub_count).ToString("N0"));
    }
    
    public void onClick()
    {
        _clickCallback?.Invoke(_momentComment);
    }
}
