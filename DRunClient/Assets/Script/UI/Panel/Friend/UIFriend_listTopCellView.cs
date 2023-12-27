using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using TMPro;

namespace Festa.Client
{
    public class UIFriend_listTopCellView : EnhancedScrollerCellView
    {
        [SerializeField]
        private TMP_Text txt_friendCount;
        [SerializeField]
        private GameObject go_descBubble;
        [SerializeField]
        private TMP_Text txt_desc;
        [SerializeField]
        private GameObject go_button;

        public void setup(int count, int max, bool questionMark, string desc)
        {
            txt_friendCount.text = $"{count}/{max}";
            txt_desc.text = desc;
            go_button.SetActive(questionMark);
        }

        public void onClickQuestionMark()
        {
            go_descBubble.SetActive(true);
            Invoke("setDescBubbleInactive", 1.2f);
        }

        private void setDescBubbleInactive()
        {
            go_descBubble.SetActive(false);
        }
    }
}