using DRun.Client.NetData;

using Festa.Client.Module.UI;

namespace Script.Module.UI
{
    public class UIPanelOpenParam_PFPDetail : UIPanelOpenParam
    {
        public ClientNFTItem NFTItem
        {
            get => (ClientNFTItem)get("nftItem");
            private set => put("nftItem", value);
        }

        public ClientNFTBonus NFTBonus
        {
            get => (ClientNFTBonus)get("nftBonus");
            private set => put("nftBonus", value);
        }

        public UIAbstractPanel BackPanel
        {
            get => (UIAbstractPanel)get("backPanel");
            private set => put("backPanel", value);
        }

        private void init(ClientNFTItem clientNftItem,ClientNFTBonus nftBonus,UIAbstractPanel BackPanel)
        {
            base.init();
			this.NFTItem = clientNftItem;
            this.NFTBonus = nftBonus;
            this.BackPanel = BackPanel;
        }

        public static UIPanelOpenParam_PFPDetail create(ClientNFTItem newNftItem,ClientNFTBonus nftBonus, UIAbstractPanel backPanel)
        {
            var self = new UIPanelOpenParam_PFPDetail();
            self.init( newNftItem, nftBonus, backPanel);
            return self;
        }
    }
}