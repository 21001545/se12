using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	[RequireComponent(typeof(TMP_Text))]
	public class UITypingAnimation : MonoBehaviour
	{
		public float speed = 0.2f;

		private TMP_Text _tmp_text;
		private IntervalTimer _timer;
		private int _text_count;

		public void prepare(string text)
		{
			if( string.IsNullOrEmpty(text))
			{
				text = "";
			}

			_tmp_text.text = text;
			_text_count = text.Length;
			_tmp_text.maxVisibleCharacters = 0;
		}

		public void stop()
		{
			_timer.stop();
			_tmp_text.maxVisibleCharacters = 0;
		}

		public float start(float delay)
		{
			if( _text_count > 0)
			{
				_timer.init(delay, false, false);
			}

			return (delay + _text_count * speed);
		}

		// 초기화 필수
		public void init()
		{
			_tmp_text = GetComponent<TMP_Text>();
			_timer = IntervalTimer.create(speed, false, false);
		}

		// 누군가 호출을 해줘야 한다
		public void update()
		{
			if( _timer.update())
			{
				_tmp_text.maxVisibleCharacters++;
				if(_tmp_text.maxVisibleCharacters < _text_count)
				{
					_timer.setNext(speed);
				}
			}
		}
	}
}
