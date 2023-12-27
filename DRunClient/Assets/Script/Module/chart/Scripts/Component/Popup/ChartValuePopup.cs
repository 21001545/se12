using UnityEngine;
using UnityEngine.UI;

namespace AwesomeCharts
{
    public abstract class ChartValuePopupAbstract : MonoBehaviour
    {
        public abstract void SetText(string text);
    }

    public class ChartValuePopup : ChartValuePopupAbstract
    {

        private Text text;

        public override void SetText(string text)
        {
            this.text.text = text;
        }
    }
}