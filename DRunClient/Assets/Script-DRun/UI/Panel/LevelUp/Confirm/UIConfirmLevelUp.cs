using DRun.Client.Logic.BasicMode;
using DRun.Client.NetData;
using DRun.Client.RefData;
using Festa.Client;
using Festa.Client.Module.UI;
using System;
using DRun.Client.Logic.ProMode;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;
using DRun.Client.Module;
using Festa.Client.Module;

namespace DRun.Client
{
    public class UIConfirmLevelUp : UISingletonPanel<UIConfirmLevelUp>
    {
        [SerializeField, ReadOnly]
        private WhereLevelUp _whereLevelUp;

        [SerializeField, ReadOnly]
        private int _tokenLevelup;

        [Space(10)]
        [SerializeField]
        private UILoadingButton btn_entry_levelup;

        [SerializeField]
        private UILoadingButton btn_pro_levelup;

        private Texture[] BasicLevelImageList => ClientMain.instance.basicModeLevelImage.levelList;

        [Space(10)]
        [Header("========== Container ==========")]

        [SerializeField]
        private GameObject _entryModeContainer;

        [SerializeField]
        private GameObject _proModeContainer;

        [Space(10)]
        [Header("========== Entry mode ==========")]
        [SerializeField]
        private TMP_Text _txt_entry_title;

        [Header("current level")]
        [SerializeField]
        private UIPhotoThumbnail entry_current_portarit;

        [SerializeField]
        private TMP_Text txt_entry_current_level;

        [SerializeField]
        private TMP_Text txt_entry_current_bonus;

        [Header("next level")]
        [SerializeField]
        private UIPhotoThumbnail entry_next_portrait;

        [SerializeField]
        private TMP_Text txt_entry_next_level;

        [SerializeField]
        private TMP_Text txt_entry_next_bonus;

        [Header("========== Pro mode ==========")]
        [SerializeField]
        private TMP_Text _txt_pro_title;

        [SerializeField]
        private TMP_Text[] _txt_pro_bonus_labels;

        [SerializeField]
        private TMP_Text _txt_pro_confirmButton_label;

        [Header("current level")]
        [SerializeField]
        TMP_Text _txt_pro_current_level;

        [SerializeField]
        TMP_Text _txt_pro_current_bonus;

        [Header("next level")]
        [SerializeField]
        TMP_Text _txt_pro_next_level;

        [SerializeField]
        TMP_Text _txt_pro_next_bonus;

        private Action _afterLevelUp;
        private string _formattedPurchaseCost;
        private double _levelUpCost;

        private RefStringCollection StrCollection => GlobalRefDataContainer.getStringCollection();
        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        
        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        {
            base.open(param, transitionType, closeType);

            if (param is UIPanelOpenParam_ConfirmLevelUp levelUpParam)
            {
                switch (levelUpParam.whereLevelUp)
                {
                    case WhereLevelUp.EntryMode:
                    {
                        _whereLevelUp = levelUpParam.whereLevelUp;
                        _tokenLevelup = levelUpParam.tokenLevelup;
                        _afterLevelUp = levelUpParam.afterLevelUp;
                    }
                        break;

                    case WhereLevelUp.ProMode:
                    {
                        _whereLevelUp = levelUpParam.whereLevelUp;
                        _tokenLevelup = levelUpParam.tokenLevelup;
                        _afterLevelUp = levelUpParam.afterLevelUp;
                        _formattedPurchaseCost = levelUpParam.formattedPurchaseCost;
                        _levelUpCost = levelUpParam.levelUpCost;
                    }
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                

            }

            setupUI(_whereLevelUp);
        }

        private void setupUI(WhereLevelUp whereLevelUp)
        {
            switch (whereLevelUp)
            {
                case WhereLevelUp.EntryMode:
                {
                    _entryModeContainer.SetActive(true);
                    _proModeContainer.SetActive(false);

                    _txt_entry_title.text = StrCollection.get("basic.levelup.button", 0);
                    // _txt_entry_bonus_labels.text = StrCollection

                    ClientBasicMode levelData = ClientMain.instance.getViewModel().BasicMode.LevelData;
                    RefEntryLevel refLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(levelData.level);
                    RefEntryLevel refNextLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(levelData.level + 1);

                    entry_current_portarit.setImage((Texture2D)BasicLevelImageList[levelData.level - 1], true);
                    txt_entry_current_level.text = $"Lv.{levelData.level}";
                    txt_entry_current_bonus.text = $"+{refLevel.bonus_percent}%";

                    entry_next_portrait.setImage((Texture2D)BasicLevelImageList[levelData.level], true);
                    txt_entry_next_level.text = $"Lv.{levelData.level + 1}";
                    txt_entry_next_bonus.text = $"+{refNextLevel.bonus_percent}%";
                }
                    break;

                case WhereLevelUp.ProMode:
                {
                    _entryModeContainer.SetActive(false);
                    _proModeContainer.SetActive(true);

                    _txt_pro_title.text = StrCollection.get("pro.levelup.popup.title", 0);
                        
                    // 윤상 2022.12.13 : 기존에 확인 -> 가격표시로 변경.
                    // _txt_pro_confirmButton_label.text = StrCollection.get("pro.levelup.popup.button", 0);
                    string bonusLabel = StrCollection.get("pro.levelup.popup.desc", 0);
                    foreach (TMP_Text label in _txt_pro_bonus_labels)
                        label.text = bonusLabel;

                    var proModeViewModel = ClientMain.instance.getViewModel().ProMode;
                    int level = proModeViewModel.EquipedNFTItem.level;
                    var refLevel = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(level);
                    var refNextLevel = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(level + 1);

                    _txt_pro_current_level.text = $"Lv.{level}";
                    _txt_pro_current_bonus.text = $"+{refLevel.mining_bonus_percent}%";

                    _txt_pro_next_level.text = $"Lv.{level + 1}";
                    _txt_pro_next_bonus.text = $"+{refNextLevel.mining_bonus_percent}%";
                    _txt_pro_confirmButton_label.text = _formattedPurchaseCost;
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(whereLevelUp), whereLevelUp, null);
            }
        }

        public void onClick_Background() => base.close();

        public void onClick_Confirm()
        {
            switch (_whereLevelUp)
            {
                case WhereLevelUp.EntryMode:
                {
                    btn_entry_levelup.beginLoading();
                    LevelUpProcessor.create()
                        .run(result =>
                        {
                            btn_entry_levelup.endLoading();

                            if (result.succeeded())
                            {
                                UILevelUp.getInstance().open(new UIPanelOpenParam_Levelup
                                {
                                    WhereLevelUp = _whereLevelUp,
                                    afterLevelUp = _afterLevelUp
                                });
                            }
                            else
                            {
                                UIPopup.spawnError(StrCollection.get("pro.levelup.fail", 0));
                            }
                        });
                }
                    break;

                case WhereLevelUp.ProMode:
                {
                    if( checkBalance() == false)
                    {
                        return;
                    }

                    int tokenID = _tokenLevelup;

                    btn_pro_levelup.beginLoading();
                    LevelUpNFTProcessor.create(tokenID)
                        .run(result =>
                        {
                            btn_pro_levelup.endLoading();

                            if (result.succeeded())
                            {
                                _afterLevelUp?.Invoke();
                                    
                                UILevelUp.getInstance().open(
                                    new UIPanelOpenParam_Levelup
                                    {
                                        WhereLevelUp = _whereLevelUp,
                                        pfpTokenID = tokenID
                                    });
                            }
                            else
                            {
                                UIPopup.spawnError(StrCollection.get("pro.levelup.fail", 0));
                            }
                        });
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool checkBalance()
        {
            double drnBalance = StringUtil.toDRN(ViewModel.Wallet.DRN_Balance.balance);
            if (drnBalance < (double)_levelUpCost)
            {
                UIPopup.spawnError(StrCollection.get("pro.purchase.not_enough_drn.title", 0));
                return false;
            }

            return true;
        }
    }
}