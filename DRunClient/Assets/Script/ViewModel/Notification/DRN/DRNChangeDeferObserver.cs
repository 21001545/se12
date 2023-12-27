using System;
using System.Collections;
using Drun.Client;
using DRun.Client.Module;

using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Festa.Client.ViewModel
{
    class DRNChangeDeferObserver : IObserver<long>
    {
        private readonly GameObject _drnNotiPanel;
        private readonly Animator _animDrnNotiPanel;
        private readonly TMP_Text _text_DrnChangeAmount;
        private readonly float _closingDelay;

        private bool _opened = false;

        private static readonly int Close = Animator.StringToHash("close");
        private static readonly int Open = Animator.StringToHash("open");

        public DRNChangeDeferObserver(
            GameObject drnNotiPanel,
            Animator animDrnNotiPanel,
            TMP_Text text_drnChangeAmount,
            float closingDelay = 3.0f)
        {
            _closingDelay = closingDelay;
            _drnNotiPanel = drnNotiPanel;
            _text_DrnChangeAmount = text_drnChangeAmount;
            _animDrnNotiPanel = animDrnNotiPanel;
        }

        public void OnCompleted()
        {
            if (!_opened)
                return;

			Yielder.SharedMono.StartCoroutine(ResetText());
            IEnumerator ResetText()
            {
                yield return new WaitForSeconds(_closingDelay);

                _animDrnNotiPanel.SetTrigger(Close);
				_drnNotiPanel.SetActive(false);
                _text_DrnChangeAmount.text = string.Empty;
            }
        }

        public void OnError(Exception error)
        {
            _opened = false;
            throw error;
        }

        public void OnNext(long accumulatedDRNAmount)
        {
            if (accumulatedDRNAmount == 0)
                return;

            _opened = true;

			_drnNotiPanel.SetActive(true);
            string changeAmountSymbol = accumulatedDRNAmount > 0 ? "+ " : "  ";
            _text_DrnChangeAmount.text = changeAmountSymbol + StringUtil.toDRNStringDefault(accumulatedDRNAmount);

			_animDrnNotiPanel.gameObject.SetActive(true);
            _animDrnNotiPanel.SetTrigger(Open);
        }
    }
}