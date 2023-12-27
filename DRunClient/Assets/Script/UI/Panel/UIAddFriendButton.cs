using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Festa.Client
{
    public class UIAddFriendButton : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text txt_add;
        [SerializeField]
        private Image img_bg;

        public void setAdd()
        {
            txt_add.text = GlobalRefDataContainer.getStringCollection().get("friends.add", 0);
            txt_add.color = ColorChart.primary_400;
            img_bg.color = ColorChart.primary_100;
        }

        public void setAdded()
        {
            txt_add.text = GlobalRefDataContainer.getStringCollection().get("friends.add", 1);
            txt_add.color = ColorChart.gray_500;
            img_bg.color = ColorChart.gray_200;
        }
    }
}