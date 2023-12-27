using System;
using System.Collections.Generic;

namespace Festa.Client.Module
{
	public static class MainThreadDispatcher
	{
		private static List<Action> _dispatchQueue = new List<Action>();
		private static List<Action> _flushList = new List<Action>();

		//fixed queue
		private static List<Action> _dispatchQueueFixed = new List<Action>();


		public static void dispatch(Action action)
		{
			lock(_dispatchQueue)
			{
				_dispatchQueue.Add(action);
			}
		}

		public static void dispatchFixedUpdate(Action action)
		{
			lock(_dispatchQueueFixed)
			{
				_dispatchQueueFixed.Add(action);
			}
		}

		public static void flush()
		{
			if( _dispatchQueue.Count > 0)
			{
				lock (_dispatchQueue)
				{
					_flushList.AddRange(_dispatchQueue);
					_dispatchQueue.Clear();
				}

				foreach(Action action in _flushList)
				{
					action();
				}

				_flushList.Clear();
			}
		}

		public static void flushFixedUpdate()
		{
			if( _dispatchQueueFixed.Count > 0)
			{
				lock( _dispatchQueueFixed)
				{
					_flushList.AddRange(_dispatchQueueFixed);
					_dispatchQueueFixed.Clear();
				}

				foreach(Action action in _flushList)
				{
					action();
				}

				_flushList.Clear();
			}
		}

	}
}
