using System.Collections;
using System.Collections.Generic;
using Festa.Client.Module.UI;
using UnityEngine;

namespace Festa.Client
{
    public class UICheers : UIPanel
    {
        [SerializeField]
        private UIMap_cheersScroller scroller_cheers;

        public static UICheers spawn()
        {
            UICheers popup = UIManager.getInstance().spawnInstantPanel<UICheers>();
            return popup;
        }

        public override void onTransitionEvent(int type)
        {
            base.onTransitionEvent(type);

            if(type == TransitionEventType.start_open)
            {
                scroller_cheers.setupData();
            }
        }
    }
}