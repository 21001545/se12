using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace Festa.Client
{
    public class VerificationTimer : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text txt_displayTime;

        private int _countSec;

        private DateTime _targetTime;
        private int _timeLeft;
        private bool _isCounting;

        public void setCountTime(int time)
        {
            _countSec = time;
        }

        public void turnOnTimer()
        {
            _targetTime = DateTime.Now.AddSeconds(_countSec);
            _isCounting = true;

            txt_displayTime.color = ColorChart.primary_300;
        }

        public bool isCounting()
        {
            return _isCounting;
        }

        public int getTimeLeft()
        {
            return _timeLeft;
        }

        private void Update()
        {
            if (_isCounting)
            {
                TimeSpan timeDiff = _targetTime - DateTime.Now;
                int diffSec = timeDiff.Seconds;

                if (diffSec > 0)
                {
                    txt_displayTime.text = timeDiff.ToString("mm':'ss");
                    _timeLeft = diffSec;
                }
                else
                {
                    txt_displayTime.text = "00:00";
                    _timeLeft = 0;
                    _isCounting = false;

                    txt_displayTime.color = ColorChart.error_300;
                }
            }
        }
    }
}
