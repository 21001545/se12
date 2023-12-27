using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Festa.Client
{
    public class VerificationInputFieldSettings : MonoBehaviour
    {
        private static int _codeLength = 6;
        private string[] _input_text = {
        "──────", "─────", "────", "───", "──", "─", ""
        };

        [SerializeField]
        private TMP_InputField input;
        [SerializeField]
        private TMP_Text placeholder;
        [SerializeField]
        private GameObject go_errorMsg;
        [SerializeField]
        private Image[] img_backLights = new Image[_codeLength];

        private void Update()
        {
            int length = input.text.Length;

            if (length < 0)
                length = 0;
            else if (length > _codeLength)
                length = _codeLength;

            if(input.isFocused)
            {
                // 백라이팅
                if (go_errorMsg.activeSelf)
                {
                    // 에러
                    for (int i = 0; i < _codeLength; ++i)
                    {
                        img_backLights[i].color = ColorChart.secondary_100;
                    }
                }
                else
                {
                    int i = 0;
                    if (length < 6)
                    {
                        // 그냥,,
                        for (; i <= length; ++i)
                        {
                            img_backLights[i].color = ColorChart.gray_500;
                        }

                        for (; i < _codeLength; ++i)
                        {
                            img_backLights[i].color = ColorChart.gray_150;
                        }
                    }
                    else
                    {
                        for (; i < 6; ++i)
                        {
                            img_backLights[i].color = ColorChart.gray_500;
                        }
                    }
                }

                // 플레이스홀더
                if (length == 6)
                    placeholder.text = _input_text[6];
                else
                    placeholder.text = _input_text[length + 1];
            }
            else
            {
                // 백라이팅
                if (go_errorMsg.activeSelf)
                {
                    // 에러
                    for (int i = 0; i < _codeLength; ++i)
                    {
                        img_backLights[i].color = ColorChart.secondary_100;
                    }
                }
                else
                {
                    for (int i = 0; i < _codeLength; ++i)
                    {
                        img_backLights[i].color = ColorChart.gray_150;
                    }
                }

                // 플레이스홀더
                placeholder.text = _input_text[length];
            }
        }
    }
}
