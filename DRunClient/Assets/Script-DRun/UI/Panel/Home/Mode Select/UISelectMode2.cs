using System;
using System.Collections;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using UnityEngine;

namespace DRun.Client
{
    public class UISelectMode2 : UISingletonPanel<UISelectMode2>
    {
        #region fields

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

        [SerializeField] private int _selectedMode;
        [SerializeField] private UISelectModeItem _basicSelectMode;
        [SerializeField] private UISelectModeItem _proSelectMode;
        [SerializeField] private UISelectModeItem _marathonSelectMode;
        
        private Action _onClosePanel;

        [SerializeField]
        private Animator _anim_mode_dropdown;

        private static readonly int Close = Animator.StringToHash("close");

        #endregion fields

        #region overrides

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            int defaultMode = ViewModel.PlayMode.Mode;

            // setup and select default mode from ViewModel Mode status.
            _basicSelectMode.setup(defaultMode == PlayModeViewModel.PlayMode.Basic);
            _proSelectMode.setup(defaultMode == PlayModeViewModel.PlayMode.Pro);
            _marathonSelectMode.setup(defaultMode == PlayModeViewModel.PlayMode.Marathon);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0,
            int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);

            if (param is UIPanelOpenParam_UISelectMode2 param2)
            {
                _onClosePanel = param2.onClosePanel;
            }

            selectMode(ViewModel.PlayMode.Mode);
        }

        public override void close(int transitionType = 0)
        {
            _onClosePanel?.Invoke();
            
            base.close(transitionType);
        }

        #endregion overrides

        #region behaviours

        public void onClick_ProMode()
        {
            selectMode(PlayModeViewModel.PlayMode.Pro);
            
            ViewModel.PlayMode.Mode = _selectedMode;
            _anim_mode_dropdown.SetTrigger(Close);
        }

        public void onClick_MarathonMode()
        {
            selectMode(PlayModeViewModel.PlayMode.Marathon);
            
            ViewModel.PlayMode.Mode = _selectedMode;
            _anim_mode_dropdown.SetTrigger(Close);
        }

        public void onClick_BasicMode()
        {
            selectMode(PlayModeViewModel.PlayMode.Basic);
            
            ViewModel.PlayMode.Mode = _selectedMode;
            _anim_mode_dropdown.SetTrigger(Close);
        }

        public void onClick_Backdrop()
        {
            _anim_mode_dropdown.SetTrigger(Close);
        }

        #endregion behaviours

        #region impl

        /// <summary>
        /// Select Mode among 3 of which are "Basic", "Pro" and "Marathon".
        /// </summary>
        /// <param name="newMode"></param>
        void selectMode(int newMode)
        {
            this._selectedMode = newMode;

            switch (newMode)
            {
                case PlayModeViewModel.PlayMode.Basic:
                {
                    _basicSelectMode.select();
                    _proSelectMode.deselect();
                    _marathonSelectMode.deselect();
                }
                    break;

                case PlayModeViewModel.PlayMode.Pro:
                {
                    _basicSelectMode.deselect();
                    _proSelectMode.select();
                    _marathonSelectMode.deselect();
                }
                    break;

                case PlayModeViewModel.PlayMode.Marathon:
                {
                    _basicSelectMode.deselect();
                    _proSelectMode.deselect();
                    _marathonSelectMode.select();
                }
                    break;
            }
        }

        #endregion impl
    }
}