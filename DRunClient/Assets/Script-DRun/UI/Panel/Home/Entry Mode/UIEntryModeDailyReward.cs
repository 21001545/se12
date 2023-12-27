using System;
using UnityEngine;
using DRun.Client.Logic.BasicMode;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.RefData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace DRun.Client
{
    public class UIEntryModeDailyReward : MonoBehaviour
    {
        #region fields

        #region resources

        [Space(10)]
        [Header("0 - enabled / 1 - disabled icon")]
        [SerializeField]
        private Sprite[] _enable_disable_icons;

        #endregion resources

        #region ui refs

        [SerializeField]
        private UISwitcher _statusSwitcher;

        [Space(10)]
        [SerializeField]
        private UILoadingButton _btn_claim;

        [SerializeField]
        private TMP_Text _txt_required_steps;

        [SerializeField]
        private TMP_Text _txt_claimed_rewards;

        [SerializeField]
        private Image _img_normal_icon;

        [SerializeField]
        private ProceduralImage _img_fill_bg;

        [SerializeField]
        private Graphic _outline;

        #endregion ui refs

        #region events

        public event Action onSwitchState;

        public event Action onNotifyAllDailyRewardsRecived;

        #endregion events

        #region data

        private static BasicModeViewModel ViewModel => ClientMain.instance.getViewModel().BasicMode;
        private static RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        [Space(20)]
        [Header("Debug")]
        [SerializeField]
        [ReadOnly]
        private int _required_steps = 0;

        [SerializeField]
        [ReadOnly]
        private string _assignedRewardAmountInDRN;

        private static int _currentSlotStage = 0;

        [SerializeField]
        private int _order;

        private int SafeOrder => _order + 1;

        #endregion data

        #endregion fields

        #region behaviours

        private void switchTo(int state)
        {
            switch (state)
            {
                case ClientBasicDailyRewardSlot.Status.none:
                {
                    // 무조건 가장 작은 Order 가 설정되도록.
                    _currentSlotStage = Mathf.Min(_currentSlotStage, _order);
                    toggleOutlineAndActiveIcon(_currentSlotStage == _order);
                    _statusSwitcher.@switch(0);
                }
                    break;

                case ClientBasicDailyRewardSlot.Status.wait_claim:
                {
                    toggleOutlineAndActiveIcon(false);
                    ++_currentSlotStage;
                    _statusSwitcher.@switch(1);
                }
                    break;

                case ClientBasicDailyRewardSlot.Status.claimed:
                {
                    _statusSwitcher.@switch(2);
                    ++_currentSlotStage;
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            onSwitchState?.Invoke();
        }

        public void updateDailyReward(object _)
        {
            ClientBasicDailyRewardSlot slot = ViewModel.DailyReward[SafeOrder];
            switch (slot.status)
            {
                case 1: // 일반 상태
                    this.switchTo(ClientBasicDailyRewardSlot.Status.none);
                    break;

                case 2: // 보상 받을 수 있는 상태 
                    this.switchTo(ClientBasicDailyRewardSlot.Status.wait_claim);
                    break;

                case 3: // Already Claimed 처리
                    this.switchTo(ClientBasicDailyRewardSlot.Status.claimed);
                    break;
            }

            var entry = GlobalRefDataContainer.getInstance().get<RefEntry>(SafeOrder);

            _required_steps = entry.required_steps;
            _txt_required_steps.text = StringCollection.getFormat(
                "basic.dailyrewards.condition",
                0,
                _required_steps.ToString("N0")
            );

            _assignedRewardAmountInDRN = StringUtil.toDRNStringDefault(slot.reward_count);
            _txt_claimed_rewards.text = StringCollection.getFormat(
                "basic.weeklyrewards.finish.desc",
                0,
                _assignedRewardAmountInDRN
            );
        }

        /// <summary>
        /// normal 상태 버튼의 배경을 채우기 (아래 -> 위 로 차오르는 Fill)
        /// </summary>
        /// <param name="stepCount">계산: 각 order 별 min: basis count / max: next step count</param>
        /// <exception cref="IndexOutOfRangeException">Order [0, 2]</exception>
        public void updateStepCounts(int stepCount)
        {
            // divided by 0 막기.
            if (Mathf.Approximately(_required_steps, 0))
            {
                Debug.LogWarning ("[Divided by 0][UI Entry Mode Daily Reward] required steps (슬롯당 필요 걸음 수) 이 0 !!");
                return;
            }

            // slot 에서 max 걸음 수 보다 큰 값이 들어오면 더 이상 업데이트 안되게 막음.
            // if (stepCount >= _required_steps)
            // {
            //     _stopUpdatingStepCounts = true;
            //
            //     this.switchTo(ClientBasicDailyRewardSlot.Status.wait_claim);
            //     ++currentOrder;
            //     Debug.Log("current order: " + currentOrder);
            //     
            //     _img_normal_icon.sprite = _enable_disable_icons[1];
            //
            //     return;
            // }

            // 1: 3000 max -> 2500 / 3000 -> 0.83
            // 2: 6000 max -> 2500 / 6000 -> 0.41
            // 3: 10000 max -> 2500 / 10000 -> 0.25
            float calculatedFillAmount = (float)stepCount / _required_steps;
            _img_fill_bg.fillAmount = calculatedFillAmount;
        }

        public void onClick_Claim()
        {
			_btn_claim.beginLoading();

            var step = ClaimDailyRewardProcessor.create(SafeOrder);
            step.run(result =>
            {
                _btn_claim.endLoading();

                if (result.succeeded())
                    UIClaimDailyReward.getInstance().open( step.getBoxReward(), onCloseClaimReward);
            });

			void onCloseClaimReward()
			{
				switchTo(ClientBasicDailyRewardSlot.Status.claimed);
				if (ViewModel.DailyRewardAllComplete)
				{
					onNotifyAllDailyRewardsRecived?.Invoke();
				}
			}
		}

        private void toggleOutlineAndActiveIcon(bool isEnable)
        {
            if (isEnable)
            {
                _img_normal_icon.sprite = _enable_disable_icons[0];
                _outline.gameObject.SetActive(true);
            }
            else
            {
                _img_normal_icon.sprite = _enable_disable_icons[1];
                _outline.gameObject.SetActive(false);
            }
        }

        #endregion behaviours
    }
}