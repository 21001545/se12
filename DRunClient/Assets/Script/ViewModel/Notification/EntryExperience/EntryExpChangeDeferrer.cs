using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Festa.Client.ViewModel
{
	/// <summary>
	/// Entry 모드 Profile Exp 획득 알림 연기해주는 친구.
	/// </summary>
	public class EntryExpChangeDeferrer : IObservable<string>
	{
		internal class UnSubscriber : IDisposable
		{
			private readonly IList<IObserver<string>> _observers;
			private readonly IObserver<string> _disposingObserver;

			public UnSubscriber(
				IList<IObserver<string>> observers,
				IObserver<string> disposingObserver)
			{
				_observers = observers;
				_disposingObserver = disposingObserver;
			}

			public void Dispose()
			{
				if (_disposingObserver != null && _observers.Contains(_disposingObserver))
					_observers.Remove(_disposingObserver);
			}
		}

		private readonly List<IObserver<string>> _changeObservers = new();
		private string _lastChange;
		public bool alreadyDeferred;
		public bool used;

		public int ObserversCount => _changeObservers.Count;

		// 알림으로 쌓기.
		public void defer(string change)
		{
			_lastChange = change;
			alreadyDeferred = true;
		}

		// 알림 받을 Observer 구독
		public IDisposable Subscribe(IObserver<string> observer)
		{
			if (!_changeObservers.Contains((IObserver<string>)observer))
				_changeObservers.Add(observer);

			return new UnSubscriber(_changeObservers, observer);
		}

		/// <summary>
		/// 쌓아둔 큐를 합산하여 알림 발신.
		/// </summary>
		public void notify()
		{
			foreach (var observer in _changeObservers)
			{
				observer.OnNext(_lastChange);
				observer.OnCompleted();
			}

			used = true;
		}

		public void reset()
		{
			_lastChange = null;
			alreadyDeferred = false;
			used = false;
		}
	}
}