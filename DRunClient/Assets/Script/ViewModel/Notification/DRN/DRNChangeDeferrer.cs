using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Festa.Client.ViewModel
{
    /// <summary>
    /// DRN Balance(잔고) 의 Transaction 를 가지고 polling 하여
    /// 필요 시점에 사용하는 자료구조.
    /// </summary>
    public class DRNChangeDeferrer : IObservable<long>
    {
        internal class UnSubscriber : IDisposable
        {
            private readonly IList<IObserver<long>> _observers;
            private readonly IObserver<long> _disposingObserver;

            public UnSubscriber(
                IList<IObserver<long>> observers,
                IObserver<long> disposingObserver)
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

        private readonly List<IObserver<long>> _changeObservers = new List<IObserver<long>>();
        private readonly Queue<long> _changeQ;

        public DRNChangeDeferrer(int expectedCapacity = 10)
        {
            _changeQ = new Queue<long>(capacity: expectedCapacity);
        }


        // 알림 받을 Observer 구독
        public IDisposable Subscribe(IObserver<long> observer)
        {
            if (!_changeObservers.Contains((IObserver<long>)observer))
                _changeObservers.Add(observer);

            return new UnSubscriber(_changeObservers, observer);
        }

        /// <summary>
        /// 쌓아둔 큐를 합산하여 알림 발신.
        /// </summary>
        public void notify()
        {
	        if (_changeQ.Count == 0)
		        return;

            long sum = _changeQ.Sum();
            foreach (var observer in _changeObservers)
            {
                observer.OnNext(sum);
                observer.OnCompleted();
            }
			_changeQ.Clear();
        }
        // 알림으로 쌓기.
        public void defer(long change) => _changeQ.Enqueue(change);
        public void reset() => _changeQ.Clear();
    }
}