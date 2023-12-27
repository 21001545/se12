using System;
using System.Collections;
using Drun.Client;
using TMPro;
using UnityEngine;

namespace Festa.Client.ViewModel
{
    class EntryExpChangeDeferObserver : IObserver<string>
    {
	    private readonly TMP_Text _notiText;
	    private readonly float _closingDelay;

        public EntryExpChangeDeferObserver(TMP_Text notiText, float closingDelay = 3.0f)
        {
            if (_closingDelay < 0.0f)
            {
                Debug.LogException(new ArgumentOutOfRangeException("<color=red>닫는 delay 는 무조건 양수로..</color>"));
                return;
            }

            _notiText = notiText;
            _closingDelay = closingDelay;
        }

        public void OnCompleted() { }

        public void OnError(Exception error) => throw error;

        public void OnNext(string allString)
        {
            if (string.IsNullOrEmpty(allString))
                return;

            _notiText.text = allString;
        }
    }
}