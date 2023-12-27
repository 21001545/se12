using Festa.Client.Module;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
	[RequireComponent(typeof(TMP_Text))]
	public class UINumberCountingAnimation : MonoBehaviour
	{
		public float smoothTime = 1.0f;
		public string formatString = "N0";

		[Serializable]
		public class NumberChangeEvent : UnityEvent<double>
		{

		}

		public NumberChangeEvent onNumberChanged = new NumberChangeEvent();

		private TMP_Text _tmp_text;
		private DoubleSmoothDamper _damper;
		private bool _play;

		public void init()
		{
			_tmp_text = GetComponent<TMP_Text>();
			_damper = DoubleSmoothDamper.create(0, smoothTime, 0.5f);
			_play = false;

			applyText();
		}

		public void setValue(int number,bool isInit)
		{
			if( isInit)
			{
				_damper.reset(number);
				onNumberChanged.Invoke(number);
			}
			else
			{
				_damper.setTarget((float)number);
			}
		}

		public void start()
		{
			_damper.setSmoothTime(smoothTime);
			_play = true;
		}

		public void stop()
		{
			_play = false;
		}

		public void update()
		{
			if( _play == false)
			{
				return;
			}

			if( _damper.update())
			{
				applyText();
				onNumberChanged.Invoke(_damper.getCurrent());
			}
		}

		public double getCurrentValue()
		{
			return _damper.getCurrent();
		}

		public double getTargetValue()
		{
			return _damper.getTarget();
		}

		private void applyText()
		{
			int current = (int)Math.Ceiling(_damper.getCurrent());
			_tmp_text.text = current.ToString(formatString);
		}

	}
}
