using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace Festa.Client.Module
{ 
	public class MultiThreadWorker
	{
		public delegate void Executor<T>(Job<T> promise) where T:class;
		public delegate void ResultHandler<T>(AsyncResult<T> result);

		public interface JobBase
		{
			public void runExecute();
			public void runResult();
			public void fail(String message);
			public void fail(Exception e);
		}

		public class Job<T> : JobBase where T:class
		{
			private Executor<T>		_executeHandler;
			private ResultHandler<T> _resultHandler;
			private AsyncResult<T> _result;

			public Job(Executor<T> executor,ResultHandler<T> resultHandler)
			{
				_executeHandler = executor;
				_resultHandler = resultHandler;
			}
		
			public void runExecute()
			{
				_executeHandler(this);
			}

			public void runResult()
			{
				_resultHandler(_result);
			}

			public void fail(String message)
			{
				_result = Future.failedFuture<T>(message);
			}

			public void fail(Exception e)
			{
				_result = Future.failedFuture<T>(e);
			}

			public void complete(T result = null)
			{
				_result = Future.succeededFuture<T>(result);
			}
		}

		private List<WorkerThread> _thread_list;
		private int _last_index;

		public static MultiThreadWorker create()
		{
			MultiThreadWorker worker = new MultiThreadWorker();
			worker.init();
			return worker;
		}

		private void init()
		{
			_thread_list = new List<WorkerThread>();
			_last_index = 0;

#if UNITY_EDITOR
			int thread_count = 1;
#else
			int thread_count = 3;
#endif
			for (int i =0; i < thread_count; ++i)
			{
				_thread_list.Add(WorkerThread.create(i));
			}
		}

		public void stop()
		{
			foreach(WorkerThread t in _thread_list)
			{
				t.stop();
			}
		}

		public void execute<T>(Executor<T> executor, ResultHandler<T> resultHandler) where T:class
		{
			_thread_list[_last_index].execute<T>(executor, resultHandler);
			_last_index++;
			if( _last_index >= _thread_list.Count)
			{
				_last_index = 0;
			}
		}
	}
}
