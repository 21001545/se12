using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.RefData;
using Festa.Client;
using Festa.Client.RefData;
using UnityEngine;

namespace DRun.Client
{
    public class UIPFPDetailDataContext
    {
        private ClientNFTItem _nftItem;
        private ClientNFTBonus _nftBonus;

        private static RefStringCollection StrCollection => GlobalRefDataContainer.getStringCollection();

        public UIPFPDetailDataContext(ClientNFTItem nftItem,ClientNFTBonus nftBonus)
        {
            _nftItem = nftItem;
            _nftBonus = nftBonus;
		}

        #region data

        public ClientNFTItem NFTItem {
        
            get
            {
                return _nftItem;
            }
            set
            {
                _nftItem = value;
			}
		}


        #region level

        // Level
        public int Level => _nftItem.level;
        public int NextLevel => this.Level + 1;
        public bool IsMaxLevel => GlobalRefDataContainer.getInstance().get<RefNFTLevel>(NextLevel) is null;

        public float LevelUpCost => GlobalRefDataContainer.getInstance().get<RefNFTLevel>(NextLevel).levelup_drnt;
        public string LevelUpCostAsStr => LevelUpCost.ToString();

        #endregion level

        #region purchase amount

        // Purchase Amount
        // 충전 로직에 있어서 슬라이더, 버튼으로 선택해놓은 값. 
        public float? SelectedPurchaseAmount { get; set; }

        /// <summary>
        /// 슬라이더, 버튼을 조작해서 유저가 구매하고 싶은 만큼의 양. (UI 표시, 가격 산정에도 쓰임).
        /// </summary>
        public float FinalPurchaseAmount => SelectedPurchaseAmount.HasValue
            ? SelectedPurchaseAmount.Value - MinPurchaseAmount
            : 0;

        /// <summary>
        /// 슬라이더 최소값, UI 최소값 표시.
        /// </summary>
        public float MinPurchaseAmount => purchasingType switch
        {
            PFPDetailPurchasingType.Stamina => Stamina,
            PFPDetailPurchasingType.Experience => Experience,
			_ => throw new System.NotImplementedException(),
		};

        /// <summary>
        /// 슬라이더 최대값, UI 최대값 표시.
        /// </summary>
        public float MaxPurchaseAmount => purchasingType switch
		{
			PFPDetailPurchasingType.Stamina => MaxStamina,
			PFPDetailPurchasingType.Experience => MaxExperience,
			_ => throw new System.NotImplementedException(),
		};

        /// <summary>
        /// TODO: 2023/01/03 - 윤상
        /// 현재 drn 구매 시에 2자리 수만 처리하도록 UX 가 잡혀 있는데,
        /// 실제 0.009 -> 0.00 으로 표시되서 이슈가 있음.
        ///
        /// 구매 가격 설정은 기획 결정이 필요 하므로 대기.
        /// 0.00 이후의 가격을 비교하기 위한 임계값.
        /// </summary>
        private const double purchaseCostFloatingDigitEpsilon = 0.00001;
        public double PurchaseCost
        {
            get
            {
                // 슬라이더로 선택한 양이 0 또는 음수 -> 구매 계산 필요 X.
                float purchaseAmount = FinalPurchaseAmount;
                if (purchaseAmount <= 0.0)
                    return 0.0;

                // float fractionalPart = purchaseAmount % 1;
                // if ((int)purchaseAmount == Mathf.RoundToInt(fractionalPart)) {
                //     purchaseAmount = Mathf.Round(purchaseAmount);
                // }
                
                return purchasingType switch
                {
                    PFPDetailPurchasingType.Experience =>
                        GlobalRefDataContainer.getRefDataHelper()
                            .calcBoostExpCost(nftLevel: Level, boostAmount: (int)purchaseAmount),

                    PFPDetailPurchasingType.Stamina =>
                        GlobalRefDataContainer.getRefDataHelper()
                            .calcStaminaCost(
                                nftGrade: Grade, 
                                refllAmount: Mathf.RoundToInt(purchaseAmount * 100) 
                                    // 부동소숫점 차로 발생하는 0.xxxxx9~ 반올림.
                            ),
                                                        
                    
					_ => throw new System.NotImplementedException(),
				};
            }
        }

        public string PurchaseCostAsStr => PurchaseCost.ToString("F20").TrimAllZeroWithinFloatingPoints();
        public PFPDetailPurchasingType purchasingType;

        // TODO: RefString 처리. (고정 recharge change step)
        float PurchaseAmountStepMoveDelta => purchasingType switch {
            PFPDetailPurchasingType.Stamina => 0.01f,
            PFPDetailPurchasingType.Experience => 10
        };
        
        public void increaseTargetAmountToPurchase()
        {
            stepMovePurchaseAmount(PurchaseAmountStepMoveDelta, true);
        }

        public void decreaseTargetAmountToPurchase()
        {
			stepMovePurchaseAmount(PurchaseAmountStepMoveDelta, false);
		}

		private void stepMovePurchaseAmount(float delta,bool plus)
        {
            if (delta > 0 && delta < 1) 
            {
                SelectedPurchaseAmount += plus ? delta : -delta;
                return;
            }
                
			float mod = SelectedPurchaseAmount.Value % delta;

			if ( mod != 0)
            {
				if ( plus)
                {
                    SelectedPurchaseAmount = Mathf.Ceil(SelectedPurchaseAmount.Value + (delta - mod));
                }
                else
                {
                    SelectedPurchaseAmount = Mathf.Floor(SelectedPurchaseAmount.Value - mod);
                }
            }
            else
            {
                SelectedPurchaseAmount += plus ? delta : -delta;
            }
        }

        #endregion purchase amount

        #region PFP

        // PFP Token
        public int TokenId => _nftItem.token_id;

        // Grade
        public int Grade => _nftItem.grade;
        public string GradeTitle => StrCollection.get("pro.detail.grade", this.Grade);

        /// <summary>
        /// 0 - bronze
        /// 1 - silver
        /// 3 - gold
        /// 4 - platinum
        /// 5 - ultra
        /// </summary>
        /// <returns>각 Grade 의 ID 에 맞는 Grade Sprite</returns>
        public Sprite GradeIcon(Sprite[] gradeIcons) => gradeIcons[
			Grade switch
			{
				201 => 0,
				202 => 1,
				203 => 2,
				204 => 3,
				205 => 4,
				_ => throw new System.NotImplementedException()
			}];

        #region Experience

        // Experience
        public int Experience => _nftItem.exp;

        public int MaxExperience => GlobalRefDataContainer.getInstance()
            .get<RefNFTLevel>(NextLevel)
            .required_disance * 1000;

        public float ExperienceRatio => _nftItem.getEXPRatio();
        public bool IsExperienceAlreadyFull => ExperienceRatio >= 1.0f;

        #endregion Experience

        #endregion PFP

        #region HDS

        // Heart
        public int Heart => _nftItem.heart;
        public int MaxHeart => _nftItem.max_heart;
        public int BonusHeart
        {
            get
            {
                if( _nftBonus != null && _nftBonus.nft_item_id == _nftItem.token_id)
                {
                    return _nftBonus.heart;
                }
                else
                {
                    return 0;
                }
            }
        }

        // Distance
        public float Distance => _nftItem.distance;
        public float MaxDistance => _nftItem.max_distance;
        public float DistanceRatio => _nftItem.getDistanceRatio();
        public float BonusDistance
        {
            get
            {
                if( _nftBonus != null && _nftBonus.nft_item_id == _nftItem.token_id)
                {
                    return _nftBonus.distance;
                }
                else
                {
                    return 0;
                }
            }
        }

        // Stamina
        public float Stamina => (float)_nftItem.stamina / 100;

        public float MaxStamina => (float)GlobalRefDataContainer.getRefDataHelper()
            .getNFTGrade(Grade)
            .stamina;

        public float StaminaRatio => _nftItem.getDistanceRatio();
        public bool IsStaminaAlreadyFull => Stamina >= MaxStamina;

        #endregion HDS

        #endregion data

        #region ref string

        public string selectPurchaseTopLabel => purchasingType switch
		{
			PFPDetailPurchasingType.Stamina => StrCollection.get("pro.purchase.title", 0),
			PFPDetailPurchasingType.Experience => StrCollection.get("pro.purchase.title", 1),
			_ => throw new System.NotImplementedException()
		};

        public string selectAlreadyFullLabel => purchasingType switch
		{
			PFPDetailPurchasingType.Stamina => StrCollection.get("pro.alreadyfull.label.desc", 0),
			PFPDetailPurchasingType.Experience => StrCollection.get("pro.alreadyfull.label.desc", 1),
			_ => throw new System.NotImplementedException()
		};

        public string selectLevelUpMaxLevelPopupDesc => StrCollection.get("pro.levelup.desc.max_level", 0);

        #endregion ref string
    }
}