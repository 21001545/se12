using Festa.Client.Module.UI;

namespace DRun.Client
{
    public class UIPanelOpenParam_Levelup : UIPanelOpenParam
    {
        public WhereLevelUp WhereLevelUp;
        public int pfpTokenID;
        public System.Action afterLevelUp;
    }
}