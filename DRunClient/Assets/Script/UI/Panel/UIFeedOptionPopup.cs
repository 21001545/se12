using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFeedOptionPopup : UIPanel
{
    [SerializeField]
    private GameObject go_optionPopupMyMenu;

    [SerializeField]
    private GameObject go_optionPopupFriendMenu;

    private ClientNetwork Network => ClientMain.instance.getNetwork();

    private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

    // 옵션창에서 타겟이 되는 피드.
    private ClientFeed _selectFeed = null;

    public static UIFeedOptionPopup spawn(ClientFeed target)
    {
        UIFeedOptionPopup popup = UIManager.getInstance().spawnInstantPanel<UIFeedOptionPopup>();

        popup.setup(target);

        return popup;
    }

    public void onClickFeedReport()
    {
        onClickOptionPopupCancel();
    }

    public void onClickFeedEdit()
    {
        if (_selectFeed == null)
            return;

        if (_selectFeed.object_owner_account_id != Network.getAccountID())
        {
            return;
        }

        ViewModel.MakeMoment.reset(MakeMomentViewModel.EditMode.modify, _selectFeed._moment);

        UIMakeMomentCommit.getInstance().open();
        var stack = ClientMain.instance.getPanelNavigationStack().push(UIFeed.getInstance(), UIMakeMomentCommit.getInstance());
        stack.addPrev(UIMainTab.getInstance());
        onClickOptionPopupCancel();
    }

    public void onClickFeedDelete()
    {
        if (_selectFeed == null)
            return;

        if (_selectFeed.object_owner_account_id != Network.getAccountID())
        {
            return;
        }

        close();
        var sc = GlobalRefDataContainer.getInstance().getStringCollection();
        UIPopup.spawnDeleteCancel(sc.get("moment.feed.delete.popup.title", 0), sc.get("moment.feed.delete.popup.desc", 0), () =>
        {
            DeleteMomentProcessor step = DeleteMomentProcessor.create(_selectFeed._moment.id);
            step.run(result => { });

            onClickOptionPopupCancel();
        });
    }

    public void onClickOptionPopupCancel()
    {
        _selectFeed = null;
        close();
    }

    public void setup(ClientFeed feed)
    {
        _selectFeed = feed;

        go_optionPopupMyMenu.SetActive(feed.object_owner_account_id == Network.getAccountID());
        go_optionPopupFriendMenu.SetActive(feed.object_owner_account_id != Network.getAccountID());
    }
}
