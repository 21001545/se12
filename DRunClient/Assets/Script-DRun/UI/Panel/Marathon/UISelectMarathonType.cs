using DRun.Client;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Script_DRun.UI.Panel.Marathon
{
    public class UISelectMarathonType : UISingletonPanel<UISelectMarathonType>
    {
        public TMP_Text[] options;

        private int _currentType;
        private UnityAction<int> _callback;

        [SerializeField]
        private Animator _anim_mode_dropdown;

        private static readonly int Close = Animator.StringToHash("close");

        public void open(int currentType,UnityAction<int> callback)
        {
            base.open();

            _currentType = currentType;
            _callback = callback;
            for(int i = 0; i < options.Length; i++)
            {
                TMP_Text text = options[i];
                bool selected = (i + 1) == _currentType;

                if( selected)
                {
                    text.color = UIStyleDefine.ColorStyle.gray200;
                }
                else
                {
                    text.color = UIStyleDefine.ColorStyle.gray500;
                }
            }
        }

        public void onClick_Type(int type)
        {
            _anim_mode_dropdown.SetTrigger(Close);
            _callback(type);
        }

        public void onClick_Back()
        {
            _anim_mode_dropdown.SetTrigger(Close);
            _callback(_currentType);
        }
    }
}