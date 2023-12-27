using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace Festa.Client.Module
{
	public class WorkerThread
	{
		private List<MultiThreadWorker.JobBase> _job_list;
		private List<MultiThreadWorker.JobBase> _process_job_list;
		private bool _stop_thread;
		private ManualResetEvent _mre;
		private string _name;

		public static WorkerThread create(int index)
		{
			WorkerThread t = new WorkerThread();
			t.init(index);
			return t;
		}

		private void init(int index)
		{
			_stop_thread = false;
			_mre = new ManualResetEvent(false);
			_job_list = new List<MultiThreadWorker.JobBase>();
			_process_job_list = new List<MultiThreadWorker.JobBase>();

			var thread = new Thread(_ThreadWork);
			thread.Name = _name = $"F.Thread.{index}.{DateTime.Now}";
			thread.Start();
		}

		public void stop()
		{
			_stop_thread = true;
			_mre.Set();
		}

		public void execute<T>(MultiThreadWorker.Executor<T> executor, MultiThreadWorker.ResultHandler<T> resultHandler) where T : class
		{
			MultiThreadWorker.Job<T> job = new MultiThreadWorker.Job<T>(executor, resultHandler);

			lock (_job_list)
			{
				_job_list.Add(job);
			}

			_mre.Set();
		}

		private void _ThreadWork()
		{
			while (_stop_thread == false)
			{
				if (_job_list.Count > 0)
				{
					lock (_job_list)
					{
						_process_job_list.AddRange(_job_list);
						_job_list.Clear();
					}

					for (int i = 0; i < _process_job_list.Count; ++i)
					{
						MultiThreadWorker.JobBase job = _process_job_list[i];

						try
						{
							job.runExecute();
						}
						catch (Exception e)
						{
							job.fail(e);
						}
						finally
						{
							MainThreadDispatcher.dispatch(() => {
								job.runResult();
							});
						}
					}

					_process_job_list.Clear();
				}
				else
				{
					_mre.Reset();
				}

				_mre.WaitOne();
			}

			Debug.Log("end thread : " + _name);
		}

	}
}
