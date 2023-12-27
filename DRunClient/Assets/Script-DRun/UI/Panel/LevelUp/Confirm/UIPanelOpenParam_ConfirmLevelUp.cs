using Festa.Client.Module.UI;

namespace DRun.Client
{
    public class UIPanelOpenParam_ConfirmLevelUp : UIPanelOpenParam
    {
        public WhereLevelUp whereLevelUp;
        public int tokenLevelup;
        public System.Action afterLevelUp;
        public string formattedPurchaseCost;
        public double levelUpCost;
    }
}