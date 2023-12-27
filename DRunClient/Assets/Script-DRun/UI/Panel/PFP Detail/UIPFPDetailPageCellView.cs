using DRun.Client.Module;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.RefData;
using System;
using DRun.Client.ViewModel;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DRun.Client.NetData;

namespace DRun.Client
{
    public sealed class UIPFPDetailPageCellView : EnhancedScrollerCellView
    {
        #region data

        public float cellHeight;

        [Space(10)]
        [Header("구매 슬라이더 Fill 색상")]
        [SerializeField] 
        private Gradient _stamina_gradient_colors;
        
        [SerializeField] 
        private Gradient _experience_gradient_colors;

        #endregion data

        #region resources

        /// <summary>
        /// 0 - stamina(thunder)
        /// 1- experience
        /// </summary>
        [Header("========== Resources ==========")]
        public Sprite[] sprites_icons;

        /// <summary>xAsd
        /// 0 - bronze
        /// 1 - silver
        /// 3 - gold
        /// 4 - platinum
        /// 5 - ultra
        /// </summary>
        public Sprite[] sprites_grade_icons;

        #endregion resources

        #region UI Ref

        [Header("========== Middle Stats Container (HDS, Links) ==========")]
        public GameObject stats_container;

        public GameObject bottom_detail_container;
        public GameObject purchase_top_container;
        public GameObject bottom_purchase_container;
        public GameObject bottom_already_full_container;
        public RectTransform stamina_container;
        public RectTransform stamina_container_br;

        [Space(10)]
        [Header("PFP Stat")]
        public TMP_Text txt_pfp_heart;

        public TMP_Text txt_pfp_max_heart;
        public TMP_Text txt_pfp_distance;
        public TMP_Text txt_pfp_max_distance;
        public TMP_Text txt_pfp_stamina;
        public TMP_Text txt_pfp_max_stamina;
        //public TMP_Text txt_pfp_stamina_br;
        //public TMP_Text txt_pfp_max_stamina_br;

        [Space(10)]
        [Header("========= PFP ==========")]

        public UIPhotoThumbnail pfp_profileImage;
        public UICircleLineFillAnimator exp_gauge_fill_animator;
        public UICircleLinePointMover exp_gauge_point_mover;

        [Space(10)]
        [Header("========== PFP Detail Panel ==========")]
        public TMP_Text txt_pfp_grade_title;
        public Image img_pfp_grade_icon;

        [Space(10)]
        [Header("========== Purchase Stamina / Experience ==========")]
        [Header("Confirm Purchase")]
        public Button btn_confirm_purchase;

        public Image image_logo_btn_confirm_purchase;
        public UIGradientEffect gradient_btn_confirm_purchase;
        public Graphic image_btn_confirm_purchase;

        [Space(10)]
        public Button btn_decrement_charge_amount;
        public Button btn_increment_charge_amount;

        [Space(10)]
        public Image img_purchase_top_icon;
        public TMP_Text txt_purchase_top_label;
        public Slider slider_purchase_amount;
        public UIGradientEffect slider_gradient;
        public TMP_Text txt_selected_purchase_amount;
        public TMP_Text txt_final_purchase_amount;
        public TMP_Text txt_purchase_cost;

        [Space(10)]
        [Header("=========== Already Full ==========")]
        public TMP_Text txt_full_amount;
        public TMP_Text txt_full_alert;

        private UIPFPDetailDataContext DataContext => UIPFPDetail.getInstance().DataContext;
        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

        #endregion UI Ref

        private static RefStringCollection StrCollection => GlobalRefDataContainer.getStringCollection();

        #region behaviour

        bool _initExpGauge = false;

        private void initExpGauge()
        {
            if(_initExpGauge)
            {
                return;
            }

            _initExpGauge = true;
            exp_gauge_fill_animator.init();
            exp_gauge_point_mover.init();
        }

        public override void RefreshCellView()
        {
            setupUI();
        }

        public void setupUI()
        {
            initExpGauge();
//			openDetail();

            // 2022.12.12 이강희
            if (DataContext.MaxHeart == 0)
            {
                txt_pfp_heart.text = "∞";
                txt_pfp_max_heart.text = "";
            }
            else
            {
                // update heart.

                if( DataContext.BonusHeart > 0)
                {
					txt_pfp_heart.text = $"{DataContext.Heart} <color=#E33ABC>+{DataContext.BonusHeart}</color>";
				}
                else
                {
					txt_pfp_heart.text = DataContext.Heart.ToString();
				}


				txt_pfp_max_heart.text = $"/{StrCollection.getFormat("pro.home.stat.heart", 0, DataContext.MaxHeart.ToString())}";
            }

			// update distance.
			string strDistance = StringUtil.toDistanceString(DataContext.Distance / 1000.0).TrimAllZeroWithinFloatingPoints();
			if ( DataContext.BonusDistance > 0)
            {
                string strBonusDistance = StringUtil.toDistanceString(DataContext.BonusDistance / 1000.0).TrimAllZeroWithinFloatingPoints();

                txt_pfp_distance.text = $"{strDistance} <color=#4F92E7>+{strBonusDistance}</color>";
            }
            else
            {
				txt_pfp_distance.text = strDistance;
			}

			string maxDist = StrCollection.getFormat(
				"pro.home.stat.distance",
				0,
				StringUtil.toDistanceString(DataContext.MaxDistance / 1000.0f)
					.TrimAllZeroWithinFloatingPoints()
			);
			txt_pfp_max_distance.text = $"/{maxDist}";

            

            // update stamina.
            txt_pfp_stamina.text = DataContext.Stamina.ToString("N2").TrimAllZeroWithinFloatingPoints();
            txt_pfp_max_stamina.text = $"/{StrCollection.getFormat("pro.home.stat.stamina", 0, DataContext.MaxStamina.ToString("N2").TrimAllZeroWithinFloatingPoints())}";

            // 2023/01/02 윤상
            // drn-398
            // HDS 중 Stamina 표시 width > 87px -> line break 처리
            //if (txt_pfp_stamina.fontSize * txt_pfp_stamina.text.Length +
            //    txt_pfp_max_stamina.fontSize * txt_pfp_max_stamina.text.Length > 87)
            //{
            //    stamina_container.gameObject.SetActive(false);
            //    stamina_container_br.gameObject.SetActive(true);
            //}
            //else
            //{
            //    stamina_container.gameObject.SetActive(true);
            //    stamina_container_br.gameObject.SetActive(false);
            //}

            // Exp Gauge Arch 갱신.
            setExpGauge(DataContext.ExperienceRatio);

            // 충전 UI 최대량
            txt_final_purchase_amount.text = DataContext.MaxPurchaseAmount.ToString();


            // TODO: PFP 총 걸음 수 (Marketplace 구현 이후 다시 보기).
            img_pfp_grade_icon.sprite = DataContext.GradeIcon(sprites_grade_icons);
            txt_pfp_grade_title.text = DataContext.GradeTitle;

            // 소숫점 사용 안함.
            slider_purchase_amount.wholeNumbers = true;
            
            // slider binding.
            // 0.15 초 마다 값 갱신 되도록 최적화.
            // slider_purchase_amount.onValueChanged.AddListener(_ => 
            //     Debouncer.debounce(
            //         this,
            //         () => onValueChanged_PurchaseSlider(_),
            //         0.15f
            //     )
            // );
            slider_purchase_amount.onValueChanged.AddListener(onValueChanged_PurchaseSlider);

            resetPurchaseSlider();
            UIPFPDetailNetwork.setPFPImage(DataContext.TokenId, pfp_profileImage);

            //  this.bindUI();

            switchStateTo(UIPFPDetailState.DetailHome);
        }

        public void setExpGauge(float experienceRatio)
        {
            exp_gauge_fill_animator.transtion(experienceRatio);
            exp_gauge_point_mover.moveWithTransition(experienceRatio);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spriteIdx">0 - thunder / 1 - experience</param>
        private void setPurchaseTopContainerContents(int spriteIdx, string topLabel)
        {
            if (spriteIdx < 0 || spriteIdx > 1)
                throw new ArgumentOutOfRangeException("purchase top 에 들어가는 sprite idx <= 1 && idx >= 0!");

            img_purchase_top_icon.sprite = sprites_icons[spriteIdx]; // thunder.
            txt_purchase_top_label.text = topLabel;
        }

        private void resetPurchaseSlider()
        {
            if (!DataContext.SelectedPurchaseAmount.HasValue)
                DataContext.SelectedPurchaseAmount = DataContext.MinPurchaseAmount;

            switch (DataContext.purchasingType)
            {
                case PFPDetailPurchasingType.Stamina:
                {
                    slider_purchase_amount.maxValue = DataContext.MaxStamina * 100;
                    slider_purchase_amount.minValue = DataContext.Stamina * 100;
                    slider_purchase_amount.value = DataContext.Stamina * 100;
                }
                    break;

                case PFPDetailPurchasingType.Experience:
                {
                    slider_purchase_amount.maxValue = DataContext.MaxExperience;
                    slider_purchase_amount.minValue = DataContext.Experience;
                    slider_purchase_amount.value = DataContext.Experience;
                }
                    break;
            }
        }

        private void updatePurchaseUI()
        {
            if (!DataContext.SelectedPurchaseAmount.HasValue)
                return;

            float selectedAmount = DataContext.SelectedPurchaseAmount.Value!;

            bool isDecrementButtonInteractable = selectedAmount > DataContext.MinPurchaseAmount;
            bool isIncrementButtonInteractable = selectedAmount < DataContext.MaxPurchaseAmount;

            btn_decrement_charge_amount.interactable = isDecrementButtonInteractable;
            btn_increment_charge_amount.interactable = isIncrementButtonInteractable;

            // TODO: 2023/01/03 - 윤상
            // 구매 버튼 활성화
            // 구매할 양이 범위 안 (구매 이전 값 <= 구매할 양 <= 최대값 (100))
            // 3. 구매에 필요한 금액이 0.00232 로 소수점 2자리 잘림이 되어 표시가 안되면 구매 안되게 일단 막음.
            bool isPurchaseButtonInteractable = selectedAmount >= DataContext.MinPurchaseAmount&& 
                                                selectedAmount <= DataContext.MaxPurchaseAmount &&
                                                DataContext.FinalPurchaseAmount > 0.0f;

            if (isPurchaseButtonInteractable)
            {
                btn_confirm_purchase.interactable = true;
                
                image_btn_confirm_purchase.color = Color.white;
                txt_purchase_cost.color = Color.white;
                gradient_btn_confirm_purchase.enabled = true;
                image_logo_btn_confirm_purchase.color = Color.white;
            }
            else
            {
                btn_confirm_purchase.interactable = false;
                
                Color imageDisabledColor = UIStyleDefine.ColorStyle.gray700;
                imageDisabledColor.a = 0.5f;
                image_btn_confirm_purchase.color = imageDisabledColor;

                Color textDisabledColor = UIStyleDefine.ColorStyle.gray200;
                textDisabledColor.a = 0.5f;
                txt_purchase_cost.color = textDisabledColor;
                
                gradient_btn_confirm_purchase.enabled = false;
                image_logo_btn_confirm_purchase.color = textDisabledColor;
            }

            // 구매 가격 text 갱신.
            txt_purchase_cost.text =
                StrCollection.getFormat("pro.purchase", 0, DataContext.PurchaseCostAsStr);
            
            //Debug.Log($"최종 구매 가격: {DataContext.PurchaseCostAsStr} / 표시 가격: {txt_purchase_cost.text}");

            // 충전 바의 현재, 충전 후 수치의 텍스트 갱신
            if (DataContext.purchasingType == PFPDetailPurchasingType.Stamina)
            {
                txt_selected_purchase_amount.text = selectedAmount.ToString("N2");
                txt_final_purchase_amount.text = DataContext.MaxPurchaseAmount.ToString("N2");
            }
            else
            {
                // 반올림 이슈가 있네
                txt_selected_purchase_amount.text = StringUtil.toDistanceString(selectedAmount / 1000.0);
                txt_final_purchase_amount.text = (DataContext.MaxPurchaseAmount / 1000.0).ToString("N2");
            }
        }

        public void onClick_RechargeStamina()
        {
            if (DataContext.IsStaminaAlreadyFull)
            {
                switchStateTo(UIPFPDetailState.StaminaFull);

                DataContext.purchasingType = PFPDetailPurchasingType.Stamina;
                setPurchaseTopContainerContents(0, DataContext.selectPurchaseTopLabel);

                txt_full_amount.text = DataContext.Stamina.ToString("N2");
                txt_full_alert.text = DataContext.selectAlreadyFullLabel;
                
                return;
            }

            switchStateTo(UIPFPDetailState.ChargeStamina);

            DataContext.purchasingType = PFPDetailPurchasingType.Stamina;
            setPurchaseTopContainerContents(0, DataContext.selectPurchaseTopLabel);
            
            // 슬라이더 fill 색상 stamina gradient 로 설정.
            slider_gradient.gradient = _stamina_gradient_colors;

            // 처음에 한번 매핑
            resetPurchaseSlider();
            updatePurchaseUI();
        }

        public void onClick_BoostExp()
        {
            // 만렙
            if (DataContext.IsMaxLevel)
            {
                UIPopup.spawnOK(DataContext.selectLevelUpMaxLevelPopupDesc);
                return;
            }

            // Already Full
            if (DataContext.IsExperienceAlreadyFull)
            {
                switchStateTo(UIPFPDetailState.ExperienceFull);
                DataContext.purchasingType = PFPDetailPurchasingType.Experience;
                setPurchaseTopContainerContents(1, DataContext.selectPurchaseTopLabel);

                txt_full_amount.text = (DataContext.Experience / 1000.0).ToString("N2");
                txt_full_alert.text = DataContext.selectAlreadyFullLabel;
                return;
            }
            
            switchStateTo(UIPFPDetailState.PurchaseExperience);

            DataContext.purchasingType = PFPDetailPurchasingType.Experience;
            setPurchaseTopContainerContents(1, DataContext.selectPurchaseTopLabel);
            
            // 슬라이더 fill 색상 stamina gradient 로 설정.
            slider_gradient.gradient = _experience_gradient_colors;
            
            // 처음에 한번 매핑
            resetPurchaseSlider();
            updatePurchaseUI();
        }

        public void onClick_ClosePurchase()
        {
            switchStateTo(UIPFPDetailState.DetailHome);
        }

        public void onValueChanged_PurchaseSlider(float _)
        {
            // 경험치 2342 -> 2.342 가 되므로 1의 자리 자름.

            float val = 0.0f;

            if (DataContext.purchasingType == PFPDetailPurchasingType.Experience)
            {
                val = slider_purchase_amount.value;
                // 1의 자리 자르기
                val -= (val % 10);
            }
            else
            {
                val = slider_purchase_amount.value / 100;
            }
            
            //Debug.Log($"원래 값: {slider_purchase_amount.value} -> 자른 값: {val}");
            
            DataContext.SelectedPurchaseAmount = Mathf.Max(DataContext.MinPurchaseAmount, val);
            
            updatePurchaseUI();
        }

        public void onClick_Slider_Increase()
        {
            DataContext.increaseTargetAmountToPurchase();

            if(!DataContext.SelectedPurchaseAmount.HasValue)
            {
                DataContext.SelectedPurchaseAmount = DataContext.MinPurchaseAmount;
            }

            switch (DataContext.purchasingType) {
                case PFPDetailPurchasingType.Experience:
                    slider_purchase_amount.value
                        = DataContext.SelectedPurchaseAmount.Value;
                    break;

                case PFPDetailPurchasingType.Stamina:
                    slider_purchase_amount.value
                        = DataContext.SelectedPurchaseAmount.Value * 100;
                    break;
            }

            updatePurchaseUI();
        }

        public void onClick_Slider_Decrease()
        {
            DataContext.decreaseTargetAmountToPurchase();

            if (!DataContext.SelectedPurchaseAmount.HasValue)
            {
                DataContext.SelectedPurchaseAmount = DataContext.MinPurchaseAmount;
            }

            switch (DataContext.purchasingType) {
                case PFPDetailPurchasingType.Experience:
                    slider_purchase_amount.value
                        = DataContext.SelectedPurchaseAmount.Value;
                    break;

                case PFPDetailPurchasingType.Stamina:
                    slider_purchase_amount.value
                        = DataContext.SelectedPurchaseAmount.Value * 100;
                    break;
            }
            updatePurchaseUI();
        }

        public void onClick_ConfirmPurchase()
        {
            // 잔액 부족 처리
            double drnBalance = StringUtil.toDRN(ViewModel.Wallet.DRN_Balance.balance);
            if( drnBalance < DataContext.PurchaseCost)
            {
                UIPopup.spawnOK(
                    StrCollection.get("pro.purchase.not_enough_drn.title", 0),
                    StrCollection.get("pro.purchase.not_enough_drn.desc", 0)
                );
                return;
            }

            if( DataContext.purchasingType == PFPDetailPurchasingType.Stamina)
            {
                purchaseStamina();
            }
            else if( DataContext.purchasingType == PFPDetailPurchasingType.Experience)
            {
                purchaseEXP();
            }
        }

        private void purchaseStamina()
        {
            int finalPurchaseAmount = Mathf.RoundToInt(DataContext.FinalPurchaseAmount * 100);
            UIPFPDetailNetwork.rechargeNFTStamina(DataContext.NFTItem, finalPurchaseAmount,
                onSucceed: handler =>
                {
                    DataContext.NFTItem = ViewModel.Wallet.getNFTItem(DataContext.TokenId);
                    setupUI();

                    UIPFPPurchaseResult.getInstance().open(DataContext.purchasingType, (DataContext.Stamina).ToString("N2"));
                },
                onFail: handler =>
                {
                    UIPopup.spawnError(StrCollection.get("pro.purchase.fail.charge_stamina", 0));
                    switchStateTo(UIPFPDetailState.DetailHome);
                });
        }

        private void purchaseEXP()
        {
            int finalPurchaseAmount = (int)DataContext.FinalPurchaseAmount;
            UIPFPDetailNetwork.purchaseExperience(DataContext.NFTItem, finalPurchaseAmount,
                onSucceed: handler => {
                    DataContext.NFTItem = ViewModel.Wallet.getNFTItem(DataContext.TokenId);
                    setupUI();

                    UIPFPDetail.getInstance().setupTopUI();

                    UIPFPPurchaseResult.getInstance().open(DataContext.purchasingType, StringUtil.toDistanceString((DataContext.Experience / 1000.0)));
                },
                onFail: handler => {
                    UIPopup.spawnError(StrCollection.get("pro.purchase.fail.charge_stamina", 0));
                    switchStateTo(UIPFPDetailState.DetailHome);
                });
        }

        public void onClick_CloseAlreadyFulll()
        {
            switchStateTo(UIPFPDetailState.DetailHome);
        }

        #endregion behaviour

        #region switch states

        public void switchStateTo(UIPFPDetailState state)
        {
            switch (state)
            {
                case UIPFPDetailState.DetailHome:
                {
                    stats_container.SetActive(true);
                    bottom_detail_container.SetActive(true);
                    purchase_top_container.SetActive(false);
                    bottom_purchase_container.SetActive(false);
                    bottom_already_full_container.SetActive(false);
                    //btn_close_PFP_detail.gameObject.SetActive(true);
                }
                    break;

                case UIPFPDetailState.ChargeStamina:
                {
                    stats_container.SetActive(false);
                    bottom_detail_container.SetActive(false);
                    purchase_top_container.SetActive(true);
                    bottom_purchase_container.SetActive(true);
                    bottom_already_full_container.SetActive(false);
                    //btn_close_PFP_detail.gameObject.SetActive(false);
                }
                    break;

                case UIPFPDetailState.PurchaseExperience:
                {
                    stats_container.SetActive(false);
                    bottom_detail_container.SetActive(false);
                    purchase_top_container.SetActive(true);
                    bottom_purchase_container.SetActive(true);
                    bottom_already_full_container.SetActive(false);
                    //btn_close_PFP_detail.gameObject.SetActive(false);
                }
                    break;

                case UIPFPDetailState.StaminaFull:
                {
                    stats_container.SetActive(false);
                    bottom_detail_container.SetActive(false);
                    purchase_top_container.SetActive(true);
                    bottom_purchase_container.SetActive(false);
                    bottom_already_full_container.SetActive(true);
                    //btn_close_PFP_detail.gameObject.SetActive(false);
                }
                    break;

                case UIPFPDetailState.ExperienceFull:
                {
                    stats_container.SetActive(false);
                    bottom_detail_container.SetActive(false);
                    purchase_top_container.SetActive(true);
                    bottom_purchase_container.SetActive(false);
                    bottom_already_full_container.SetActive(true);
                    //btn_close_PFP_detail.gameObject.SetActive(false);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #endregion
    }
}