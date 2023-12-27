using System;
using System.Collections;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
    public sealed class UIPFPPurchaseResult : UISingletonPanel<UIPFPPurchaseResult>
    {
        #region resources

        /// <summary>
        /// 0 - stamina
        /// 1 - experience
        /// </summary>
        [Header("BG Gradient Colors")]
        [SerializeField]
        private Gradient _stamina_bg_color;

        [SerializeField]
        private Gradient _experience_bg_color;

        [Header("Particle Gradient Colors")]
        [SerializeField]
        private Gradient _stamina_particle_color;

        [SerializeField]
        private Gradient _experience_particle_color;

        [Space(10)]
        public Texture[] titleImages;

        public Sprite[] resSprites;

        #endregion resources

        #region ref

        [Space(20)]
        [SerializeField]
        private ParticleSystem _particle;

        public Image imageBase;
        public UIGradientEffect baseGradientEffect;


        [Space(10)]
        [Header("========== Panel UI ==========")]
        [Header("top container")]
        public RawImage _img_icon;

        public TMP_Text _txt_charge_result;
        public TMP_Text _txt_charge_result2;

        [Space(10)]
        [Header("charge amount container")]
        public Image _img_charge_icon;

        [SerializeField]
        private TMP_Text _txt_charge_amount;

        [SerializeField]
        private UILoadingButton _btn_levelUp;

        #endregion ref

        private bool _canClose;
        private PFPDetailPurchasingType _pfpDetailPurchasingType;

        private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        #region override

        public void open(PFPDetailPurchasingType type, string finalValue)
        {
            base.open();

            _pfpDetailPurchasingType = type;

            setupUI(type, finalValue);
        }

        private void setupUI(PFPDetailPurchasingType type, string finalValue)
        {
            var particleMain = _particle.main;
            if (type == PFPDetailPurchasingType.Stamina)
            {
                // bg, particle 색상 설정
                baseGradientEffect.gradient = _stamina_bg_color;
                particleMain.startColor = new ParticleSystem.MinMaxGradient(_stamina_particle_color);

                imageBase.SetAllDirty();

                _img_icon.texture = titleImages[0];
                _txt_charge_result.text = StringCollection.get("pro.purchase_result.label.stamina", 0);
                _txt_charge_result2.text = StringCollection.get("pro.purchase_result.type.stamina", 0);

                _img_charge_icon.sprite = resSprites[0];
            }
            else if (type == PFPDetailPurchasingType.Experience)
            {
                // bg, particle 색상 설정
                baseGradientEffect.gradient = _experience_bg_color;
                particleMain.startColor = new ParticleSystem.MinMaxGradient(_experience_particle_color);

                imageBase.SetAllDirty();

                _img_icon.texture = titleImages[1];
                _txt_charge_result.text = StringCollection.get("pro.purchase_result.label.experience", 0);
                _txt_charge_result2.text = StringCollection.get("pro.purcahse_result.type.experience", 0);

                _img_charge_icon.sprite = resSprites[1];
            }

            _txt_charge_amount.text = finalValue;
        }

        public override void onTransitionEvent(int type)
        {
            if (type == TransitionEventType.start_open)
            {
                _canClose = false;
                _btn_levelUp.beginLoading();

                StartCoroutine(runParticle());
            }
        }

        #endregion override

        #region behaviours

        IEnumerator runParticle()
        {
            yield return new WaitForSeconds(0.05f);

            _particle.Play();

            yield return new WaitForSeconds(2.0f);

            _canClose = true;
            
            _btn_levelUp.endLoading();
        }

        public void onClick_closePurchaseResult()
        {
            if (_canClose)
                base.close();
        }

        #endregion behaviours
    }
}