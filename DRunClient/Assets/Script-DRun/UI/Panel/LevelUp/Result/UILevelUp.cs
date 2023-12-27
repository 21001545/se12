using DRun.Client.NetData;
using DRun.Client.RefData;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System;
using System.Collections;
using Festa.Client.Module;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
    /// <summary>
    /// 12.22.2022 / 윤상 / Entry & Pro 레벨업 둘다 똑같은 디자인 수치, but 색상들만 다름.
    /// </summary>
    public class UILevelUp : UISingletonPanel<UILevelUp>
    {
        #region resources

        [Space(10)]
        [Header("Background Gradient Colors")]
        [SerializeField]
        private Gradient _entry_bg_color;

        [SerializeField]
        private Gradient _pro_bg_color;
        
        [Space(10)]
        [Header("Particle Gradient Colors")]
        [SerializeField]
        private Gradient _entry_particle_color;

        [SerializeField]
        private Gradient _pro_particle_color;

        #endregion resources

        #region Refs

        [Space(20)]
        [SerializeField]
        private UIGradientEffect _bgGradient;

        public ParticleSystem particle;
        public UIPhotoThumbnail profile_image;
        public TMP_Text text_title;
        public TMP_Text text_desc;

        #endregion Refs

        #region data

        private bool _canClose;

        [Space(20)]
        [Header("Debug")]
        [SerializeField]
        [ReadOnly]
        private WhereLevelUp _whereLevelUp;

        [SerializeField]
        [ReadOnly]
        private int _pfpTokenID;

        #endregion data

        private Action _afterLevelUp;

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        {
            base.open(param, transitionType, closeType);

            if (param is UIPanelOpenParam_Levelup levelupParam)
            {
                _whereLevelUp = levelupParam.WhereLevelUp;
                _pfpTokenID = levelupParam.pfpTokenID;
                _afterLevelUp = levelupParam.afterLevelUp;
            }

            setupUI();
        }

        private void setupUI()
        {
            var particleMain = particle.main;

            switch (_whereLevelUp)
            {
                case WhereLevelUp.EntryMode:
                {
                    ClientBasicMode levelData = ViewModel.BasicMode.LevelData;
                    RefEntryLevel refLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(levelData.level);

                    profile_image.setImage(
                        (Texture2D)ClientMain.instance.basicModeLevelImage.levelList[levelData.level - 1], true);
                    text_title.text = StringCollection.getFormat("basic.levelup.finish.title", 0, levelData.level);
                    text_desc.text = StringCollection.getFormat("basic.levelup.finish.desc", 0, refLevel.bonus_percent);

                    _bgGradient.gradient = _entry_bg_color;
                    particleMain.startColor = new ParticleSystem.MinMaxGradient(_entry_particle_color);
                }
                    break;

                case WhereLevelUp.ProMode:
                {
                    ClientNFTItem NFTItem = ViewModel.Wallet.getNFTItem(_pfpTokenID);

                    var refLevel = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(NFTItem.level);

                    UIPFPDetailNetwork.setPFPImage(NFTItem.token_id, profile_image);

                    text_title.text = StringCollection.getFormat("pro.levelup.finish.title", 0, refLevel.level);
                    text_desc.text =
                        StringCollection.getFormat("pro.levelup.finish.desc", 0, refLevel.mining_bonus_percent);

                    _bgGradient.gradient = _pro_bg_color;
                    particleMain.startColor = new ParticleSystem.MinMaxGradient(_pro_particle_color);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void onTransitionEvent(int type)
        {
            if (type == TransitionEventType.start_open)
            {
                _canClose = false;
                StartCoroutine(_runParticle());
            }
        }

        IEnumerator _runParticle()
        {
            yield return new WaitForSeconds(0.05f);

            particle.Play();

            yield return new WaitForSeconds(2.0f);

            _canClose = true;
        }

        public void onClickClose()
        {
            if (_canClose)
            {
                close();
                _afterLevelUp?.Invoke();
            }
        }
    }
}