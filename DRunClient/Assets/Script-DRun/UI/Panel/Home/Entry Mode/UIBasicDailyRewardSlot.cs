using System;
using DRun.Client.Logic.BasicMode;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.RefData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
    public class UIBasicDailyRewardSlot : MonoBehaviour
    {
        public GameObject[] pages;
        public TMP_Text text_requireStep;
        public TMP_Text text_reward;
        public UILoadingButton btn_claim;

        public int slotID;

        public event System.Action onClaimReward;

        BasicModeViewModel ViewModel => ClientMain.instance.getViewModel().BasicMode;
        RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        public void onUpdateViewModel(object obj)
        {
            ClientBasicDailyRewardSlot slot = ViewModel.DailyReward[slotID];
            RefEntry entry = GlobalRefDataContainer.getInstance().get<RefEntry>(slotID);

            for (int i = 0; i < pages.Length; ++i)
            {
                pages[i].SetActive((i + 1) == slot.status);
            }

            text_requireStep.text =
                StringCollection.getFormat("basic.dailyrewards.condition", 0, entry.required_steps.ToString("N0"));
            text_reward.text = StringCollection.getFormat("basic.weeklyrewards.finish.desc", 0,
                StringUtil.toDRNStringDefault(slot.reward_count));
        }

        public void onClick_Claim()
        {
            btn_claim.beginLoading();

            ClaimDailyRewardProcessor step = ClaimDailyRewardProcessor.create(slotID);
            step.run(result =>
            {
                btn_claim.endLoading();

                if (result.succeeded())
                {
                    UIClaimDailyReward.getInstance().open(step.getBoxReward(), onClaimReward);
                }
            });
        }
    }
}