using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Festa.Client
{
    public class scrollItem : MonoBehaviour
    {
        [SerializeField]
        private Image img_background;
        [SerializeField]
        private TMP_Text txt_language;
        [SerializeField]
        private GameObject go_check;


        private int _iTemIndex;

        public void setItemIndex(int _index)
        {
            _iTemIndex = _index;
        }

        public int getItemIndex()
        {
            return _iTemIndex;
        }

        public void setText(string _lang)
        {
            txt_language.text = _lang;
        }

        public string getText()
        {
            return txt_language.text;
        }

        public void setPicked(bool _checked)
        {
            if(_checked)
            {
                img_background.color = ColorChart.primary_100;
                go_check.SetActive(true);
            }    
            else
            {
                img_background.color = Color.white;
                go_check.SetActive(false);
            }
        }
    }
}